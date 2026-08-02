using System.Collections.Generic;
using System.Threading.Tasks;
using SPT.Launcher.Models.Launcher;
using SPT.Launcher.Sync;
using Xunit;

namespace SPT.Launcher.Tests.Sync
{
    /// <summary>
    /// Item 030 — canal config-optional (OptionalConfigToConfig). Cobre o eixo ligar/desligar, a
    /// precedência performance &gt; force &gt; config, a convergência (baseline) e o namespace da quarentena.
    /// O resolver padrão já resolve config-optional via FallbackRules (é o que o cliente usa em prod).
    /// </summary>
    public class SyncOptionalConfigChannelTests
    {
        private const string PerfSource = "BepInEx/config-optional/x.cfg";
        private const string PerfRef = "BepInEx/config-optional-ref/x.cfg";
        private const string Target = "BepInEx/config/x.cfg";
        private const string ConfigSource = "BepInEx/config/x.cfg";
        private const string ForceSource = "BepInEx/config-force/x.cfg";

        private static async Task<SyncPlan> PlanAsync(SyncTestFixture fx, IReadOnlyList<ManifestFile> manifest, SyncPlannerOptions options)
        {
            var planner = new SyncPlanner(new SyncRuleResolver(), fx.LoadBaseline(), options);
            return await planner.BuildPlanAsync(manifest);
        }

        // CA-030.1 / CA-030.2 — ligar (ação explícita) aplica MESMO sobre config customizada, preservando
        // a anterior em config-disabled/optional-config/replaced/ (D-16 / D-20).
        [Fact]
        public async Task Enabling_applies_over_existing_config_and_backs_it_up_as_replaced()
        {
            using var fx = new SyncTestFixture();
            fx.WriteLocal(Target, "minha-config");

            var manifest = new[] { fx.Entry(PerfSource, "perf-config", optionalConfigId: "perf1") };
            var options = fx.Options(performanceEnabled: id => id == "perf1", justToggled: new[] { "perf1" });

            var plan = await PlanAsync(fx, manifest, options);

            var apply = Assert.Single(plan.Actions, a => a.Kind == SyncActionKind.OptionalConfigCopy);
            Assert.Equal(PerfSource, apply.RelativePath);
            Assert.Equal(Target, apply.SeedTargetRelative);
            Assert.Equal("BepInEx/config-disabled/optional-config/replaced/x.cfg", apply.MoveTargetRelative);
        }

        // CA-030.3 — sync de ROTINA (não recém-alternado) respeita a customização do player.
        [Fact]
        public async Task Routine_sync_preserves_customized_config()
        {
            using var fx = new SyncTestFixture();
            fx.WriteLocal(Target, "customizei-depois"); // sem baseline == tratado como customizado (R1.5)

            var manifest = new[] { fx.Entry(PerfSource, "perf-config", optionalConfigId: "perf1") };
            var options = fx.Options(performanceEnabled: id => id == "perf1"); // ligado, mas NÃO justToggled

            var plan = await PlanAsync(fx, manifest, options);

            Assert.DoesNotContain(plan.Actions, a => a.Kind == SyncActionKind.OptionalConfigCopy);
            Assert.Contains(plan.Actions, a => a.Kind == SyncActionKind.PreserveCustomized && a.RelativePath == Target);
        }

        // CA-030.4 + convergência (PA-02-04) — aplica no 1º sync, grava baseline, e o 2º sync não faz I/O.
        [Fact]
        public async Task Applies_then_converges_second_sync_has_no_io()
        {
            using var fx = new SyncTestFixture();
            var manifest = new[] { fx.Entry(PerfSource, "perf-config", optionalConfigId: "perf1") };

            // 1º sync: ligado + recém-alternado → aplica de fato (engine grava o arquivo E o baseline).
            var run1 = await fx.PlanAndRunAsync(manifest,
                fx.Options(performanceEnabled: id => id == "perf1", justToggled: new[] { "perf1" }));
            Assert.Equal(1, run1.Plan.OptionalConfigCount);
            Assert.Equal("perf-config", fx.ReadLocal(Target));
            Assert.Equal(1, run1.Result.OptionalConfigApplied);

            // 2º sync: ligado, NÃO alternado, arquivo == servidor → no-op (sem isto o híbrido não convergiria).
            var run2 = await fx.PlanAndRunAsync(manifest,
                fx.Options(performanceEnabled: id => id == "perf1"));
            Assert.Equal(0, run2.Plan.IoActionCount);
        }

        // CA-030.5 / D-8 / D-20 — desligar um arquivo SÓ-de-performance (sem versão base) → quarentena
        // em config-disabled/optional-config/removed/.
        [Fact]
        public async Task Disabling_performance_only_file_quarantines_as_removed()
        {
            using var fx = new SyncTestFixture();
            var manifest = new[] { fx.Entry(PerfSource, "perf-config", optionalConfigId: "perf1") };

            // liga e aplica (cria baseline p/ o alvo)…
            await fx.PlanAndRunAsync(manifest,
                fx.Options(performanceEnabled: id => id == "perf1", justToggled: new[] { "perf1" }));

            // …depois desliga: alvo intocado + sem base em config/ → quarentena "removed".
            var off = await fx.PlanAndRunAsync(manifest, fx.Options(performanceEnabled: _ => false));

            var move = Assert.Single(off.Plan.Actions, a => a.Kind == SyncActionKind.MoveToDisabled);
            Assert.Equal("BepInEx/config-disabled/optional-config/removed/x.cfg", move.MoveTargetRelative);
            Assert.False(fx.LocalExists(Target)); // saiu de config/
        }

