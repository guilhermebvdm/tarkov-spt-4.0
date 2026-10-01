using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using EFT;
using Comfort.Common;
using Newtonsoft.Json;
using SPT.Common.Http;
using TRLDynamicSpawn.Helpers;
using TRLDynamicSpawn.Models;
using EFT.Game.Spawning;

namespace TRLDynamicSpawn.Components
{
    public class MapReviewFile
    {
        public string MapName { get; set; }
        public string LastUpdatedUtc { get; set; }
        public int TotalPoints { get; set; }
        public int ApprovedCount { get; set; }
        public int RejectedCount { get; set; }
        public int PendingCount { get; set; }
        public Dictionary<string, SpawnPointReviewEntry> Reviews { get; set; } = new Dictionary<string, SpawnPointReviewEntry>(StringComparer.OrdinalIgnoreCase);
    }

    public class SpawnPointReviewEntry
    {
        public string Id { get; set; }
        public string Source { get; set; } // "Native", "MOAR_PMC", "MOAR_SCAV"
        public string ZoneName { get; set; }
        public Vector3Model OriginalPosition { get; set; }
        public Vector3Model AdjustedPosition { get; set; }
        public bool WasMoved { get; set; }
        public SpawnReviewStatus Status { get; set; } = SpawnReviewStatus.Pending;
        public string ReviewTimestampUtc { get; set; }
    }

    /// <summary>
    /// Gerenciador do Spawn Point Reviewer (Item de Backlog 015).
    /// Permite inspecionar visualmente todos os pontos de spawn (Nativos e MOAR) via câmera livre,
    /// realizar ajustes finos nas coordenadas 3D (Setas, PgUp/PgDn, Snap no Solo) e aprovar/reprovar com persistência JSON.
    /// </summary>
    public class SpawnPointReviewerManager : MonoBehaviour
    {
        public static SpawnPointReviewerManager Instance { get; private set; }

        public bool IsActive { get; private set; }
        public string CurrentMapName { get; private set; }

        private readonly List<SpawnMarkerVisual> _markers = new List<SpawnMarkerVisual>();
        private readonly Dictionary<string, SpawnMarkerVisual> _markersById = new Dictionary<string, SpawnMarkerVisual>(StringComparer.OrdinalIgnoreCase);

        private MapReviewFile _currentReviewFile;
        private SpawnMarkerVisual _hoveredMarker;
        private SpawnMarkerVisual _selectedMarker;
        private string _saveFilePath;
        private float _pollTimer = 0f;
        private float _feedbackTimer = 0f;
        private string _feedbackMessage = "";
        private string _feedbackColorHex = "50fa7b";

        public void ShowFeedback(string message, string colorHex = "50fa7b")
        {
            _feedbackMessage = message;
            _feedbackColorHex = colorHex;
            _feedbackTimer = 3.5f;
        }

        // Estilos de GUI
        private static GUIStyle _panelStyle;
        private static GUIStyle _titleStyle;
        private static GUIStyle _headerStyle;
        private static GUIStyle _infoStyle;
        private static GUIStyle _shortcutStyle;
        private static GUIStyle _highlightStyle;

        public static SpawnPointReviewerManager EnsureInstance()
        {
            if (Instance != null)
            {
                try
                {
                    if (Instance.gameObject != null) return Instance;
                }
                catch { }
            }

            var go = new GameObject("TRL_SpawnPointReviewerManager");
            Instance = go.AddComponent<SpawnPointReviewerManager>();
            DontDestroyOnLoad(go);
            return Instance;
        }

        public static void Enable()
        {
            EnsureInstance();
        }

        public static Camera GetActiveCamera()
        {
            Camera cam = Camera.main;
            if (cam != null && cam.enabled && cam.gameObject.activeInHierarchy) return cam;
            cam = Camera.current;
            if (cam != null && cam.enabled && cam.gameObject.activeInHierarchy) return cam;
            var allCams = Camera.allCameras;
            if (allCams != null)
            {
                for (int i = 0; i < allCams.Length; i++)
                {
                    if (allCams[i] != null && allCams[i].enabled && allCams[i].gameObject.activeInHierarchy)
                        return allCams[i];
                }
            }
            return null;
        }

        public static string ResolveCurrentMapName()
        {
            var gw = Singleton<GameWorld>.Instance;
            if (gw != null)
            {
                if (gw.MainPlayer != null && !string.IsNullOrEmpty(gw.MainPlayer.Location))
                    return gw.MainPlayer.Location;
                if (!string.IsNullOrEmpty(gw.LocationId))
                    return gw.LocationId;
                var anyPlayer = gw.RegisteredPlayers.OfType<Player>().FirstOrDefault(p => p != null && !string.IsNullOrEmpty(p.Location));
                if (anyPlayer != null) return anyPlayer.Location;
            }
            return "unknown_map";
        }

        public void StartReviewSession(string mapName)
        {
            if (string.IsNullOrEmpty(mapName) || mapName.Equals("unknown_map", StringComparison.OrdinalIgnoreCase))
            {
                mapName = ResolveCurrentMapName();
            }

            if (IsActive)
            {
                StopReviewSession();
            }

            CurrentMapName = mapName;

            try
            {
                string assemblyPath = typeof(Plugin).Assembly.Location;
                string baseDir = Path.GetDirectoryName(assemblyPath);
                string reviewsDir = Path.Combine(baseDir, "SpawnReviews");
                if (!Directory.Exists(reviewsDir))
                {
                    Directory.CreateDirectory(reviewsDir);
                }

                _saveFilePath = Path.Combine(reviewsDir, $"{mapName}.json");

                // Carrega ou inicializa o arquivo de review
                if (File.Exists(_saveFilePath))
                {
                    try
                    {
                        string json = File.ReadAllText(_saveFilePath);
                        _currentReviewFile = JsonConvert.DeserializeObject<MapReviewFile>(json);
                    }
                    catch (Exception ex)
                    {
                        Plugin.LogSource?.LogError($"[TRL-DynamicSpawn] Failed to parse existing review file '{_saveFilePath}': {ex.Message}");
                    }
                }

                if (_currentReviewFile == null)
                {
                    _currentReviewFile = new MapReviewFile
                    {
                        MapName = mapName,
                        LastUpdatedUtc = DateTime.UtcNow.ToString("o")
                    };
                }

                // 1. Coleta pontos nativos de todas as BotZones da raid
                CollectNativeSpawnPoints();

                // 2. Coleta pontos nativos de Player (Infiltração de jogador humano)
                CollectPlayerSpawnPoints();

                // 3. Coleta pontos MOAR (PMC e Scav)
                CollectMoarSpawnPoints(mapName);

                // 4. Atualiza contadores e persiste estado inicial com todos os pontos descobertos
                SaveReviewFile();

                IsActive = true;
                Plugin.LogSource?.LogInfo($"[TRL-DynamicSpawn] SpawnPointReviewer started for map '{mapName}'. Total points instantiated: {_markers.Count}");
            }
            catch (Exception ex)
            {
                Plugin.LogSource?.LogError($"[TRL-DynamicSpawn] Error starting review session: {ex}");
            }
        }

