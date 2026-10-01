#if BENCH
using EFT;
using HarmonyLib;

namespace CompoundingPerf.Client.Patches;

/// <summary>C6 sampling hook (BENCH dev builds only) — rides EFT's own per-frame world
/// ticker. Release builds contain no patches at all.</summary>
[HarmonyPatch(typeof(GameWorldUnityTickListener), nameof(GameWorldUnityTickListener.Update))]
public static class WorldTickPatch
{
    [HarmonyPostfix]
    public static void Postfix()
    {
        FrameStatsRecorder.SampleTick();
    }
}

/// <summary>C6 raid-end hook. OnDestroy is declared on GameWorld itself (FIKA's world
/// classes don't override it), and it fires for hideout worlds too — the recorder
/// ignores those.</summary>
[HarmonyPatch(typeof(GameWorld), "OnDestroy")]
public static class WorldDestroyPatch
{
    [HarmonyPostfix]
    public static void Postfix()
    {
        FrameStatsRecorder.OnWorldDestroyed();
    }
}
#endif
