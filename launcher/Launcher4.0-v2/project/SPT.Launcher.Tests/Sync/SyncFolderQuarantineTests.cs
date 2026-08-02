using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SPT.Launcher.Models.Launcher;
using SPT.Launcher.Sync;
using Xunit;

namespace SPT.Launcher.Tests.Sync
{
    /// <summary>
    /// Item 034 — a quarentena move a PASTA do mod inteira (Directory.Move) em vez de arquivo a arquivo,
    /// sem deixar casca vazia na origem, com fallback per-file quando há arquivo protegido, e uma faxina
    /// de pastas vazias no fim do sync. Cobre CA-034.1..7 e CC-1/3/4/5/9.
    /// </summary>
    public class SyncFolderQuarantineTests
    {
        // CA-034.1 — mod opcional desligado que vive em pasta → 1 move de pasta, origem sem casca.
        [Fact]
        public async Task Disabled_optional_folder_mod_moves_whole_folder_without_empty_shell()
        {
            using var fx = new SyncTestFixture();
            fx.WriteLocal("BepInEx/plugins/PiP-Disabler/PiP-Disabler.dll", "dll");
            fx.WriteLocal("BepInEx/plugins/PiP-Disabler/reticle.bundle", "bundle");

            var manifest = new[]
            {
                fx.Entry("BepInEx/plugins/PiP-Disabler/PiP-Disabler.dll", "dll", optional: true, optionalId: "pip"),
                fx.Entry("BepInEx/plugins/PiP-Disabler/reticle.bundle", "bundle", optional: true, optionalId: "pip"),
            };

            var (plan, _, _) = await fx.PlanAndRunAsync(manifest, fx.Options(optionalEnabled: _ => false));

            Assert.Equal(1, plan.MoveDirCount);
            Assert.Equal(0, plan.MoveCount);
            Assert.False(Directory.Exists(fx.LocalPath("BepInEx/plugins/PiP-Disabler")));
            Assert.True(fx.LocalExists("BepInEx/plugins-disabled/optional/PiP-Disabler/PiP-Disabler.dll"));
            Assert.True(fx.LocalExists("BepInEx/plugins-disabled/optional/PiP-Disabler/reticle.bundle"));
        }

        // CA-034.2 / CA-034.6 — extra fora do manifesto que vive em pasta (patchers) → pasta inteira.
        [Fact]
        public async Task Extra_folder_in_patchers_moves_whole_folder()
        {
            using var fx = new SyncTestFixture();
            fx.WriteLocal("BepInEx/patchers/WTT-Backport/patch.dll", "p1");
            fx.WriteLocal("BepInEx/patchers/WTT-Backport/data.json", "d1");

            var (plan, _, _) = await fx.PlanAndRunAsync(Array.Empty<ManifestFile>());

            Assert.Equal(1, plan.MoveDirCount);
            Assert.False(Directory.Exists(fx.LocalPath("BepInEx/patchers/WTT-Backport")));
            Assert.True(fx.LocalExists("BepInEx/patchers-disabled/WTT-Backport/patch.dll"));
            Assert.True(fx.LocalExists("BepInEx/patchers-disabled/WTT-Backport/data.json"));
        }

        // CA-034.3 — mod que é .dll solto no root → move de arquivo, nenhuma pasta é movida.
        [Fact]
        public async Task Disabled_optional_loose_dll_moves_as_file_not_folder()
        {
            using var fx = new SyncTestFixture();
            fx.WriteLocal("BepInEx/plugins/Solo.dll", "x");

            var manifest = new[] { fx.Entry("BepInEx/plugins/Solo.dll", "x", optional: true, optionalId: "solo") };
            var (plan, _, _) = await fx.PlanAndRunAsync(manifest, fx.Options(optionalEnabled: _ => false));

            Assert.Equal(0, plan.MoveDirCount);
            Assert.Equal(1, plan.MoveCount);
            Assert.True(fx.LocalExists("BepInEx/plugins-disabled/optional/Solo.dll"));
            Assert.True(Directory.Exists(fx.LocalPath("BepInEx/plugins")));
        }