        private void CollectNativeSpawnPoints()
        {
            var zones = ZoneCache.GetAllZones();
            if (zones == null || zones.Count == 0)
            {
                ZoneCache.Initialize();
                zones = ZoneCache.GetAllZones();
            }

            if (zones == null || zones.Count == 0)
            {
                var sceneZones = LocationScene.GetAllObjects<BotZone>();
                if (sceneZones != null) zones = sceneZones.ToList();
            }

            Plugin.LogSource?.LogInfo($"[TRL-DynamicSpawn] CollectNativeSpawnPoints: Found {zones?.Count ?? 0} BotZones.");
            if (zones == null) return;

            foreach (var zone in zones)
            {
                if (zone == null || zone.SpawnPoints == null) continue;

                string zoneName = zone.NameZone ?? "UnknownZone";

                for (int i = 0; i < zone.SpawnPoints.Length; i++)
                {
                    var sp = zone.SpawnPoints[i];
                    if (sp == null) continue;

                    string spId = !string.IsNullOrEmpty(sp.Id) ? sp.Id : $"idx_{i}";
                    string uniqueId = $"Native_{zoneName}_{spId}";

                    if (_markersById.ContainsKey(uniqueId)) continue;

                    Vector3 originalPos = sp.Position;
                    Vector3 currentPos = originalPos;
                    SpawnReviewStatus status = SpawnReviewStatus.Pending;

                    if (_currentReviewFile.Reviews.TryGetValue(uniqueId, out var entry) && entry != null)
                    {
                        status = entry.Status;
                        if (entry.WasMoved && entry.AdjustedPosition != null)
                        {
                            currentPos = new Vector3(entry.AdjustedPosition.X, entry.AdjustedPosition.Y, entry.AdjustedPosition.Z);
                        }
                    }

                    var marker = CreateMarkerObject(uniqueId, "Native", zoneName, originalPos, currentPos, status);
                    if (marker != null)
                    {
                        _markers.Add(marker);
                        _markersById[uniqueId] = marker;
                    }
                }
            }
        }

        private void CollectPlayerSpawnPoints()
        {
            try
            {
                IEnumerable<SpawnPointMarker> markers = null;
                try
                {
                    markers = LocationScene.GetAllObjects<SpawnPointMarker>();
                }
                catch (Exception ex)
                {
                    Plugin.LogSource?.LogWarning($"[TRL-DynamicSpawn] LocationScene.GetAllObjects<SpawnPointMarker> failed: {ex.Message}");
                }

                if (markers == null)
                {
                    markers = UnityEngine.Object.FindObjectsOfType<SpawnPointMarker>();
                }

                if (markers == null) return;

                int addedCount = 0;
                foreach (var m in markers)
                {
                    if (m == null || m.SpawnPoint == null) continue;

                    // Filtra marcadores de Player (Infiltração humana BSG)
                    if (!m.SpawnPoint.Categories.ContainPlayerCategory()) continue;

                    string spId = !string.IsNullOrEmpty(m.SpawnPoint.Id) ? m.SpawnPoint.Id : m.gameObject.name;
                    string uniqueId = $"Player_{spId}";

                    if (_markersById.ContainsKey(uniqueId)) continue;

                    Vector3 originalPos = m.SpawnPoint.Position;
                    Vector3 currentPos = originalPos;
                    SpawnReviewStatus status = SpawnReviewStatus.Pending;

                    if (_currentReviewFile.Reviews.TryGetValue(uniqueId, out var entry) && entry != null)
                    {
                        status = entry.Status;
                        if (entry.WasMoved && entry.AdjustedPosition != null)
                        {
                            currentPos = new Vector3(entry.AdjustedPosition.X, entry.AdjustedPosition.Y, entry.AdjustedPosition.Z);
                        }
                    }

                    string infiltration = !string.IsNullOrEmpty(m.SpawnPoint.Infiltration) ? m.SpawnPoint.Infiltration : "PlayerDeploy";

                    var marker = CreateMarkerObject(uniqueId, "PLAYER", infiltration, originalPos, currentPos, status);
                    if (marker != null)
                    {
                        _markers.Add(marker);
                        _markersById[uniqueId] = marker;
                        addedCount++;
                    }
                }

                Plugin.LogSource?.LogInfo($"[TRL-DynamicSpawn] CollectPlayerSpawnPoints: Found and instantiated {addedCount} Player spawn points.");
            }
            catch (Exception ex)
            {
                Plugin.LogSource?.LogError($"[TRL-DynamicSpawn] Error in CollectPlayerSpawnPoints: {ex}");
            }
        }

        private void CollectMoarSpawnPoints(string mapName)
        {
            // Garante que as listas MOAR estejam carregadas
            EnsureMoarSpawnsLoaded();

            // PMC Spawns
            if (DynamicSpawnManager.PmcSpawns != null && DynamicSpawnManager.PmcSpawns.TryGetValue(mapName, out var pmcPoints) && pmcPoints != null)
            {
                for (int i = 0; i < pmcPoints.Count; i++)
                {
                    var p = pmcPoints[i];
                    if (p == null) continue;

                    string uniqueId = $"MOAR_PMC_{mapName}_{i}";
                    if (_markersById.ContainsKey(uniqueId)) continue;

                    Vector3 originalPos = new Vector3(p.X, p.Y, p.Z);
                    Vector3 currentPos = originalPos;
                    SpawnReviewStatus status = SpawnReviewStatus.Pending;

                    if (_currentReviewFile.Reviews.TryGetValue(uniqueId, out var entry) && entry != null)
                    {
                        status = entry.Status;
                        if (entry.WasMoved && entry.AdjustedPosition != null)
                        {
                            currentPos = new Vector3(entry.AdjustedPosition.X, entry.AdjustedPosition.Y, entry.AdjustedPosition.Z);
                        }
                    }                    // 1. BotZone fixa persistida no JSON (se já foi salva e for válida E NÃO for de sniper)
                    string zoneName = null;
                    if (entry != null && !string.IsNullOrEmpty(entry.ZoneName) && 
                        !entry.ZoneName.Equals("CustomPMC", StringComparison.OrdinalIgnoreCase) && 
                        !entry.ZoneName.Equals("CustomScav", StringComparison.OrdinalIgnoreCase) && 
                        !entry.ZoneName.Equals("UnknownZone", StringComparison.OrdinalIgnoreCase) &&
                        !IsSniperZoneName(entry.ZoneName))
                    {
                        zoneName = entry.ZoneName;
                    }
                    else
                    {
                        // Fallback Inteligente: Calcula a BotZone REGULAR física real mais próxima (excluindo zonas de sniper)
                        zoneName = ResolveClosestBotZoneName(originalPos, isSniper: false, out float _);
                        if (entry != null)
                        {
                            entry.ZoneName = zoneName;
                        }
                    }

                    var marker = CreateMarkerObject(uniqueId, "MOAR_PMC", zoneName, originalPos, currentPos, status);
                    if (marker != null)
                    {
                        _markers.Add(marker);
                        _markersById[uniqueId] = marker;
                    }
                }
            }

            // Scav Spawns
            if (DynamicSpawnManager.ScavSpawns != null && DynamicSpawnManager.ScavSpawns.TryGetValue(mapName, out var scavPoints) && scavPoints != null)
            {
                for (int i = 0; i < scavPoints.Count; i++)
                {
                    var p = scavPoints[i];
                    if (p == null) continue;

                    string uniqueId = $"MOAR_SCAV_{mapName}_{i}";
                    if (_markersById.ContainsKey(uniqueId)) continue;

                    Vector3 originalPos = new Vector3(p.X, p.Y, p.Z);
                    Vector3 currentPos = originalPos;
                    SpawnReviewStatus status = SpawnReviewStatus.Pending;

                    if (_currentReviewFile.Reviews.TryGetValue(uniqueId, out var entry) && entry != null)
                    {
                        status = entry.Status;
                        if (entry.WasMoved && entry.AdjustedPosition != null)
                        {
                            currentPos = new Vector3(entry.AdjustedPosition.X, entry.AdjustedPosition.Y, entry.AdjustedPosition.Z);
                        }
                    }

                    // 1. BotZone fixa persistida no JSON (se já foi salva e for válida E NÃO for de sniper)
                    string zoneName = null;
                    if (entry != null && !string.IsNullOrEmpty(entry.ZoneName) && 
                        !entry.ZoneName.Equals("CustomPMC", StringComparison.OrdinalIgnoreCase) && 
                        !entry.ZoneName.Equals("CustomScav", StringComparison.OrdinalIgnoreCase) && 
                        !entry.ZoneName.Equals("UnknownZone", StringComparison.OrdinalIgnoreCase) &&
                        !IsSniperZoneName(entry.ZoneName))
                    {
                        zoneName = entry.ZoneName;
                    }
                    else
                    {
                        // Fallback Inteligente: Calcula a BotZone REGULAR física real mais próxima (excluindo zonas de sniper)
                        zoneName = ResolveClosestBotZoneName(originalPos, isSniper: false, out float _);
                        if (entry != null)
                        {
                            entry.ZoneName = zoneName;
                        }
                    }

                    var marker = CreateMarkerObject(uniqueId, "MOAR_SCAV", zoneName, originalPos, currentPos, status);
                    if (marker != null)
                    {
                        _markers.Add(marker);
                        _markersById[uniqueId] = marker;
                    }
                }
            }

            // Sniper Spawns
            if (DynamicSpawnManager.SniperSpawns != null && DynamicSpawnManager.SniperSpawns.TryGetValue(mapName, out var sniperPoints) && sniperPoints != null)
            {
                for (int i = 0; i < sniperPoints.Count; i++)
                {
                    var p = sniperPoints[i];
                    if (p == null) continue;

                    string uniqueId = $"MOAR_SNIPER_{mapName}_{i}";
                    if (_markersById.ContainsKey(uniqueId)) continue;

                    Vector3 originalPos = new Vector3(p.X, p.Y, p.Z);
                    Vector3 currentPos = originalPos;
                    SpawnReviewStatus status = SpawnReviewStatus.Pending;

                    if (_currentReviewFile.Reviews.TryGetValue(uniqueId, out var entry) && entry != null)
                    {
                        status = entry.Status;
                        if (entry.WasMoved && entry.AdjustedPosition != null)
                        {
                            currentPos = new Vector3(entry.AdjustedPosition.X, entry.AdjustedPosition.Y, entry.AdjustedPosition.Z);
                        }
                    }

                    // 1. BotZone fixa persistida no JSON (se já foi salva e for ESTRITAMENTE de sniper)
                    string zoneName = null;
                    if (entry != null && !string.IsNullOrEmpty(entry.ZoneName) && 
                        !entry.ZoneName.Equals("CustomSniper", StringComparison.OrdinalIgnoreCase) && 
                        !entry.ZoneName.Equals("UnknownZone", StringComparison.OrdinalIgnoreCase) &&
                        IsSniperZoneName(entry.ZoneName))
                    {
                        zoneName = entry.ZoneName;
                    }
                    else
                    {
                        // Fallback Inteligente: Calcula a SniperZone física real mais próxima
                        zoneName = ResolveClosestBotZoneName(originalPos, isSniper: true, out float _);
                        if (entry != null)
                        {
                            entry.ZoneName = zoneName;
                        }
                    }

                    var marker = CreateMarkerObject(uniqueId, "MOAR_SNIPER", zoneName, originalPos, currentPos, status);
                    if (marker != null)
                    {
                        _markers.Add(marker);
                        _markersById[uniqueId] = marker;
                    }
                }
            }
        }

