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
[Route(ModRouting.RoutePrefix + "redline")]
public class PasswordController : ControllerBase
{
    // Usar o diretório base do executável do SPT para evitar problemas com atalhos
    private static readonly string ProfilesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "user", "profiles");

    // Nomes de arquivos PRÓPRIOS do mod, sufixados no build homolog (ModRouting.StateSuffix) para não
    // colidir com os do build de produção rodando no mesmo processo. Const em tempo de compilação
    // (concatenação de const string), reusados no VaultPath E nos EndsWith que auto-excluem o cofre do
    // scan de profiles — assim o homolog exclui o SEU cofre, não o de prod.
    private const string VaultFileName = "redline_passwords" + ModRouting.StateSuffix + ".json";
    private const string DebugLogFileName = "password_debug_log" + ModRouting.StateSuffix + ".txt";

    private static string VaultPath => Path.Combine(ProfilesPath, VaultFileName);
    private readonly SaveServer _saveServer;

    public PasswordController(SaveServer saveServer)
    {
        _saveServer = saveServer;
    }

    // ── Item 020 (DP-020.A = A2): critério ÚNICO de igualdade de username do cofre ──
    // Espelha SPT.Launcher.Base/Helpers/VaultKeyMatcher (assemblies separados não compartilham
    // código; a suíte .Tests referencia só o .Base). A2 = chave canônica lowercase + match
    // case-insensitive, comportamento PRESERVADO do fix D4 — a colisão Bob/bob é barrada na ORIGEM
    // pelo launcher (bloqueio no registro), não aqui. Aqui só unificamos o critério espalhado
    // (antes: ToLowerInvariant na chave + OrdinalIgnoreCase no match, em ~6 pontos).

    /// <summary>Chave canônica do cofre: lowercase invariant. null/vazio/whitespace → "".</summary>
    private static string CanonicalKey(string username)
        => string.IsNullOrWhiteSpace(username) ? string.Empty : username.ToLowerInvariant();

    /// <summary>true quando dois usernames são "o mesmo usuário" sob o critério do cofre. Vazios nunca casam.</summary>
    private static bool UsernameMatches(string a, string b)
    {
        string ca = CanonicalKey(a);
        return ca.Length > 0 && string.Equals(ca, CanonicalKey(b), StringComparison.Ordinal);
    }

    /// <summary>
    /// CC-5 — escrita ATÔMICA do cofre (temp + move com overwrite): clientes coop concorrentes não
    /// podem observar nem produzir um JSON truncado. Não serializa writers (last-writer-wins), mas
    /// nunca corrompe o arquivo (era File.WriteAllText direto).
    /// </summary>
    private static void WriteVaultAtomic(string json)
    {
        string tmp = VaultPath + ".tmp";
        System.IO.File.WriteAllText(tmp, json);
        System.IO.File.Move(tmp, VaultPath, overwrite: true);
    }

    /// <summary>
    /// Item 020 A4/DP-020.C — reconciliação de órfãos do cofre. Enumera o redline_passwords.json,
    /// cruza com os perfis vivos (info.username) e remove chaves sem perfil correspondente (mesmo
    /// critério de casing). Quando <paramref name="explicitDeleteUsername"/> != null, remove também a
    /// chave desse username (cujo perfil já pode ter sido apagado pelo core no delete/wipe). Escrita
    /// atômica. FAIL-SAFE: só varre órfãos se conseguiu ler os perfis SEM erro — nunca apaga a senha de
    /// um perfil vivo por causa de uma leitura falha (R-3). Idempotente: roda de novo e converge.
    /// ref DP-020.C — só no delete/wipe; SEM sweep no boot do server.
    /// </summary>
    private static void ReconcileVault(string explicitDeleteUsername = null)
    {
        if (!System.IO.File.Exists(VaultPath)) return;

        JsonObject vault;
        try
        {
            if (JsonNode.Parse(System.IO.File.ReadAllText(VaultPath)) is not JsonObject parsed) return; // corrompido → não tocar
            vault = parsed;
        }
        catch { return; }

        string debugLogPath = Path.Combine(Directory.GetCurrentDirectory(), DebugLogFileName);

        // Conjunto de usernames vivos (canônicos), a partir dos arquivos de perfil.
        var liveUsers = new HashSet<string>(StringComparer.Ordinal);
        bool profilesReadClean = true;

        if (Directory.Exists(ProfilesPath))
        {
            foreach (var file in Directory.GetFiles(ProfilesPath, "*.json"))
            {
                if (file.EndsWith(VaultFileName)) continue;

                try
                {
                    var json = JsonNode.Parse(System.IO.File.ReadAllText(file));
                    var u = json?["info"]?["username"]?.GetValue<string>();
                    if (!string.IsNullOrEmpty(u)) liveUsers.Add(CanonicalKey(u));
                }
                catch
                {
                    profilesReadClean = false; // leitura suja → não varrer órfãos neste run (fail-safe)
                }
            }
        }
        else
        {
            profilesReadClean = false; // sem diretório de perfis → não arriscar apagar tudo
        }

        var keysToRemove = new List<string>();
        foreach (var entry in vault)
        {
            bool isExplicitTarget = explicitDeleteUsername != null && UsernameMatches(entry.Key, explicitDeleteUsername);
            bool isOrphan = profilesReadClean && !liveUsers.Contains(CanonicalKey(entry.Key));

            if (isExplicitTarget || isOrphan)
            {
                keysToRemove.Add(entry.Key);
            }
        }

        if (keysToRemove.Count == 0) return;

        foreach (var key in keysToRemove)
        {
            bool explicitTarget = explicitDeleteUsername != null && UsernameMatches(key, explicitDeleteUsername);
            vault.Remove(key);
            try { System.IO.File.AppendAllText(debugLogPath, $"[Vault] Removed key '{key}' (explicitDelete={explicitTarget})\n"); } catch { }
        }

        WriteVaultAtomic(vault.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
    }

    /// <summary>
    /// D4: lookup do cofre case-insensitive, consistente com o match de profile.
    /// ref: CR-01-01 — prioridade de leitura quando há duplicatas legadas de casing:
    /// 1) chave canônica lowercase (formato pós-D4 = sempre a escrita mais recente),
    /// 2) match exato de casing, 3) qualquer match case-insensitive. Sem isso, a
    /// primeira duplicata em ordem de documento sombrearia a senha mais recente.
    /// ref: CR-01-02 — retorna false em erro de leitura (cofre corrompido/IO), para o
    /// gate D2 poder ser fail-closed: "erro lendo o cofre" ≠ "não há senha".
    /// </summary>
    private static bool TryGetVaultPassword(string username, out string? password)
    {
        password = null;

        try
        {
            // Sem arquivo = sem entrada (estado legítimo, não é erro)
            if (!System.IO.File.Exists(VaultPath)) return true;

            // Raiz que não é objeto = cofre corrompido
            if (JsonNode.Parse(System.IO.File.ReadAllText(VaultPath)) is not JsonObject vault) return false;

            string canonicalKey = CanonicalKey(username);
            string? exactMatch = null, looseMatch = null;
            bool hasExact = false, hasLoose = false;

            foreach (var entry in vault)
            {
                if (!UsernameMatches(entry.Key, username)) continue;

                string? value = entry.Value?.GetValue<string>();

                if (string.Equals(entry.Key, canonicalKey, StringComparison.Ordinal))
                {
                    password = value; // prioridade 1 — encerra a busca
                    return true;
                }

                if (!hasExact && string.Equals(entry.Key, username, StringComparison.Ordinal))
                {
                    exactMatch = value;
                    hasExact = true;
                }
                else if (!hasLoose)
                {
                    looseMatch = value;
                    hasLoose = true;
                }
            }

            if (hasExact) password = exactMatch;
            else if (hasLoose) password = looseMatch;

            return true;
        }
        catch
        {
            // Erro de leitura/parse/valor não-string → sinalizar erro (fail-closed no gate)
            return false;
        }
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
                string debugLogPath = Path.Combine(Directory.GetCurrentDirectory(), DebugLogFileName);
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
                // ref: CR-01-02 — fail-closed: erro lendo o cofre nega a troca em vez de
                // tratar como "sem senha" e liberar a troca cega que o gate existe pra impedir.
                if (!TryGetVaultPassword(request.username, out string? currentVaultPassword))
                {
                    System.IO.File.AppendAllText(debugLogPath, $"[DENIED] Vault unreadable while changing password for {request.username}\n");
                    return Content("FAILED", "text/plain");
                }

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
                    if (file.EndsWith(VaultFileName)) continue;

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

                                if (UsernameMatches(fileUsername, request.username))
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
                                                .Where(entry => UsernameMatches(entry.Key, request.username))
                                                .Select(entry => entry.Key)
                                                .ToList();

                                            foreach (var legacyKey in legacyKeys)
                                            {
                                                vaultObj.Remove(legacyKey);
                                            }
                                        }

                                        vault[CanonicalKey(request.username)] = request.change;
                                        // CC-5: escrita atômica (temp+move) — coop concorrente não corrompe o cofre.
                                        WriteVaultAtomic(vault.ToJsonString(options));
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
                                            if (UsernameMatches(kvp.Value.ProfileInfo?.Username, request.username))
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
                string debugLogPath = Path.Combine(Directory.GetCurrentDirectory(), DebugLogFileName);
                System.IO.File.AppendAllText(debugLogPath, $"[CRITICAL ERROR] changing password: {ex.Message}\n{ex.StackTrace}\n");
            }

            return Content("FAILED", "text/plain");
    }

    /// <summary>
    /// Item 020 (A4/BR-020.3/BR-020.4) — APAGA a chave do username no cofre (nunca grava vazio) e
    /// reconcilia órfãos no mesmo passo (DP-020.C: só no delete/wipe). Chamado pelo launcher DEPOIS
    /// de um remove/wipe bem-sucedido no core. Idempotente: username inexistente responde "OK".
    /// </summary>
    [HttpPost("password/delete")]
    public IActionResult DeletePassword([FromBody] ChangeRequestData request)
    {
        if (string.IsNullOrEmpty(request?.username))
        {
            return Content("FAILED", "text/plain");
        }

        try
        {
            ReconcileVault(request.username);
            return Content("OK", "text/plain");
        }
        catch (Exception ex)
        {
            try
            {
                string debugLogPath = Path.Combine(Directory.GetCurrentDirectory(), DebugLogFileName);
                System.IO.File.AppendAllText(debugLogPath, $"[ERROR] deleting vault entry for {request.username}: {ex.Message}\n");
            }
            catch { }
            return Content("FAILED", "text/plain");
        }
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
                if (file.EndsWith(VaultFileName)) continue;

                var content = System.IO.File.ReadAllText(file);
                var json = JsonNode.Parse(content);
                if (json != null && json["info"] != null)
                {
                    var usernameNode = json["info"]["username"];
                    if (usernameNode != null && UsernameMatches(usernameNode.GetValue<string>(), request.username))
                    {
                        // Injetar a senha do cofre de volta no JSON antes de mandar pro Launcher!
                        // D4/CR-01-01: lookup priorizado (canônica lowercase > casing exato > case-insensitive).
                        // Aqui a falha de leitura segue best-effort (sem injeção) — o fail-closed
                        // do CR-01-02 é só no gate do change, onde há decisão de segurança.
                        try
                        {
                            if (TryGetVaultPassword(request.username, out string? vaultPassword) && vaultPassword != null) {
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
