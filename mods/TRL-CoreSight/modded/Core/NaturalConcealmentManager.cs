using System;
using System.Collections.Generic;
using Comfort.Common;
using EFT;
using GPUInstancer;
using TRLCoreSight.Configuration;
using UnityEngine;

namespace TRLCoreSight.Core
{
    /// <summary>
    /// Gerenciador de Camuflagem Zonal Anatômica e Oclusão de Visão de IA.
    /// Divide o corpo do soldado em 5 zonas independentes (Cabeça, Peito/Mochila, Pelve, Perna Esq, Perna Dir),
    /// projetando envelopes de oclusão com contorno de segurança de +30cm no layer 'Foliage'.
    /// Zonas no capim alto ou moitas ativam o escudo; membros expostos permanecem visíveis para os bots.
    /// </summary>
    public class NaturalConcealmentManager : MonoBehaviour
    {
        public static NaturalConcealmentManager Instance { get; private set; }

        public class BodyConcealmentZone
        {
            public string Name;
            public Transform BoneTransform;
            public GameObject ZoneObject;
            public CapsuleCollider ZoneCollider;
            public Vector3 LocalOffset;
            public float BaseRadius;
            public float BaseHeight;
            public int Direction; // 0=X, 1=Y, 2=Z
            public bool IsCovered;
        }

        private struct TallGrassLayerInfo
        {
            public Terrain OwnerTerrain;
            public int LayerIndex;
            public float MaxHeight;
            public string Name;
        }

        private class GPUGrassLayer
        {
            public string UniqueKey;
            public Terrain OwnerTerrain;
            public string Name;
            public float MinHeight;
            public float MaxHeight;
            public bool IsTallGrass;
            public bool IsMediumGrass;
            public int[,] DensityMap;
            public int Resolution;
        }

        private Player _mainPlayer;
        private GameObject _shieldRootObject;
        private ConcealmentVisualizer _visualizer;
        private readonly List<BodyConcealmentZone> _zones = new List<BodyConcealmentZone>();
        private readonly List<TallGrassLayerInfo> _tallGrassLayers = new List<TallGrassLayerInfo>();
        private readonly List<GPUGrassLayer> _gpuGrassLayers = new List<GPUGrassLayer>();
        private float _lastGpuGrassScanTime = 0f;
        private Terrain[] _cachedTerrains;
        private float _lastTerrainScanTime = 0f;

        private int _foliageLayer;
        private int _foliageMask;
        private float _lastSensorCheckTime = 0f;
        private readonly Collider[] _overlapResults = new Collider[16];

        public bool IsConcealed { get; private set; }
        public bool IsUnderCanopy { get; private set; }
        public float CoverageRatio { get; private set; } // 0.0 (totalmente exposto) a 1.0 (100% camuflado)
        public int CoveredZonesCount { get; private set; }

        private void Awake()
        {
            Instance = this;
            _foliageLayer = LayerMask.NameToLayer("Foliage");
            if (_foliageLayer < 0)
            {
                _foliageLayer = 15;
            }
            _foliageMask = 1 << _foliageLayer;

            ModConfig.OnConcealmentSettingsChanged += OnSettingsChanged;
            ModConfig.OnVisualizerToggled += OnVisualizerToggled;
        }

        public void Initialize(Player mainPlayer)
        {
            _mainPlayer = mainPlayer;
            ScanMapTerrains();
            VegetationSpatialIndex.IndexSceneVegetation();
            CreateZonalShields();

            // Anexa o componente de depuração visual
            if (_shieldRootObject != null)
            {
                _visualizer = _shieldRootObject.AddComponent<ConcealmentVisualizer>();
                _visualizer.Initialize(this, _mainPlayer);
            }

            Plugin.LogSource?.LogInfo("[TRL-CoreSight] NaturalConcealmentManager: Sistema Zonal Anatômico Fino (5 zonas, +10cm) inicializado.");
        }

