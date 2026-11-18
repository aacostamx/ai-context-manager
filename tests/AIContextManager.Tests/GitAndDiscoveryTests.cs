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
    public async Task Snapshot_is_last_commit_of_checked_out_branch_and_repo_is_untouched()
    {
        var clone = _repos.CreateRepository("intsol-dna-list-matching-api"); // checked out on feature/my-work (from main)
        File.WriteAllText(Path.Combine(clone, "Program.cs"), "// uncommitted local edit\n");
        var statusBefore = TestRepos.Git(clone, "status", "--porcelain");
        var git = Git();

        var current = await git.GetCurrentAsync(clone, CancellationToken.None);
        Assert.NotNull(current);
        Assert.Equal("feature/my-work", current.Value.Branch);

        var target = Path.Combine(_repos.Root, "snap");
        var (written, skipped) = await git.ExportSnapshotAsync(clone, current.Value.Sha, target, CancellationToken.None);

        // Committed content only (uncommitted edit not included), .env never copied.
        Assert.Equal("Console.WriteLine(\"main\");", File.ReadAllText(Path.Combine(target, "Program.cs")).TrimEnd()); // autocrlf may apply
        Assert.False(File.Exists(Path.Combine(target, ".env")), ".env must never be copied");
        Assert.Equal(1, skipped);
        Assert.Equal(2, written);

        // The user's repository is exactly as before: same branch, same uncommitted change, no worktrees added.
        Assert.Equal("feature/my-work", TestRepos.Git(clone, "rev-parse", "--abbrev-ref", "HEAD"));
        Assert.Equal(statusBefore, TestRepos.Git(clone, "status", "--porcelain"));
        Assert.Equal("// uncommitted local edit\n", File.ReadAllText(Path.Combine(clone, "Program.cs")));
        Assert.Single(TestRepos.Git(clone, "worktree", "list").Split('\n'));
    }

    [Fact]
    public async Task Switching_branch_in_github_desktop_regenerates_the_context()
    {
        var clone = _repos.CreateRepository("intsol-dna-list-matching-ui");
        var git = Git();
        var before = (await git.GetCurrentAsync(clone, CancellationToken.None))!.Value;
        var previous = new ContextState { Branch = before.Branch, CommitSha = before.Sha, Status = ContextStatus.Ok, Fingerprint = "f" };
        Assert.False(ChangeDetector.NeedsRegeneration(previous, before.Branch, before.Sha, outputExists: true, "f"));

        TestRepos.Git(clone, "checkout", "-q", "develop"); // the Current Branch dropdown in GitHub Desktop
        var after = (await git.GetCurrentAsync(clone, CancellationToken.None))!.Value;

        Assert.Equal("develop", after.Branch);
        Assert.True(ChangeDetector.NeedsRegeneration(previous, after.Branch, after.Sha, outputExists: true, "f"));

        var target = Path.Combine(_repos.Root, "snap-dev");
        await git.ExportSnapshotAsync(clone, after.Sha, target, CancellationToken.None);
        Assert.Equal("Console.WriteLine(\"develop\");", File.ReadAllText(Path.Combine(target, "Program.cs")).TrimEnd());
    }

    [Fact]
    public async Task Repository_without_commits_has_no_current_commit()
    {
        var empty = Path.Combine(_repos.Source, "intsol-new-api");
        TestRepos.Git(_repos.Source, "init", "-q", empty);

        Assert.Null(await Git().GetCurrentAsync(empty, CancellationToken.None));
    }

    public void Dispose() => _repos.Dispose();
}
