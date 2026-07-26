using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SPT.Launcher.Models.Launcher;
using SPT.Launcher.Sync;
using Xunit;

namespace SPT.Launcher.Tests.Sync
{
    public class SyncPlannerTests
    {
        private const string ConfigPath = "BepInEx/config/mod.cfg";

        private static async Task<SyncPlan> PlanAsync(SyncTestFixture fx, IReadOnlyList<ManifestFile> manifest,
            SyncPlannerOptions options = null, SyncBaseline baseline = null, SyncRuleResolver resolver = null)
        {
            var planner = new SyncPlanner(resolver ?? new SyncRuleResolver(), baseline ?? fx.LoadBaseline(), options ?? fx.Options());
            return await planner.BuildPlanAsync(manifest);
        }

        // === R1 — PreserveDivergent (config) ===

        [Fact]
        public async Task Config_new_file_is_downloaded()
        {
            using var fx = new SyncTestFixture();
            var manifest = new[] { fx.Entry(ConfigPath, "server-v1") };

            var plan = await PlanAsync(fx, manifest);

            var action = Assert.Single(plan.Actions);
            Assert.Equal(SyncActionKind.Download, action.Kind);
            Assert.Equal(SyncFolderRule.PreserveDivergent, action.Rule);
        }

        [Fact]
        public async Task Config_equal_to_baseline_is_updated()
        {
            using var fx = new SyncTestFixture();
            fx.WriteLocal(ConfigPath, "old-content");

            var baseline = fx.LoadBaseline();
            baseline.SetHash(ConfigPath, SyncTestFixture.Md5Of("old-content"));

            var manifest = new[] { fx.Entry(ConfigPath, "server-v2") };
            var plan = await PlanAsync(fx, manifest, baseline: baseline);

            var action = Assert.Single(plan.Actions);
            Assert.Equal(SyncActionKind.Download, action.Kind); // R1.3
        }

        [Fact]
        public async Task Config_customized_is_preserved()
        {
            using var fx = new SyncTestFixture();
            fx.WriteLocal(ConfigPath, "user-tweaked");

            var baseline = fx.LoadBaseline();
            baseline.SetHash(ConfigPath, SyncTestFixture.Md5Of("old-content")); // user diverged from this

            var manifest = new[] { fx.Entry(ConfigPath, "server-v2") };
            var plan = await PlanAsync(fx, manifest, baseline: baseline);

            var action = Assert.Single(plan.Actions);
            Assert.Equal(SyncActionKind.PreserveCustomized, action.Kind); // R1.4
        }

        [Fact]
        public async Task Config_first_run_without_baseline_is_preserved()
        {
            using var fx = new SyncTestFixture();
            fx.WriteLocal(ConfigPath, "whatever-the-user-has");

            var manifest = new[] { fx.Entry(ConfigPath, "server-v2") };
            var plan = await PlanAsync(fx, manifest); // empty baseline = first run

            var action = Assert.Single(plan.Actions);
            Assert.Equal(SyncActionKind.PreserveCustomized, action.Kind); // R1.5 conservative
        }

        [Fact]
        public async Task Config_equal_to_server_seeds_baseline()
        {
            using var fx = new SyncTestFixture();
            fx.WriteLocal(ConfigPath, "same-everywhere");

            var manifest = new[] { fx.Entry(ConfigPath, "same-everywhere") };
            var plan = await PlanAsync(fx, manifest);

            Assert.Empty(plan.Actions);
            var seeded = Assert.Single(plan.UpToDate); // CC7
            Assert.Equal(SyncTestFixture.Md5Of("same-everywhere"), seeded.Value);
        }

        // === Extras — mirror rules and protections ===

        [Fact]
        public async Task ConfigServer_extra_is_planned_for_delete()
        {
            using var fx = new SyncTestFixture();
            fx.WriteLocal("config-server/stale.json", "old");

            var manifest = new[] { fx.Entry("config-server/keep.json", "keep") };
            // ref: CR-01-03 — mirror-delete só via folderRules explícito do server
            var plan = await PlanAsync(fx, manifest, resolver: SyncTestFixture.ResolverWithConfigServerMirror());

            Assert.Contains(plan.Actions, a =>
                a.Kind == SyncActionKind.DeleteExtra && a.RelativePath.Replace('\\', '/') == "config-server/stale.json");
        }

