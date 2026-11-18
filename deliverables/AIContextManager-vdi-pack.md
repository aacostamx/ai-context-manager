# AIContextManager - VDI copy pack

Create each file below at the given path (relative to a new folder `ai-context-manager`), then run `dotnet test` and `dotnet run --project src/AIContextManager -- --once`. Targets .NET 8. Sample output is not included; see the README.

Files:
- `.gitignore`
- `AIContextManager.sln`
- `README.md`
- `src/AIContextManager/AIContextManager.csproj`
- `src/AIContextManager/appsettings.json`
- `src/AIContextManager/ChangeDetection.cs`
- `src/AIContextManager/ContextGenerator.cs`
- `src/AIContextManager/ContextManagerOptions.cs`
- `src/AIContextManager/ContextMonitorService.cs`
- `src/AIContextManager/GitBranchService.cs`
- `src/AIContextManager/IndexWriter.cs`
- `src/AIContextManager/Models.cs`
- `src/AIContextManager/PathUtil.cs`
- `src/AIContextManager/ProcessRunner.cs`
- `src/AIContextManager/Program.cs`
- `src/AIContextManager/RepomixService.cs`
- `src/AIContextManager/RepositoryDiscoveryService.cs`
- `src/AIContextManager/RepositoryGroupingService.cs`
- `src/AIContextManager/SecretScanner.cs`
- `src/AIContextManager/StateStore.cs`
- `tests/AIContextManager.Tests/AIContextManager.Tests.csproj`
- `tests/AIContextManager.Tests/ChangeDetectionTests.cs`
- `tests/AIContextManager.Tests/GitAndDiscoveryTests.cs`
- `tests/AIContextManager.Tests/GroupingTests.cs`
- `tests/AIContextManager.Tests/SecretScannerTests.cs`
- `tests/AIContextManager.Tests/TestRepos.cs`

## .gitignore

````text
bin/
obj/
.vs/
*.user
TestResults/
````

## AIContextManager.sln

````text

Microsoft Visual Studio Solution File, Format Version 12.00
# Visual Studio Version 17
VisualStudioVersion = 17.0.31903.59
MinimumVisualStudioVersion = 10.0.40219.1
Project("{2150E333-8FDC-42A3-9474-1A3956D46DE8}") = "src", "src", "{827E0CD3-B72D-47B6-A68D-7590B98EB39B}"
EndProject
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "AIContextManager", "src\AIContextManager\AIContextManager.csproj", "{DF920379-FDA2-421B-9FC3-683AD44957C7}"
EndProject
Project("{2150E333-8FDC-42A3-9474-1A3956D46DE8}") = "tests", "tests", "{0AB3BF05-4346-4AA6-1389-037BE0695223}"
EndProject
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "AIContextManager.Tests", "tests\AIContextManager.Tests\AIContextManager.Tests.csproj", "{A87849C4-84DB-4C50-BDD4-FBFD69B1F5EC}"
EndProject
Global
	GlobalSection(SolutionConfigurationPlatforms) = preSolution
		Debug|Any CPU = Debug|Any CPU
		Debug|x64 = Debug|x64
		Debug|x86 = Debug|x86
		Release|Any CPU = Release|Any CPU
		Release|x64 = Release|x64
		Release|x86 = Release|x86
	EndGlobalSection
	GlobalSection(ProjectConfigurationPlatforms) = postSolution
		{DF920379-FDA2-421B-9FC3-683AD44957C7}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{DF920379-FDA2-421B-9FC3-683AD44957C7}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{DF920379-FDA2-421B-9FC3-683AD44957C7}.Debug|x64.ActiveCfg = Debug|Any CPU
		{DF920379-FDA2-421B-9FC3-683AD44957C7}.Debug|x64.Build.0 = Debug|Any CPU
		{DF920379-FDA2-421B-9FC3-683AD44957C7}.Debug|x86.ActiveCfg = Debug|Any CPU
		{DF920379-FDA2-421B-9FC3-683AD44957C7}.Debug|x86.Build.0 = Debug|Any CPU
		{DF920379-FDA2-421B-9FC3-683AD44957C7}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{DF920379-FDA2-421B-9FC3-683AD44957C7}.Release|Any CPU.Build.0 = Release|Any CPU
		{DF920379-FDA2-421B-9FC3-683AD44957C7}.Release|x64.ActiveCfg = Release|Any CPU
		{DF920379-FDA2-421B-9FC3-683AD44957C7}.Release|x64.Build.0 = Release|Any CPU
		{DF920379-FDA2-421B-9FC3-683AD44957C7}.Release|x86.ActiveCfg = Release|Any CPU
		{DF920379-FDA2-421B-9FC3-683AD44957C7}.Release|x86.Build.0 = Release|Any CPU
		{A87849C4-84DB-4C50-BDD4-FBFD69B1F5EC}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{A87849C4-84DB-4C50-BDD4-FBFD69B1F5EC}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{A87849C4-84DB-4C50-BDD4-FBFD69B1F5EC}.Debug|x64.ActiveCfg = Debug|Any CPU
		{A87849C4-84DB-4C50-BDD4-FBFD69B1F5EC}.Debug|x64.Build.0 = Debug|Any CPU
		{A87849C4-84DB-4C50-BDD4-FBFD69B1F5EC}.Debug|x86.ActiveCfg = Debug|Any CPU
		{A87849C4-84DB-4C50-BDD4-FBFD69B1F5EC}.Debug|x86.Build.0 = Debug|Any CPU
		{A87849C4-84DB-4C50-BDD4-FBFD69B1F5EC}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{A87849C4-84DB-4C50-BDD4-FBFD69B1F5EC}.Release|Any CPU.Build.0 = Release|Any CPU
		{A87849C4-84DB-4C50-BDD4-FBFD69B1F5EC}.Release|x64.ActiveCfg = Release|Any CPU
		{A87849C4-84DB-4C50-BDD4-FBFD69B1F5EC}.Release|x64.Build.0 = Release|Any CPU
		{A87849C4-84DB-4C50-BDD4-FBFD69B1F5EC}.Release|x86.ActiveCfg = Release|Any CPU
		{A87849C4-84DB-4C50-BDD4-FBFD69B1F5EC}.Release|x86.Build.0 = Release|Any CPU
	EndGlobalSection
	GlobalSection(SolutionProperties) = preSolution
		HideSolutionNode = FALSE
	EndGlobalSection
	GlobalSection(NestedProjects) = preSolution
		{DF920379-FDA2-421B-9FC3-683AD44957C7} = {827E0CD3-B72D-47B6-A68D-7590B98EB39B}
		{A87849C4-84DB-4C50-BDD4-FBFD69B1F5EC} = {0AB3BF05-4346-4AA6-1389-037BE0695223}
	EndGlobalSection
EndGlobal
````

## README.md

````markdown
# AIContextManager

