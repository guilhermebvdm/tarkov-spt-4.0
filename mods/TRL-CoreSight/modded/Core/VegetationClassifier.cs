using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace TRLCoreSight.Core
{
    /// <summary>
    /// Gerenciador dinâmico de regras de vegetação.
    /// Permite classificar in-game o que deve e o que não deve ser considerado camuflagem,
    /// persistindo a configuração em um arquivo JSON.
    /// </summary>
    public static class VegetationClassifier
    {
        [Serializable]
        public class ClassificationData
        {
            public List<string> BlockedVegetation = new List<string>();
            public List<string> AllowedCamouflage = new List<string>();
        }

        private static ClassificationData _data = new ClassificationData();
        private static string _configFilePath;
        private static bool _initialized = false;

        public static void Initialize()
        {
            if (_initialized) return;

            try
            {
                string baseDir = Path.Combine(BepInEx.Paths.PluginPath, "TRL-CoreSight");
                if (!Directory.Exists(baseDir))
                {
                    Directory.CreateDirectory(baseDir);
                }

                _configFilePath = Path.Combine(baseDir, "vegetation_classification.json");

                if (File.Exists(_configFilePath))
                {
                    LoadFromFile();
                }
                else
                {
                    // Valores padrão iniciais com base nos dados in-game coletados
                    _data.BlockedVegetation = new List<string>
                    {
                        "field_grass",
                        "grass_new_1",
                        "grass_new_2",
                        "grass_new_3",
                        "grass_02",
                        "grass_02_512",
                        "vertexlit_rock",
                        "rock",
                        "stone",
                        "pebble",
                        "debris"
                    };

                    _data.AllowedCamouflage = new List<string>
                    {
                        "grass2",
                        "grass5_512",
                        "grass6",
                        "grass11",
                        "grass4",
                        "krapiva",
                        "whitgrass",
                        "fern",
                        "plant_wolf"
                    };

                    SaveToFile();
                }

                _initialized = true;
                Plugin.LogSource?.LogInfo($"[TRL-CoreSight] VegetationClassifier: Carregado com {_data.BlockedVegetation.Count} bloqueados e {_data.AllowedCamouflage.Count} permitidos.");
            }
            catch (Exception ex)
            {
                Plugin.LogSource?.LogError($"[TRL-CoreSight] Erro ao inicializar VegetationClassifier: {ex.Message}");
            }
        }

        public static bool IsBlocked(string name)
        {
            if (string.IsNullOrEmpty(name)) return false;
            Initialize();

            string lower = name.ToLowerInvariant();
            for (int i = 0; i < _data.BlockedVegetation.Count; i++)
            {
                if (lower.Contains(_data.BlockedVegetation[i].ToLowerInvariant()))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool IsExplicitlyAllowed(string name)
        {
            if (string.IsNullOrEmpty(name)) return false;
            Initialize();

            string lower = name.ToLowerInvariant();
            for (int i = 0; i < _data.AllowedCamouflage.Count; i++)
            {
                if (lower.Contains(_data.AllowedCamouflage[i].ToLowerInvariant()))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool RegisterAsCamouflage(string name)
        {
            if (string.IsNullOrEmpty(name)) return false;
            Initialize();

            string clean = CleanName(name);

            // Remove de blocked se estiver lá
            _data.BlockedVegetation.RemoveAll(x => clean.Equals(x, StringComparison.OrdinalIgnoreCase));

            // Adiciona a allowed se ainda não estiver
            if (!_data.AllowedCamouflage.Exists(x => clean.Equals(x, StringComparison.OrdinalIgnoreCase)))
            {
                _data.AllowedCamouflage.Add(clean);
                SaveToFile();
                Plugin.LogSource?.LogInfo($"[TRL-CoreSight] VegetationClassifier: '{clean}' registrado como CAMUFLAGEM.");
                return true;
            }

            return false;
        }

        public static bool RegisterAsNonCamouflage(string name)
        {
            if (string.IsNullOrEmpty(name)) return false;
            Initialize();

            string clean = CleanName(name);

            // Remove de allowed se estiver lá
            _data.AllowedCamouflage.RemoveAll(x => clean.Equals(x, StringComparison.OrdinalIgnoreCase));

            // Adiciona a blocked se ainda não estiver
            if (!_data.BlockedVegetation.Exists(x => clean.Equals(x, StringComparison.OrdinalIgnoreCase)))
            {
                _data.BlockedVegetation.Add(clean);
                SaveToFile();
                Plugin.LogSource?.LogInfo($"[TRL-CoreSight] VegetationClassifier: '{clean}' registrado como NÃO-CAMUFLAGEM (Bloqueado).");
                return true;
            }

            return false;
        }

        private static string CleanName(string raw)
        {
            string clean = raw.Trim().ToLowerInvariant();
            // Remove extensões se houver
            if (clean.EndsWith(".png") || clean.EndsWith(".tga"))
            {
                clean = Path.GetFileNameWithoutExtension(clean);
            }
            return clean;
        }

        private static void LoadFromFile()
        {
            try
            {
                if (!File.Exists(_configFilePath)) return;

                string[] lines = File.ReadAllLines(_configFilePath, Encoding.UTF8);
                bool inBlocked = false;
                bool inAllowed = false;

                _data.BlockedVegetation.Clear();
                _data.AllowedCamouflage.Clear();

                foreach (string line in lines)
                {
                    string trimmed = line.Trim();
                    if (trimmed.Contains("\"BlockedVegetation\""))
                    {
                        inBlocked = true;
                        inAllowed = false;
                        continue;
                    }
                    if (trimmed.Contains("\"AllowedCamouflage\""))
                    {
                        inBlocked = false;
                        inAllowed = true;
                        continue;
                    }
                    if (trimmed.StartsWith("]"))
                    {
                        inBlocked = false;
                        inAllowed = false;
                        continue;
                    }

                    if (trimmed.StartsWith("\""))
                    {
                        int firstQuote = trimmed.IndexOf('"');
                        int secondQuote = trimmed.IndexOf('"', firstQuote + 1);
                        if (firstQuote >= 0 && secondQuote > firstQuote)
                        {
                            string val = trimmed.Substring(firstQuote + 1, secondQuote - firstQuote - 1).Trim().ToLowerInvariant();
                            if (!string.IsNullOrEmpty(val))
                            {
                                if (inBlocked && !_data.BlockedVegetation.Contains(val))
                                {
                                    _data.BlockedVegetation.Add(val);
                                }
                                else if (inAllowed && !_data.AllowedCamouflage.Contains(val))
                                {
                                    _data.AllowedCamouflage.Add(val);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Plugin.LogSource?.LogError($"[TRL-CoreSight] Falha ao ler {_configFilePath}: {ex.Message}");
            }
        }

        private static void SaveToFile()
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("{");
                sb.AppendLine("  \"BlockedVegetation\": [");
                for (int i = 0; i < _data.BlockedVegetation.Count; i++)
                {
                    string comma = (i < _data.BlockedVegetation.Count - 1) ? "," : "";
                    sb.AppendLine($"    \"{_data.BlockedVegetation[i]}\"{comma}");
                }
                sb.AppendLine("  ],");
                sb.AppendLine("  \"AllowedCamouflage\": [");
                for (int i = 0; i < _data.AllowedCamouflage.Count; i++)
                {
                    string comma = (i < _data.AllowedCamouflage.Count - 1) ? "," : "";
                    sb.AppendLine($"    \"{_data.AllowedCamouflage[i]}\"{comma}");
                }
                sb.AppendLine("  ]");
                sb.AppendLine("}");

                File.WriteAllText(_configFilePath, sb.ToString(), Encoding.UTF8);
            }
            catch (Exception ex)
            {
                Plugin.LogSource?.LogError($"[TRL-CoreSight] Falha ao salvar {_configFilePath}: {ex.Message}");
            }
        }
    }
}
