using System.Text.Json.Serialization;

namespace AIContextManager;

public enum ContextEnvironment { Dev, Prod }

[JsonConverter(typeof(JsonStringEnumConverter<ContextStatus>))]
public enum ContextStatus
{
    Pending,
    Ok,
    MissingBranch,
    Blocked,
    Failed,
}

/// <summary>A discovered repository and where it belongs in the output tree.</summary>
public sealed record RepositoryInfo(
    string Name,
    string RootPath,
    string Solution,
    string Component,
    string ComponentFolder,
    bool IsShared)
{
    public const string SharedSolution = "Shared-Infrastructure";

    /// <summary>Output folder relative to OutputPath, for example "intsol-dna-list-matching/API".</summary>
    public string OutputFolder => IsShared ? $"{SharedSolution}/{Name}" : $"{Solution}/{ComponentFolder}";

    /// <summary>Full path of this repository's context file below <paramref name="root"/> (OutputPath or staging).</summary>
    public string ContextFile(string root, ContextEnvironment env) =>
        Path.Combine(root, OutputFolder.Replace('/', Path.DirectorySeparatorChar), ContextFileName(env));

    public static string ContextFileName(ContextEnvironment env) =>
        env == ContextEnvironment.Dev ? "DEV-context.md" : "PROD-context.md";
}

/// <summary>What we know about one repository + environment. Persisted in state.json.</summary>
public sealed record EnvState
{
    public string Branch { get; init; } = "";
    public string? CommitSha { get; init; }
    public ContextStatus Status { get; init; } = ContextStatus.Pending;
    public string? Message { get; init; }
    public DateTimeOffset? GeneratedUtc { get; init; }
    public DateTimeOffset? CheckedUtc { get; init; }
    public long? Tokens { get; init; }

    /// <summary>True when the last "git fetch" failed, so the remote-tracking ref may be behind the server.</summary>
    public bool RemoteStale { get; init; }

    /// <summary>Hash of the generation settings; a change forces regeneration.</summary>
    public string? Fingerprint { get; init; }
}