        private void EnsureMoarSpawnsLoaded()
        {
            if (DynamicSpawnManager.PmcSpawns == null || DynamicSpawnManager.PmcSpawns.Count == 0)
            {
                try
                {
                    string pmcJson = RequestHandler.GetJson("/trldynamicspawn/getPmcSpawns");
                    if (!string.IsNullOrEmpty(pmcJson))
                    {
                        DynamicSpawnManager.PmcSpawns = JsonConvert.DeserializeObject<Dictionary<string, List<Vector3Model>>>(pmcJson) ?? new Dictionary<string, List<Vector3Model>>();
                    }
                }
                catch (Exception ex)
                {
                    Plugin.LogSource?.LogWarning($"[TRL-DynamicSpawn] Could not fetch PMC spawns from server: {ex.Message}");
                }
            }

            if (DynamicSpawnManager.ScavSpawns == null || DynamicSpawnManager.ScavSpawns.Count == 0)
            {
                try
                {
                    string scavJson = RequestHandler.GetJson("/trldynamicspawn/getScavSpawns");
                    if (!string.IsNullOrEmpty(scavJson))
                    {
                        DynamicSpawnManager.ScavSpawns = JsonConvert.DeserializeObject<Dictionary<string, List<Vector3Model>>>(scavJson) ?? new Dictionary<string, List<Vector3Model>>();
                    }
                }
                catch (Exception ex)
                {
                    Plugin.LogSource?.LogWarning($"[TRL-DynamicSpawn] Could not fetch Scav spawns from server: {ex.Message}");
                }
            }

            if (DynamicSpawnManager.SniperSpawns == null || DynamicSpawnManager.SniperSpawns.Count == 0)
            {
                try
                {
                    string sniperJson = RequestHandler.GetJson("/trldynamicspawn/getSniperSpawns");
                    if (!string.IsNullOrEmpty(sniperJson))
                    {
                        DynamicSpawnManager.SniperSpawns = JsonConvert.DeserializeObject<Dictionary<string, List<Vector3Model>>>(sniperJson) ?? new Dictionary<string, List<Vector3Model>>();
                    }
                }
                catch (Exception ex)
                {
                    Plugin.LogSource?.LogWarning($"[TRL-DynamicSpawn] Could not fetch Sniper spawns from server: {ex.Message}");
                }
            }
        }

        private SpawnMarkerVisual CreateMarkerObject(string id, string source, string zone, Vector3 originalPos, Vector3 currentPos, SpawnReviewStatus status)
        {
            try
            {
                var markerObj = new GameObject($"SpawnMarker_{id}");
                var visual = markerObj.AddComponent<SpawnMarkerVisual>();
                visual.Initialize(id, source, zone, originalPos, status);
                if ((currentPos - originalPos).sqrMagnitude > 0.001f)
                {
                    visual.SetPosition(currentPos);
                }
                return visual;
            }
            catch (Exception ex)
            {
                Plugin.LogSource?.LogError($"[TRL-DynamicSpawn] Failed to create visual marker '{id}': {ex}");
                return null;
            }
        }

        public static bool IsSniperZoneName(string zoneName)
        {
            if (string.IsNullOrEmpty(zoneName)) return false;
            if (zoneName.IndexOf("snip", StringComparison.OrdinalIgnoreCase) >= 0 || 
                zoneName.IndexOf("marksman", StringComparison.OrdinalIgnoreCase) >= 0)
                return true;

            var allZones = ZoneCache.GetAllZones();
            if (allZones != null)
            {
                for (int i = 0; i < allZones.Count; i++)
                {
                    var z = allZones[i];
                    if (z != null && string.Equals(z.NameZone, zoneName, StringComparison.OrdinalIgnoreCase))
                    {
                        return SpawnPointHelper.IsSniperZone(z);
                    }
                }
            }
            return false;
        }