        // CA-034.4 — pasta com um plugin coop-essencial (Fika.*.dll) → fallback per-file, protegido fica.
        [Fact]
        public async Task Coop_essential_in_folder_falls_back_to_per_file_and_preserves()
        {
            using var fx = new SyncTestFixture();
            fx.WriteLocal("BepInEx/plugins/BundleMod/BundleMod.dll", "b");
            fx.WriteLocal("BepInEx/plugins/BundleMod/Fika.Core.dll", "f");

            var manifest = new[]
            {
                fx.Entry("BepInEx/plugins/BundleMod/BundleMod.dll", "b", optional: true, optionalId: "bundle"),
                fx.Entry("BepInEx/plugins/BundleMod/Fika.Core.dll", "f", optional: true, optionalId: "bundle"),
            };
            var (plan, _, _) = await fx.PlanAndRunAsync(manifest, fx.Options(optionalEnabled: _ => false));

            Assert.Equal(0, plan.MoveDirCount);
            Assert.Equal(1, plan.MoveCount);                                            // caiu para per-file
            Assert.True(fx.LocalExists("BepInEx/plugins/BundleMod/Fika.Core.dll"));      // coop-safe preservado
            Assert.True(fx.LocalExists("BepInEx/plugins-disabled/optional/BundleMod/BundleMod.dll"));
        }

        // CC-1 — pasta compartilhada por um mod desligado e um arquivo mandatório → fallback per-file.
        [Fact]
        public async Task Shared_folder_with_mandatory_file_falls_back_to_per_file()
        {
            using var fx = new SyncTestFixture();
            fx.WriteLocal("BepInEx/plugins/Shared/off.dll", "off");
            fx.WriteLocal("BepInEx/plugins/Shared/on.dll", "on");

            var manifest = new[]
            {
                fx.Entry("BepInEx/plugins/Shared/off.dll", "off", optional: true, optionalId: "off"),
                fx.Entry("BepInEx/plugins/Shared/on.dll", "on"), // mandatório (fica)
            };
            var (plan, _, _) = await fx.PlanAndRunAsync(manifest, fx.Options(optionalEnabled: _ => false));

            Assert.Equal(0, plan.MoveDirCount);
            Assert.True(fx.LocalExists("BepInEx/plugins/Shared/on.dll"));
            Assert.True(fx.LocalExists("BepInEx/plugins-disabled/optional/Shared/off.dll"));
        }

        // CC-9 — mod opcional com paths misto (pasta + .dll solto): a pasta consolida, o .dll vai per-file.
        [Fact]
        public async Task Mixed_paths_folder_and_loose_dll_both_quarantined()
        {
            using var fx = new SyncTestFixture();
            fx.WriteLocal("BepInEx/plugins/DragonDen/DragonDen.dll", "d");
            fx.WriteLocal("BepInEx/plugins/Drexira.DragonDen.dll", "loose");

            var manifest = new[]
            {
                fx.Entry("BepInEx/plugins/DragonDen/DragonDen.dll", "d", optional: true, optionalId: "dragon"),
                fx.Entry("BepInEx/plugins/Drexira.DragonDen.dll", "loose", optional: true, optionalId: "dragon"),
            };
            var (plan, _, _) = await fx.PlanAndRunAsync(manifest, fx.Options(optionalEnabled: _ => false));

            Assert.Equal(1, plan.MoveDirCount);
            Assert.Equal(1, plan.MoveCount);
            Assert.False(Directory.Exists(fx.LocalPath("BepInEx/plugins/DragonDen")));
            Assert.True(fx.LocalExists("BepInEx/plugins-disabled/optional/DragonDen/DragonDen.dll"));
            Assert.True(fx.LocalExists("BepInEx/plugins-disabled/optional/Drexira.DragonDen.dll"));
        }

