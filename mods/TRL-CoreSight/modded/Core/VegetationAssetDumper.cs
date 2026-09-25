using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Comfort.Common;
using EFT;
using EFT.Communications;
using TRLCoreSight.Configuration;
using UnityEngine;

namespace TRLCoreSight.Core
{
    /// <summary>
    /// Ferramenta de Depuração e Inspeção de Vegetação.
    /// Varre a cena ativa do mapa, extrai todas as texturas de grama, mato e folhagem
    /// para arquivos PNG reais na máquina local e gera um catálogo descritivo com as
    /// dimensões físicas (altura e largura em metros) dos modelos 3D encontrados,
    /// identificando se possuem ou não colisores físicos (para aferição de visão de IA).
    /// </summary>
    public static class VegetationAssetDumper
    {
        public class VegetationItemInfo
        {
            public string AssetName;
            public string TextureFileName;
            public string MaterialName;
            public string LayerName;
            public float HeightMeters;
            public float WidthMeters;
            public float LengthMeters;
            public string ColliderInfo;
            public string CategorySuggestion;
        }

        public static void DumpActiveSceneVegetation()
        {
            try
            {
                Plugin.LogSource?.LogInfo("[TRL-CoreSight] Iniciando varredura e dump de texturas de vegetação...");

                string mapName = "UnknownMap";
                var gameWorld = Singleton<GameWorld>.Instance;
                if (gameWorld != null && gameWorld.MainPlayer != null && gameWorld.MainPlayer.Location != null)
                {
                    mapName = gameWorld.MainPlayer.Location;
                }
                else
                {
                    mapName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
                }

                string baseDumpDir = Path.Combine(BepInEx.Paths.PluginPath, "TRL-CoreSight", "Dump", $"Grass_{SanitizeFileName(mapName)}");
                string texturesDir = Path.Combine(baseDumpDir, "Textures");

                if (!Directory.Exists(texturesDir))
                {
                    Directory.CreateDirectory(texturesDir);
                }

                HashSet<Texture2D> exportedTextures = new HashSet<Texture2D>();
                List<VegetationItemInfo> catalog = new List<VegetationItemInfo>();

                int foliageLayer = LayerMask.NameToLayer("Foliage");
                int grassLayer = LayerMask.NameToLayer("Grass");

                // 1. Varredura de todos os Renderers na cena
                Renderer[] allRenderers = UnityEngine.Object.FindObjectsOfType<Renderer>();
                Plugin.LogSource?.LogInfo($"[TRL-CoreSight] Analisando {allRenderers.Length} renderers na cena...");

                for (int i = 0; i < allRenderers.Length; i++)
                {
                    Renderer rend = allRenderers[i];
                    if (rend == null || rend.gameObject == null) continue;

                    GameObject go = rend.gameObject;
                    int layer = go.layer;
                    string goNameLower = go.name.ToLower();

                    bool isTargetLayer = (foliageLayer >= 0 && layer == foliageLayer) || (grassLayer >= 0 && layer == grassLayer);
                    bool isVegetationKeyword = goNameLower.Contains("grass") || goNameLower.Contains("weed") || 
                                              goNameLower.Contains("reed") || goNameLower.Contains("bush") || 
                                              goNameLower.Contains("fern") || goNameLower.Contains("flower") ||
                                              goNameLower.Contains("plant");

                    // Verifica se o componente ou o nome pertence à vegetação
                    if (!isTargetLayer && !isVegetationKeyword)
                    {
                        continue;
                    }

                    Material[] mats = rend.sharedMaterials;
                    if (mats == null || mats.Length == 0) continue;

                    Bounds b = rend.bounds;
                    float height = b.size.y;
                    float width = b.size.x;
                    float length = b.size.z;

                    Collider col = go.GetComponent<Collider>();
                    string colliderInfo = col == null ? "Sem Colisor (Transpassável)" : col.GetType().Name;

                    for (int m = 0; m < mats.Length; m++)
                    {
                        Material mat = mats[m];
                        if (mat == null) continue;

                        Texture mainTex = null;
                        if (mat.HasProperty("_MainTex"))
                        {
                            mainTex = mat.GetTexture("_MainTex");
                        }
                        else if (mat.HasProperty("_BaseMap"))
                        {
                            mainTex = mat.GetTexture("_BaseMap");
                        }
                        else if (mat.HasProperty("_Albedo"))
                        {
                            mainTex = mat.GetTexture("_Albedo");
                        }

                        Texture2D tex2D = mainTex as Texture2D;
                        string savedPngName = "SemTextura.png";

                        if (tex2D != null)
                        {
                            string safeTexName = SanitizeFileName(tex2D.name);
                            if (string.IsNullOrEmpty(safeTexName)) safeTexName = "Texture";

                            if (!exportedTextures.Contains(tex2D))
                            {
                                exportedTextures.Add(tex2D);
                                savedPngName = $"{safeTexName}_{tex2D.width}x{tex2D.height}.png";
                                string filePath = Path.Combine(texturesDir, savedPngName);

                                ExportTextureToPNG(tex2D, filePath);
                            }
                            else
                            {
                                savedPngName = $"{safeTexName}_{tex2D.width}x{tex2D.height}.png";
                            }
                        }

                        string category;
                        if (height >= 0.60f)
                        {
                            category = "Capim Alto / Mato Denso (Camuflagem Recomendada)";
                        }
                        else if (height >= 0.30f)
                        {
                            category = "Mato Médio (Camuflagem Parcial)";
                        }
                        else
                        {
                            category = "Grama Rasteira / Flor (Sem Camuflagem)";
                        }

                        catalog.Add(new VegetationItemInfo
                        {
                            AssetName = go.name,
                            TextureFileName = savedPngName,
                            MaterialName = mat.name,
                            LayerName = LayerMask.LayerToName(layer),
                            HeightMeters = height,
                            WidthMeters = width,
                            LengthMeters = length,
                            ColliderInfo = colliderInfo,
                            CategorySuggestion = category
                        });
                    }
                }

                // 2. Gravação do Catálogo em JSON
                string jsonPath = Path.Combine(baseDumpDir, "vegetation_catalog.json");
                string jsonContent = SimpleJsonSerialize(catalog);
                File.WriteAllText(jsonPath, jsonContent, Encoding.UTF8);

                // 3. Gravação do Resumo em Markdown
                string mdPath = Path.Combine(baseDumpDir, "RESUMO_VEGETACAO.md");
                string mdContent = GenerateMarkdownSummary(mapName, catalog, exportedTextures.Count, texturesDir);
                File.WriteAllText(mdPath, mdContent, Encoding.UTF8);

                Plugin.LogSource?.LogInfo($"[TRL-CoreSight] DUMP CONCLUÍDO COM ÊXITO!");
                Plugin.LogSource?.LogInfo($"[TRL-CoreSight] Total de Texturas PNG exportadas: {exportedTextures.Count}");
                Plugin.LogSource?.LogInfo($"[TRL-CoreSight] Total de Modelos catalogados: {catalog.Count}");
                Plugin.LogSource?.LogInfo($"[TRL-CoreSight] Pasta de Destino: {baseDumpDir}");

                NotificationManagerClass.DisplayMessageNotification(
                    $"[TRL-CoreSight] Dump concluído: {exportedTextures.Count} texturas PNG e {catalog.Count} modelos exportados!",
                    ENotificationDurationType.Long
                );
            }
            catch (Exception ex)
            {
                Plugin.LogSource?.LogError($"[TRL-CoreSight] Erro ao executar dump de vegetação: {ex.Message}\n{ex.StackTrace}");
                NotificationManagerClass.DisplayWarningNotification(
                    $"[TRL-CoreSight] Falha no Dump: {ex.Message}",
                    ENotificationDurationType.Long
                );
            }
        }