        public static string ResolveClosestBotZoneName(Vector3 pos, bool isSniper, out float distance)
        {
            distance = float.MaxValue;
            var candidateZones = isSniper ? ZoneCache.GetSniperZones() : ZoneCache.GetRegularZones();
            if (candidateZones == null || candidateZones.Count == 0)
            {
                candidateZones = ZoneCache.GetAllZones();
            }

            if (candidateZones == null || candidateZones.Count == 0) return "UnknownZone";

            BotZone bestZone = null;
            float bestDistSq = float.MaxValue;

            for (int i = 0; i < candidateZones.Count; i++)
            {
                var z = candidateZones[i];
                if (z == null) continue;

                // Restrição Absoluta: Spawns regulares terrestres (PMC/Scav) NUNCA podem receber zona de sniper!
                if (!isSniper && SpawnPointHelper.IsSniperZone(z)) continue;

                // Restrição de Sniper: Spawns de sniper só aceitam SniperZone se houver alguma na lista
                if (isSniper && !SpawnPointHelper.IsSniperZone(z) && candidateZones.Any(cz => cz != null && SpawnPointHelper.IsSniperZone(cz))) continue;

                Vector3 center = z.transform.position;
                if (z.SpawnPoints != null && z.SpawnPoints.Length > 0)
                {
                    Vector3 sum = Vector3.zero;
                    int count = 0;
                    for (int spIdx = 0; spIdx < z.SpawnPoints.Length; spIdx++)
                    {
                        if (z.SpawnPoints[spIdx] != null)
                        {
                            sum += z.SpawnPoints[spIdx].Position;
                            count++;
                        }
                    }
                    if (count > 0) center = sum / count;
                }

                float distSq = (pos - center).sqrMagnitude;
                if (distSq < bestDistSq)
                {
                    bestDistSq = distSq;
                    bestZone = z;
                }
            }

            if (bestZone != null && !string.IsNullOrEmpty(bestZone.NameZone))
            {
                distance = Mathf.Sqrt(bestDistSq);
                return bestZone.NameZone;
            }

            return "UnknownZone";
        }

        public static bool EvaluatePointCriteria(Vector3 pos, out string closestZoneName, out float distToZone, out string failureReason, bool isSniper = false)
        {
            distToZone = float.MaxValue;
            failureReason = null;

            // 1. Verificação de NavMesh
            bool hasNavMesh = UnityEngine.AI.NavMesh.SamplePosition(pos, out UnityEngine.AI.NavMeshHit navHit, 1.2f, UnityEngine.AI.NavMesh.AllAreas);
            if (!hasNavMesh)
            {
                closestZoneName = ResolveClosestBotZoneName(pos, isSniper, out distToZone);
                failureReason = "Sem NavMesh próximo (<1.2m)";
                return false;
            }

            // 2. Verificação de Solo Sólido e cota Y
            int groundMask = LayerMaskClass.HighPolyWithTerrainMask | LayerMaskClass.PlayerStaticCollisionsMask;
            Vector3 rayOrigin = pos + Vector3.up * 0.5f;
            if (!Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit groundHit, 1.5f, groundMask))
            {
                closestZoneName = ResolveClosestBotZoneName(pos, isSniper, out distToZone);
                failureReason = "Sem chão sólido detectado abaixo";
                return false;
            }

            float deltaY = Mathf.Abs(groundHit.point.y - pos.y);
            if (deltaY > 0.40f)
            {
                closestZoneName = ResolveClosestBotZoneName(pos, isSniper, out distToZone);
                failureReason = $"Cota Y fora de nível ({deltaY:F2}m do piso - use [End])";
                return false;
            }

            // 3. Verificação de BotZone mais próxima
            closestZoneName = ResolveClosestBotZoneName(pos, isSniper, out distToZone);
            if (string.IsNullOrEmpty(closestZoneName) || closestZoneName.Equals("UnknownZone", StringComparison.OrdinalIgnoreCase))
            {
                failureReason = "Nenhuma BotZone encontrada no mapa";
                return false;
            }

            if (distToZone > 150f)
            {
                failureReason = $"Zona '{closestZoneName}' distante ({distToZone:F0}m > 150m)";
                return false;
            }

            return true;
        }

        public void CreateGhostSpawnPoint()
        {
            var cam = GetActiveCamera();
            if (cam == null) return;

            // Posiciona o novo ponto no chão a 8 metros à frente da câmera
            Vector3 forwardPos = cam.transform.position + cam.transform.forward * 8f;
            int groundMask = LayerMaskClass.HighPolyWithTerrainMask | LayerMaskClass.PlayerStaticCollisionsMask;
            if (Physics.Raycast(forwardPos + Vector3.up * 2f, Vector3.down, out RaycastHit hit, 20f, groundMask))
            {
                forwardPos.y = hit.point.y;
            }

            string pointId = $"CUSTOM_{CurrentMapName}_{DateTime.UtcNow.Ticks % 100000}";
            string zoneName = ResolveClosestBotZoneName(forwardPos, isSniper: false, out float _);
            EvaluatePointCriteria(forwardPos, out _, out _, out string failReason, isSniper: false);

            var marker = CreateMarkerObject(pointId, "Custom_User", zoneName, forwardPos, forwardPos, SpawnReviewStatus.Pending);
            if (marker != null)
            {
                bool criteriaMet = string.IsNullOrEmpty(failReason);
                marker.SetGhostState(true, criteriaMet, failReason);
                _markers.Add(marker);
                _markersById[pointId] = marker;
                _selectedMarker = marker; // Trava seleção imediatamente nele para ajuste fino
                Plugin.LogSource?.LogInfo($"[TRL-DynamicSpawn] Ghost spawn point created: {pointId} at {forwardPos} (Zone: {zoneName})");
            }
        }

        public void StopReviewSession()
        {
            if (!IsActive) return;

            SaveReviewFile();

            for (int i = 0; i < _markers.Count; i++)
            {
                if (_markers[i] != null && _markers[i].gameObject != null)
                {
                    Destroy(_markers[i].gameObject);
                }
            }

            _markers.Clear();
            _markersById.Clear();
            _hoveredMarker = null;
            _selectedMarker = null;
            IsActive = false;

            Plugin.LogSource?.LogInfo("[TRL-DynamicSpawn] SpawnPointReviewer session stopped.");
        }

        public void SaveReviewFile()
        {
            if (_currentReviewFile == null || string.IsNullOrEmpty(_saveFilePath)) return;

            try
            {
                _currentReviewFile.LastUpdatedUtc = DateTime.UtcNow.ToString("o");
                _currentReviewFile.TotalPoints = _markers.Count;
                _currentReviewFile.ApprovedCount = _markers.Count(m => m != null && m.Status == SpawnReviewStatus.Approved);
                _currentReviewFile.RejectedCount = _markers.Count(m => m != null && m.Status == SpawnReviewStatus.Rejected);
                _currentReviewFile.PendingCount = _markers.Count(m => m != null && m.Status == SpawnReviewStatus.Pending);

                foreach (var m in _markers)
                {
                    if (m == null) continue;

                    if (!_currentReviewFile.Reviews.TryGetValue(m.PointId, out var entry) || entry == null)
                    {
                        entry = new SpawnPointReviewEntry
                        {
                            Id = m.PointId,
                            Source = m.Source,
                            ZoneName = m.ZoneName,
                            OriginalPosition = new Vector3Model { X = m.OriginalPosition.x, Y = m.OriginalPosition.y, Z = m.OriginalPosition.z }
                        };
                        _currentReviewFile.Reviews[m.PointId] = entry;
                    }

                    entry.ZoneName = m.ZoneName;
                    entry.AdjustedPosition = new Vector3Model { X = m.CurrentPosition.x, Y = m.CurrentPosition.y, Z = m.CurrentPosition.z };
                    entry.WasMoved = m.WasMoved;
                    entry.Status = m.Status;
                    entry.ReviewTimestampUtc = DateTime.UtcNow.ToString("o");
                }

                string json = JsonConvert.SerializeObject(_currentReviewFile, Formatting.Indented);
                File.WriteAllText(_saveFilePath, json);
            }
            catch (Exception ex)
            {
                Plugin.LogSource?.LogError($"[TRL-DynamicSpawn] Error saving review file '{_saveFilePath}': {ex.Message}");
            }
        }

