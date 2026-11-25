using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AIContextManager;

/// <summary>
/// One repository + one environment: resolve origin/{branch}, skip if unchanged, snapshot, Repomix, secret scan, publish.
/// Everything happens in StagingPath; only the finished file is copied to OutputPath.
/// </summary>
public sealed class ContextGenerator(
    IOptions<ContextManagerOptions> options,
    GitBranchService git,
    RepomixService repomix,
    ILogger<ContextGenerator> log)
{
    private const string FormatVersion = "4"; // bump when the output format changes to force regeneration
    private readonly ContextManagerOptions _o = options.Value;

    // BlockOnSecrets is part of the fingerprint so blocked repos are retried as soon as the setting is turned off.
    public string Fingerprint => ChangeDetector.Fingerprint([FormatVersion, $"block={_o.BlockOnSecrets}", .. repomix.IgnorePatterns]);

    public async Task<ContextState> ProcessAsync(RepositoryInfo repo, EnvironmentOptions env, ContextState? previous, CancellationToken ct)
    {
        var branch = env.Branch;
        var label = $"{repo.Name} {env.Name} ({GitBranchService.Remote}/{branch})";

        var sha = await git.ResolveRemoteBranchAsync(repo.RootPath, branch, ct);
        if (sha is null)
        {
            if (previous?.Status != ContextStatus.MissingBranch)
                log.LogWarning("{Context}: branch not found, skipped. No other branch is used.", label);
            return new ContextState { Branch = branch, Status = ContextStatus.MissingBranch, Message = $"{GitBranchService.Remote}/{branch} not found" };
        }

        var publishedFile = repo.ContextFile(_o.OutputPath, env);
        if (!ChangeDetector.NeedsRegeneration(previous, branch, sha, File.Exists(publishedFile), Fingerprint))
        {
            log.LogDebug("{Context}: unchanged at {Sha}", label, sha[..7]);
            return previous!;
        }

        log.LogInformation("{Context}: generating context for commit {Sha}", label, sha[..7]);
        var now = DateTimeOffset.UtcNow;
        var work = Path.Combine(_o.StagingPath, "work", $"{repo.Name}-{env.Name}");
        var snapshot = Path.Combine(work, "snapshot");
        var rawOutput = Path.Combine(work, "repomix-output.md");
        var stagedFile = repo.ContextFile(Path.Combine(_o.StagingPath, "contexts"), env);
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

            var content = BuildHeader(repo, env, sha, now, result.TotalTokens, skipped) +
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

    private static string BuildHeader(RepositoryInfo repo, EnvironmentOptions env, string sha, DateTimeOffset now, long? tokens, int skippedSensitive)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"# {repo.Name} - {env.Name} context ({GitBranchService.Remote}/{env.Branch})");
        sb.AppendLine();
        sb.AppendLine("| Field | Value |");
        sb.AppendLine("|---|---|");
        sb.AppendLine($"| Repository | `{repo.Name}` |");
        sb.AppendLine($"| Original path | `{repo.RootPath}` |");
        sb.AppendLine($"| Environment | {env.Name} |");
        sb.AppendLine($"| Branch | `{GitBranchService.Remote}/{env.Branch}` |");
        sb.AppendLine($"| Commit | `{sha}` |");
        sb.AppendLine($"| Generated (UTC) | {now:yyyy-MM-dd HH:mm:ss} |");
        sb.AppendLine($"| Repomix total tokens | {tokens?.ToString("N0") ?? "not reported"} |");
        sb.AppendLine($"| Sensitive files excluded | {skippedSensitive} |");
        sb.AppendLine();
        sb.AppendLine($"> Last commit of `{GitBranchService.Remote}/{env.Branch}` as of the last fetch, independent of the branch checked out locally. " +
                      "Uncommitted changes are not included. Generated by AIContextManager; do not edit.");
        sb.AppendLine();
        sb.AppendLine("---");
        sb.AppendLine();
        return sb.ToString();
    }

    private static string BuildReport(string label, string sha, IReadOnlyList<SecretFinding> findings)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Possible secrets in {label} at commit {sha}. Values are intentionally not shown.");
        sb.AppendLine("To silence a false positive, exclude the file with .repomixignore / ExtraIgnorePatterns.");
        sb.AppendLine();
        foreach (var f in findings) sb.AppendLine($"- {f.Rule}: {f.File} (approx. line {f.Line})");
        return sb.ToString();
    }
}
