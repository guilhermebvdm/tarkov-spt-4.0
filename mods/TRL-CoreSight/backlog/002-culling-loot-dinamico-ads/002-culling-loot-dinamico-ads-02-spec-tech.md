# 002 — culling-loot-dinamico-ads · Spec Técnica

**Mod:** TRL-CoreSight  
**Spec funcional:** [002-culling-loot-dinamico-ads-01-spec.md](002-culling-loot-dinamico-ads-01-spec.md)  
**Criado:** 2026-09-15T22:25:00Z  

> Fonte primária de verdade para qualquer assinatura, fórmula ou ponto de patch: [references/eft-decompiled/Assembly-CSharp/](../../../../references/eft-decompiled/Assembly-CSharp/). Toda referência ao código do EFT cita `arquivo.cs:linha`.

---

## 1. Estratégia

Otimizar o pipeline gráfico e aliviar centenas de Draw Calls da GPU em mapas densos (como Streets of Tarkov, Interchange e Reserve) gerenciando a visibilidade de itens soltos no cenário ([`LootItem.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/references/eft-decompiled/Assembly-CSharp/EFT.Interactive/LootItem.cs)).

A estratégia baseia-se em:
1. **Classificação por Tamanho de Células de Inventário (`XYCellSizeStruct`):**
   - Ler o tamanho do item através de `lootItem.Item.CalculateCellSize()` ([`Item.cs:689`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/references/eft-decompiled/Assembly-CSharp/EFT.InventoryLogic/Item.cs#L689)):
     - **Itens Pequenos ($\le 2$ slots, ex.: 1x1 ou 1x2):** Balas, parafusos, porcas, chaves, pilhas. Distância de culling: 25 metros.
     - **Itens Médios ($3$ a $6$ slots, ex.: 2x2 ou 2x3):** Maletas médicas, kits de limpeza, capacetes, comida volumosa. Distância de culling: 45 metros.
     - **Itens Grandes ($> 6$ slots, ex.: 2x4 ou maior):** Fuzis de assalto, snipers, mochilas, armaduras pesadas. **100% Imunes** (sempre visíveis).
2. **Preservação Física e de Interação (Zero-Side-Effects):**
   - Alternar apenas a renderização visual via `lootItem.method_10(isVisible)` ([`LootItem.cs:646`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/references/eft-decompiled/Assembly-CSharp/EFT.Interactive/LootItem.cs#L646)) ou desabilitando os `Renderer` e `LODGroup` de malha.
   - O `BoxCollider` rígido e os colisores de física do `AssetPoolObject` **nunca são tocados**. O item mantém colisão física com mesas/pisos e resposta de interação com a tecla 'F'.
3. **ADS Bypass Suave com Frustum Filtering:**
   - Ao levantar a mira (`ProceduralWeaponAnimation.IsAiming == true`), os itens contidos no cone de visão (Frustum da câmera ativa / luneta PiP) são restaurados imediatamente.
   - Itens fora do cone de visão continuam culled para evitar picos de carregamento de materiais.
4. **Time-Slicing Amortizado:**
   - Varredura em lotes distribuídos na *Main Thread* (50 a 100 itens por frame) para garantir tempo de frame estável (< 0.2ms).

---

## 2. Pontos de patch e ganchos

| Alvo (Assembly) | Tipo | Motivo |
|---|---|---|
| [`GameWorld.cs:1514`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/GameWorld.cs#L1514) (`RegisterLoot<T>`) | Postfix | Registrar itens dropados dinamicamente em raid (`GameWorld.ThrowItem`) no gerenciador de culling. |
| [`LootItem.cs:646`](../../../../references/eft-decompiled/Assembly-CSharp/EFT.Interactive/LootItem.cs#L646) (`method_10`) | Chamada Direta | Ativar/desativar visibilidade e LODGroup de itens sem afetar colisores. |
| [`Item.cs:689`](../../../../references/eft-decompiled/Assembly-CSharp/EFT.InventoryLogic/Item.cs#L689) (`CalculateCellSize`) | Chamada Direta | Obter dimensões de grade X e Y de cada item solto no mundo. |

---

## 3. Novas propriedades F12 (BepInEx)

Adicionar na seção **`9. Otimização de Loot (Loose Loot Culling)`** em `Configuration/ModConfig.cs`:

| Seção | Nome (EN) | Tipo | Padrão | Faixa | Avançado | Tooltip (pt-BR) |
|---|---|---|---|---|---|---|
| `9. Otimização de Loot` | `EnableLootCulling` | bool | `true` | — | — | Oculta a renderização de pequenos itens soltos no chão a média/longa distância para economizar draw calls. |
| `9. Otimização de Loot` | `SmallLootDistance` | float | `25.0` | 15.0 a 40.0 | — | Distância máxima para renderizar itens pequenos de 1x1 e 1x2 (balas, porcas, chaves). |
| `9. Otimização de Loot` | `MediumLootDistance` | float | `45.0` | 25.0 a 70.0 | — | Distância máxima para renderizar itens médios de 2x2 e 2x3 (medkits, capacetes, comida). |
| `9. Otimização de Loot` | `EnableADSLootBypass` | bool | `true` | — | — | Restaura imediatamente a visibilidade de itens dentro do cone de visão ao mirar com a arma (ADS / Luneta). |

---

## 4. Arquivos do mod

| Arquivo | Ação | Resumo |
|---|---|---|
| `Configuration/ModConfig.cs` | MODIFICAR | Registrar opções na seção 9 de F12. |
| `PROPRIEDADES.md` | MODIFICAR | Documentar as opções da seção 9 em pt-BR. |
| `Core/LootCullingManager.cs` | CRIAR | Componente que varre e gerencia o culling amortizado de `LootItem` em raid. |
| `Patches/LootRegisterPatch.cs` | CRIAR | Patch Postfix em `GameWorld.RegisterLoot` para registrar itens dropados dinamicamente. |
| `Core/PerformanceManager.cs` | MODIFICAR | Inicializar e limpar `LootCullingManager` no ciclo da raid e adicionar status no `OnGUI`. |
| `Plugin.cs` | MODIFICAR | Registrar `LootRegisterPatch` no `Awake()`. |
| `TRL-CoreSight.csproj` | MODIFICAR | Incluir `LootCullingManager.cs` e `LootRegisterPatch.cs`. |

---

## 5. Stubs de código

### `Core/LootCullingManager.cs`

```csharp
using System.Collections.Generic;
using Comfort.Common;
using EFT;
using EFT.Interactive;
using EFT.InventoryLogic;
using TRLCoreSight.Configuration;
using UnityEngine;

namespace TRLCoreSight.Core
{
    public class LootCullingManager : MonoBehaviour
    {
        public static LootCullingManager Instance { get; private set; }

        private readonly List<TrackedLoot> _trackedItems = new List<TrackedLoot>(2048);
        private readonly Plane[] _cameraPlanes = new Plane[6];
        private Player _mainPlayer;
        private Camera _mainCamera;
        private int _currentIndex;
        private const int BATCH_SIZE = 64;

        private struct TrackedLoot
        {
            public LootItem Loot;
            public Transform Transform;
            public float MaxDistanceSqr;
            public bool IsCulled;
        }

        private void Awake()
        {
            Instance = this;
        }

        public void Initialize(Player mainPlayer)
        {
            _mainPlayer = mainPlayer;
            _mainCamera = GetActiveCamera();
            ScanExistingLoot();
        }

        private Camera GetActiveCamera()
        {
            if (_mainCamera == null || !_mainCamera.isActiveAndEnabled)
            {
                _mainCamera = (CameraClass.Exist && CameraClass.Instance.Camera != null)
                    ? CameraClass.Instance.Camera
                    : Camera.main;
            }
            return _mainCamera;
        }

        public void ScanExistingLoot()
        {
            _trackedItems.Clear();
            var gameWorld = Singleton<GameWorld>.Instance;
            if (gameWorld == null || gameWorld.LootList == null)
            {
                return;
            }

            var list = gameWorld.LootList;
            int count = list.Count;
            for (int i = 0; i < count; i++)
            {
                if (list[i] is LootItem lootItem)
                {
                    RegisterLootItem(lootItem);
                }
            }
        }

        public void RegisterLootItem(LootItem lootItem)
        {
            if (lootItem == null || lootItem.Item == null)
            {
                return;
            }

            XYCellSizeStruct size = lootItem.Item.CalculateCellSize();
            int slots = size.X * size.Y;

            // Armas longas, coletes e mochilas (> 6 slots) nunca sofrem culling
            if (slots > 6)
            {
                return;
            }

            float maxDist = (slots <= 2)
                ? ModConfig.SmallLootDistance.Value
                : ModConfig.MediumLootDistance.Value;

            _trackedItems.Add(new TrackedLoot
            {
                Loot = lootItem,
                Transform = lootItem.transform,
                MaxDistanceSqr = maxDist * maxDist,
                IsCulled = false
            });
        }

        public void OnUpdate()
        {
            if (!ModConfig.ModEnabled.Value || !ModConfig.EnableLootCulling.Value || _trackedItems.Count == 0)
            {
                return;
            }

            Camera activeCam = GetActiveCamera();
            if (activeCam == null || _mainPlayer == null)
            {
                return;
            }

            bool isAiming = _mainPlayer.ProceduralWeaponAnimation != null && _mainPlayer.ProceduralWeaponAnimation.IsAiming;
            bool adsBypass = isAiming && ModConfig.EnableADSLootBypass.Value;

            if (adsBypass)
            {
                GeometryUtility.CalculateFrustumPlanes(activeCam, _cameraPlanes);
            }

            Vector3 camPos = activeCam.transform.position;
            int total = _trackedItems.Count;
            int processed = 0;

            while (processed < BATCH_SIZE && _currentIndex < total)
            {
                TrackedLoot item = _trackedItems[_currentIndex];
                if (item.Loot == null || item.Transform == null)
                {
                    _trackedItems.RemoveAt(_currentIndex);
                    total--;
                    continue;
                }

                float sqrDist = (item.Transform.position - camPos).sqrMagnitude;
                bool shouldCull = sqrDist > item.MaxDistanceSqr;

                // ADS Bypass com filtro de frustum
                if (adsBypass && shouldCull)
                {
                    if (GeometryUtility.TestPlanesAABB(_cameraPlanes, new Bounds(item.Transform.position, Vector3.one * 0.5f)))
                    {
                        shouldCull = false;
                    }
                }

                if (shouldCull != item.IsCulled)
                {
                    item.IsCulled = shouldCull;
                    item.Loot.method_10(!shouldCull);
                    _trackedItems[_currentIndex] = item;
                }

                _currentIndex++;
                processed++;
            }

            if (_currentIndex >= total)
            {
                _currentIndex = 0;
            }
        }

        public void RestoreAll()
        {
            for (int i = 0; i < _trackedItems.Count; i++)
            {
                var item = _trackedItems[i];
                if (item.Loot != null && item.IsCulled)
                {
                    item.Loot.method_10(true);
                }
            }
            _trackedItems.Clear();
            Instance = null;
        }

        private void OnDestroy()
        {
            RestoreAll();
        }
    }
}
```

### `Patches/LootRegisterPatch.cs`

```csharp
using System.Reflection;
using EFT;
using EFT.Interactive;
using HarmonyLib;
using SPT.Reflection.Patching;
using TRLCoreSight.Core;

namespace TRLCoreSight.Patches
{
    /// <summary>
    /// Registra dinamicamente itens soltos no mundo no gerenciador de culling.
    /// ref: Assembly-CSharp/GameWorld.cs:1514
    /// </summary>
    public class LootRegisterPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            // ref: Assembly-CSharp/GameWorld.cs:1514
            return AccessTools.Method(typeof(GameWorld), nameof(GameWorld.RegisterLoot))
                .MakeGenericMethod(typeof(InteractableObject));
        }

        [PatchPostfix]
        private static void Postfix(InteractableObject loot)
        {
            if (loot is LootItem lootItem && LootCullingManager.Instance != null)
            {
                LootCullingManager.Instance.RegisterLootItem(lootItem);
            }
        }
    }
}
```

---

## 6. Fluxo de dados

```
[Início da Raid / Spawn de Loot]
         │
         ▼
