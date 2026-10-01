using System.Text.Json;
using Xunit;

namespace CompoundingPerf.Tests;

public class ConfigTests
{
    [Fact]
    public void Defaults_match_expected_safe_values()
    {
        var c = new CompoundingPerfConfig();

        Assert.True(c.Server.ProfileSaveDebouncer.Enabled);
        Assert.True(c.Server.ResponseCache.Enabled);
        Assert.True(c.Server.ThreadSafeRandom.Enabled);
        Assert.True(c.Server.ResponseSanitizer.Enabled);
        Assert.True(c.Server.RagfairCalmUpdates.Enabled);
        Assert.True(c.Server.FastCompression.Enabled);
        Assert.Equal("Fastest", c.Server.FastCompression.Level);

        // Telemetry off by default — opt-in is intentional.
        Assert.False(c.Telemetry.Enabled);
        Assert.False(c.Telemetry.TimingEnabled);
    }

    [Fact]
    public void Json_roundtrip_preserves_values()
    {
        var original = new CompoundingPerfConfig
        {
            Server = new ServerToggles
            {
                ProfileSaveDebouncer = new ProfileSaveDebouncerOptions { Enabled = false },
                ResponseCache = new ResponseCacheOptions { Enabled = false, AdditionalPaths = new List<string> { "/custom/path" } },
            },
            Client = new ClientToggles
            {
                FrameStats = new FrameStatsOptions { Enabled = false, WarmupSkipSeconds = 35 },
            },
        };

        var json = JsonSerializer.Serialize(original);
        var roundTripped = JsonSerializer.Deserialize<CompoundingPerfConfig>(json)!;

        Assert.False(roundTripped.Server.ProfileSaveDebouncer.Enabled);
        Assert.False(roundTripped.Server.ResponseCache.Enabled);
        Assert.Contains("/custom/path", roundTripped.Server.ResponseCache.AdditionalPaths);
        Assert.False(roundTripped.Client.FrameStats.Enabled);
        Assert.Equal(35, roundTripped.Client.FrameStats.WarmupSkipSeconds);
    }

    [Fact]
    public void ShippedConfig_json_parses_cleanly()
    {
        // The shipping config.json next to the csproj is the source of truth users see —
        // make sure it still deserializes after any schema changes.
        var path = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "config.json");
        path = Path.GetFullPath(path);
        Assert.True(File.Exists(path), $"config.json not found at {path}");

        var raw = File.ReadAllText(path);
        // STRICT options on purpose: SPT's ModHelper.GetJsonDataFromFile does not allow
        // trailing commas — a lenient test here once passed a config the live loader
        // rejected, killing every feature at boot (2026-06-13).
        var parsed = JsonSerializer.Deserialize<CompoundingPerfConfig>(raw);
        Assert.NotNull(parsed);
        Assert.True(parsed!.Server.ProfileSaveDebouncer.Enabled);
        Assert.True(parsed.Server.ResponseCache.Enabled);
        Assert.True(parsed.Server.ThreadSafeRandom.Enabled);
        Assert.True(parsed.Client.FrameStats.Enabled);
    }
}
