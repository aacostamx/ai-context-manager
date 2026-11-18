using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AIContextManager;

/// <summary>Remembers the last processed commit per repository/environment in StagingPath\state.json.</summary>
public sealed class StateStore(IOptions<ContextManagerOptions> options, ILogger<StateStore> log)
{
    private static readonly JsonSerializerOptions Json = new() { WriteIndented = true };
    private readonly string _file = Path.Combine(options.Value.StagingPath, "state.json");
    private Dictionary<string, EnvState> _states = new(StringComparer.OrdinalIgnoreCase);

    public static string Key(RepositoryInfo repo, ContextEnvironment env) => $"{repo.RootPath}|{env}";

    public void Load()
    {
        try
        {
            if (File.Exists(_file))
            {
                var loaded = JsonSerializer.Deserialize<Dictionary<string, EnvState>>(File.ReadAllText(_file), Json);
                _states = new Dictionary<string, EnvState>(loaded ?? [], StringComparer.OrdinalIgnoreCase);
            }
        }
        catch (Exception ex) when (ex is JsonException or IOException)
        {
            log.LogWarning("Could not read {File} ({Error}); all contexts will be regenerated.", _file, ex.Message);
            _states = new(StringComparer.OrdinalIgnoreCase);
        }
    }

    public EnvState? Get(RepositoryInfo repo, ContextEnvironment env) => _states.GetValueOrDefault(Key(repo, env));

    public void Set(RepositoryInfo repo, ContextEnvironment env, EnvState state) => _states[Key(repo, env)] = state;

    public void Save() => PathUtil.WriteAtomically(_file, JsonSerializer.Serialize(_states, Json));
}
