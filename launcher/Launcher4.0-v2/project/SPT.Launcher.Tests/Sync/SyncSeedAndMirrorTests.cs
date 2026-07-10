using System.Linq;
using System.Threading.Tasks;
using SPT.Launcher.Sync;
using Xunit;

namespace SPT.Launcher.Tests.Sync
{
    /// <summary>
    /// config-server DUAL (seed-and-mirror, o default atual do fallback). O MESMO arquivo do server
    /// alimenta dois destinos no cliente ao mesmo tempo:
    ///   1. SEED em 'config/&lt;rel&gt;' — só se ausente (nunca sobrescreve/deleta as configs vivas).
    ///   2. MIRROR em 'config-server/&lt;rel&gt;' — réplica: baixa a última versão sempre (sobrescreve
    ///      edições), mas NÃO deleta extras (conservador, ref CR-01-03).
    /// </summary>
    public class SyncSeedAndMirrorTests
    {
        [Fact]
        public async Task Seeds_config_and_mirrors_config_server_on_first_sync()
        {
            using var fx = new SyncTestFixture();
            var manifest = new[] { fx.Entry("BepInEx/config-server/graphics.cfg", "server-v1") };

            // default resolver = seed-and-mirror para config-server
            var (_, result, _) = await fx.PlanAndRunAsync(manifest);

            // SEED -> config (estava ausente)
            Assert.Equal(1, result.Seeded);
            Assert.Equal("server-v1", fx.ReadLocal("BepInEx/config/graphics.cfg"));

            // MIRROR -> config-server (materializado no disco, cópia exata)
            Assert.Equal(1, result.Updated);
            Assert.Equal("server-v1", fx.ReadLocal("BepInEx/config-server/graphics.cfg"));
        }

        [Fact]
        public async Task Mirror_runs_even_when_config_already_present()
        {
            using var fx = new SyncTestFixture();
            fx.WriteLocal("BepInEx/config/graphics.cfg", "user-customized");
            var manifest = new[] { fx.Entry("BepInEx/config-server/graphics.cfg", "server-v1") };

            var (_, result, _) = await fx.PlanAndRunAsync(manifest);

            Assert.Equal(0, result.Seeded);                                        // config presente -> seed no-op
            Assert.Equal("user-customized", fx.ReadLocal("BepInEx/config/graphics.cfg")); // nunca sobrescrito
            Assert.Equal(1, result.Updated);                                       // mirror mesmo assim
            Assert.Equal("server-v1", fx.ReadLocal("BepInEx/config-server/graphics.cfg"));
        }

        [Fact]
        public async Task Mirror_overwrites_user_edited_config_server_with_latest()
        {
            using var fx = new SyncTestFixture();
            fx.WriteLocal("BepInEx/config-server/graphics.cfg", "user-edited-the-reference"); // difere do server
            fx.WriteLocal("BepInEx/config/graphics.cfg", "whatever");                          // config presente
            var manifest = new[] { fx.Entry("BepInEx/config-server/graphics.cfg", "server-v2") };

            var (_, result, _) = await fx.PlanAndRunAsync(manifest);

            Assert.Equal(1, result.Updated);                                       // réplica exata: sobrescreve
            Assert.Equal("server-v2", fx.ReadLocal("BepInEx/config-server/graphics.cfg"));
            Assert.Equal(0, result.Seeded);
        }

        [Fact]
        public async Task Mirror_is_noop_when_config_server_already_matches()
        {
            using var fx = new SyncTestFixture();
            fx.WriteLocal("BepInEx/config-server/graphics.cfg", "server-v1"); // == server
            fx.WriteLocal("BepInEx/config/graphics.cfg", "x");               // config presente
            var manifest = new[] { fx.Entry("BepInEx/config-server/graphics.cfg", "server-v1") };

            var (_, result, _) = await fx.PlanAndRunAsync(manifest);

            Assert.Equal(0, result.Updated);
            Assert.Equal(0, result.Seeded);
            Assert.Empty(fx.DownloadedPaths); // nada a baixar: hash bate e config já existe
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
    }
}