        public void ResetSelectedSpawnPoint()
        {
            var target = _selectedMarker ?? _hoveredMarker;
            if (target == null) return;

            if (target.IsGhost)
            {
                _markers.Remove(target);
                _markersById.Remove(target.PointId);
                if (_selectedMarker == target) _selectedMarker = null;
                if (_hoveredMarker == target) _hoveredMarker = null;
                Destroy(target.gameObject);
                ShowFeedback($"Ghost '{target.PointId}' descartado.", "ffb86c");
                Plugin.LogSource?.LogInfo($"[TRL-DynamicSpawn] Discarded ghost point '{target.PointId}'.");
                return;
            }

            target.SetPosition(target.OriginalPosition);
            target.SetStatus(SpawnReviewStatus.Pending);

            if (_currentReviewFile != null && _currentReviewFile.Reviews.TryGetValue(target.PointId, out var entry) && entry != null)
            {
                entry.AdjustedPosition = new Vector3Model { X = target.OriginalPosition.x, Y = target.OriginalPosition.y, Z = target.OriginalPosition.z };
                entry.WasMoved = false;
                entry.Status = SpawnReviewStatus.Pending;
                entry.ReviewTimestampUtc = DateTime.UtcNow.ToString("o");
            }

            SaveReviewFile();
            ShowFeedback($"Ponto '{target.PointId}' restaurado ao padrão!", "50fa7b");
            Plugin.LogSource?.LogInfo($"[TRL-DynamicSpawn] Reset spawn point '{target.PointId}' to original factory position {target.OriginalPosition}");
        }

        public void DeleteSelectedSpawnPoint()
        {
            var target = _selectedMarker ?? _hoveredMarker;
            if (target == null) return;

            // 1. Se for um holograma (Ghost) ainda não confirmado
            if (target.IsGhost)
            {
                _markers.Remove(target);
                _markersById.Remove(target.PointId);
                if (_selectedMarker == target) _selectedMarker = null;
                if (_hoveredMarker == target) _hoveredMarker = null;
                Destroy(target.gameObject);
                ShowFeedback($"Holograma '{target.PointId}' descartado.", "ffb86c");
                Plugin.LogSource?.LogInfo($"[TRL-DynamicSpawn] Discarded ghost point '{target.PointId}'.");
                return;
            }

            // 2. Se for um ponto customizado criado pelo usuário
            bool isCustom = (!string.IsNullOrEmpty(target.PointId) && target.PointId.StartsWith("CUSTOM_", StringComparison.OrdinalIgnoreCase)) ||
                            (!string.IsNullOrEmpty(target.Source) && target.Source.IndexOf("Custom", StringComparison.OrdinalIgnoreCase) >= 0);

            if (isCustom)
            {
                string pointId = target.PointId;
                _markers.Remove(target);
                _markersById.Remove(pointId);
                if (_selectedMarker == target) _selectedMarker = null;
                if (_hoveredMarker == target) _hoveredMarker = null;

                if (_currentReviewFile != null && _currentReviewFile.Reviews.ContainsKey(pointId))
                {
                    _currentReviewFile.Reviews.Remove(pointId);
                }

                Destroy(target.gameObject);
                SaveReviewFile();
                ShowFeedback($"Ponto customizado '{pointId}' EXCLUÍDO definitivamente!", "ff5555");
                Plugin.LogSource?.LogInfo($"[TRL-DynamicSpawn] Permanently deleted custom spawn point '{pointId}'.");
                return;
            }

            // 3. Se for um ponto nativo da BSG ou MOAR:
            // Pontos de mapa oficiais não podem ser apagados fisicamente dos arquivos do jogo, mas são marcados como REPROVADOS.
            target.SetStatus(SpawnReviewStatus.Rejected);
            SaveReviewFile();
            ShowFeedback($"Ponto nativo não pode ser apagado fisicamente. Marcado como REPROVADO.", "ffb86c");
            Plugin.LogSource?.LogInfo($"[TRL-DynamicSpawn] Native/MOAR point '{target.PointId}' cannot be physically deleted. Marked as Rejected.");
        }

        public void ResetAllSpawnPoints()
        {
            var ghosts = _markers.Where(m => m != null && m.IsGhost).ToList();
            foreach (var g in ghosts)
            {
                _markers.Remove(g);
                _markersById.Remove(g.PointId);
                Destroy(g.gameObject);
            }
            if (_selectedMarker != null && _selectedMarker.IsGhost) _selectedMarker = null;
            if (_hoveredMarker != null && _hoveredMarker.IsGhost) _hoveredMarker = null;

            int count = 0;
            for (int i = 0; i < _markers.Count; i++)
            {
                var m = _markers[i];
                if (m == null) continue;

                m.SetPosition(m.OriginalPosition);
                m.SetStatus(SpawnReviewStatus.Pending);
                count++;

                if (_currentReviewFile != null && _currentReviewFile.Reviews.TryGetValue(m.PointId, out var entry) && entry != null)
                {
                    entry.AdjustedPosition = new Vector3Model { X = m.OriginalPosition.x, Y = m.OriginalPosition.y, Z = m.OriginalPosition.z };
                    entry.WasMoved = false;
                    entry.Status = SpawnReviewStatus.Pending;
                    entry.ReviewTimestampUtc = DateTime.UtcNow.ToString("o");
                }
            }

            SaveReviewFile();
            ShowFeedback($"Mapa '{CurrentMapName}' resetado! {count} pontos restaurados.", "50fa7b");
            Plugin.LogSource?.LogInfo($"[TRL-DynamicSpawn] Reset all {count} spawn points for map '{CurrentMapName}' to factory original.");
        }

        public void ElevateAllVanillaSpawnPoints(float offset = 0.15f)
        {
            int count = 0;
            for (int i = 0; i < _markers.Count; i++)
            {
                var m = _markers[i];
                if (m == null) continue;

                if (string.Equals(m.Source, "Native", StringComparison.OrdinalIgnoreCase))
                {
                    Vector3 elevatedPos = m.CurrentPosition;
                    elevatedPos.y += offset;
                    m.SetPosition(elevatedPos);
                    count++;

                    if (_currentReviewFile != null && _currentReviewFile.Reviews.TryGetValue(m.PointId, out var entry) && entry != null)
                    {
                        entry.AdjustedPosition = new Vector3Model { X = elevatedPos.x, Y = elevatedPos.y, Z = elevatedPos.z };
                        entry.WasMoved = m.WasMoved;
                        entry.ReviewTimestampUtc = DateTime.UtcNow.ToString("o");
                    }
                }
            }

            SaveReviewFile();
            ShowFeedback($"Elevação de +{offset:F2}m aplicada a {count} spawns Vanilla!", "50fa7b");
            Plugin.LogSource?.LogInfo($"[TRL-DynamicSpawn] Batch elevated {count} native vanilla spawns by +{offset:F2}m on '{CurrentMapName}'.");
        }

        private static readonly string[] SpawnTypeCategories = new[]
        {
            "PMC",
            "SCAV",
            "SNIPER",
            "BOSS",
            "ROGUE",
            "RAIDER",
            "PLAYER"
        };

