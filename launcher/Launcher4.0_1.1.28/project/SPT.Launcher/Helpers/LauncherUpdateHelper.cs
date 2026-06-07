using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace SPT.Launcher.Helpers
{
    public static class LauncherUpdateHelper
    {
        public const string CurrentVersion = "1.1.28"; // Versão corrente do launcher que será compilado agora.

        public static async Task<bool> CheckAndUpdateAsync(string serverUrl, IProgress<int> progress = null)
        {
            try
            {
                using var client = new HttpClient();
                client.Timeout = TimeSpan.FromSeconds(10);
                
                // O mod.js roda um servidor HTTP puro na porta 7075
                var uri = new Uri(serverUrl);
                string updateServerUrl = $"http://{uri.Host}:7075";
                
                string versionUrl = $"{updateServerUrl}/redline/launcher/version";
                var response = await client.GetAsync(versionUrl);
                
                if (!response.IsSuccessStatusCode)
                    return false;

                string json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                string newVersion = doc.RootElement.GetProperty("version").GetString() ?? "1.0.0";

                if (IsNewerVersion(CurrentVersion, newVersion))
                {
                    string downloadUrl = $"{updateServerUrl}/redline/launcher/download";
                    await DownloadAndPatchAsync(downloadUrl, progress);
                    return true; // Retorna true para sinalizar que vai fechar/atualizar
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AutoUpdate] Erro ao buscar atualização do launcher: {ex.Message}");
            }
            
            return false;
        }

        private static bool IsNewerVersion(string currentVersion, string newVersion)
        {
            if (Version.TryParse(currentVersion, out var curr) && Version.TryParse(newVersion, out var next))
            {
                return next > curr;
            }
            return false;
        }

        private static async Task DownloadAndPatchAsync(string downloadUrl, IProgress<int> progress = null)
        {
            string currentExe = Process.GetCurrentProcess().MainModule?.FileName;
            if (string.IsNullOrEmpty(currentExe)) return;

            string updateExe = Path.Combine(Environment.CurrentDirectory, "SPT.Launcher_Update.exe");
            string batFile = Path.Combine(Environment.CurrentDirectory, "update_launcher.bat");

            using var client = new HttpClient();
            using var response = await client.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            var totalBytes = response.Content.Headers.ContentLength ?? -1L;
            var canReportProgress = totalBytes != -1 && progress != null;

            using var contentStream = await response.Content.ReadAsStreamAsync();
            using var fileStream = new FileStream(updateExe, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);

            var totalRead = 0L;
            var buffer = new byte[8192];
            var isMoreToRead = true;
            var lastReportedProgress = -1;

            do
            {
                var read = await contentStream.ReadAsync(buffer, 0, buffer.Length);
                if (read == 0)
                {
                    isMoreToRead = false;
                }
                else
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
            while (isMoreToRead);

            // Close the file stream so the bat file can run and do its job without issues
            fileStream.Close();

            string currentFileName = Path.GetFileName(currentExe);
            
            // Cria um arquivo bat que espera 3 segundos (para dar o tempo do processo originador fechar),
            // deleta o executável antigo, renomeia o novo executável pro nome antigo, inicia ele, e por fim se autodestroi.
            string batContent = $@"@echo off
title Atualizando Tarkov Red Line
echo Instalando a nova versao do Launcher... aguarde alguns segundos.
timeout /t 3 /nobreak >nul
del /f /q ""{currentFileName}""
ren ""SPT.Launcher_Update.exe"" ""{currentFileName}""
start """" ""{currentFileName}""
del ""%~f0""
";
            await File.WriteAllTextAsync(batFile, batContent);

            var startInfo = new ProcessStartInfo
            {
                FileName = batFile,
                UseShellExecute = true,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };
            Process.Start(startInfo);
            
            // Derruba o launcher imediatamente para não prender o arquivo exe
            Environment.Exit(0);
        }
    }
}