A small .NET 8 console app that finds every Git repository under your `source` folder and keeps **one** Markdown
context per repository, built with [Repomix](https://github.com/yamadashy/repomix).

The context is the last commit of the branch you have checked out in GitHub Desktop. Want DEV? Pick `develop` in
GitHub Desktop's **Current Branch** dropdown. Want PROD? Pick `main`. The context regenerates by itself within seconds.

Contexts are grouped by solution with a navigable `00-INDEX.md`, ready to attach to GitHub Copilot,
Microsoft 365 Copilot, Copilot Notebooks or any assistant that reads Markdown.

No repository list, no scripts, no database, no GitHub credentials. It only creates files; it never calls an AI
service and never contacts GitHub.

## Requirements

- .NET 8 SDK (or runtime, to run a published build)
- GitHub Desktop (its bundled Git is found automatically) or Git for Windows on `PATH`
- Repomix: `npm install -g repomix` (or set `RepomixCommand` to its full path)

## Run

From Visual Studio: open `AIContextManager.sln`, set `AIContextManager` as startup project, edit
`src/AIContextManager/appsettings.json`, press F5.

From a terminal:

```bash
dotnet run --project src/AIContextManager
```

One pass and exit:

```bash
dotnet run --project src/AIContextManager -- --once
```

Any setting can be overridden on the command line:

```bash
dotnet run --project src/AIContextManager -- --ContextManager:SourcePath=C:\Users\O7U\source --ContextManager:OutputPath=C:\Users\O7U\AI-Context
```

Tests:

```bash
dotnet test
```

## Configuration (`appsettings.json`)

| Setting | Default | Meaning |
|---|---|---|
| `SourcePath` | `C:\Users\O7U\source` | Folder with your repositories. Required. |
| `ExcludedFolders` | `["Repos"]` | Folders ignored completely, with everything inside (no discovery, no watching). Relative to `SourcePath`, or absolute. |
| `OutputPath` | `C:\Users\O7U\AI-Context` | Where contexts and `00-INDEX.md` go. Must not be inside `SourcePath`. |
| `StagingPath` | `%LOCALAPPDATA%\AIContextManager\staging` | Local work area: snapshots, scan reports, `state.json`. Never cloud-synced. |
| `AutoDiscover` | `true` | Pick up added/removed repositories while running. |
| `WatchChanges` | `true` | React to branch switches and new commits via `FileSystemWatcher`. |
| `GitCheckMinutes` | `15` | Safety-net re-check of all repositories (no network). |
| `DebounceSeconds` | `10` | Quiet time after file events before a check. |
| `RepomixCommand` | `repomix` | Name or full path of the Repomix CLI. |
| `AllowCloudPublish` | `false` | Must be `true` to write into a OneDrive/SharePoint folder. Set only if your policy allows full source in that location. |
| `SharedRepositories` | `[]` | Extra names to treat as shared infrastructure. |
| `ExtraIgnorePatterns` | `[]` | Extra Repomix `--ignore` globs for all repositories. |

Configuration is validated at startup; the app exits with a clear message when something is wrong.

## How it works

1. **Discovery.** Walks `SourcePath` (default depth 3) and treats a folder as a repository when it has a `.git`
   folder with `HEAD`, or a `.git` file starting with `gitdir:` (worktrees). `ExcludedFolders`, `bin`, `obj`, `.vs`,
   `node_modules`, `dist`, `build` and `.git` are never traversed.
2. **Grouping** (naming only, conservative):
   - `*gitops*`, `*-helm-chart` and `SharedRepositories` go to `Shared-Infrastructure/`.
   - Suffixes `-api`, `-ui`, `-data-api`, `-worker`, `-sync`, `-cronjob`, `-cron-job` give the component.
   - Repos with the same base form one solution (`intsol-biography-api` + `-ui` + `-sync`).
   - A repo joins another solution only if its base is that solution plus one word
     (`intsol-sa-rules-salesforce-api` joins `intsol-sa-rules` as "Salesforce API").
   - Anything else stays its own solution. A shared text prefix alone never merges repos.
3. **Read-only Git.** The app reads the checked-out branch and its last commit (`git rev-parse HEAD`) and streams
   that commit with `git archive` into a temporary folder in `StagingPath`. It never runs `fetch`, `pull`,
   `checkout`, `reset` or `clean`, never creates worktrees, and uncommitted changes are never included.
4. **Repomix.** One run per repository, Markdown style, respecting the snapshot's `.gitignore` and `.repomixignore`,
   plus built-in ignores (build output, lock files, logs, previously generated contexts).
5. **Security, fail-closed.**
   - Known sensitive files are never copied into the snapshot: `.env*` (except `.env.example`), `*.pfx`, `*.pem`,
     `*.key`, `id_rsa*`, `.npmrc`, `secrets.json`, `*.local.json`, `terraform.tfstate`, and similar.
   - The generated file is scanned (private keys, AWS/GitHub/Slack/Azure keys, JWTs, passwords in connection strings,
     hard-coded `password`/`secret`/`apiKey` values; placeholders like `#{X}#`, `${X}`, `""` are allowed).
   - Any finding **blocks** publication. The report in `StagingPath\contexts\...\*.secret-scan.txt` lists rule, file
     and line only, never the value. Fix the repository or exclude the file, then the next commit is processed.
   - Only clean files are copied to `OutputPath`.
6. **Change detection.** `state.json` remembers the commit and settings used for each context. A context is rebuilt
   only when the checked-out commit changes (branch switch, commit, pull), the settings change, or the published
   file was deleted. The watcher reacts to `.git/HEAD`, `.git/refs/heads/*` and `packed-refs`, and to folders
   appearing/disappearing; editing working files does nothing.
7. **Index.** `00-INDEX.md` shows the latest successful update and, per repository, its branch, commit, date and
   token count, plus GitOps files that mention the solution's repositories. A starter `ARCHITECTURE.md` is created
   per solution only if missing; it is never overwritten.

## Output

```text
AI-Context/
├── 00-INDEX.md
├── intsol-dna-list-matching/
│   ├── ARCHITECTURE.md
│   ├── intsol-dna-list-matching-api-context.md
│   └── intsol-dna-list-matching-ui-context.md
└── Shared-Infrastructure/
    └── intsol-gitops-context.md
```

See [`samples/dna`](samples/dna/00-INDEX.md) for output generated from synthetic DNA repositories.

## Microsoft 365 Copilot / OneDrive

Putting `OutputPath` inside an approved corporate OneDrive folder makes the files available to Microsoft 365 Copilot,
but only according to that folder's permissions; local files are not visible to Copilot by themselves.
Because contexts contain full source, the app refuses a OneDrive/SharePoint `OutputPath` unless
`AllowCloudPublish` is `true`. Turn it on only if corporate policy permits it.

## Limitations

- Grouping is name-based. An unusual name ends up as its own solution rather than being merged.
- The secret scanner is pattern-based. It errs on blocking; it cannot prove a file is secret-free.
- Repositories that are worktrees (`.git` file) are rechecked on the periodic check, not by the watcher.
- Uncommitted changes are not included; commit (no need to push) to see them in the context.
````

## src/AIContextManager/AIContextManager.csproj

````xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <RootNamespace>AIContextManager</RootNamespace>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.Extensions.Hosting" Version="8.0.1" />
  </ItemGroup>

  <ItemGroup>
    <None Update="appsettings.json" CopyToOutputDirectory="PreserveNewest" />
  </ItemGroup>

  <ItemGroup>
    <InternalsVisibleTo Include="AIContextManager.Tests" />
  </ItemGroup>

</Project>
````

## src/AIContextManager/appsettings.json

````json
{
  "ContextManager": {
    "SourcePath": "C:\\Users\\O7U\\source",
    "ExcludedFolders": [ "Repos" ],
    "OutputPath": "C:\\Users\\O7U\\AI-Context",
    "AutoDiscover": true,
    "WatchChanges": true,
    "GitCheckMinutes": 15,
    "DebounceSeconds": 10,
    "RepomixCommand": "repomix",
    "AllowCloudPublish": false
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.Hosting.Lifetime": "Warning"
    }
  }
}
````

## src/AIContextManager/ChangeDetection.cs

````csharp
using System.Security.Cryptography;
using System.Text;

namespace AIContextManager;

public static class ChangeDetector
{
    /// <summary>
    /// A context only depends on the checked-out commit and the generation settings, so it is regenerated only
    /// when one of those changed (new commit, pull or branch switch) or the published file disappeared.
    /// </summary>
    public static bool NeedsRegeneration(ContextState? previous, string currentSha, bool outputExists, string fingerprint)
    {
        if (previous is null) return true;
        if (!string.Equals(previous.CommitSha, currentSha, StringComparison.OrdinalIgnoreCase)) return true;
        if (!string.Equals(previous.Fingerprint, fingerprint, StringComparison.Ordinal)) return true;

        return previous.Status switch
        {
            ContextStatus.Ok => !outputExists,       // someone deleted the published file
            ContextStatus.Blocked => false,          // same commit would be blocked again; wait for a new commit
            _ => true,                               // Pending or Failed: try again
        };
    }

    public static string Fingerprint(IEnumerable<string> settings)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(string.Join('\n', settings)));
        return Convert.ToHexString(bytes)[..16];
    }
}

/// <summary>
/// Collects file-system signals and releases a key only after it has been quiet for the debounce period,
/// so a burst of events (a fetch touches many refs) results in one check.
/// </summary>
public sealed class ChangeDebouncer(TimeSpan quietPeriod, TimeProvider time)
{
    private readonly Dictionary<string, DateTimeOffset> _lastSignal = new(StringComparer.OrdinalIgnoreCase);
    private readonly object _lock = new();

    public void Signal(string key)
    {
        lock (_lock) _lastSignal[key] = time.GetUtcNow();
    }

    public IReadOnlyList<string> TakeReady()
    {
        var now = time.GetUtcNow();
        lock (_lock)
        {
            var ready = _lastSignal.Where(kv => now - kv.Value >= quietPeriod).Select(kv => kv.Key).ToList();
            foreach (var key in ready) _lastSignal.Remove(key);
            return ready;
        }
    }
}
````

## src/AIContextManager/ContextGenerator.cs

````csharp
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

    public string Fingerprint => ChangeDetector.Fingerprint([FormatVersion, .. repomix.IgnorePatterns]);

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
            if (findings.Count > 0)
            {
                // Fail closed: do not publish, do not keep the staged copy, report locations only.
                File.Delete(stagedFile);
                PathUtil.WriteAtomically(reportFile, BuildReport(label, sha, findings));
                log.LogError("{Context}: publication BLOCKED, {Count} possible secret(s) found. Locations (no values): {Report}",
                    label, findings.Count, reportFile);
                return state with { Status = ContextStatus.Blocked, Message = $"{findings.Count} possible secret(s); see {reportFile}" };
            }

            if (File.Exists(reportFile)) File.Delete(reportFile);
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
````

## src/AIContextManager/ContextManagerOptions.cs

````csharp
using Microsoft.Extensions.Options;

namespace AIContextManager;

public sealed class ContextManagerOptions
{
    public const string SectionName = "ContextManager";

    /// <summary>Folder that contains the Git repositories (for example C:\source).</summary>
    public string SourcePath { get; set; } = "";

    /// <summary>Where the published Markdown contexts and 00-INDEX.md are written.</summary>
    public string OutputPath { get; set; } = "";

    /// <summary>Local working area (snapshots, raw Repomix output, state). Defaults to %LOCALAPPDATA%\AIContextManager\staging.</summary>
    public string StagingPath { get; set; } = "";

    /// <summary>Re-scan SourcePath when folders are added or removed. When false, repositories are discovered once at startup.</summary>
    public bool AutoDiscover { get; set; } = true;

    /// <summary>Use FileSystemWatcher to react to new repositories and Git reference updates.</summary>
    public bool WatchChanges { get; set; } = true;

    /// <summary>Minutes between checks of the local origin/develop and origin/main refs of all repositories (no network).</summary>
    public int GitCheckMinutes { get; set; } = 15;

    /// <summary>Quiet period after the last file-system event before a repository is checked.</summary>
    public int DebounceSeconds { get; set; } = 10;

    /// <summary>Repomix executable name or full path.</summary>
    public string RepomixCommand { get; set; } = "repomix";

    public int RepomixTimeoutMinutes { get; set; } = 10;

    /// <summary>How deep below SourcePath to look for repositories.</summary>
    public int MaxDiscoveryDepth { get; set; } = 3;