        // CA-034.5 — faxina remove a casca vazia (recursiva) e preserva o root e mods ligados.
        [Fact]
        public async Task Cleanup_removes_empty_shell_recursively_keeps_root()
        {
            using var fx = new SyncTestFixture();
            Directory.CreateDirectory(fx.LocalPath("BepInEx/plugins/GhostMod/Nested")); // casca vazia aninhada
            fx.WriteLocal("BepInEx/plugins/Keep/keep.dll", "k");

            var manifest = new[] { fx.Entry("BepInEx/plugins/Keep/keep.dll", "k") };
            await fx.PlanAndRunAsync(manifest);

            Assert.False(Directory.Exists(fx.LocalPath("BepInEx/plugins/GhostMod")));
            Assert.True(Directory.Exists(fx.LocalPath("BepInEx/plugins")));
            Assert.True(fx.LocalExists("BepInEx/plugins/Keep/keep.dll"));
        }

        // CC-3 — em Dev Mode não move nem faxina.
        [Fact]
        public async Task DevMode_skips_move_and_cleanup()
        {
            using var fx = new SyncTestFixture();
            fx.WriteLocal("BepInEx/plugins/PiP-Disabler/PiP-Disabler.dll", "dll");
            Directory.CreateDirectory(fx.LocalPath("BepInEx/plugins/GhostMod")); // casca vazia

            var manifest = new[]
            {
                fx.Entry("BepInEx/plugins/PiP-Disabler/PiP-Disabler.dll", "dll", optional: true, optionalId: "pip"),
            };
            var (plan, _, _) = await fx.PlanAndRunAsync(manifest, fx.Options(devMode: true, optionalEnabled: _ => false));

            Assert.Equal(0, plan.MoveDirCount);
            Assert.Empty(plan.EmptyDirCleanupRoots);
            Assert.True(Directory.Exists(fx.LocalPath("BepInEx/plugins/PiP-Disabler")));
            Assert.True(Directory.Exists(fx.LocalPath("BepInEx/plugins/GhostMod")));
        }

        // CC-4 — destino de quarentena já existe → mescla sobrescrevendo homônimos e remove a origem.
        [Fact]
        public async Task Merge_into_existing_quarantine_overwrites_and_removes_source()
        {
            using var fx = new SyncTestFixture();
            fx.WriteLocal("BepInEx/plugins-disabled/optional/PiP-Disabler/old.dll", "OLD");
            fx.WriteLocal("BepInEx/plugins-disabled/optional/PiP-Disabler/PiP-Disabler.dll", "V1");
            fx.WriteLocal("BepInEx/plugins/PiP-Disabler/PiP-Disabler.dll", "V2");

            var manifest = new[]
            {
                fx.Entry("BepInEx/plugins/PiP-Disabler/PiP-Disabler.dll", "V2", optional: true, optionalId: "pip"),
            };
            await fx.PlanAndRunAsync(manifest, fx.Options(optionalEnabled: _ => false));

            Assert.False(Directory.Exists(fx.LocalPath("BepInEx/plugins/PiP-Disabler")));
            Assert.Equal("V2", fx.ReadLocal("BepInEx/plugins-disabled/optional/PiP-Disabler/PiP-Disabler.dll"));
            Assert.Equal("OLD", fx.ReadLocal("BepInEx/plugins-disabled/optional/PiP-Disabler/old.dll"));
        }

        // CC-5 — segundo sync é idempotente (nada a mover, sem erro).
        [Fact]
        public async Task Second_sync_is_idempotent()
        {
            using var fx = new SyncTestFixture();
            fx.WriteLocal("BepInEx/plugins/PiP-Disabler/PiP-Disabler.dll", "dll");
            var manifest = new[]
            {
                fx.Entry("BepInEx/plugins/PiP-Disabler/PiP-Disabler.dll", "dll", optional: true, optionalId: "pip"),
            };
            var opts = fx.Options(optionalEnabled: _ => false);

            await fx.PlanAndRunAsync(manifest, opts);
            var (plan2, result2, _) = await fx.PlanAndRunAsync(manifest, opts);

            Assert.Equal(0, plan2.MoveDirCount);
            Assert.Equal(0, result2.Errors);
            Assert.True(fx.LocalExists("BepInEx/plugins-disabled/optional/PiP-Disabler/PiP-Disabler.dll"));
        }

