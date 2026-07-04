using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using SPT.Launcher.Sync;
using Xunit;

namespace SPT.Launcher.Tests.Sync
{
    public class SyncEngineTests
    {
        [Fact]
        public async Task Full_run_downloads_deletes_and_moves()
        {
            using var fx = new SyncTestFixture();

            // outdated manifest file + one extra to delete + one extra to move
            fx.WriteLocal("config-server/keep.json", "old");
            fx.WriteLocal("config-server/stale.json", "stale");
            fx.WriteLocal("BepInEx/plugins/OldMod/old.dll", "old-plugin");

            var manifest = new[]
            {
                fx.Entry("config-server/keep.json", "new-content"),
                fx.Entry("BepInEx/plugins/Current/cur.dll", "cur"),
            };

            var (plan, result, baseline) = await fx.PlanAndRunAsync(manifest);

            Assert.Equal(2, result.Updated);
            Assert.Equal(1, result.Deleted);
            Assert.Equal(1, result.MovedToDisabled);
            Assert.Equal(0, result.Errors);
            Assert.False(result.Cancelled);

            Assert.Equal("new-content", fx.ReadLocal("config-server/keep.json"));
            Assert.False(fx.LocalExists("config-server/stale.json"));
            Assert.False(fx.LocalExists("BepInEx/plugins/OldMod/old.dll"));
            Assert.Equal("old-plugin", fx.ReadLocal("BepInEx/plugins-disabled/OldMod/old.dll"));

            // Baseline round-trip: applied hashes persisted, removed paths dropped
            var reloaded = fx.LoadBaseline();
            Assert.True(reloaded.TryGetHash("config-server/keep.json", out string keepHash));
            Assert.Equal(SyncTestFixture.Md5Of("new-content"), keepHash);
            Assert.True(reloaded.TryGetHash("BepInEx/plugins/Current/cur.dll", out _));
            Assert.False(reloaded.TryGetHash("config-server/stale.json", out _));
        }

        [Fact]
        public async Task Move_collision_in_disabled_folder_is_replaced()
        {
            using var fx = new SyncTestFixture();

            fx.WriteLocal("BepInEx/plugins/OldMod/old.dll", "fresh-quarantine");
            fx.WriteLocal("BepInEx/plugins-disabled/OldMod/old.dll", "stale-quarantine");

            var manifest = new[] { fx.Entry("BepInEx/plugins/Current/cur.dll", "cur") };

            var (_, result, _) = await fx.PlanAndRunAsync(manifest);

            Assert.Equal(1, result.MovedToDisabled);
            Assert.Equal(0, result.Errors);
            Assert.Equal("fresh-quarantine", fx.ReadLocal("BepInEx/plugins-disabled/OldMod/old.dll")); // R3.3
            Assert.False(fx.LocalExists("BepInEx/plugins/OldMod/old.dll"));
        }

        [Fact]
        public async Task Second_run_after_move_is_idempotent()
        {
            using var fx = new SyncTestFixture();
            fx.WriteLocal("BepInEx/plugins/OldMod/old.dll", "old-plugin");

            var manifest = new[] { fx.Entry("BepInEx/plugins/Current/cur.dll", "cur") };

            await fx.PlanAndRunAsync(manifest);
            var (plan2, result2, _) = await fx.PlanAndRunAsync(manifest);

            Assert.Empty(plan2.Actions); // quarantined file is not seen again (R3.4)
            Assert.Equal(0, result2.MovedToDisabled);
        }

        [Fact]
        public async Task Preserved_files_are_reported_but_untouched()
        {
            using var fx = new SyncTestFixture();
            fx.WriteLocal("BepInEx/config/mod.cfg", "user-tweaked");

            var manifest = new[] { fx.Entry("BepInEx/config/mod.cfg", "server-v2") };

            var (_, result, _) = await fx.PlanAndRunAsync(manifest); // first run: no baseline

            Assert.Equal(1, result.Preserved);
            Assert.Equal(0, result.Updated);
            Assert.Equal("user-tweaked", fx.ReadLocal("BepInEx/config/mod.cfg"));
            Assert.Contains(result.Entries, e => e.action == "preserved");
        }

        [Fact]
        public async Task Cancellation_between_files_keeps_partial_state_consistent()
        {
            using var fx = new SyncTestFixture();
            using var cts = new CancellationTokenSource();

            var manifest = new[]
            {
                fx.Entry("user/mods/a/a.js", "content-a"),
                fx.Entry("user/mods/b/b.js", "content-b"),
                fx.Entry("user/mods/c/c.js", "content-c"),
            };

            var baseline = fx.LoadBaseline();
            var planner = new SyncPlanner(new SyncRuleResolver(), baseline, fx.Options());
            var plan = await planner.BuildPlanAsync(manifest);
            Assert.Equal(3, plan.IoActionCount);

            // Downloader cancels the token after delivering the first file
            SyncDownloader cancellingDownloader = async (path, ct) =>
            {
                var data = await fx.Downloader(path, ct);
                cts.Cancel();
                return data;
            };

            var engine = new SyncEngine(fx.Root, baseline, cancellingDownloader);
            var result = await engine.ExecuteAsync(plan, fx.ReportPath, null, cts.Token);

            Assert.True(result.Cancelled);
            Assert.Equal(1, result.Updated);
            Assert.Equal(2, result.Pending);

            Assert.True(fx.LocalExists("user/mods/a/a.js"));
            Assert.False(fx.LocalExists("user/mods/b/b.js"));
            Assert.False(fx.LocalExists("user/mods/c/c.js"));

            // C4: baseline persisted with only the applied file
            var reloaded = fx.LoadBaseline();
            Assert.True(reloaded.TryGetHash("user/mods/a/a.js", out _));
            Assert.False(reloaded.TryGetHash("user/mods/b/b.js", out _));

            // Report written even on cancel, flagged as cancelled
            var report = JObject.Parse(File.ReadAllText(fx.ReportPath));
            Assert.True(report["cancelled"].Value<bool>());
        }

