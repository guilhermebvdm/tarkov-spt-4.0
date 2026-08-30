using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text.Json;
using System.Threading;
using Microsoft.AspNetCore.Mvc;

namespace TarkovRedLine.Server.Controllers;

public class BandwidthConfig
{
    public int TotalUploadMbps { get; set; } = 100;
    public int IdlePercent { get; set; } = 85;
    public int RaidActivePercent { get; set; } = 40;
}

public static class BandwidthConfigManager
{
    private static BandwidthConfig _cachedConfig = new();
    private static long _lastReadTime = 0;

    public static BandwidthConfig GetConfig()
    {
        long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        if (now - _lastReadTime < 5)
        {
            return _cachedConfig;
        }

        _lastReadTime = now;
        try
        {
            string configPath = Path.Combine(LauncherUpdaterController.GetUpdaterBasePath(), "bandwidth.json");
            if (File.Exists(configPath))
            {
                string json = File.ReadAllText(configPath);
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var loaded = JsonSerializer.Deserialize<BandwidthConfig>(json, options);
                if (loaded != null)
                {
                    _cachedConfig = loaded;
                }
            }
        }
        catch
        {
            // Silencioso: mantém o fallback seguro padrão
        }

        return _cachedConfig;
    }
}

public static class RaidActivityTracker
{
    private static readonly ConcurrentDictionary<string, long> _activeRaids = new();
    private static readonly Timer _watchdogTimer;

    static RaidActivityTracker()
    {
        // Watchdog limpa sessões inativas há mais de 60s a cada 15 segundos (CR-01-03 / CR-02-02)
        _watchdogTimer = new Timer(_ => CleanupInactive(), null, 15000, 15000);
    }

    public static void RecordActivity(string sessionId)
    {
        if (string.IsNullOrWhiteSpace(sessionId)) return;
        _activeRaids[sessionId] = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }

    public static void EndRaid(string sessionId)
    {
        if (string.IsNullOrWhiteSpace(sessionId)) return;
        _activeRaids.TryRemove(sessionId, out _);
    }

    public static int GetActiveRaidCount()
    {
        return _activeRaids.Count;
    }

    private static void CleanupInactive()
    {
        try
        {
            long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            foreach (var kvp in _activeRaids)
            {
                if (now - kvp.Value > 60000)
                {
                    _activeRaids.TryRemove(kvp.Key, out _);
                }
            }
        }
        catch
        {
            // Evitar vazamento de exceção em timer em background
        }
    }
}

[ApiController]
[Route(ModRouting.RoutePrefix + "redline/server")]
public class ServerBandwidthController : ControllerBase
{
    [HttpGet("bandwidth-status")]
    public IActionResult GetBandwidthStatus()
    {
        var config = BandwidthConfigManager.GetConfig();
        int activeRaids = RaidActivityTracker.GetActiveRaidCount();
        bool inRaid = activeRaids > 0;

        double percent = inRaid ? (config.RaidActivePercent / 100.0) : (config.IdlePercent / 100.0);
        int maxBytesSec = (int)Math.Floor((config.TotalUploadMbps * 1024 * 1024 / 8.0) * percent);
        double maxMBps = Math.Round(maxBytesSec / (1024.0 * 1024.0), 1);

        return Ok(new
        {
            inRaid = inRaid,
            activeRaids = activeRaids,
            maxDownloadRateBytesSec = maxBytesSec,
            maxDownloadRateMBps = maxMBps,
            mode = inRaid ? "RaidActive" : "Turbo"
        });
    }
}