        private void ScanMapTerrains()
        {
            _tallGrassLayers.Clear();
            _lastTerrainScanTime = Time.time;

            _cachedTerrains = Terrain.activeTerrains;
            if ((_cachedTerrains == null || _cachedTerrains.Length == 0) && Terrain.activeTerrain != null)
            {
                _cachedTerrains = new Terrain[] { Terrain.activeTerrain };
            }

            if (_cachedTerrains == null || _cachedTerrains.Length == 0)
            {
                _cachedTerrains = FindObjectsOfType<Terrain>();
            }

            if (_cachedTerrains == null) return;

            for (int t = 0; t < _cachedTerrains.Length; t++)
            {
                Terrain terrain = _cachedTerrains[t];
                if (terrain == null || terrain.terrainData == null) continue;

                DetailPrototype[] protos = terrain.terrainData.detailPrototypes;
                if (protos == null) continue;

                for (int i = 0; i < protos.Length; i++)
                {
                    DetailPrototype proto = protos[i];
                    string protoName = "";
                    if (proto.prototype != null)
                    {
                        protoName = proto.prototype.name.ToLower();
                    }
                    else if (proto.prototypeTexture != null)
                    {
                        protoName = proto.prototypeTexture.name.ToLower();
                    }

                    // Identifica se é capim alto, matagal, erva ou folhagem de solo (maxHeight >= 0.35m ou palavras-chave)
                    bool isTallGrass = proto.maxHeight >= 0.35f ||
                                       protoName.Contains("grass") ||
                                       protoName.Contains("reed") ||
                                       protoName.Contains("weed") ||
                                       protoName.Contains("plant") ||
                                       protoName.Contains("fern") ||
                                       protoName.Contains("flower") ||
                                       protoName.Contains("bush") ||
                                       protoName.Contains("tall");

                    if (isTallGrass)
                    {
                        _tallGrassLayers.Add(new TallGrassLayerInfo
                        {
                            OwnerTerrain = terrain,
                            LayerIndex = i,
                            MaxHeight = Mathf.Max(proto.maxHeight, 0.55f),
                            Name = protoName
                        });
                    }
                }
            }

            Plugin.LogSource?.LogInfo($"[TRL-CoreSight] NaturalConcealmentManager: {_tallGrassLayers.Count} camadas de capim alto indexadas em {_cachedTerrains.Length} terrenos.");
            ScanGPUInstancerGrass();
        }

        private void CreateZonalShields()
        {
            if (_shieldRootObject != null)
            {
                Destroy(_shieldRootObject);
            }
            _zones.Clear();

            _shieldRootObject = new GameObject("TRL_ZonalConcealmentRoot");

            if (_mainPlayer == null || _mainPlayer.PlayerBones == null)
            {
                return;
            }

            float offset = ModConfig.ConcealmentOffsetMeters.Value;

            // Proporções anatômicas reais do soldado (cabeça, tórax, cintura e pernas finas e separadas)
            // 1. Zona Cabeça / Olhos (Head) - Esférica justa ao capacete
            AddZone("Zone_Head", _mainPlayer.PlayerBones.Head?.Original, Vector3.zero,
                0.10f, 0.16f, 1, offset);

            // 2. Zona Peito / Tórax / Mochila (Ribcage) - Contorno fino do tronco e mochila
            AddZone("Zone_Chest", _mainPlayer.PlayerBones.Ribcage?.Original, new Vector3(0f, 0f, -0.04f),
                0.15f, 0.38f, 1, offset);

            // 3. Zona Pelve / Quadril (Pelvis)
            AddZone("Zone_Pelvis", _mainPlayer.PlayerBones.Pelvis?.Original, Vector3.zero,
                0.13f, 0.24f, 1, offset);

            // 4. Zona Perna / Pé Esquerdo (LeftThigh1) - Cilindro fino e esguio
            AddZone("Zone_LeftLeg", _mainPlayer.PlayerBones.LeftThigh1?.Original, new Vector3(0f, -0.22f, 0f),
                0.08f, 0.48f, 1, offset);

            // 5. Zona Perna / Pé Direito (RightThigh1) - Cilindro fino e esguio
            AddZone("Zone_RightLeg", _mainPlayer.PlayerBones.RightThigh1?.Original, new Vector3(0f, -0.22f, 0f),
                0.08f, 0.48f, 1, offset);
        }