        [Fact]
        public async Task ConfigServer_extra_without_explicit_rule_is_untouched()
        {
            // ref: CR-01-03 — sem folderRules do server, config-server NÃO é espelho: extra sobrevive
            using var fx = new SyncTestFixture();
            fx.WriteLocal("config-server/stale.json", "old");

            var manifest = new[] { fx.Entry("config-server/keep.json", "keep") };
            var plan = await PlanAsync(fx, manifest); // fallback puro, sem regra explícita

            Assert.DoesNotContain(plan.Actions, a => a.Kind == SyncActionKind.DeleteExtra);
        }

        [Fact]
        public async Task Manifest_file_matching_ignored_substring_is_still_downloaded()
        {
            // ref: CR-01-06a / CR-01-02 — ignoredFiles protege extras de deleção, NUNCA bloqueia
            // update de arquivo do manifesto (senão o SPT core para de atualizar silenciosamente).
            using var fx = new SyncTestFixture();
            fx.WriteLocal("BepInEx/plugins/spt/spt-core.dll", "old-core");

            var options = fx.Options();
            options.IgnoredFiles = new[] { "BepInEx/plugins/spt" };

            var manifest = new[] { fx.Entry("BepInEx/plugins/spt/spt-core.dll", "new-core") };
            var plan = await PlanAsync(fx, manifest, options);

            var action = Assert.Single(plan.Actions);
            Assert.Equal(SyncActionKind.Download, action.Kind);
        }

        [Fact]
        public async Task Plugins_extra_is_planned_for_move_to_disabled()
        {
            using var fx = new SyncTestFixture();
            fx.WriteLocal("BepInEx/plugins/OldMod/old.dll", "bytes");

            var manifest = new[] { fx.Entry("BepInEx/plugins/Current/cur.dll", "cur") };
            var plan = await PlanAsync(fx, manifest);

            var move = Assert.Single(plan.Actions, a => a.Kind == SyncActionKind.MoveToDisabled);
            Assert.Equal("BepInEx/plugins-disabled/OldMod/old.dll", move.MoveTargetRelative);
        }

        [Fact]
        public async Task Disabled_optional_mod_files_are_quarantined_with_optional_origin()
        {
            using var fx = new SyncTestFixture();
            fx.WriteLocal("BepInEx/plugins/Gore/gore.dll", "gore-bytes");

            var manifest = new[]
            {
                fx.Entry("BepInEx/plugins/Current/cur.dll", "cur"),
                fx.Entry("BepInEx/plugins/Gore/gore.dll", "gore-bytes", optional: true, optionalId: "gore"),
            };

            // Item 030 (CA-030.8): mod "gore" DESLIGADO (default) + arquivo presente → quarentena
            // EXPLÍCITA sob a subpasta de origem "optional" (D-14). Nunca deletado como extra (o
            // ScanExtras não toca — manifestPaths protege optional inativo dele).
            var plan = await PlanAsync(fx, manifest);

            var move = Assert.Single(plan.Actions,
                a => a.Kind == SyncActionKind.MoveToDisabled && a.RelativePath.Contains("gore.dll"));
            Assert.Equal("BepInEx/plugins-disabled/optional/Gore/gore.dll", move.MoveTargetRelative);
            Assert.DoesNotContain(plan.Actions, a => a.Kind == SyncActionKind.DeleteExtra);
        }

        [Fact]
        public async Task Enabled_optional_mod_files_are_not_quarantined()
        {
            using var fx = new SyncTestFixture();
            fx.WriteLocal("BepInEx/plugins/Gore/gore.dll", "gore-bytes");

            var manifest = new[]
            {
                fx.Entry("BepInEx/plugins/Gore/gore.dll", "gore-bytes", optional: true, optionalId: "gore"),
            };

            // Mod "gore" LIGADO → o arquivo fica (up-to-date), nada é movido para quarentena.
            var plan = await PlanAsync(fx, manifest,
                fx.Options(optionalEnabled: id => id == "gore"));

            Assert.DoesNotContain(plan.Actions, a => a.Kind == SyncActionKind.MoveToDisabled);
        }