        // CA-034.7 — mover uma pasta de N arquivos gera UMA entrada agregada no relatório; a faxina é silenciosa.
        [Fact]
        public async Task Folder_move_emits_single_aggregated_report_entry_and_cleanup_is_silent()
        {
            using var fx = new SyncTestFixture();
            fx.WriteLocal("BepInEx/plugins/PiP-Disabler/a.dll", "a");
            fx.WriteLocal("BepInEx/plugins/PiP-Disabler/b.dll", "b");
            fx.WriteLocal("BepInEx/plugins/PiP-Disabler/c.bundle", "c");
            Directory.CreateDirectory(fx.LocalPath("BepInEx/plugins/GhostMod")); // casca p/ a faxina remover

            var manifest = new[]
            {
                fx.Entry("BepInEx/plugins/PiP-Disabler/a.dll", "a", optional: true, optionalId: "pip"),
                fx.Entry("BepInEx/plugins/PiP-Disabler/b.dll", "b", optional: true, optionalId: "pip"),
                fx.Entry("BepInEx/plugins/PiP-Disabler/c.bundle", "c", optional: true, optionalId: "pip"),
            };
            var (_, result, _) = await fx.PlanAndRunAsync(manifest, fx.Options(optionalEnabled: _ => false));

            var moves = result.Entries.Where(e => e.action == "moved-to-disabled").ToList();
            Assert.Single(moves);                                     // 1 entrada, não 3 por-arquivo
            Assert.Equal("BepInEx/plugins/PiP-Disabler", moves[0].path);
            Assert.DoesNotContain(result.Entries, e => e.path != null && e.path.Contains("GhostMod")); // faxina silenciosa
        }

        // CR-01-01 (TOCTOU) — arquivo mandatório do manifesto AUSENTE no disco (a baixar) na mesma pasta:
        // NÃO consolida (senão o download futuro seria varrido para a quarentena).
        [Fact]
        public async Task Folder_with_missing_mandatory_file_does_not_consolidate()
        {
            using var fx = new SyncTestFixture();
            fx.WriteLocal("BepInEx/plugins/ModA/extra.dll", "e"); // extra a quarentenar
            var manifest = new[]
            {
                fx.Entry("BepInEx/plugins/ModA/keep.dll", "k"), // mandatório, AUSENTE no disco → Download
            };
            var (plan, _, _) = await fx.PlanAndRunAsync(manifest);

            Assert.Equal(0, plan.MoveDirCount);                                          // não consolidou
            Assert.True(fx.LocalExists("BepInEx/plugins-disabled/ModA/extra.dll"));       // extra foi per-file
            Assert.True(fx.LocalExists("BepInEx/plugins/ModA/keep.dll"));                 // mandatório baixado e mantido
        }

        // CC-10 — mod-pasta com subpastas aninhadas: move a árvore inteira, origem sem órfã.
        [Fact]
        public async Task Nested_folder_mod_moves_whole_tree_without_orphan()
        {
            using var fx = new SyncTestFixture();
            fx.WriteLocal("BepInEx/plugins/Borkel/main.dll", "m");
            fx.WriteLocal("BepInEx/plugins/Borkel/Assets/Shaders/s.shader", "s");
            fx.WriteLocal("BepInEx/plugins/Borkel/Assets/NVG/pvs14/lens.png", "l");

            var manifest = new[]
            {
                fx.Entry("BepInEx/plugins/Borkel/main.dll", "m", optional: true, optionalId: "borkel"),
                fx.Entry("BepInEx/plugins/Borkel/Assets/Shaders/s.shader", "s", optional: true, optionalId: "borkel"),
                fx.Entry("BepInEx/plugins/Borkel/Assets/NVG/pvs14/lens.png", "l", optional: true, optionalId: "borkel"),
            };
            var (plan, _, _) = await fx.PlanAndRunAsync(manifest, fx.Options(optionalEnabled: _ => false));

            Assert.Equal(1, plan.MoveDirCount);
            Assert.False(Directory.Exists(fx.LocalPath("BepInEx/plugins/Borkel")));
            Assert.True(fx.LocalExists("BepInEx/plugins-disabled/optional/Borkel/main.dll"));
            Assert.True(fx.LocalExists("BepInEx/plugins-disabled/optional/Borkel/Assets/Shaders/s.shader"));
            Assert.True(fx.LocalExists("BepInEx/plugins-disabled/optional/Borkel/Assets/NVG/pvs14/lens.png"));
        }

