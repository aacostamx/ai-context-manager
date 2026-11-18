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
        var all = repos.SelectMany(r => new[] { state.Get(r, ContextEnvironment.Dev), state.Get(r, ContextEnvironment.Prod) }).ToList();
        var lastOk = all.Where(s => s?.Status == ContextStatus.Ok).Max(s => s!.GeneratedUtc);

        sb.AppendLine("# AI Context Index");
        sb.AppendLine();
        sb.AppendLine($"- Latest successful update: **{(lastOk is { } t ? $"{t:yyyy-MM-dd HH:mm} UTC" : "none yet")}**");
        sb.AppendLine($"- Index written: {DateTimeOffset.UtcNow:yyyy-MM-dd HH:mm} UTC");
        sb.AppendLine($"- DEV = `{_o.RemoteName}/{_o.Branches.Dev}`, PROD = `{_o.RemoteName}/{_o.Branches.Prod}` (committed content only)");
        sb.AppendLine($"- Repositories: {repos.Count}");
        sb.AppendLine();
        sb.AppendLine("Status: **OK** published · **STALE** fetch failed, built from last known ref · **NO BRANCH** branch does not exist · " +
                      "**BLOCKED** possible secrets, not published · **FAILED** generation error · **PENDING** not processed yet");
        sb.AppendLine();

        foreach (var solution in repos.Where(r => !r.IsShared).GroupBy(r => r.Solution, StringComparer.OrdinalIgnoreCase))
        {
            EnsureArchitectureFile(solution.Key, solution.ToList());
            sb.AppendLine($"## {solution.Key}");
            sb.AppendLine();
            sb.AppendLine($"[ARCHITECTURE.md]({PathUtil.MarkdownLink($"{solution.Key}/ARCHITECTURE.md")}) (maintained by you)");
            sb.AppendLine();
            AppendTable(sb, "Component", solution, r => r.Component);

            if (deploymentFiles.TryGetValue(solution.Key, out var files) && files.Count > 0)
            {
                sb.AppendLine("Deployment configuration in GitOps (DEV branch, see Shared-Infrastructure):");
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
            AppendTable(sb, "Repository", shared, null);
        }

        PathUtil.WriteAtomically(Path.Combine(_o.OutputPath, IndexFileName), sb.ToString());
    }

    /// <param name="name">Optional first column (component name); null for a plain repository table.</param>
    private void AppendTable(StringBuilder sb, string firstColumn, IEnumerable<RepositoryInfo> repos, Func<RepositoryInfo, string>? name)
    {
        var lead = name is null ? "" : $"| {firstColumn} ";
        sb.AppendLine($"{lead}| Repository | DEV ({_o.Branches.Dev}) | PROD ({_o.Branches.Prod}) |");
        sb.AppendLine(name is null ? "|---|---|---|" : "|---|---|---|---|");
        foreach (var r in repos)
        {
            var first = name is null ? "" : $"| {name(r)} ";
            sb.AppendLine($"{first}| `{r.Name}` | {Cell(r, ContextEnvironment.Dev)} | {Cell(r, ContextEnvironment.Prod)} |");
        }

        sb.AppendLine();
    }

    private string Cell(RepositoryInfo repo, ContextEnvironment env)
    {
        var s = state.Get(repo, env);
        var link = PathUtil.MarkdownLink($"{repo.OutputFolder}/{RepositoryInfo.ContextFileName(env)}");
        var published = File.Exists(repo.ContextFile(_o.OutputPath, env));
        var previous = published ? $" ([previous]({link}))" : "";

        return s?.Status switch
        {
            ContextStatus.Ok => $"[{(s.RemoteStale ? "STALE" : "OK")} `{s.CommitSha?[..7]}`]({link}) {s.GeneratedUtc:yyyy-MM-dd HH:mm}" +
                                (s.Tokens is { } t ? $" · {t:N0} tokens" : ""),
            ContextStatus.MissingBranch => "NO BRANCH",
            ContextStatus.Blocked => $"BLOCKED `{s.CommitSha?[..7]}`{previous}",
            ContextStatus.Failed => $"FAILED `{s.CommitSha?[..7]}`{previous}",
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
            sb.AppendLine($"- **{c.Component}** (`{c.Name}`): [DEV]({PathUtil.MarkdownLink($"{c.ComponentFolder}/DEV-context.md")}) · " +
                          $"[PROD]({PathUtil.MarkdownLink($"{c.ComponentFolder}/PROD-context.md")})");
        sb.AppendLine();
        sb.AppendLine("## How the components talk to each other");
        sb.AppendLine();
        sb.AppendLine("_Describe flows, data stores and integrations here._");
        PathUtil.WriteAtomically(file, sb.ToString());
    }
}