        private static void ExportTextureToPNG(Texture2D sourceTex, string targetFilePath)
        {
            try
            {
                int width = Mathf.Clamp(sourceTex.width, 16, 4096);
                int height = Mathf.Clamp(sourceTex.height, 16, 4096);

                RenderTexture tempRT = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32);
                Graphics.Blit(sourceTex, tempRT);

                RenderTexture previousActive = RenderTexture.active;
                RenderTexture.active = tempRT;

                Texture2D readableTex = new Texture2D(width, height, TextureFormat.RGBA32, false);
                readableTex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                readableTex.Apply();

                RenderTexture.active = previousActive;
                RenderTexture.ReleaseTemporary(tempRT);

                byte[] pngBytes = ImageConversion.EncodeToPNG(readableTex);
                UnityEngine.Object.Destroy(readableTex);

                if (pngBytes != null && pngBytes.Length > 0)
                {
                    File.WriteAllBytes(targetFilePath, pngBytes);
                }
            }
            catch (Exception ex)
            {
                Plugin.LogSource?.LogWarning($"[TRL-CoreSight] Falha ao exportar PNG de {sourceTex.name}: {ex.Message}");
            }
        }

        private static string GenerateMarkdownSummary(string mapName, List<VegetationItemInfo> items, int textureCount, string texturesFolder)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"# Catálogo de Vegetação — {mapName}");
            sb.AppendLine();
            sb.AppendLine($"**Data do Dump:** {DateTime.Now:dd/MM/yyyy HH:mm:ss}  ");
            sb.AppendLine($"**Total de Texturas PNG Exportadas:** {textureCount}  ");
            sb.AppendLine($"**Total de Modelos Catalogados:** {items.Count}  ");
            sb.AppendLine($"**Pasta das Imagens:** `{texturesFolder}`  ");
            sb.AppendLine();
            sb.AppendLine("---");
            sb.AppendLine();
            sb.AppendLine("## Tabela de Modelos e Texturas Encontradas");
            sb.AppendLine();
            sb.AppendLine("| Modelo (Asset) | Colisor | Textura PNG | Altura (m) | Largura (m) | Layer | Classificação Sugerida |");
            sb.AppendLine("|---|---|---|:---:|:---:|---|---|");

            HashSet<string> uniqueEntries = new HashSet<string>();

            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                string key = $"{item.AssetName}_{item.TextureFileName}";
                if (uniqueEntries.Contains(key)) continue;
                uniqueEntries.Add(key);

                sb.AppendLine($"| `{item.AssetName}` | {item.ColliderInfo} | `{item.TextureFileName}` | {item.HeightMeters:F2}m | {item.WidthMeters:F2}m | {item.LayerName} | **{item.CategorySuggestion}** |");
            }

            sb.AppendLine();
            sb.AppendLine("---");
            sb.AppendLine("### Legenda de Altura:");
            sb.AppendLine("- 🟢 **Capim Alto (>= 0.60m):** Cobre um soldado deitado ou agachado. Deve conceder camuflagem.");
            sb.AppendLine("- 🟡 **Mato Médio (0.30m a 0.59m):** Cobre apenas se completamente deitado.");
            sb.AppendLine("- 🔴 **Grama Rasteira / Flor (< 0.30m):** Terreno liso/baixo. NÃO deve conceder camuflagem.");

            return sb.ToString();
        }

        private static string SimpleJsonSerialize(List<VegetationItemInfo> items)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("[");
            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                sb.AppendLine("  {");
                sb.AppendLine($"    \"assetName\": \"{EscapeJson(item.AssetName)}\",");
                sb.AppendLine($"    \"colliderInfo\": \"{EscapeJson(item.ColliderInfo)}\",");
                sb.AppendLine($"    \"textureFileName\": \"{EscapeJson(item.TextureFileName)}\",");
                sb.AppendLine($"    \"materialName\": \"{EscapeJson(item.MaterialName)}\",");
                sb.AppendLine($"    \"layerName\": \"{EscapeJson(item.LayerName)}\",");
                sb.AppendLine($"    \"heightMeters\": {item.HeightMeters.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)},");
                sb.AppendLine($"    \"widthMeters\": {item.WidthMeters.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)},");
                sb.AppendLine($"    \"categorySuggestion\": \"{EscapeJson(item.CategorySuggestion)}\"");
                sb.Append("  }");
                if (i < items.Count - 1) sb.Append(",");
                sb.AppendLine();
            }
            sb.AppendLine("]");
            return sb.ToString();
        }

        private static string EscapeJson(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            return s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\r", "").Replace("\n", " ");
        }

        private static string SanitizeFileName(string name)
        {
            if (string.IsNullOrEmpty(name)) return "unnamed";
            char[] invalidChars = Path.GetInvalidFileNameChars();
            for (int i = 0; i < invalidChars.Length; i++)
            {
                name = name.Replace(invalidChars[i], '_');
            }
            return name.Trim();
        }
    }
}