        [Fact]
        public async Task Ignored_and_excluded_extras_are_protected()
        {
            using var fx = new SyncTestFixture();
            fx.WriteLocal("config-server/ignored-dir/file.json", "x");
            fx.WriteLocal("config-server/user-thing.json", "y");

            var options = fx.Options();
            options.IgnoredFiles = new[] { "ignored-dir" };
            options.ExcludeFromCleanup = new[] { "config-server/user-thing.json" };

            var manifest = new[] { fx.Entry("config-server/keep.json", "keep") };
            // ref: CR-01-03 — regra de espelho ativada explicitamente p/ o teste de proteção ser real
            var plan = await PlanAsync(fx, manifest, options, resolver: SyncTestFixture.ResolverWithConfigServerMirror());

            Assert.DoesNotContain(plan.Actions, a => a.Kind == SyncActionKind.DeleteExtra); // R2.3
        }

        [Fact]
        public async Task Disabled_folders_are_never_rescanned()
        {
            using var fx = new SyncTestFixture();
            fx.WriteLocal("BepInEx/plugins-disabled/OldMod/old.dll", "quarantined");

            var manifest = new[] { fx.Entry("BepInEx/plugins/Current/cur.dll", "cur") };
            var options = fx.Options();
            options.ManagedPaths = new[] { "BepInEx" }; // even when a managed path covers it

            var plan = await PlanAsync(fx, manifest, options);

            Assert.DoesNotContain(plan.Actions, a => a.RelativePath.Contains("plugins-disabled")); // R3.4
        }

        [Fact]
        public async Task Managed_path_default_extra_is_deleted_but_config_extra_is_not()
        {
            using var fx = new SyncTestFixture();
            fx.WriteLocal("user/mods/leftover/mod.js", "junk");
            fx.WriteLocal("BepInEx/config/user-added.cfg", "mine");

            var options = fx.Options();
            options.ManagedPaths = new[] { "user/mods", "BepInEx/config" };

            var manifest = new[] { fx.Entry("user/mods/current/mod.js", "ok") };
            var plan = await PlanAsync(fx, manifest, options);

            Assert.Contains(plan.Actions, a =>
                a.Kind == SyncActionKind.DeleteExtra && a.RelativePath.Replace('\\', '/') == "user/mods/leftover/mod.js"); // R4.2
            Assert.DoesNotContain(plan.Actions, a => a.RelativePath.Contains("user-added.cfg")); // R1: config is not a mirror
        }

        // === R5 — Dev Mode ===

        [Fact]
        public async Task DevMode_preserves_locally_modified_plugin()
        {
            using var fx = new SyncTestFixture();
            fx.WriteLocal("BepInEx/plugins/MyMod/my.dll", "local-dev-build");

            var manifest = new[] { fx.Entry("BepInEx/plugins/MyMod/my.dll", "server-build") };
            var plan = await PlanAsync(fx, manifest, fx.Options(devMode: true));

            var action = Assert.Single(plan.Actions);
            Assert.Equal(SyncActionKind.PreserveDevMode, action.Kind); // R5.1
            Assert.NotEmpty(plan.Warnings);
        }

        [Fact]
        public async Task DevMode_off_downloads_locally_modified_plugin()
        {
            using var fx = new SyncTestFixture();
            fx.WriteLocal("BepInEx/plugins/MyMod/my.dll", "local-dev-build");

            var manifest = new[] { fx.Entry("BepInEx/plugins/MyMod/my.dll", "server-build") };
            var plan = await PlanAsync(fx, manifest, fx.Options(devMode: false));

            var action = Assert.Single(plan.Actions);
            Assert.Equal(SyncActionKind.Download, action.Kind); // R5.3
        }

        [Fact]
        public async Task DevMode_preserves_extras_in_mirrored_folders()
        {
            using var fx = new SyncTestFixture();
            fx.WriteLocal("BepInEx/plugins/WipMod/wip.dll", "work-in-progress");

            var manifest = new[] { fx.Entry("BepInEx/plugins/Current/cur.dll", "cur") };
            var plan = await PlanAsync(fx, manifest, fx.Options(devMode: true));

            Assert.DoesNotContain(plan.Actions, a => a.Kind == SyncActionKind.MoveToDisabled);
            Assert.Contains(plan.Actions, a =>
                a.Kind == SyncActionKind.PreserveDevMode && a.RelativePath.Contains("wip.dll")); // R5.2
        }

