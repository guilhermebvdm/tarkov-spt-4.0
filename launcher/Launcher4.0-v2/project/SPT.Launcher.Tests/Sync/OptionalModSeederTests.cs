using System;
using System.Collections.Generic;
using System.IO;
using SPT.Launcher.Helpers;
using SPT.Launcher.Models.Launcher;
using Xunit;

namespace SPT.Launcher.Tests.Sync
{
    /// <summary>
    /// Item 033: seed do disco. Cobre CA-033.1/2/3/5 + CC-1/1c/6/13/16. ComputeSeed é PURA (retorna o dict,
    /// não persiste), então basta um gameRoot de fixture no disco.
    /// </summary>
    public class OptionalModSeederTests : IDisposable
    {
        private readonly string _root;

        public OptionalModSeederTests()
        {
            _root = Path.Combine(Path.GetTempPath(), "seedtest_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_root);
        }

        public void Dispose()
        {
            try { Directory.Delete(_root, true); } catch { /* best-effort */ }
        }

        private void WriteFile(string rel, string content = "x")
        {
            string full = Path.Combine(_root, rel.Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(Path.GetDirectoryName(full));
            File.WriteAllText(full, content);
        }

        private static ManifestFile OptFile(string path, string optionalId) =>
            new ManifestFile { path = path, optional = true, optionalId = optionalId };

        // CA-033.1 — jogador COM plugins: instalado → ligado; ausente → desligado.
        [Fact]
        public void Seed_with_plugins_enables_installed_disables_absent()
        {
            WriteFile("BepInEx/plugins/SomeOther.dll");   // garante hasPlugins = true
            WriteFile("BepInEx/plugins/Foo/main.dll");    // Foo instalado

            var manifest = new List<ManifestFile>
            {
                OptFile("BepInEx/plugins/Foo/main.dll", "foo"),
                OptFile("BepInEx/plugins/Bar/main.dll", "bar"),   // ausente
            };
            var cats = new Dictionary<string, string> { ["foo"] = "pesados", ["bar"] = "pesados" };

            var seeded = OptionalModSeeder.ComputeSeed(manifest, _root, new HashSet<string>(), cats);

            Assert.True(seeded["foo"]);
            Assert.False(seeded["bar"]);
        }

        // CA-033.2 — jogador SEM plugins: Optional/Heavy/Performance ligados; dev desligado.
        [Fact]
        public void Seed_without_plugins_enables_optional_heavy_performance_not_dev()
        {
            Directory.CreateDirectory(Path.Combine(_root, "BepInEx", "plugins")); // pasta vazia → sem plugins

            var manifest = new List<ManifestFile>
            {
                OptFile("BepInEx/plugins/A.dll", "a"),
                OptFile("BepInEx/plugins/B.dll", "b"),
                OptFile("BepInEx/plugins/C.dll", "c"),
                OptFile("BepInEx/plugins/D.dll", "d"),
            };
            var cats = new Dictionary<string, string>
            {
                ["a"] = "opcionais", ["b"] = "pesados", ["c"] = "performance", ["d"] = "dev",
            };

            var seeded = OptionalModSeeder.ComputeSeed(manifest, _root, new HashSet<string>(), cats);

            Assert.True(seeded["a"]);
            Assert.True(seeded["b"]);
            Assert.True(seeded["c"]);
            Assert.False(seeded["d"]); // dev fica off
        }

        // CA-033.3 — id já decidido não é re-semeado.
        [Fact]
        public void Seed_respects_already_decided_ids()
        {
            WriteFile("BepInEx/plugins/Foo/main.dll");
            var manifest = new List<ManifestFile> { OptFile("BepInEx/plugins/Foo/main.dll", "foo") };
            var cats = new Dictionary<string, string> { ["foo"] = "pesados" };

            var seeded = OptionalModSeeder.ComputeSeed(manifest, _root, new HashSet<string> { "foo" }, cats);

            Assert.DoesNotContain("foo", seeded.Keys);
        }

        // CC-1 — mod-pasta conta como instalado por QUALQUER arquivo presente.
        [Fact]
        public void Seed_folder_mod_counts_as_installed_by_any_file()
        {
            WriteFile("BepInEx/plugins/SomeOther.dll");       // hasPlugins
            WriteFile("BepInEx/plugins/FooMod/data.bundle");  // só 1 dos arquivos do mod presente

            var manifest = new List<ManifestFile>
            {
                OptFile("BepInEx/plugins/FooMod/main.dll", "foomod"),     // ausente
                OptFile("BepInEx/plugins/FooMod/data.bundle", "foomod"),  // presente
            };
            var cats = new Dictionary<string, string> { ["foomod"] = "pesados" };

            var seeded = OptionalModSeeder.ComputeSeed(manifest, _root, new HashSet<string>(), cats);

            Assert.True(seeded["foomod"]);
        }

        // CC-1c / CC-13 — plugins-disabled NÃO conta para o gate "tem plugins".
        [Fact]
        public void Seed_ignores_plugins_disabled_for_hasPlugins_gate()
        {
            Directory.CreateDirectory(Path.Combine(_root, "BepInEx", "plugins")); // plugins/ vazio
            WriteFile("BepInEx/plugins-disabled/optional/Foo/main.dll");          // só na quarentena

            var manifest = new List<ManifestFile> { OptFile("BepInEx/plugins/Foo/main.dll", "foo") };
            var cats = new Dictionary<string, string> { ["foo"] = "pesados" };

            var seeded = OptionalModSeeder.ComputeSeed(manifest, _root, new HashSet<string>(), cats);

            // hasPlugins = false (plugins/ sem .dll) → gate "sem plugins" → liga por categoria.
            Assert.True(seeded["foo"]);
        }

        // CA-033.5 — configs (optionalConfigId, optional=false) nunca entram no seed de mods.
        [Fact]
        public void Seed_never_returns_configs()
        {
            WriteFile("BepInEx/plugins/SomeOther.dll");
            var manifest = new List<ManifestFile>
            {
                new ManifestFile { path = "BepInEx/config-optional/x.cfg", optional = false, optionalConfigId = "cfg1" },
            };

            var seeded = OptionalModSeeder.ComputeSeed(manifest, _root, new HashSet<string>(), new Dictionary<string, string>());

            Assert.Empty(seeded);
        }

        // CC-6 / CC-16 — idempotência: id já semeado numa passada anterior é pulado na seguinte.
        [Fact]
        public void Seed_is_idempotent_on_repeated_runs()
        {
            WriteFile("BepInEx/plugins/Foo/main.dll");
            var manifest = new List<ManifestFile> { OptFile("BepInEx/plugins/Foo/main.dll", "foo") };
            var cats = new Dictionary<string, string> { ["foo"] = "pesados" };

            var first = OptionalModSeeder.ComputeSeed(manifest, _root, new HashSet<string>(), cats);
            Assert.True(first.ContainsKey("foo"));

            var second = OptionalModSeeder.ComputeSeed(manifest, _root, new HashSet<string> { "foo" }, cats);
            Assert.Empty(second);
        }
    }
}
