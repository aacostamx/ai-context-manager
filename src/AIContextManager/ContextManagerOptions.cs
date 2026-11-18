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

    public BranchOptions Branches { get; set; } = new();

    /// <summary>Re-scan SourcePath when folders are added or removed. When false, repositories are discovered once at startup.</summary>
    public bool AutoDiscover { get; set; } = true;

    /// <summary>Use FileSystemWatcher to react to new repositories and Git reference updates.</summary>
    public bool WatchChanges { get; set; } = true;

    /// <summary>Minutes between "git fetch" + reference checks for all repositories.</summary>
    public int GitCheckMinutes { get; set; } = 15;

    /// <summary>Quiet period after the last file-system event before a repository is checked.</summary>
    public int DebounceSeconds { get; set; } = 10;

    /// <summary>Run "git fetch" before checking references. When false only existing remote-tracking refs are used.</summary>
    public bool FetchRemote { get; set; } = true;

    public string RemoteName { get; set; } = "origin";

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

    public string BranchFor(ContextEnvironment env) => env == ContextEnvironment.Dev ? Branches.Dev : Branches.Prod;

    /// <summary>True when <paramref name="fullPath"/> is one of the ExcludedFolders or inside one.</summary>
    public bool IsExcluded(string fullPath) =>
        ExcludedFolders.Any(f => !string.IsNullOrWhiteSpace(f) &&
                                 PathUtil.IsSameOrUnder(fullPath, Path.IsPathRooted(f) ? f : Path.Combine(SourcePath, f)));
}

public sealed class BranchOptions
{
    public string Dev { get; set; } = "develop";
    public string Prod { get; set; } = "main";
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

        if (!IsValidRefName(o.Branches.Dev)) errors.Add("Branches:Dev is not a valid branch name.");
        if (!IsValidRefName(o.Branches.Prod)) errors.Add("Branches:Prod is not a valid branch name.");
        if (string.Equals(o.Branches.Dev, o.Branches.Prod, StringComparison.OrdinalIgnoreCase))
            errors.Add("Branches:Dev and Branches:Prod must be different.");
        if (!IsValidRefName(o.RemoteName)) errors.Add("RemoteName is not valid.");
        if (o.GitCheckMinutes < 1) errors.Add("GitCheckMinutes must be at least 1.");
        if (o.DebounceSeconds < 1) errors.Add("DebounceSeconds must be at least 1.");
        if (o.RepomixTimeoutMinutes < 1) errors.Add("RepomixTimeoutMinutes must be at least 1.");
        if (o.MaxDiscoveryDepth is < 1 or > 10) errors.Add("MaxDiscoveryDepth must be between 1 and 10.");
        if (string.IsNullOrWhiteSpace(o.RepomixCommand)) errors.Add("RepomixCommand is required.");

        return errors.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(errors);
    }

    private static bool IsValidRefName(string? b) =>
        !string.IsNullOrWhiteSpace(b) && !b.StartsWith('-') && !b.Contains("..") &&
        b.All(c => char.IsLetterOrDigit(c) || c is '-' or '_' or '.' or '/');
}
