using System.Collections.Generic;
using BepInEx.Bootstrap;

namespace CompoundingPerf.Client.Compat;

/// <summary>
/// Detects other installed BepInEx plugins by GUID so individual features can adjust
/// behavior (e.g. add SAIN's GUID to the log-suppressor allowlist automatically).
/// Runs once at <c>Plugin.Awake</c> after <see cref="Chainloader.PluginInfos"/> is populated.
/// </summary>
public static class ModDetector
{
    // Known perf-relevant mod GUIDs. Verified against actual installed plugin GUIDs
    // (see BepInEx's KmyTarkovConfiguration listing) — earlier guesses based on author
    // names were wrong (e.g. SAIN ships as me.sol.sain, not com.solarint.SAIN).
    public static readonly HashSet<string> KnownPerfMods = new()
    {
        "me.sol.sain",
        "com.dvize.AILimit",
        "com.dirtbikercj.PerformanceImprovements",
        "com.tyfon.uifixes",
        "VIP.TommySoucy.MoreCheckmarks",
    };

    public static DetectedMods Scan()
    {
        var present = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);
        try
        {
            foreach (var kv in Chainloader.PluginInfos)
                present.Add(kv.Key);
        }
        catch
        {
            // Chainloader may not be ready in unusual hosting setups — treat as nothing detected.
        }

        return new DetectedMods
        {
            HasSain                    = present.Contains("me.sol.sain"),
            HasAiLimit                 = present.Contains("com.dvize.AILimit"),
            HasPerformanceImprovements = present.Contains("com.dirtbikercj.PerformanceImprovements"),
            HasTyfonUIFixes            = present.Contains("com.tyfon.uifixes"),
            AllPluginGuids             = present,
        };
    }
}

public class DetectedMods
{
    public bool HasSain                    { get; set; }
    public bool HasAiLimit                 { get; set; }
    public bool HasPerformanceImprovements { get; set; }
    public bool HasTyfonUIFixes            { get; set; }
    public HashSet<string> AllPluginGuids  { get; set; } = new();
}
