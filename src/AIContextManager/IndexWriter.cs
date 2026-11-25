using System.Text;
using Microsoft.Extensions.Options;

namespace AIContextManager;

/// <summary>Writes OutputPath\00-INDEX.md: one row per repository, one column per environment.</summary>
public sealed class IndexWriter(IOptions<ContextManagerOptions> options, StateStore state)
{
    public const string IndexFileName = "00-INDEX.md";
    private readonly ContextManagerOptions _o = options.Value;

    public void Write(IReadOnlyList<RepositoryInfo> repos)
    {
        var envs = _o.Environments;
        var lastOk = repos.SelectMany(r => envs.Select(e => state.Get(r, e)))
            .Where(s => s?.Status == ContextStatus.Ok).Max(s => s!.GeneratedUtc);

        var sb = new StringBuilder();
        sb.AppendLine("# AI Context Index");
        sb.AppendLine();
        sb.AppendLine($"- Latest successful update: **{(lastOk is { } t ? $"{t:yyyy-MM-dd HH:mm} UTC" : "none yet")}**");
        sb.AppendLine($"- Repositories: {repos.Count}");
        sb.AppendLine("- " + string.Join(", ", envs.Select(e => $"{e.Name} = `{GitBranchService.Remote}/{e.Branch}`")) +
                      " (independent of the branch checked out locally)");
        sb.AppendLine();
        sb.AppendLine("Status: **OK** published · **NO BRANCH** branch does not exist in that repository · " +
                      "**BLOCKED** possible secrets, not published · **FAILED** generation error · **PENDING** not processed yet");
        sb.AppendLine();
        sb.AppendLine("| Repository | " + string.Join(" | ", envs.Select(e => $"{e.Name} ({e.Branch})")) + " |");
        sb.AppendLine("|---|" + string.Concat(envs.Select(_ => "---|")));
        foreach (var r in repos)
            sb.AppendLine($"| `{r.Name}` | " + string.Join(" | ", envs.Select(e => Cell(r, e))) + " |");

        PathUtil.WriteAtomically(Path.Combine(_o.OutputPath, IndexFileName), sb.ToString());
    }

    private string Cell(RepositoryInfo repo, EnvironmentOptions env)
    {
        var s = state.Get(repo, env);
        var link = PathUtil.MarkdownLink(repo.RelativeContextFile(env));
        var previous = File.Exists(repo.ContextFile(_o.OutputPath, env)) ? $" ([previous]({link}))" : "";
        var sha = s?.CommitSha is { Length: >= 7 } c ? c[..7] : "";

        return s?.Status switch
        {
            ContextStatus.Ok => $"[OK `{sha}`]({link}) {s.GeneratedUtc:yyyy-MM-dd}" + (s.Tokens is { } t ? $" · {t:N0} tok" : ""),
            ContextStatus.MissingBranch => "NO BRANCH",
            ContextStatus.Blocked => $"BLOCKED `{sha}`{previous}",
            ContextStatus.Failed => $"FAILED{previous}",
            _ => "PENDING",
        };
    }
}
