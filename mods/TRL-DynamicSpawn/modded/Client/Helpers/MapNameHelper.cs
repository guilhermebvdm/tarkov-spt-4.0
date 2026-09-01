using System;
using TRLDynamicSpawn.Models;

namespace TRLDynamicSpawn.Helpers
{
    public static class MapNameHelper
    {
        /// <summary>
        /// Normaliza o nome do mapa retornado pelo EFT (ex: "bigmap") para a chave correspondente no Servidor (ex: "customs").
        /// </summary>
        public static string Normalize(string rawMapName)
        {
            if (string.IsNullOrEmpty(rawMapName)) return "";
            string lower = rawMapName.ToLower();

            if (lower == "bigmap" || lower.Contains("customs")) return "bigmap";
            if (lower.Contains("factory4_day") || lower == "factory") return "factory4_day";
            if (lower.Contains("factory4_night")) return "factory4_night";
            if (lower.Contains("interchange")) return "interchange";
            if (lower.Contains("laboratory") || lower == "lab") return "laboratory";
            if (lower.Contains("lighthouse")) return "lighthouse";
            if (lower.Contains("rezervbase") || lower.Contains("reserve")) return "rezervbase";
            if (lower.Contains("sandbox_high")) return "sandbox_high";
            if (lower.Contains("sandbox") || lower.Contains("groundzero")) return "sandbox";
            if (lower.Contains("shoreline")) return "shoreline";
            if (lower.Contains("tarkovstreets") || lower.Contains("streets")) return "tarkovstreets";
            if (lower.Contains("woods")) return "woods";

            return lower;
        }

        /// <summary>
        /// Obtém com segurança a configuração MapSettings para o mapa atual vinda do TRLConfig,
        /// respeitando o Preset de Performance ativo (High-End, Balanced ou Performance).
        /// </summary>
        public static MapSettings GetMapSettings(TRLConfig config, string rawMapName)
        {
            if (config == null) return null;
            string key = Normalize(rawMapName);

            // Determina qual preset utilizar (F12 tem precedência ou fallback para o server config)
            string selectedPreset = Settings.activePerformancePreset?.Value?.ToLower();
            if (string.IsNullOrEmpty(selectedPreset) || selectedPreset == "default")
            {
                selectedPreset = config.ActivePreset?.ToLower() ?? "balanced";
            }

            PresetProfile activeProfile = null;
            if (config.Presets != null)
            {
                if (selectedPreset.Contains("high"))
                    activeProfile = config.Presets.HighEnd;
                else if (selectedPreset.Contains("perf") || selectedPreset.Contains("low"))
                    activeProfile = config.Presets.Performance;
                else
                    activeProfile = config.Presets.Balanced;
            }

            // 1. Tenta buscar no MapConfigs do Preset ativo
            if (activeProfile?.MapConfigs != null)
            {
                if (activeProfile.MapConfigs.TryGetValue(key, out var presetSettings))
                    return presetSettings;
                if (key == "bigmap" && activeProfile.MapConfigs.TryGetValue("customs", out presetSettings))
                    return presetSettings;
                if (key == "customs" && activeProfile.MapConfigs.TryGetValue("bigmap", out presetSettings))
                    return presetSettings;
                if (key == "factory4_night" && activeProfile.MapConfigs.TryGetValue("factory4_day", out presetSettings))
                    return presetSettings;
            }

            // 2. Fallback para MapConfigs raiz se existir
            if (config.MapConfigs != null)
            {
                if (config.MapConfigs.TryGetValue(key, out var settings))
                    return settings;
                if (key == "bigmap" && config.MapConfigs.TryGetValue("customs", out settings))
                    return settings;
                if (key == "customs" && config.MapConfigs.TryGetValue("bigmap", out settings))
                    return settings;
                if (key == "factory4_night" && config.MapConfigs.TryGetValue("factory4_day", out settings))
                    return settings;
            }

            return null;
        }

        /// <summary>
        /// Obtém com segurança a configuração WaveTimerConfig para o mapa atual vinda do TRLConfig.
        /// </summary>
        public static WaveTimerConfig GetWaveTimerConfig(TRLConfig config, string rawMapName)
        {
            if (config?.MapTimers == null) return null;
            string key = Normalize(rawMapName);

            if (config.MapTimers.TryGetValue(key, out var timerConfig))
                return timerConfig;

            if (key == "bigmap" && config.MapTimers.TryGetValue("customs", out timerConfig))
                return timerConfig;
            if (key == "customs" && config.MapTimers.TryGetValue("bigmap", out timerConfig))
                return timerConfig;
            if (key == "factory4_night" && config.MapTimers.TryGetValue("factory4_day", out timerConfig))
                return timerConfig;

            if (config.MapTimers.TryGetValue("global", out timerConfig))
                return timerConfig;

            return null;
        }
    }
}
