using System.Threading.Tasks;
using SPT.Launcher.Sync;
using Xunit;

namespace SPT.Launcher.Tests.Sync
{
    /// <summary>
    /// config-force → config (ForceToConfig): o canal deliberado de "essa config vai pra TODO MUNDO".
    /// Arquivos do server em 'config-force/&lt;rel&gt;' SOBRESCREVEM o 'config/&lt;rel&gt;' do usuário sempre que
    /// divergirem (ou faltarem) — **ignoram customização**, ao contrário do 'config' (preserve-divergent)
    /// e do 'config-server' (seed, só se ausente). A pasta config-force nunca aparece no cliente.
    /// </summary>
    public class SyncForceConfigTests
    {
        // === Mapeamento de destino (strip do sufixo "-force") ===

        [Theory]
        [InlineData("BepInEx/config-force/graphics.cfg", "bepinex/config-force", "BepInEx/config/graphics.cfg")]
        [InlineData("config-force/a/X.CFG", "config-force", "config/a/X.CFG")] // casing do remainder preservado
        public void DeriveTarget_strips_the_force_suffix(string source, string prefix, string expected)
        {
            Assert.Equal(expected, SyncPathUtil.DeriveSeedTarget(source, prefix));
        }

        [Fact]
        public void Fallback_maps_config_force_to_ForceToConfig()
        {
            var resolver = new SyncRuleResolver();
            Assert.Equal(SyncFolderRule.ForceToConfig, resolver.Resolve("BepInEx/config-force/x.cfg"));
            Assert.Equal(SyncFolderRule.ForceToConfig, resolver.Resolve("config-force/x.cfg"));
        }

        // === Comportamento (fixture, resolver default = fallback force-to-config) ===

        [Fact]
        public async Task Force_OVERWRITES_a_user_customized_config()
        {
            // O ponto da feature: mesmo o usuário tendo customizado, a config do server ganha.
            using var fx = new SyncTestFixture();
            fx.WriteLocal("BepInEx/config/graphics.cfg", "user-customized");
            var manifest = new[] { fx.Entry("BepInEx/config-force/graphics.cfg", "server-forced") };

            var (_, result, _) = await fx.PlanAndRunAsync(manifest);

            Assert.Equal(1, result.Forced);
            Assert.Equal("server-forced", fx.ReadLocal("BepInEx/config/graphics.cfg")); // sobrescrito
        }

        [Fact]
        public async Task Force_creates_the_config_when_absent()
        {
            using var fx = new SyncTestFixture();
            var manifest = new[] { fx.Entry("BepInEx/config-force/x.cfg", "forced") };

            var (_, result, _) = await fx.PlanAndRunAsync(manifest);

            Assert.Equal(1, result.Forced);
            Assert.Equal("forced", fx.ReadLocal("BepInEx/config/x.cfg"));
        }

        [Fact]
        public async Task Force_is_noop_when_the_config_already_matches()
        {
            using var fx = new SyncTestFixture();
            fx.WriteLocal("BepInEx/config/x.cfg", "forced"); // já igual ao do server
            var manifest = new[] { fx.Entry("BepInEx/config-force/x.cfg", "forced") };

            var (plan, result, _) = await fx.PlanAndRunAsync(manifest);

            Assert.Equal(0, result.Forced);
            Assert.Empty(plan.Actions);
            Assert.Empty(fx.DownloadedPaths); // nada a baixar
        }

        [Fact]
        public async Task Force_reapplies_whenever_the_user_edits_again()
        {
            // "Sempre a última versão do server": o force não tem memória — se o jogador mexer de novo,
            // o próximo sync devolve a config do server.
            using var fx = new SyncTestFixture();
            var manifest = new[] { fx.Entry("BepInEx/config-force/x.cfg", "forced") };

            var (_, first, _) = await fx.PlanAndRunAsync(manifest);
            Assert.Equal(1, first.Forced);

            fx.WriteLocal("BepInEx/config/x.cfg", "user-edited-again");

            var (_, second, _) = await fx.PlanAndRunAsync(manifest);
            Assert.Equal(1, second.Forced);
            Assert.Equal("forced", fx.ReadLocal("BepInEx/config/x.cfg")); // volta pro do server
        }

        [Fact]
        public async Task Force_source_folder_is_never_materialized_on_the_client()
        {
            using var fx = new SyncTestFixture();
            var manifest = new[] { fx.Entry("BepInEx/config-force/x.cfg", "forced") };

            var (_, result, _) = await fx.PlanAndRunAsync(manifest);

            Assert.Equal(1, result.Forced);
            Assert.True(fx.LocalExists("BepInEx/config/x.cfg"));         // destino: config
            Assert.False(fx.LocalExists("BepInEx/config-force/x.cfg"));  // fonte é só do server
        }

