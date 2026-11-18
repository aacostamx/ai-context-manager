using System.Text;
using Microsoft.Extensions.Options;

namespace AIContextManager;

/// <summary>Writes OutputPath\00-INDEX.md and creates (never overwrites) a starter ARCHITECTURE.md per solution.</summary>
public sealed class IndexWriter(IOptions<ContextManagerOptions> options, StateStore state)
{
    public const string IndexFileName = "00-INDEX.md";
    private readonly ContextManagerOptions _o = options.Value;

    /// <param name="deploymentFiles">Per solution: GitOps file paths that mention one of its repositories.</param>
    public void Write(IReadOnlyList<RepositoryInfo> repos, IReadOnlyDictionary<string, IReadOnlyList<string>> deploymentFiles)
    {
        var sb = new StringBuilder();
        var lastOk = repos.Select(state.Get).Where(s => s?.Status == ContextStatus.Ok).Max(s => s!.GeneratedUtc);

        sb.AppendLine("# AI Context Index");
        sb.AppendLine();
        sb.AppendLine($"- Latest successful update: **{(lastOk is { } t ? $"{t:yyyy-MM-dd HH:mm} UTC" : "none yet")}**");
        sb.AppendLine($"- Repositories: {repos.Count}");
        sb.AppendLine("- Each context is the last commit of the branch checked out in GitHub Desktop (see the Branch column).");
        sb.AppendLine();
        sb.AppendLine("Status: **OK** published · **BLOCKED** possible secrets, not published · **FAILED** generation error · " +
                      "**PENDING** not processed yet");
        sb.AppendLine();

        foreach (var solution in repos.Where(r => !r.IsShared).GroupBy(r => r.Solution, StringComparer.OrdinalIgnoreCase))
        {
            EnsureArchitectureFile(solution.Key, solution.ToList());
            sb.AppendLine($"## {solution.Key}");
            sb.AppendLine();
            sb.AppendLine($"[ARCHITECTURE.md]({PathUtil.MarkdownLink($"{solution.Key}/ARCHITECTURE.md")}) (maintained by you)");
            sb.AppendLine();
            AppendTable(sb, solution);

            if (deploymentFiles.TryGetValue(solution.Key, out var files) && files.Count > 0)
            {
                sb.AppendLine("Deployment configuration in GitOps (see Shared-Infrastructure):");
                sb.AppendLine();
                foreach (var f in files) sb.AppendLine($"- `{f}`");
                sb.AppendLine();
            }
        }

        var shared = repos.Where(r => r.IsShared).ToList();
        if (shared.Count > 0)
        {
            sb.AppendLine($"## {RepositoryInfo.SharedSolution}");
            sb.AppendLine();
            AppendTable(sb, shared);
        }

        PathUtil.WriteAtomically(Path.Combine(_o.OutputPath, IndexFileName), sb.ToString());
    }

    private void AppendTable(StringBuilder sb, IEnumerable<RepositoryInfo> repos)
    {
        sb.AppendLine("| Component | Repository | Branch | Context |");
        sb.AppendLine("|---|---|---|---|");
        foreach (var r in repos)
        {
            var s = state.Get(r);
            sb.AppendLine($"| {r.Component} | `{r.Name}` | {(string.IsNullOrEmpty(s?.Branch) ? "" : $"`{s.Branch}`")} | {Cell(r, s)} |");
        }

        sb.AppendLine();
    }

    private string Cell(RepositoryInfo repo, ContextState? s)
    {
        var link = PathUtil.MarkdownLink(repo.RelativeContextFile);
        var previous = File.Exists(repo.ContextFile(_o.OutputPath)) ? $" ([previous]({link}))" : "";
        var sha = s?.CommitSha is { Length: >= 7 } c ? c[..7] : "";

        return s?.Status switch
        {
            ContextStatus.Ok => $"[OK `{sha}`]({link}) {s.GeneratedUtc:yyyy-MM-dd HH:mm}" + (s.Tokens is { } t ? $" · {t:N0} tokens" : ""),
            ContextStatus.Blocked => $"BLOCKED `{sha}`{previous}",
            ContextStatus.Failed => $"FAILED {s.Message}{previous}",
            _ => "PENDING",
        };
    }

    private void EnsureArchitectureFile(string solution, IReadOnlyList<RepositoryInfo> components)
    {
        var file = Path.Combine(_o.OutputPath, solution, "ARCHITECTURE.md");
        if (File.Exists(file)) return; // never overwrite manual documentation

        var sb = new StringBuilder();
        sb.AppendLine($"# {solution} - Architecture");
        sb.AppendLine();
        sb.AppendLine("> Starter file created once by AIContextManager. It is yours to edit; it is never overwritten.");
        sb.AppendLine();
        sb.AppendLine("## Components");
        sb.AppendLine();
        foreach (var c in components)
            sb.AppendLine($"- **{c.Component}**: [{c.Name}]({PathUtil.MarkdownLink($"{c.Name}-context.md")})");
        sb.AppendLine();
        sb.AppendLine("## How the components talk to each other");
        sb.AppendLine();
        sb.AppendLine("_Describe flows, data stores and integrations here._");
        PathUtil.WriteAtomically(file, sb.ToString());
    }
}
