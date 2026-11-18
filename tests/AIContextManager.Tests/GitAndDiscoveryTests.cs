using AIContextManager;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace AIContextManager.Tests;

public class GitAndDiscoveryTests : IDisposable
{
    private readonly TestRepos _repos = new();

    private ContextManagerOptions Options() => new()
    {
        SourcePath = _repos.Source,
        OutputPath = Path.Combine(_repos.Root, "out"),
        StagingPath = Path.Combine(_repos.Root, "staging"),
    };

    private GitBranchService Git() => new(Microsoft.Extensions.Options.Options.Create(Options()), NullLogger<GitBranchService>.Instance);

    private RepositoryDiscoveryService Discovery() =>
        new(Microsoft.Extensions.Options.Options.Create(Options()), NullLogger<RepositoryDiscoveryService>.Instance);

    [Fact]
    public void Discovers_repositories_by_git_metadata_and_skips_noise()
    {
        _repos.CreateRepository("intsol-dna-list-matching-api");
        _repos.CreateRepository("intsol-dna-list-matching-ui");
        Directory.CreateDirectory(Path.Combine(_repos.Source, "not-a-repo", "src"));
        Directory.CreateDirectory(Path.Combine(_repos.Source, "node_modules", "pkg", ".git")); // must be skipped

        var found = Discovery().Discover().Select(Path.GetFileName).ToList();

        Assert.Equal(["intsol-dna-list-matching-api", "intsol-dna-list-matching-ui"], found);
    }

    [Fact]
    public void Discovers_worktree_with_git_file()
    {
        var worktree = Path.Combine(_repos.Source, "intsol-biography-api");
        Directory.CreateDirectory(worktree);
        File.WriteAllText(Path.Combine(worktree, ".git"), "gitdir: C:/elsewhere/.git/worktrees/x\n");

        Assert.Single(Discovery().Discover());
    }

    [Fact]
    public void New_repository_is_discovered_without_configuration()
    {
        _repos.CreateRepository("intsol-biography-api");
        Assert.Single(Discovery().Discover());

        _repos.CreateRepository("intsol-biography-ui");

        Assert.Equal(2, Discovery().Discover().Count);
    }

    [Fact]
    public async Task Snapshot_reads_remote_branches_without_touching_the_working_repository()
    {
        var clone = _repos.CreateRepository("intsol-dna-list-matching-api");
        File.WriteAllText(Path.Combine(clone, "Program.cs"), "// uncommitted local edit\n");
        var statusBefore = TestRepos.Git(clone, "status", "--porcelain");
        var headBefore = TestRepos.Git(clone, "rev-parse", "--abbrev-ref", "HEAD");
        var git = Git();

        Assert.True(await git.FetchAsync(clone, CancellationToken.None));
        var dev = await git.ResolveRemoteBranchAsync(clone, "develop", CancellationToken.None);
        var prod = await git.ResolveRemoteBranchAsync(clone, "main", CancellationToken.None);
        Assert.NotNull(dev);
        Assert.NotNull(prod);
        Assert.NotEqual(dev, prod);

        var target = Path.Combine(_repos.Root, "snap");
        var (written, skipped) = await git.ExportSnapshotAsync(clone, dev!, target, CancellationToken.None);

        Assert.Equal("Console.WriteLine(\"develop\");", File.ReadAllText(Path.Combine(target, "Program.cs")).TrimEnd()); // autocrlf may apply
        Assert.False(File.Exists(Path.Combine(target, ".env")), ".env must never be copied");
        Assert.Equal(1, skipped);
        Assert.Equal(2, written);

        // The user's repository is exactly as before: same branch, same uncommitted change, no worktrees added.
        Assert.Equal(headBefore, TestRepos.Git(clone, "rev-parse", "--abbrev-ref", "HEAD"));
        Assert.Equal(statusBefore, TestRepos.Git(clone, "status", "--porcelain"));
        Assert.Equal("// uncommitted local edit\n", File.ReadAllText(Path.Combine(clone, "Program.cs")));
        Assert.Single(TestRepos.Git(clone, "worktree", "list").Split('\n'));
    }

    [Fact]
    public async Task Missing_branch_is_reported_not_substituted()
    {
        var clone = _repos.CreateRepository("intsol-biography-sync", withDevelop: false);

        Assert.Null(await Git().ResolveRemoteBranchAsync(clone, "develop", CancellationToken.None));
        Assert.NotNull(await Git().ResolveRemoteBranchAsync(clone, "main", CancellationToken.None));
    }

    [Fact]
    public async Task Push_to_develop_changes_only_the_dev_commit()
    {
        var clone = _repos.CreateRepository("intsol-dna-list-matching-ui");
        var git = Git();
        var devBefore = await git.ResolveRemoteBranchAsync(clone, "develop", CancellationToken.None);
        var prodBefore = await git.ResolveRemoteBranchAsync(clone, "main", CancellationToken.None);
        var previousDev = new EnvState { CommitSha = devBefore, Status = ContextStatus.Ok, Fingerprint = "f" };
        var previousProd = new EnvState { CommitSha = prodBefore, Status = ContextStatus.Ok, Fingerprint = "f" };

        _repos.PushChange("intsol-dna-list-matching-ui", "develop", "App.tsx", "export {}\n");
        await git.FetchAsync(clone, CancellationToken.None);
        var devAfter = await git.ResolveRemoteBranchAsync(clone, "develop", CancellationToken.None);
        var prodAfter = await git.ResolveRemoteBranchAsync(clone, "main", CancellationToken.None);

        Assert.True(ChangeDetector.NeedsRegeneration(previousDev, devAfter!, outputExists: true, "f"));
        Assert.False(ChangeDetector.NeedsRegeneration(previousProd, prodAfter!, outputExists: true, "f"));
    }

    public void Dispose() => _repos.Dispose();
}
