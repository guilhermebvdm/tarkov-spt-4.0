---
title: "009 — Especificação Técnica: Catálogo Dinâmico de Malhas e Indexação Espacial O(1)"
date: "2026-09-17"
status: "🟡 Em progresso"
authors: ["TRL Team", "Antigravity"]
---

# 009 — Especificação Técnica: Catálogo Dinâmico de Malhas e Indexação Espacial O(1)

## 1. Arquitetura do Componente

Criaremos uma nova classe de suporte estática de alta performance:
`TRLCoreSight.Core.VegetationSpatialIndex`

### 1.1 Estrutura de Dados do Objeto Indexado
```csharp
public struct VegetationMeshEntry
{
    public Vector3 Center;
    public float MinY;
    public float MaxY;
    public float RadiusSqr; // (max(extents.x, extents.z) + 0.15f)^2
    public float Height;
    public bool IsTallCover; // Height >= 0.60f
}
```

### 1.2 Estrutura do Spatial Hash Grid
* Tamanho de célula: `CellSize = 16.0f`
* Chave da célula:
  ```csharp
  public static long GetCellKey(int cx, int cz) => ((long)cx << 32) | (uint)cz;
  ```
* Dicionário indexado:
  `Dictionary<long, List<VegetationMeshEntry>> _grid`

### 1.3 Algoritmo de Indexação na Inicialização da Raid
* Chamado uma única vez em `NaturalConcealmentManager.Initialize(mainPlayer)` ou `OnRaidStarted()`:
  1. `Renderer[] renderers = Object.FindObjectsOfType<Renderer>();`
  2. Filtro com base nas camadas e nomes identificados em `RESUMO_VEGETACAO.md`:
     - Keywords: `grass`, `fern`, `plant_`, `reed`, `weed`, `burdock`, `flower`
     - Checagem: sem colisor (`col == null`) ou com colisor não-sólido (`col.isTrigger`).
     - Descarte de malhas gigantes de chão (extents > 15m) ou muito baixas (height < 0.25m).
  3. Prevenção de duplicatas por `LODGroup` (armazena apenas uma entrada por grupo de malhas no mesmo centro).
  4. Inserção na célula correspondente da grade.

### 1.4 Algoritmo de Consulta Zonal por Ponto
```csharp
public static bool IsPointCoveredByVegetation(Vector3 worldPos, bool isProne, float poseLevel, out float plantTopY)
{
    plantTopY = 0f;
    int cx = Mathf.FloorToInt(worldPos.x / 16.0f);
    int cz = Mathf.FloorToInt(worldPos.z / 16.0f);
    long key = ((long)cx << 32) | (uint)cz;

    if (!_grid.TryGetValue(key, out var entries)) return false;

    for (int i = 0; i < entries.Count; i++)
    {
        var plant = entries[i];
        float dx = worldPos.x - plant.Center.x;
        float dz = worldPos.z - plant.Center.z;
        if ((dx * dx + dz * dz) <= plant.RadiusSqr)
        {
            if (worldPos.y >= (plant.MinY - 0.2f) && worldPos.y <= (plant.MaxY + 0.1f))
            {
                if (plant.IsTallCover) // >= 0.60m
                {
                    plantTopY = plant.MaxY;
                    return true;
                }
                if (plant.Height >= 0.30f && (isProne || poseLevel < 0.4f))
                {
                    plantTopY = plant.MaxY;
                    return true;
                }
            }
        }
    }
    return false;
}
```

## 2. Integração no `NaturalConcealmentManager.cs`

Em `EvaluateZonalConcealment()`:
```csharp
bool coveredByMesh = VegetationSpatialIndex.IsPointCoveredByVegetation(
    worldPos, isProne, poseLevel, out float plantTopY);

zone.IsCovered = coveredByFoliage || coveredByTreeTrigger || coveredByTallGrass || coveredByMesh;
```

## 3. Conformidade e Performance (§9)
* **Zero Alocação no Update:** Estruturas em `struct`, `Dictionary` estático pré-alocado, reuso de buffers.
* **Complexidade:** O(1) lookup temporal no tick de 0.2s.
* **Isolamento de Memória:** Limpeza explícita de `_grid.Clear()` em `OnRaidFinished()` / `Cleanup()`.