[LootCullingManager.ScanExistingLoot] (GameWorld.LootList)
         │
         ├─── slots > 6 (Armas/Mochilas)? ──► Ignorado (Sempre visível)
         │
         └─── slots <= 6 (Pequeno/Médio)? ──► Registrado no Tracker
                   │
                   ▼
       [Loop Amortizado: 64 itens/frame]
                   │
                   ├─── dist <= maxDist? ──► method_10(true)  (Visível)
                   │
                   └─── dist > maxDist?
                             │
                             ├─── Em ADS dentro do Frustum? ──► method_10(true) (Visível)
                             │
                             └─── Fora do Frustum / Normal?  ──► method_10(false) (Ocultado)
```

---

## 7. Riscos e dependências

- **Colisores de Física:** `method_10(bool)` desativa apenas `Renderer` e `LODGroup`; o `BoxCollider` e os componentes da PhysX continuam ativos, prevenindo quedas no chão ou perdas de item.
- **Micro-stutters:** O processamento em lotes de 64 itens por frame e a filtragem de frustum no ADS evitam picos repentinos de CPU.
- **Interação 'F':** A checagem de proximidade da BSG usa o colisor e a distância euclidiana do player (< 2.0m). Como o item é visível a até 25m, o jogador sempre o verá antes de poder interagir.

---

## 8. Checklist de implementação

- [ ] Adicionar as opções `EnableLootCulling`, `SmallLootDistance`, `MediumLootDistance` e `EnableADSLootBypass` em `Configuration/ModConfig.cs`.
- [ ] Atualizar `PROPRIEDADES.md` com a seção 9 de Otimização de Loot.
- [ ] Criar `Core/LootCullingManager.cs` com a lógica amortizada e ADS Frustum Bypass.
- [ ] Criar `Patches/LootRegisterPatch.cs` para suportar itens dropados dinamicamente.
- [ ] Inicializar e limpar `LootCullingManager` em `Core/PerformanceManager.cs`.
- [ ] Registrar `new LootRegisterPatch().Enable()` em `Plugin.cs`.
- [ ] Incluir os novos arquivos em `TRL-CoreSight.csproj`.
- [ ] Incrementar SemVer para `0.3.3` em `Plugin.cs`, `TRL-CoreSight.csproj` e `mod.json`.
- [ ] Compilar a build via `dotnet build` e validar `mods/TRL-CoreSight/builds/TRL-CoreSight.dll`.

---

## 9. Conformidade com skills (auto-checklist)

| # | Check | Status | Evidência / razão |
|---|---|:---:|---|
| 1 | Lifecycle de raid: start hook + stop hooks idempotentes (`GameWorld.OnDestroy` + `BaseLocalGame.Stop`) — AP-01 | ✅ | `LootCullingManager` é gerenciado como componente do `PerformanceManager` e restaurado no `Cleanup()`. |
| 2 | Filtro MainPlayer/Fika em todo patch que reage a ação de player — AP-02 | ✅ | `_mainPlayer` é resolvido de forma dinâmica e itens são medidos pela câmera ativa (`CameraClass.Instance.Camera`). |
| 3 | Alvos ofuscados/virtuais resolvidos por assinatura; TODOS os overrides auditados — AP-03 | ✅ | Métodos `GameWorld.RegisterLoot`, `Item.CalculateCellSize` e `LootItem.method_10` auditados e tipados. |
| 4 | Mudança de estado via API canônica do EFT; side-effects mapeados — AP-04 | ✅ | Utiliza `LootItem.method_10(bool)` oficial da BSG para alternar visibilidade sem tocar colisores. |
| 5 | Estado entre raids: raid1→exit→raid2 e alt-F4/morte/MIA cobertos | ✅ | `RestoreAll()` restaura todos os renderers ao final da raid e limpa listas internas. |
| 6 | Semântica/defaults/faixas de cada ConfigEntry sem ambiguidade (incl. estado neutro) — AP-05 | ✅ | Configurações tipadas com `AcceptableValueRange<float>` e tooltips em pt-BR. |
| 7 | Re-invocação de método patcheado tem reentry-guard/`ReversePatch` (sem recursão infinita) — AP-07 | ✅ | Patch puramente Postfix no `RegisterLoot`, sem chamadas recursivas. |
| 8 | Flags/caches de intercept validados contra o contexto atual após troca (arma/operação/tela) — AP-08 | ✅ | Frustum recalculado a cada frame durante o ADS. |
| 9 | Todo patch-point reconfirmado no `.cs` do dump (não só no recon); "não existe" conferido no `types-index.json`, nunca num grep vazio — AP-09 | ✅ | Confirmado em `GameWorld.cs:1514`, `LootItem.cs:646` e `Item.cs:689`. |
| 10 | Skill EFT usada como lever confirmada **não-inerte** (`SkillsSettings` ≠ `[]` no `globals.json`); se inerte, efeito entregue por patch direto — AP-10 | N/A | Não utiliza skills de RPG como alavanca. |
| 11 | Pacote FIKA próprio: envelope de comprimento + só `TryGet*` + flag `Valid`, campos resetados no `Deserialize`, envio só na main thread, registro por instância/evento (nunca `bool`), zero `UnregisterPacket`, airbag com throttle em todo callback — AP-11 | N/A | Não cria pacotes de rede próprios; renderização puramente local do cliente. |

---

## Histórico

| Data | Evento |
|---|---|
| 2026-09-15 | Spec técnica criada com time-slicing amortizado, classificação por slots e ADS frustum filtering. |