    /// <summary>
    /// Corporate policy switch. Full-code contexts are never written to a OneDrive/SharePoint-synced
    /// OutputPath unless this is explicitly set to true.
    /// </summary>
    public bool AllowCloudPublish { get; set; }

    /// <summary>Extra repository names to treat as shared infrastructure (GitOps and *-helm-chart are detected automatically).</summary>
    public string[] SharedRepositories { get; set; } = [];

    /// <summary>Extra glob patterns passed to Repomix --ignore for every repository.</summary>
    public string[] ExtraIgnorePatterns { get; set; } = [];

    /// <summary>
    /// Folders skipped completely, with everything inside them: no discovery, no watching.
    /// Relative paths are resolved against SourcePath (e.g. "Repos" = SourcePath\Repos); absolute paths are also accepted.
    /// </summary>
    public string[] ExcludedFolders { get; set; } = [];

    /// <summary>True when <paramref name="fullPath"/> is one of the ExcludedFolders or inside one.</summary>
    public bool IsExcluded(string fullPath) =>
        ExcludedFolders.Any(f => !string.IsNullOrWhiteSpace(f) &&
                                 PathUtil.IsSameOrUnder(fullPath, Path.IsPathRooted(f) ? f : Path.Combine(SourcePath, f)));
}

public sealed class ContextManagerOptionsSetup : IPostConfigureOptions<ContextManagerOptions>
{
    public void PostConfigure(string? name, ContextManagerOptions o)
    {
        if (string.IsNullOrWhiteSpace(o.StagingPath))
        {
            o.StagingPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AIContextManager", "staging");
        }

        o.SourcePath = Normalize(o.SourcePath);
        o.OutputPath = Normalize(o.OutputPath);
        o.StagingPath = Normalize(o.StagingPath);
    }

    private static string Normalize(string path) =>
        string.IsNullOrWhiteSpace(path)
            ? ""
            : Path.TrimEndingDirectorySeparator(Path.GetFullPath(Environment.ExpandEnvironmentVariables(path)));
}

public sealed class ContextManagerOptionsValidator : IValidateOptions<ContextManagerOptions>
{
    public ValidateOptionsResult Validate(string? name, ContextManagerOptions o)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(o.SourcePath)) errors.Add("SourcePath is required.");
        else if (!Directory.Exists(o.SourcePath)) errors.Add($"SourcePath does not exist: {o.SourcePath}");

        if (string.IsNullOrWhiteSpace(o.OutputPath)) errors.Add("OutputPath is required.");

        if (errors.Count == 0)
        {
            if (PathUtil.IsSameOrUnder(o.OutputPath, o.SourcePath))
                errors.Add("OutputPath must not be inside SourcePath (generated files would end up next to your repositories).");
            if (PathUtil.IsSameOrUnder(o.StagingPath, o.SourcePath))
                errors.Add("StagingPath must not be inside SourcePath.");
            if (PathUtil.IsSameOrUnder(o.StagingPath, o.OutputPath))
                errors.Add("StagingPath must not be inside OutputPath (unvalidated snapshots would be published).");
            if (PathUtil.IsCloudSynced(o.StagingPath))
                errors.Add("StagingPath must be a local folder, not a OneDrive/SharePoint-synced folder.");
            if (PathUtil.IsCloudSynced(o.OutputPath) && !o.AllowCloudPublish)
                errors.Add("OutputPath is inside a OneDrive/SharePoint-synced folder. Full-code contexts are only published there when " +
                           "'AllowCloudPublish' is true. Set it only if your corporate policy permits it.");
        }

        if (o.GitCheckMinutes < 1) errors.Add("GitCheckMinutes must be at least 1.");
        if (o.DebounceSeconds < 1) errors.Add("DebounceSeconds must be at least 1.");
        if (o.RepomixTimeoutMinutes < 1) errors.Add("RepomixTimeoutMinutes must be at least 1.");
        if (o.MaxDiscoveryDepth is < 1 or > 10) errors.Add("MaxDiscoveryDepth must be between 1 and 10.");
        if (string.IsNullOrWhiteSpace(o.RepomixCommand)) errors.Add("RepomixCommand is required.");

        return errors.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(errors);
    }
}
````

## src/AIContextManager/ContextMonitorService.cs

````csharp
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AIContextManager;

public sealed record RunMode(bool Once);

/// <summary>
/// Main loop:
///  - startup: discover, generate what changed, write the index;
///  - FileSystemWatcher: new/removed repositories, branch switches and new commits are debounced and checked;
///  - every GitCheckMinutes: check all repositories (safety net for missed events).
/// One context per repository: the last commit of the branch checked out in GitHub Desktop.
/// This service never contacts GitHub. Working-tree edits are ignored on purpose.
/// </summary>
public sealed class ContextMonitorService(
    IOptions<ContextManagerOptions> options,
    RunMode mode,
    RepositoryDiscoveryService discovery,
    RepositoryGroupingService grouping,
    GitBranchService git,
    RepomixService repomix,
    ContextGenerator generator,
    StateStore state,
    IndexWriter index,
    IHostApplicationLifetime lifetime,
    ILogger<ContextMonitorService> log) : BackgroundService
{
    private const string DiscoverKey = "<discover>";
    private readonly ContextManagerOptions _o = options.Value;
    private readonly ChangeDebouncer _debouncer = new(TimeSpan.FromSeconds(options.Value.DebounceSeconds), TimeProvider.System);
    private readonly Dictionary<string, IReadOnlyList<string>> _gitOpsFilesCache = new(StringComparer.OrdinalIgnoreCase);
    private volatile IReadOnlyList<RepositoryInfo> _repos = [];
    private FileSystemWatcher? _watcher;

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        await Task.Yield();
        try
        {
            if (!await CheckDependenciesAsync(ct))
            {
                Environment.ExitCode = 2;
                lifetime.StopApplication();
                return;
            }

            state.Load();
            Discover();
            await ProcessAsync(_repos, ct);

            if (mode.Once)
            {
                log.LogInformation("Single run finished (--once).");
                lifetime.StopApplication();
                return;
            }

            if (_o.WatchChanges) StartWatcher();
            log.LogInformation("Monitoring {Source}. Local ref check every {Minutes} min{Watch}. Press Ctrl+C to stop.",
                _o.SourcePath, _o.GitCheckMinutes, _o.WatchChanges ? ", file watcher on" : "");

            var nextGitCheck = DateTimeOffset.UtcNow.AddMinutes(_o.GitCheckMinutes);
            while (!ct.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(1), ct);

                var ready = _debouncer.TakeReady();
                if (ready.Contains(DiscoverKey))
                {
                    var before = _repos.Select(r => r.RootPath).ToHashSet(StringComparer.OrdinalIgnoreCase);
                    Discover();
                    var added = _repos.Where(r => !before.Contains(r.RootPath)).ToList();
                    await ProcessAsync(added, ct, forceIndex: true);
                }

                var changed = _repos.Where(r => ready.Contains(r.RootPath)).ToList();
                if (changed.Count > 0) await ProcessAsync(changed, ct);

                if (DateTimeOffset.UtcNow >= nextGitCheck)
                {
                    await ProcessAsync(_repos, ct);
                    nextGitCheck = DateTimeOffset.UtcNow.AddMinutes(_o.GitCheckMinutes);
                }
            }
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
        }
        catch (Exception ex)
        {
            log.LogCritical(ex, "AIContextManager stopped because of an unexpected error.");
            Environment.ExitCode = 1;
            lifetime.StopApplication();
        }
    }

    public override void Dispose()
    {
        _watcher?.Dispose();
        base.Dispose();
    }

    private async Task<bool> CheckDependenciesAsync(CancellationToken ct)
    {
        var gitVersion = await git.GetVersionAsync(ct);
        if (gitVersion is null)
        {
            log.LogCritical("Git was not found: not on PATH and no GitHub Desktop installation in %LOCALAPPDATA%\\GitHubDesktop.");
            return false;
        }

        var repomixVersion = await repomix.GetVersionAsync(ct);
        if (repomixVersion is null)
        {
            log.LogCritical("Repomix was not found ('{Command}'). Install it with 'npm install -g repomix' or set ContextManager:RepomixCommand to its full path.",
                _o.RepomixCommand);
            return false;
        }

        log.LogInformation("Using {Git}, repomix {Repomix}. Source: {Source} -> Output: {Output} (staging: {Staging})",
            gitVersion, repomixVersion, _o.SourcePath, _o.OutputPath, _o.StagingPath);
        return true;
    }

    private void Discover()
    {
        var previous = _repos;
        _repos = grouping.Group(discovery.Discover());

        var added = _repos.Select(r => r.Name).Except(previous.Select(r => r.Name)).ToList();
        var removed = previous.Select(r => r.Name).Except(_repos.Select(r => r.Name)).ToList();
        if (previous.Count == 0)
        {
            foreach (var r in _repos)
                log.LogInformation("Found {Repo} -> {File}", r.Name, r.RelativeContextFile);
        }
        else
        {
            foreach (var name in added) log.LogInformation("New repository discovered: {Repo}", name);
            foreach (var name in removed) log.LogInformation("Repository no longer present: {Repo} (published files are kept)", name);
        }
    }

    private async Task ProcessAsync(IReadOnlyList<RepositoryInfo> repos, CancellationToken ct, bool forceIndex = false)
    {
        var changed = forceIndex;
        foreach (var repo in repos)
        {
            ct.ThrowIfCancellationRequested();
            var before = state.Get(repo);
            var after = await generator.ProcessAsync(repo, before, ct);
            state.Set(repo, after);
            changed |= before != after;
        }

        state.Save();
        if (changed || !File.Exists(Path.Combine(_o.OutputPath, IndexWriter.IndexFileName)))
            index.Write(_repos, await FindDeploymentFilesAsync(ct));
    }

    /// <summary>For each solution, GitOps files (checked-out branch) whose path mentions one of its repositories.</summary>
    private async Task<IReadOnlyDictionary<string, IReadOnlyList<string>>> FindDeploymentFilesAsync(CancellationToken ct)
    {
        var allFiles = new List<string>();
        foreach (var gitops in _repos.Where(r => r.IsShared && r.Name.Contains("gitops", StringComparison.OrdinalIgnoreCase)))
        {
            var sha = state.Get(gitops)?.CommitSha;
            if (sha is null) continue;
            if (!_gitOpsFilesCache.TryGetValue(sha, out var files))
                _gitOpsFilesCache[sha] = files = await git.ListFilesAsync(gitops.RootPath, sha, ct);
            allFiles.AddRange(files.Select(f => $"{gitops.Name}/{f}"));
        }

        var result = new Dictionary<string, IReadOnlyList<string>>(StringComparer.OrdinalIgnoreCase);
        foreach (var solution in _repos.Where(r => !r.IsShared).GroupBy(r => r.Solution, StringComparer.OrdinalIgnoreCase))
        {
            var names = solution.Select(r => r.Name).ToList();
            result[solution.Key] = allFiles
                .Where(f => names.Any(n => f.Contains(n, StringComparison.OrdinalIgnoreCase)))
                .Take(25)
                .ToList();
        }

        return result;
    }

    private void StartWatcher()
    {
        _watcher = new FileSystemWatcher(_o.SourcePath)
        {
            IncludeSubdirectories = true,
            NotifyFilter = NotifyFilters.DirectoryName | NotifyFilters.FileName | NotifyFilters.LastWrite,
            InternalBufferSize = 64 * 1024,
        };
        _watcher.Created += (_, e) => OnFileSystemEvent(e.FullPath);
        _watcher.Deleted += (_, e) => OnFileSystemEvent(e.FullPath);
        _watcher.Changed += (_, e) => OnFileSystemEvent(e.FullPath);
        _watcher.Renamed += (_, e) =>
        {
            OnFileSystemEvent(e.OldFullPath);
            OnFileSystemEvent(e.FullPath);
        };
        _watcher.Error += (_, e) =>
        {
            log.LogWarning("File watcher overflow ({Error}); scheduling a full check.", e.GetException().Message);
            if (_o.AutoDiscover) _debouncer.Signal(DiscoverKey);
            foreach (var r in _repos) _debouncer.Signal(r.RootPath);
        };
        _watcher.EnableRaisingEvents = true;
    }

    internal void OnFileSystemEvent(string fullPath)
    {
        if (_o.IsExcluded(fullPath)) return; // ExcludedFolders are invisible to the app

        var repo = _repos.FirstOrDefault(r => PathUtil.IsSameOrUnder(fullPath, r.RootPath));
        if (repo is null)
        {
            // Something changed outside any known repository: maybe a new clone or a deleted folder.
            var depth = Path.GetRelativePath(_o.SourcePath, fullPath).Split(Path.DirectorySeparatorChar).Length;
            var segments = fullPath.Split(Path.DirectorySeparatorChar);
            if (_o.AutoDiscover && depth <= _o.MaxDiscoveryDepth + 1 &&
                !segments.Any(s => RepositoryDiscoveryService.SkippedFolders.Contains(s) && s != ".git"))
                _debouncer.Signal(DiscoverKey);
            return;
        }

        var inRepo = Path.GetRelativePath(repo.RootPath, fullPath).Replace('\\', '/');
        if (inRepo is "." or ".git")
        {
            if (_o.AutoDiscover) _debouncer.Signal(DiscoverKey); // repository folder or its .git removed/renamed
            return;
        }

        // Branch switch (.git/HEAD) or new commit/pull on a local branch (refs/heads, packed-refs).
        // Working-tree edits are ignored: the context is the last commit.
        if (inRepo.Equals(".git/HEAD", StringComparison.OrdinalIgnoreCase) ||
            inRepo.StartsWith(".git/refs/heads/", StringComparison.OrdinalIgnoreCase) ||
            inRepo.Equals(".git/packed-refs", StringComparison.OrdinalIgnoreCase))
            _debouncer.Signal(repo.RootPath);
    }
}
````

