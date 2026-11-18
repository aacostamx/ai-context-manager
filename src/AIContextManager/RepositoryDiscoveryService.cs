using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AIContextManager;

/// <summary>Finds Git repositories under SourcePath by looking for Git metadata (.git folder or .git file).</summary>
public sealed class RepositoryDiscoveryService(IOptions<ContextManagerOptions> options, ILogger<RepositoryDiscoveryService> log)
{
    /// <summary>Folders that never contain a repository root we care about.</summary>
    public static readonly HashSet<string> SkippedFolders = new(StringComparer.OrdinalIgnoreCase)
    {
        ".git", ".vs", ".vscode", ".idea", "bin", "obj", "node_modules", "dist", "build", "packages", "TestResults",
    };

    private readonly ContextManagerOptions _o = options.Value;

    /// <summary>Returns full paths of repository roots, sorted by path.</summary>
    public IReadOnlyList<string> Discover()
    {
        var found = new List<string>();
        Walk(_o.SourcePath, 0, found);
        found.Sort(StringComparer.OrdinalIgnoreCase);
        log.LogDebug("Discovery found {Count} repositories under {Source}", found.Count, _o.SourcePath);
        return found;
    }

    private void Walk(string dir, int depth, List<string> found)
    {
        if (IsGitRepository(dir))
        {
            found.Add(dir);
            return; // never descend into a repository (submodules/nested repos are part of their parent)
        }

        if (depth >= _o.MaxDiscoveryDepth) return;

        IEnumerable<DirectoryInfo> children;
        try
        {
            children = new DirectoryInfo(dir).EnumerateDirectories();
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException or IOException)
        {
            log.LogDebug("Skipping unreadable folder {Folder}: {Error}", dir, ex.Message);
            return;
        }

        foreach (var child in children)
        {
            if (SkippedFolders.Contains(child.Name)) continue;
            if (child.Attributes.HasFlag(FileAttributes.ReparsePoint)) continue; // junctions/symlinks: avoid loops
            if (PathUtil.IsSameOrUnder(child.FullName, _o.OutputPath) || PathUtil.IsSameOrUnder(child.FullName, _o.StagingPath)) continue;
            Walk(child.FullName, depth + 1, found);
        }
    }

    /// <summary>A repository has a ".git" folder with a HEAD file, or a ".git" file pointing to the real Git dir (worktrees).</summary>
    public static bool IsGitRepository(string dir)
    {
        var git = Path.Combine(dir, ".git");
        if (Directory.Exists(git)) return File.Exists(Path.Combine(git, "HEAD"));
        if (!File.Exists(git)) return false;
        try
        {
            using var reader = new StreamReader(git);
            return reader.ReadLine()?.TrimStart().StartsWith("gitdir:", StringComparison.OrdinalIgnoreCase) == true;
        }
        catch (IOException)
        {
            return false;
        }
    }
}
