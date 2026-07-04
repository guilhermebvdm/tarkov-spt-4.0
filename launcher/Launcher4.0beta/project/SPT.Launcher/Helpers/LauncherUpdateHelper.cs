using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Tasks;
using SPT.Launcher.Security;

namespace SPT.Launcher.Helpers
{
    /// <summary>
    /// Item 018 (auto-update-security) — outcome of a launcher update check. Replaces the old opaque
    /// <c>bool</c> so the UI can tell a VERIFICATION FAILURE (fail-closed, keep playing the current
    /// version + non-blocking warning) apart from "no update" and a plain network error.
    /// </summary>
    public enum UpdateOutcome
    {
        /// <summary>Server version ≤ current, or updates disabled — nothing to do (CA-5).</summary>
        UpToDate,

        /// <summary>A verified exe was promoted; the launcher is exiting to let the .bat swap it in (CA-1).</summary>
        Restarting,

        /// <summary>Update ABORTED by the security gate: no signature / bad signature / wrong key
        /// (CA-2/CA-3). Current version stays fully playable; UI shows a non-blocking warning.</summary>
        VerificationFailed,

        /// <summary>Could not reach the server / download failed. No update; login proceeds silently.</summary>
        NetworkError,
    }

    public static class LauncherUpdateHelper
    {
        // Fonte única: lê a AssemblyVersion do csproj (item 014) — nada de const p/ manter em sincronia.
        public static readonly string CurrentVersion =
            System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "2.0.0";

        private const string QuarantineFileName = "SPT.Launcher_Update.exe.tmp"; // download lands here first (RN-3)
        private const string PromotedFileName = "SPT.Launcher_Update.exe";       // only a VERIFIED exe reaches this name

        /// <summary>
        /// Item 018 — builds an HttpClientHandler whose TLS validation is PINNED (RN-5). The old blanket
        /// <c>=&gt; true</c> is gone: the callback now delegates to <see cref="CertificatePinning.IsAllowedForConnection"/>
        /// (strict when pins are configured; bootstrap-accept while the pin list is empty — the exe
        /// signature is the real RCE barrier regardless).
        /// </summary>
        private static HttpClientHandler CreatePinnedHandler()
        {
            return new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback =
                    (sender, cert, chain, sslPolicyErrors) => CertificatePinning.IsAllowedForConnection(cert)
            };
        }

        private static void LogInfo(string msg) =>
            global::SPT.Launcher.Controllers.LogManager.Instance.Info(msg);

        private static void LogError(string msg) =>
            global::SPT.Launcher.Controllers.LogManager.Instance.Error(msg);

        /// <summary>
        /// Checks the server for a newer launcher and, if one exists, downloads it to QUARANTINE, verifies
        /// its detached signature against the embedded public key, and only then promotes + restarts.
        /// FAIL-CLOSED (01-spec RN-1): any doubt aborts without touching the running binary. Returns an
        /// <see cref="UpdateOutcome"/> so the caller can surface a verification failure without blocking play.
        /// </summary>
        public static async Task<UpdateOutcome> CheckAndUpdateAsync(string serverUrl, IProgress<int> progress = null)
        {
            string versionJson;
            string newVersion;
            string signatureB64;

            // --- Phase 1: fetch version metadata (network errors are non-fatal → NetworkError) ---
            try
            {
                using var client = new HttpClient(CreatePinnedHandler());
                client.Timeout = TimeSpan.FromSeconds(10);

                // O mod roda dentro do SPT Server, então usa a mesma URL e Porta
                string updateServerUrl = serverUrl.TrimEnd('/');
                string versionUrl = $"{updateServerUrl}/redline/launcher/version";

                var response = await client.GetAsync(versionUrl);
                if (!response.IsSuccessStatusCode)
                    return UpdateOutcome.NetworkError;

                versionJson = await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                LogError($"[AutoUpdate] Falha de rede ao consultar versão do launcher: {ex.Message}");
                Debug.WriteLine($"[AutoUpdate] Falha de rede ao consultar versão: {ex.Message}");
                return UpdateOutcome.NetworkError;
            }

            // --- Phase 2: parse + version gate (parse errors are non-fatal → NetworkError) ---
            try
            {
                using var doc = JsonDocument.Parse(versionJson);
                var root = doc.RootElement;

                newVersion = root.TryGetProperty("version", out var vEl) && vEl.ValueKind == JsonValueKind.String
                    ? vEl.GetString()
                    : "1.0.0";

                signatureB64 = root.TryGetProperty("signature", out var sEl) && sEl.ValueKind == JsonValueKind.String
                    ? sEl.GetString()
                    : null;
            }
            catch (Exception ex)
            {
                LogError($"[AutoUpdate] Resposta de versão malformada: {ex.Message}");
                return UpdateOutcome.NetworkError;
            }

            if (!UpdateVersion.IsNewerVersion(CurrentVersion, newVersion))
            {
                return UpdateOutcome.UpToDate; // CA-5: nothing newer (or malformed → fail-safe no-op)
            }

            // --- Phase 3: server announces a newer version but published NO signature → fail-closed (CA-3) ---
            if (string.IsNullOrWhiteSpace(signatureB64))
            {
                LogError($"[AutoUpdate] Versão {newVersion} anunciada SEM assinatura — update abortado (fail-closed, CA-3). " +
                         "Servidor precisa publicar o .sig ao lado do exe.");
                return UpdateOutcome.VerificationFailed;
            }

            // --- Phase 4: download → verify → promote ---
            string downloadUrl = $"{serverUrl.TrimEnd('/')}/redline/launcher/download";
            return await DownloadVerifyAndPatchAsync(downloadUrl, newVersion, signatureB64, progress);
        }