        private void AddZone(string name, Transform bone, Vector3 localOffset, float baseRadius, float baseHeight, int direction, float offsetMeters)
        {
            if (bone == null) return;

            GameObject zoneGo = new GameObject(name);
            zoneGo.layer = _foliageLayer;
            zoneGo.transform.SetParent(_shieldRootObject.transform, false);

            CapsuleCollider col = zoneGo.AddComponent<CapsuleCollider>();
            col.isTrigger = false;
            col.radius = baseRadius + offsetMeters;
            col.height = baseHeight + (offsetMeters * 1.5f);
            col.direction = direction;
            col.center = Vector3.zero;
            col.enabled = false;

            _zones.Add(new BodyConcealmentZone
            {
                Name = name,
                BoneTransform = bone,
                ZoneObject = zoneGo,
                ZoneCollider = col,
                LocalOffset = localOffset,
                BaseRadius = baseRadius,
                BaseHeight = baseHeight,
                Direction = direction,
                IsCovered = false
            });
        }

        public void OnUpdate()
        {
            if (!ModConfig.ModEnabled.Value || !ModConfig.EnableNaturalConcealment.Value || _mainPlayer == null || _shieldRootObject == null)
            {
                DisableAllZones();
                return;
            }

            // Salvaguarda: se o soldado estiver morto, desativa imediatamente
            if (_mainPlayer.HealthController != null && !_mainPlayer.HealthController.IsAlive)
            {
                DisableAllZones();
                return;
            }

            Vector3 playerPos = _mainPlayer.Position;

            // 1. Proximidade Imediata / Tropeço (< BreakProximityDistance)
            if (IsAnyBotTooClose(playerPos, ModConfig.BreakProximityDistance.Value))
            {
                DisableAllZones();
                return;
            }

            // 2. Atualização física de posição dos colidentes das zonas
            for (int i = 0; i < _zones.Count; i++)
            {
                BodyConcealmentZone zone = _zones[i];
                if (zone.ZoneObject != null && zone.BoneTransform != null)
                {
                    zone.ZoneObject.transform.position = zone.BoneTransform.position + zone.BoneTransform.TransformDirection(zone.LocalOffset);
                    zone.ZoneObject.transform.rotation = zone.BoneTransform.rotation;
                }
            }

            // Rescan periódico de terrenos e GPUInstancer grass
            if (Time.time - _lastTerrainScanTime > 30f)
            {
                ScanMapTerrains();
            }

            if (Time.time - _lastGpuGrassScanTime > 2.0f && (_gpuGrassLayers.Count < 5 || Time.time < 30f))
            {
                ScanGPUInstancerGrass();
            }

            // 3. Ciclo de amostragem volumétrica dos sensores (a cada 0.2s - 5x/segundo)
            if (Time.time - _lastSensorCheckTime > 0.2f)
            {
                _lastSensorCheckTime = Time.time;
                EvaluateZonalConcealment(playerPos);
            }
        }

        private void EvaluateZonalConcealment(Vector3 playerBasePos)
        {
            bool isInTreeNative = _mainPlayer.AIData != null && _mainPlayer.AIData.IsInTree;
            bool isProne = _mainPlayer.MovementContext != null && _mainPlayer.MovementContext.IsInPronePose;
            float poseLevel = _mainPlayer.MovementContext != null ? _mainPlayer.MovementContext.PoseLevel : 1.0f;

            int coveredCount = 0;

            for (int i = 0; i < _zones.Count; i++)
            {
                BodyConcealmentZone zone = _zones[i];
                if (zone.BoneTransform == null || zone.ZoneCollider == null) continue;

                Vector3 worldPos = zone.BoneTransform.position + zone.BoneTransform.TransformDirection(zone.LocalOffset);

                // Sensor 1: Intersecção direta com colisores de folhagem/árvores/arbustos
                bool coveredByFoliage = CheckFoliageOverlap(worldPos, zone.ZoneCollider.radius);

                // Sensor 2: Arbusto nativo EFT (TreeInteractive)
                bool coveredByTreeTrigger = isInTreeNative && CheckProximityToBush(worldPos);

                // Sensor 3: Capim volumétrico calibrado (Grass_new_1_D e Grass6_D por postura e densidade)
                bool coveredByTallGrass = CheckCalibratedGrassConcealment(worldPos, isProne, poseLevel, zone.Name, out _);
                if (!coveredByTallGrass && (zone.Name != "Zone_Head" || isProne))
                {
                    coveredByTallGrass = CheckCalibratedGrassConcealment(playerBasePos, isProne, poseLevel, zone.Name, out _);
                }

                // Sensor 4: Catálogo Dinâmico de Malhas 3D de Vegetação sem Colisor (Spatial Hash Grid O(1))
                bool coveredByMeshVegetation = VegetationSpatialIndex.IsPointCoveredByVegetation(worldPos, isProne, poseLevel, out _);

                zone.IsCovered = coveredByFoliage || coveredByTreeTrigger || coveredByTallGrass || coveredByMeshVegetation;
                zone.ZoneCollider.enabled = zone.IsCovered;

                if (zone.IsCovered)
                {
                    coveredCount++;
                }
            }

            CoveredZonesCount = coveredCount;
            CoverageRatio = (float)coveredCount / _zones.Count;
            IsConcealed = coveredCount >= 3;
        }

