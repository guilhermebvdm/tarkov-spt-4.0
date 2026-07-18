using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using SPT.Launcher.Sync;
using Xunit;

namespace SPT.Launcher.Tests.Sync
{
    /// <summary>
    /// config-server = MIRROR-REFERENCE (o default do fallback desde a 2.3.0). A pasta 'config-server/'
    /// do cliente é apenas uma biblioteca de referência espelhada do server:
    ///   • baixa a última versão sempre que faltar ou o hash divergir (restaura — pasta pristina);
    ///   • NUNCA semeia/escreve em 'config/' (quem distribui defaults é o canal 'config');
    ///   • NÃO deleta extras (fora de MirrorPrefixes; pulado no ScanExtras — conservador, ref CR-01-03).
    /// Substitui o antigo SeedAndMirror (que também semeava em config/).
    /// </summary>
    public class SyncMirrorReferenceTests
    {
        [Fact]
        public async Task Mirrors_config_server_without_seeding_config_on_first_sync()
        {
            using var fx = new SyncTestFixture();
            var manifest = new[] { fx.Entry("BepInEx/config-server/graphics.cfg", "server-v1") };

            // resolver default (fallback) = mirror-reference para config-server
            var (_, result, _) = await fx.PlanAndRunAsync(manifest);

            // MIRROR -> config-server (materializado no disco, cópia exata)
            Assert.Equal(1, result.Updated);
            Assert.Equal("server-v1", fx.ReadLocal("BepInEx/config-server/graphics.cfg"));

            // NÃO semeia mais em config/
            Assert.Equal(0, result.Seeded);
            Assert.False(fx.LocalExists("BepInEx/config/graphics.cfg"));
        }

        [Fact]
        public async Task Mirror_overwrites_user_edited_config_server_with_latest()
        {
            using var fx = new SyncTestFixture();
            fx.WriteLocal("BepInEx/config-server/graphics.cfg", "user-edited-the-reference"); // difere do server
            var manifest = new[] { fx.Entry("BepInEx/config-server/graphics.cfg", "server-v2") };

            var (_, result, _) = await fx.PlanAndRunAsync(manifest);

            Assert.Equal(1, result.Updated);                                       // réplica exata: sobrescreve
            Assert.Equal("server-v2", fx.ReadLocal("BepInEx/config-server/graphics.cfg"));
            Assert.Equal(0, result.Seeded);
            Assert.False(fx.LocalExists("BepInEx/config/graphics.cfg"));           // nunca toca config/
        }

        [Fact]
        public async Task Mirror_is_noop_when_config_server_already_matches()
        {
            using var fx = new SyncTestFixture();
            fx.WriteLocal("BepInEx/config-server/graphics.cfg", "server-v1"); // == server
            var manifest = new[] { fx.Entry("BepInEx/config-server/graphics.cfg", "server-v1") };

            var (_, result, _) = await fx.PlanAndRunAsync(manifest);

            Assert.Equal(0, result.Updated);
            Assert.Equal(0, result.Seeded);
            Assert.Empty(fx.DownloadedPaths); // nada a baixar: hash bate
        }

        [Fact]
        public async Task Reference_updated_is_labelled_distinctly_in_report()
        {
            using var fx = new SyncTestFixture();
            var manifest = new[] { fx.Entry("BepInEx/config-server/graphics.cfg", "server-v1") };

            var (_, result, _) = await fx.PlanAndRunAsync(manifest);

            // O mirror reusa Download, mas o relatório separa "a SUA config" da "referência".
            Assert.Contains(result.Entries, e => e.action == "reference-updated");
            Assert.DoesNotContain(result.Entries, e => e.action == "updated");
        }

        [Fact]
        public async Task Report_counts_separate_reference_from_updated()
        {
            using var fx = new SyncTestFixture();
            var manifest = new[] { fx.Entry("BepInEx/config-server/graphics.cfg", "server-v1") };

            await fx.PlanAndRunAsync(manifest);

            // O relatório desconta as referências do 'updated' → não dupla-conta (CR passada B).
            var counts = JObject.Parse(File.ReadAllText(fx.ReportPath))["counts"];
            Assert.Equal(0, (int)counts["updated"]);          // nenhuma config do usuário mudou
            Assert.Equal(1, (int)counts["referenceUpdated"]); // só a biblioteca de referência
        }

        [Fact]
        public async Task Extra_in_config_server_is_never_deleted()
        {
            using var fx = new SyncTestFixture();
            fx.WriteLocal("BepInEx/config-server/stale.json", "old"); // não está no manifesto
            var manifest = new[] { fx.Entry("BepInEx/config-server/keep.cfg", "keep") };

            var options = fx.Options();
            options.ManagedPaths = new[] { "BepInEx" }; // força a varredura cobrir config-server

            var (plan, result, _) = await fx.PlanAndRunAsync(manifest, options);

            Assert.DoesNotContain(plan.Actions, a => a.Kind == SyncActionKind.DeleteExtra);
            Assert.Equal(0, result.Deleted);
            Assert.True(fx.LocalExists("BepInEx/config-server/stale.json")); // preservado (conservador)
        }

        /// <summary>
        /// Regressão-chave da 2.3.0: um arquivo presente SÓ em config-server/ (sem par em config/)
        /// NÃO pode criar/alterar config/&lt;rel&gt;. É o coração da mudança "mirror-only".
        /// </summary>
        [Fact]
        public async Task File_only_in_config_server_never_creates_config_target()
        {
            using var fx = new SyncTestFixture();
            var manifest = new[] { fx.Entry("BepInEx/config-server/only-here.cfg", "reference-content") };

            var (plan, result, _) = await fx.PlanAndRunAsync(manifest);

            // Nenhuma ação escreve em config/ (nem SeedCopy nem ForceCopy)
            Assert.DoesNotContain(plan.Actions, a => a.Kind == SyncActionKind.SeedCopy);
            Assert.Equal(0, result.Seeded);
            Assert.False(fx.LocalExists("BepInEx/config/only-here.cfg"));

            // A referência em si é materializada
            Assert.True(fx.LocalExists("BepInEx/config-server/only-here.cfg"));
        }

        /// <summary>
        /// Ganho colateral da 2.3.0: com config-server mirror-only, ele nunca escreve em config/.
        /// Quando config-force/x e config-server/x coexistem com config/x ausente, só o force escreve
        /// em config/x — a colisão latente da 2.2.1 (ForceCopy + SeedCopy no mesmo alvo) desaparece.
        /// </summary>
        [Fact]
        public async Task Config_force_and_config_server_same_relpath_only_force_writes_config()
        {
            using var fx = new SyncTestFixture();
            var manifest = new[]
            {
                fx.Entry("BepInEx/config-force/graphics.cfg", "forced-v1"),
                fx.Entry("BepInEx/config-server/graphics.cfg", "reference-v1"),
            };

            var (_, result, _) = await fx.PlanAndRunAsync(manifest);

            // config/ recebe só o force (config/graphics.cfg absent -> sem backup)
            Assert.Equal(1, result.Forced);
            Assert.Equal(0, result.Seeded);
            Assert.Equal("forced-v1", fx.ReadLocal("BepInEx/config/graphics.cfg"));

            // a referência fica só na pasta de referência
            Assert.Equal("reference-v1", fx.ReadLocal("BepInEx/config-server/graphics.cfg"));
        }
    }
}
