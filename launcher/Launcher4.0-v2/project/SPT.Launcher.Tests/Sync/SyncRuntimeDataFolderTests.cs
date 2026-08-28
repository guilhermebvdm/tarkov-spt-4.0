using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using SPT.Launcher.Models.Launcher;
using SPT.Launcher.Sync;
using Xunit;

namespace SPT.Launcher.Tests.Sync
{
    /// <summary>
    /// Pastas de DADOS DE RUNTIME de um mod (plugins/&lt;mod&gt;/data/...) nunca são quarentenadas: são
    /// criadas pelo mod (histórico de raids do CareerLog, progressão do SPTMapProgression), nunca vêm no
    /// manifesto, e some-las como "extra" deslocaria/perderia o histórico do jogador. Vale com o mod
    /// LIGADO (preservada no lugar) e DESLIGADO (o mod vai à quarentena, a pasta data/ fica).
    /// </summary>
    public class SyncRuntimeDataFolderTests
    {
        // ---- Regra PURA: SyncPathUtil.IsRuntimeDataPath ----

        [Theory]
        [InlineData("bepinex/plugins/softwyx.careerlog/data/raid1.json", "bepinex/plugins", true)]
        [InlineData("bepinex/patchers/foo/data/x.bin", "bepinex/patchers", true)]
        [InlineData("bepinex/plugins/mod/sub/data/x.json", "bepinex/plugins", true)]   // data em nível fundo
        [InlineData("bepinex/plugins/careerlog/careerlog.dll", "bepinex/plugins", false)] // não é data
        [InlineData("bepinex/plugins/data/foo.dll", "bepinex/plugins", false)]          // "data" é o nome do mod (1º nível)
        [InlineData("bepinex/plugins/mod/database/x.json", "bepinex/plugins", false)]   // "database" != segmento "data"
        [InlineData("bepinex/config/foo/data/x", "bepinex/plugins", false)]             // fora do prefixo casado
        [InlineData("bepinex/plugins/mod/data/evil.dll", "bepinex/plugins", false)]     // .dll sob data/ = código → quarentena
        [InlineData("bepinex/plugins/mod/data/tool.exe", "bepinex/plugins", false)]     // .exe idem
        [InlineData("bepinex/plugins/mod/data", "bepinex/plugins", false)]              // arquivo "data" terminal (não é pasta)
        public void IsRuntimeDataPath_identifies_mod_data_folders(string path, string prefix, bool expected)
        {
            Assert.Equal(expected, SyncPathUtil.IsRuntimeDataPath(path, prefix));
        }

        // "Data" com D maiúsculo (SPTMapProgression) → Normalize lowercaseia → a regra pega de graça.
        [Fact]
        public void IsRuntimeDataPath_is_case_insensitive_via_normalize()
        {
            string norm = SyncPathUtil.Normalize("BepInEx/plugins/SPTMapProgression/Data/progress.json");
            Assert.True(SyncPathUtil.IsRuntimeDataPath(norm, "bepinex/plugins"));
        }

        // ---- Integração no planner/engine ----

        // Mod LIGADO: a pasta data/ (fora do manifesto) é preservada no lugar, nada é movido.
        [Fact]
        public async Task Enabled_optional_mod_keeps_its_runtime_data_folder()
        {
            using var fx = new SyncTestFixture();
            fx.WriteLocal("BepInEx/plugins/Softwyx.CareerLog/Softwyx.CareerLog.dll", "dll");
            fx.WriteLocal("BepInEx/plugins/Softwyx.CareerLog/data/raid1.json", "r1");
            fx.WriteLocal("BepInEx/plugins/Softwyx.CareerLog/data/raid2.json", "r2");

            var manifest = new[]
            {
                fx.Entry("BepInEx/plugins/Softwyx.CareerLog/Softwyx.CareerLog.dll", "dll", optional: true, optionalId: "career-log"),
            };

            var (plan, _, _) = await fx.PlanAndRunAsync(manifest, fx.Options(optionalEnabled: _ => true));

            Assert.Equal(0, plan.MoveCount);
            Assert.Equal(0, plan.MoveDirCount);
            Assert.True(fx.LocalExists("BepInEx/plugins/Softwyx.CareerLog/data/raid1.json"));
            Assert.True(fx.LocalExists("BepInEx/plugins/Softwyx.CareerLog/data/raid2.json"));
            Assert.False(fx.LocalExists("BepInEx/plugins-disabled/optional/Softwyx.CareerLog/data/raid1.json"));
        }

