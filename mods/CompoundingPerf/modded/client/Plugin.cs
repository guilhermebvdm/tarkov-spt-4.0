using System;
using System.IO;
using BepInEx;
using BepInEx.Logging;
using CompoundingPerf.Client.Compat;
using CompoundingPerf.Client.Telemetry;
#if BENCH
using System.Reflection;
using HarmonyLib;
#endif

namespace CompoundingPerf.Client;

[BepInPlugin(ModGuid, ModName, ModVersion)]
public class Plugin : BaseUnityPlugin
{
    public const string ModGuid    = "com.echostarz.compoundingperf.client";
    public const string ModName    = "CompoundingPerf.Client";
    public const string ModVersion = "1.3.0";

    public static CompoundingPerfConfig LoadedConfig { get; private set; } = new();
    public static DetectedMods Mods { get; private set; } = new();
    public static ManualLogSource? Log { get; private set; }
    public static string? SptUserLogsDir { get; private set; }

    private void Awake()
    {
        Log = Logger;
        Log.LogInfo($"{ModName} v{ModVersion} loading");

        try
        {
            var (loaded, sptLogsDir) = ConfigLoader.Load();
            LoadedConfig = loaded;
            SptUserLogsDir = sptLogsDir;
            ClientTelemetry.TimingEnabled = LoadedConfig.Telemetry.TimingEnabled;

            Mods = ModDetector.Scan();
            if (LoadedConfig.Compat.Verbose)
            {
                Log.LogInfo($"{ModName} compat scan: SAIN={Mods.HasSain} AILimit={Mods.HasAiLimit} PerfImprovements={Mods.HasPerformanceImprovements} Tyfon={Mods.HasTyfonUIFixes}");
            }

#if BENCH
            // C6: frame-stats benchmark recorder, the only client feature since the
            // in-raid log suppressor was removed in 1.3 (its value never survived
            // measurement, and suppressing other mods' in-raid logs costs the
            // ecosystem more than it saves). Compiled only into dev builds
            // (-p:Bench=true); release packages ship NO client plugin at all.
            // Sampling rides EFT's own GameWorldUnityTickListener via Harmony
            // postfix — see FrameStatsRecorder doc for why we don't use our own
            // MonoBehaviour. masterEnabled is still read so each stats line tags
            // which side of an A/B run it belongs to.
            FrameStatsRecorder.Enabled = LoadedConfig.Client.FrameStats.Enabled;
            if (FrameStatsRecorder.Enabled)
            {
                Log.LogInfo($"{ModName} [C6/BENCH] frame-stats recorder armed — per-raid FPS stats will append to user/logs/CompoundingPerf-framestats.jsonl");
            }

            var harmony = new Harmony(ModGuid);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
#endif

            Log.LogInfo($"{ModName} ready");
        }
        catch (Exception ex)
        {
            Log.LogError($"{ModName} failed to initialize: {ex}");
        }
    }

    private void OnApplicationQuit()
    {
        if (!LoadedConfig.Telemetry.Enabled || string.IsNullOrEmpty(SptUserLogsDir)) return;
        try { TelemetryDumper.Dump(SptUserLogsDir!, "shutdown"); }
        catch { /* best-effort */ }
    }
}
