using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SPT.Launcher.Models.Launcher;
using SPT.Launcher.Sync;
using Xunit;

namespace SPT.Launcher.Tests.Sync
{
    /// <summary>
    /// Item 017 — config-server → config seed (SeedIfMissingByName): copy a default BY NAME only
    /// when the user's target is absent; never delete, never overwrite, no hash/baseline (the seed
    /// is memory-less — a file the user deleted reappears next seed).
    /// </summary>
    public class SyncSeedTests
    {
        // === DeriveSeedTarget (pure mapping) ===

        [Theory]
        [InlineData("BepInEx/config-server/graphics.cfg", "bepinex/config-server", "BepInEx/config/graphics.cfg")]
        [InlineData("config-server/a/X.CFG", "config-server", "config/a/X.CFG")] // remainder casing preserved
        [InlineData("BepInEx/config-server/sub/deep/f.json", "bepinex/config-server", "BepInEx/config/sub/deep/f.json")]
        public void DeriveSeedTarget_maps_source_to_config_preserving_casing(string source, string prefix, string expected)
        {
            Assert.Equal(expected, SyncPathUtil.DeriveSeedTarget(source, prefix));
        }

        [Fact]
        public void DeriveSeedTarget_returns_null_when_no_file_remainder()
        {
            Assert.Null(SyncPathUtil.DeriveSeedTarget("config-server", "config-server"));
        }

        // === Plan + apply (fixture, default resolver = fallback seed rule) ===

        [Fact]
        public async Task Seed_copies_when_target_is_absent()
        {
            using var fx = new SyncTestFixture();

            var manifest = new[] { fx.Entry("BepInEx/config-server/graphics.cfg", "server-default") };

            var (plan, result, _) = await fx.PlanAndRunAsync(manifest, resolver: SyncTestFixture.ResolverWithConfigServerSeed());

            var action = Assert.Single(plan.Actions);
            Assert.Equal(SyncActionKind.SeedCopy, action.Kind);
            Assert.Equal("BepInEx/config/graphics.cfg", action.SeedTargetRelative);

            Assert.Equal(1, result.Seeded);
            Assert.Equal(0, result.Updated);
            Assert.Equal("server-default", fx.ReadLocal("BepInEx/config/graphics.cfg"));
            // the config-server SOURCE folder is server-only — never materialized on the user disk
            Assert.False(fx.LocalExists("BepInEx/config-server/graphics.cfg"));
        }

        [Fact]
        public async Task Seed_does_not_overwrite_existing_target_even_with_different_content()
        {
            using var fx = new SyncTestFixture();
            fx.WriteLocal("BepInEx/config/graphics.cfg", "user-customized"); // differs from the server default

            var manifest = new[] { fx.Entry("BepInEx/config-server/graphics.cfg", "server-default") };

            var (plan, result, _) = await fx.PlanAndRunAsync(manifest, resolver: SyncTestFixture.ResolverWithConfigServerSeed());

            Assert.Empty(plan.Actions);   // present by name → no-op (content/metadata irrelevant)
            Assert.Equal(0, result.Seeded);
            Assert.Empty(fx.DownloadedPaths); // presence check is by name only — nothing downloaded
            Assert.Equal("user-customized", fx.ReadLocal("BepInEx/config/graphics.cfg"));
        }

        [Fact]
        public async Task Seed_preserves_subfolders_by_relative_path()
        {
            using var fx = new SyncTestFixture();

            var manifest = new[]
            {
                fx.Entry("BepInEx/config-server/sub/a.cfg", "a"),
                fx.Entry("BepInEx/config-server/sub/deep/b.cfg", "b"),
            };

            var (_, result, _) = await fx.PlanAndRunAsync(manifest, resolver: SyncTestFixture.ResolverWithConfigServerSeed());

            Assert.Equal(2, result.Seeded);
            Assert.Equal("a", fx.ReadLocal("BepInEx/config/sub/a.cfg"));
            Assert.Equal("b", fx.ReadLocal("BepInEx/config/sub/deep/b.cfg"));
        }

        [Fact]
        public async Task Seed_copies_only_the_absent_sibling_in_a_subfolder()
        {
            using var fx = new SyncTestFixture();
            fx.WriteLocal("BepInEx/config/sub/a.cfg", "mine"); // present by name → keep

            var manifest = new[]
            {
                fx.Entry("BepInEx/config-server/sub/a.cfg", "server-a"),
                fx.Entry("BepInEx/config-server/sub/b.cfg", "server-b"),
            };

            var (_, result, _) = await fx.PlanAndRunAsync(manifest, resolver: SyncTestFixture.ResolverWithConfigServerSeed());

            Assert.Equal(1, result.Seeded);
            Assert.Equal("mine", fx.ReadLocal("BepInEx/config/sub/a.cfg"));    // preserved
            Assert.Equal("server-b", fx.ReadLocal("BepInEx/config/sub/b.cfg")); // seeded
        }

