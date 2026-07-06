using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SPT.Launcher.Models.Launcher;
using SPT.Launcher.Sync;
using Xunit;

namespace SPT.Launcher.Tests.Sync
{
    /// <summary>
    /// Item 008 — performance-config overlay (spec 008: D1 revert via baseline, D2 preserve
    /// customizations, D3 merge instead of a churning second pass).
    /// </summary>
    public sealed class SyncOverlayTests : IDisposable
    {
        private readonly SyncTestFixture _fx = new();

        public void Dispose() => _fx.Dispose();

        private static ManifestFile Overlay(string path, string content) => new()
        {
            path = path,
            hash = SyncTestFixture.Md5Of(content),
            size = content.Length,
        };

        [Fact]
        public void Merge_OverridesByPath_AppendsPackOnly_AndKeepsOptionalFlags()
        {
            var baseFiles = new List<ManifestFile>
            {
                new() { path = "BepInEx/config/graphics.cfg", hash = "aaa", size = 1, optional = false },
                new() { path = "BepInEx/config/optional.cfg", hash = "bbb", size = 2, optional = true, optionalGroup = "gore" },
                new() { path = "BepInEx/plugins/Mod.dll", hash = "ccc", size = 3 },
            };

            var overlayFiles = new List<ManifestFile>
            {
                // different casing/separators on purpose — must still override
                new() { path = "bepinex\\config\\GRAPHICS.CFG", hash = "perf-1", size = 10 },
                new() { path = "BepInEx/config/optional.cfg", hash = "perf-2", size = 20 },
                new() { path = "BepInEx/config/pack-only.cfg", hash = "perf-3", size = 30 },
            };

            var merged = SyncManifestOverlay.Merge(baseFiles, overlayFiles);

            Assert.Equal(4, merged.Files.Count);

            var graphics = merged.Files.Single(f => SyncPathUtil.Normalize(f.path) == "bepinex/config/graphics.cfg");
            Assert.Equal("perf-1", graphics.hash);
            Assert.Equal(10, graphics.size);
            Assert.Equal("BepInEx/config/graphics.cfg", graphics.path); // base casing kept

            // optional flags come from the BASE entry (pack never force-installs disabled groups)
            var optional = merged.Files.Single(f => SyncPathUtil.Normalize(f.path) == "bepinex/config/optional.cfg");
            Assert.Equal("perf-2", optional.hash);
            Assert.True(optional.optional);
            Assert.Equal("gore", optional.optionalGroup);

            // untouched base entry passes through
            var plugin = merged.Files.Single(f => f.path == "BepInEx/plugins/Mod.dll");
            Assert.Equal("ccc", plugin.hash);

            // pack-only entry appended as mandatory
            var packOnly = merged.Files.Single(f => f.path == "BepInEx/config/pack-only.cfg");
            Assert.False(packOnly.optional);

            Assert.True(merged.IsOverlayPath("BepInEx/config/Graphics.cfg"));
            Assert.True(merged.IsOverlayPath("BepInEx/config/pack-only.cfg"));
            Assert.False(merged.IsOverlayPath("BepInEx/plugins/Mod.dll"));
        }

