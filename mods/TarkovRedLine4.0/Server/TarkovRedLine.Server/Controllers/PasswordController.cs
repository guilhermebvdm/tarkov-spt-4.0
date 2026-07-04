using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    private static string VaultPath => Path.Combine(ProfilesPath, "redline_passwords.json");
    private readonly SaveServer _saveServer;

    public PasswordController(SaveServer saveServer)
    {
        _saveServer = saveServer;
    }

    /// <summary>
    /// D4: lookup do cofre case-insensitive, consistente com o match de profile
    /// (cobre chaves novas em lowercase e chaves legadas com casing original).
    /// Retorna null quando não há entrada.
    /// </summary>
    private static string? GetVaultPassword(string username)
    {
        try
        {
            if (!System.IO.File.Exists(VaultPath)) return null;
            if (JsonNode.Parse(System.IO.File.ReadAllText(VaultPath)) is not JsonObject vault) return null;

            foreach (var entry in vault)
            {
                if (string.Equals(entry.Key, username, StringComparison.OrdinalIgnoreCase))
                {
                    return entry.Value?.GetValue<string>();
                }
            }
        }
        catch
        {
            // cofre ilegível = sem entrada
        }

        return null;
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

                // D2: cofre já tem senha não-vazia → exigir a senha atual correta (troca livre
                // só quando não há senha). Server-only e compatível com o launcher atual: todos
                // os fluxos enviam request.password com a senha real obtida no login via
                // /redline/profile/get (inclusive o reset HWID, que loga antes de zerar).
                string? currentVaultPassword = GetVaultPassword(request.username);
                if (!string.IsNullOrEmpty(currentVaultPassword) && request.password != currentVaultPassword)
                {
                    System.IO.File.AppendAllText(debugLogPath, $"[DENIED] Wrong current password for {request.username}\n");
                    return Content("FAILED", "text/plain");
                }

                var files = Directory.GetFiles(ProfilesPath, "*.json");
                System.IO.File.AppendAllText(debugLogPath, $"Found {files.Length} JSON files in profiles directory.\n");

                foreach (var file in files)
                {
                    // Ignorar nosso arquivo de senhas (não é profile)
                    if (file.EndsWith("redline_passwords.json")) continue;

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
                                        JsonNode vault = null;
                                        if (System.IO.File.Exists(VaultPath)) {
                                            vault = JsonNode.Parse(System.IO.File.ReadAllText(VaultPath));
                                        }
                                        if (vault == null) vault = new JsonObject();

                                        // D4: chave normalizada (lowercase invariant), consistente com o
                                        // match case-insensitive do profile; remove duplicatas legadas
                                        // com casing diferente (migração lazy).
                                        if (vault is JsonObject vaultObj)
                                        {
                                            List<string> legacyKeys = vaultObj
                                                .Where(entry => string.Equals(entry.Key, request.username, StringComparison.OrdinalIgnoreCase))
                                                .Select(entry => entry.Key)
                                                .ToList();

                                            foreach (var legacyKey in legacyKeys)
                                            {
                                                vaultObj.Remove(legacyKey);
                                            }
                                        }

                                        vault[request.username.ToLowerInvariant()] = request.change;
                                        System.IO.File.WriteAllText(VaultPath, vault.ToJsonString(options));
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
                        // D4: lookup case-insensitive (chaves novas em lowercase, legadas com casing original)
                        try
                        {
                            string? vaultPassword = GetVaultPassword(request.username);
                            if (vaultPassword != null) {
                                json["info"]["password"] = vaultPassword;
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
