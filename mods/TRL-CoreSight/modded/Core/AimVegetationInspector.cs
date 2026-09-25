using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Comfort.Common;
using EFT;
using EFT.Communications;
using GPUInstancer;
using TRLCoreSight.Configuration;
using UnityEngine;

namespace TRLCoreSight.Core
{
    /// <summary>
    /// Inspetor de Vegetação e Camuflagem por Mira (Tecla '[').
    /// Dispara um raycast a partir da visão do jogador para identificar exatamente
    /// a folhagem/capim/grama 2D ou 3D no ponto mirado (não apenas o terreno),
    /// extraindo nomes de imagens/texturas, prefabs, dimensões físicas (detailScale min/max)
    /// e densidade de GPUInstancer, exportando texturas em PNG e exibindo no HUD.
    /// </summary>
    public class AimVegetationInspector : MonoBehaviour
    {
        public class DetectedGrassInfo
        {
            public string PrototypeName;
            public string TextureName;
            public string PrefabName;
            public float MinHeight;
            public float MaxHeight;
            public float MinWidth;
            public float MaxWidth;
            public int DensityAtPoint;
            public string Classification;
            public bool IsDirectlyUnderCrosshair;
            public Texture2D TextureRef;
        }

        public class AimInspectionResult
        {
            public Vector3 AimWorldPoint;
            public float DistanceMeters;
            public string HitObjectName;
            public string HitObjectLayer;
            public string HitColliderType;
            public List<DetectedGrassInfo> PresentAtPoint = new List<DetectedGrassInfo>();
            public List<DetectedGrassInfo> AllMapPrototypes = new List<DetectedGrassInfo>();
            public List<string> NearbyMeshes = new List<string>();
            public DateTime Timestamp;
        }

        private Player _player;
        private AimInspectionResult _lastResult;
        private float _displayEndTime = 0f;
        private Vector2 _scrollPos = Vector2.zero;
        private GUIStyle _panelStyle;
        private GUIStyle _headerStyle;
        private GUIStyle _labelStyle;
        private GUIStyle _highlightStyle;

        public void Initialize(Player player)
        {
            _player = player;
        }

