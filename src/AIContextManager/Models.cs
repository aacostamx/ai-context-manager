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
