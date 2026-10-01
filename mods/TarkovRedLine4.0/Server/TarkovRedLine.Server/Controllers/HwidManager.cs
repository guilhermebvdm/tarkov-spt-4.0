using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using SPTarkov.Server.Core.Servers;

namespace TarkovRedLine.Server.Controllers;

[ApiController]
[Route(ModRouting.RoutePrefix + "launcher/hwid")]
public class HwidManagerController : ControllerBase
{
    private readonly SaveServer _saveServer;

    public HwidManagerController(SaveServer saveServer)
    {
        _saveServer = saveServer;
    }

    public class HwidRequest
    {
        public string username { get; set; }
        public string hwid { get; set; }
    }

    [HttpPost("register")]
    public async System.Threading.Tasks.Task<IActionResult> Register([FromBody] HwidRequest request)
    {
        if (string.IsNullOrEmpty(request?.username) || string.IsNullOrEmpty(request?.hwid))
        {
            return BadRequest(new { status = "INVALID_REQUEST" });
        }

        try
        {
            var profiles = _saveServer.GetProfiles().ToArray();
            foreach (var kvp in profiles)
            {
                var profile = kvp.Value;
                if (string.Equals(profile?.ProfileInfo?.Username, request.username, StringComparison.OrdinalIgnoreCase))
                {
                    var extData = profile.ProfileInfo.ExtensionData ??= new Dictionary<string, object>();
                    if (extData.TryGetValue("hwid", out var existingHwid) && 
                        !string.IsNullOrWhiteSpace(existingHwid?.ToString()))
                    {
                        return Ok(new { status = "ALREADY_REGISTERED" });
                    }

                    extData["hwid"] = request.hwid;
                    await _saveServer.SaveProfileAsync(kvp.Key);
                    return Ok(new { status = "OK" });
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[HwidManager] Erro ao registrar HWID: {ex.Message}");
        }

        return Ok(new { status = "OK" });
    }

    [HttpPost("reset-password")]
    public IActionResult ResetPassword([FromBody] HwidRequest request)
    {
        if (string.IsNullOrEmpty(request?.username) || string.IsNullOrEmpty(request?.hwid))
        {
            return BadRequest(new { status = "INVALID_REQUEST" });
        }

        try
        {
            var profiles = _saveServer.GetProfiles().ToArray();
            foreach (var kvp in profiles)
            {
                var profile = kvp.Value;
                if (string.Equals(profile?.ProfileInfo?.Username, request.username, StringComparison.OrdinalIgnoreCase))
                {
                    var extData = profile.ProfileInfo?.ExtensionData;
                    if (extData == null || !extData.TryGetValue("hwid", out var existingHwid) || 
                        string.IsNullOrWhiteSpace(existingHwid?.ToString()))
                    {
                        return StatusCode(403, new { status = "NO_HWID_REGISTERED" });
                    }

                    string storedHwid = existingHwid.ToString()!;
                    if (!string.Equals(storedHwid, request.hwid, StringComparison.OrdinalIgnoreCase))
                    {
                        return StatusCode(403, new { status = "HWID_MISMATCH" });
                    }

                    return Ok(new { status = "OK" });
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[HwidManager] Erro ao validar HWID para reset: {ex.Message}");
            return StatusCode(500, new { status = "SERVER_ERROR" });
        }

        return StatusCode(403, new { status = "NO_HWID_REGISTERED" });
    }

    [HttpGet("version")]
    public IActionResult GetVersion()
    {
        return Ok(new { version = "1.0.0" });
    }
}