        public void InspectAtAim(bool registerAsCamouflage = true, bool registerAsNonCamouflage = false)
        {
            if (ModConfig.EnableVegetationInspector != null && !ModConfig.EnableVegetationInspector.Value)
            {
                return;
            }

            try
            {
                Camera cam = Camera.main;
                Vector3 origin = Vector3.zero;
                Vector3 direction = Vector3.forward;

                if (cam != null)
                {
                    origin = cam.transform.position;
                    direction = cam.transform.forward;
                }
                else if (_player != null && _player.PlayerBones != null && _player.PlayerBones.Head != null)
                {
                    origin = _player.PlayerBones.Head.Original.position + Vector3.up * 0.1f;
                    direction = _player.LookDirection;
                }
                else
                {
                    Plugin.LogSource?.LogWarning("[TRL-CoreSight] Inspetor de Mira: Câmera ou jogador não encontrados.");
                    return;
                }

                // Raycast ignorando colisores do jogador e seus escudos
                Ray ray = new Ray(origin, direction);
                Vector3 targetPoint = origin + direction * 15f;
                float distance = 15f;
                string hitName = "Vácuo (Sem Colisor)";
                string hitLayer = "Nenhum";
                string colType = "Nenhum";

                RaycastHit[] hits = Physics.RaycastAll(ray, 150f, ~0, QueryTriggerInteraction.Ignore);
                if (hits != null && hits.Length > 0)
                {
                    Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

                    for (int h = 0; h < hits.Length; h++)
                    {
                        var currentHit = hits[h];
                        if (currentHit.collider == null) continue;

                        // Ignora se for parte do próprio corpo do jogador ou colidentes de escudo/HitCollider do jogador
                        if (_player != null && (currentHit.collider.transform.IsChildOf(_player.gameObject.transform) || currentHit.collider.gameObject.layer == LayerMask.NameToLayer("HitCollider")))
                        {
                            continue;
                        }

                        targetPoint = currentHit.point;
                        distance = currentHit.distance;
                        hitName = currentHit.collider.gameObject.name;
                        hitLayer = LayerMask.LayerToName(currentHit.collider.gameObject.layer);
                        colType = currentHit.collider.GetType().Name;
                        break;
                    }
                }

                AimInspectionResult result = new AimInspectionResult
                {
                    AimWorldPoint = targetPoint,
                    DistanceMeters = distance,
                    HitObjectName = hitName,
                    HitObjectLayer = hitLayer,
                    HitColliderType = colType,
                    Timestamp = DateTime.Now
                };

                string mapName = "UnknownMap";
                var gw = Singleton<GameWorld>.Instance;
                if (gw != null && gw.MainPlayer != null && gw.MainPlayer.Location != null)
                {
                    mapName = gw.MainPlayer.Location;
                }

                string dumpDir = Path.Combine(BepInEx.Paths.PluginPath, "TRL-CoreSight", "Dump", "AimInspector", SanitizeFileName(mapName));
                string texturesDir = Path.Combine(dumpDir, "Textures");
                if (!Directory.Exists(texturesDir))
                {
                    Directory.CreateDirectory(texturesDir);
                }

                // 1. Varredura no GPUInstancerDetailManager
                GPUInstancerDetailManager[] managers = UnityEngine.Object.FindObjectsOfType<GPUInstancerDetailManager>();
                for (int m = 0; m < managers.Length; m++)
                {
                    var mgr = managers[m];
                    if (mgr == null || mgr.prototypeList == null || mgr.prototypeList.Count == 0) continue;

                    Terrain terrain = mgr.terrain;
                    if (terrain == null)
                    {
                        terrain = Terrain.activeTerrain;
                    }

                    Vector3 terrainPos = terrain != null ? terrain.transform.position : Vector3.zero;
                    Vector3 terrainSize = (terrain != null && terrain.terrainData != null) ? terrain.terrainData.size : Vector3.zero;

                    for (int p = 0; p < mgr.prototypeList.Count; p++)
                    {
                        var proto = mgr.prototypeList[p] as GPUInstancerDetailPrototype;
                        if (proto == null) continue;

                        string protoName = proto.name;
                        string texName = proto.prototypeTexture != null ? proto.prototypeTexture.name : "Nenhuma";
                        string prefabName = proto.prefabObject != null ? proto.prefabObject.name : "Nenhum";

                        float minW = proto.detailScale.x;
                        float maxW = proto.detailScale.y;
                        float minH = proto.detailScale.z;
                        float maxH = proto.detailScale.w > 0 ? proto.detailScale.w : (proto.detailScale.y > 0 ? proto.detailScale.y : 0.85f);

                        // Classificação de tamanho em metros
                        string classification;
                        if (maxH >= 0.60f)
                        {
                            classification = "CAPIM ALTO / MATO DENSO";
                        }
                        else if (maxH >= 0.35f)
                        {
                            classification = "MATO MÉDIO";
                        }
                        else
                        {
                            classification = "GRAMA RASTEIRA / CURTA";
                        }

                        // Leitura de densidade no ponto atingido pelo raycast
                        int densityAtPoint = 0;
                        if (terrain != null && terrainSize.x > 0 && terrainSize.z > 0)
                        {
                            float localX = targetPoint.x - terrainPos.x;
                            float localZ = targetPoint.z - terrainPos.z;

                            if (localX >= 0f && localX <= terrainSize.x && localZ >= 0f && localZ <= terrainSize.z)
                            {
                                int[,] densityMap = null;
                                if (mgr.isInitialized)
                                {
                                    try
                                    {
                                        densityMap = mgr.GetDetailLayer(p);
                                    }
                                    catch
                                    {
                                        densityMap = null;
                                    }
                                }

                                if (densityMap == null && proto.cachedDensityMapForInstance != null && proto.cachedDensityMapForInstance.Length > 0)
                                {
                                    int sqrtLen = Mathf.FloorToInt(Mathf.Sqrt(proto.cachedDensityMapForInstance.Length));
                                    if (sqrtLen > 0)
                                    {
                                        densityMap = new int[sqrtLen, sqrtLen];
                                        for (int r = 0; r < sqrtLen; r++)
                                        {
                                            int rOffset = r * sqrtLen;
                                            for (int c = 0; c < sqrtLen; c++)
                                            {
                                                densityMap[r, c] = proto.cachedDensityMapForInstance[rOffset + c];
                                            }
                                        }
                                    }
                                }

                                if (densityMap != null)
                                {
                                    int res = densityMap.GetLength(0);
                                    int x = Mathf.Clamp(Mathf.FloorToInt((localX / terrainSize.x) * res), 0, res - 1);
                                    int z = Mathf.Clamp(Mathf.FloorToInt((localZ / terrainSize.z) * res), 0, res - 1);

                                    // Avalia ponto central e vizinhança 3x3 (~1 metro)
                                    for (int ox = -1; ox <= 1; ox++)
                                    {
                                        int sx = Mathf.Clamp(x + ox, 0, res - 1);
                                        for (int oz = -1; oz <= 1; oz++)
                                        {
                                            int sz = Mathf.Clamp(z + oz, 0, res - 1);
                                            int val = Mathf.Max(densityMap[sz, sx], densityMap[sx, sz]);
                                            if (val > densityAtPoint)
                                            {
                                                densityAtPoint = val;
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        // Exporta a textura em PNG apenas se a vegetação estiver REALMENTE presente no ponto mirado
                        if (densityAtPoint > 0 && proto.prototypeTexture != null)
                        {
                            string safeTexName = SanitizeFileName(proto.prototypeTexture.name);
                            string pngPath = Path.Combine(texturesDir, $"{safeTexName}.png");
                            if (!File.Exists(pngPath))
                            {
                                ExportTexture(proto.prototypeTexture, pngPath);
                            }
                        }

                        DetectedGrassInfo item = new DetectedGrassInfo
                        {
                            PrototypeName = protoName,
                            TextureName = texName,
                            PrefabName = prefabName,
                            MinHeight = minH,
                            MaxHeight = maxH,
                            MinWidth = minW,
                            MaxWidth = maxW,
                            DensityAtPoint = densityAtPoint,
                            Classification = classification,
                            IsDirectlyUnderCrosshair = densityAtPoint > 0,
                            TextureRef = proto.prototypeTexture
                        };

                        result.AllMapPrototypes.Add(item);

                        if (densityAtPoint > 0)
                        {
                            result.PresentAtPoint.Add(item);
                        }
                    }
                }

                // 2. Varredura de malhas 3D próximas ao ponto de impacto da mira
                Collider[] colliders = Physics.OverlapSphere(targetPoint, 2.0f);
                for (int c = 0; c < colliders.Length; c++)
                {
                    Collider col = colliders[c];
                    if (col == null || col.gameObject == null) continue;
                    string colName = col.gameObject.name;
                    string colLower = colName.ToLower();

                    if (colLower.Contains("grass") || colLower.Contains("fern") || colLower.Contains("weed") ||
                        colLower.Contains("plant") || colLower.Contains("reed") || colLower.Contains("bush") ||
                        col.gameObject.layer == LayerMask.NameToLayer("Foliage"))
                    {
                        string entry = $"{colName} (Layer: {LayerMask.LayerToName(col.gameObject.layer)}, Colisor: {col.GetType().Name})";
                        if (!result.NearbyMeshes.Contains(entry))
                        {
                            result.NearbyMeshes.Add(entry);
                        }
                    }
                }

                // Registro no VegetationClassifier com base na tecla pressionada
                List<string> processedNames = new List<string>();
                if (result.PresentAtPoint.Count > 0)
                {
                    for (int i = 0; i < result.PresentAtPoint.Count; i++)
                    {
                        string tex = result.PresentAtPoint[i].TextureName;
                        if (string.IsNullOrEmpty(tex) || tex == "Nenhuma")
                        {
                            tex = result.PresentAtPoint[i].PrototypeName;
                        }

                        if (registerAsNonCamouflage)
                        {
                            // Salvaguarda: em tufos mistos, não bloqueia acidentalmente folhagens já calibradas ou permitidas
                            if (result.PresentAtPoint.Count > 1 && (VegetationClassifier.IsExplicitlyAllowed(tex) || 
                                tex.ToLowerInvariant().Contains("grass2") || 
                                tex.ToLowerInvariant().Contains("grass5_512") || 
                                tex.ToLowerInvariant().Contains("grass6")))
                            {
                                continue;
                            }

                            VegetationClassifier.RegisterAsNonCamouflage(tex);
                            if (!processedNames.Contains(tex)) processedNames.Add(tex);
                        }
                        else if (registerAsCamouflage)
                        {
                            // Salvaguarda: em tufos mistos, não adiciona como camuflagem as plantas que o usuário quer ignorar
                            if (result.PresentAtPoint.Count > 1 && (
                                tex.ToLowerInvariant().Contains("field_grass") || 
                                tex.ToLowerInvariant().Contains("grass_new_1") || 
                                tex.ToLowerInvariant().Contains("grass_new_2") || 
                                tex.ToLowerInvariant().Contains("grass_new_3")))
                            {
                                continue;
                            }

                            VegetationClassifier.RegisterAsCamouflage(tex);
                            if (!processedNames.Contains(tex)) processedNames.Add(tex);
                        }
                    }
                }

                // Salva relatório em disco
                SaveInspectionReport(dumpDir, result);

                _lastResult = result;
                _displayEndTime = Time.time + 12f; // Exibe painel na tela por 12 segundos

                // Dispara notificação nativa EFT com status de ação
                string actionHeader = "";
                if (registerAsNonCamouflage)
                {
                    actionHeader = "🚫 [REGISTRADO: NÃO-CAMUFLAGEM (IGNORADO)]\n";
                }
                else if (registerAsCamouflage)
                {
                    actionHeader = "✅ [REGISTRADO: CAMUFLAGEM ATIVA]\n";
                }

                string notifText;
                if (result.PresentAtPoint.Count > 0)
                {
                    string namesStr = string.Join(", ", processedNames.ToArray());
                    var mainGrass = result.PresentAtPoint[0];
                    notifText = $"{actionHeader}Folhagens: {namesStr} | Altura: {mainGrass.MinHeight:F2}m-{mainGrass.MaxHeight:F2}m | Densidade: {mainGrass.DensityAtPoint}";
                }
                else
                {
                    notifText = $"{actionHeader}Mira no Chão: {hitName} | Nenhuma vegetação com densidade neste ponto.";
                }

                NotificationManagerClass.DisplayMessageNotification(notifText, ENotificationDurationType.Long);
                Plugin.LogSource?.LogInfo(notifText);
            }
            catch (Exception ex)
            {
                Plugin.LogSource?.LogError($"[TRL-CoreSight] Falha na inspeção de mira: {ex.Message}\n{ex.StackTrace}");
                NotificationManagerClass.DisplayWarningNotification($"[TRL-CoreSight] Erro na inspeção: {ex.Message}", ENotificationDurationType.Default);
            }
        }

        private void OnGUI()
        {
            if (ModConfig.EnableVegetationInspector != null && !ModConfig.EnableVegetationInspector.Value)
            {
                return;
            }

            if (Time.time > _displayEndTime || _lastResult == null) return;

            InitStyles();

            float panelWidth = 650f;
            float panelHeight = 420f;
            Rect panelRect = new Rect(20f, 60f, panelWidth, panelHeight);

            GUI.Box(panelRect, GUIContent.none, _panelStyle);

            GUILayout.BeginArea(new Rect(panelRect.x + 10f, panelRect.y + 10f, panelWidth - 20f, panelHeight - 20f));

            GUILayout.Label("🔬 TRL-CoreSight — CLASSIFICADOR DE VEGETAÇÃO ([ = Camuflar | ] = Ignorar)", _headerStyle);
            GUILayout.Space(4f);

            GUILayout.Label($"<b>Ponto Mirado:</b> ({_lastResult.AimWorldPoint.x:F1}, {_lastResult.AimWorldPoint.y:F1}, {_lastResult.AimWorldPoint.z:F1}) | <b>Distância:</b> {_lastResult.DistanceMeters:F1}m", _labelStyle);
            GUILayout.Label($"<b>Objeto de Colisão:</b> {_lastResult.HitObjectName} (Layer: {_lastResult.HitObjectLayer}, Colisor: {_lastResult.HitColliderType})", _labelStyle);
            GUILayout.Space(6f);

            _scrollPos = GUILayout.BeginScrollView(_scrollPos, GUILayout.Height(300f));

            // Seção 1: Vegetações Presentes no Ponto de Impacto
            GUILayout.Label($"<b>🌿 VEGETAÇÃO PRESENTE NA MIRA ({_lastResult.PresentAtPoint.Count} encontrada(s)):</b>", _highlightStyle);
            if (_lastResult.PresentAtPoint.Count == 0)
            {
                GUILayout.Label("  <i>Nenhuma folhagem/grama detectada com densidade no ponto exato (solo descoberto ou rocha).</i>", _labelStyle);
            }
            else
            {
                for (int i = 0; i < _lastResult.PresentAtPoint.Count; i++)
                {
                    var g = _lastResult.PresentAtPoint[i];
                    bool isBlocked = VegetationClassifier.IsBlocked(g.TextureName) || VegetationClassifier.IsBlocked(g.PrototypeName);
                    bool isAllowed = VegetationClassifier.IsExplicitlyAllowed(g.TextureName) || VegetationClassifier.IsExplicitlyAllowed(g.PrototypeName);

                    string badge;
                    if (isBlocked)
                    {
                        badge = "<color=#FF4444><b>[IGNORADO / NÃO-CAMUFLA]</b></color>";
                    }
                    else if (isAllowed)
                    {
                        badge = "<color=#44FF44><b>[CAMUFLAGEM ATIVA]</b></color>";
                    }
                    else
                    {
                        string cor = g.MaxHeight >= 0.60f ? "#55FF55" : (g.MaxHeight >= 0.35f ? "#FFFF55" : "#AAAAAA");
                        badge = $"<color={cor}><b>[{g.Classification}]</b></color>";
                    }

                    GUILayout.Label($"  • {badge} <b>Textura:</b> {g.TextureName} | <b>Prefab:</b> {g.PrefabName}", _labelStyle);
                    GUILayout.Label($"    <b>Altura:</b> {g.MinHeight:F2}m a {g.MaxHeight:F2}m | <b>Largura:</b> {g.MinWidth:F2}m a {g.MaxWidth:F2}m | <b>Densidade:</b> {g.DensityAtPoint}", _labelStyle);
                }
            }

            GUILayout.Space(8f);

            // Seção 2: Malhas 3D no local
            if (_lastResult.NearbyMeshes.Count > 0)
            {
                GUILayout.Label($"<b>🌲 MALHAS 3D / ARBUSTOS PRÓXIMOS ({_lastResult.NearbyMeshes.Count}):</b>", _highlightStyle);
                for (int i = 0; i < _lastResult.NearbyMeshes.Count; i++)
                {
                    GUILayout.Label($"  • {_lastResult.NearbyMeshes[i]}", _labelStyle);
                }
                GUILayout.Space(8f);
            }

            // Seção 3: Catálogo Completo de Protótipos de Grama do Mapa
            GUILayout.Label($"<b>📋 TODAS AS GRAMAS DO MAPA ({_lastResult.AllMapPrototypes.Count} protótipos GPUInstancer):</b>", _highlightStyle);
            for (int i = 0; i < _lastResult.AllMapPrototypes.Count; i++)
            {
                var g = _lastResult.AllMapPrototypes[i];
                string marker = g.IsDirectlyUnderCrosshair ? "<color=#FF5555><b>[AQUI]</b></color> " : "";
                GUILayout.Label($"  {i + 1}. {marker}<b>{g.TextureName}</b> (Alt: {g.MinHeight:F2}m-{g.MaxHeight:F2}m, Lar: {g.MinWidth:F2}m-{g.MaxWidth:F2}m) → <i>{g.Classification}</i>", _labelStyle);
            }

            GUILayout.EndScrollView();

            GUILayout.Label("<i>Relatório e texturas PNG salvos em: BepInEx/plugins/TRL-CoreSight/Dump/AimInspector/</i>", _labelStyle);

            GUILayout.EndArea();
        }

        private void InitStyles()
        {
            if (_panelStyle != null) return;

            Texture2D bgTex = new Texture2D(1, 1);
            bgTex.SetPixel(0, 0, new Color(0.08f, 0.09f, 0.11f, 0.92f));
            bgTex.Apply();

            _panelStyle = new GUIStyle(GUI.skin.box);
            _panelStyle.normal.background = bgTex;
            _panelStyle.border = new RectOffset(4, 4, 4, 4);

            _headerStyle = new GUIStyle();
            _headerStyle.fontSize = 14;
            _headerStyle.richText = true;
            _headerStyle.normal.textColor = new Color(1.0f, 0.8f, 0.2f, 1.0f);

            _labelStyle = new GUIStyle();
            _labelStyle.fontSize = 12;
            _labelStyle.richText = true;
            _labelStyle.normal.textColor = Color.white;

            _highlightStyle = new GUIStyle();
            _highlightStyle.fontSize = 13;
            _highlightStyle.richText = true;
            _highlightStyle.normal.textColor = new Color(0.4f, 0.9f, 1.0f, 1.0f);
        }

        private void SaveInspectionReport(string folder, AimInspectionResult res)
        {
            try
            {
                string filePath = Path.Combine(folder, "inspecao_mira_detalhes.txt");
                StringBuilder sb = new StringBuilder();

                sb.AppendLine("================================================================================");
                sb.AppendLine($"TRL-CoreSight — INSPEÇÃO DE VEGETAÇÃO DA MIRA ({res.Timestamp:yyyy-MM-dd HH:mm:ss})");
                sb.AppendLine("================================================================================");
                sb.AppendLine($"Ponto Mirado: ({res.AimWorldPoint.x:F3}, {res.AimWorldPoint.y:F3}, {res.AimWorldPoint.z:F3})");
                sb.AppendLine($"Distância: {res.DistanceMeters:F2} metros");
                sb.AppendLine($"Objeto Colidido: {res.HitObjectName}");
                sb.AppendLine($"Layer: {res.HitObjectLayer} | Tipo de Colisor: {res.HitColliderType}");
                sb.AppendLine();
                sb.AppendLine("--- VEGETAÇÕES PRESENTES NO PONTO MIRADO ---");
                if (res.PresentAtPoint.Count == 0)
                {
                    sb.AppendLine("Nenhuma folhagem de terreno detectada com densidade neste ponto.");
                }
                else
                {
                    foreach (var g in res.PresentAtPoint)
                    {
                        sb.AppendLine($"* Textura: {g.TextureName} | Prefab: {g.PrefabName}");
                        sb.AppendLine($"  Classificação: {g.Classification}");
                        sb.AppendLine($"  Altura Real: {g.MinHeight:F2}m a {g.MaxHeight:F2}m | Largura: {g.MinWidth:F2}m a {g.MaxWidth:F2}m");
                        sb.AppendLine($"  Densidade no Ponto: {g.DensityAtPoint}");
                        sb.AppendLine();
                    }
                }

                sb.AppendLine("--- MALHAS 3D / OBSTÁCULOS NO LOCAL ---");
                foreach (var m in res.NearbyMeshes)
                {
                    sb.AppendLine($"* {m}");
                }
                sb.AppendLine();

                sb.AppendLine("--- CATÁLOGO COMPLETO DE PROTÓTIPOS GPUINSTANCER DO MAPA (DISTINÇÃO DE TAMANHOS) ---");
                for (int i = 0; i < res.AllMapPrototypes.Count; i++)
                {
                    var g = res.AllMapPrototypes[i];
                    sb.AppendLine($"[{i + 1}] Textura: '{g.TextureName}', Prefab: '{g.PrefabName}', Protótipo: '{g.PrototypeName}'");
                    sb.AppendLine($"    Escala (detailScale): Altura=[{g.MinHeight:F2}m .. {g.MaxHeight:F2}m], Largura=[{g.MinWidth:F2}m .. {g.MaxWidth:F2}m]");
                    sb.AppendLine($"    Classificação: {g.Classification} | Presente na Mira: {(g.IsDirectlyUnderCrosshair ? "SIM" : "NÃO")}");
                }

                File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
            }
            catch (Exception ex)
            {
                Plugin.LogSource?.LogError($"[TRL-CoreSight] Erro ao gravar relatório de inspeção: {ex.Message}");
            }
        }

        private static void ExportTexture(Texture2D sourceTex, string targetFilePath)
        {
            try
            {
                int width = Mathf.Clamp(sourceTex.width, 16, 2048);
                int height = Mathf.Clamp(sourceTex.height, 16, 2048);

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
                Plugin.LogSource?.LogWarning($"[TRL-CoreSight] Não foi possível exportar textura '{sourceTex?.name}': {ex.Message}");
            }
        }

        private static string SanitizeFileName(string name)
        {
            if (string.IsNullOrEmpty(name)) return "unnamed";
            char[] invalid = Path.GetInvalidFileNameChars();
            StringBuilder sb = new StringBuilder(name.Length);
            for (int i = 0; i < name.Length; i++)
            {
                char c = name[i];
                bool isInvalid = false;
                for (int j = 0; j < invalid.Length; j++)
                {
                    if (c == invalid[j])
                    {
                        isInvalid = true;
                        break;
                    }
                }
                sb.Append(isInvalid ? '_' : c);
            }
            return sb.ToString();
        }
    }
}
