using System;
using System.Collections.Generic;
using UnityEngine;

namespace TRLCoreSight.Core
{
    /// <summary>
    /// Catálogo Dinâmico de Malhas de Vegetação e Indexação Espacial O(1).
    /// Indexa todos os modelos 3D de plantas de cena sem colisor físico (capim alto,
    /// samambaias, mato denso, juncos) em um Spatial Hash Grid 2D (células de 16m × 16m)
    /// com suporte multi-célula para evitar falhas de fronteira.
    /// Permite consultas instantâneas em microsegundos com zero alocação de memória no Update.
    /// </summary>
    public static class VegetationSpatialIndex
    {
        public struct VegetationMeshEntry
        {
            public Vector3 Center;
            public float MinY;
            public float MaxY;
            public float RadiusSqr;
            public float Height;
            public bool IsTallCover; // Altura >= 0.60m (Capim alto / samambaia densa)
        }

        public const float CellSize = 16.0f;
        private static readonly Dictionary<long, List<VegetationMeshEntry>> _grid = new Dictionary<long, List<VegetationMeshEntry>>(256);
        private static int _totalIndexedPlants = 0;

        public static int TotalIndexedPlants => _totalIndexedPlants;
        public static int TotalActiveCells => _grid.Count;

        public static long GetCellKey(int cx, int cz)
        {
            return ((long)cx << 32) | (uint)cz;
        }

        /// <summary>
        /// Varre a cena ativa e indexa todas as malhas de vegetação sem colisor físico.
        /// Executado uma única vez ao carregar a raid.
        /// </summary>
        public static void IndexSceneVegetation()
        {
            try
            {
                Clear();

                Renderer[] allRenderers = UnityEngine.Object.FindObjectsOfType<Renderer>();
                if (allRenderers == null || allRenderers.Length == 0)
                {
                    Plugin.LogSource?.LogInfo("[TRL-CoreSight] VegetationSpatialIndex: Nenhum renderer encontrado na cena.");
                    return;
                }

                int foliageLayer = LayerMask.NameToLayer("Foliage");
                int grassLayer = LayerMask.NameToLayer("Grass");

                HashSet<Vector3> indexedCenters = new HashSet<Vector3>();

                for (int i = 0; i < allRenderers.Length; i++)
                {
                    Renderer rend = allRenderers[i];
                    if (rend == null || rend.gameObject == null) continue;

                    GameObject go = rend.gameObject;
                    int layer = go.layer;
                    string nameLower = go.name.ToLower();

                    // Ignora apenas LODs secundários (LOD1, LOD2, LOD3, etc.), preservando todos os LOD0
                    if (nameLower.Contains("_lod1") || nameLower.Contains("_lod2") || nameLower.Contains("_lod3") || nameLower.Contains("_lod4"))
                    {
                        continue;
                    }

                    bool isTargetLayer = (foliageLayer >= 0 && layer == foliageLayer) || (grassLayer >= 0 && layer == grassLayer);
                    bool isVegetationKeyword = nameLower.Contains("grass") || 
                                              nameLower.Contains("fern") || 
                                              nameLower.Contains("plant_") || 
                                              nameLower.Contains("reed") || 
                                              nameLower.Contains("weed") || 
                                              nameLower.Contains("burdock") || 
                                              nameLower.Contains("flower");

                    if (!isTargetLayer && !isVegetationKeyword)
                    {
                        continue;
                    }

                    // Ignora objetos com colisor físico sólido (arbustos sólidos já são tratados pelo raycast nativo)
                    Collider col = go.GetComponent<Collider>();
                    if (col != null && !col.isTrigger)
                    {
                        continue;
                    }

                    Bounds b = rend.bounds;
                    float height = b.size.y;
                    float width = b.size.x;
                    float length = b.size.z;

                    // Descarte de coberturas gigantescas de terreno (ex.: bunker grass overlay > 20m) e micro-detalhes (< 0.25m)
                    if (width > 20f || length > 20f || height < 0.25f)
                    {
                        continue;
                    }

                    // Deduplicação de posição (mesmo centro com tolerância)
                    Vector3 quantizedCenter = new Vector3(
                        Mathf.Round(b.center.x * 2f) / 2f,
                        Mathf.Round(b.center.y * 2f) / 2f,
                        Mathf.Round(b.center.z * 2f) / 2f
                    );

                    if (indexedCenters.Contains(quantizedCenter))
                    {
                        continue;
                    }
                    indexedCenters.Add(quantizedCenter);

                    float radius = Mathf.Max(b.extents.x, b.extents.z) + 0.15f; // Margem de 15cm para envolver as folhas
                    float radiusSqr = radius * radius;

                    VegetationMeshEntry entry = new VegetationMeshEntry
                    {
                        Center = b.center,
                        MinY = b.min.y,
                        MaxY = b.max.y,
                        RadiusSqr = radiusSqr,
                        Height = height,
                        IsTallCover = height >= 0.60f
                    };

                    // Registro multi-célula para proteger plantas nas fronteiras da grade
                    int minCX = Mathf.FloorToInt((b.min.x - 0.15f) / CellSize);
                    int maxCX = Mathf.FloorToInt((b.max.x + 0.15f) / CellSize);
                    int minCZ = Mathf.FloorToInt((b.min.z - 0.15f) / CellSize);
                    int maxCZ = Mathf.FloorToInt((b.max.z + 0.15f) / CellSize);

                    for (int cx = minCX; cx <= maxCX; cx++)
                    {
                        for (int cz = minCZ; cz <= maxCZ; cz++)
                        {
                            long key = GetCellKey(cx, cz);
                            if (!_grid.TryGetValue(key, out var cellList))
                            {
                                cellList = new List<VegetationMeshEntry>(8);
                                _grid[key] = cellList;
                            }
                            cellList.Add(entry);
                        }
                    }

                    _totalIndexedPlants++;
                }

                Plugin.LogSource?.LogInfo($"[TRL-CoreSight] VegetationSpatialIndex: {_totalIndexedPlants} plantas 3D indexadas em {_grid.Count} células espaciais ({CellSize}m × {CellSize}m).");
            }
            catch (Exception ex)
            {
                Plugin.LogSource?.LogError($"[TRL-CoreSight] Erro ao indexar malhas de vegetação: {ex.Message}\n{ex.StackTrace}");
            }
        }