        // CC-6 — a faxina nunca desce/remove uma pasta sob um segmento -disabled (mesmo dentro de um root varrido).
        [Fact]
        public async Task Cleanup_skips_disabled_segment_folders_under_root()
        {
            using var fx = new SyncTestFixture();
            Directory.CreateDirectory(fx.LocalPath("BepInEx/plugins/mod-disabled/EmptyInside"));
            fx.WriteLocal("BepInEx/plugins/Keep/keep.dll", "k"); // p/ o root não ficar vazio

            var manifest = new[] { fx.Entry("BepInEx/plugins/Keep/keep.dll", "k") };
            await fx.PlanAndRunAsync(manifest);

            Assert.True(Directory.Exists(fx.LocalPath("BepInEx/plugins/mod-disabled/EmptyInside")));
        }

        // CC-7 — casing divergente entre manifesto (minúsculo) e disco: agrupa numa chave só e consolida.
        [Fact]
        public async Task Divergent_casing_still_consolidates_and_removes_source()
        {
            using var fx = new SyncTestFixture();
            fx.WriteLocal("BepInEx/plugins/PiP-Disabler/x.dll", "x");       // disco: PiP-Disabler
            var manifest = new[]
            {
                fx.Entry("BepInEx/plugins/pip-disabler/x.dll", "x", optional: true, optionalId: "pip"), // manifesto minúsculo
            };
            var (plan, _, _) = await fx.PlanAndRunAsync(manifest, fx.Options(optionalEnabled: _ => false));

            Assert.Equal(1, plan.MoveDirCount);                                          // agrupou apesar do casing
            Assert.False(Directory.Exists(fx.LocalPath("BepInEx/plugins/PiP-Disabler"))); // origem removida, sem duplicata
            Assert.True(fx.LocalExists("BepInEx/plugins-disabled/optional/PiP-Disabler/x.dll"));
        }

        // R1 (R3.3) — no caminho per-file (pasta compartilhada não consolida), o arquivo movido vence a
        // colisão no destino da quarentena (re-fixa a garantia do MoveWithOverwrite).
        [Fact]
        public async Task PerFile_move_overwrites_colliding_target_in_disabled()
        {
            using var fx = new SyncTestFixture();
            fx.WriteLocal("BepInEx/plugins/Shared/off.dll", "NEW");
            fx.WriteLocal("BepInEx/plugins/Shared/on.dll", "on");
            fx.WriteLocal("BepInEx/plugins-disabled/optional/Shared/off.dll", "OLD"); // colisão pré-existente

            var manifest = new[]
            {
                fx.Entry("BepInEx/plugins/Shared/off.dll", "NEW", optional: true, optionalId: "off"),
                fx.Entry("BepInEx/plugins/Shared/on.dll", "on"), // mandatório → força per-file
            };
            var (plan, _, _) = await fx.PlanAndRunAsync(manifest, fx.Options(optionalEnabled: _ => false));

            Assert.Equal(0, plan.MoveDirCount);                                          // compartilhada → per-file
            Assert.Equal("NEW", fx.ReadLocal("BepInEx/plugins-disabled/optional/Shared/off.dll")); // recém-movido vence
        }
    }
}