## src/AIContextManager/GitBranchService.cs

````csharp
using System.Formats.Tar;
using Microsoft.Extensions.Logging;

namespace AIContextManager;

/// <summary>
/// Read-only Git access. Needs no credentials and no configuration; it never contacts GitHub.
/// The context follows the branch you have checked out in GitHub Desktop (Current Branch dropdown):
///  - "git rev-parse" reads the current branch and its last commit (HEAD);
///  - "git archive" streams that commit's files and System.Formats.Tar extracts them into a temporary folder.
/// The repository is never checked out, reset, cleaned, pulled or fetched. Uncommitted changes are not included.
/// </summary>
public sealed class GitBranchService(ILogger<GitBranchService> log)
{
    private static readonly TimeSpan ShortTimeout = TimeSpan.FromSeconds(30);
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

    /// <summary>The checked-out branch ("(detached)" when none) and its commit, or null when the repository has no commits.</summary>
    public async Task<(string Branch, string Sha)?> GetCurrentAsync(string repo, CancellationToken ct)
    {
        var sha = await Git(repo, ShortTimeout, ct, "rev-parse", "--verify", "--quiet", "HEAD^{commit}");
        if (!sha.Success || sha.StdOut.Trim().Length < 40) return null;

        var branch = await Git(repo, ShortTimeout, ct, "symbolic-ref", "--short", "--quiet", "HEAD");
        return (branch.Success ? branch.StdOut.Trim() : "(detached)", sha.StdOut.Trim());
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
````

## src/AIContextManager/IndexWriter.cs

````csharp
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
````

## src/AIContextManager/Models.cs

````csharp
using System.Text.Json.Serialization;

namespace AIContextManager;

[JsonConverter(typeof(JsonStringEnumConverter<ContextStatus>))]
public enum ContextStatus
{
    Pending,
    Ok,
    Blocked,
    Failed,
}

/// <summary>A discovered repository and where it belongs in the output tree.</summary>
public sealed record RepositoryInfo(string Name, string RootPath, string Solution, string Component, bool IsShared)
{
    public const string SharedSolution = "Shared-Infrastructure";

    /// <summary>Context file relative to OutputPath, for example "intsol-dna-list-matching/intsol-dna-list-matching-api-context.md".</summary>
    public string RelativeContextFile => $"{(IsShared ? SharedSolution : Solution)}/{Name}-context.md";

    /// <summary>Full path of the context file below <paramref name="root"/> (OutputPath or staging).</summary>
    public string ContextFile(string root) => Path.Combine(root, RelativeContextFile.Replace('/', Path.DirectorySeparatorChar));
}

/// <summary>What we know about a repository's context. Persisted in state.json.</summary>
public sealed record ContextState
{
    public string Branch { get; init; } = "";
    public string? CommitSha { get; init; }
    public ContextStatus Status { get; init; } = ContextStatus.Pending;
    public string? Message { get; init; }
    public DateTimeOffset? GeneratedUtc { get; init; }
    public long? Tokens { get; init; }

    /// <summary>Hash of the generation settings; a change forces regeneration.</summary>
    public string? Fingerprint { get; init; }
}
````

## src/AIContextManager/PathUtil.cs

````csharp
namespace AIContextManager;

internal static class PathUtil
{
    public static bool IsSameOrUnder(string path, string root)
    {
        if (string.IsNullOrEmpty(path) || string.IsNullOrEmpty(root)) return false;
        var p = Path.TrimEndingDirectorySeparator(Path.GetFullPath(path)) + Path.DirectorySeparatorChar;
        var r = Path.TrimEndingDirectorySeparator(Path.GetFullPath(root)) + Path.DirectorySeparatorChar;
        return p.StartsWith(r, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>Best-effort detection of OneDrive / SharePoint synced folders.</summary>
    public static bool IsCloudSynced(string path)
    {
        if (string.IsNullOrEmpty(path)) return false;
        foreach (var variable in new[] { "OneDrive", "OneDriveCommercial", "OneDriveConsumer" })
        {
            var root = Environment.GetEnvironmentVariable(variable);
            if (!string.IsNullOrWhiteSpace(root) && IsSameOrUnder(path, root)) return true;
        }

        return Path.GetFullPath(path)
            .Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            .Any(s => s.StartsWith("OneDrive", StringComparison.OrdinalIgnoreCase) ||
                      s.StartsWith("SharePoint", StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>Relative Markdown link with forward slashes and escaped segments.</summary>
    public static string MarkdownLink(string relativePath) =>
        string.Join('/', relativePath.Replace('\\', '/').Split('/').Select(Uri.EscapeDataString));

    public static void WriteAtomically(string file, string content)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(file)!);
        var tmp = file + ".tmp";
        File.WriteAllText(tmp, content);
        File.Move(tmp, file, overwrite: true);
    }

    public static void TryDeleteDirectory(string dir)
    {
        try
        {
            if (!Directory.Exists(dir)) return;
            foreach (var f in Directory.EnumerateFiles(dir, "*", SearchOption.AllDirectories))
                File.SetAttributes(f, FileAttributes.Normal);
            Directory.Delete(dir, recursive: true);
        }
        catch (IOException) { }
        catch (UnauthorizedAccessException) { }
    }
}
````

## src/AIContextManager/ProcessRunner.cs

````csharp
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace AIContextManager;

public sealed record ProcessResult(int ExitCode, string StdOut, string StdErr)
{
    public const int NotFound = -1;
    public const int TimedOut = -2;
    public bool Success => ExitCode == 0;
}

/// <summary>Thin wrapper around System.Diagnostics.Process. No shell is involved; arguments are passed as a list.</summary>
public static partial class ProcessRunner
{
    public static async Task<ProcessResult> RunAsync(
        string fileName,
        IEnumerable<string> arguments,
        string? workingDirectory,
        TimeSpan timeout,
        CancellationToken ct,
        Func<Stream, CancellationToken, Task>? stdoutConsumer = null)
    {
        var psi = new ProcessStartInfo(fileName)
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            StandardOutputEncoding = stdoutConsumer is null ? Encoding.UTF8 : null,
            StandardErrorEncoding = Encoding.UTF8,
        };
        if (workingDirectory is not null) psi.WorkingDirectory = workingDirectory;
        foreach (var a in arguments) psi.ArgumentList.Add(a);

        // Never block on a credential prompt; never colour output we parse.
        psi.Environment["GIT_TERMINAL_PROMPT"] = "0";
        psi.Environment["GCM_INTERACTIVE"] = "never";
        psi.Environment["NO_COLOR"] = "1";
        psi.Environment["FORCE_COLOR"] = "0";

        using var process = new Process { StartInfo = psi };
        try
        {
            process.Start();
        }
        catch (Win32Exception ex)
        {
            return new ProcessResult(ProcessResult.NotFound, "", $"Could not start '{fileName}': {ex.Message}");
        }

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(timeout);

        var stderrTask = process.StandardError.ReadToEndAsync(cts.Token);
        try
        {
            var stdout = "";
            if (stdoutConsumer is null)
            {
                stdout = await process.StandardOutput.ReadToEndAsync(cts.Token);
            }
            else
            {
                await stdoutConsumer(process.StandardOutput.BaseStream, cts.Token);
                await process.StandardOutput.BaseStream.CopyToAsync(Stream.Null, cts.Token);
            }

            await process.WaitForExitAsync(cts.Token);
            return new ProcessResult(process.ExitCode, stdout, await stderrTask);
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            Kill(process);
            return new ProcessResult(ProcessResult.TimedOut, "", $"'{fileName}' timed out after {timeout.TotalMinutes:0.#} min.");
        }
        catch
        {
            Kill(process);
            throw;
        }
    }

    /// <summary>Removes credentials embedded in URLs (https://user:token@host) and trims long text before logging.</summary>
    public static string SafeForLog(string? text, int max = 400)
    {
        var clean = UrlCredentials().Replace(text ?? "", "://***@").Trim();
        return clean.Length <= max ? clean : clean[..max] + "...";
    }

    private static void Kill(Process p)
    {
        try { if (!p.HasExited) p.Kill(entireProcessTree: true); }
        catch { /* already gone */ }
    }

    [GeneratedRegex(@"://[^/\s@]+@")]
    private static partial Regex UrlCredentials();
}
````

## src/AIContextManager/Program.cs

````csharp
using AIContextManager;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

// Usage:
//   dotnet run --project src/AIContextManager                 -> run and keep monitoring
//   dotnet run --project src/AIContextManager -- --once       -> one pass, then exit
//   ... -- --ContextManager:SourcePath=C:\source              -> override any setting
var once = args.Any(a => a.Equals("--once", StringComparison.OrdinalIgnoreCase));
var hostArgs = args.Where(a => !a.Equals("--once", StringComparison.OrdinalIgnoreCase)).ToArray();

var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
{
    Args = hostArgs,
    ContentRootPath = AppContext.BaseDirectory, // find appsettings.json next to the exe, whatever the current folder
});

builder.Logging.ClearProviders();
builder.Logging.AddSimpleConsole(o =>
{
    o.SingleLine = true;
    o.TimestampFormat = "HH:mm:ss ";
});

builder.Services.AddOptions<ContextManagerOptions>()
    .Bind(builder.Configuration.GetSection(ContextManagerOptions.SectionName))
    .ValidateOnStart();
builder.Services.AddSingleton<IPostConfigureOptions<ContextManagerOptions>, ContextManagerOptionsSetup>();
builder.Services.AddSingleton<IValidateOptions<ContextManagerOptions>, ContextManagerOptionsValidator>();

builder.Services.AddSingleton(new RunMode(once));
builder.Services.AddSingleton<RepositoryDiscoveryService>();
builder.Services.AddSingleton<RepositoryGroupingService>();
builder.Services.AddSingleton<GitBranchService>();
builder.Services.AddSingleton<RepomixService>();
builder.Services.AddSingleton<ContextGenerator>();
builder.Services.AddSingleton<StateStore>();
builder.Services.AddSingleton<IndexWriter>();
builder.Services.AddHostedService<ContextMonitorService>();

try
{
    await builder.Build().RunAsync();
}
catch (OptionsValidationException ex)
{
    Console.Error.WriteLine("Invalid configuration (appsettings.json, section 'ContextManager'):");
    foreach (var failure in ex.Failures) Console.Error.WriteLine($"  - {failure}");
    return 1;
}

return Environment.ExitCode;
````

## src/AIContextManager/RepomixService.cs

````csharp
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AIContextManager;

public sealed record RepomixResult(bool Success, long? TotalTokens, string? Error);

/// <summary>Runs the Repomix CLI once per snapshot. Repomix itself respects .gitignore, .repomixignore and its own security check.</summary>
public sealed partial class RepomixService(IOptions<ContextManagerOptions> options, ILogger<RepomixService> log)
{
    /// <summary>Always ignored, on top of .gitignore and .repomixignore.</summary>
    public static readonly string[] DefaultIgnorePatterns =
    [
        "**/.git/**", "**/.vs/**", "**/bin/**", "**/obj/**", "**/node_modules/**", "**/dist/**", "**/build/**",
        "**/coverage/**", "**/TestResults/**", "**/*.tmp", "**/*.temp", "**/*.log", "**/*.min.js", "**/*.map",
        "**/package-lock.json", "**/yarn.lock", "**/pnpm-lock.yaml",
        "**/*-context.md", "**/repomix-output.*", "**/00-INDEX.md",
    ];

    private readonly ContextManagerOptions _o = options.Value;
    private (string File, string[] Prefix)? _command;

    public IReadOnlyList<string> IgnorePatterns => [.. DefaultIgnorePatterns, .. _o.ExtraIgnorePatterns];

    public async Task<string?> GetVersionAsync(CancellationToken ct)
    {
        var (file, prefix) = Command;
        var r = await ProcessRunner.RunAsync(file, [.. prefix, "--version"], null, TimeSpan.FromMinutes(1), ct);
        if (r.Success) return r.StdOut.Trim();
        log.LogDebug("repomix --version failed: {Error}", ProcessRunner.SafeForLog(r.StdErr));
        return null;
    }

    public async Task<RepomixResult> RunAsync(string snapshotDir, string outputFile, CancellationToken ct)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(outputFile)!);
        if (File.Exists(outputFile)) File.Delete(outputFile);

        var (file, prefix) = Command;
        string[] args =
        [
            .. prefix, ".",
            "--style", "markdown",
            "--no-file-summary", // drop Repomix's generic boilerplate; our header carries the metadata
            "--output", outputFile,
            "--ignore", string.Join(',', IgnorePatterns),
        ];

        var r = await ProcessRunner.RunAsync(file, args, snapshotDir, TimeSpan.FromMinutes(_o.RepomixTimeoutMinutes), ct);
        if (!r.Success || !File.Exists(outputFile))
        {
            var detail = string.IsNullOrWhiteSpace(r.StdErr) ? r.StdOut : r.StdErr;
            return new RepomixResult(false, null, $"repomix exit code {r.ExitCode}: {ProcessRunner.SafeForLog(detail)}");
        }

        var match = TotalTokens().Match(r.StdOut);
        long? tokens = match.Success && long.TryParse(match.Groups[1].Value.Replace(",", ""), out var t) ? t : null;
        return new RepomixResult(true, tokens, null);
    }

    private (string File, string[] Prefix) Command => _command ??= ResolveCommand(_o.RepomixCommand);

    /// <summary>
    /// "repomix" installed with npm on Windows is a .cmd shim. Process.Start cannot run .cmd files directly, so we
    /// prefer "node &lt;repomix.cjs&gt;" (no shell, no quoting issues) and fall back to "cmd.exe /d /c &lt;shim&gt;".
    /// </summary>
    internal static (string File, string[] Prefix) ResolveCommand(string command)
    {
        var resolved = FindOnPath(command);
        if (resolved is null) return (command, []);

        var ext = Path.GetExtension(resolved);
        if (!ext.Equals(".cmd", StringComparison.OrdinalIgnoreCase) && !ext.Equals(".bat", StringComparison.OrdinalIgnoreCase))
            return (resolved, []);

        // Global install: %APPDATA%\npm\repomix.cmd + node_modules\repomix. Local install: node_modules\.bin\repomix.cmd.
        var shimDir = Path.GetDirectoryName(resolved)!;
        var script = new[]
            {
                Path.Combine(shimDir, "node_modules", "repomix", "bin", "repomix.cjs"),
                Path.Combine(shimDir, "..", "repomix", "bin", "repomix.cjs"),
            }
            .Select(Path.GetFullPath)
            .FirstOrDefault(File.Exists);
        var node = FindOnPath("node");
        if (script is not null && node is not null) return (node, [script]);

        return ("cmd.exe", ["/d", "/c", resolved]);
    }

    private static string? FindOnPath(string command)
    {
        string[] extensions = OperatingSystem.IsWindows()
            ? ["", .. (Environment.GetEnvironmentVariable("PATHEXT") ?? ".EXE;.CMD;.BAT").Split(';', StringSplitOptions.RemoveEmptyEntries)]
            : [""];

        IEnumerable<string> candidates = Path.IsPathRooted(command) || command.Contains(Path.DirectorySeparatorChar)
            ? extensions.Select(e => command + e)
            : (Environment.GetEnvironmentVariable("PATH") ?? "")
                .Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries)
                .SelectMany(dir => extensions.Select(e => Path.Combine(dir.Trim('"'), command + e)));

        foreach (var c in candidates)
        {
            try
            {
                // On Windows a bare "repomix" (no extension) is the POSIX shell script; skip it.
                if (File.Exists(c) && (!OperatingSystem.IsWindows() || Path.HasExtension(c))) return c;
            }
            catch (ArgumentException) { }
        }

        return null;
    }

