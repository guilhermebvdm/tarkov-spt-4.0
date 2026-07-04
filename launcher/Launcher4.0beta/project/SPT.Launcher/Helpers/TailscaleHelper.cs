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
        private const string TailscalePath = @"C:\Program Files\Tailscale\tailscale.exe";
        private const string TailscaleIpnPath = @"C:\Program Files\Tailscale\tailscale-ipn.exe";
        private const string AuthKeyGistUrl = "https://gist.githubusercontent.com/rockettechnology-dev/658aa44f55b3ee907f3e65f07664d112/raw/gistfile1.txt";
        private const string FallbackAuthKey = "tskey-auth-kk2aP5b1GG11CNTRL-q87keuNrjfCdZinyiQhAgCr7f9nR2HSx"; // Fallback embutido

        /// <summary>
        /// Ensures Tailscale is installed, authenticated (via authkey, never browser) and connected.
        /// Returns true when a Tailscale IP was obtained; false on failure (caller must surface the error).
        /// Never opens a browser: auth is authkey-only and the GUI is only started AFTER a confirmed connection.
        /// </summary>
        public static async Task<bool> EnsureTailscaleConnected()
        {
            // Always disable Tailscale GUI Auto-Start on Windows boot to prevent browser login popup
            DisableGuiAutostart();

            for (int retry = 0; retry < 2; retry++)
            {
                LogManager.Instance.Info($"[Connect] Tailscale connection attempt {retry + 1}...");

                // 1. Fetch AuthKey dynamically FIRST
                string authKey;
                try
                {
                    using var httpClient = new System.Net.Http.HttpClient();
                    httpClient.Timeout = TimeSpan.FromSeconds(10); // don't hang launcher start on slow/broken network
                    authKey = (await httpClient.GetStringAsync(AuthKeyGistUrl)).Trim();

                    if (string.IsNullOrEmpty(authKey))
                    {
                        LogManager.Instance.Warning("[Connect] Gist returned an empty AuthKey. Using embedded fallback key.");
                        authKey = FallbackAuthKey;
                    }
                }
                catch (Exception ex)
                {
                    LogManager.Instance.Error($"[Connect] Failed to fetch Tailscale AuthKey: {ex.Message}");
                    authKey = FallbackAuthKey;
                    LogManager.Instance.Info("[Connect] Usando AuthKey de fallback embutida devido à falha de rede/DNS.");
                }

                // 2. Install if not found (fully silent: /quiet /norestart suppresses the MSI UI sequence,
                //    so the installer never launches the GUI / browser post-install)
                if (!File.Exists(TailscalePath))
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
                            if (!process.WaitForExit(300_000)) // 5 min safety timeout
                            {
                                LogManager.Instance.Error("[Connect] Tailscale MSI install timed out after 5 minutes.");
                                try { process.Kill(); } catch { }
                            }
                            LogManager.Instance.Info("[Connect] Tailscale installation finished.");

                            // The MSI re-creates the GUI autostart (Run key). Remove it again so a future
                            // Windows boot never auto-starts an unauthenticated GUI (which opens the browser).
                            DisableGuiAutostart();

                            // Aguarda o serviço (daemon) do Tailscale subir antes de tentar se comunicar via CLI
                            LogManager.Instance.Info("[Connect] Aguardando o serviço Tailscale iniciar...");
                            await Task.Delay(5000);
                        }
                        else
                        {
                            LogManager.Instance.Error("[Connect] Embedded TailscaleInstaller resource not found in launcher binary.");
                        }
                    }
                    catch (Exception ex)
                    {
                        LogManager.Instance.Error($"[Connect] Failed to install Tailscale: {ex.Message}");
                    }
                }

                // 3. Force Tailscale UP and authenticate via authkey — never interactive, never a browser.
                //    We do this every time to refresh/repair the login and ensure the connection is alive.
                bool upSucceeded = false;
                if (File.Exists(TailscalePath) && !string.IsNullOrEmpty(authKey))
                {
                    LogManager.Instance.Info("[Connect] Connecting Tailscale via CLI with AuthKey...");
                    upSucceeded = RunTailscaleUp(authKey);

                    if (!upSucceeded)
                    {
                        // Auth failed (expired/invalid key, control server unreachable, etc.).
                        // Kill any running GUI so it cannot pop a browser login while unauthenticated.
                        LogManager.Instance.Error("[Connect] 'tailscale up' FAILED — AuthKey inválida/expirada ou servidor de controle inacessível. Nenhum navegador será aberto.");
                        KillTailscaleGui();
                    }
                }
                else if (!File.Exists(TailscalePath))
                {
                    LogManager.Instance.Error("[Connect] Tailscale CLI not found after install attempt.");
                }

                // 4. Wait up to 10 seconds for Tailscale to assign an IP.
                //    Note: even if 'up' failed, a previous valid session may still hold an IP — that counts as success.
                for (int wait = 0; wait < 10; wait++)
                {
                    if (!string.IsNullOrEmpty(GetTailscaleIp()))
                    {
                        LogManager.Instance.Info("[Connect] Tailscale is connected with IP.");
                        // 5. Only start the GUI AFTER a confirmed, authenticated connection —
                        //    an authenticated GUI never opens the browser login.
                        StartGuiIfNotRunning();
                        return true; // Sucesso!
                    }
                    await Task.Delay(1000);
                }

                LogManager.Instance.Warning($"[Connect] Tailscale IP not found after attempt {retry + 1}. Retrying process...");
            }

            LogManager.Instance.Error("[Connect] Tailscale FAILED to connect after 2 attempts (install/auth/IP). Propagating error to caller — no browser fallback.");
            return false;
        }

        /// <summary>
        /// Runs 'tailscale up' with the authkey in unattended mode. Captures exit code and stderr.
        /// Returns true on exit code 0. Never opens a browser (authkey login is non-interactive).
        /// </summary>
        private static bool RunTailscaleUp(string authKey)
        {
            try
            {
                var tsProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = TailscalePath,
                        Arguments = $"up --authkey={authKey} --unattended --reset --accept-dns=false --accept-routes=false",
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    }
                };
                tsProcess.Start();
                string stdErr = tsProcess.StandardError.ReadToEnd();
                tsProcess.StandardOutput.ReadToEnd(); // drain to avoid pipe deadlock

                if (!tsProcess.WaitForExit(60_000)) // 'up' can hang if the control plane is unreachable
                {
                    LogManager.Instance.Error("[Connect] 'tailscale up' timed out after 60s. Killing process.");
                    try { tsProcess.Kill(); } catch { }
                    return false;
                }

                if (tsProcess.ExitCode != 0)
                {
                    LogManager.Instance.Error($"[Connect] 'tailscale up' exited with code {tsProcess.ExitCode}. stderr: {stdErr.Trim()}");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error($"[Connect] Failed to run tailscale up: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Removes every known Tailscale GUI autostart entry (HKCU/HKLM Run keys and Startup folder
        /// shortcuts) so tailscale-ipn never auto-starts on Windows boot in an unauthenticated state
        /// and pops a browser login. Called on every launcher start AND right after MSI install
        /// (the installer re-creates the Run key).
        /// </summary>
        private static void DisableGuiAutostart()
        {
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
        }

        /// <summary>
        /// Starts the Tailscale tray GUI if not already running. Only called after the connection
        /// is confirmed (authenticated), so the GUI has no reason to open a browser login.
        /// </summary>
        private static void StartGuiIfNotRunning()
        {
            if (!File.Exists(TailscaleIpnPath)) return;

            if (Process.GetProcessesByName("tailscale-ipn").Length == 0)
            {
                LogManager.Instance.Info("[Connect] Starting Tailscale GUI app (post-auth)...");
                try
                {
                    Process.Start(new ProcessStartInfo { FileName = TailscaleIpnPath, UseShellExecute = true });
                }
                catch { }
            }
        }

        /// <summary>
        /// Kills any running Tailscale GUI processes. Used when authentication failed, so an
        /// unauthenticated GUI cannot open a browser login popup on its own.
        /// </summary>
        private static void KillTailscaleGui()
        {
            try
            {
                foreach (var proc in Process.GetProcessesByName("tailscale-ipn"))
                {
                    LogManager.Instance.Info("[Connect] Killing unauthenticated Tailscale GUI to prevent browser login popup.");
                    try { proc.Kill(); } catch { }
                    proc.Dispose();
                }
            }
            catch { }
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
