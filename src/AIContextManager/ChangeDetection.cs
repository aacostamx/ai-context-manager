using System.Security.Cryptography;
using System.Text;

namespace AIContextManager;

public static class ChangeDetector
{
    /// <summary>
    /// A context only depends on the checked-out branch, its commit and the generation settings, so it is regenerated
    /// only when one of those changed (new commit, pull, branch switch) or the published file disappeared.
    /// </summary>
    public static bool NeedsRegeneration(ContextState? previous, string currentBranch, string currentSha, bool outputExists, string fingerprint)
    {
        if (previous is null) return true;
        if (!string.Equals(previous.CommitSha, currentSha, StringComparison.OrdinalIgnoreCase)) return true;
        if (!string.Equals(previous.Branch, currentBranch, StringComparison.Ordinal)) return true; // new branch on the same commit
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
