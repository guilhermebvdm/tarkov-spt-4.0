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

        // === Não-destrutivo: NADA é excluído em silêncio ===

        [Fact]
        public async Task Force_backs_up_the_player_config_to_config_disabled_before_overwriting()
        {
            using var fx = new SyncTestFixture();
            fx.WriteLocal("BepInEx/config/x.cfg", "a-config-do-jogador");
            var manifest = new[] { fx.Entry("BepInEx/config-force/x.cfg", "forced") };

            var (_, result, _) = await fx.PlanAndRunAsync(manifest);

            Assert.Equal(1, result.Forced);
            Assert.Equal("forced", fx.ReadLocal("BepInEx/config/x.cfg"));                        // substituída
            Assert.Equal("a-config-do-jogador", fx.ReadLocal("BepInEx/config-disabled/x.cfg")); // e PRESERVADA
        }

        [Fact]
        public async Task Force_preserves_subfolders_in_the_backup()
        {
            using var fx = new SyncTestFixture();
            fx.WriteLocal("BepInEx/config/sub/deep/x.cfg", "minha");
            var manifest = new[] { fx.Entry("BepInEx/config-force/sub/deep/x.cfg", "forced") };

            var (_, result, _) = await fx.PlanAndRunAsync(manifest);

            Assert.Equal(1, result.Forced);
            Assert.Equal("minha", fx.ReadLocal("BepInEx/config-disabled/sub/deep/x.cfg"));
        }

        [Fact]
        public async Task Force_does_not_create_a_backup_when_there_was_nothing_there()
        {
            using var fx = new SyncTestFixture();
            var manifest = new[] { fx.Entry("BepInEx/config-force/x.cfg", "forced") };

            var (_, result, _) = await fx.PlanAndRunAsync(manifest);

            Assert.Equal(1, result.Forced);
            Assert.False(fx.LocalExists("BepInEx/config-disabled/x.cfg")); // não havia o que preservar
        }

        [Fact]
        public async Task Backup_survives_the_next_syncs_and_is_never_deleted()
        {
            // Pastas "-disabled" nunca são re-sincronizadas nem deletadas (R3.4) — o backup fica lá.
            using var fx = new SyncTestFixture();
            fx.WriteLocal("BepInEx/config/x.cfg", "minha-config");
            var manifest = new[] { fx.Entry("BepInEx/config-force/x.cfg", "forced") };

            var options = fx.Options();
            options.ManagedPaths = new[] { "BepInEx" }; // varredura agressiva

            await fx.PlanAndRunAsync(manifest, options);
            Assert.Equal("minha-config", fx.ReadLocal("BepInEx/config-disabled/x.cfg"));

            // 2º sync: nada muda (config já forçada) e o backup continua lá
            var (plan2, result2, _) = await fx.PlanAndRunAsync(manifest, options);
            Assert.Equal(0, result2.Forced);
            Assert.Equal(0, result2.Deleted);
            Assert.DoesNotContain(plan2.Actions, a => a.Kind == SyncActionKind.DeleteExtra);
            Assert.Equal("minha-config", fx.ReadLocal("BepInEx/config-disabled/x.cfg"));
        }

        // === Achados do /code-review (CR-01..CR-05) — cenários que destruíam config em silêncio ===

        [Fact]
        public async Task CR01_TOCTOU_config_that_appears_between_plan_and_apply_is_still_backed_up()
        {
            // O planner viu o alvo AUSENTE; o arquivo SURGE antes do apply (BepInEx regerando o default,
            // o jogador restaurando um backup). Antes do fix: sobrescrevia sem backup nenhum.
            using var fx = new SyncTestFixture();
            var manifest = new[] { fx.Entry("BepInEx/config-force/x.cfg", "forced") };

            var baseline = fx.LoadBaseline();
            var plan = await new SyncPlanner(new SyncRuleResolver(), baseline, fx.Options()).BuildPlanAsync(manifest);

            // ...surge no meio do caminho
            fx.WriteLocal("BepInEx/config/x.cfg", "apareceu-no-meio");

            var result = await new SyncEngine(fx.Root, baseline, fx.Downloader).ExecuteAsync(plan, fx.ReportPath);

            Assert.Equal(1, result.Forced);
            Assert.Equal("forced", fx.ReadLocal("BepInEx/config/x.cfg"));
            Assert.Equal("apareceu-no-meio", fx.ReadLocal("BepInEx/config-disabled/x.cfg")); // NÃO sumiu
            Assert.Equal(1, result.ConfigsBackedUp);
        }

        [Fact]
        public async Task CR02_fail_safe_never_overwrites_an_existing_config_without_a_backup_target()
        {
            // Sem destino de backup + alvo existente → ABORTA (erro visível), config intacta.
            // Falhar alto > destruir em silêncio.
            using var fx = new SyncTestFixture();
            fx.WriteLocal("BepInEx/config/x.cfg", "a-config-do-jogador");
            fx.Entry("BepInEx/config-force/x.cfg", "forced"); // registra o conteúdo no "server"

            var baseline = fx.LoadBaseline();
            var plan = new SyncPlan();
            plan.Actions.Add(new SyncAction
            {
                RelativePath = "BepInEx/config-force/x.cfg",
                SeedTargetRelative = "BepInEx/config/x.cfg",
                MoveTargetRelative = null, // <-- sem backup derivável
                Kind = SyncActionKind.ForceCopy,
                Rule = SyncFolderRule.ForceToConfig,
            });

            var result = await new SyncEngine(fx.Root, baseline, fx.Downloader).ExecuteAsync(plan, fx.ReportPath);

            Assert.Equal(1, result.Errors);
            Assert.Equal(0, result.Forced);
            Assert.Equal("a-config-do-jogador", fx.ReadLocal("BepInEx/config/x.cfg")); // INTACTA
        }

        [Fact]
        public async Task CR03_second_force_does_not_clobber_the_first_backup()
        {
            // O jogador customiza, é forçado (backup v1), customiza DE NOVO, e o server publica outra
            // versão. Antes do fix: o 2º backup sobrescrevia o 1º e a customização ORIGINAL sumia.
            using var fx = new SyncTestFixture();
            fx.WriteLocal("BepInEx/config/x.cfg", "CUSTOM-ORIGINAL");

            var (_, first, _) = await fx.PlanAndRunAsync(new[] { fx.Entry("BepInEx/config-force/x.cfg", "server-v1") });
            Assert.Equal(1, first.Forced);
            Assert.Equal("CUSTOM-ORIGINAL", fx.ReadLocal("BepInEx/config-disabled/x.cfg"));

            fx.WriteLocal("BepInEx/config/x.cfg", "custom-v2");                    // customiza de novo
            var (_, second, _) = await fx.PlanAndRunAsync(new[] { fx.Entry("BepInEx/config-force/x.cfg", "server-v2") });

            Assert.Equal(1, second.Forced);
            Assert.Equal("server-v2", fx.ReadLocal("BepInEx/config/x.cfg"));
            Assert.Equal("CUSTOM-ORIGINAL", fx.ReadLocal("BepInEx/config-disabled/x.cfg"));   // preservada!
            Assert.Equal("custom-v2", fx.ReadLocal("BepInEx/config-disabled/x.cfg.1"));       // versionada
        }

        [Fact]
        public async Task CR03_identical_backup_is_reused_and_not_duplicated()
        {
            // Re-forçar o MESMO arquivo (ex.: manifesto stale) não deve encher config-disabled de .1/.2.
            using var fx = new SyncTestFixture();
            fx.WriteLocal("BepInEx/config/x.cfg", "minha");
            var manifest = new[] { fx.Entry("BepInEx/config-force/x.cfg", "forced") };

            await fx.PlanAndRunAsync(manifest);
            fx.WriteLocal("BepInEx/config/x.cfg", "minha"); // volta a divergir com o MESMO conteúdo
            await fx.PlanAndRunAsync(manifest);

            Assert.Equal("minha", fx.ReadLocal("BepInEx/config-disabled/x.cfg"));
            Assert.False(fx.LocalExists("BepInEx/config-disabled/x.cfg.1")); // não duplicou
        }

        [Fact]
        public async Task CR04_report_entry_says_where_the_backup_is()
        {
            using var fx = new SyncTestFixture();
            fx.WriteLocal("BepInEx/config/x.cfg", "minha");
            var manifest = new[] { fx.Entry("BepInEx/config-force/x.cfg", "forced") };

            var (_, result, _) = await fx.PlanAndRunAsync(manifest);

            var entry = Assert.Single(result.Entries, e => e.action == "forced");
            Assert.Contains("config-disabled", entry.detail);        // o jogador sabe ONDE está
            Assert.Contains("preservada", result.Summary);           // e o Summary (auto-apply) também diz
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