        public void CycleSelectedSpawnType()
        {
            var target = _selectedMarker ?? _hoveredMarker;
            if (target == null) return;

            string currentSource = target.Source ?? "";
            int currentIndex = -1;
            for (int i = 0; i < SpawnTypeCategories.Length; i++)
            {
                if (currentSource.IndexOf(SpawnTypeCategories[i], StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    currentIndex = i;
                    break;
                }
            }

            int nextIndex = (currentIndex + 1) % SpawnTypeCategories.Length;
            string newType = SpawnTypeCategories[nextIndex];

            // Preserva o prefixo da fonte ("MOAR_", "CUSTOM_", "NATIVE_")
            string prefix = "MOAR_";
            if (currentSource.StartsWith("CUSTOM", StringComparison.OrdinalIgnoreCase)) prefix = "CUSTOM_";
            else if (currentSource.StartsWith("NATIVE", StringComparison.OrdinalIgnoreCase)) prefix = "NATIVE_";

            string newSource = $"{prefix}{newType}";
            target.SetSource(newSource);

            // Ajusta a BotZone com base na nova categoria (Sniper vs Regular/Player)
            bool isSniper = newType == "SNIPER";
            string newZone = ResolveClosestBotZoneName(target.CurrentPosition, isSniper, out float dist);
            target.SetZoneName(newZone);

            // Persiste no arquivo de review
            if (_currentReviewFile != null)
            {
                if (!_currentReviewFile.Reviews.TryGetValue(target.PointId, out var entry) || entry == null)
                {
                    entry = new SpawnPointReviewEntry
                    {
                        Id = target.PointId,
                        OriginalPosition = new Vector3Model { X = target.OriginalPosition.x, Y = target.OriginalPosition.y, Z = target.OriginalPosition.z }
                    };
                    _currentReviewFile.Reviews[target.PointId] = entry;
                }

                entry.Source = newSource;
                entry.ZoneName = newZone;
                entry.AdjustedPosition = new Vector3Model { X = target.CurrentPosition.x, Y = target.CurrentPosition.y, Z = target.CurrentPosition.z };
                entry.WasMoved = target.WasMoved;
                entry.ReviewTimestampUtc = DateTime.UtcNow.ToString("o");
            }

            SaveReviewFile();

            string colorHex = isSniper ? "ff79c6" : (newType == "BOSS" ? "ff5555" : (newType == "ROGUE" ? "bd93f9" : (newType == "RAIDER" ? "ffb86c" : (newType == "PLAYER" ? "50b4ff" : "50fa7b"))));
            string typeDesc = newType == "PLAYER" ? "PLAYER (Humano)" : newType;
            ShowFeedback($"Tipo: [{typeDesc}] | Zona: {newZone} ({dist:F0}m)", colorHex);
            Plugin.LogSource?.LogInfo($"[TRL-DynamicSpawn] Cycled spawn point '{target.PointId}' to '{newSource}' in zone '{newZone}' ({dist:F1}m)");
        }

        private void Update()
        {
            bool isEnabled = Settings.enableSpawnPointReviewer != null && Settings.enableSpawnPointReviewer.Value;

            // 1. Se o usuário desativou a opção no F12, encerra imediatamente
            if (!isEnabled)
            {
                if (IsActive)
                {
                    StopReviewSession();
                }
                return;
            }

            // 2. Se está ativado no F12, mas a sessão ainda não iniciou nesta raid:
            if (!IsActive)
            {
                _pollTimer += Time.deltaTime;
                if (_pollTimer >= 0.8f) // Verifica a cada 800ms
                {
                    _pollTimer = 0f;
                    string map = ResolveCurrentMapName();
                    if (!string.IsNullOrEmpty(map) && !map.Equals("unknown_map", StringComparison.OrdinalIgnoreCase) && !map.ToLower().Contains("hideout"))
                    {
                        var gw = Singleton<GameWorld>.Instance;
                        if (gw != null && gw.MainPlayer != null && !(gw.MainPlayer is HideoutPlayer))
                        {
                            Plugin.LogSource?.LogInfo($"[TRL-DynamicSpawn] Auto-detected active raid on '{map}'. Starting SpawnPointReviewer session...");
                            StartReviewSession(map);
                        }
                    }
                }
                return;
            }

            // 3. Se a sessão está ativa, mas a raid acabou (GameWorld destruído)
            var activeGw = Singleton<GameWorld>.Instance;
            if (activeGw == null)
            {
                StopReviewSession();
                return;
            }

            // 4. Se a sessão está ativa mas nenhum marcador foi gerado (delay das BotZones), tenta re-coletar
            if (_markers.Count == 0 && Time.frameCount % 120 == 0)
            {
                CollectNativeSpawnPoints();
                if (_markers.Count > 0)
                {
                    SaveReviewFile();
                    Plugin.LogSource?.LogInfo($"[TRL-DynamicSpawn] Delayed native spawn collection succeeded: {_markers.Count} points active.");
                }
            }

            var cam = GetActiveCamera();
            if (cam == null) return;

            if (_feedbackTimer > 0f)
            {
                _feedbackTimer -= Time.deltaTime;
            }

            // 1. Raycast para Hover a partir do centro da câmera
            Ray ray = new Ray(cam.transform.position, cam.transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, 150f, ~0, QueryTriggerInteraction.Collide))
            {
                var targetMarker = hit.collider.GetComponent<SpawnMarkerVisual>() ?? hit.collider.GetComponentInParent<SpawnMarkerVisual>();
                _hoveredMarker = targetMarker;
            }
            else
            {
                _hoveredMarker = null;
            }

            // 2. Tecla de Criação de Novo Ponto Custom (Holograma/Ghost)
            if (Settings.reviewerCreateKey != null && Settings.reviewerCreateKey.Value.IsDown())
            {
                CreateGhostSpawnPoint();
            }

            // 3. Tecla de Seleção / Trava de Alvo (KeypadEnter)
            if (Settings.reviewerSelectKey != null && Settings.reviewerSelectKey.Value.IsDown())
            {
                if (_selectedMarker != null)
                {
                    // Se estiver mirando em outro ponto, trava no novo
                    if (_hoveredMarker != null && _hoveredMarker != _selectedMarker)
                    {
                        _selectedMarker = _hoveredMarker;
                    }
                    else
                    {
                        // Destrava
                        _selectedMarker = null;
                    }
                }
                else if (_hoveredMarker != null)
                {
                    _selectedMarker = _hoveredMarker;
                }
            }

            // Controles de modificadores do teclado
            bool isCtrlDown = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
            bool isShiftDown = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

            // 4. Tecla de Reset do Mapa Inteiro (Ctrl + Shift + R) - Operação Global
            bool isResetMapTriggered = (isCtrlDown && isShiftDown && Input.GetKeyDown(KeyCode.R)) ||
                                       (Settings.reviewerResetMapKey != null && Settings.reviewerResetMapKey.Value.IsDown());
            if (isResetMapTriggered)
            {
                ResetAllSpawnPoints();
            }

            // 5. Tecla de Elevação em Lote de Spawns Vanilla (Ctrl + U) - Operação Global
            bool isElevateVanillaTriggered = (isCtrlDown && !isShiftDown && Input.GetKeyDown(KeyCode.U)) ||
                                             (Settings.reviewerElevateVanillaKey != null && Settings.reviewerElevateVanillaKey.Value.IsDown());
            if (isElevateVanillaTriggered)
            {
                ElevateAllVanillaSpawnPoints(0.15f);
            }

            // Atualiza o destaque visual de todos os marcadores
            var activeTarget = _selectedMarker ?? _hoveredMarker;
            for (int i = 0; i < _markers.Count; i++)
            {
                var m = _markers[i];
                if (m == null) continue;
                bool isSel = (m == _selectedMarker);
                bool isHov = (m == _hoveredMarker);
                m.SetHighlight(isHov, isSel);
            }

            if (activeTarget == null) return;

            // 6. Tecla de Cópia de Informações do Ponto (Ctrl + C)
            bool isCopyTriggered = (isCtrlDown && !isShiftDown && Input.GetKeyDown(KeyCode.C)) || 
                                   (Settings.reviewerCopyKey != null && Settings.reviewerCopyKey.Value.IsDown());

            if (isCopyTriggered)
            {
                string textToCopy = $"{activeTarget.PointId} [{activeTarget.ZoneName}]";
                GUIUtility.systemCopyBuffer = textToCopy;
                ShowFeedback($"COPIADO: {textToCopy}", "50fa7b");
                Plugin.LogSource?.LogInfo($"[TRL-DynamicSpawn] Copied spawn info to clipboard: '{textToCopy}'");
            }

            // 7. Tecla de Reset do Ponto Selecionado (Backspace)
            bool isResetPointTriggered = (!isCtrlDown && Input.GetKeyDown(KeyCode.Backspace)) ||
                                         (Settings.reviewerResetPointKey != null && Settings.reviewerResetPointKey.Value.IsDown());
            if (isResetPointTriggered)
            {
                ResetSelectedSpawnPoint();
            }

            // 8. Tecla de Exclusão de Ponto Customizado (Delete / Del)
            bool isDeleteTriggered = Input.GetKeyDown(KeyCode.Delete) ||
                                     (Settings.reviewerDeleteKey != null && Settings.reviewerDeleteKey.Value.IsDown());
            if (isDeleteTriggered)
            {
                DeleteSelectedSpawnPoint();
                return;
            }

            // 9. Tecla de Alternância de Categoria de Spawn (NumPad 7)
            bool isCycleTypeTriggered = Input.GetKeyDown(KeyCode.Keypad7) ||
                                        (Settings.reviewerCycleTypeKey != null && Settings.reviewerCycleTypeKey.Value.IsDown());
            if (isCycleTypeTriggered)
            {
                CycleSelectedSpawnType();
            }

            // 4. Validação Contínua de Requisitos (NavMesh, Solo e Proximidade de BotZone)
            bool isTargetSniper = (activeTarget.Source != null && activeTarget.Source.IndexOf("SNIPER", StringComparison.OrdinalIgnoreCase) >= 0) ||
                                  (activeTarget.PointId != null && activeTarget.PointId.IndexOf("SNIPER", StringComparison.OrdinalIgnoreCase) >= 0);
            bool criteriaMet = EvaluatePointCriteria(activeTarget.CurrentPosition, out string closestZone, out float distToZone, out string failReason, isSniper: isTargetSniper);
            if (activeTarget.IsGhost)
            {
                activeTarget.SetGhostState(true, criteriaMet, failReason);
            }

            // 5. Movimentação Fina Orientada à Visão da Câmera (NumPad 8/2, 4/6, 9/3, Snap ao Solo)
            float step = (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) ? 0.5f : 0.1f;
            Vector3 pos = activeTarget.CurrentPosition;
            bool moved = false;

            // Vetores de orientação horizontal alinhados com a visão da câmera ativa
            Vector3 camFwd = cam.transform.forward;
            camFwd.y = 0f;
            if (camFwd.sqrMagnitude > 0.001f) camFwd.Normalize();
            else camFwd = Vector3.forward;

            Vector3 camRight = cam.transform.right;
            camRight.y = 0f;
            if (camRight.sqrMagnitude > 0.001f) camRight.Normalize();
            else camRight = Vector3.right;

            // Frente / Afastar na visão (NumPad 8)
            if (Input.GetKeyDown(KeyCode.Keypad8))
            {
                pos += camFwd * step;
                moved = true;
            }
            // Trás / Puxar para perto na visão (NumPad 2)
            if (Input.GetKeyDown(KeyCode.Keypad2))
            {
                pos -= camFwd * step;
                moved = true;
            }

            // Esquerda na visão (NumPad 4)
            if (Input.GetKeyDown(KeyCode.Keypad4))
            {
                pos -= camRight * step;
                moved = true;
            }
            // Direita na visão (NumPad 6)
            if (Input.GetKeyDown(KeyCode.Keypad6))
            {
                pos += camRight * step;
                moved = true;
            }

            // Subir cota Y Vertical (NumPad 9)
            if (Input.GetKeyDown(KeyCode.Keypad9))
            {
                pos.y += step;
                moved = true;
            }
            // Descer cota Y Vertical (NumPad 3)
            if (Input.GetKeyDown(KeyCode.Keypad3))
            {
                pos.y -= step;
                moved = true;
            }

            // Snap no Solo (Tecla End)
            if (Settings.reviewerSnapKey != null && Settings.reviewerSnapKey.Value.IsDown())
            {
                int groundMask = LayerMaskClass.HighPolyWithTerrainMask | LayerMaskClass.PlayerStaticCollisionsMask;
                Vector3 snapRayOrigin = pos + Vector3.up * 1.5f;
                if (Physics.Raycast(snapRayOrigin, Vector3.down, out RaycastHit snapHit, 20f, groundMask))
                {
                    pos.y = snapHit.point.y;
                    moved = true;
                }
            }

            if (moved)
            {
                activeTarget.SetPosition(pos);
                if (activeTarget.IsGhost)
                {
                    string newZone = ResolveClosestBotZoneName(pos, isSniper: isTargetSniper, out float _);
                    activeTarget.SetZoneName(newZone);
                }
                else
                {
                    SaveReviewFile();
                }
            }

            // 6. Confirmação / Aprovação / Reprovação / Reset de Status
            bool isApproveTriggered = Input.GetKeyDown(KeyCode.Keypad1) || Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.KeypadPlus) || (Settings.reviewerApproveKey != null && Settings.reviewerApproveKey.Value.IsDown());
            bool isRejectTriggered = Input.GetKeyDown(KeyCode.Keypad0) || Input.GetKeyDown(KeyCode.Alpha0) || Input.GetKeyDown(KeyCode.KeypadMinus) || (Settings.reviewerRejectKey != null && Settings.reviewerRejectKey.Value.IsDown());
            bool isResetStatusTriggered = Input.GetKeyDown(KeyCode.KeypadPeriod) || Input.GetKeyDown(KeyCode.Comma) || (Settings.reviewerResetStatusKey != null && Settings.reviewerResetStatusKey.Value.IsDown());

