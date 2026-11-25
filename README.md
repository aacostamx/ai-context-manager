# AIContextManager

A small .NET 8 console app that finds every Git repository under your `source` folder and keeps one Markdown
context per environment, built with [Repomix](https://github.com/yamadashy/repomix):

| Environment | Branch | File |
|---|---|---|
| DEV | `origin/develop` | `<repo>/<repo>-DEV-context.md` |
| QA | `origin/release` | `<repo>/<repo>-QA-context.md` |
| PROD | `origin/main` | `<repo>/<repo>-PROD-context.md` |

The branch you have checked out locally does not matter. You can be on any feature branch; the contexts always
come from `develop`, `release` and `main` on GitHub. Each repository gets its own folder, and `00-INDEX.md` links
everything. The files are ready to attach to GitHub Copilot, Microsoft 365 Copilot or any assistant that reads Markdown.

## Requirements

- .NET 8 SDK (or runtime, to run a published build)
- Git for Windows (includes Git Credential Manager), or GitHub Desktop's bundled Git as a fallback
- Repomix: `npm install -g repomix` (or set `RepomixCommand` to its full path)

## One-time GitHub login (SSO)

The app runs `git fetch` to get the latest `develop`, `release` and `main`. It uses the login Git already has
and never asks for a password or token itself. If your normal login is SSO in the GitHub Enterprise web page,
log in once like this:

1. Open a terminal inside any repository in your `source` folder.
2. Run:

   ```bash
   git fetch
   ```

3. Git Credential Manager opens your browser. Sign in with SSO as you normally do.

That's it. Windows Credential Manager remembers the login, and every `git fetch` after that (including the app's)
works without asking again. When the login expires, repeat the three steps.

If the fetch fails (not logged in, VPN down), the app does not stop. It logs one warning and uses the branches as
of the last successful fetch, including fetches done by GitHub Desktop (**Fetch origin**).

To never contact GitHub, set `FetchFromGitHub` to `false`; then only GitHub Desktop's fetches update the contexts.

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

Tests:

```bash
dotnet test
```

## Configuration (`appsettings.json`)

| Setting | Default | Meaning |
|---|---|---|
| `SourcePath` | `C:\Users\O7U\source` | Folder with your repositories. Required. |
| `ExcludedFolders` | `["Repos"]` | Folders ignored completely, with everything inside. Relative to `SourcePath`, or absolute. |
| `OutputPath` | `C:\Users\O7U\AI-Context` | Where contexts and `00-INDEX.md` go. Must not be inside `SourcePath`. |
| `Environments` | DEV=`develop`, QA=`release`, PROD=`main` | One context per entry. Change a `Branch` if your QA branch has another name. |
| `FetchFromGitHub` | `true` | Run `git fetch` before each check (see the login section). |
| `GitCheckMinutes` | `15` | How often to fetch and check all repositories. |
| `WatchChanges` | `true` | Also react within seconds when GitHub Desktop fetches or a repo is added. |
| `AutoDiscover` | `true` | Pick up added/removed repositories while running. |
| `DebounceSeconds` | `10` | Quiet time after file events before a check. |
| `StagingPath` | `%LOCALAPPDATA%\AIContextManager\staging` | Local work area: snapshots, scan reports, `state.json`. |
| `RepomixCommand` | `repomix` | Name or full path of the Repomix CLI. |
| `BlockOnSecrets` | `false` | `true`: a context with possible secrets is not published. `false`: published anyway, with a warning and a report. |
| `AllowCloudPublish` | `false` | Must be `true` to write into a OneDrive/SharePoint folder. |
| `ExtraIgnorePatterns` | `[]` | Extra Repomix `--ignore` globs for all repositories. |

## How it works

1. **Discovery.** Any folder under `SourcePath` with Git metadata (`.git` folder or worktree `.git` file) is a
   repository. `ExcludedFolders`, `bin`, `obj`, `.vs`, `node_modules`, `dist`, `build` are never traversed.
2. **Fetch.** `git fetch origin` updates only the remote-tracking branches (`origin/*`), exactly like GitHub
   Desktop's **Fetch origin**. Your checked-out branch and your files are never touched.
3. **Snapshot.** For each environment, `git rev-parse origin/<branch>` gives the commit and `git archive` streams
   its files into a temporary folder. The app never runs `checkout`, `pull`, `reset` or `clean`, never creates
   worktrees, and uncommitted changes are never included. Sensitive files (`.env`, `*.pfx`, `*.pem`, `*.key`,
   `id_rsa*`, `.npmrc`, `secrets.json`, `*.local.json`, ...) are never copied.
4. **Repomix.** One run per repository and environment, respecting `.gitignore` and `.repomixignore`.
5. **Secret scan.** Possible secrets are reported (file and line, never the value). With `BlockOnSecrets: true`
   the context is not published.
6. **Change detection.** `state.json` remembers the commit used for each context. A context is rebuilt only when
   its branch moved, the settings changed, or the file was deleted. A push to `release` rebuilds only QA.
   A branch that does not exist in a repository is shown as **NO BRANCH**; no other branch is used instead.

## Output

```text
AI-Context/
├── 00-INDEX.md
├── intsol-dna-list-matching-api/
│   ├── intsol-dna-list-matching-api-DEV-context.md
│   ├── intsol-dna-list-matching-api-QA-context.md
│   └── intsol-dna-list-matching-api-PROD-context.md
└── intsol-gitops/
    └── ...
```

See [`samples/dna`](samples/dna/00-INDEX.md) for output generated from synthetic repositories.

## Limitations

- Uncommitted or unpushed work is not included: the contexts are what is on GitHub.
- The secret scanner is pattern-based; it can miss secrets and flag harmless text.
- Repositories that are worktrees (`.git` file) are rechecked on the periodic check, not by the watcher.
