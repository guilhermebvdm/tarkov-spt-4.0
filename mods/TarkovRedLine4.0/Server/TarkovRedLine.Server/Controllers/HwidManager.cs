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
    private static readonly string ProfilesPath = Path.Combine(Directory.GetCurrentDirectory(), "user", "profiles");
    private readonly SPTarkov.Server.Core.Servers.SaveServer _saveServer;

    public HwidManagerController(SPTarkov.Server.Core.Servers.SaveServer saveServer)
    {
        _saveServer = saveServer;
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

        string debugLogPath = Path.Combine(Directory.GetCurrentDirectory(), "hwid_debug_log.txt");

        try
        {
            var files = Directory.GetFiles(ProfilesPath, "*.json");
            foreach (var file in files)
            {
                var content = System.IO.File.ReadAllText(file);
                var json = System.Text.Json.Nodes.JsonNode.Parse(content);

                if (json?["info"]?["username"] != null)
                {
                    string fileUsername = json["info"]["username"].GetValue<string>();
                    if (string.Equals(fileUsername, request.username, StringComparison.OrdinalIgnoreCase))
                    {
                        var hwidNode = json["info"]["hwid"];
                        if (hwidNode != null && !string.IsNullOrEmpty(hwidNode.GetValue<string>()))
                        {
                            return Ok(new { status = "ALREADY_REGISTERED" });
                        }

                        // Registra o HWID no json
                        json["info"]["hwid"] = request.hwid;
                        var options = new JsonSerializerOptions { WriteIndented = true };
                        System.IO.File.WriteAllText(file, json.ToJsonString(options));

                        // Atualiza na memória
                        try
                        {
                            var profiles = _saveServer.GetProfiles();
                            foreach (var kvp in profiles)
                            {
                                if (string.Equals(kvp.Value.ProfileInfo?.Username, request.username, StringComparison.OrdinalIgnoreCase))
                                {
                                    if (kvp.Value.ProfileInfo.ExtensionData == null)
                                    {
                                        kvp.Value.ProfileInfo.ExtensionData = new Dictionary<string, object>();
                                    }
                                    kvp.Value.ProfileInfo.ExtensionData["hwid"] = request.hwid;
                                    break;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            System.IO.File.AppendAllText(debugLogPath, $"Failed to update memory for HWID: {ex.Message}\n");
                        }

                        return Ok(new { status = "OK" });
                    }
                }
            }
        }
        catch (Exception ex)
        {
            System.IO.File.AppendAllText(debugLogPath, $"[ERROR] HWID Register: {ex.Message}\n");
        }

        return Ok(new { status = "OK" }); // Retorna OK mesmo se não achar perfil ainda, o perfil pode ser criado depois
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
            var files = Directory.GetFiles(ProfilesPath, "*.json");
            foreach (var file in files)
            {
                var content = System.IO.File.ReadAllText(file);
                var json = System.Text.Json.Nodes.JsonNode.Parse(content);

                if (json?["info"]?["username"] != null)
                {
                    string fileUsername = json["info"]["username"].GetValue<string>();
                    if (string.Equals(fileUsername, request.username, StringComparison.OrdinalIgnoreCase))
                    {
                        var hwidNode = json["info"]["hwid"];
                        if (hwidNode == null || string.IsNullOrEmpty(hwidNode.GetValue<string>()))
                        {
                            return StatusCode(403, new { status = "NO_HWID_REGISTERED" });
                        }

                        string storedHwid = hwidNode.GetValue<string>();
                        if (!string.Equals(storedHwid, request.hwid, StringComparison.OrdinalIgnoreCase))
                        {
                            return StatusCode(403, new { status = "HWID_MISMATCH" });
                        }

                        return Ok(new { status = "OK" });
                    }
                }
            }
        }
        catch (Exception)
        {
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