    [GeneratedRegex(@"Total Tokens:\s*([\d,]+)", RegexOptions.IgnoreCase)]
    private static partial Regex TotalTokens();
}
````

## src/AIContextManager/RepositoryDiscoveryService.cs

````csharp
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
            if (_o.IsExcluded(child.FullName)) continue; // ExcludedFolders, e.g. SourcePath\Repos and everything inside
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
````

## src/AIContextManager/RepositoryGroupingService.cs

````csharp
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
````

## src/AIContextManager/SecretScanner.cs

````csharp
using System.Text.RegularExpressions;

namespace AIContextManager;

/// <summary>Where a possible secret was found. The value itself is never kept.</summary>
public sealed record SecretFinding(string Rule, string File, int Line);

/// <summary>
/// Fail-closed scan of a generated context before it is published. Any finding blocks publication.
/// It deliberately does not redact: a blocked file is never published, the user excludes the file
/// (.repomixignore / ExtraIgnorePatterns) or removes the secret from the repository.
/// </summary>
public static partial class SecretScanner
{
    private static readonly (string Rule, Regex Pattern, bool CheckValue)[] Rules =
    [
        ("Private key", PrivateKey(), false),
        ("AWS access key", AwsAccessKey(), false),
        ("AWS secret key", AwsSecretKey(), false),
        ("GitHub token", GitHubToken(), false),
        ("Slack token", SlackToken(), false),
        ("Azure storage/service bus key", AzureKey(), false),
        ("JSON Web Token", Jwt(), false),
        ("Password in connection string", ConnectionStringPassword(), true),
        ("Hard-coded credential", KeyValueSecret(), true),
    ];

