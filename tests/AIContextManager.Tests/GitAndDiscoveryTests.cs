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

    private static GitBranchService Git() => new(NullLogger<GitBranchService>.Instance);

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
    public void Excluded_folder_and_everything_inside_is_ignored()
    {
        _repos.CreateRepository("intsol-biography-api");
        var hidden = Path.Combine(_repos.Source, "Repos", "old-stuff", "intsol-biography-ui");
        Directory.CreateDirectory(Path.Combine(hidden, ".git"));
        File.WriteAllText(Path.Combine(hidden, ".git", "HEAD"), "ref: refs/heads/main\n");
        var directRepo = Path.Combine(_repos.Source, "Repos");
        File.WriteAllText(Path.Combine(directRepo, ".git"), "gitdir: C:/elsewhere/.git\n"); // even Repos itself as a repo

        var options = Options();
        options.ExcludedFolders = ["Repos"];
        var found = new RepositoryDiscoveryService(Microsoft.Extensions.Options.Options.Create(options),
            NullLogger<RepositoryDiscoveryService>.Instance).Discover();

        Assert.Equal(["intsol-biography-api"], found.Select(Path.GetFileName));
        Assert.True(options.IsExcluded(Path.Combine(hidden, "src", "App.tsx")));
        Assert.False(options.IsExcluded(Path.Combine(_repos.Source, "Repositories")));
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
    public async Task Develop_and_main_are_read_from_origin_whatever_branch_is_checked_out()
    {
        var clone = _repos.CreateRepository("intsol-dna-list-matching-api"); // checked out on feature/my-work
        File.WriteAllText(Path.Combine(clone, "Program.cs"), "// uncommitted local edit\n");
        var statusBefore = TestRepos.Git(clone, "status", "--porcelain");
        var git = Git();

        Assert.Null(await git.FetchAsync(clone, CancellationToken.None));
        var dev = await git.ResolveRemoteBranchAsync(clone, "develop", CancellationToken.None);
        var prod = await git.ResolveRemoteBranchAsync(clone, "main", CancellationToken.None);
        Assert.NotNull(dev);
        Assert.NotNull(prod);
        Assert.NotEqual(dev, prod);

        var devSnap = Path.Combine(_repos.Root, "snap-dev");
        var (written, skipped) = await git.ExportSnapshotAsync(clone, dev!, devSnap, CancellationToken.None);
        var prodSnap = Path.Combine(_repos.Root, "snap-prod");
        await git.ExportSnapshotAsync(clone, prod!, prodSnap, CancellationToken.None);

        // Each environment gets its own branch content; uncommitted edits and .env are never included.
        Assert.Equal("Console.WriteLine(\"develop\");", File.ReadAllText(Path.Combine(devSnap, "Program.cs")).TrimEnd()); // autocrlf may apply
        Assert.Equal("Console.WriteLine(\"main\");", File.ReadAllText(Path.Combine(prodSnap, "Program.cs")).TrimEnd());
        Assert.False(File.Exists(Path.Combine(devSnap, ".env")), ".env must never be copied");
        Assert.Equal(1, skipped);
        Assert.Equal(2, written);

        // The user's repository is exactly as before: same branch, same uncommitted change, no worktrees added.
        Assert.Equal("feature/my-work", TestRepos.Git(clone, "rev-parse", "--abbrev-ref", "HEAD"));
        Assert.Equal(statusBefore, TestRepos.Git(clone, "status", "--porcelain"));
        Assert.Equal("// uncommitted local edit\n", File.ReadAllText(Path.Combine(clone, "Program.cs")));
        Assert.Single(TestRepos.Git(clone, "worktree", "list").Split('\n'));
    }

    [Fact]
    public async Task Missing_release_branch_is_reported_not_substituted()
    {
        var clone = _repos.CreateRepository("intsol-biography-sync");

        Assert.Null(await Git().ResolveRemoteBranchAsync(clone, "release", CancellationToken.None));
        Assert.NotNull(await Git().ResolveRemoteBranchAsync(clone, "main", CancellationToken.None));
    }

    [Fact]
    public async Task Push_to_develop_changes_only_the_dev_context_after_fetch()
    {
        var clone = _repos.CreateRepository("intsol-dna-list-matching-ui");
        var git = Git();
        var devBefore = (await git.ResolveRemoteBranchAsync(clone, "develop", CancellationToken.None))!;
        var prodBefore = (await git.ResolveRemoteBranchAsync(clone, "main", CancellationToken.None))!;
        var previousDev = new ContextState { Branch = "develop", CommitSha = devBefore, Status = ContextStatus.Ok, Fingerprint = "f" };
        var previousProd = new ContextState { Branch = "main", CommitSha = prodBefore, Status = ContextStatus.Ok, Fingerprint = "f" };

        _repos.PushChange("intsol-dna-list-matching-ui", "develop", "App.tsx", "export {}\n"); // a teammate pushes
        Assert.Null(await git.FetchAsync(clone, CancellationToken.None));
        var devAfter = (await git.ResolveRemoteBranchAsync(clone, "develop", CancellationToken.None))!;
        var prodAfter = (await git.ResolveRemoteBranchAsync(clone, "main", CancellationToken.None))!;

        Assert.True(ChangeDetector.NeedsRegeneration(previousDev, "develop", devAfter, outputExists: true, "f"));
        Assert.False(ChangeDetector.NeedsRegeneration(previousProd, "main", prodAfter, outputExists: true, "f"));
    }

    [Fact]
    public async Task Fetch_failure_is_reported_without_throwing()
    {
        var clone = _repos.CreateRepository("intsol-biography-api");
        TestRepos.Git(clone, "remote", "set-url", "origin", Path.Combine(_repos.Root, "does-not-exist.git"));

        Assert.NotNull(await Git().FetchAsync(clone, CancellationToken.None));
        Assert.NotNull(await Git().ResolveRemoteBranchAsync(clone, "main", CancellationToken.None)); // last fetched ref still usable
    }

    [Fact]
    public void Each_repository_gets_its_own_folder()
    {
        var repo = new RepositoryInfo("intsol-dna-list-matching-api", @"C:\source\intsol-dna-list-matching-api");

        Assert.Equal("intsol-dna-list-matching-api/intsol-dna-list-matching-api-QA-context.md",
            repo.RelativeContextFile(new EnvironmentOptions { Name = "QA", Branch = "release" }));
    }

    public void Dispose() => _repos.Dispose();
}
