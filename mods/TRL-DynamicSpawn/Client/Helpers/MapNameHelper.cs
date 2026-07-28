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

            if (lower == "bigmap" || lower.Contains("customs")) return "customs";
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
        /// Obtém com segurança a configuração MapSettings para o mapa atual vinda do TRLConfig.
        /// </summary>
        public static MapSettings GetMapSettings(TRLConfig config, string rawMapName)
        {
            if (config?.MapConfigs == null) return null;
            string key = Normalize(rawMapName);

            if (config.MapConfigs.TryGetValue(key, out var settings))
                return settings;

            // Fallback para variantes de mapa se a chave exata não for encontrada
            if (key == "factory4_night" && config.MapConfigs.TryGetValue("factory4_day", out settings))
                return settings;
            if (key == "sandbox_high" && config.MapConfigs.TryGetValue("sandbox", out settings))
                return settings;

            return null;
        }
    }
}
