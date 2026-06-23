using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;
using SPT.Launcher.Controllers;

namespace SPT.Launcher.Helpers
{
    public static class WireGuardHelper
    {


        public static async Task EnsureWireGuardConnected()
        {
            string wireguardPath = @"C:\Program Files\WireGuard\wireguard.exe";
            string wgPath = @"C:\Program Files\WireGuard\wg.exe";
            string wgConfigFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "RedLineLauncher", "WireGuard");
            string wgConfigFile = Path.Combine(wgConfigFolder, "redline.conf");

            if (!Directory.Exists(wgConfigFolder))
            {
                Directory.CreateDirectory(wgConfigFolder);
            }

            // 0. Limpar túneis WireGuard indesejados (manter apenas o "redline")
            try
            {
                var scOutput = RunCommandAndGetOutput("sc", "query state= all");
                var lines = scOutput.Split('\n');
                foreach (var line in lines)
                {
                    if (line.Contains("WireGuardTunnel$", StringComparison.OrdinalIgnoreCase))
                    {
                        // Extrai o nome do serviço ex: "NOME_DO_SERVIÇO: WireGuardTunnel$Tarkov_Red_Line"
                        int dollarIdx = line.IndexOf("WireGuardTunnel$", StringComparison.OrdinalIgnoreCase);
                        string tunnelName = line.Substring(dollarIdx + "WireGuardTunnel$".Length).Trim();
                        if (!tunnelName.Equals("redline", StringComparison.OrdinalIgnoreCase) && tunnelName.Length > 0)
                        {
                            LogManager.Instance.Info($"[Connect] Removendo túnel WireGuard indesejado: {tunnelName}");
                            try
                            {
                                var removeProcess = new Process
                                {
                                    StartInfo = new ProcessStartInfo
                                    {
                                        FileName = wireguardPath,
                                        Arguments = $"/uninstalltunnelservice {tunnelName}",
                                        UseShellExecute = true,
                                        Verb = "runas",
                                        CreateNoWindow = true
                                    }
                                };
                                removeProcess.Start();
                                removeProcess.WaitForExit();
                            }
                            catch (Exception ex)
                            {
                                LogManager.Instance.Warning($"[Connect] Falha ao remover túnel {tunnelName}: {ex.Message}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.Instance.Warning($"[Connect] Falha ao listar serviços WireGuard: {ex.Message}");
            }

            // Sempre registrar/atualizar na API antes de qualquer coisa (reutiliza chaves existentes)
            LogManager.Instance.Info("[Connect] Verificando registro VPN no servidor...");
            
            // 1. Instalar o WireGuard se não existir
            if (!File.Exists(wireguardPath))
            {
                LogManager.Instance.Info("[Connect] WireGuard not found. Extracting and installing silently...");
                try
                {
                    var assembly = System.Reflection.Assembly.GetEntryAssembly();
                    using var stream = assembly?.GetManifestResourceStream("WireGuardInstaller");
                    if (stream != null)
                    {
                        string tempPath = Path.Combine(Path.GetTempPath(), "wireguard_setup.msi");
                        using (var fileStream = File.Create(tempPath))
                        {
                            stream.CopyTo(fileStream);
                        }
                        var process = new Process
                        {
                            StartInfo = new ProcessStartInfo
                            {
                                FileName = "msiexec.exe",
                                Arguments = $"/i \"{tempPath}\" DO_NOT_LAUNCH=1 /qn /norestart",
                                UseShellExecute = true,
                                Verb = "runas"
                            }
                        };
                        process.Start();
                        process.WaitForExit();
                        LogManager.Instance.Info("[Connect] WireGuard installation finished.");
                    }
                }
                catch (Exception ex)
                {
                    LogManager.Instance.Error($"[Connect] Failed to install WireGuard: {ex.Message}");
                }
            }

            // 2. Reutilizar chaves existentes OU gerar novas
            string privateKey = null;
            string publicKey = null;

            if (File.Exists(wgConfigFile) && File.Exists(wgPath))
            {
                try
                {
                    var existingLines = File.ReadAllLines(wgConfigFile);
                    var privLine = existingLines.FirstOrDefault(l => l.TrimStart().StartsWith("PrivateKey"));
                    if (privLine != null)
                    {
                        privateKey = privLine.Split('=', 2)[1].Trim();
                        publicKey = RunCommandAndGetOutput(wgPath, "pubkey", privateKey).Trim();
                        LogManager.Instance.Info("[Connect] Reutilizando chaves WireGuard existentes.");
                    }
                }
                catch { }
            }

            if (privateKey == null && File.Exists(wgPath))
            {
                privateKey = RunCommandAndGetOutput(wgPath, "genkey").Trim();
                publicKey = RunCommandAndGetOutput(wgPath, "pubkey", privateKey).Trim();
                LogManager.Instance.Info("[Connect] Novas chaves WireGuard geradas.");
            }

            if (privateKey == null)
            {
                LogManager.Instance.Error("[Connect] wg.exe não encontrado. Não foi possível gerar chaves.");
                return;
            }

            // 3. SEMPRE registrar/atualizar na API (servidor atualiza a chave se necessário)
            string assignedIp = null;
            string serverPublicKey = null;
            try
            {
                var handler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
                };
                using var httpClient = new HttpClient(handler);
                httpClient.Timeout = TimeSpan.FromSeconds(10);
                string playerName = !string.IsNullOrEmpty(LauncherSettingsProvider.Instance.LastUsername) ? LauncherSettingsProvider.Instance.LastUsername : Environment.UserName;
                var requestData = new { publicKey = publicKey, username = playerName };

                string serverUrl = LauncherSettingsProvider.Instance.Server.Url.TrimEnd('/');
                string registerApiUrl = serverUrl.Replace("https://", "http://").Replace(":7073", ":7075") + "/api/vpn/register";

                LogManager.Instance.Info($"[Connect] Enviando chave pública para {registerApiUrl}...");
                var response = await httpClient.PostAsJsonAsync(registerApiUrl, requestData);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<WireGuardRegistrationResponse>();
                if (result == null || string.IsNullOrEmpty(result.AssignedIp))
                    throw new Exception("Resposta inválida da API de registro VPN.");

                assignedIp = result.AssignedIp;
                serverPublicKey = result.ServerPublicKey;
                LogManager.Instance.Info($"[Connect] VPN registrada! IP atribuído: {assignedIp}");
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error($"[Connect] Falha no registro VPN: {ex.Message}");
            }

            // 4. Atualizar o arquivo .conf e (re)instalar o túnel APENAS se necessário
            if (assignedIp != null)
            {
                string currentIp = GetWireGuardIp();
                string serverUrl2 = LauncherSettingsProvider.Instance.Server.Url.TrimEnd('/');
                string serverHostname = new Uri(serverUrl2).Host;

                string confContent = $"[Interface]\nPrivateKey = {privateKey}\nAddress = {assignedIp}/24\n\n[Peer]\nPublicKey = {serverPublicKey}\nAllowedIPs = 10.0.0.0/24\nEndpoint = {serverHostname}:7073\nPersistentKeepalive = 25";
                File.WriteAllText(wgConfigFile, confContent);

                bool needsReinstall = string.IsNullOrEmpty(currentIp) || currentIp != assignedIp;
                if (needsReinstall)
                {
                    LogManager.Instance.Info("[Connect] Instalando/atualizando serviço do túnel WireGuard...");
                    try
                    {
                        // Remove o serviço antigo se existir
                        var removeProcess = new Process { StartInfo = new ProcessStartInfo { FileName = wireguardPath, Arguments = "/uninstalltunnelservice redline", UseShellExecute = true, Verb = "runas", CreateNoWindow = true } };
                        removeProcess.Start();
                        removeProcess.WaitForExit();
                        await Task.Delay(1500);

                        var installProcess = new Process { StartInfo = new ProcessStartInfo { FileName = wireguardPath, Arguments = $"/installtunnelservice \"{wgConfigFile}\"", UseShellExecute = true, Verb = "runas", CreateNoWindow = true } };
                        installProcess.Start();
                        installProcess.WaitForExit();
                    }
                    catch (Exception ex)
                    {
                        LogManager.Instance.Error($"[Connect] Falha ao instalar serviço WireGuard: {ex.Message}");
                    }
                }
                else
                {
                    LogManager.Instance.Info($"[Connect] WireGuard já conectado com IP correto ({currentIp}). Nenhuma ação de túnel necessária.");
                }
            }

            // 5. Aguarda até 10 segundos para o adaptador subir
            bool adapterIsUp = false;
            for (int wait = 0; wait < 10; wait++)
            {
                if (!string.IsNullOrEmpty(GetWireGuardIp()))
                {
                    adapterIsUp = true;
                    break;
                }
                await Task.Delay(1000);
            }

            if (!adapterIsUp)
            {
                LogManager.Instance.Warning($"[Connect] WireGuard IP não encontrado.");
                return;
            }

            // 6. Configurar Firewall e Rede Privada automaticamente
            LogManager.Instance.Info("[Connect] Verificando regras de Firewall e Perfil de Rede...");
            try
            {
                bool needsFirewallUpdate = false;

                try
                {
                    // Check Network Category
                    string profileCheck = RunCommandAndGetOutput("powershell", "-NoProfile -Command \"(Get-NetConnectionProfile -InterfaceAlias 'redline' -ErrorAction SilentlyContinue).NetworkCategory\"");
                    if (!profileCheck.Contains("Private")) needsFirewallUpdate = true;

                    // Check Fika Firewall Rule
                    string fikaCheck = RunCommandAndGetOutput("netsh", "advfirewall firewall show rule name=\"Tarkov Fika P2P\"");
                    if (!fikaCheck.Contains("Tarkov Fika P2P")) needsFirewallUpdate = true;

                    // Check Ping Firewall Rule
                    string pingCheck = RunCommandAndGetOutput("netsh", "advfirewall firewall show rule name=\"Allow ICMPv4 WireGuard\"");
                    if (!pingCheck.Contains("Allow ICMPv4 WireGuard")) needsFirewallUpdate = true;
                }
                catch (Exception)
                {
                    // Se falhar a leitura silenciosa por falta de permissão, assumimos que precisa rodar o script
                    needsFirewallUpdate = true;
                }

                if (needsFirewallUpdate)
                {
                    LogManager.Instance.Info("[Connect] Regras ausentes. Solicitando permissão (UAC) para configurar Firewall...");
                    string psScript = @"
$interfaceAlias = 'redline'

# 1. Definir como Rede Privada
try {
    $profile = Get-NetConnectionProfile -InterfaceAlias $interfaceAlias -ErrorAction SilentlyContinue
    if ($profile -and $profile.NetworkCategory -ne 'Private') {
        Set-NetConnectionProfile -InterfaceAlias $interfaceAlias -NetworkCategory Private
    }
} catch { }

# 2. Liberar Porta do Fika (UDP 25565)
try {
    $fikaRule = Get-NetFirewallRule -DisplayName 'Tarkov Fika P2P' -ErrorAction SilentlyContinue
    if (-not $fikaRule) {
        New-NetFirewallRule -DisplayName 'Tarkov Fika P2P' -Direction Inbound -Action Allow -Protocol UDP -LocalPort 25565 -Profile Any -ErrorAction SilentlyContinue
    }
} catch { }

# 3. Liberar Ping (ICMPv4)
try {
    $pingRule = Get-NetFirewallRule -DisplayName 'Allow ICMPv4 WireGuard' -ErrorAction SilentlyContinue
    if (-not $pingRule) {
        New-NetFirewallRule -DisplayName 'Allow ICMPv4 WireGuard' -Direction Inbound -Action Allow -Protocol ICMPv4 -IcmpType 8 -Profile Any -ErrorAction SilentlyContinue
    }
} catch { }
";
                    string scriptPath = Path.Combine(Path.GetTempPath(), "wg_firewall_setup.ps1");
                    File.WriteAllText(scriptPath, psScript);
                    
                    var fwProcess = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = "powershell.exe",
                            Arguments = $"-ExecutionPolicy Bypass -NoProfile -WindowStyle Hidden -File \"{scriptPath}\"",
                            UseShellExecute = true,
                            Verb = "runas",
                            CreateNoWindow = true
                        }
                    };
                    fwProcess.Start();
                    fwProcess.WaitForExit();
                    
                    try { File.Delete(scriptPath); } catch { }
                }
                else
                {
                    LogManager.Instance.Info("[Connect] Rede Privada e regras de Firewall já estão configuradas. Pulando script de Administrador.");
                }
            }
            catch (Exception ex)
            {
                LogManager.Instance.Warning($"[Connect] Falha ao configurar Firewall: {ex.Message}");
            }
        }

        public static string GetWireGuardIp()
        {
            try
            {
                foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
                {
                    // A interface criada pelo wireguard costuma ter o nome do arquivo .conf, no caso "redline"
                    if (ni.OperationalStatus == OperationalStatus.Up && 
                       (ni.Name.Equals("redline", StringComparison.OrdinalIgnoreCase) || ni.Description.Contains("WireGuard", StringComparison.OrdinalIgnoreCase)))
                    {
                        foreach (var ip in ni.GetIPProperties().UnicastAddresses)
                        {
                            // IP da rede do WireGuard (10.0.0.x)
                            if (ip.Address.AddressFamily == AddressFamily.InterNetwork && ip.Address.ToString().StartsWith("10.0.0."))
                            {
                                return ip.Address.ToString();
                            }
                        }
                    }
                }
            }
            catch { }
            return null;
        }

        public static async Task ConfigureFikaAsync(string gamePath)
        {
            try
            {
                string fikaConfigPath = Path.Combine(gamePath, "BepInEx", "config", "com.fika.core.cfg");
                
                if (File.Exists(fikaConfigPath))
                {
                    string wgIp = GetWireGuardIp();

                    if (string.IsNullOrEmpty(wgIp))
                    {
                        LogManager.Instance.Error("[Connect] CRITICAL: Could not get WireGuard IP. Fika config not updated.");
                        return; 
                    }

                    LogManager.Instance.Info($"[Connect] Injecting Fika Auto-Config for P2P... WireGuard IP: {wgIp}");
                    
                    var lines = File.ReadAllLines(fikaConfigPath).ToList();
                    bool modified = false;

                    for (int i = 0; i < lines.Count; i++)
                    {
                        if (lines[i].StartsWith("Force Bind IP ="))
                        {
                            lines[i] = $"Force Bind IP = {wgIp}";
                            modified = true;
                        }
                        else if (lines[i].StartsWith("Force IP ="))
                        {
                            lines[i] = $"Force IP = {wgIp}";
                            modified = true;
                        }
                    }

                    if (modified)
                    {
                        File.WriteAllLines(fikaConfigPath, lines);
                        LogManager.Instance.Info("[Connect] Fika config updated successfully.");
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error($"[Connect] Failed to configure Fika: {ex.Message}");
            }
        }

        private static string RunCommandAndGetOutput(string fileName, string arguments, string standardInput = null)
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardInput = standardInput != null,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(processStartInfo);
            if (process == null) throw new Exception($"Could not start {fileName}");

            if (standardInput != null)
            {
                process.StandardInput.WriteLine(standardInput);
                process.StandardInput.Close();
            }

            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            return output;
        }

        // Classe de modelo para a resposta da API (Placeholder)
        public class WireGuardRegistrationResponse
        {
            public string AssignedIp { get; set; }
            public string ServerPublicKey { get; set; }
            public string ServerEndpoint { get; set; }
        }
    }
}
