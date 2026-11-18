using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AIContextManager;

/// <summary>
/// One repository + one environment: resolve commit, skip if unchanged, snapshot, Repomix, secret scan, publish.
/// Everything happens in StagingPath; only a clean file is copied to OutputPath.
/// </summary>
public sealed class ContextGenerator(
    IOptions<ContextManagerOptions> options,
    GitBranchService git,
    RepomixService repomix,
    ILogger<ContextGenerator> log)
{
    private const string FormatVersion = "2"; // bump when the output format changes to force regeneration
    private readonly ContextManagerOptions _o = options.Value;

    public string Fingerprint => ChangeDetector.Fingerprint([FormatVersion, .. repomix.IgnorePatterns]);

    public async Task<EnvState> ProcessAsync(RepositoryInfo repo, ContextEnvironment env, bool remoteStale, EnvState? previous, CancellationToken ct)
    {
        var branch = _o.BranchFor(env);
        var now = DateTimeOffset.UtcNow;
        var label = $"{repo.Name} {env.ToString().ToUpperInvariant()} ({_o.RemoteName}/{branch})";

        var sha = await git.ResolveRemoteBranchAsync(repo.RootPath, branch, ct);
        if (sha is null)
        {
            if (previous?.Status != ContextStatus.MissingBranch)
                log.LogWarning("{Context}: branch not found. No context is generated and no other branch is used.", label);
            return new EnvState { Branch = branch, Status = ContextStatus.MissingBranch, CheckedUtc = now, Message = $"{_o.RemoteName}/{branch} not found" };
        }

        var publishedFile = repo.ContextFile(_o.OutputPath, env);
        if (!ChangeDetector.NeedsRegeneration(previous, sha, File.Exists(publishedFile), Fingerprint))
        {
            log.LogDebug("{Context}: unchanged at {Sha}", label, sha[..7]);
            return previous! with { CheckedUtc = now, RemoteStale = remoteStale };
        }

        log.LogInformation("{Context}: generating context for commit {Sha}", label, sha[..7]);
        var work = Path.Combine(_o.StagingPath, "work", $"{repo.Name}-{env}");
        var snapshot = Path.Combine(work, "snapshot");
        var rawOutput = Path.Combine(work, "repomix-output.md");
        var stagedFile = repo.ContextFile(Path.Combine(_o.StagingPath, "contexts"), env);
        var reportFile = Path.ChangeExtension(stagedFile, ".secret-scan.txt");

        try
        {
            var (written, skipped) = await git.ExportSnapshotAsync(repo.RootPath, sha, snapshot, ct);
            log.LogDebug("{Context}: snapshot has {Files} files ({Skipped} sensitive files skipped)", label, written, skipped);

            var result = await repomix.RunAsync(snapshot, rawOutput, ct);
            if (!result.Success)
            {
                log.LogError("{Context}: Repomix failed. {Error}", label, result.Error);
                return Failed(branch, sha, now, remoteStale, result.Error ?? "Repomix failed");
            }

            var content = BuildHeader(repo, env, branch, sha, now, remoteStale, result.TotalTokens, skipped) +
                          await File.ReadAllTextAsync(rawOutput, ct);
            PathUtil.WriteAtomically(stagedFile, content);

            var findings = SecretScanner.Scan(content);
            if (findings.Count > 0)
            {
                // Fail closed: do not publish, do not keep the staged copy, report locations only.
                File.Delete(stagedFile);
                PathUtil.WriteAtomically(reportFile, BuildReport(label, sha, findings));
                log.LogError("{Context}: publication BLOCKED, {Count} possible secret(s) found. Locations (no values): {Report}",
                    label, findings.Count, reportFile);
                return new EnvState
                {
                    Branch = branch, CommitSha = sha, Status = ContextStatus.Blocked, CheckedUtc = now, RemoteStale = remoteStale,
                    Fingerprint = Fingerprint, Message = $"{findings.Count} possible secret(s); see {reportFile}",
                };
            }

            if (File.Exists(reportFile)) File.Delete(reportFile);
            Directory.CreateDirectory(Path.GetDirectoryName(publishedFile)!);
            File.Copy(stagedFile, publishedFile + ".tmp", overwrite: true);
            File.Move(publishedFile + ".tmp", publishedFile, overwrite: true);

            log.LogInformation("{Context}: published {File} ({Tokens} tokens)", label, publishedFile,
                result.TotalTokens?.ToString("N0") ?? "unknown");
            return new EnvState
            {
                Branch = branch, CommitSha = sha, Status = ContextStatus.Ok, GeneratedUtc = now, CheckedUtc = now,
                Tokens = result.TotalTokens, RemoteStale = remoteStale, Fingerprint = Fingerprint,
            };
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            log.LogError("{Context}: generation failed. {Error}", label, ProcessRunner.SafeForLog(ex.Message));
            return Failed(branch, sha, now, remoteStale, ProcessRunner.SafeForLog(ex.Message));
        }
        finally
        {
            PathUtil.TryDeleteDirectory(work);
        }
    }

    private EnvState Failed(string branch, string sha, DateTimeOffset now, bool stale, string message) => new()
    {
        Branch = branch, CommitSha = sha, Status = ContextStatus.Failed, CheckedUtc = now, RemoteStale = stale,
        Fingerprint = Fingerprint, Message = message,
    };

    private string BuildHeader(RepositoryInfo repo, ContextEnvironment env, string branch, string sha, DateTimeOffset now,
        bool stale, long? tokens, int skippedSensitive)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"# {repo.Name} - {env.ToString().ToUpperInvariant()} context");
        sb.AppendLine();
        sb.AppendLine("| Field | Value |");
        sb.AppendLine("|---|---|");
        sb.AppendLine($"| Repository | `{repo.Name}` |");
        sb.AppendLine($"| Original path | `{repo.RootPath}` |");
        sb.AppendLine($"| Solution / component | {repo.Solution} / {repo.Component} |");
        sb.AppendLine($"| Branch | `{_o.RemoteName}/{branch}` |");
        sb.AppendLine($"| Commit | `{sha}` |");
        sb.AppendLine($"| Generated (UTC) | {now:yyyy-MM-dd HH:mm:ss} |");
        sb.AppendLine($"| Repomix total tokens | {tokens?.ToString("N0") ?? "not reported"} |");
        sb.AppendLine($"| Sensitive files excluded | {skippedSensitive} |");
        if (stale)
            sb.AppendLine("| Freshness | **Potentially stale**: `git fetch` failed, built from the last known remote ref |");
        sb.AppendLine();
        sb.AppendLine("> Committed content only. Uncommitted local changes are not included. Generated by AIContextManager; do not edit.");
        sb.AppendLine();
        sb.AppendLine("---");
        sb.AppendLine();
        return sb.ToString();
    }

    private static string BuildReport(string label, string sha, IReadOnlyList<SecretFinding> findings)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Publication blocked for {label} at commit {sha}.");
        sb.AppendLine("Possible secrets were detected. Values are intentionally not shown.");
        sb.AppendLine("Fix: remove the secret from the repository, or exclude the file with .repomixignore / ExtraIgnorePatterns.");
        sb.AppendLine();
        foreach (var f in findings) sb.AppendLine($"- {f.Rule}: {f.File} (approx. line {f.Line})");
        return sb.ToString();
    }
}