            if (isApproveTriggered)
            {
                if (activeTarget.IsGhost)
                {
                    if (!criteriaMet)
                    {
                        Plugin.LogSource?.LogWarning($"[TRL-DynamicSpawn] Cannot confirm ghost point '{activeTarget.PointId}': {failReason}");
                        ShowFeedback($"Inválido: {failReason}", "ff5555");
                    }
                    else
                    {
                        string finalZone = ResolveClosestBotZoneName(activeTarget.CurrentPosition, isSniper: isTargetSniper, out float _);
                        activeTarget.SetZoneName(finalZone);
                        activeTarget.ConfirmGhost();
                        SaveReviewFile();
                        ShowFeedback($"Ponto Custom '{activeTarget.PointId}' ATIVADO na zona '{finalZone}'!", "50fa7b");
                        Plugin.LogSource?.LogInfo($"[TRL-DynamicSpawn] Custom point '{activeTarget.PointId}' confirmed and active in zone '{finalZone}'!");
                    }
                }
                else
                {
                    activeTarget.SetStatus(SpawnReviewStatus.Approved);
                    SaveReviewFile();
                    ShowFeedback($"Ponto '{activeTarget.PointId}' APROVADO.", "50fa7b");
                }
            }
            else if (isRejectTriggered)
            {
                if (activeTarget.IsGhost)
                {
                    // Descarta o ghost marker
                    _markers.Remove(activeTarget);
                    _markersById.Remove(activeTarget.PointId);
                    if (_selectedMarker == activeTarget) _selectedMarker = null;
                    Destroy(activeTarget.gameObject);
                    ShowFeedback("Ponto fantasma descartado.", "ffb86c");
                    Plugin.LogSource?.LogInfo($"[TRL-DynamicSpawn] Cancelled and removed ghost point '{activeTarget.PointId}'.");
                }
                else
                {
                    activeTarget.SetStatus(SpawnReviewStatus.Rejected);
                    SaveReviewFile();
                    ShowFeedback($"Ponto '{activeTarget.PointId}' REPROVADO.", "ff5555");
                }
            }
            else if (isResetStatusTriggered)
            {
                if (!activeTarget.IsGhost)
                {
                    activeTarget.SetStatus(SpawnReviewStatus.Pending);
                    SaveReviewFile();
                    ShowFeedback($"Status do ponto '{activeTarget.PointId}' redefinido para PENDENTE.", "ffff55");
                    Plugin.LogSource?.LogInfo($"[TRL-DynamicSpawn] Spawn point '{activeTarget.PointId}' status reset to Pending.");
                }
            }
        }

        private void InitGUIStyles()
        {
            if (_panelStyle != null) return;

            var bgTex = new Texture2D(1, 1);
            bgTex.SetPixel(0, 0, new Color(0.08f, 0.09f, 0.11f, 0.92f));
            bgTex.Apply();

            _panelStyle = new GUIStyle(GUI.skin.box)
            {
                normal = { background = bgTex },
                padding = new RectOffset(14, 14, 10, 10)
            };

            _titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.95f, 0.8f, 0.2f) }
            };

            _headerStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };

            _infoStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                normal = { textColor = new Color(0.85f, 0.88f, 0.9f) }
            };

            _highlightStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.cyan }
            };

            _shortcutStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 10,
                normal = { textColor = new Color(0.7f, 0.75f, 0.8f) }
            };

            _richInfoStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                richText = true,
                normal = { textColor = Color.white }
            };
        }

        private static GUIStyle _richInfoStyle;

        private void OnGUI()
        {
            if (!IsActive || Settings.enableSpawnPointReviewer == null || !Settings.enableSpawnPointReviewer.Value) return;

            InitGUIStyles();

            float panelWidth = 470f;
            float panelHeight = 425f;
            float margin = 20f;
            Rect panelRect = new Rect(Screen.width - panelWidth - margin, margin, panelWidth, panelHeight);

            GUI.Box(panelRect, GUIContent.none, _panelStyle);

            GUILayout.BeginArea(new Rect(panelRect.x + 12, panelRect.y + 8, panelRect.width - 24, panelRect.height - 16));

            // Título
            GUILayout.Label("TRL SPAWN POINT REVIEWER [DEBUG]", _titleStyle);

            int total = _markers.Count;
            int approved = _markers.Count(m => m != null && m.Status == SpawnReviewStatus.Approved);
            int rejected = _markers.Count(m => m != null && m.Status == SpawnReviewStatus.Rejected);
            int pending = _markers.Count(m => m != null && m.Status == SpawnReviewStatus.Pending);
            int players = _markers.Count(m => m != null && !string.IsNullOrEmpty(m.Source) && m.Source.IndexOf("PLAYER", StringComparison.OrdinalIgnoreCase) >= 0);

            GUILayout.Label($"Mapa: {CurrentMapName} | Total: {total} (Players: {players})", _headerStyle);
            GUILayout.Label($"Aprovados: {approved} (Verde) | Reprovados: {rejected} (Preto) | Pendentes: {pending}", _infoStyle);
            GUILayout.Label("Cores: Nativo (Vermelho) | MOAR (Magenta) | Player (Azul Escuro)", _infoStyle);

            GUILayout.Space(4);

            var activeTarget = _selectedMarker ?? _hoveredMarker;
            if (activeTarget != null)
            {
                string lockState = activeTarget == _selectedMarker ? "[TRAVADO]" : "[HOVER]";
                string ghostState = activeTarget.IsGhost ? " <color=#ffff55>[HOLOGRAMA]</color>" : "";
                string movedState = activeTarget.WasMoved ? " *MOVIDO*" : "";
                GUILayout.Label($"Alvo: {activeTarget.PointId} {lockState}{ghostState}{movedState}", _richInfoStyle);
                GUILayout.Label($"Origem: <color=#8be9fd>{activeTarget.Source}</color> | Zona: {activeTarget.ZoneName} | Status: {activeTarget.Status}", _richInfoStyle);

                var cam = GetActiveCamera();
                float dist = cam != null ? Vector3.Distance(cam.transform.position, activeTarget.CurrentPosition) : 0f;
                GUILayout.Label($"Distância Câmera: {dist:F1} m", _infoStyle);

                var orig = activeTarget.OriginalPosition;
                var cur = activeTarget.CurrentPosition;
                GUILayout.Label($"Atual: ({cur.x:F2}, {cur.y:F2}, {cur.z:F2})", _infoStyle);

                // Checklist de Validação em Tempo Real
                bool criteriaOk = EvaluatePointCriteria(activeTarget.CurrentPosition, out string cZone, out float zDist, out string failRsn);
                bool hasNav = UnityEngine.AI.NavMesh.SamplePosition(activeTarget.CurrentPosition, out _, 1.2f, UnityEngine.AI.NavMesh.AllAreas);
                string navText = hasNav ? "<color=#50fa7b>[✓] NavMesh</color>" : "<color=#ff5555>[✗] NavMesh</color>";
                string zoneText = zDist <= 85f ? $"<color=#50fa7b>[✓] Zona: {cZone} ({zDist:F0}m)</color>" : $"<color=#ff5555>[✗] Zona Longe ({zDist:F0}m)</color>";
                string groundText = string.IsNullOrEmpty(failRsn) || !failRsn.Contains("piso") ? "<color=#50fa7b>[✓] Solo</color>" : "<color=#ffb86c>[!] Solo [End]</color>";

                GUILayout.Label($"Requisitos: {navText} | {groundText} | {zoneText}", _richInfoStyle);

                if (activeTarget.IsGhost)
                {
                    if (criteriaOk)
                        GUILayout.Label("<color=#50fa7b>★ VÁLIDO: Pressione [NumPad 1] para ativar no JSON!</color>", _richInfoStyle);
                    else
                        GUILayout.Label($"<color=#ff5555>⚠ INVÁLIDO: {failRsn}</color>", _richInfoStyle);
                }
            }
            else
            {
                GUILayout.Label("Nenhum ponto em foco (mire em um poste ou aperte [Insert] para criar).", _shortcutStyle);
                GUILayout.Space(24);
            }

            if (_feedbackTimer > 0f && !string.IsNullOrEmpty(_feedbackMessage))
            {
                GUILayout.Space(2);
                GUILayout.Label($"<color=#{_feedbackColorHex}>★ {_feedbackMessage}</color>", _richInfoStyle);
            }

            GUILayout.Space(6);
            GUILayout.Label("--- ATALHOS ---", _shortcutStyle);
            GUILayout.Label("[Insert]: Ponto Custom | [Del]: Deletar Ponto | [NumPad Enter]: Travar | [Ctrl+C]: Copiar", _shortcutStyle);
            GUILayout.Label("[NumPad 8/2]: Frente/Trás | [NumPad 4/6]: Esq/Dir (Visão) | [End]: Snap Solo", _shortcutStyle);
            GUILayout.Label("[NumPad 9/3]: Subir/Descer Y | [NumPad 1]: Aprovar | [NumPad 0]: Reprovar | [Shift]: 0.5m", _shortcutStyle);
            GUILayout.Label("[NumPad ,]: Resetar Status | [Backspace]: Reset Ponto | [Ctrl+Shift+R]: Reset Mapa | [Ctrl+U]: Elevar Vanilla", _shortcutStyle);
            GUILayout.Label("[NumPad 7]: Mudar Tipo (PMC / Scav / Sniper / Boss / Rogue / Raider / Player)", _shortcutStyle);

            GUILayout.EndArea();
        }
    }
}