        // "Data" com D maiúsculo (SPTMapProgression) também é preservada (case-insensitive).
        [Fact]
        public async Task Enabled_optional_mod_keeps_data_folder_case_insensitive()
        {
            using var fx = new SyncTestFixture();
            fx.WriteLocal("BepInEx/plugins/SPTMapProgression/SPTMapProgression.dll", "dll");
            fx.WriteLocal("BepInEx/plugins/SPTMapProgression/Data/progress.json", "p");

            var manifest = new[]
            {
                fx.Entry("BepInEx/plugins/SPTMapProgression/SPTMapProgression.dll", "dll", optional: true, optionalId: "mapprogression"),
            };

            var (plan, _, _) = await fx.PlanAndRunAsync(manifest, fx.Options(optionalEnabled: _ => true));

            Assert.Equal(0, plan.MoveCount);
            Assert.True(fx.LocalExists("BepInEx/plugins/SPTMapProgression/Data/progress.json"));
        }

        // Regressão: um extra NORMAL (fora de data/) sob um mod ligado continua sendo quarentenado.
        [Fact]
        public async Task Non_data_extra_under_enabled_mod_is_still_quarantined()
        {
            using var fx = new SyncTestFixture();
            fx.WriteLocal("BepInEx/plugins/Softwyx.CareerLog/Softwyx.CareerLog.dll", "dll");
            fx.WriteLocal("BepInEx/plugins/Softwyx.CareerLog/stray.dll", "stray"); // extra, NÃO em data/

            var manifest = new[]
            {
                fx.Entry("BepInEx/plugins/Softwyx.CareerLog/Softwyx.CareerLog.dll", "dll", optional: true, optionalId: "career-log"),
            };

            var (plan, _, _) = await fx.PlanAndRunAsync(manifest, fx.Options(optionalEnabled: _ => true));

            Assert.Equal(1, plan.MoveCount);       // só o stray
            Assert.Equal(0, plan.MoveDirCount);
            Assert.False(fx.LocalExists("BepInEx/plugins/Softwyx.CareerLog/stray.dll"));
        }

        // 🟠 CR: um .dll escondido sob data/ NÃO é preservado — o BepInEx o carregaria como plugin fora do
        // manifesto (coop-desync); só dados (.json/etc.) ficam.
        [Fact]
        public async Task Dll_inside_data_folder_is_still_quarantined()
        {
            using var fx = new SyncTestFixture();
            fx.WriteLocal("BepInEx/plugins/Softwyx.CareerLog/Softwyx.CareerLog.dll", "dll");
            fx.WriteLocal("BepInEx/plugins/Softwyx.CareerLog/data/raid1.json", "r1");     // dado → preservado
            fx.WriteLocal("BepInEx/plugins/Softwyx.CareerLog/data/sneaky.dll", "sneaky"); // assembly → quarentena

            var manifest = new[]
            {
                fx.Entry("BepInEx/plugins/Softwyx.CareerLog/Softwyx.CareerLog.dll", "dll", optional: true, optionalId: "career-log"),
            };

            var (plan, _, _) = await fx.PlanAndRunAsync(manifest, fx.Options(optionalEnabled: _ => true));

            Assert.Equal(1, plan.MoveCount);                                                   // só o sneaky.dll
            Assert.False(fx.LocalExists("BepInEx/plugins/Softwyx.CareerLog/data/sneaky.dll"));  // quarentenado
            Assert.True(fx.LocalExists("BepInEx/plugins/Softwyx.CareerLog/data/raid1.json"));   // dado ficou
        }