        [Fact]
        public async Task Server_without_config_server_is_a_noop_for_seeding()
        {
            using var fx = new SyncTestFixture();
            fx.WriteLocal("BepInEx/config/existing.cfg", "mine");

            var manifest = new[] { fx.Entry("user/mods/some/mod.js", "x") };

            var (plan, result, _) = await fx.PlanAndRunAsync(manifest, resolver: SyncTestFixture.ResolverWithConfigServerSeed());

            Assert.Equal(0, result.Seeded);
            Assert.DoesNotContain(plan.Actions, a => a.Kind == SyncActionKind.SeedCopy);
            Assert.Equal("mine", fx.ReadLocal("BepInEx/config/existing.cfg"));
        }

        [Fact]
        public async Task Deleted_seed_reappears_on_next_seed_memoryless()
        {
            using var fx = new SyncTestFixture();
            var manifest = new[] { fx.Entry("BepInEx/config-server/x.cfg", "default") };

            var (_, first, _) = await fx.PlanAndRunAsync(manifest, resolver: SyncTestFixture.ResolverWithConfigServerSeed());
            Assert.Equal(1, first.Seeded);

            File.Delete(fx.LocalPath("BepInEx/config/x.cfg")); // user removes the default on purpose
            Assert.False(fx.LocalExists("BepInEx/config/x.cfg"));

            var (_, second, _) = await fx.PlanAndRunAsync(manifest, resolver: SyncTestFixture.ResolverWithConfigServerSeed());
            Assert.Equal(1, second.Seeded); // no memory of a previous seed → it comes back
            Assert.Equal("default", fx.ReadLocal("BepInEx/config/x.cfg"));
        }

        [Fact]
        public async Task Seeded_file_survives_a_managed_path_mirror_sweep()
        {
            using var fx = new SyncTestFixture();

            var manifest = new[]
            {
                fx.Entry("BepInEx/config-server/x.cfg", "default"),
                fx.Entry("BepInEx/plugins/Current/cur.dll", "cur"),
            };

            var options = fx.Options();
            options.ManagedPaths = new[] { "BepInEx" }; // extras under BepInEx would normally be deleted

            var baseline = fx.LoadBaseline();
            var plan = await new SyncPlanner(SyncTestFixture.ResolverWithConfigServerSeed(), baseline, options).BuildPlanAsync(manifest);
            var result = await new SyncEngine(fx.Root, baseline, fx.Downloader).ExecuteAsync(plan, fx.ReportPath);

            Assert.Equal(1, result.Seeded);
            Assert.Equal(0, result.Deleted);
            Assert.True(fx.LocalExists("BepInEx/config/x.cfg"));

            // 2nd run: seeded file (a config extra, never a manifest entry) is neither deleted nor re-seeded
            var baseline2 = fx.LoadBaseline();
            var plan2 = await new SyncPlanner(SyncTestFixture.ResolverWithConfigServerSeed(), baseline2, options).BuildPlanAsync(manifest);
            Assert.DoesNotContain(plan2.Actions, a => a.Kind == SyncActionKind.DeleteExtra);
            Assert.DoesNotContain(plan2.Actions, a => a.Kind == SyncActionKind.SeedCopy);

            var result2 = await new SyncEngine(fx.Root, baseline2, fx.Downloader).ExecuteAsync(plan2, fx.ReportPath);
            Assert.Equal(0, result2.Deleted);
            Assert.True(fx.LocalExists("BepInEx/config/x.cfg"));
        }

        [Fact]
        public async Task Seed_skips_at_apply_when_target_appeared_after_planning()
        {
            // TOCTOU guard: target created between plan and apply must NOT be overwritten (and not error).
            using var fx = new SyncTestFixture();
            var manifest = new[] { fx.Entry("BepInEx/config-server/x.cfg", "default") };

            var baseline = fx.LoadBaseline();
            var plan = await new SyncPlanner(SyncTestFixture.ResolverWithConfigServerSeed(), baseline, fx.Options()).BuildPlanAsync(manifest);
            Assert.Equal(1, plan.SeedCount);

            fx.WriteLocal("BepInEx/config/x.cfg", "appeared-meanwhile");

            var result = await new SyncEngine(fx.Root, baseline, fx.Downloader)
                .ExecuteAsync(plan, fx.ReportPath, null, CancellationToken.None);

            Assert.Equal(0, result.Seeded);
            Assert.Equal(0, result.Errors);
            Assert.Empty(fx.DownloadedPaths); // skipped before hitting the downloader
            Assert.Equal("appeared-meanwhile", fx.ReadLocal("BepInEx/config/x.cfg"));
        }

        [Fact]
        public async Task Seed_does_not_write_a_baseline_entry_for_the_seeded_file()
        {
            using var fx = new SyncTestFixture();
            var manifest = new[] { fx.Entry("BepInEx/config-server/x.cfg", "default") };

            var (_, result, _) = await fx.PlanAndRunAsync(manifest, resolver: SyncTestFixture.ResolverWithConfigServerSeed());
            Assert.Equal(1, result.Seeded);

            var reloaded = fx.LoadBaseline();
            // memory-less: neither the source nor the target enters the baseline
            Assert.False(reloaded.TryGetHash("BepInEx/config-server/x.cfg", out _));
            Assert.False(reloaded.TryGetHash("BepInEx/config/x.cfg", out _));
        }
    }
}
