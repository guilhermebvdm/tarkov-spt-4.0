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
        /// Item 023 (Frente C / RN-7): returns a <see cref="TailscaleConnectResult"/> instead of a bare
        /// bool so the caller can distinguish an authkey rejection from a network failure.
        /// <see cref="TailscaleConnectResult.Connected"/> when a Tailscale IP was obtained; otherwise the
        /// most specific failure reason seen across the attempts (caller must surface the error).
        /// Never opens a browser: auth is authkey-only and the GUI is only started AFTER a confirmed connection.
        /// </summary>
        public static async Task<TailscaleConnectResult> EnsureTailscaleConnected()
        {
            // Always disable Tailscale GUI Auto-Start on Windows boot to prevent browser login popup
            DisableGuiAutostart();

            // Remembers WHY the last attempt failed so the final return distinguishes authkey vs network
            // vs not-installed (RN-7). Defaults to a generic network failure until a more specific
            // reason is observed.
            var lastFailure = TailscaleConnectResult.NetworkFailure;

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

                    // ref: CR-01-05 — gist é URL pública: nunca injetar conteúdo não validado
                    // na linha de comando (ex.: "key --exit-node=..." hostil). Formato ≠ tskey → fallback.
                    if (!IsValidAuthKey(authKey))
                    {
                        LogManager.Instance.Warning("[Connect] Gist AuthKey is empty or malformed. Using embedded fallback key.");
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
                    var upResult = RunTailscaleUp(authKey);
                    upSucceeded = upResult == TailscaleConnectResult.Connected;

                    if (!upSucceeded)
                    {
                        // Auth failed (expired/invalid key, control server unreachable, etc.).
                        // Remember the specific reason (authkey vs network) for the final return (RN-7).
                        // Kill any running GUI so it cannot pop a browser login while unauthenticated.
                        lastFailure = upResult;
                        LogManager.Instance.Error("[Connect] 'tailscale up' FAILED — AuthKey inválida/expirada ou servidor de controle inacessível. Nenhum navegador será aberto.");
                        KillTailscaleGui();
                    }
                }
                else if (!File.Exists(TailscalePath))
                {
                    lastFailure = TailscaleConnectResult.NotInstalled;
                    LogManager.Instance.Error("[Connect] Tailscale CLI not found after install attempt.");
                }

                // 4. Wait up to 10 seconds for Tailscale to assign an IP.
                //    A previous valid session may still hold an IP even if 'up' failed — but only
                //    counts as success if the backend is actually Running (ref: CR-01-03 — o adapter
                //    pode reter IP stale com o túnel morto; sucesso falso viraria "server unavailable" genérico).
                for (int wait = 0; wait < 10; wait++)
                {
                    if (!string.IsNullOrEmpty(GetTailscaleIp()))
                    {
                        if (!upSucceeded && !IsBackendRunning())
                        {
                            LogManager.Instance.Warning("[Connect] Tailscale IP present but backend is not Running (stale session). Not counting as connected.");
                            break; // sai do wait-loop → retry externo / falha propagada
                        }
                        LogManager.Instance.Info("[Connect] Tailscale is connected with IP.");
                        // 5. Only start the GUI AFTER a confirmed, authenticated connection —
                        //    an authenticated GUI never opens the browser login.
                        StartGuiIfNotRunning();
                        return TailscaleConnectResult.Connected; // Sucesso!
                    }
                    await Task.Delay(1000);
                }

                LogManager.Instance.Warning($"[Connect] Tailscale IP not found after attempt {retry + 1}. Retrying process...");
            }

            LogManager.Instance.Error($"[Connect] Tailscale FAILED to connect after 2 attempts (install/auth/IP), reason={lastFailure}. Propagating error to caller — no browser fallback.");
            return lastFailure;
        }

        /// <summary>
        /// Runs 'tailscale up' with the authkey in unattended mode. Captures exit code and stderr.
        /// Item 023 (Frente C / RN-7): returns a <see cref="TailscaleConnectResult"/> so the caller can
        /// distinguish an authkey rejection (rejected/expired/exhausted) from a network failure — the
        /// stderr is classified by <see cref="TailscaleUpClassifier.ClassifyUpFailure"/>. Exit code 0 →
        /// <see cref="TailscaleConnectResult.Connected"/>. Never opens a browser (authkey login is
        /// non-interactive).
        /// </summary>
        private static TailscaleConnectResult RunTailscaleUp(string authKey)
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
                // ref: CR-01-01 — dreno concorrente dos dois pipes: ReadToEnd() síncrono só retorna
                // quando o processo fecha o stream, o que anulava o timeout exatamente no cenário
                // de control plane inacessível ('up' vivo tentando reconectar → travava para sempre).
                var stdErrTask = tsProcess.StandardError.ReadToEndAsync();
                var stdOutTask = tsProcess.StandardOutput.ReadToEndAsync();

                if (!tsProcess.WaitForExit(60_000)) // 'up' can hang if the control plane is unreachable
                {
                    LogManager.Instance.Error("[Connect] 'tailscale up' timed out after 60s. Killing process.");
                    try { tsProcess.Kill(); } catch { }
                    return TailscaleConnectResult.NetworkFailure; // a hang is a control-plane/network symptom, not the key
                }

                // Processo saiu → pipes fechados → as tasks completam sem bloquear.
                string stdErr = stdErrTask.GetAwaiter().GetResult();
                stdOutTask.GetAwaiter().GetResult();

                if (tsProcess.ExitCode != 0)
                {
                    var classified = TailscaleUpClassifier.ClassifyUpFailure(stdErr, tsProcess.ExitCode);
                    LogManager.Instance.Error($"[Connect] 'tailscale up' exited with code {tsProcess.ExitCode} ({classified}). stderr: {stdErr.Trim()}");
                    return classified;
                }

                return TailscaleConnectResult.Connected;
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error($"[Connect] Failed to run tailscale up: {ex.Message}");
                return TailscaleConnectResult.NetworkFailure;
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
            // ref: CR-01-02 — cada remoção é independente: SecurityException no HKLM (launcher
            // roda sem elevação) não pode pular a limpeza dos atalhos de Startup abaixo.
            try
            {
                using var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
                key?.DeleteValue("Tailscale", false);
            }
            catch (Exception ex) { LogManager.Instance.Warning($"[Connect] Could not remove HKCU Tailscale autostart: {ex.Message}"); }

            try
            {
                using var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
                key?.DeleteValue("Tailscale", false);
            }
            catch (Exception ex) { LogManager.Instance.Warning($"[Connect] Could not remove HKLM Tailscale autostart (needs elevation?): {ex.Message}"); }

            try
            {
                string tsShortcut = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Startup), "Tailscale.lnk");
                if (File.Exists(tsShortcut)) File.Delete(tsShortcut);
            }
            catch (Exception ex) { LogManager.Instance.Warning($"[Connect] Could not remove Startup shortcut: {ex.Message}"); }

            try
            {
                string commonTsShortcut = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonStartup), "Tailscale.lnk");
                if (File.Exists(commonTsShortcut)) File.Delete(commonTsShortcut);
            }
            catch (Exception ex) { LogManager.Instance.Warning($"[Connect] Could not remove common Startup shortcut: {ex.Message}"); }
        }

        /// <summary>
        /// A valid Tailscale auth key is a single "tskey-..." token (letters, digits, dashes).
        /// Anything else coming from the public gist is rejected. // ref: CR-01-05
        /// </summary>
        private static bool IsValidAuthKey(string key) =>
            !string.IsNullOrEmpty(key)
            && key.StartsWith("tskey-", StringComparison.Ordinal)
            && key.All(c => char.IsAsciiLetterOrDigit(c) || c == '-');

        /// <summary>
        /// Checks whether the Tailscale backend is actually Running (tunnel alive), to reject
        /// stale interface IPs left by a dead session. // ref: CR-01-03
        /// </summary>
        private static bool IsBackendRunning()
        {
            try
            {
                var p = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = TailscalePath,
                        Arguments = "status --json",
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    }
                };
                p.Start();
                var outTask = p.StandardOutput.ReadToEndAsync();
                var errTask = p.StandardError.ReadToEndAsync();
                if (!p.WaitForExit(10_000))
                {
                    try { p.Kill(); } catch { }
                    return false;
                }
                string json = outTask.GetAwaiter().GetResult();
                errTask.GetAwaiter().GetResult();
                // Parse leve: só precisamos do BackendState no topo do JSON.
                return json.Contains("\"BackendState\": \"Running\"") || json.Contains("\"BackendState\":\"Running\"");
            }
            catch
            {
                return false;
            }
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
