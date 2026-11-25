using System.Text.Json.Serialization;

namespace AIContextManager;

[JsonConverter(typeof(JsonStringEnumConverter<ContextStatus>))]
public enum ContextStatus
{
    Pending,
    Ok,
    MissingBranch,
    Blocked,
    Failed,
}

/// <summary>A discovered repository. Each repository gets its own output folder.</summary>
public sealed record RepositoryInfo(string Name, string RootPath)
{
    /// <summary>Context file relative to OutputPath, for example "intsol-dna-list-matching-api/intsol-dna-list-matching-api-DEV-context.md".</summary>
    public string RelativeContextFile(EnvironmentOptions env) => $"{Name}/{Name}-{env.Name}-context.md";

    /// <summary>Full path of the context file below <paramref name="root"/> (OutputPath or staging).</summary>
    public string ContextFile(string root, EnvironmentOptions env) =>
        Path.Combine(root, RelativeContextFile(env).Replace('/', Path.DirectorySeparatorChar));
}

/// <summary>What we know about one repository + environment. Persisted in state.json.</summary>
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
