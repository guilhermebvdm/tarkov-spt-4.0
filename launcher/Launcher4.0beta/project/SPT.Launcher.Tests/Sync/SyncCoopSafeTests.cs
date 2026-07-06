using SPT.Launcher.Sync;
using Xunit;

namespace SPT.Launcher.Tests.Sync
{
    /// <summary>
    /// Item 023 (Frente A) — unit coverage for the embedded coop-safe allowlist predicate.
    /// Matching is by assembly-name family (Fika.*.dll) under a plugins/patchers prefix (RN-1/CC-3).
    /// </summary>
    public class SyncCoopSafeTests
    {
        [Theory]
        [InlineData("BepInEx/plugins/Fika.Core.dll")]           // family core, plugins root
        [InlineData("BepInEx/plugins/Fika.Dedicated.dll")]      // any Fika.* member (CA-A2)
        [InlineData("BepInEx/plugins/Fika/Fika.Core.dll")]      // nested in a subfolder
        [InlineData("BepInEx/patchers/Fika.Patcher.dll")]       // patchers prefix also mirror
        [InlineData("plugins/Fika.Core.dll")]                   // raw card-name prefix (no BepInEx/)
        [InlineData("FIKA.core.DLL")]                            // *the file itself* under an implicit... see below
        public void CoopEssential_true_for_fika_family_under_plugin_prefix(string path)
        {
            // The last case is a bare filename with no prefix — normalize keeps it prefix-less, so it is
            // NOT coop-safe. Guard that one separately; the rest must be true.
            if (path == "FIKA.core.DLL")
            {
                Assert.False(SyncCoopSafe.IsCoopEssentialPlugin(path));
                return;
            }

            Assert.True(SyncCoopSafe.IsCoopEssentialPlugin(path));
        }

        [Fact]
        public void CoopEssential_true_for_mixed_case_under_prefix()
        {
            // Mixed casing on both the prefix and the file name → still true (predicate normalizes).
            Assert.True(SyncCoopSafe.IsCoopEssentialPlugin("BepInEx/Plugins/FIKA.Core.DLL"));
        }

        [Theory]
        [InlineData("BepInEx/plugins/OldMod/old.dll")]          // unrelated extra (CA-A3)
        [InlineData("BepInEx/plugins/SomeOther.dll")]           // non-Fika assembly
        [InlineData("BepInEx/plugins/FikaExtras/other.dll")]    // "fika" only in FOLDER name, not assembly (CC-3)
        [InlineData("BepInEx/plugins/Fika.Core.txt")]           // Fika name but not a .dll
        [InlineData("user/mods/Fika.Core.dll")]                 // Fika.* but outside a mirror prefix
        [InlineData("config/Fika.Core.dll")]                    // preserve-divergent folder, not a plugin prefix
        [InlineData("")]                                        // empty
        [InlineData(null)]                                      // null
        public void CoopEssential_false_for_non_matching(string path)
        {
            Assert.False(SyncCoopSafe.IsCoopEssentialPlugin(path));
        }

        [Fact]
        public void Patterns_are_exposed_and_non_empty()
        {
            Assert.NotEmpty(SyncCoopSafe.Patterns);
            Assert.Contains("fika.*.dll", SyncCoopSafe.Patterns);
        }
    }
}
