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
