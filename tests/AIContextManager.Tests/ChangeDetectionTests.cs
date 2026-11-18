using AIContextManager;

namespace AIContextManager.Tests;

public class ChangeDetectionTests
{
    private const string ShaA = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
    private const string ShaB = "bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb";
    private const string Fp = "fp1";

    private static EnvState Ok(string sha) => new() { CommitSha = sha, Status = ContextStatus.Ok, Fingerprint = Fp };

    [Fact]
    public void First_time_is_generated() =>
        Assert.True(ChangeDetector.NeedsRegeneration(null, ShaA, outputExists: false, Fp));

    [Fact]
    public void Same_commit_is_not_regenerated() =>
        Assert.False(ChangeDetector.NeedsRegeneration(Ok(ShaA), ShaA, outputExists: true, Fp));

    [Fact]
    public void New_commit_is_regenerated() =>
        Assert.True(ChangeDetector.NeedsRegeneration(Ok(ShaA), ShaB, outputExists: true, Fp));

    [Fact]
    public void Deleted_output_is_regenerated() =>
        Assert.True(ChangeDetector.NeedsRegeneration(Ok(ShaA), ShaA, outputExists: false, Fp));

    [Fact]
    public void Changed_settings_regenerate() =>
        Assert.True(ChangeDetector.NeedsRegeneration(Ok(ShaA), ShaA, outputExists: true, "fp2"));

    [Fact]
    public void Blocked_commit_waits_for_a_new_commit()
    {
        var blocked = Ok(ShaA) with { Status = ContextStatus.Blocked };

        Assert.False(ChangeDetector.NeedsRegeneration(blocked, ShaA, outputExists: false, Fp));
        Assert.True(ChangeDetector.NeedsRegeneration(blocked, ShaB, outputExists: false, Fp));
    }

    [Fact]
    public void Failed_generation_is_retried() =>
        Assert.True(ChangeDetector.NeedsRegeneration(Ok(ShaA) with { Status = ContextStatus.Failed }, ShaA, true, Fp));

    [Fact]
    public void Debouncer_releases_key_only_after_quiet_period()
    {
        var clock = new ManualClock();
        var debouncer = new ChangeDebouncer(TimeSpan.FromSeconds(10), clock);

        debouncer.Signal("repo");
        clock.Advance(TimeSpan.FromSeconds(6));
        debouncer.Signal("repo"); // burst keeps it waiting
        clock.Advance(TimeSpan.FromSeconds(6));
        Assert.Empty(debouncer.TakeReady());

        clock.Advance(TimeSpan.FromSeconds(5));
        Assert.Equal(["repo"], debouncer.TakeReady());
        Assert.Empty(debouncer.TakeReady()); // released once
    }

    [Fact]
    public void Debouncer_merges_many_signals_into_one_per_key()
    {
        var clock = new ManualClock();
        var debouncer = new ChangeDebouncer(TimeSpan.FromSeconds(1), clock);

        for (var i = 0; i < 100; i++) debouncer.Signal(i % 2 == 0 ? "api" : "ui");
        clock.Advance(TimeSpan.FromSeconds(2));

        Assert.Equal(["api", "ui"], debouncer.TakeReady().Order());
    }

    private sealed class ManualClock : TimeProvider
    {
        private DateTimeOffset _now = new(2026, 11, 18, 12, 0, 0, TimeSpan.Zero);
        public override DateTimeOffset GetUtcNow() => _now;
        public void Advance(TimeSpan by) => _now += by;
    }
}
