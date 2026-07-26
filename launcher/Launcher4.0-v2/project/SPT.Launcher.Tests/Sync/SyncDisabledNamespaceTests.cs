using SPT.Launcher.Sync;
using Xunit;

namespace SPT.Launcher.Tests.Sync
{
    /// <summary>
    /// Item 030 (D-14/D-20 · gate G-7) — a quarentena por ORIGEM impede que backups de canais diferentes
    /// (config-force, mod opcional, config de performance) se sobrescrevam por homonímia. O force fica na
    /// RAIZ (formato legado, em produção desde a 2.3.0); as origens novas ganham subpasta.
    /// </summary>
    public class SyncDisabledNamespaceTests
    {
        [Fact]
        public void Force_backup_stays_at_the_root_legacy_format()
        {
            // O overload sem origem = Force = raiz (compatível com os backups já existentes na máquina).
            string path = SyncPathUtil.DeriveDisabledBackup("BepInEx/config/x.cfg", "bepinex/config-force");
            Assert.Equal("BepInEx/config-disabled/x.cfg", path);
        }

        [Fact]
        public void Optional_and_performance_get_their_own_subfolders()
        {
            string optional = SyncPathUtil.DeriveDisabledBackup("BepInEx/config/x.cfg", "bepinex/config-force",
                SyncPathUtil.DisabledOrigin.Optional);
            string replaced = SyncPathUtil.DeriveDisabledBackup("BepInEx/config/x.cfg", "bepinex/config-performance",
                SyncPathUtil.DisabledOrigin.PerformanceReplaced);
            string removed = SyncPathUtil.DeriveDisabledBackup("BepInEx/config/x.cfg", "bepinex/config-performance",
                SyncPathUtil.DisabledOrigin.PerformanceRemoved);

            Assert.Equal("BepInEx/config-disabled/optional/x.cfg", optional);
            Assert.Equal("BepInEx/config-disabled/performance/replaced/x.cfg", replaced);
            Assert.Equal("BepInEx/config-disabled/performance/removed/x.cfg", removed);
        }

        // G-7: o MESMO nome de arquivo vindo das três origens cai em três caminhos distintos — nenhuma
        // sobrescreve o backup da outra.
        [Fact]
        public void Same_filename_from_three_origins_never_collides()
        {
            string force = SyncPathUtil.DeriveDisabledBackup("BepInEx/config/dupe.cfg", "bepinex/config-force",
                SyncPathUtil.DisabledOrigin.Force);
            string optional = SyncPathUtil.DeriveDisabledBackup("BepInEx/config/dupe.cfg", "bepinex/config-force",
                SyncPathUtil.DisabledOrigin.Optional);
            string perf = SyncPathUtil.DeriveDisabledBackup("BepInEx/config/dupe.cfg", "bepinex/config-performance",
                SyncPathUtil.DisabledOrigin.PerformanceReplaced);

            Assert.NotEqual(force, optional);
            Assert.NotEqual(force, perf);
            Assert.NotEqual(optional, perf);
        }

        // O guard que impede o deleteFiles/ScanExtras de tocar a quarentena é por SEGMENTO — cobre as
        // subpastas novas por herança (config-disabled/performance/... casa no 2º segmento).
        [Fact]
        public void Disabled_segment_guard_covers_the_new_subfolders()
        {
            Assert.True(SyncPathUtil.ContainsDisabledSegment("bepinex/config-disabled/optional/x.cfg"));
            Assert.True(SyncPathUtil.ContainsDisabledSegment("bepinex/config-disabled/performance/replaced/x.cfg"));
        }
    }
}
