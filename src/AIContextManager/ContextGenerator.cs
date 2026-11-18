using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AIContextManager;

/// <summary>
/// One repository: read the checked-out branch and commit, skip if unchanged, snapshot, Repomix, secret scan, publish.
/// Everything happens in StagingPath; only a clean file is copied to OutputPath.
/// </summary>
public sealed class ContextGenerator(
    IOptions<ContextManagerOptions> options,
    GitBranchService git,
    RepomixService repomix,
    ILogger<ContextGenerator> log)
{
    private const string FormatVersion = "3"; // bump when the output format changes to force regeneration
    private readonly ContextManagerOptions _o = options.Value;

    // BlockOnSecrets is part of the fingerprint so blocked repos are retried as soon as the setting is turned off.
    public string Fingerprint => ChangeDetector.Fingerprint([FormatVersion, $"block={_o.BlockOnSecrets}", .. repomix.IgnorePatterns]);

    public async Task<ContextState> ProcessAsync(RepositoryInfo repo, ContextState? previous, CancellationToken ct)
    {
        var current = await git.GetCurrentAsync(repo.RootPath, ct);
        if (current is not var (branch, sha))
        {
            if (previous?.Status != ContextStatus.Failed) log.LogWarning("{Repo}: no commits yet, nothing to generate.", repo.Name);
            return new ContextState { Status = ContextStatus.Failed, Message = "No commits" };
        }

        var label = $"{repo.Name} ({branch})";
        var publishedFile = repo.ContextFile(_o.OutputPath);
        if (!ChangeDetector.NeedsRegeneration(previous, sha, File.Exists(publishedFile), Fingerprint))
        {
            log.LogDebug("{Context}: unchanged at {Sha}", label, sha[..7]);
            return previous!;
        }

        log.LogInformation("{Context}: generating context for commit {Sha}", label, sha[..7]);
        var now = DateTimeOffset.UtcNow;
        var work = Path.Combine(_o.StagingPath, "work", repo.Name);
        var snapshot = Path.Combine(work, "snapshot");
        var rawOutput = Path.Combine(work, "repomix-output.md");
        var stagedFile = repo.ContextFile(Path.Combine(_o.StagingPath, "contexts"));
        var reportFile = Path.ChangeExtension(stagedFile, ".secret-scan.txt");
        var state = new ContextState { Branch = branch, CommitSha = sha, Fingerprint = Fingerprint };

        try
        {
            var (_, skipped) = await git.ExportSnapshotAsync(repo.RootPath, sha, snapshot, ct);

            var result = await repomix.RunAsync(snapshot, rawOutput, ct);
            if (!result.Success)
            {
                log.LogError("{Context}: Repomix failed. {Error}", label, result.Error);
                return state with { Status = ContextStatus.Failed, Message = result.Error };
            }

            var content = BuildHeader(repo, branch, sha, now, result.TotalTokens, skipped) +
                          await File.ReadAllTextAsync(rawOutput, ct);
            PathUtil.WriteAtomically(stagedFile, content);

            var findings = SecretScanner.Scan(content);
            if (findings.Count == 0)
            {
                if (File.Exists(reportFile)) File.Delete(reportFile);
            }
            else if (_o.BlockOnSecrets)
            {
                // Fail closed: do not publish, do not keep the staged copy, report locations only.
                File.Delete(stagedFile);
                PathUtil.WriteAtomically(reportFile, BuildReport(label, sha, findings));
                log.LogError("{Context}: publication BLOCKED, {Count} possible secret(s) found. Locations (no values): {Report}",
                    label, findings.Count, reportFile);
                return state with { Status = ContextStatus.Blocked, Message = $"{findings.Count} possible secret(s); see {reportFile}" };
            }
            else
            {
                PathUtil.WriteAtomically(reportFile, BuildReport(label, sha, findings));
                log.LogWarning("{Context}: {Count} possible secret(s) found, published anyway (BlockOnSecrets is false). Locations: {Report}",
                    label, findings.Count, reportFile);
            }

            Directory.CreateDirectory(Path.GetDirectoryName(publishedFile)!);
            File.Copy(stagedFile, publishedFile + ".tmp", overwrite: true);
            File.Move(publishedFile + ".tmp", publishedFile, overwrite: true);

            log.LogInformation("{Context}: published {File} ({Tokens} tokens)", label, publishedFile,
                result.TotalTokens?.ToString("N0") ?? "unknown");
            return state with { Status = ContextStatus.Ok, GeneratedUtc = now, Tokens = result.TotalTokens };
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            log.LogError("{Context}: generation failed. {Error}", label, ProcessRunner.SafeForLog(ex.Message));
            return state with { Status = ContextStatus.Failed, Message = ProcessRunner.SafeForLog(ex.Message) };
        }
        finally
        {
            PathUtil.TryDeleteDirectory(work);
        }
    }

    private static string BuildHeader(RepositoryInfo repo, string branch, string sha, DateTimeOffset now, long? tokens, int skippedSensitive)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"# {repo.Name} context ({branch})");
        sb.AppendLine();
        sb.AppendLine("| Field | Value |");
        sb.AppendLine("|---|---|");
        sb.AppendLine($"| Repository | `{repo.Name}` |");
        sb.AppendLine($"| Original path | `{repo.RootPath}` |");
        sb.AppendLine($"| Solution / component | {repo.Solution} / {repo.Component} |");
        sb.AppendLine($"| Branch | `{branch}` |");
        sb.AppendLine($"| Commit | `{sha}` |");
        sb.AppendLine($"| Generated (UTC) | {now:yyyy-MM-dd HH:mm:ss} |");
        sb.AppendLine($"| Repomix total tokens | {tokens?.ToString("N0") ?? "not reported"} |");
        sb.AppendLine($"| Sensitive files excluded | {skippedSensitive} |");
        sb.AppendLine();
        sb.AppendLine("> Last commit of the branch checked out in GitHub Desktop. Uncommitted changes are not included. " +
                      "Generated by AIContextManager; do not edit.");
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
