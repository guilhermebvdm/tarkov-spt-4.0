using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;
using SPT.Launcher.Controllers;

namespace SPT.Launcher.Helpers
{
    public static class TailscaleHelper
    {
        public static async Task EnsureTailscaleConnected()
        {
            string tailscalePath = @"C:\Program Files\Tailscale\tailscale.exe";
            string tailscaleIpnPath = @"C:\Program Files\Tailscale\tailscale-ipn.exe";

            // Always disable Tailscale GUI Auto-Start on Windows boot to prevent browser login popup
            try
            {
                using (var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true))
                    key?.DeleteValue("Tailscale", false);
                using (var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true))
                    key?.DeleteValue("Tailscale", false);

                string startupFolder = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
                string tsShortcut = Path.Combine(startupFolder, "Tailscale.lnk");
                if (File.Exists(tsShortcut)) File.Delete(tsShortcut);
                
                string commonStartup = Environment.GetFolderPath(Environment.SpecialFolder.CommonStartup);
                string commonTsShortcut = Path.Combine(commonStartup, "Tailscale.lnk");
                if (File.Exists(commonTsShortcut)) File.Delete(commonTsShortcut);
            }
            catch { }

            for (int retry = 0; retry < 2; retry++)
            {
                LogManager.Instance.Info($"[Connect] Tailscale connection attempt {retry + 1}...");

                // 1. Fetch AuthKey dynamically FIRST
                string authKey = "";
                try
                {
                    using var httpClient = new System.Net.Http.HttpClient();
                    authKey = await httpClient.GetStringAsync("https://gist.githubusercontent.com/rockettechnology-dev/658aa44f55b3ee907f3e65f07664d112/raw/gistfile1.txt");
                    authKey = authKey.Trim();
                }
                catch (Exception ex)
                {
                    LogManager.Instance.Error($"[Connect] Failed to fetch Tailscale AuthKey: {ex.Message}");
                    authKey = "tskey-auth-kk2aP5b1GG11CNTRL-q87keuNrjfCdZinyiQhAgCr7f9nR2HSx"; // Fallback embutido
                    LogManager.Instance.Info("[Connect] Usando AuthKey de fallback embutida devido à falha de DNS.");
                }

                // 2. Install if not found
                bool justInstalled = false;
                if (!File.Exists(tailscalePath))
                {
                    LogManager.Instance.Info("[Connect] Tailscale not found. Extracting and installing silently...");
                    try
                    {
                        var assembly = System.Reflection.Assembly.GetEntryAssembly();
                        using var stream = assembly?.GetManifestResourceStream("TailscaleInstaller");
                        if (stream != null)
                        {
                            string tempPath = Path.Combine(Path.GetTempPath(), "tailscale_setup.msi");
                            using (var fileStream = File.Create(tempPath))
                            {
                                stream.CopyTo(fileStream);
                            }
                            
                            string arguments = $"/i \"{tempPath}\" /quiet /norestart";
                            if (!string.IsNullOrEmpty(authKey))
                            {
                                arguments += $" TS_AUTHKEY=\"{authKey}\"";
                            }

                            var process = new Process
                            {
                                StartInfo = new ProcessStartInfo
                                {
                                    FileName = "msiexec.exe",
                                    Arguments = arguments,
                                    UseShellExecute = true,
                                    Verb = "runas"
                                }
                            };
                            process.Start();
                            process.WaitForExit();
                            justInstalled = true;
                            LogManager.Instance.Info("[Connect] Tailscale installation finished.");
                            
                            // Aguarda o serviço (daemon) do Tailscale subir antes de tentar se comunicar via CLI
                            LogManager.Instance.Info("[Connect] Aguardando o serviço Tailscale iniciar...");
                            await Task.Delay(5000);
                        }
                    }
                    catch (Exception ex)
                    {
                        LogManager.Instance.Error($"[Connect] Failed to install Tailscale: {ex.Message}");
                    }
                }

                // 3. Force Tailscale UP and authenticate BEFORE opening GUI
                // We do this every time to intercept browser login and ensure connection is alive!
                if (File.Exists(tailscalePath) && !string.IsNullOrEmpty(authKey))
                {
                    LogManager.Instance.Info("[Connect] Connecting Tailscale via CLI with AuthKey...");
                    try
                    {
                        // Se o Tailscale-ipn estiver rodando, ele pode tentar abrir o browser. 
                        // Mas --authkey deve fazer o login invisivel.
                        var tsProcess = new Process
                        {
                            StartInfo = new ProcessStartInfo
                            {
                                FileName = tailscalePath,
                                Arguments = $"up --authkey={authKey} --unattended --reset --accept-dns=false --accept-routes=false",
                                UseShellExecute = false,
                                CreateNoWindow = true
                            }
                        };
                        tsProcess.Start();
                        tsProcess.WaitForExit();
                    }
                    catch (Exception ex)
                    {
                        LogManager.Instance.Error($"[Connect] Failed to run tailscale up: {ex.Message}");
                    }
                }

                // 4. Ensure GUI is running AFTER authentication
                if (File.Exists(tailscaleIpnPath))
                {
                    if (Process.GetProcessesByName("tailscale-ipn").Length == 0)
                    {
                        LogManager.Instance.Info("[Connect] Starting Tailscale GUI app...");
                        try
                        {
                            Process.Start(new ProcessStartInfo { FileName = tailscaleIpnPath, UseShellExecute = true });
                            await Task.Delay(2000); // Aguarda o app iniciar na bandeja
                        }
                        catch { }
                    }
                }

                // Wait up to 10 seconds for Tailscale to assign an IP
                for (int wait = 0; wait < 10; wait++)
                {
                    if (!string.IsNullOrEmpty(GetTailscaleIp()))
                    {
                        LogManager.Instance.Info("[Connect] Tailscale is connected with IP.");
                        return; // Sucesso!
                    }
                    await Task.Delay(1000);
                }
                
                LogManager.Instance.Warning($"[Connect] Tailscale IP not found after attempt {retry + 1}. Retrying process...");
            }
        }

        public static string GetTailscaleIp()
        {
            try
            {
                foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (ni.OperationalStatus == OperationalStatus.Up && ni.Description.Contains("Tailscale", StringComparison.OrdinalIgnoreCase))
                    {
                        foreach (var ip in ni.GetIPProperties().UnicastAddresses)
                        {
                            if (ip.Address.AddressFamily == AddressFamily.InterNetwork && ip.Address.ToString().StartsWith("100."))
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
                    string tailscaleIp = GetTailscaleIp();

                    if (string.IsNullOrEmpty(tailscaleIp))
                    {
                        LogManager.Instance.Error("[Connect] CRITICAL: Could not get Tailscale IP. Fika config not updated.");
                        return; // Do not apply 0.0.0.0
                    }

                    LogManager.Instance.Info($"[Connect] Injecting Fika Auto-Config for P2P... Tailscale IP: {tailscaleIp}");
                    
                    var lines = File.ReadAllLines(fikaConfigPath).ToList();
                    bool modified = false;

                    for (int i = 0; i < lines.Count; i++)
                    {
                        if (lines[i].StartsWith("Force Bind IP ="))
                        {
                            lines[i] = $"Force Bind IP = {tailscaleIp}";
                            modified = true;
                        }
                        else if (lines[i].StartsWith("Force IP ="))
                        {
                            lines[i] = $"Force IP = {tailscaleIp}";
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
    }
}