        [Fact]
        public async Task DevMode_preserves_extras_in_mirror_delete_folders()
        {
            // ref: CR-01-06f — R5.2 também em MirrorDelete (só plugins/move tinha cobertura)
            using var fx = new SyncTestFixture();
            fx.WriteLocal("config-server/wip-config.json", "work-in-progress");

            var manifest = new[] { fx.Entry("config-server/keep.json", "keep") };
            var plan = await PlanAsync(fx, manifest, fx.Options(devMode: true),
                resolver: SyncTestFixture.ResolverWithConfigServerMirror());

            Assert.DoesNotContain(plan.Actions, a => a.Kind == SyncActionKind.DeleteExtra);
            Assert.Contains(plan.Actions, a =>
                a.Kind == SyncActionKind.PreserveDevMode && a.RelativePath.Contains("wip-config.json"));
        }

        // === Item 023 (Frente A) — coop-safe allowlist (Fika) ===

        [Fact]
        public async Task Fika_plugin_extra_is_never_quarantined()
        {
            // CA-A1/CA-A2: Fika.*.dll local under plugins, absent from the manifest → preserved (no
            // MoveToDisabled touches it) + a "coop-safe" warning (RN-3, never silent).
            using var fx = new SyncTestFixture();
            fx.WriteLocal("BepInEx/plugins/Fika.Core.dll", "fika-bytes");

            var manifest = new[] { fx.Entry("BepInEx/plugins/Current/cur.dll", "cur") };
            var plan = await PlanAsync(fx, manifest);

            Assert.DoesNotContain(plan.Actions, a => a.RelativePath.Contains("Fika.Core.dll"));
            Assert.Contains(plan.Warnings, w => w.Contains("coop-safe") && w.Contains("Fika.Core.dll"));
        }

        [Fact]
        public async Task NonFika_plugin_extra_still_quarantined()
        {
            // CA-A3 regression: the allowlist must NOT loosen cleanup of unrelated extras.
            using var fx = new SyncTestFixture();
            fx.WriteLocal("BepInEx/plugins/OldMod/old.dll", "bytes");

            var manifest = new[] { fx.Entry("BepInEx/plugins/Current/cur.dll", "cur") };
            var plan = await PlanAsync(fx, manifest);

            var move = Assert.Single(plan.Actions, a => a.Kind == SyncActionKind.MoveToDisabled);
            Assert.Equal("BepInEx/plugins-disabled/OldMod/old.dll", move.MoveTargetRelative);
        }

        [Fact]
        public async Task Fika_in_manifest_is_downloaded_not_preserved()
        {
            // CA-A4/RN-2: Fika present in the manifest with a divergent hash → normal Download; the
            // allowlist never blocks a server-driven update and emits no coop-safe warning.
            using var fx = new SyncTestFixture();
            fx.WriteLocal("BepInEx/plugins/Fika.Core.dll", "old-fika");

            var manifest = new[] { fx.Entry("BepInEx/plugins/Fika.Core.dll", "new-fika") };
            var plan = await PlanAsync(fx, manifest);

            var action = Assert.Single(plan.Actions);
            Assert.Equal(SyncActionKind.Download, action.Kind);
            Assert.DoesNotContain(plan.Warnings, w => w.Contains("coop-safe"));
        }

        [Fact]
        public async Task DevMode_preserves_fika_without_coopsafe_warning_conflict()
        {
            // CA-A5: Dev Mode already preserves any extra (as PreserveDevMode); the coop-safe skip sits
            // AFTER the Dev Mode block, so Fika is preserved with the Dev Mode warning and NOT a
            // duplicate coop-safe action/warning.
            using var fx = new SyncTestFixture();
            fx.WriteLocal("BepInEx/plugins/Fika.Core.dll", "local-fika");

            var manifest = new[] { fx.Entry("BepInEx/plugins/Current/cur.dll", "cur") };
            var plan = await PlanAsync(fx, manifest, fx.Options(devMode: true));

            Assert.Contains(plan.Actions, a =>
                a.Kind == SyncActionKind.PreserveDevMode && a.RelativePath.Contains("Fika.Core.dll"));
            Assert.DoesNotContain(plan.Actions, a => a.Kind == SyncActionKind.MoveToDisabled);
            Assert.DoesNotContain(plan.Warnings, w => w.Contains("coop-safe"));
        }
    }
}