    private static readonly HashSet<string> PlaceholderWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "changeme", "change-me", "password", "secret", "dummy", "example", "sample", "placeholder", "redacted",
        "null", "none", "empty", "test", "todo", "string", "your-password", "yourpassword", "your-secret", "notset",
    };

    public static IReadOnlyList<SecretFinding> Scan(string markdown)
    {
        var findings = new List<SecretFinding>();
        var currentFile = "(header)";
        var lineInFile = 0;

        using var reader = new StringReader(markdown);
        while (reader.ReadLine() is { } line)
        {
            lineInFile++;
            if (line.StartsWith("## File: ", StringComparison.Ordinal))
            {
                currentFile = line["## File: ".Length..].Trim();
                lineInFile = -1; // the opening ``` fence follows; first code line becomes 1
                continue;
            }

            foreach (var (rule, pattern, checkValue) in Rules)
            {
                foreach (Match m in pattern.Matches(line))
                {
                    if (checkValue && IsPlaceholder(m.Groups["value"].Value)) continue;
                    findings.Add(new SecretFinding(rule, currentFile, Math.Max(lineInFile, 1)));
                    break;
                }
            }
        }

        return findings;
    }

    internal static bool IsPlaceholder(string value)
    {
        var v = value.Trim().Trim('"', '\'');
        if (v.Length < 6) return true;
        if (v.StartsWith("${") || v.StartsWith("{{") || v.StartsWith("#{") || v.StartsWith("$(") ||
            v.StartsWith('<') || v.StartsWith('%') || v.StartsWith('[') || v.StartsWith("__")) return true;
        if (v.All(c => c is '*' or 'x' or 'X' or '.' or '-')) return true;
        if (PlaceholderWords.Contains(v)) return true;
        return v.StartsWith("your", StringComparison.OrdinalIgnoreCase) || v.Contains("example", StringComparison.OrdinalIgnoreCase);
    }

    [GeneratedRegex(@"-----BEGIN (?:RSA |DSA |EC |OPENSSH |PGP |ENCRYPTED )?PRIVATE KEY(?: BLOCK)?-----")]
    private static partial Regex PrivateKey();

    [GeneratedRegex(@"\b(?:AKIA|ASIA)[0-9A-Z]{16}\b")]
    private static partial Regex AwsAccessKey();

    [GeneratedRegex(@"(?i)aws_?secret_?access_?key[""']?\s*[:=]\s*[""']?[A-Za-z0-9/+=]{40}\b")]
    private static partial Regex AwsSecretKey();

    [GeneratedRegex(@"\b(?:ghp|gho|ghu|ghs|ghr)_[A-Za-z0-9]{36}\b|\bgithub_pat_[A-Za-z0-9_]{50,}")]
    private static partial Regex GitHubToken();

    [GeneratedRegex(@"\bxox[abprs]-[A-Za-z0-9-]{10,}")]
    private static partial Regex SlackToken();

    [GeneratedRegex(@"(?i)(?:AccountKey|SharedAccessKey)\s*=\s*[A-Za-z0-9+/]{30,}={0,2}")]
    private static partial Regex AzureKey();

    [GeneratedRegex(@"\beyJ[A-Za-z0-9_-]{10,}\.eyJ[A-Za-z0-9_-]{10,}\.[A-Za-z0-9_-]{10,}")]
    private static partial Regex Jwt();

    [GeneratedRegex(@"(?i)\b(?:password|pwd)\s*=\s*(?<value>[^;""'\s]+)")]
    private static partial Regex ConnectionStringPassword();

    [GeneratedRegex(@"(?i)[""']?\b(?:password|passwd|secret|client_?secret|api_?key|access_?token|auth_?token|private_?key)[""']?\s*[:=]\s*[""'](?<value>[^""'\s]{6,})[""']")]
    private static partial Regex KeyValueSecret();
}
````

## src/AIContextManager/StateStore.cs

````csharp
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AIContextManager;

/// <summary>Remembers the last processed branch/commit per repository in StagingPath\state.json.</summary>
public sealed class StateStore(IOptions<ContextManagerOptions> options, ILogger<StateStore> log)
{
    private static readonly JsonSerializerOptions Json = new() { WriteIndented = true };
    private readonly string _file = Path.Combine(options.Value.StagingPath, "state.json");
    private Dictionary<string, ContextState> _states = new(StringComparer.OrdinalIgnoreCase);

    public void Load()
    {
        try
        {
            if (File.Exists(_file))
            {
                var loaded = JsonSerializer.Deserialize<Dictionary<string, ContextState>>(File.ReadAllText(_file), Json);
                _states = new Dictionary<string, ContextState>(loaded ?? [], StringComparer.OrdinalIgnoreCase);
            }
        }
        catch (Exception ex) when (ex is JsonException or IOException)
        {
            log.LogWarning("Could not read {File} ({Error}); all contexts will be regenerated.", _file, ex.Message);
            _states = new(StringComparer.OrdinalIgnoreCase);
        }
    }

    public ContextState? Get(RepositoryInfo repo) => _states.GetValueOrDefault(repo.RootPath);

    public void Set(RepositoryInfo repo, ContextState state) => _states[repo.RootPath] = state;

    public void Save() => PathUtil.WriteAtomically(_file, JsonSerializer.Serialize(_states, Json));
}
````

## tests/AIContextManager.Tests/AIContextManager.Tests.csproj

````xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <IsPackable>false</IsPackable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.13.0" />
    <PackageReference Include="xunit" Version="2.9.3" />
    <PackageReference Include="xunit.runner.visualstudio" Version="3.1.5" />
  </ItemGroup>

  <ItemGroup>
    <Using Include="Xunit" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\src\AIContextManager\AIContextManager.csproj" />
  </ItemGroup>

</Project>
````

## tests/AIContextManager.Tests/ChangeDetectionTests.cs

````csharp
using AIContextManager;

namespace AIContextManager.Tests;

public class ChangeDetectionTests
{
    private const string ShaA = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
    private const string ShaB = "bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb";
    private const string Fp = "fp1";

    private static ContextState Ok(string sha) => new() { CommitSha = sha, Status = ContextStatus.Ok, Fingerprint = Fp };

    [Fact]
    public void First_time_is_generated() =>
        Assert.True(ChangeDetector.NeedsRegeneration(null, ShaA, outputExists: false, Fp));

    [Fact]
    public void Same_commit_is_not_regenerated() =>
        Assert.False(ChangeDetector.NeedsRegeneration(Ok(ShaA), ShaA, outputExists: true, Fp));

    [Fact]
    public void New_commit_is_regenerated() =>
        Assert.True(ChangeDetector.NeedsRegeneration(Ok(ShaA), ShaB, outputExists: true, Fp));

    [Fact]
    public void Deleted_output_is_regenerated() =>
        Assert.True(ChangeDetector.NeedsRegeneration(Ok(ShaA), ShaA, outputExists: false, Fp));

    [Fact]
    public void Changed_settings_regenerate() =>
        Assert.True(ChangeDetector.NeedsRegeneration(Ok(ShaA), ShaA, outputExists: true, "fp2"));

    [Fact]
    public void Blocked_commit_waits_for_a_new_commit()
    {
        var blocked = Ok(ShaA) with { Status = ContextStatus.Blocked };

        Assert.False(ChangeDetector.NeedsRegeneration(blocked, ShaA, outputExists: false, Fp));
        Assert.True(ChangeDetector.NeedsRegeneration(blocked, ShaB, outputExists: false, Fp));
    }

    [Fact]
    public void Failed_generation_is_retried() =>
        Assert.True(ChangeDetector.NeedsRegeneration(Ok(ShaA) with { Status = ContextStatus.Failed }, ShaA, true, Fp));

