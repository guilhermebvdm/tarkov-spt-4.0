using System;
using System.IO;
using SPT.Launcher.Sync;
using Xunit;

namespace SPT.Launcher.Tests.Sync
{
    /// <summary>
    /// Item 019 — unit coverage for the shared root guard <see cref="SyncPathUtil.ResolveUnderRoot"/>.
    /// This is the single source of truth used by BOTH the sync engine and the legacy FS paths
    /// (deleteFiles loop + OptionalModsHelper), so proving it here proves the guard for all of them
    /// (CA-1/2/3/4/6/8). Pure — no IO: <see cref="Path.GetFullPath(string)"/> resolves non-existent
    /// paths, so the roots below need not exist on disk.
    /// </summary>
    public class SyncPathGuardTests
    {
        // Root deliberately ends in "SPT" so a sibling "SPTevil" exercises the prefix guard (RN-2).
        private static readonly string Root =
            Path.Combine(Path.GetTempPath(), "spt-guard-root", "SPT");

        [Theory]
        [InlineData("../../evil.txt")]      // CA-1 — traversal above the root
        [InlineData("../evil.dll")]         // CA-8 — same input as the engine traversal test
        [InlineData("../SPTevil/x.dll")]    // RN-2 — sibling-prefix escape (SPT vs SPTevil)
        [InlineData("a/../../../outside")]  // traversal that climbs past the root after normalizing
        public void Escaping_paths_throw_with_stable_message(string relativePath)
        {
            var ex = Assert.Throws<InvalidOperationException>(
                () => SyncPathUtil.ResolveUnderRoot(Root, relativePath));

            // The engine's traversal test (SyncEngineTests) asserts on this exact substring —
            // keeping it identical is what makes the guard a single source of truth (CA-8).
            Assert.Contains("escapes game root", ex.Message);
        }

        [Fact]
        public void Absolute_path_escapes_the_root() // CA-2
        {
            // An absolute path makes Path.Combine discard the root entirely, so it resolves outside.
            string absoluteOutside = Path.Combine(Path.GetTempPath(), "spt-guard-outside.txt");

            var ex = Assert.Throws<InvalidOperationException>(
                () => SyncPathUtil.ResolveUnderRoot(Root, absoluteOutside));
            Assert.Contains("escapes game root", ex.Message);
        }

        [Theory]
        [InlineData("BepInEx/plugins/x.dll")]   // CA-3 — plain legit path
        [InlineData("user/mods/ok/ok.js")]      // CA-8 — the legit sibling of the engine test
        [InlineData("a/../b.txt")]              // corner case — benign traversal that stays under root
        public void Legit_paths_resolve_under_the_root(string relativePath)
        {
            string resolved = SyncPathUtil.ResolveUnderRoot(Root, relativePath);

            string rootPrefix = Path.GetFullPath(Root)
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                + Path.DirectorySeparatorChar;
            Assert.StartsWith(rootPrefix, resolved, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Benign_traversal_resolves_to_the_expected_file() // corner case (01-spec)
        {
            string resolved = SyncPathUtil.ResolveUnderRoot(Root, "a/../b.txt");

            string expected = Path.GetFullPath(Path.Combine(Root, "b.txt"));
            Assert.Equal(expected, resolved);
        }

        [Fact]
        public void Legit_path_returns_the_full_combined_path() // CA-3/CA-8
        {
            string resolved = SyncPathUtil.ResolveUnderRoot(Root, "BepInEx/plugins/x.dll");

            string expected = Path.GetFullPath(
                Path.Combine(Root, "BepInEx", "plugins", "x.dll"));
            Assert.Equal(expected, resolved);
        }

        [Fact]
        public void Backslash_and_forwardslash_are_treated_equally() // RN-5 — separators normalized
        {
            string viaForward = SyncPathUtil.ResolveUnderRoot(Root, "BepInEx/config/mod.cfg");
            string viaBack = SyncPathUtil.ResolveUnderRoot(Root, "BepInEx\\config\\mod.cfg");

            Assert.Equal(viaForward, viaBack);
        }

        // --- Guard de quarentena (R3.4): o loop deleteFiles do ProfileViewModel pula qualquer path
        // com segmento "-disabled" via ContainsDisabledSegment(Normalize(...)) — protege o backup do
        // config-force (config-disabled/) e toda quarentena de ser apagada por um manifesto. ---

        [Theory]
        [InlineData("config-disabled/graphics.cfg")]          // backup do config-force
        [InlineData("BepInEx/config-disabled/graphics.cfg")]  // idem, layout BepInEx
        [InlineData("BepInEx/plugins-disabled/mod.dll")]      // quarentena de plugin
        [InlineData("CONFIG-DISABLED/x.cfg")]                 // casing normalizado
        [InlineData("config\\config-disabled\\x.cfg")]        // backslash normalizado
        public void Disabled_segments_are_flagged_for_the_deleteFiles_guard(string deleteFile)
        {
            Assert.True(SyncPathUtil.ContainsDisabledSegment(SyncPathUtil.Normalize(deleteFile)));
        }

        [Theory]
        [InlineData("config/graphics.cfg")]           // config viva — deletável
        [InlineData("config-server/reference.cfg")]   // referência — não é -disabled
        [InlineData("disabled/x.cfg")]                // "disabled" sozinho NÃO termina em "-disabled"
        [InlineData("BepInEx/plugins/mod.dll")]
        public void Non_disabled_segments_are_not_flagged(string deleteFile)
        {
            Assert.False(SyncPathUtil.ContainsDisabledSegment(SyncPathUtil.Normalize(deleteFile)));
        }
    }
}
