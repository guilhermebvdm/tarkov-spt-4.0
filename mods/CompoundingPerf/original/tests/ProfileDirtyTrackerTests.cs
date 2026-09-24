using CompoundingPerf.Features;
using SPTarkov.Server.Core.Models.Common;
using Xunit;

namespace CompoundingPerf.Tests;

[Collection("DirtyTrackerSerial")] // static state — keep these off the parallel test runner
public class ProfileDirtyTrackerTests
{
    private static MongoId NewSession() => new MongoId(Guid.NewGuid().ToString("N")[..24]);

    public ProfileDirtyTrackerTests()
    {
        ProfileDirtyTracker.IsEnabled = true;
        ProfileDirtyTracker.ForceSaveIntervalSeconds = 60;
    }

    [Fact]
    public void Never_skips_before_first_real_save()
    {
        var s = NewSession();
        // No save has happened yet — must not skip even though session is clean.
        Assert.False(ProfileDirtyTracker.MaySkipSave(s));
    }

    [Fact]
    public void Skips_when_clean_and_recent()
    {
        var s = NewSession();
        ProfileDirtyTracker.OnRealSaveStarting(s);
        Assert.True(ProfileDirtyTracker.MaySkipSave(s));
    }

    [Fact]
    public void Mutating_request_blocks_skip_until_next_real_save()
    {
        var s = NewSession();
        ProfileDirtyTracker.OnRealSaveStarting(s);

        ProfileDirtyTracker.MarkRequest(s, "/client/game/profile/items/moving");
        Assert.False(ProfileDirtyTracker.MaySkipSave(s));

        ProfileDirtyTracker.OnRealSaveStarting(s);
        Assert.True(ProfileDirtyTracker.MaySkipSave(s));
    }

    [Theory]
    [InlineData("/client/game/keepalive")]
    [InlineData("/launcher/ping")]
    [InlineData("/launcher/server/version")]
    [InlineData("/fika/update/ping")]
    [InlineData("/client/notifier/channel/create")]
    [InlineData("/notifierServer/getwebsocket/abc")]
    [InlineData("/client/putMetrics")]
    [InlineData("/singleplayer/settings/raid/menu")]
    [InlineData("/files/launcher/bg.png")]
    [InlineData("/client/items")]
    [InlineData("/client/globals")]
    public void Pure_paths_do_not_dirty(string purePath)
    {
        var s = NewSession();
        ProfileDirtyTracker.OnRealSaveStarting(s);

        ProfileDirtyTracker.MarkRequest(s, purePath);
        Assert.True(ProfileDirtyTracker.MaySkipSave(s));
    }

    [Theory]
    [InlineData("/client/game/profile/items/moving")]
    [InlineData("/client/match/local/end")]
    [InlineData("/client/quest/list")]
    [InlineData("/client/ragfair/find")]
    [InlineData("/some/unknown/modded/route")]
    public void Unknown_and_mutating_paths_dirty(string path)
    {
        var s = NewSession();
        ProfileDirtyTracker.OnRealSaveStarting(s);

        ProfileDirtyTracker.MarkRequest(s, path);
        Assert.False(ProfileDirtyTracker.MaySkipSave(s));
    }

    [Fact]
    public void Force_interval_expiry_blocks_skip()
    {
        var s = NewSession();
        ProfileDirtyTracker.ForceSaveIntervalSeconds = 0; // everything is instantly stale
        ProfileDirtyTracker.OnRealSaveStarting(s);
        Assert.False(ProfileDirtyTracker.MaySkipSave(s));
        ProfileDirtyTracker.ForceSaveIntervalSeconds = 60;
    }

    [Fact]
    public void Disabled_never_skips_and_never_marks()
    {
        var s = NewSession();
        ProfileDirtyTracker.OnRealSaveStarting(s);
        ProfileDirtyTracker.IsEnabled = false;
        Assert.False(ProfileDirtyTracker.MaySkipSave(s));
        ProfileDirtyTracker.IsEnabled = true;
    }
}
