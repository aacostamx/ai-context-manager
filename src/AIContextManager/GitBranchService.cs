using System.Formats.Tar;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AIContextManager;

/// <summary>
/// Read-only access to branch content. The user's working repository is never checked out, reset, cleaned or pulled:
///  - "git fetch" only updates remote-tracking refs (refs/remotes/origin/*), exactly like GitHub Desktop's background fetch.
///  - "git rev-parse" resolves origin/develop and origin/main.
///  - "git archive" streams the committed tree of a commit; we extract it with System.Formats.Tar into a temporary folder.
/// No worktree metadata is added to the repository and uncommitted changes can never leak into a snapshot.
/// </summary>
public sealed class GitBranchService(IOptions<ContextManagerOptions> options, ILogger<GitBranchService> log)
{
    private static readonly TimeSpan FetchTimeout = TimeSpan.FromMinutes(3);
    private static readonly TimeSpan ShortTimeout = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan ArchiveTimeout = TimeSpan.FromMinutes(5);

    private readonly ContextManagerOptions _o = options.Value;

    public async Task<string?> GetVersionAsync(CancellationToken ct)
    {
        var r = await ProcessRunner.RunAsync("git", ["--version"], null, ShortTimeout, ct);
        return r.Success ? r.StdOut.Trim() : null;
    }

    /// <summary>Updates remote-tracking refs. Returns false when the remote could not be reached.</summary>
    public async Task<bool> FetchAsync(string repo, CancellationToken ct)
    {
        var r = await Git(repo, FetchTimeout, ct, "fetch", "--quiet", "--no-tags", _o.RemoteName);
        if (!r.Success)
            log.LogWarning("git fetch failed for {Repo}; using last known remote refs (contexts may be stale). {Error}",
                Path.GetFileName(repo), ProcessRunner.SafeForLog(r.StdErr));
        return r.Success;
    }

    /// <summary>Commit SHA of refs/remotes/{remote}/{branch}, or null when that branch does not exist.</summary>
    public async Task<string?> ResolveRemoteBranchAsync(string repo, string branch, CancellationToken ct)
    {
        var r = await Git(repo, ShortTimeout, ct, "rev-parse", "--verify", "--quiet", $"refs/remotes/{_o.RemoteName}/{branch}^{{commit}}");
        var sha = r.StdOut.Trim();
        return r.Success && sha.Length >= 40 ? sha : null;
    }

    /// <summary>Paths of all files in a commit (used to find GitOps files that mention a repository).</summary>
    public async Task<IReadOnlyList<string>> ListFilesAsync(string repo, string sha, CancellationToken ct)
    {
        var r = await Git(repo, ShortTimeout, ct, "ls-tree", "-r", "--name-only", "-z", sha);
        return r.Success ? r.StdOut.Split('\0', StringSplitOptions.RemoveEmptyEntries) : [];
    }

    /// <summary>
    /// Extracts the committed files of <paramref name="sha"/> into <paramref name="targetDir"/>.
    /// Sensitive files (see <see cref="SensitiveFiles"/>) and non-regular entries (symlinks) are skipped.
    /// </summary>
    public async Task<(int Written, int Skipped)> ExportSnapshotAsync(string repo, string sha, string targetDir, CancellationToken ct)
    {
        PathUtil.TryDeleteDirectory(targetDir);
        Directory.CreateDirectory(targetDir);
        var root = Path.GetFullPath(targetDir) + Path.DirectorySeparatorChar;
        int written = 0, skipped = 0;

        async Task Extract(Stream stdout, CancellationToken token)
        {
            await using var reader = new TarReader(stdout, leaveOpen: true);
            while (await reader.GetNextEntryAsync(copyData: false, token) is { } entry)
            {
                if (entry.EntryType is not (TarEntryType.RegularFile or TarEntryType.V7RegularFile)) continue;

                var relative = entry.Name.Replace('/', Path.DirectorySeparatorChar);
                var full = Path.GetFullPath(Path.Combine(root, relative));
                if (!full.StartsWith(root, StringComparison.OrdinalIgnoreCase) || SensitiveFiles.IsSensitive(entry.Name))
                {
                    skipped++;
                    continue;
                }

                Directory.CreateDirectory(Path.GetDirectoryName(full)!);
                await using var file = File.Create(full);
                if (entry.DataStream is not null) await entry.DataStream.CopyToAsync(file, token);
                written++;
            }
        }

        var r = await ProcessRunner.RunAsync("git", ["archive", "--format=tar", sha], repo, ArchiveTimeout, ct, Extract);
        if (!r.Success)
            throw new InvalidOperationException($"git archive failed: {ProcessRunner.SafeForLog(r.StdErr)}");
        return (written, skipped);
    }

    private static Task<ProcessResult> Git(string repo, TimeSpan timeout, CancellationToken ct, params string[] args) =>
        ProcessRunner.RunAsync("git", ["-c", "core.quotepath=off", .. args], repo, timeout, ct);
}

/// <summary>Files that are never copied into a snapshot, whatever the repository's ignore files say.</summary>
public static class SensitiveFiles
{
    private static readonly HashSet<string> Names = new(StringComparer.OrdinalIgnoreCase)
    {
        ".env", ".npmrc", ".pypirc", ".netrc", "_netrc", ".git-credentials", "credentials", "credentials.json",
        "secrets.json", "secrets.yaml", "secrets.yml", "id_rsa", "id_dsa", "id_ecdsa", "id_ed25519",
        "appsettings.local.json", "local.settings.json", "terraform.tfvars", "terraform.tfstate",
    };

    private static readonly string[] Extensions =
    [
        ".pfx", ".p12", ".pem", ".key", ".jks", ".keystore", ".kdbx", ".publishsettings", ".pubxml.user", ".tfstate", ".ppk",
    ];

    private static readonly string[] SafeEnvSuffixes = [".example", ".sample", ".template", ".dist"];

    public static bool IsSensitive(string path)
    {
        var name = Path.GetFileName(path.Replace('\\', '/').TrimEnd('/'));
        if (Names.Contains(name)) return true;
        if (Extensions.Any(e => name.EndsWith(e, StringComparison.OrdinalIgnoreCase))) return true;

        // .env.production, .env.local ... but keep .env.example / .env.sample templates.
        if (name.StartsWith(".env.", StringComparison.OrdinalIgnoreCase))
            return !SafeEnvSuffixes.Any(s => name.EndsWith(s, StringComparison.OrdinalIgnoreCase));

        // appsettings.Development.local.json, *.secrets.json
        return name.EndsWith(".local.json", StringComparison.OrdinalIgnoreCase) ||
               name.EndsWith(".secrets.json", StringComparison.OrdinalIgnoreCase);
    }
}
