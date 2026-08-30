using System;
using System.IO;
using Newtonsoft.Json;
using SPT.Launcher.Controllers;
using SPT.Launcher.Models.Launcher;

namespace SPT.Launcher.Helpers
{
    public static class GameStateDetector
    {
        private const string StateFileName = "base-game-state.json";

        public static string GetStateFilePath(string gamePath)
        {
            if (string.IsNullOrWhiteSpace(gamePath))
                return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "user", "launcher", StateFileName);

            return Path.Combine(gamePath, "user", "launcher", StateFileName);
        }

        public static BaseGameState LoadState(string gamePath)
        {
            try
            {
                string filePath = GetStateFilePath(gamePath);
                if (File.Exists(filePath))
                {
                    string json = File.ReadAllText(filePath);
                    var state = JsonConvert.DeserializeObject<BaseGameState>(json);
                    if (state != null)
                        return state;
                }
            }
            catch (Exception ex)
            {
                LogManager.Instance.Warning($"[GameStateDetector] Falha ao ler {StateFileName}: {ex.Message}");
            }

            // Fallback: se o arquivo não existe, mas os binários completos já existem no disco (instalação pré-existente)
            bool exeExists = File.Exists(Path.Combine(gamePath, "EscapeFromTarkov.exe"));
            bool dataExists = Directory.Exists(Path.Combine(gamePath, "EscapeFromTarkov_Data"));

            if (exeExists && dataExists)
            {
                var legacyState = new BaseGameState
                {
                    Status = "Installed",
                    Completed = true,
                    ProgressPercentage = 100.0,
                    LastUpdated = DateTime.UtcNow
                };
                SaveState(gamePath, legacyState);
                return legacyState;
            }

            return new BaseGameState
            {
                Status = "NotInstalled",
                Completed = false,
                ProgressPercentage = 0.0,
                LastUpdated = DateTime.UtcNow
            };
        }

        public static void SaveState(string gamePath, BaseGameState state)
        {
            try
            {
                if (state == null) return;
                state.LastUpdated = DateTime.UtcNow;

                string filePath = GetStateFilePath(gamePath);
                string dir = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                string json = JsonConvert.SerializeObject(state, Formatting.Indented);
                File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error($"[GameStateDetector] Falha ao salvar {StateFileName}: {ex.Message}");
            }
        }

        public static bool IsBaseGameInstalled(string gamePath)
        {
            if (string.IsNullOrWhiteSpace(gamePath))
                return false;

            bool exeExists = File.Exists(Path.Combine(gamePath, "EscapeFromTarkov.exe"));
            bool dataExists = Directory.Exists(Path.Combine(gamePath, "EscapeFromTarkov_Data"));

            if (!exeExists || !dataExists)
                return false;

            var state = LoadState(gamePath);
            return state.Completed;
        }

        public static void MarkAsInstalled(string gamePath, string torrentHash = "", long totalBytes = 0, string version = "")
        {
            var state = new BaseGameState
            {
                Status = "Installed",
                Completed = true,
                TorrentHash = torrentHash,
                ProgressPercentage = 100.0,
                DownloadedBytes = totalBytes,
                TotalBytes = totalBytes,
                InstalledVersion = version,
                LastUpdated = DateTime.UtcNow
            };
            SaveState(gamePath, state);
            LogManager.Instance.Info("[GameStateDetector] Jogo base marcado como 100% INSTALADO.");
        }

        public static void MarkAsDownloading(string gamePath, string torrentHash, double progress, long downloaded, long total)
        {
            var state = LoadState(gamePath);
            state.Status = "Downloading";
            state.Completed = false;
            state.TorrentHash = torrentHash;
            state.ProgressPercentage = progress;
            state.DownloadedBytes = downloaded;
            state.TotalBytes = total;
            state.LastUpdated = DateTime.UtcNow;
            SaveState(gamePath, state);
        }

        public static void MarkAsPaused(string gamePath, double progress)
        {
            var state = LoadState(gamePath);
            state.Status = "Paused";
            state.Completed = false;
            state.ProgressPercentage = progress;
            state.LastUpdated = DateTime.UtcNow;
            SaveState(gamePath, state);
            LogManager.Instance.Info($"[GameStateDetector] Download base pausado em {progress:F1}%.");
        }
    }
}