    [Fact]
    public void Debouncer_releases_key_only_after_quiet_period()
    {
        var clock = new ManualClock();
        var debouncer = new ChangeDebouncer(TimeSpan.FromSeconds(10), clock);

        debouncer.Signal("repo");
        clock.Advance(TimeSpan.FromSeconds(6));
        debouncer.Signal("repo"); // burst keeps it waiting
        clock.Advance(TimeSpan.FromSeconds(6));
        Assert.Empty(debouncer.TakeReady());

        clock.Advance(TimeSpan.FromSeconds(5));
        Assert.Equal(["repo"], debouncer.TakeReady());
        Assert.Empty(debouncer.TakeReady()); // released once
    }

    [Fact]
    public void Debouncer_merges_many_signals_into_one_per_key()
    {
        var clock = new ManualClock();
        var debouncer = new ChangeDebouncer(TimeSpan.FromSeconds(1), clock);

        for (var i = 0; i < 100; i++) debouncer.Signal(i % 2 == 0 ? "api" : "ui");
        clock.Advance(TimeSpan.FromSeconds(2));

        Assert.Equal(["api", "ui"], debouncer.TakeReady().Order());
    }

    private sealed class ManualClock : TimeProvider
    {
        private DateTimeOffset _now = new(2026, 11, 18, 12, 0, 0, TimeSpan.Zero);
        public override DateTimeOffset GetUtcNow() => _now;
        public void Advance(TimeSpan by) => _now += by;
    }
}
````

## tests/AIContextManager.Tests/GitAndDiscoveryTests.cs

````csharp
using AIContextManager;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace AIContextManager.Tests;

public class GitAndDiscoveryTests : IDisposable
{
    private readonly TestRepos _repos = new();

    private ContextManagerOptions Options() => new()
    {
        SourcePath = _repos.Source,
        OutputPath = Path.Combine(_repos.Root, "out"),
        StagingPath = Path.Combine(_repos.Root, "staging"),
    };

    private static GitBranchService Git() => new(NullLogger<GitBranchService>.Instance);

    private RepositoryDiscoveryService Discovery() =>
        new(Microsoft.Extensions.Options.Options.Create(Options()), NullLogger<RepositoryDiscoveryService>.Instance);

    [Fact]
    public void Discovers_repositories_by_git_metadata_and_skips_noise()
    {
        _repos.CreateRepository("intsol-dna-list-matching-api");
        _repos.CreateRepository("intsol-dna-list-matching-ui");
        Directory.CreateDirectory(Path.Combine(_repos.Source, "not-a-repo", "src"));
        Directory.CreateDirectory(Path.Combine(_repos.Source, "node_modules", "pkg", ".git")); // must be skipped

        var found = Discovery().Discover().Select(Path.GetFileName).ToList();

        Assert.Equal(["intsol-dna-list-matching-api", "intsol-dna-list-matching-ui"], found);
    }

    [Fact]
    public void Excluded_folder_and_everything_inside_is_ignored()
    {
        _repos.CreateRepository("intsol-biography-api");
        var hidden = Path.Combine(_repos.Source, "Repos", "old-stuff", "intsol-biography-ui");
        Directory.CreateDirectory(Path.Combine(hidden, ".git"));
        File.WriteAllText(Path.Combine(hidden, ".git", "HEAD"), "ref: refs/heads/main\n");
        var directRepo = Path.Combine(_repos.Source, "Repos");
        File.WriteAllText(Path.Combine(directRepo, ".git"), "gitdir: C:/elsewhere/.git\n"); // even Repos itself as a repo

        var options = Options();
        options.ExcludedFolders = ["Repos"];
        var found = new RepositoryDiscoveryService(Microsoft.Extensions.Options.Options.Create(options),
            NullLogger<RepositoryDiscoveryService>.Instance).Discover();

        Assert.Equal(["intsol-biography-api"], found.Select(Path.GetFileName));
        Assert.True(options.IsExcluded(Path.Combine(hidden, "src", "App.tsx")));
        Assert.False(options.IsExcluded(Path.Combine(_repos.Source, "Repositories")));
    }

    [Fact]
    public void Discovers_worktree_with_git_file()
    {
        var worktree = Path.Combine(_repos.Source, "intsol-biography-api");
        Directory.CreateDirectory(worktree);
        File.WriteAllText(Path.Combine(worktree, ".git"), "gitdir: C:/elsewhere/.git/worktrees/x\n");

        Assert.Single(Discovery().Discover());
    }

    [Fact]
    public void New_repository_is_discovered_without_configuration()
    {
        _repos.CreateRepository("intsol-biography-api");
        Assert.Single(Discovery().Discover());

        _repos.CreateRepository("intsol-biography-ui");

        Assert.Equal(2, Discovery().Discover().Count);
    }

    [Fact]
    public async Task Snapshot_is_last_commit_of_checked_out_branch_and_repo_is_untouched()
    {
        var clone = _repos.CreateRepository("intsol-dna-list-matching-api"); // checked out on feature/my-work (from main)
        File.WriteAllText(Path.Combine(clone, "Program.cs"), "// uncommitted local edit\n");
        var statusBefore = TestRepos.Git(clone, "status", "--porcelain");
        var git = Git();

        var current = await git.GetCurrentAsync(clone, CancellationToken.None);
        Assert.NotNull(current);
        Assert.Equal("feature/my-work", current.Value.Branch);

        var target = Path.Combine(_repos.Root, "snap");
        var (written, skipped) = await git.ExportSnapshotAsync(clone, current.Value.Sha, target, CancellationToken.None);

        // Committed content only (uncommitted edit not included), .env never copied.
        Assert.Equal("Console.WriteLine(\"main\");", File.ReadAllText(Path.Combine(target, "Program.cs")).TrimEnd()); // autocrlf may apply
        Assert.False(File.Exists(Path.Combine(target, ".env")), ".env must never be copied");
        Assert.Equal(1, skipped);
        Assert.Equal(2, written);

        // The user's repository is exactly as before: same branch, same uncommitted change, no worktrees added.
        Assert.Equal("feature/my-work", TestRepos.Git(clone, "rev-parse", "--abbrev-ref", "HEAD"));
        Assert.Equal(statusBefore, TestRepos.Git(clone, "status", "--porcelain"));
        Assert.Equal("// uncommitted local edit\n", File.ReadAllText(Path.Combine(clone, "Program.cs")));
        Assert.Single(TestRepos.Git(clone, "worktree", "list").Split('\n'));
    }

    [Fact]
    public async Task Switching_branch_in_github_desktop_regenerates_the_context()
    {
        var clone = _repos.CreateRepository("intsol-dna-list-matching-ui");
        var git = Git();
        var before = (await git.GetCurrentAsync(clone, CancellationToken.None))!.Value;
        var previous = new ContextState { Branch = before.Branch, CommitSha = before.Sha, Status = ContextStatus.Ok, Fingerprint = "f" };
        Assert.False(ChangeDetector.NeedsRegeneration(previous, before.Sha, outputExists: true, "f"));

        TestRepos.Git(clone, "checkout", "-q", "develop"); // the Current Branch dropdown in GitHub Desktop
        var after = (await git.GetCurrentAsync(clone, CancellationToken.None))!.Value;

        Assert.Equal("develop", after.Branch);
        Assert.True(ChangeDetector.NeedsRegeneration(previous, after.Sha, outputExists: true, "f"));

        var target = Path.Combine(_repos.Root, "snap-dev");
        await git.ExportSnapshotAsync(clone, after.Sha, target, CancellationToken.None);
        Assert.Equal("Console.WriteLine(\"develop\");", File.ReadAllText(Path.Combine(target, "Program.cs")).TrimEnd());
    }

    [Fact]
    public async Task Repository_without_commits_has_no_current_commit()
    {
        var empty = Path.Combine(_repos.Source, "intsol-new-api");
        TestRepos.Git(_repos.Source, "init", "-q", empty);

        Assert.Null(await Git().GetCurrentAsync(empty, CancellationToken.None));
    }

    public void Dispose() => _repos.Dispose();
}
````

## tests/AIContextManager.Tests/GroupingTests.cs

````csharp
using AIContextManager;
using Microsoft.Extensions.Options;

namespace AIContextManager.Tests;

public class GroupingTests
{
    private static readonly string[] CorporateLayout =
    [
        "intsol-dna-list-matching-api", "intsol-dna-list-matching-ui",
        "intsol-biography-api", "intsol-biography-ui", "intsol-biography-sync",
        "intsol-mkt-bio-api", "intsol-mkt-bio-data-api", "intsol-mkt-bio-ui",
        "intsol-sa-rules-salesforce-api", "intsol-sa-rules-ui",
        "intsol-gitops", "netcore-helm-chart", "react-helm-chart",
    ];

    private static IReadOnlyList<RepositoryInfo> Group(params string[] names) =>
        new RepositoryGroupingService(Options.Create(new ContextManagerOptions()))
            .Group(names.Select(n => Path.Combine(@"C:\source", n)));

    [Theory]
    [InlineData("intsol-dna-list-matching-api", "intsol-dna-list-matching", "API")]
    [InlineData("intsol-dna-list-matching-ui", "intsol-dna-list-matching", "UI")]
    [InlineData("intsol-biography-api", "intsol-biography", "API")]
    [InlineData("intsol-biography-ui", "intsol-biography", "UI")]
    [InlineData("intsol-biography-sync", "intsol-biography", "Sync")]
    [InlineData("intsol-mkt-bio-data-api", "intsol-mkt-bio", "Data API")]
    [InlineData("intsol-mkt-bio-ui", "intsol-mkt-bio", "UI")]
    [InlineData("intsol-sa-rules-salesforce-api", "intsol-sa-rules", "Salesforce API")]
    [InlineData("intsol-sa-rules-ui", "intsol-sa-rules", "UI")]
    public void Groups_the_documented_examples(string repo, string solution, string component)
    {
        var info = Group(CorporateLayout).Single(r => r.Name == repo);

        Assert.Equal(solution, info.Solution);
        Assert.Equal(component, info.Component);
        Assert.False(info.IsShared);
    }

