using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace TarkovRedLine.Server.Controllers;

[ApiController]
[Route("launcher/hwid")]
public class HwidManagerController : ControllerBase
{
    private static readonly string HwidRegistryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "user", "hwid_registry.json");
    private static Dictionary<string, string> _hwidRegistry = new();
    private static bool _loaded = false;
    private static readonly object _lock = new();

    public HwidManagerController()
    {
        LoadHwidRegistry();
    }

    private void LoadHwidRegistry()
    {
        if (_loaded) return;
        lock (_lock)
        {
            if (_loaded) return;
            try
            {
                if (System.IO.File.Exists(HwidRegistryPath))
                {
                    var json = System.IO.File.ReadAllText(HwidRegistryPath);
                    _hwidRegistry = JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? new Dictionary<string, string>();
                }
            }
            catch
            {
                // Ignorar erros de parse iniciais
            }
            finally
            {
                _loaded = true;
            }
        }
    }

    private void SaveHwidRegistry()
    {
        lock (_lock)
        {
            try
            {
                var dir = Path.GetDirectoryName(HwidRegistryPath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                var json = JsonSerializer.Serialize(_hwidRegistry, new JsonSerializerOptions { WriteIndented = true });
                System.IO.File.WriteAllText(HwidRegistryPath, json);
            }
            catch
            {
                // Ignorar erro de salvamento
            }
        }
    }

    public class HwidRequest
    {
        public string username { get; set; }
        public string hwid { get; set; }
    }

    [HttpPost("register")]
    public IActionResult Register([FromBody] HwidRequest request)
    {
        if (string.IsNullOrEmpty(request?.username) || string.IsNullOrEmpty(request?.hwid))
        {
            return BadRequest(new { status = "INVALID_REQUEST" });
        }

        var lowerUser = request.username.ToLowerInvariant();
        if (_hwidRegistry.ContainsKey(lowerUser))
        {
            return Ok(new { status = "ALREADY_REGISTERED" });
        }

        _hwidRegistry[lowerUser] = request.hwid;
        SaveHwidRegistry();
        return Ok(new { status = "OK" });
    }

    [HttpPost("reset-password")]
    public IActionResult ResetPassword([FromBody] HwidRequest request)
    {
        if (string.IsNullOrEmpty(request?.username) || string.IsNullOrEmpty(request?.hwid))
        {
            return BadRequest(new { status = "INVALID_REQUEST" });
        }

        var lowerUser = request.username.ToLowerInvariant();
        if (!_hwidRegistry.TryGetValue(lowerUser, out var storedHwid))
        {
            return StatusCode(403, new { status = "NO_HWID_REGISTERED" });
        }

        if (storedHwid != request.hwid)
        {
            return StatusCode(403, new { status = "HWID_MISMATCH" });
        }

        return Ok(new { status = "OK" });
    }

    [HttpGet("version")]
    public IActionResult GetVersion()
    {
        return Ok(new { version = "1.0.0" });
    }
}