        [Fact]
        public async Task Force_preserves_subfolders()
        {
            using var fx = new SyncTestFixture();
            var manifest = new[] { fx.Entry("BepInEx/config-force/sub/deep/a.cfg", "v1") };

            var (_, result, _) = await fx.PlanAndRunAsync(manifest);

            Assert.Equal(1, result.Forced);
            Assert.Equal("v1", fx.ReadLocal("BepInEx/config/sub/deep/a.cfg"));
        }

        // === Guardas do review adversarial ===

        [Fact]
        public async Task Force_WINS_over_a_colliding_config_manifest_entry()
        {
            // Cenário real: o operador copia a cfg pro config-force/ e ESQUECE de tirar do config/.
            // Sem guard, as duas ações caem no mesmo alvo e o vencedor dependia da ORDEM do manifesto
            // (arbitrária) — podendo entregar justamente a config errada e envenenar o baseline.
            using var fx = new SyncTestFixture();
            var manifest = new[]
            {
                fx.Entry("BepInEx/config/x.cfg", "versao-do-config"),      // entrada concorrente
                fx.Entry("BepInEx/config-force/x.cfg", "versao-FORCADA"),  // a que deve valer
            };

            var (plan, result, _) = await fx.PlanAndRunAsync(manifest);

            Assert.Equal(1, result.Forced);
            Assert.Equal("versao-FORCADA", fx.ReadLocal("BepInEx/config/x.cfg")); // o force vence
            Assert.Equal(0, plan.DownloadCount);                                   // entrada config/ ignorada
            Assert.Contains(plan.Warnings, w => w.Contains("config-force"));
        }

        [Fact]
        public async Task DevMode_preserves_a_locally_edited_forced_config()
        {
            // R5.1: Dev Mode é o escape hatch "não reverta minha edição local" — nem o force sobrescreve.
            using var fx = new SyncTestFixture();
            fx.WriteLocal("BepInEx/config/x.cfg", "dev-local-edit");
            var manifest = new[] { fx.Entry("BepInEx/config-force/x.cfg", "forced") };

            var (plan, result, _) = await fx.PlanAndRunAsync(manifest, fx.Options(devMode: true));

            Assert.Equal(0, result.Forced);
            Assert.Equal(1, result.PreservedDevMode);
            Assert.Equal("dev-local-edit", fx.ReadLocal("BepInEx/config/x.cfg")); // preservado
            Assert.Contains(plan.Warnings, w => w.Contains("Dev Mode"));
        }

        [Fact]
        public async Task DevMode_still_creates_a_missing_forced_config()
        {
            // Dev Mode só protege o que JÁ EXISTE local; se falta, semeia normalmente.
            using var fx = new SyncTestFixture();
            var manifest = new[] { fx.Entry("BepInEx/config-force/x.cfg", "forced") };

            var (_, result, _) = await fx.PlanAndRunAsync(manifest, fx.Options(devMode: true));

            Assert.Equal(1, result.Forced);
            Assert.Equal("forced", fx.ReadLocal("BepInEx/config/x.cfg"));
        }

        [Fact]
        public async Task Misconfigured_force_without_the_force_suffix_is_ignored()
        {
            // Misconfig do operador: folderRules com "BepInEx/config": "force-to-config" (prefixo SEM
            // o sufixo "-force"). O alvo derivado seria o próprio arquivo → materializaria a fonte.
            using var fx = new SyncTestFixture();
            var resolver = new SyncRuleResolver(new System.Collections.Generic.Dictionary<string, string>
            {
                ["BepInEx/config"] = "force-to-config",
            });
            var manifest = new[] { fx.Entry("BepInEx/config/x.cfg", "v1") };

            var (plan, result, _) = await fx.PlanAndRunAsync(manifest, resolver: resolver);

            Assert.Equal(0, result.Forced);
            Assert.Empty(plan.Actions);
            Assert.Contains(plan.Warnings, w => w.Contains("-force"));
            Assert.False(fx.LocalExists("BepInEx/config/x.cfg")); // nada escrito
        }

        [Fact]
        public async Task Forced_config_is_never_deleted_as_an_extra()
        {
            // O arquivo forçado vive em 'config' (que NÃO é entrada do manifesto) — precisa sobreviver
            // a uma varredura de managedPaths.
            using var fx = new SyncTestFixture();
            var manifest = new[] { fx.Entry("BepInEx/config-force/x.cfg", "forced") };

            var options = fx.Options();
            options.ManagedPaths = new[] { "BepInEx" };

            var (plan, result, _) = await fx.PlanAndRunAsync(manifest, options);

            Assert.Equal(1, result.Forced);
            Assert.Equal(0, result.Deleted);
            Assert.DoesNotContain(plan.Actions, a => a.Kind == SyncActionKind.DeleteExtra);
            Assert.True(fx.LocalExists("BepInEx/config/x.cfg"));
        }
    }
}
