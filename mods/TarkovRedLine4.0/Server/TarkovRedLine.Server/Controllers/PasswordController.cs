using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;
using SPTarkov.Server.Core.Servers;

namespace TarkovRedLine.Server.Controllers;

public class ChangeRequestData
{
    public string? username { get; set; }
    public string? password { get; set; }
    public string? change { get; set; }
}

[ApiController]
[Route("redline")]
public class PasswordController : ControllerBase
{
    // Usar o diretório base do executável do SPT para evitar problemas com atalhos
    private static readonly string ProfilesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "user", "profiles");
    private readonly SaveServer _saveServer;

    public PasswordController(SaveServer saveServer)
    {
        _saveServer = saveServer;
    }

    [HttpPost("password/change")]
    public IActionResult ChangePassword([FromBody] ChangeRequestData request)
    {
            if (string.IsNullOrEmpty(request?.username) || request?.change == null)
            {
                return Content("FAILED", "text/plain");
            }

            try
            {
                string debugLogPath = Path.Combine(Directory.GetCurrentDirectory(), "password_debug_log.txt");
                System.IO.File.AppendAllText(debugLogPath, $"--- New Change Password Request for {request.username} ---\n");

                if (!Directory.Exists(ProfilesPath)) 
                {
                    System.IO.File.AppendAllText(debugLogPath, $"[ERROR] Profiles directory not found at: {ProfilesPath}\n");
                    return Content("FAILED", "text/plain");
                }

                var files = Directory.GetFiles(ProfilesPath, "*.json");
                System.IO.File.AppendAllText(debugLogPath, $"Found {files.Length} JSON files in profiles directory.\n");
                
                bool userFound = false;

                foreach (var file in files)
                {
                    try 
                    {
                        var content = System.IO.File.ReadAllText(file);
                        var json = JsonNode.Parse(content);
                        
                        if (json != null && json["info"] != null)
                        {
                            var usernameNode = json["info"]["username"];
                            if (usernameNode != null)
                            {
                                string fileUsername = usernameNode.GetValue<string>();
                                System.IO.File.AppendAllText(debugLogPath, $"Checking file {Path.GetFileName(file)} -> username in file is: '{fileUsername}'\n");

                                if (string.Equals(fileUsername, request.username, StringComparison.OrdinalIgnoreCase))
                                {
                                    // Encontrou o perfil! Atualizar a senha
                                    json["info"]["password"] = request.change;
                                    
                                    var options = new JsonSerializerOptions { WriteIndented = true };
                                    System.IO.File.WriteAllText(file, json.ToJsonString(options));

                                    // Salvar também em um cofre separado porque o SPT 4.0 deleta senhas do info!
                                    try
                                    {
                                        string vaultPath = Path.Combine(ProfilesPath, "redline_passwords.json");
                                        JsonNode vault = null;
                                        if (System.IO.File.Exists(vaultPath)) {
                                            vault = JsonNode.Parse(System.IO.File.ReadAllText(vaultPath));
                                        }
                                        if (vault == null) vault = new JsonObject();
                                        vault[request.username] = request.change;
                                        System.IO.File.WriteAllText(vaultPath, vault.ToJsonString(options));
                                    }
                                    catch(Exception exVault)
                                    {
                                        System.IO.File.AppendAllText(debugLogPath, $"[WARNING] Failed to update vault: {exVault.Message}\n");
                                    }

                                    // Atualizar em memória também
                                    try
                                    {
                                        var profiles = _saveServer.GetProfiles();
                                        foreach (var kvp in profiles)
                                        {
                                            if (string.Equals(kvp.Value.ProfileInfo?.Username, request.username, StringComparison.OrdinalIgnoreCase))
                                            {
                                                if (kvp.Value.ProfileInfo.ExtensionData == null)
                                                {
                                                    kvp.Value.ProfileInfo.ExtensionData = new System.Collections.Generic.Dictionary<string, object>();
                                                }
                                                kvp.Value.ProfileInfo.ExtensionData["password"] = request.change;
                                                System.IO.File.AppendAllText(debugLogPath, $"SUCCESS! Password updated in MEMORY for {request.username}\n");
                                                break;
                                            }
                                        }
                                    }
                                    catch(Exception memEx)
                                    {
                                        System.IO.File.AppendAllText(debugLogPath, $"[WARNING] Failed to update memory: {memEx.Message}\n");
                                    }
                                    
                                    System.IO.File.AppendAllText(debugLogPath, $"SUCCESS! Password updated for {request.username} in file {Path.GetFileName(file)}\n");
                                    return Content("OK", "text/plain");
                                }
                            }
                            else
                            {
                                System.IO.File.AppendAllText(debugLogPath, $"File {Path.GetFileName(file)} does not have info.username node.\n");
                            }
                        }
                        else
                        {
                            System.IO.File.AppendAllText(debugLogPath, $"File {Path.GetFileName(file)} does not have info node.\n");
                        }
                    }
                    catch (Exception exInner)
                    {
                        System.IO.File.AppendAllText(debugLogPath, $"[ERROR] Skipping file {Path.GetFileName(file)} due to error: {exInner.Message}\n");
                    }
                }
                
                System.IO.File.AppendAllText(debugLogPath, $"[ERROR] User {request.username} not found in any profile file!\n");
            }
            catch (Exception ex)
            {
                string debugLogPath = Path.Combine(Directory.GetCurrentDirectory(), "password_debug_log.txt");
                System.IO.File.AppendAllText(debugLogPath, $"[CRITICAL ERROR] changing password: {ex.Message}\n{ex.StackTrace}\n");
            }

            return Content("FAILED", "text/plain");
    }

    [HttpPost("profile/get")]
    public IActionResult ProfileInfo([FromBody] ChangeRequestData request)
    {
        if (string.IsNullOrEmpty(request?.username)) return BadRequest();

        try
        {
            if (!Directory.Exists(ProfilesPath)) return NotFound();

            var files = Directory.GetFiles(ProfilesPath, "*.json");
            foreach (var file in files)
            {
                // Ignorar nosso arquivo de senhas
                if (file.EndsWith("redline_passwords.json")) continue;

                var content = System.IO.File.ReadAllText(file);
                var json = JsonNode.Parse(content);
                if (json != null && json["info"] != null)
                {
                    var usernameNode = json["info"]["username"];
                    if (usernameNode != null && string.Equals(usernameNode.GetValue<string>(), request.username, StringComparison.OrdinalIgnoreCase))
                    {
                        // Injetar a senha do cofre de volta no JSON antes de mandar pro Launcher!
                        try
                        {
                            string vaultPath = Path.Combine(ProfilesPath, "redline_passwords.json");
                            if (System.IO.File.Exists(vaultPath)) {
                                var vault = JsonNode.Parse(System.IO.File.ReadAllText(vaultPath));
                                if (vault != null && vault[request.username] != null) {
                                    json["info"]["password"] = vault[request.username].GetValue<string>();
                                }
                            }
                        } catch {}

                        // Retorna o bloco 'info' inteiro, que inclui a senha e atende ao AccountInfo do Launcher
                        return Content(json["info"].ToJsonString(), "application/json");
                    }
                }
            }
            return NotFound(new { error = "Profile not found" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
}