        private bool CheckCalibratedGrassConcealment(Vector3 worldPos, bool isProne, float poseLevel, string zoneName, out float grassHeight)
        {
            grassHeight = 0f;
            if (_gpuGrassLayers.Count == 0) return false;

            bool isHead = zoneName == "Zone_Head";
            bool covered = false;

            for (int i = 0; i < _gpuGrassLayers.Count; i++)
            {
                GPUGrassLayer layer = _gpuGrassLayers[i];
                Terrain terrain = layer.OwnerTerrain;
                if (terrain == null || terrain.terrainData == null || layer.DensityMap == null) continue;

                string nameLower = layer.Name.ToLowerInvariant();

                // 1. Folhagens 100% ignoradas pela camuflagem (mesmo em alta densidade)
                if (nameLower.Contains("field_grass") ||
                    nameLower.Contains("grass_new_1") ||
                    nameLower.Contains("grass_new_2") ||
                    nameLower.Contains("grass_new_3"))
                {
                    continue;
                }

                // 2. Folhagens que ativam camuflagem: Grass2_D, Grass5_512_D, Grass6_D
                bool isGrass2 = nameLower.Contains("grass2");
                bool isGrass5 = nameLower.Contains("grass5_512");
                bool isGrass6 = nameLower.Contains("grass6");
                if (!isGrass2 && !isGrass5 && !isGrass6) continue;

                Vector3 terrainPos = terrain.transform.position;
                Vector3 terrainSize = terrain.terrainData.size;

                float localX = worldPos.x - terrainPos.x;
                float localZ = worldPos.z - terrainPos.z;

                if (localX >= 0f && localX <= terrainSize.x && localZ >= 0f && localZ <= terrainSize.z)
                {
                    int x = Mathf.Clamp(Mathf.FloorToInt((localX / terrainSize.x) * layer.Resolution), 0, layer.Resolution - 1);
                    int z = Mathf.Clamp(Mathf.FloorToInt((localZ / terrainSize.z) * layer.Resolution), 0, layer.Resolution - 1);

                    // Amostragem no kernel 3x3 para obter a densidade máxima no ponto
                    int pointDensity = 0;
                    for (int ox = -1; ox <= 1; ox++)
                    {
                        int sx = Mathf.Clamp(x + ox, 0, layer.Resolution - 1);
                        for (int oz = -1; oz <= 1; oz++)
                        {
                            int sz = Mathf.Clamp(z + oz, 0, layer.Resolution - 1);
                            int d1 = layer.DensityMap[sz, sx];
                            int d2 = layer.DensityMap[sx, sz];
                            if (d1 > pointDensity) pointDensity = d1;
                            if (d2 > pointDensity) pointDensity = d2;
                        }
                    }

                    // Regra: Camuflagem ativa acima de 8 de densidade (>= 8)
                    if (pointDensity >= 8)
                    {
                        if (isProne)
                        {
                            // Deitado cobre o corpo inteiro (todas as zonas corporais)
                            covered = true;
                            grassHeight = Mathf.Max(grassHeight, layer.MaxHeight);
                        }
                        else if (poseLevel <= 0.25f)
                        {
                            // Em agachamento baixo (poseLevel 0f a 0.25f): cobre pernas, pelve e peito
                            if (!isHead)
                            {
                                covered = true;
                                grassHeight = Mathf.Max(grassHeight, layer.MaxHeight);
                            }
                        }
                    }
                }
            }

            return covered;
        }

