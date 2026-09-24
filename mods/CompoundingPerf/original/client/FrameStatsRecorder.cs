#if BENCH
using System;
using System.Collections.Generic;
using System.IO;
using Comfort.Common;
using EFT;
using Newtonsoft.Json;
using UnityEngine;

namespace CompoundingPerf.Client;

/// <summary>
/// C6 — In-raid frame-time recorder for with/without-mod benchmarking.
/// DEV BUILDS ONLY: the entire type is compiled out unless the build defines BENCH
/// (<c>dotnet build -p:Bench=true</c>). Release packages contain no recorder, so
/// shipped users carry zero per-frame overhead and the mod makes no claims it
/// isn't measuring.
///
/// <para><b>How it samples</b>: a Harmony postfix on EFT's own
/// <c>GameWorldUnityTickListener.Update</c> — the engine's world ticker — calls
/// <see cref="SampleTick"/> once per rendered frame while a world exists. We
/// deliberately do NOT use our own MonoBehaviour: two independent attempts
/// (plugin-component Update, dedicated DontDestroyOnLoad GameObject) never received a
/// single Update callback in this SPT/FIKA environment. Riding the game's own ticker
/// cannot be suppressed without breaking the game itself.</para>
///
/// <para>The hideout is also a GameWorld, so arming checks the world type and ignores
/// <c>HideoutGameWorld</c>. Raid end is detected via a postfix on
/// <c>GameWorld.OnDestroy</c>, which finalizes the buffer into one JSON stats line
/// (map, population, avg FPS, 1%/0.1% lows, hitch counts, worst spike) appended to
/// <c>SPT/user/logs/CompoundingPerf-framestats.jsonl</c>.</para>
///
/// <para><b>Population context</b>: every 256 recorded frames (~2–4s) the alive-player
/// list is sampled — peak concurrent count plus unique profile IDs seen across the
/// raid (both include the local player, and FIKA co-op players when present). Bots
/// that spawn and die entirely between samples can be missed; the numbers are context
/// for comparing rows, not a census.</para>
/// </summary>
public static class FrameStatsRecorder
{
    private const int MaxSamples = 480_000; // ~33 min at 240fps; ~1.9MB buffer

    private static float[]? _buffer;
    private static int _count;
    private static bool _armed;
    private static DateTime _raidStartUtc;
    private static string _locationId = "unknown";
    private static int _peakAlive;
    private static readonly HashSet<string> SeenProfiles = new(StringComparer.Ordinal);

    public static bool Enabled;

    /// <summary>Called from the GameWorldUnityTickListener.Update postfix — once per
    /// frame while any world exists.</summary>
    public static void SampleTick()
    {
        if (!Enabled)
        {
            return;
        }

        if (!_armed)
        {
            var world = Singleton<GameWorld>.Instance;
            if (world is null || world is HideoutGameWorld)
            {
                return;
            }

            _buffer ??= new float[MaxSamples];
            _count = 0;
            _raidStartUtc = DateTime.UtcNow;
            _locationId = SafeLocationId(world);
            _peakAlive = 0;
            SeenProfiles.Clear();
            _armed = true;
            Plugin.Log?.LogInfo($"[C6] raid detected ({world.GetType().Name}, map={_locationId}) — frame-stats recording started");
        }

        if (_count < MaxSamples)
        {
            _buffer![_count++] = Time.unscaledDeltaTime;
            if ((_count & 255) == 0)
            {
                SamplePopulation();
            }
        }
    }

    /// <summary>Called from the GameWorld.OnDestroy postfix — fires for raid worlds and
    /// hideout alike; only finalizes when we actually armed.</summary>
    public static void OnWorldDestroyed()
    {
        if (!_armed)
        {
            return;
        }

        _armed = false;
        DumpStats();
    }

    private static string SafeLocationId(GameWorld world)
    {
        try
        {
            var id = world.LocationId;
            return string.IsNullOrEmpty(id) ? "unknown" : id;
        }
        catch
        {
            return "unknown";
        }
    }

    private static void SamplePopulation()
    {
        try
        {
            var alive = Singleton<GameWorld>.Instance?.AllAlivePlayersList;
            if (alive is null)
            {
                return;
            }

            if (alive.Count > _peakAlive)
            {
                _peakAlive = alive.Count;
            }

            for (var i = 0; i < alive.Count; i++)
            {
                var id = alive[i]?.ProfileId;
                if (!string.IsNullOrEmpty(id))
                {
                    SeenProfiles.Add(id!);
                }
            }
        }
        catch
        {
            // Population context is best-effort — never let it break the frame loop.
        }
    }

    private static void DumpStats()
    {
        try
        {
            var warmup = Plugin.LoadedConfig.Client.FrameStats.WarmupSkipSeconds;
            var stats = FrameStatsMath.Compute(_buffer!, _count, warmup);
            if (stats is null)
            {
                return;
            }

            var line = JsonConvert.SerializeObject(new
            {
                ts = _raidStartUtc.ToString("O"),
                modVersion = Plugin.ModVersion,
                masterEnabled = Plugin.LoadedConfig.MasterEnabled,
                map = _locationId,
                peakAlivePlayers = _peakAlive,
                uniquePlayersSeen = SeenProfiles.Count,
                warmupSkippedSec = warmup,
                frames = stats.Frames,
                durationSec = Math.Round(stats.DurationSec, 1),
                avgFps = Math.Round(stats.AvgFps, 1),
                onePercentLowFps = Math.Round(stats.OnePercentLowFps, 1),
                pointOnePercentLowFps = Math.Round(stats.PointOnePercentLowFps, 1),
                hitchesOver50Ms = stats.HitchesOver50Ms,
                hitchesOver100Ms = stats.HitchesOver100Ms,
                maxFrameMs = Math.Round(stats.MaxFrameMs, 1),
                maxFrameAtSec = Math.Round(stats.MaxFrameAtSec, 1),
                truncated = stats.Frames >= MaxSamples,
            });

            if (!string.IsNullOrEmpty(Plugin.SptUserLogsDir))
            {
                File.AppendAllText(
                    Path.Combine(Plugin.SptUserLogsDir!, "CompoundingPerf-framestats.jsonl"),
                    line + Environment.NewLine);
            }

            Plugin.Log?.LogInfo($"[C6] frame stats: {line}");
        }
        catch (Exception ex)
        {
            Plugin.Log?.LogWarning($"[C6] frame stats dump failed: {ex.Message}");
        }
    }
}
#endif
