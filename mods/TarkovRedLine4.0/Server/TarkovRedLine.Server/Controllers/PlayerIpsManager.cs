using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace TarkovRedLine.Server.Controllers;

[ApiController]
[Route(ModRouting.RoutePrefix + "redline")]
public class PlayerIpsManagerController : ControllerBase
{
    private static readonly string PlayerIpsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "user", $"player_ips{ModRouting.StateSuffix}.json");
    private static Dictionary<string, PlayerIpEntry> _playerIps = new();
    private static bool _loaded = false;
    private static readonly object _lock = new();

    public class PlayerIpEntry
    {
        public string ip { get; set; }
        public string lastSeen { get; set; }
    }

    public class RegisterIpRequest
    {
        public string username { get; set; }
        public string ip { get; set; }
    }

    public PlayerIpsManagerController()
    {
        LoadPlayerIps();
    }

    private void LoadPlayerIps()
    {
        if (_loaded) return;
        lock (_lock)
        {
            if (_loaded) return;
            try
            {
                if (System.IO.File.Exists(PlayerIpsPath))
                {
                    var json = System.IO.File.ReadAllText(PlayerIpsPath);
                    _playerIps = JsonSerializer.Deserialize<Dictionary<string, PlayerIpEntry>>(json) ?? new Dictionary<string, PlayerIpEntry>();
                }
            }
            catch
            {
                // Ignorar erro
            }
            finally
            {
                _loaded = true;
            }
        }
    }

    private void SavePlayerIps()
    {
        lock (_lock)
        {
            try
            {
                var dir = Path.GetDirectoryName(PlayerIpsPath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                var json = JsonSerializer.Serialize(_playerIps, new JsonSerializerOptions { WriteIndented = true });
                System.IO.File.WriteAllText(PlayerIpsPath, json);
            }
            catch
            {
                // Ignorar
            }
        }
    }

    // Canônico = "register-player-ip" (o que o launcher chama, RequestHandler.cs:163, e o mod TS de prod).
    // "register-ip" mantido como alias por retrocompat.
    [HttpPost("register-player-ip")]
    [HttpPost("register-ip")]
    public IActionResult RegisterIp([FromBody] RegisterIpRequest request)
    {
        if (string.IsNullOrEmpty(request?.username) || string.IsNullOrEmpty(request?.ip))
        {
            return BadRequest(new { status = "INVALID_REQUEST" });
        }

        var timeZone = TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");
        var localTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone);
        var lastSeen = localTime.ToString("dd/MM/yyyy HH:mm:ss");

        lock (_lock)
        {
            _playerIps[request.username] = new PlayerIpEntry { ip = request.ip, lastSeen = lastSeen };
            SavePlayerIps();
        }

        return Ok(new { status = "OK" });
    }

    [HttpGet("player-ips")]
    public IActionResult GetList()
    {
        return Ok(_playerIps);
    }
}