        private bool CheckTallGrassAtPosition(Vector3 worldPos, out float grassMaxHeight)
        {
            grassMaxHeight = 0f;

            // 1. Tenta leitura detalhada se houver camadas indexadas no mapa
            if (_tallGrassLayers.Count > 0)
            {
                for (int i = 0; i < _tallGrassLayers.Count; i++)
                {
                    TallGrassLayerInfo info = _tallGrassLayers[i];
                    Terrain terrain = info.OwnerTerrain;
                    if (terrain == null || terrain.terrainData == null) continue;

                    Vector3 terrainPos = terrain.transform.position;
                    Vector3 localPos = worldPos - terrainPos;
                    TerrainData data = terrain.terrainData;
                    Vector3 size = data.size;

                    if (localPos.x >= 0f && localPos.x <= size.x && localPos.z >= 0f && localPos.z <= size.z)
                    {
                        float normX = localPos.x / size.x;
                        float normZ = localPos.z / size.z;
                        int detailX = Mathf.FloorToInt(normX * data.detailWidth);
                        int detailZ = Mathf.FloorToInt(normZ * data.detailHeight);

                        if (detailX >= 0 && detailX < data.detailWidth && detailZ >= 0 && detailZ < data.detailHeight)
                        {
                            int[,] density = data.GetDetailLayer(detailX, detailZ, 1, 1, info.LayerIndex);
                            if (density != null && density.Length > 0 && density[0, 0] > 0)
                            {
                                grassMaxHeight = Mathf.Max(grassMaxHeight, info.MaxHeight);
                            }
                        }
                    }
                }

                if (grassMaxHeight > 0f)
                {
                    return true;
                }
            }

            return false;
        }

        private void ScanGPUInstancerGrass()
        {
            _lastGpuGrassScanTime = Time.time;

            GPUInstancerDetailManager[] managers = UnityEngine.Object.FindObjectsOfType<GPUInstancerDetailManager>();
            if (managers == null || managers.Length == 0)
            {
                return;
            }

            for (int m = 0; m < managers.Length; m++)
            {
                var mgr = managers[m];
                if (mgr == null || mgr.prototypeList == null || mgr.prototypeList.Count == 0) continue;

                // Em EFT, o terrain do GPUInstancer pode ser nulo, recorrendo a Terrain.activeTerrain
                Terrain terrain = mgr.terrain != null ? mgr.terrain : Terrain.activeTerrain;
                if (terrain == null || terrain.terrainData == null) continue;

                for (int p = 0; p < mgr.prototypeList.Count; p++)
                {
                    var proto = mgr.prototypeList[p] as GPUInstancerDetailPrototype;
                    if (proto == null) continue;

                    string protoName = proto.name != null ? proto.name.ToLower() : "";
                    string texName = proto.prototypeTexture != null ? proto.prototypeTexture.name.ToLower() : "";
                    string prefabName = proto.prefabObject != null ? proto.prefabObject.name.ToLower() : "";

                    // Filtro estrito: ignora apenas rochas, pedras, entulho, lixo e cascalho
                    if (prefabName.Contains("rock") || prefabName.Contains("stone") || prefabName.Contains("pebble") ||
                        prefabName.Contains("debris") || prefabName.Contains("trash") || prefabName.Contains("gravel") ||
                        texName.Contains("rock") || texName.Contains("stone") || texName.Contains("pebble") ||
                        protoName.Contains("rock") || protoName.Contains("stone"))
                    {
                        continue;
                    }

                    // Altura real extraída das dimensões reais do protótipo
                    float minH = proto.detailScale.z;
                    float maxH = proto.detailScale.w > 0 ? proto.detailScale.w : 
                                 (proto.detailScale.z > 0 ? proto.detailScale.z : 
                                 (proto.detailScale.y > 0 ? proto.detailScale.y : 0.85f));

                    bool isExplicitAllowed = VegetationClassifier.IsExplicitlyAllowed(texName) || VegetationClassifier.IsExplicitlyAllowed(protoName);
                    bool isTall = isExplicitAllowed || maxH >= 0.65f;
                    bool isMedium = !isTall && (maxH >= 0.40f);

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

                    // Fallback imediato: se o manager ainda não inicializou o spData, extrai do mapa serializado do protótipo
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

                    if (densityMap == null) continue;

                    // Chave única para indexar todos os setores sem conflito
                    string uniqueKey = $"{mgr.GetInstanceID()}_{p}_{proto.name}";

                    bool alreadyExists = false;
                    for (int el = 0; el < _gpuGrassLayers.Count; el++)
                    {
                        if (_gpuGrassLayers[el].UniqueKey == uniqueKey)
                        {
                            alreadyExists = true;
                            // Se a camada já existia com resolução temporária, atualiza a matriz
                            if (mgr.isInitialized && _gpuGrassLayers[el].DensityMap != densityMap)
                            {
                                _gpuGrassLayers[el].DensityMap = densityMap;
                                _gpuGrassLayers[el].Resolution = densityMap.GetLength(0);
                            }
                            break;
                        }
                    }
                    if (alreadyExists) continue;

                    string displayTex = proto.prototypeTexture != null ? proto.prototypeTexture.name : proto.name;

                    _gpuGrassLayers.Add(new GPUGrassLayer
                    {
                        UniqueKey = uniqueKey,
                        OwnerTerrain = terrain,
                        Name = displayTex,
                        MinHeight = minH,
                        MaxHeight = maxH,
                        IsTallGrass = isTall,
                        IsMediumGrass = isMedium,
                        DensityMap = densityMap,
                        Resolution = densityMap.GetLength(0)
                    });

                    Plugin.LogSource?.LogInfo($"[TRL-CoreSight] GPU Grass indexado: '{displayTex}' ({proto.name}), Res: {densityMap.GetLength(0)}x{densityMap.GetLength(1)}, Altura: {maxH:F2}m, CapimAlto: {isTall}.");
                }
            }

            if (_gpuGrassLayers.Count > 0)
            {
                Plugin.LogSource?.LogInfo($"[TRL-CoreSight] Total de camadas GPUInstancer Grass indexadas: {_gpuGrassLayers.Count}.");
            }
        }