        /// <summary>
        /// Downloads the exe to a QUARANTINE file, recomputes SHA-256, verifies the detached signature
        /// over the EXACT bytes on disk (CA-6), and only on success promotes that SAME file and restarts
        /// (no re-download → no TOCTOU window, R-1). Any failure deletes the quarantine file and returns
        /// without touching the running binary or writing the .bat (CA-2).
        /// </summary>
        private static async Task<UpdateOutcome> DownloadVerifyAndPatchAsync(
            string downloadUrl, string newVersion, string signatureB64, IProgress<int> progress)
        {
            string currentExe = Process.GetCurrentProcess().MainModule?.FileName;
            if (string.IsNullOrEmpty(currentExe))
            {
                LogError("[AutoUpdate] Não foi possível resolver o caminho do exe atual — update abortado.");
                return UpdateOutcome.VerificationFailed;
            }

            string currentDir = Path.GetDirectoryName(currentExe);
            string quarantineExe = Path.Combine(currentDir, QuarantineFileName);
            string promotedExe = Path.Combine(currentDir, PromotedFileName);
            string batFile = Path.Combine(currentDir, "update_launcher.bat");

            // --- Download to quarantine (network/IO problems → NetworkError, abort gracious) ---
            try
            {
                TryDelete(quarantineExe); // clear any stale/locked leftover before writing

                using var client = new HttpClient(CreatePinnedHandler());
                client.Timeout = TimeSpan.FromMinutes(5);
                using var response = await client.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead);
                response.EnsureSuccessStatusCode();

                var totalBytes = response.Content.Headers.ContentLength ?? -1L;
                var canReportProgress = totalBytes != -1 && progress != null;

                using (var contentStream = await response.Content.ReadAsStreamAsync())
                using (var fileStream = new FileStream(quarantineExe, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
                {
                    var totalRead = 0L;
                    var buffer = new byte[8192];
                    var lastReportedProgress = -1;
                    int read;

                    while ((read = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                    {
                        await fileStream.WriteAsync(buffer, 0, read);
                        totalRead += read;
                        if (canReportProgress)
                        {
                            var percentage = (int)((totalRead * 100) / totalBytes);
                            if (percentage != lastReportedProgress)
                            {
                                progress.Report(percentage);
                                lastReportedProgress = percentage;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogError($"[AutoUpdate] Falha ao baixar a atualização: {ex.Message}");
                TryDelete(quarantineExe);
                return UpdateOutcome.NetworkError;
            }

            // --- Verify signature over the bytes ON DISK (authority = recomputed hash, CA-6) ---
            try
            {
                byte[] payload = await File.ReadAllBytesAsync(quarantineExe);

                // Recompute SHA-256 purely for the audit log; the signature itself binds the exact bytes.
                string sha256Hex = Convert.ToHexString(SHA256.HashData(payload)).ToLowerInvariant();

                bool ok = UpdateSignatureVerifier.VerifyBase64(
                    payload, signatureB64, UpdateTrust.EmbeddedPublicKeyPem, UpdateTrust.SignatureAlgorithm);

                if (!ok)
                {
                    LogError($"[AutoUpdate] Assinatura INVÁLIDA para versão {newVersion} (sha256={sha256Hex}) — " +
                             "update abortado, binário atual intacto (fail-closed, CA-2). Quarentena apagada.");
                    TryDelete(quarantineExe);
                    return UpdateOutcome.VerificationFailed;
                }

                // Item 018 (review 🟡 anti-rollback): a assinatura liga BYTES, não versão. Um server
                // malicioso (MITM/Gist/DNS) poderia anunciar versão alta e servir um exe ANTIGO
                // legitimamente assinado → downgrade forçado a cada login. Ligamos a versão à
                // identidade VERIFICADA: a ProductVersion do binário que passou na assinatura tem de
                // ser estritamente maior que a atual; senão é replay de build assinado antigo → fail-closed.
                string verifiedVersion = FileVersionInfo.GetVersionInfo(quarantineExe).ProductVersion;
                if (!UpdateVersion.IsNewerVersion(CurrentVersion, verifiedVersion))
                {
                    LogError($"[AutoUpdate] Binário verificado reporta versão '{verifiedVersion}', que NÃO é mais nova que a atual " +
                             $"'{CurrentVersion}' (possível replay/downgrade de build assinado antigo) — update abortado (fail-closed). Quarentena apagada.");
                    TryDelete(quarantineExe);
                    return UpdateOutcome.VerificationFailed;
                }

                LogInfo($"[AutoUpdate] Assinatura OK e versão verificada {verifiedVersion} > {CurrentVersion} (anunciada {newVersion}, sha256={sha256Hex}). Promovendo binário verificado.");
            }
            catch (Exception ex)
            {
                LogError($"[AutoUpdate] Erro ao verificar a assinatura: {ex.Message} — update abortado (fail-closed).");
                TryDelete(quarantineExe);
                return UpdateOutcome.VerificationFailed;
            }

            // --- Promote the VERIFIED file (same bytes; the .bat renames exactly this file) ---
            try
            {
                File.Move(quarantineExe, promotedExe, overwrite: true);

                string currentFileName = Path.GetFileName(currentExe);

                // Cria um arquivo bat que espera 3 segundos (para dar o tempo do processo originador fechar),
                // deleta o executável antigo, renomeia o novo executável (JÁ VERIFICADO) pro nome antigo,
                // inicia ele, e por fim se autodestroi.
                string batContent = $@"@echo off
title Atualizando Tarkov Red Line
echo Instalando a nova versao do Launcher... aguarde alguns segundos.
timeout /t 3 /nobreak >nul
del /f /q ""{currentFileName}""
ren ""{PromotedFileName}"" ""{currentFileName}""
start """" ""{currentFileName}""
del ""%~f0""
";
                await File.WriteAllTextAsync(batFile, batContent);

                var startInfo = new ProcessStartInfo
                {
                    FileName = batFile,
                    WorkingDirectory = currentDir,
                    UseShellExecute = true,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                };
                Process.Start(startInfo);

                // Derruba o launcher imediatamente para não prender o arquivo exe
                Environment.Exit(0);
                return UpdateOutcome.Restarting; // unreachable after Exit, kept for the type contract
            }
            catch (Exception ex)
            {
                // Promotion failed AFTER a good verification: don't run a half-written swap. Keep current.
                LogError($"[AutoUpdate] Falha ao promover o binário verificado: {ex.Message} — mantendo versão atual.");
                TryDelete(quarantineExe);
                TryDelete(batFile);
                return UpdateOutcome.VerificationFailed;
            }
        }

        private static void TryDelete(string path)
        {
            try
            {
                if (!string.IsNullOrEmpty(path) && File.Exists(path))
                    File.Delete(path);
            }
            catch (Exception ex)
            {
                // Locked quarantine file: log and move on — the next attempt overwrites it (corner case).
                LogError($"[AutoUpdate] Não foi possível apagar '{path}': {ex.Message}");
            }
        }
    }
}