        [Fact]
        public async Task Overlay_Applies_WhenLocalEqualsBaseline_AndRoutesToOverlaySource()
        {
            const string path = "BepInEx/config/graphics.cfg";

            // 1st run without overlay: local == server default -> baseline seeded (CC7)
            _fx.WriteLocal(path, "default");
            var baseManifest = new List<ManifestFile> { _fx.Entry(path, "default") };
            await _fx.PlanAndRunAsync(baseManifest);

            // 2nd run with the overlay merged in
            var merged = SyncManifestOverlay.Merge(baseManifest, new List<ManifestFile> { Overlay(path, "performance") });

            var overlayDownloads = new List<string>();
            SyncDownloader overlayDownloader = (p, ct) =>
            {
                overlayDownloads.Add(p);
                return Task.FromResult(System.Text.Encoding.UTF8.GetBytes("performance"));
            };

            var baseline = _fx.LoadBaseline();
            var planner = new SyncPlanner(new SyncRuleResolver(), baseline, _fx.Options());
            var plan = await planner.BuildPlanAsync(merged.Files.ToList());

            Assert.Equal(1, plan.DownloadCount);

            var engine = new SyncEngine(_fx.Root, baseline, merged.CreateDownloader(_fx.Downloader, overlayDownloader));
            var result = await engine.ExecuteAsync(plan, _fx.ReportPath);

            Assert.Equal(1, result.Updated);
            Assert.Equal("performance", _fx.ReadLocal(path));

            // download came from the overlay source, not the normal endpoint
            // (run 1 was fully up-to-date, so the base downloader was never hit at all)
            Assert.Single(overlayDownloads);
            Assert.Empty(_fx.DownloadedPaths);

            // baseline updated to the PACK hash — this is what makes OFF revertible (D1)
            var persisted = _fx.LoadBaseline();
            Assert.True(persisted.TryGetHash(path, out var hash));
            Assert.Equal(SyncTestFixture.Md5Of("performance"), hash);
        }

        [Fact]
        public async Task Overlay_PreservesUserCustomizedConfig()
        {
            const string path = "BepInEx/config/graphics.cfg";

            // converge baseline on the server default first
            _fx.WriteLocal(path, "default");
            var baseManifest = new List<ManifestFile> { _fx.Entry(path, "default") };
            await _fx.PlanAndRunAsync(baseManifest);

            // user customizes the file afterwards (local != baseline)
            _fx.WriteLocal(path, "user-tuned");

            var merged = SyncManifestOverlay.Merge(baseManifest, new List<ManifestFile> { Overlay(path, "performance") });

            var baseline = _fx.LoadBaseline();
            var planner = new SyncPlanner(new SyncRuleResolver(), baseline, _fx.Options());
            var plan = await planner.BuildPlanAsync(merged.Files.ToList());

            var action = Assert.Single(plan.Actions);
            Assert.Equal(SyncActionKind.PreserveCustomized, action.Kind);

            var engine = new SyncEngine(_fx.Root, baseline, merged.CreateDownloader(_fx.Downloader, _fx.Downloader));
            var result = await engine.ExecuteAsync(plan, _fx.ReportPath);

            Assert.Equal(0, result.Updated);
            Assert.Equal(1, result.Preserved);
            Assert.Equal("user-tuned", _fx.ReadLocal(path)); // pack never clobbers customizations (D2)
        }

        [Fact]
        public async Task TurningOverlayOff_RevertsToServerDefault_ViaNormalSync()
        {
            const string path = "BepInEx/config/graphics.cfg";

            // Regime ON: local == pack, baseline == pack hash (simulated end state of overlay apply)
            var baseManifest = new List<ManifestFile> { _fx.Entry(path, "default") };
            var merged = SyncManifestOverlay.Merge(baseManifest, new List<ManifestFile> { Overlay(path, "performance") });

            _fx.WriteLocal(path, "default");
            var (_, _, _) = await _fx.PlanAndRunAsync(baseManifest); // seed baseline = default

            var baseline = _fx.LoadBaseline();
            var planner = new SyncPlanner(new SyncRuleResolver(), baseline, _fx.Options());
            var plan = await planner.BuildPlanAsync(merged.Files.ToList());
            var engine = new SyncEngine(_fx.Root, baseline,
                merged.CreateDownloader(_fx.Downloader, (p, ct) => Task.FromResult(System.Text.Encoding.UTF8.GetBytes("performance"))));
            await engine.ExecuteAsync(plan, _fx.ReportPath);
            Assert.Equal("performance", _fx.ReadLocal(path));

            // Toggle OFF: next verification runs with the BASE manifest only.
            // local == baseline (pack hash) => "equals baseline, server evolved" => re-download default.
            var (offPlan, offResult, _) = await _fx.PlanAndRunAsync(baseManifest);

            Assert.Equal(1, offPlan.DownloadCount);
            Assert.Equal(1, offResult.Updated);
            Assert.Equal("default", _fx.ReadLocal(path));

            var persisted = _fx.LoadBaseline();
            Assert.True(persisted.TryGetHash(path, out var hash));
            Assert.Equal(SyncTestFixture.Md5Of("default"), hash);
        }