        private bool CheckGPUGrassAtPosition(Vector3 worldPos, out float grassMaxHeight, out bool isTallGrass, out bool isMediumGrass)
        {
            grassMaxHeight = 0f;
            isTallGrass = false;
            isMediumGrass = false;
            if (_gpuGrassLayers.Count == 0) return false;

            for (int i = 0; i < _gpuGrassLayers.Count; i++)
            {
                GPUGrassLayer layer = _gpuGrassLayers[i];
                Terrain terrain = layer.OwnerTerrain;
                if (terrain == null || terrain.terrainData == null || layer.DensityMap == null) continue;

                // Consulta dinâmica em tempo real ao classificador de regras do usuário (teclas '[' e ']')
                if (VegetationClassifier.IsBlocked(layer.Name)) continue;

                bool explicitlyAllowed = VegetationClassifier.IsExplicitlyAllowed(layer.Name);

                // Descarta grama rasteira (< 0.40m) se não foi explicitamente permitida pelo usuário
                if (!explicitlyAllowed && layer.MaxHeight < 0.40f && !layer.IsTallGrass && !layer.IsMediumGrass) continue;

                Vector3 terrainPos = terrain.transform.position;
                Vector3 terrainSize = terrain.terrainData.size;

                float localX = worldPos.x - terrainPos.x;
                float localZ = worldPos.z - terrainPos.z;

                if (localX >= 0f && localX <= terrainSize.x && localZ >= 0f && localZ <= terrainSize.z)
                {
                    int x = Mathf.Clamp(Mathf.FloorToInt((localX / terrainSize.x) * layer.Resolution), 0, layer.Resolution - 1);
                    int z = Mathf.Clamp(Mathf.FloorToInt((localZ / terrainSize.z) * layer.Resolution), 0, layer.Resolution - 1);

                    // Amostragem de kernel 3x3 (vizinhança de ~1m para cobrir tufos de capim ao redor do osso/jogador)
                    bool hasDensity = false;
                    for (int ox = -1; ox <= 1 && !hasDensity; ox++)
                    {
                        int sx = Mathf.Clamp(x + ox, 0, layer.Resolution - 1);
                        for (int oz = -1; oz <= 1; oz++)
                        {
                            int sz = Mathf.Clamp(z + oz, 0, layer.Resolution - 1);
                            if (layer.DensityMap[sz, sx] > 0 || layer.DensityMap[sx, sz] > 0)
                            {
                                hasDensity = true;
                                break;
                            }
                        }
                    }

                    if (hasDensity)
                    {
                        grassMaxHeight = Mathf.Max(grassMaxHeight, layer.MaxHeight);
                        if (layer.IsTallGrass || explicitlyAllowed)
                        {
                            isTallGrass = true;
                        }
                        else if (layer.IsMediumGrass)
                        {
                            isMediumGrass = true;
                        }
                    }
                }
            }

            return grassMaxHeight >= 0.40f || isTallGrass;
        }

