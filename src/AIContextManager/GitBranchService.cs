using System.Formats.Tar;
using Microsoft.Extensions.Logging;

namespace AIContextManager;

/// <summary>
/// Git access that never changes your working copy:
///  - "git fetch" only updates remote-tracking refs (origin/*), exactly like GitHub Desktop's Fetch origin.
///    It uses the login Git already has (Git Credential Manager remembers your GitHub SSO login);
///  - "git rev-parse" resolves origin/develop, origin/release, origin/main, whatever branch is checked out;
///  - "git archive" streams that commit's files and System.Formats.Tar extracts them into a temporary folder.
/// The repository is never checked out, reset, cleaned or pulled. Uncommitted changes are never included.
/// </summary>
public sealed class GitBranchService(ILogger<GitBranchService> log)
{
    public const string Remote = "origin";

    private static readonly TimeSpan ShortTimeout = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan FetchTimeout = TimeSpan.FromMinutes(3);
    private static readonly TimeSpan ArchiveTimeout = TimeSpan.FromMinutes(5);

    private string _git = "git";

    /// <summary>Finds Git (PATH first, then the copy bundled with GitHub Desktop) and returns its version, or null.</summary>
    public async Task<string?> GetVersionAsync(CancellationToken ct)
    {
        foreach (var candidate in GitCandidates())
        {
            var r = await ProcessRunner.RunAsync(candidate, ["--version"], null, ShortTimeout, ct);
            if (!r.Success) continue;
            _git = candidate;
            if (candidate != "git") log.LogInformation("Git is not on PATH; using GitHub Desktop's Git: {Git}", candidate);
            return r.StdOut.Trim();
        }

        return null;
    }

    /// <summary>Updates remote-tracking refs. Returns null on success, otherwise a short reason (no secrets).</summary>
    public async Task<string?> FetchAsync(string repo, CancellationToken ct)
    {
        var r = await Git(repo, FetchTimeout, ct, "fetch", "--quiet", "--no-tags", Remote);
        return r.Success ? null : ProcessRunner.SafeForLog(r.StdErr, 200);
    }

    /// <summary>Commit SHA of refs/remotes/origin/{branch}, or null when that branch does not exist.</summary>
    public async Task<string?> ResolveRemoteBranchAsync(string repo, string branch, CancellationToken ct)
    {
        var r = await Git(repo, ShortTimeout, ct, "rev-parse", "--verify", "--quiet", $"refs/remotes/{Remote}/{branch}^{{commit}}");
        var sha = r.StdOut.Trim();
        return r.Success && sha.Length >= 40 ? sha : null;
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

        var r = await ProcessRunner.RunAsync(_git, ["archive", "--format=tar", sha], repo, ArchiveTimeout, ct, Extract);
        if (!r.Success)
            throw new InvalidOperationException($"git archive failed: {ProcessRunner.SafeForLog(r.StdErr)}");
        return (written, skipped);
    }

    private Task<ProcessResult> Git(string repo, TimeSpan timeout, CancellationToken ct, params string[] args) =>
        ProcessRunner.RunAsync(_git, ["-c", "core.quotepath=off", .. args], repo, timeout, ct);

    /// <summary>"git" on PATH, then GitHub Desktop's bundled Git (%LOCALAPPDATA%\GitHubDesktop\app-*\resources\app\git\cmd\git.exe), newest first.</summary>
    private static IEnumerable<string> GitCandidates()
    {
        yield return "git";

        var desktop = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "GitHubDesktop");
        if (!Directory.Exists(desktop)) yield break;

        var bundled = Directory.EnumerateDirectories(desktop, "app-*")
            .Select(dir => (Dir: dir, Version: Version.TryParse(Path.GetFileName(dir)[4..], out var v) ? v : new Version()))
            .OrderByDescending(x => x.Version)
            .Select(x => Path.Combine(x.Dir, "resources", "app", "git", "cmd", "git.exe"))
            .Where(File.Exists);
        foreach (var git in bundled) yield return git;
    }
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
