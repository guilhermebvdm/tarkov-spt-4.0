using System.IO;
using SPT.Launcher.Sync;
using Xunit;

namespace SPT.Launcher.Tests.Sync
{
    public class SyncBaselineTests
    {
        [Fact]
        public void Round_trip_preserves_entries_and_normalizes_paths()
        {
            using var fx = new SyncTestFixture();

            var baseline = SyncBaseline.Load(fx.BaselinePath);
            Assert.Equal(0, baseline.Count);

            baseline.SetHash(@"BepInEx\Config\Foo.CFG", "ABCDEF0123456789");
            baseline.SetHash("user/mods/x/mod.js", "1234");
            baseline.Save();

            var reloaded = SyncBaseline.Load(fx.BaselinePath);
            Assert.Equal(2, reloaded.Count);

            // Lookup works regardless of separator/casing; hash stored lower-case
            Assert.True(reloaded.TryGetHash("bepinex/config/foo.cfg", out string hash));
            Assert.Equal("abcdef0123456789", hash);

            reloaded.Remove("USER/MODS/X/MOD.JS");
            Assert.Equal(1, reloaded.Count);
        }

        [Fact]
        public void Missing_file_yields_empty_baseline()
        {
            using var fx = new SyncTestFixture();

            var baseline = SyncBaseline.Load(Path.Combine(fx.Root, "nope", "sync-state.json"));

            Assert.Equal(0, baseline.Count);
        }

        [Fact]
        public void Corrupt_file_yields_empty_baseline()
        {
            using var fx = new SyncTestFixture();

            Directory.CreateDirectory(Path.GetDirectoryName(fx.BaselinePath));
            File.WriteAllText(fx.BaselinePath, "{ not valid json !!!");

            var baseline = SyncBaseline.Load(fx.BaselinePath);

            Assert.Equal(0, baseline.Count);
        }
    }
}