        private bool CheckGPUGrassAtPosition(Vector3 worldPos, out float grassMaxHeight)
        {
            return CheckGPUGrassAtPosition(worldPos, out grassMaxHeight, out _, out _);
        }

        private bool CheckFoliageOverlap(Vector3 center, float radius)
        {
            int hits = Physics.OverlapSphereNonAlloc(center, radius, _overlapResults, _foliageMask);
            for (int i = 0; i < hits; i++)
            {
                Collider col = _overlapResults[i];
                if (col == null) continue;

                // Ignora os nossos próprios colidentes de escudo
                if (col.transform.IsChildOf(_shieldRootObject.transform)) continue;

                return true;
            }
            return false;
        }

        private bool CheckProximityToBush(Vector3 center)
        {
            if (_mainPlayer.AIData == null) return false;
            Transform root = _mainPlayer.AIData.CurrentFoliageRoot;
            if (root != null)
            {
                return Vector3.Distance(center, root.position) < 3.0f;
            }
            return true;
        }

        private bool IsAnyBotTooClose(Vector3 playerPos, float minDistance)
        {
            var gameWorld = Singleton<GameWorld>.Instance;
            if (gameWorld == null || gameWorld.AllAlivePlayersList == null)
            {
                return false;
            }

            float minSqr = minDistance * minDistance;
            var list = gameWorld.AllAlivePlayersList;
            for (int i = 0; i < list.Count; i++)
            {
                Player other = list[i];
                if (other != null && !other.IsYourPlayer && other.HealthController != null && other.HealthController.IsAlive)
                {
                    if ((other.Position - playerPos).sqrMagnitude < minSqr)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool IsZoneCovered(int zoneIndex)
        {
            if (zoneIndex >= 0 && zoneIndex < _zones.Count)
            {
                return _zones[zoneIndex].IsCovered;
            }
            return false;
        }

        private void DisableAllZones()
        {
            for (int i = 0; i < _zones.Count; i++)
            {
                _zones[i].IsCovered = false;
                if (_zones[i].ZoneCollider != null)
                {
                    _zones[i].ZoneCollider.enabled = false;
                }
            }
            CoveredZonesCount = 0;
            CoverageRatio = 0f;
            IsConcealed = false;
        }

        private void OnSettingsChanged()
        {
            if (!ModConfig.ModEnabled.Value || !ModConfig.EnableNaturalConcealment.Value)
            {
                DisableAllZones();
                return;
            }

            float offset = ModConfig.ConcealmentOffsetMeters.Value;
            for (int i = 0; i < _zones.Count; i++)
            {
                BodyConcealmentZone zone = _zones[i];
                if (zone.ZoneCollider != null)
                {
                    zone.ZoneCollider.radius = zone.BaseRadius + offset;
                    zone.ZoneCollider.height = zone.BaseHeight + (offset * 1.5f);
                }
            }
            _visualizer?.UpdateZoneScales();
        }

        private void OnVisualizerToggled()
        {
            // O visualizador lê a configuração no LateUpdate automaticamente
        }

        public void Cleanup()
        {
            ModConfig.OnConcealmentSettingsChanged -= OnSettingsChanged;
            ModConfig.OnVisualizerToggled -= OnVisualizerToggled;

            if (_shieldRootObject != null)
            {
                Destroy(_shieldRootObject);
            }
            _zones.Clear();
            _gpuGrassLayers.Clear();
            VegetationSpatialIndex.Clear();
            Instance = null;
        }

        private void OnDestroy()
        {
            Cleanup();
        }
    }
}
