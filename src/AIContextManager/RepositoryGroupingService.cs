using Microsoft.Extensions.Options;

namespace AIContextManager;

/// <summary>
/// Groups repositories into logical solutions using naming conventions only.
/// Rules (conservative on purpose):
///  1. GitOps repositories, *-helm-chart(s) and configured SharedRepositories go to "Shared-Infrastructure".
///  2. A known suffix (-api, -ui, -data-api, -worker, -sync, -cronjob, -cron-job) is split off: "base" + component.
///  3. Repositories whose base is identical are one solution (intsol-biography-api + intsol-biography-ui).
///  4. A repository may join another solution only when its base is exactly that solution plus ONE extra word,
///     e.g. intsol-sa-rules-salesforce-api joins intsol-sa-rules as "Salesforce API".
///  5. Everything else stays its own solution. Nothing is merged on a shared prefix alone.
/// </summary>
public sealed class RepositoryGroupingService(IOptions<ContextManagerOptions> options)
{
    // Longest first so "-data-api" wins over "-api".
    private static readonly (string Suffix, string Label)[] Suffixes =
    [
        ("-data-api", "Data API"),
        ("-cron-job", "CronJob"),
        ("-cronjob", "CronJob"),
        ("-worker", "Worker"),
        ("-sync", "Sync"),
        ("-api", "API"),
        ("-ui", "UI"),
    ];

    private readonly ContextManagerOptions _o = options.Value;

    public IReadOnlyList<RepositoryInfo> Group(IEnumerable<string> repositoryPaths)
    {
        var repos = repositoryPaths
            .Select(p => (Path: p, Name: Path.GetFileName(Path.TrimEndingDirectorySeparator(p))))
            .ToList();

        var parsed = repos.Select(r => (r.Path, r.Name, Shared: IsShared(r.Name), Split: SplitSuffix(r.Name))).ToList();

        // How many non-shared repositories share each base name.
        var baseCounts = parsed
            .Where(p => !p.Shared && p.Split is not null)
            .GroupBy(p => p.Split!.Value.Base, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.Count(), StringComparer.OrdinalIgnoreCase);

        var result = new List<RepositoryInfo>();
        foreach (var p in parsed)
        {
            if (p.Shared)
            {
                result.Add(new RepositoryInfo(p.Name, p.Path, RepositoryInfo.SharedSolution, p.Name, true));
                continue;
            }

            if (p.Split is not { } split)
            {
                result.Add(new RepositoryInfo(p.Name, p.Path, p.Name, "Main", false));
                continue;
            }

            var (solution, component) = (split.Base, split.Label);
            if (baseCounts[split.Base] < 2)
            {
                // Rule 4: base = <other solution base> + "-" + one word.
                var dash = split.Base.LastIndexOf('-');
                if (dash > 0)
                {
                    var parent = split.Base[..dash];
                    var word = split.Base[(dash + 1)..];
                    if (baseCounts.ContainsKey(parent) && word.Length > 0)
                    {
                        solution = parent;
                        component = $"{char.ToUpperInvariant(word[0])}{word[1..]} {split.Label}";
                    }
                }
            }

            result.Add(new RepositoryInfo(p.Name, p.Path, solution, component, false));
        }

        return result
            .OrderBy(r => r.IsShared)
            .ThenBy(r => r.Solution, StringComparer.OrdinalIgnoreCase)
            .ThenBy(r => r.Component, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public bool IsShared(string name) =>
        _o.SharedRepositories.Contains(name, StringComparer.OrdinalIgnoreCase) ||
        name.Contains("gitops", StringComparison.OrdinalIgnoreCase) ||
        name.EndsWith("-helm-chart", StringComparison.OrdinalIgnoreCase) ||
        name.EndsWith("-helm-charts", StringComparison.OrdinalIgnoreCase);

    private static (string Base, string Label)? SplitSuffix(string name)
    {
        foreach (var (suffix, label) in Suffixes)
        {
            if (name.Length > suffix.Length && name.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
                return (name[..^suffix.Length], label);
        }

        return null;
    }
}