        /// <summary>
        /// Consulta O(1) se um ponto 3D (osso/zona anatômica) está coberto pelo volume de uma planta catalogada.
        /// </summary>
        public static bool IsPointCoveredByVegetation(Vector3 worldPos, bool isProne, float poseLevel, out float plantTopY)
        {
            plantTopY = 0f;
            if (_totalIndexedPlants == 0) return false;

            int cx = Mathf.FloorToInt(worldPos.x / CellSize);
            int cz = Mathf.FloorToInt(worldPos.z / CellSize);
            long key = GetCellKey(cx, cz);

            if (!_grid.TryGetValue(key, out var cellList))
            {
                return false;
            }

            for (int i = 0; i < cellList.Count; i++)
            {
                var plant = cellList[i];
                float dx = worldPos.x - plant.Center.x;
                float dz = worldPos.z - plant.Center.z;

                if ((dx * dx + dz * dz) <= plant.RadiusSqr)
                {
                    // Checagem vertical: o osso deve estar entre a base da planta e o topo com tolerância suave
                    if (worldPos.y >= (plant.MinY - 0.25f) && worldPos.y <= (plant.MaxY + 0.15f))
                    {
                        // 1. Capim alto / Samambaias densas (>= 0.60m): cobre plenamente em qualquer postura abaixo do topo
                        if (plant.IsTallCover)
                        {
                            plantTopY = plant.MaxY;
                            return true;
                        }

                        // 2. Mato médio / Touceiras (0.30m a 0.59m): cobre deitado ou em agachamento baixo
                        if (plant.Height >= 0.30f && (isProne || poseLevel < 0.45f))
                        {
                            plantTopY = plant.MaxY;
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public static void Clear()
        {
            _grid.Clear();
            _totalIndexedPlants = 0;
        }
    }
}