        // CA-030.5 (outra ponta) — desligar quando HÁ versão base (config/) não quarentena; o fluxo normal
        // restaura a base na mesma passada.
        [Fact]
        public async Task Disabling_with_base_version_restores_instead_of_quarantining()
        {
            using var fx = new SyncTestFixture();
            var manifest = new[]
            {
                fx.Entry(ConfigSource, "base-v1"),                                   // canal config (base)
                fx.Entry(PerfSource, "perf-config", optionalConfigId: "perf1"),
            };

            await fx.PlanAndRunAsync(manifest,
                fx.Options(performanceEnabled: id => id == "perf1", justToggled: new[] { "perf1" }));

            var off = await fx.PlanAndRunAsync(manifest, fx.Options(performanceEnabled: _ => false));

            Assert.DoesNotContain(off.Plan.Actions, a => a.Kind == SyncActionKind.MoveToDisabled);
            Assert.Equal("base-v1", fx.ReadLocal(Target)); // base restaurada pelo canal config
        }

        // 🟡 CR — desligar performance quando HÁ base customizada não emite PreserveCustomized espúrio pelo
        // canal de performance (a base config/config-force é quem decide o alvo nesta passada).
        [Fact]
        public async Task Disabling_with_customized_base_emits_no_performance_preserve()
        {
            using var fx = new SyncTestFixture();
            var manifest = new[]
            {
                fx.Entry(ConfigSource, "base-v1"),
                fx.Entry(PerfSource, "perf-config", optionalConfigId: "perf1"),
            };

            await fx.PlanAndRunAsync(manifest,
                fx.Options(performanceEnabled: id => id == "perf1", justToggled: new[] { "perf1" }));
            fx.WriteLocal(Target, "eu-mexi-depois"); // customiza o alvo à mão

            var plan = await PlanAsync(fx, manifest, fx.Options(performanceEnabled: _ => false));

            Assert.DoesNotContain(plan.Actions,
                a => a.Rule == SyncFolderRule.OptionalConfigToConfig && a.Kind == SyncActionKind.PreserveCustomized);
        }

        // 🟡 CR-01-02 — desligar performance quando a base é config-FORCE e o alvo foi customizado: o
        // canal force RECLAMA o alvo (ForceCopy, com backup em config-disabled/), comportamento
        // INTENCIONAL (não-destrutivo + coop-correto: a config forçada volta a valer). O canal de
        // performance NÃO emite PreserveCustomized — a base decide o alvo nesta passada.
        [Fact]
        public async Task Disabling_with_customized_force_base_lets_force_reclaim_the_target()
        {
            using var fx = new SyncTestFixture();
            var manifest = new[]
            {
                fx.Entry(ForceSource, "forcada"),
                fx.Entry(PerfSource, "perf-config", optionalConfigId: "perf1"),
            };

            // liga a performance e aplica (o force fica suprimido nesta fase)…
            await fx.PlanAndRunAsync(manifest,
                fx.Options(performanceEnabled: id => id == "perf1", justToggled: new[] { "perf1" }));
            fx.WriteLocal(Target, "eu-mexi-depois"); // customiza o alvo à mão

            // …depois desliga a performance.
            var plan = await PlanAsync(fx, manifest, fx.Options(performanceEnabled: _ => false));

            Assert.DoesNotContain(plan.Actions,
                a => a.Rule == SyncFolderRule.OptionalConfigToConfig && a.Kind == SyncActionKind.PreserveCustomized);
            Assert.Contains(plan.Actions, a => a.Kind == SyncActionKind.ForceCopy);
        }

        // CA-030.1 / RN-2 — performance LIGADA vence config-force do mesmo alvo; o force é suprimido e o
        // relatório registra optional-config-suppressed-force.
        [Fact]
        public async Task Enabled_performance_suppresses_force_and_reports_it()
        {
            using var fx = new SyncTestFixture();
            fx.WriteLocal(Target, "antiga");

            var manifest = new[]
            {
                fx.Entry(ForceSource, "forcada"),
                fx.Entry(PerfSource, "perf-config", optionalConfigId: "perf1"),
            };
            var options = fx.Options(performanceEnabled: id => id == "perf1", justToggled: new[] { "perf1" });

            var plan = await PlanAsync(fx, manifest, options);

            Assert.DoesNotContain(plan.Actions, a => a.Kind == SyncActionKind.ForceCopy);
            Assert.Single(plan.Actions, a => a.Kind == SyncActionKind.OptionalConfigCopy);
            Assert.Contains(plan.InfoEntries, e => e.action == "optional-config-suppressed-force");
        }

        // D-18 — a pasta-espelho config-optional-ref é biblioteca de referência (MirrorReference):
        // baixa para o cliente, nunca deriva alvo em config/.
        [Fact]
        public async Task Performance_ref_mirror_downloads_to_client()
        {
            using var fx = new SyncTestFixture();
            var manifest = new[] { fx.Entry(PerfRef, "perf-config") };

            var run = await fx.PlanAndRunAsync(manifest, fx.Options());

            Assert.Contains(run.Result.Entries, e => e.action == "reference-updated");
            Assert.True(fx.LocalExists(PerfRef));           // espelhado na máquina do player
            Assert.False(fx.LocalExists(Target));           // nunca escreve em config/
        }
    }
}