        /// <summary>
        /// ref: CR-01-02 — comportamento DOCUMENTADO (o real, não o desejável): overlay ON sem
        /// baseline (primeira sync da instalação, sync-state.json corrompido/apagado) cai em
        /// R1.5 → o pack NÃO aplica e o arquivo fica "preservado" silenciosamente; ações
        /// preservadas não semeiam baseline, então o estado se repete enquanto ON. Destrava
        /// com OFF → verificar (CC7 semeia) → ON. Cenário somado ao P-008.1 (E2E).
        /// </summary>
        [Fact]
        public async Task OverlayOn_WithoutBaseline_DoesNotApplyPack_KnownWedge()
        {
            const string path = "BepInEx/config/graphics.cfg";

            _fx.WriteLocal(path, "default"); // local == default do server, mas SEM baseline
            var baseManifest = new List<ManifestFile> { _fx.Entry(path, "default") };
            var merged = SyncManifestOverlay.Merge(baseManifest, new List<ManifestFile> { Overlay(path, "performance") });

            var overlayDownloads = new List<string>();
            SyncDownloader overlayDownloader = (p, ct) =>
            {
                overlayDownloads.Add(p);
                return Task.FromResult(System.Text.Encoding.UTF8.GetBytes("performance"));
            };

            var baseline = _fx.LoadBaseline(); // vazio — primeiro run
            var planner = new SyncPlanner(new SyncRuleResolver(), baseline, _fx.Options());
            var plan = await planner.BuildPlanAsync(merged.Files.ToList());

            var action = Assert.Single(plan.Actions);
            Assert.Equal(SyncActionKind.PreserveCustomized, action.Kind); // R1.5 conservador

            var engine = new SyncEngine(_fx.Root, baseline, merged.CreateDownloader(_fx.Downloader, overlayDownloader));
            var result = await engine.ExecuteAsync(plan, _fx.ReportPath);

            Assert.Equal(0, result.Updated);
            Assert.Equal(1, result.Preserved);
            Assert.Equal("default", _fx.ReadLocal(path)); // pack não aplicou
            Assert.Empty(overlayDownloads);

            // e o baseline continua sem a entrada — o wedge persiste enquanto ON
            Assert.False(_fx.LoadBaseline().TryGetHash(path, out _));
        }

        /// <summary>
        /// ref: CR-01-03 — comportamento DOCUMENTADO: arquivo tocado após o apply do pack
        /// (ex.: BepInEx re-serializa cfgs no boot do plugin) diverge do baseline ⇒ o OFF
        /// preserva em vez de restaurar o padrão (R1.4 — correto p/ customização; texto da UI
        /// ajustado p/ não prometer restauração incondicional). Somado ao P-008.1 (E2E).
        /// </summary>
        [Fact]
        public async Task Off_AfterFileTouchedPostApply_PreservesInsteadOfReverting()
        {
            const string path = "BepInEx/config/graphics.cfg";

            // converge no default, aplica o pack (regime ON)
            _fx.WriteLocal(path, "default");
            var baseManifest = new List<ManifestFile> { _fx.Entry(path, "default") };
            await _fx.PlanAndRunAsync(baseManifest); // baseline := default

            var merged = SyncManifestOverlay.Merge(baseManifest, new List<ManifestFile> { Overlay(path, "performance") });
            var baseline = _fx.LoadBaseline();
            var planner = new SyncPlanner(new SyncRuleResolver(), baseline, _fx.Options());
            var plan = await planner.BuildPlanAsync(merged.Files.ToList());
            var engine = new SyncEngine(_fx.Root, baseline,
                merged.CreateDownloader(_fx.Downloader, (p, ct) => Task.FromResult(System.Text.Encoding.UTF8.GetBytes("performance"))));
            await engine.ExecuteAsync(plan, _fx.ReportPath);
            Assert.Equal("performance", _fx.ReadLocal(path));

            // o "jogo" reescreve o cfg (keys/whitespace normalizados ⇒ hash muda)
            _fx.WriteLocal(path, "performance-rewritten-by-game");

            // OFF: verificação com o manifesto base — NÃO restaura (divergiu do baseline)
            var (offPlan, offResult, _) = await _fx.PlanAndRunAsync(baseManifest);

            var offAction = Assert.Single(offPlan.Actions);
            Assert.Equal(SyncActionKind.PreserveCustomized, offAction.Kind);
            Assert.Equal(0, offResult.Updated);
            Assert.Equal("performance-rewritten-by-game", _fx.ReadLocal(path));
        }

