# AIContextManager

A small .NET 10 console app that finds every Git repository under your `source` folder and keeps two Markdown
contexts per repository, built with [Repomix](https://github.com/yamadashy/repomix):

- `DEV-context.md` from `origin/develop`
- `PROD-context.md` from `origin/main`

They are grouped by solution (API, UI, Sync, ...) with a navigable `00-INDEX.md`, ready to attach to GitHub Copilot,
Microsoft 365 Copilot, Copilot Notebooks or any assistant that reads Markdown.

No repository list, no scripts, no database. It only creates files; it never calls an AI service.

## Requirements

- .NET 10 SDK (or runtime, to run a published build)
- Git on `PATH` (Git for Windows; GitHub Desktop's bundled Git is not on `PATH`)
- Repomix: `npm install -g repomix` (or set `RepomixCommand` to its full path)

## Run

From Visual Studio: open `AIContextManager.sln`, set `AIContextManager` as startup project, edit
`src/AIContextManager/appsettings.json`, press F5.

From a terminal:

```bash
dotnet run --project src/AIContextManager
```

One pass and exit (useful the first time, or from Windows Task Scheduler):

```bash
dotnet run --project src/AIContextManager -- --once
```

Any setting can be overridden on the command line:

```bash
dotnet run --project src/AIContextManager -- --ContextManager:SourcePath=C:\source --ContextManager:OutputPath=C:\AI-Context
```

Tests:

```bash
dotnet test
```

## Configuration (`appsettings.json`)

| Setting | Default | Meaning |
|---|---|---|
| `SourcePath` | `C:\source` | Folder with your repositories. Required. |
| `OutputPath` | `C:\AI-Context` | Where contexts and `00-INDEX.md` go. Must not be inside `SourcePath`. |
| `StagingPath` | `%LOCALAPPDATA%\AIContextManager\staging` | Local work area: snapshots, scan reports, `state.json`. Never cloud-synced. |
| `Branches:Dev` / `Branches:Prod` | `develop` / `main` | Remote branches used for DEV and PROD. |
| `AutoDiscover` | `true` | Pick up added/removed repositories while running. |
| `WatchChanges` | `true` | React to Git ref updates (e.g. GitHub Desktop fetch) via `FileSystemWatcher`. |
| `GitCheckMinutes` | `15` | Interval for `git fetch` + check of all repositories. |
| `DebounceSeconds` | `10` | Quiet time after file events before a check. |
| `FetchRemote` | `true` | Run `git fetch` before checks. |
| `RepomixCommand` | `repomix` | Name or full path of the Repomix CLI. |
| `AllowCloudPublish` | `false` | Must be `true` to write into a OneDrive/SharePoint folder. Set only if your policy allows full source in that location. |
| `SharedRepositories` | `[]` | Extra names to treat as shared infrastructure. |
| `ExtraIgnorePatterns` | `[]` | Extra Repomix `--ignore` globs for all repositories. |

Configuration is validated at startup; the app exits with a clear message when something is wrong.

## How it works

1. **Discovery.** Walks `SourcePath` (default depth 3) and treats a folder as a repository when it has a `.git`
   folder with `HEAD`, or a `.git` file starting with `gitdir:` (worktrees). `bin`, `obj`, `.vs`, `node_modules`,
   `dist`, `build` and `.git` are never traversed.
2. **Grouping** (naming only, conservative):
   - `*gitops*`, `*-helm-chart` and `SharedRepositories` go to `Shared-Infrastructure/`.
   - Suffixes `-api`, `-ui`, `-data-api`, `-worker`, `-sync`, `-cronjob`, `-cron-job` give the component.
   - Repos with the same base form one solution (`intsol-biography-api` + `-ui` + `-sync`).
   - A repo joins another solution only if its base is that solution plus one word
     (`intsol-sa-rules-salesforce-api` joins `intsol-sa-rules` as "Salesforce API").
   - Anything else stays its own solution. A shared text prefix alone never merges repos.
3. **Branch content without touching your repos.** The app never runs `checkout`, `reset`, `clean` or `pull`.
   It runs `git fetch` (updates only `refs/remotes/origin/*`, like GitHub Desktop does), resolves
   `origin/develop` / `origin/main` with `git rev-parse`, and streams that commit with `git archive` into a temporary
   folder in `StagingPath`. No worktrees are created and uncommitted changes can never be included.
   - Fetch fails: the last known remote ref is used and the context is marked **STALE**.
   - Branch missing: reported as **NO BRANCH**; no other branch is substituted.
4. **Repomix.** One run per repository and environment, Markdown style, respecting the snapshot's `.gitignore`
   and `.repomixignore`, plus built-in ignores (build output, lock files, logs, previously generated contexts).
5. **Security, fail-closed.**
   - Known sensitive files are never copied into the snapshot: `.env*` (except `.env.example`), `*.pfx`, `*.pem`,
     `*.key`, `id_rsa*`, `.npmrc`, `secrets.json`, `*.local.json`, `terraform.tfstate`, and similar.
   - The generated file is scanned (private keys, AWS/GitHub/Slack/Azure keys, JWTs, passwords in connection strings,
     hard-coded `password`/`secret`/`apiKey` values; placeholders like `#{X}#`, `${X}`, `""` are allowed).
   - Any finding **blocks** publication. The report in `StagingPath\contexts\...\*.secret-scan.txt` lists rule, file
     and line only, never the value. Fix the repository or exclude the file, then the next commit is processed.
   - Only clean files are copied to `OutputPath`. Git URLs are stripped of credentials before logging.
6. **Change detection.** `state.json` remembers the commit and settings used for each context. A context is rebuilt
   only when its branch commit changes, the settings change, or the published file was deleted. A push to `develop`
   rebuilds only DEV. The watcher reacts to `.git/refs/remotes/*` and `packed-refs` changes and to folders
   appearing/disappearing; editing working files does nothing (DEV/PROD are committed content).
7. **Index.** `00-INDEX.md` shows latest successful update, per-component DEV/PROD status with commit, date and token
   count, and GitOps files that mention the solution's repositories. A starter `ARCHITECTURE.md` is created per
   solution only if missing; it is never overwritten.

## Output

```text
AI-Context/
├── 00-INDEX.md
├── intsol-dna-list-matching/
│   ├── ARCHITECTURE.md
│   ├── API/DEV-context.md, PROD-context.md
│   └── UI/DEV-context.md, PROD-context.md
└── Shared-Infrastructure/
    └── intsol-gitops/DEV-context.md, PROD-context.md
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
- Repositories that are worktrees (`.git` file) are rechecked on the periodic Git check, not by the watcher.
- No WORKING (uncommitted) context yet.