        [Fact]
        public async Task Failed_download_leaves_destination_untouched()
        {
            using var fx = new SyncTestFixture();
            fx.WriteLocal("user/mods/a/a.js", "original");

            var manifest = new[]
            {
                fx.Entry("user/mods/a/a.js", "new-a"),
                fx.Entry("user/mods/b/b.js", "new-b"),
            };

            var baseline = fx.LoadBaseline();
            var planner = new SyncPlanner(new SyncRuleResolver(), baseline, fx.Options());
            var plan = await planner.BuildPlanAsync(manifest);

            SyncDownloader failingDownloader = (path, ct) =>
                path.Contains("a.js")
                    ? throw new IOException("disk full")
                    : fx.Downloader(path, ct);

            var engine = new SyncEngine(fx.Root, baseline, failingDownloader);
            var result = await engine.ExecuteAsync(plan, fx.ReportPath, null, CancellationToken.None);

            Assert.Equal(1, result.Errors);
            Assert.Equal(1, result.Updated);
            Assert.Equal("original", fx.ReadLocal("user/mods/a/a.js")); // E1/E2
            Assert.Equal("new-b", fx.ReadLocal("user/mods/b/b.js"));

            // E3: no temp leftovers anywhere
            Assert.Empty(Directory.GetFiles(fx.Root, "*.sync-tmp", SearchOption.AllDirectories));

            // Failed file must NOT enter the baseline
            Assert.False(baseline.TryGetHash("user/mods/a/a.js", out _));
        }

        [Fact]
        public async Task Report_contains_counts_and_entries()
        {
            using var fx = new SyncTestFixture();
            fx.WriteLocal("config-server/stale.json", "stale");

            var manifest = new[] { fx.Entry("user/mods/a/a.js", "content-a") };

            var (_, result, _) = await fx.PlanAndRunAsync(manifest);

            Assert.True(File.Exists(fx.ReportPath));
            var report = JObject.Parse(File.ReadAllText(fx.ReportPath));

            Assert.Equal(1, report["counts"]["updated"].Value<int>());
            Assert.Equal(1, report["counts"]["deleted"].Value<int>());
            Assert.False(report["cancelled"].Value<bool>());

            var entries = (JArray)report["entries"];
            Assert.Equal(2, entries.Count);
            Assert.Contains(entries, e => e["action"].Value<string>() == "updated");
            Assert.Contains(entries, e => e["action"].Value<string>() == "deleted");

            // Per-folder counts for the future UI link
            var byFolder = SyncReport.CountByTopFolder(result.Entries);
            Assert.Equal(1, byFolder["user"]);
            Assert.Equal(1, byFolder["config-server"]);
        }

        [Fact]
        public async Task UpToDate_files_seed_the_baseline_without_any_write()
        {
            using var fx = new SyncTestFixture();
            fx.WriteLocal("BepInEx/config/mod.cfg", "same-everywhere");

            var manifest = new[] { fx.Entry("BepInEx/config/mod.cfg", "same-everywhere") };

            var (plan, result, _) = await fx.PlanAndRunAsync(manifest);

            Assert.Empty(plan.Actions);
            Assert.Empty(fx.DownloadedPaths);

            var reloaded = fx.LoadBaseline();
            Assert.True(reloaded.TryGetHash("BepInEx/config/mod.cfg", out string hash)); // CC7
            Assert.Equal(SyncTestFixture.Md5Of("same-everywhere"), hash);
        }

        [Fact]
        public async Task Custom_delete_strategy_is_used_for_extras()
        {
            using var fx = new SyncTestFixture();
            fx.WriteLocal("config-server/stale.json", "stale");

            var manifest = new[] { fx.Entry("config-server/keep.json", "keep") };

            var baseline = fx.LoadBaseline();
            var planner = new SyncPlanner(new SyncRuleResolver(), baseline, fx.Options());
            var plan = await planner.BuildPlanAsync(manifest);

            string deletedViaStrategy = null;
            var engine = new SyncEngine(fx.Root, baseline, fx.Downloader,
                deleteFile: path => { deletedViaStrategy = path; File.Delete(path); });

            var result = await engine.ExecuteAsync(plan, null, null, CancellationToken.None);

            Assert.Equal(1, result.Deleted);
            Assert.NotNull(deletedViaStrategy);
            Assert.EndsWith("stale.json", deletedViaStrategy);
        }
    }
}
