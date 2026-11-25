using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AIContextManager;

public sealed record RunMode(bool Once);

/// <summary>
/// Main loop:
///  - startup: discover, fetch, generate what changed (DEV/QA/PROD per repository), write the index;
///  - every GitCheckMinutes: fetch + check all repositories;
///  - FileSystemWatcher: new/removed repositories and updated remote refs (e.g. GitHub Desktop's Fetch origin)
///    are debounced and checked without fetching.
/// Working-tree edits and the locally checked-out branch are ignored on purpose.
/// </summary>
public sealed class ContextMonitorService(
    IOptions<ContextManagerOptions> options,
    RunMode mode,
    RepositoryDiscoveryService discovery,
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
            await ProcessAsync(_repos, fetch: true, ct);

            if (mode.Once)
            {
                log.LogInformation("Single run finished (--once).");
                lifetime.StopApplication();
                return;
            }

            if (_o.WatchChanges) StartWatcher();
            log.LogInformation("Monitoring {Source}. Fetch + check every {Minutes} min{Watch}. Press Ctrl+C to stop.",
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
                    await ProcessAsync(added, fetch: true, ct, forceIndex: true);
                }

                var changed = _repos.Where(r => ready.Contains(r.RootPath)).ToList();
                if (changed.Count > 0) await ProcessAsync(changed, fetch: false, ct);

                if (DateTimeOffset.UtcNow >= nextGitCheck)
                {
                    await ProcessAsync(_repos, fetch: true, ct);
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

        log.LogInformation("Using {Git}, repomix {Repomix}. Source: {Source} -> Output: {Output}. Environments: {Envs}",
            gitVersion, repomixVersion, _o.SourcePath, _o.OutputPath,
            string.Join(", ", _o.Environments.Select(e => $"{e.Name}={e.Branch}")));
        return true;
    }

    private void Discover()
    {
        var previous = _repos;
        _repos = discovery.Discover().Select(p => new RepositoryInfo(Path.GetFileName(p), p)).ToList();

        var added = _repos.Select(r => r.Name).Except(previous.Select(r => r.Name)).ToList();
        var removed = previous.Select(r => r.Name).Except(_repos.Select(r => r.Name)).ToList();
        if (previous.Count == 0)
        {
            log.LogInformation("Found {Count} repositories: {Repos}", _repos.Count, string.Join(", ", _repos.Select(r => r.Name)));
        }
        else
        {
            foreach (var name in added) log.LogInformation("New repository discovered: {Repo}", name);
            foreach (var name in removed) log.LogInformation("Repository no longer present: {Repo} (published files are kept)", name);
        }
    }

    private async Task ProcessAsync(IReadOnlyList<RepositoryInfo> repos, bool fetch, CancellationToken ct, bool forceIndex = false)
    {
        var changed = forceIndex;
        var regenerated = 0;
        var fetchFailed = false;

        foreach (var repo in repos)
        {
            ct.ThrowIfCancellationRequested();

            // After the first fetch failure (usually: not logged in, or offline) stop trying for this pass,
            // so one problem does not cost a timeout per repository.
            if (fetch && _o.FetchFromGitHub && !fetchFailed && await git.FetchAsync(repo.RootPath, ct) is { } error)
            {
                fetchFailed = true;
                log.LogWarning("git fetch failed for {Repo}: {Error}. Using the last fetched branches for this pass. " +
                               "If you are not logged in, run 'git fetch' once in a terminal inside any repository (see README).",
                    repo.Name, error);
            }

            foreach (var env in _o.Environments)
            {
                var before = state.Get(repo, env);
                var after = await generator.ProcessAsync(repo, env, before, ct);
                state.Set(repo, env, after);
                if (before != after)
                {
                    changed = true;
                    regenerated++;
                }
            }
        }

        state.Save();
        if (repos.Count > 0)
            log.LogInformation("Checked {Count} repositories, {Regenerated} context(s) updated{Fetch}.",
                repos.Count, regenerated, fetch && _o.FetchFromGitHub ? (fetchFailed ? ", fetch FAILED (used last fetched refs)" : ", fetch OK") : "");
        if (changed || !File.Exists(Path.Combine(_o.OutputPath, IndexWriter.IndexFileName)))
            index.Write(_repos);
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

        // Remote-tracking refs changed (a fetch by GitHub Desktop or by this app). Working-tree edits are ignored.
        if (inRepo.StartsWith(".git/refs/remotes/", StringComparison.OrdinalIgnoreCase) ||
            inRepo.Equals(".git/packed-refs", StringComparison.OrdinalIgnoreCase))
            _debouncer.Signal(repo.RootPath);
    }
}