    [Theory]
    [InlineData("intsol-gitops")]
    [InlineData("netcore-helm-chart")]
    [InlineData("react-helm-chart")]
    public void Treats_gitops_and_helm_charts_as_shared_infrastructure(string repo)
    {
        var info = Group(CorporateLayout).Single(r => r.Name == repo);

        Assert.True(info.IsShared);
        Assert.Equal($"Shared-Infrastructure/{repo}-context.md", info.RelativeContextFile);
    }

    [Fact]
    public void One_context_file_per_repository_inside_its_solution_folder()
    {
        var info = Group(CorporateLayout).Single(r => r.Name == "intsol-mkt-bio-data-api");

        Assert.Equal("intsol-mkt-bio/intsol-mkt-bio-data-api-context.md", info.RelativeContextFile);
    }

    [Fact]
    public void Shared_prefix_alone_does_not_merge_repositories()
    {
        // "intsol-bio" is a prefix of "intsol-biography" as text, but not at a word boundary.
        var repos = Group("intsol-bio-api", "intsol-biography-ui");

        Assert.Equal("intsol-bio", repos.Single(r => r.Name == "intsol-bio-api").Solution);
        Assert.Equal("intsol-biography", repos.Single(r => r.Name == "intsol-biography-ui").Solution);
    }

    [Fact]
    public void Only_one_extra_word_may_join_another_solution()
    {
        var repos = Group("intsol-sa-rules-ui", "intsol-sa-rules-salesforce-sync-api");

        Assert.Equal("intsol-sa-rules-salesforce-sync", repos.Single(r => r.Name == "intsol-sa-rules-salesforce-sync-api").Solution);
    }

    [Fact]
    public void Repository_without_known_suffix_is_its_own_solution()
    {
        var info = Group("intsol-tools").Single();

        Assert.Equal("intsol-tools", info.Solution);
        Assert.Equal("Main", info.Component);
    }

    [Fact]
    public void Single_repository_with_suffix_keeps_its_own_solution()
    {
        var info = Group("intsol-lonely-api").Single();

        Assert.Equal("intsol-lonely", info.Solution);
        Assert.Equal("API", info.Component);
    }

    [Fact]
    public void Configured_shared_repository_is_shared()
    {
        var service = new RepositoryGroupingService(Options.Create(new ContextManagerOptions { SharedRepositories = ["platform-config"] }));

        Assert.True(service.Group([@"C:\source\platform-config"]).Single().IsShared);
    }
}
````

## tests/AIContextManager.Tests/SecretScannerTests.cs

````csharp
using AIContextManager;

namespace AIContextManager.Tests;

public class SecretScannerTests
{
    // Built by concatenation so this test file itself does not look like it contains secrets.
    private static readonly string FakeAwsKey = "AKIA" + "ABCDEFGHIJKLMNOP";
    private static readonly string FakeGitHubToken = "ghp_" + new string('a', 36);

    private static string Context(string file, string code) => $"# header\n\n## File: {file}\n```\n{code}\n```\n";

    [Fact]
    public void Clean_code_has_no_findings()
    {
        var md = Context("src/Program.cs", "var x = 1;\nconst string ApiKeyHeader = \"X-Api-Key\";");

        Assert.Empty(SecretScanner.Scan(md));
    }

    [Fact]
    public void Finds_aws_key_with_file_and_line_but_never_the_value()
    {
        var findings = SecretScanner.Scan(Context("config/aws.json", $"{{\n  \"key\": \"{FakeAwsKey}\"\n}}"));

        var f = Assert.Single(findings);
        Assert.Equal("config/aws.json", f.File);
        Assert.Equal(2, f.Line);
        Assert.DoesNotContain(FakeAwsKey, f.ToString());
    }

    [Theory]
    [InlineData("-----BEGIN RSA PRIVATE KEY-----")]
    [InlineData("\"ConnectionStrings\": { \"Db\": \"Server=sql;User Id=app;Password=S3cr3tValue!;\" }")]
    [InlineData("\"ClientSecret\": \"q8F~kd93jfLs02\"")]
    [InlineData("api_key = 'f8a7d6c5b4a39281'")]
    public void Detects_common_secret_shapes(string line) =>
        Assert.NotEmpty(SecretScanner.Scan(Context("appsettings.json", line)));

    [Fact]
    public void Detects_github_token() =>
        Assert.NotEmpty(SecretScanner.Scan(Context("ci.yml", $"token: {FakeGitHubToken}")));

    [Theory]
    [InlineData("\"Password\": \"\"")]
    [InlineData("\"Password\": \"#{DbPassword}#\"")]
    [InlineData("\"ClientSecret\": \"${CLIENT_SECRET}\"")]
    [InlineData("\"ApiKey\": \"<your-api-key>\"")]
    [InlineData("Server=sql;Password=${DB_PASSWORD};")]
    [InlineData("password: \"changeme\"")]
    public void Ignores_placeholders(string line) =>
        Assert.Empty(SecretScanner.Scan(Context("appsettings.json", line)));

    [Theory]
    [InlineData(".env", true)]
    [InlineData("src/.env.production", true)]
    [InlineData(".env.example", false)]
    [InlineData("certs/server.pfx", true)]
    [InlineData("keys/id_rsa", true)]
    [InlineData(".npmrc", true)]
    [InlineData("src/Api/appsettings.Development.local.json", true)]
    [InlineData("src/Api/appsettings.json", false)]
    [InlineData("src/Api/Program.cs", false)]
    public void Sensitive_files_are_excluded_from_snapshots(string path, bool sensitive) =>
        Assert.Equal(sensitive, SensitiveFiles.IsSensitive(path));
}
````

## tests/AIContextManager.Tests/TestRepos.cs

````csharp
using System.Diagnostics;

namespace AIContextManager.Tests;

/// <summary>Builds synthetic Git repositories (bare "origin" + working clone) in a temp folder.</summary>
internal sealed class TestRepos : IDisposable
{
    public string Root { get; } = Path.Combine(Path.GetTempPath(), "aicm-tests", Guid.NewGuid().ToString("N")[..8]);
    public string Source => Path.Combine(Root, "source");
    public string Origins => Path.Combine(Root, "origins");

    public TestRepos()
    {
        Directory.CreateDirectory(Source);
        Directory.CreateDirectory(Origins);
    }

    /// <summary>Creates origin with "main" and "develop" and clones it to source/{name}, checked out on a feature branch.</summary>
    public string CreateRepository(string name, bool withDevelop = true)
    {
        var origin = Path.Combine(Origins, name + ".git");
        var seed = Path.Combine(Root, "seed-" + name);
        Git(Root, "init", "--bare", "-b", "main", origin);
        Git(Root, "init", "-b", "main", seed);
        File.WriteAllText(Path.Combine(seed, "README.md"), $"# {name}\n");
        File.WriteAllText(Path.Combine(seed, "Program.cs"), "Console.WriteLine(\"main\");\n");
        File.WriteAllText(Path.Combine(seed, ".env"), "SECRET=do-not-copy\n");
        Commit(seed, "main commit");
        Git(seed, "remote", "add", "origin", origin);
        Git(seed, "push", "-q", "origin", "main");
        if (withDevelop)
        {
            Git(seed, "checkout", "-q", "-b", "develop");
            File.WriteAllText(Path.Combine(seed, "Program.cs"), "Console.WriteLine(\"develop\");\n");
            Commit(seed, "develop commit");
            Git(seed, "push", "-q", "origin", "develop");
        }

        var clone = Path.Combine(Source, name);
        Git(Root, "clone", "-q", origin, clone);
        Git(clone, "checkout", "-q", "-b", "feature/my-work");
        return clone;
    }

    /// <summary>Pushes a new commit to origin/{branch} from the seed clone (simulates a teammate).</summary>
    public void PushChange(string name, string branch, string file, string content)
    {
        var seed = Path.Combine(Root, "seed-" + name);
        Git(seed, "checkout", "-q", branch);
        File.WriteAllText(Path.Combine(seed, file), content);
        Commit(seed, "change " + file);
        Git(seed, "push", "-q", "origin", branch);
    }

    public static void Commit(string repo, string message)
    {
        Git(repo, "add", "-A");
        Git(repo, "-c", "user.name=test", "-c", "user.email=test@example.invalid", "commit", "-q", "-m", message);
    }

    public static string Git(string workDir, params string[] args)
    {
        var psi = new ProcessStartInfo("git") { WorkingDirectory = workDir, RedirectStandardOutput = true, RedirectStandardError = true };
        foreach (var a in args) psi.ArgumentList.Add(a);
        using var p = Process.Start(psi)!;
        var stdout = p.StandardOutput.ReadToEnd();
        var stderr = p.StandardError.ReadToEnd();
        p.WaitForExit();
        if (p.ExitCode != 0) throw new InvalidOperationException($"git {string.Join(' ', args)} failed: {stderr}");
        return stdout.Trim();
    }

    public void Dispose()
    {
        try
        {
            foreach (var f in Directory.EnumerateFiles(Root, "*", SearchOption.AllDirectories)) File.SetAttributes(f, FileAttributes.Normal);
            Directory.Delete(Root, recursive: true);
        }
        catch (IOException) { }
        catch (UnauthorizedAccessException) { }
    }
}
````