        /// <summary>
        /// ref: CR-01-05 — o baseline grava o hash dos BYTES gravados, não o hash do manifesto:
        /// pack editado no server sem /refresh (manifesto stale) não pode envenenar o baseline
        /// (local != baseline para sempre ⇒ wedge dos CR-01-02/CR-01-03).
        /// </summary>
        [Fact]
        public async Task Baseline_RecordsHashOfWrittenBytes_NotStaleManifestHash()
        {
            const string path = "BepInEx/config/graphics.cfg";

            // manifesto anuncia o hash ANTIGO, mas o server entrega bytes NOVOS
            var staleEntry = new ManifestFile
            {
                path = path,
                hash = SyncTestFixture.Md5Of("old-pack-bytes"),
                size = 14,
            };

            SyncDownloader freshDownloader = (p, ct) =>
                Task.FromResult(System.Text.Encoding.UTF8.GetBytes("fresh-pack-bytes"));

            var warnings = new List<string>();
            var baseline = _fx.LoadBaseline();
            var planner = new SyncPlanner(new SyncRuleResolver(), baseline, _fx.Options());
            var plan = await planner.BuildPlanAsync(new List<ManifestFile> { staleEntry }); // ausente ⇒ download

            var engine = new SyncEngine(_fx.Root, baseline, freshDownloader, log: warnings.Add);
            var result = await engine.ExecuteAsync(plan, _fx.ReportPath);

            Assert.Equal(1, result.Updated);
            Assert.Equal("fresh-pack-bytes", _fx.ReadLocal(path));

            // baseline reflete o DISCO — próxima verificação vê local == baseline (sem wedge)
            var persisted = _fx.LoadBaseline();
            Assert.True(persisted.TryGetHash(path, out var hash));
            Assert.Equal(SyncTestFixture.Md5Of("fresh-pack-bytes"), hash);
            Assert.NotEqual(staleEntry.hash, hash);

            // e o desalinhamento manifesto×bytes foi logado (operador esqueceu o /refresh)
            Assert.Contains(warnings, w => w.Contains("não batem"));
        }

        [Fact]
        public async Task SteadyStateOn_SecondRunHasNoIoActions()
        {
            const string path = "BepInEx/config/graphics.cfg";

            var baseManifest = new List<ManifestFile> { _fx.Entry(path, "default") };
            var merged = SyncManifestOverlay.Merge(baseManifest, new List<ManifestFile> { Overlay(path, "performance") });

            _fx.WriteLocal(path, "default");
            await _fx.PlanAndRunAsync(baseManifest); // baseline = default

            SyncDownloader overlayDownloader = (p, ct) => Task.FromResult(System.Text.Encoding.UTF8.GetBytes("performance"));

            // run 1 with overlay: applies the pack
            var baseline1 = _fx.LoadBaseline();
            var plan1 = await new SyncPlanner(new SyncRuleResolver(), baseline1, _fx.Options()).BuildPlanAsync(merged.Files.ToList());
            await new SyncEngine(_fx.Root, baseline1, merged.CreateDownloader(_fx.Downloader, overlayDownloader))
                .ExecuteAsync(plan1, _fx.ReportPath);

            // run 2 with overlay: must converge — no churn (D3)
            var baseline2 = _fx.LoadBaseline();
            var plan2 = await new SyncPlanner(new SyncRuleResolver(), baseline2, _fx.Options()).BuildPlanAsync(merged.Files.ToList());

            Assert.Equal(0, plan2.IoActionCount);
            Assert.Empty(plan2.Actions);
            Assert.Single(plan2.UpToDate);
        }
    }
}