        // Mod DESLIGADO: o mod vai à quarentena, mas a pasta data/ FICA no lugar (dados nunca se movem).
        [Fact]
        public async Task Disabled_optional_mod_keeps_data_folder_in_place()
        {
            using var fx = new SyncTestFixture();
            fx.WriteLocal("BepInEx/plugins/Softwyx.CareerLog/Softwyx.CareerLog.dll", "dll");
            fx.WriteLocal("BepInEx/plugins/Softwyx.CareerLog/data/raid1.json", "r1");

            var manifest = new[]
            {
                fx.Entry("BepInEx/plugins/Softwyx.CareerLog/Softwyx.CareerLog.dll", "dll", optional: true, optionalId: "career-log"),
            };

            var (plan, _, _) = await fx.PlanAndRunAsync(manifest, fx.Options(optionalEnabled: _ => false));

            // o dll (manifesto) foi para a quarentena PER-FILE (a data/ impede o move da pasta inteira)...
            Assert.Equal(0, plan.MoveDirCount);
            Assert.True(fx.LocalExists("BepInEx/plugins-disabled/optional/Softwyx.CareerLog/Softwyx.CareerLog.dll"));
            // ...e a data/ ficou EXATAMENTE onde estava.
            Assert.True(fx.LocalExists("BepInEx/plugins/Softwyx.CareerLog/data/raid1.json"));
            Assert.False(fx.LocalExists("BepInEx/plugins-disabled/optional/Softwyx.CareerLog/data/raid1.json"));
        }

        // 🟡 CR #1: data/ sob um managedPath (ex.: user/mods/<mod>) resolve p/ Default → DeleteExtra. A
        // guarda (via rootPrefix) impede a DELEÇÃO silenciosa; um extra normal no mesmo managedPath é limpo.
        [Fact]
        public async Task Data_folder_under_managed_path_is_not_deleted()
        {
            using var fx = new SyncTestFixture();
            fx.WriteLocal("user/mods/SomeServerMod/data/state.json", "s");
            fx.WriteLocal("user/mods/SomeServerMod/stray.json", "x");

            var opts = fx.Options();
            opts.ManagedPaths = new[] { "user/mods" };

            await fx.PlanAndRunAsync(Array.Empty<ManifestFile>(), opts);

            Assert.True(fx.LocalExists("user/mods/SomeServerMod/data/state.json"));   // dado preservado
            Assert.False(fx.LocalExists("user/mods/SomeServerMod/stray.json"));       // extra normal deletado
        }

        // 🟡 CR #2: se plugins for configurado como mirror-delete (folderRules do server), a data/ sobrevive
        // ao DELETE por esse ramo; um extra normal é deletado.
        [Fact]
        public async Task Data_folder_survives_mirror_delete_rule()
        {
            using var fx = new SyncTestFixture();
            fx.WriteLocal("BepInEx/plugins/Mod/data/state.json", "s");
            fx.WriteLocal("BepInEx/plugins/Mod/stray.txt", "x");

            var resolver = new SyncRuleResolver(new Dictionary<string, string>
            {
                ["BepInEx/plugins"] = "mirror-delete",
                ["plugins"] = "mirror-delete",
            });

            await fx.PlanAndRunAsync(Array.Empty<ManifestFile>(), fx.Options(), resolver);

            Assert.True(fx.LocalExists("BepInEx/plugins/Mod/data/state.json"));   // data/ preservada do delete
            Assert.False(fx.LocalExists("BepInEx/plugins/Mod/stray.txt"));        // extra normal deletado
        }

        // Mod DESLIGADO com um .dll escondido sob data/: o dado (.json) fica, o assembly vai à quarentena.
        [Fact]
        public async Task Disabled_mod_dll_under_data_is_quarantined_data_stays()
        {
            using var fx = new SyncTestFixture();
            fx.WriteLocal("BepInEx/plugins/CareerLog/CareerLog.dll", "dll");
            fx.WriteLocal("BepInEx/plugins/CareerLog/data/raid1.json", "r1");
            fx.WriteLocal("BepInEx/plugins/CareerLog/data/sneaky.dll", "sneaky");

            var manifest = new[]
            {
                fx.Entry("BepInEx/plugins/CareerLog/CareerLog.dll", "dll", optional: true, optionalId: "career-log"),
            };

            await fx.PlanAndRunAsync(manifest, fx.Options(optionalEnabled: _ => false));

            Assert.True(fx.LocalExists("BepInEx/plugins/CareerLog/data/raid1.json"));   // dado fica no lugar
            Assert.False(fx.LocalExists("BepInEx/plugins/CareerLog/data/sneaky.dll"));  // assembly quarentenado
        }
    }
}
