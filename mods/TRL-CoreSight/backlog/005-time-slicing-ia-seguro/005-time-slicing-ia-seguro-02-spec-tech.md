# 005 — camuflagem-natural-e-percepcao-ia · Spec Técnica

**Mod:** TRL-CoreSight  
**Spec funcional:** [005-time-slicing-ia-seguro-01-spec.md](005-time-slicing-ia-seguro-01-spec.md)  
**Criado:** 2026-09-15T22:40:00Z  

> Fonte primária de verdade para qualquer assinatura, fórmula ou ponto de patch: [references/eft-decompiled/Assembly-CSharp/](../../../../references/eft-decompiled/Assembly-CSharp/). Toda referência ao código do EFT cita `arquivo.cs:linha`.

---

## 1. Estratégia

Eliminar a visão de raio-x milagrosa de bots através de copas de árvores e capinzais altos, permitindo ao jogador utilizar a vegetação natural como cobertura tática e estimulando reações autênticas de IA (tiros de supressão, arremesso de granadas e varredura na última posição conhecida via SAIN), combinado a um alívio de CPU via time-slicing de visão em bots pacíficos distantes.

### 1.1. Fundamentação Física dos Layers da Unity no EFT
Conforme verificado em [`LayerMaskClass.cs:103`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/references/eft-decompiled/Assembly-CSharp/LayerMaskClass.cs#L103):
```csharp
HighPolyWithTerrainMaskAI = (1 << LayerMask.NameToLayer("Terrain")) | (1 << LayerMask.NameToLayer("HighPolyCollider")) | (1 << LayerMask.NameToLayer("Grass")) | (1 << LayerMask.NameToLayer("Foliage"));
```
- Os sensores de visão da IA da BSG e do SAIN utilizam estritamente o `HighPolyWithTerrainMaskAI`.
- O layer `Foliage` (Layer 15) e `Grass` (Layer 14):
  - **Bloqueiam visão da IA:** Qualquer raycast óptico de bot colide com a superfície e sinaliza linha de visão obstruída (`LineOfSight = false`).
  - **Não bloqueiam balas:** O layer de balística do jogo (`HitColliderMask`) ignora `Foliage` e `Grass`, permitindo penetração balística com 100% de dano.
  - **Não bloqueiam movimentação do jogador:** O colisor físico de movimentação (`PlayerCollisionsMask`) não colide com `Foliage`, garantindo passagem livre e fluida sem travamento de locomoção.

### 1.2. Camuflagem Rasteira em Grama Alta (Player Grass Concealment Shield)
- Um colisor de trigger/física volumétrica posicionado no layer `LayerMask.NameToLayer("Foliage")` centrado na posição do jogador:
  - **Raio:** 2.0 metros (conforme aprovado pelo usuário).
  - **Postura Deitado (`IsInPronePose == true`):** Altura de cobertura de 0.80m a partir do solo.
  - **Postura Agachado (`IsInPronePose == false && PoseLevel < 0.6f`):** Altura de cobertura de 0.45m (cobre pernas/abdômen; cabeça permanece vulnerável a ângulos superiores).
  - **Postura Em Pé (`PoseLevel >= 0.6f`):** Barreira inativa (visão normal).
  - **Verificação de Terreno/Grama:** A barreira só é ativada se o jogador estiver em superfície de vegetação/terra/grama (verificado via raycast de solo com detecção de `Terrain` ou material sonoro de grama `BaseBallistic.ESurfaceSound.Grass`). Se o jogador deitar em asfalto, concreto ou dentro de casas, o escudo permanece desligado.
  - **Quebra por Tropeço (< 4.0 metros):** Se qualquer bot se aproximar a menos de 4 metros, a barreira é temporariamente neutralizada, permitindo que o bot detecte o jogador camuflado no capim.

### 1.3. Oclusão de Copas de Árvores e Arbustos (Raio de 10 metros)
- Detecta árvores e arbustos próximos (raio de 10m):
  - Ao entrar sob a projeção da copa de árvores densas ou dentro da malha de um arbusto, reforça a oclusão superior contra bots a longa distância (> 50m).

### 1.4. Interação Orgânica com o SAIN
- Quando a barreira bloqueia a visão de um bot que estava vendo o jogador:
  - O bot grava imediatamente a última posição em `EnemyKnownPlaces.LastKnownPosition`.
  - O SAIN ativa o comportamento orgânico de tiro de supressão (`SAINBotSuppressClass.SuppressPosition`), disparando rajadas no capim/arbusto e arremessando granadas.

### 1.5. Time-Slicing Suave de IA Distante
- Bots fora de combate (`IsPeace == true`) situados a mais de 100 metros da câmera têm sua frequência de checagem visual amortizada (distribuída em lotes a cada 0.3s a 0.5s), poupando preciosos ciclos de CPU na *Main Thread*.

---

## 2. Pontos de Integração e Ganchos

| Componente | Tipo | Motivo |
|---|---|---|
| `LayerMaskClass.Foliage` | Layer Nativo | Camada canônica do EFT que obstrui visão de bots sem interferir em balas ou movimentação. |
| `Player.MovementContext` | Leitura de Postura | Obter `IsInPronePose`, `PoseLevel` e locomoção do jogador local. |
| `PerformanceManager.Initialize()` | Ciclo de Vida | Inicialização do componente `NaturalConcealmentManager`. |
| `PerformanceManager.Cleanup()` | Ciclo de Vida | Destruição segura e remoção imediata dos colisores ao fim da raid. |

---

## 3. Novas propriedades F12 (BepInEx)

Adicionar na seção **`12. Camuflagem Natural & Visão de IA`** em `Configuration/ModConfig.cs`:

| Seção | Nome (EN) | Tipo | Padrão | Faixa | Tooltip (pt-BR) |
|---|---|---|---|---|---|
| `12. Camuflagem & Visão IA` | `EnableNaturalConcealment` | bool | `true` | — | Bloqueia visão de bots através de capim alto e copas de árvores ao redor do jogador. |
| `12. Camuflagem & Visão IA` | `GrassConcealmentRadius` | float | `2.0` | 1.0 a 4.0 | Raio em metros do escudo de camuflagem de grama ao redor do jogador deitado. |
| `12. Camuflagem & Visão IA` | `BreakProximityDistance` | float | `4.0` | 2.0 a 8.0 | Distância mínima em metros para um bot detectar o jogador camuflado no capim (tropeço). |
| `12. Camuflagem & Visão IA` | `TreeCanopyOcclusion` | bool | `true` | — | Bloqueia tiros milagrosos de bots através das folhas e copas de árvores a longa distância. |
| `12. Camuflagem & Visão IA` | `EnableBotTimeSlicing` | bool | `true` | — | Amortiza checagens de visão de bots pacíficos a mais de 100m para aliviar o uso de CPU. |

---

## 4. Arquivos do mod

| Arquivo | Ação | Resumo |
|---|---|---|
| `Configuration/ModConfig.cs` | MODIFICAR | Inclusão de configurações da Seção 12 e evento de configuração. |
| `PROPRIEDADES.md` | MODIFICAR | Documentação em pt-BR da seção 12 de F12. |
| `Core/NaturalConcealmentManager.cs` | CRIAR | Componente que cria e reposiciona dinamicamente os volumes de oclusão de vegetação no layer `Foliage`. |
| `Core/PerformanceManager.cs` | MODIFICAR | Ciclo de vida, atualização por frame e telemetria do `NaturalConcealmentManager`. |
| `Plugin.cs` | MODIFICAR | Bump de versão SemVer para `0.3.6`. |
| `TRL-CoreSight.csproj` | MODIFICAR | Inclusão do novo arquivo compilado e bump SemVer para `0.3.6`. |
| `mod.json` | MODIFICAR | Bump de versão para `0.3.6`. |

---

## 5. Stubs de código

### `Core/NaturalConcealmentManager.cs`

```csharp
using System.Collections.Generic;
using Comfort.Common;
using EFT;
using TRLCoreSight.Configuration;
using UnityEngine;

namespace TRLCoreSight.Core
{
    public class NaturalConcealmentManager : MonoBehaviour
    {
        public static NaturalConcealmentManager Instance { get; private set; }

        private Player _mainPlayer;
        private GameObject _shieldObject;
        private CapsuleCollider _grassCollider;
        private SphereCollider _canopyCollider;
        private int _foliageLayer;
        private float _lastSurfaceCheckTime = 0f;
        private bool _isOnVegetation = false;

        public bool IsConcealed { get; private set; }
        public bool IsUnderCanopy { get; private set; }

        private void Awake()
        {
            Instance = this;
            _foliageLayer = LayerMask.NameToLayer("Foliage");
            if (_foliageLayer < 0)
            {
                _foliageLayer = 15;
            }
        }

        public void Initialize(Player mainPlayer)
        {
            _mainPlayer = mainPlayer;
            CreateShieldColliders();
        }

        private void CreateShieldColliders()
        {
            if (_shieldObject != null)
            {
                Destroy(_shieldObject);
            }

            _shieldObject = new GameObject("TRL_ConcealmentShield");
            _shieldObject.layer = _foliageLayer;

            _grassCollider = _shieldObject.AddComponent<CapsuleCollider>();
            _grassCollider.isTrigger = false;
            _grassCollider.radius = ModConfig.GrassConcealmentRadius.Value;
            _grassCollider.height = 0.8f;
            _grassCollider.direction = 1; // Eixo Y
            _grassCollider.enabled = false;

            _canopyCollider = _shieldObject.AddComponent<SphereCollider>();
            _canopyCollider.isTrigger = false;
            _canopyCollider.radius = 3.5f;
            _canopyCollider.enabled = false;
        }

        public void OnUpdate()
        {
            if (!ModConfig.ModEnabled.Value || !ModConfig.EnableNaturalConcealment.Value || _mainPlayer == null)
            {
                DisableColliders();
                return;
            }

            if (_mainPlayer.HealthController != null && !_mainPlayer.HealthController.IsAlive)
            {
                DisableColliders();
                return;
            }

            Vector3 playerPos = _mainPlayer.Position;
            _shieldObject.transform.position = playerPos;

            // 1. Checagem de proximidade extrema de bots (< BreakProximityDistance)
            if (IsAnyBotTooClose(playerPos, ModConfig.BreakProximityDistance.Value))
            {
                DisableColliders();
                return;
            }

            // 2. Verificação de superfície de solo a cada 0.5s
            if (Time.time - _lastSurfaceCheckTime > 0.5f)
            {
                _lastSurfaceCheckTime = Time.time;
                _isOnVegetation = CheckIfOnVegetation(playerPos);
                _canopyCollider.enabled = ModConfig.TreeCanopyOcclusion.Value && CheckIfUnderCanopy(playerPos);
                IsUnderCanopy = _canopyCollider.enabled;
            }

            // 3. Postura do jogador e camuflagem de grama
            bool isProne = _mainPlayer.MovementContext != null && _mainPlayer.MovementContext.IsInPronePose;
            float poseLevel = _mainPlayer.MovementContext != null ? _mainPlayer.MovementContext.PoseLevel : 1.0f;

            if (_isOnVegetation && isProne)
            {
                _grassCollider.radius = ModConfig.GrassConcealmentRadius.Value;
                _grassCollider.height = 0.85f;
                _grassCollider.center = new Vector3(0f, 0.42f, 0f);
                _grassCollider.enabled = true;
                IsConcealed = true;
            }
            else if (_isOnVegetation && poseLevel < 0.6f) // Agachado
            {
                _grassCollider.radius = ModConfig.GrassConcealmentRadius.Value * 0.75f;
                _grassCollider.height = 0.5f;
                _grassCollider.center = new Vector3(0f, 0.25f, 0f);
                _grassCollider.enabled = true;
                IsConcealed = true;
            }
            else
            {
                _grassCollider.enabled = false;
                IsConcealed = false;
            }
        }

        private bool CheckIfOnVegetation(Vector3 origin)
        {
            if (Physics.Raycast(origin + Vector3.up * 0.5f, Vector3.down, out RaycastHit hit, 2.0f, LayerMaskClass.TerrainMask | LayerMaskClass.HighPolyCollider))
            {
                if (hit.collider is TerrainCollider)
                {
                    return true;
                }

                string colName = hit.collider.gameObject.name.ToLower();
                if (colName.Contains("grass") || colName.Contains("dirt") || colName.Contains("ground") || colName.Contains("mud"))
                {
                    return true;
                }
            }
            return false;
        }

        private bool CheckIfUnderCanopy(Vector3 origin)
        {
            if (Physics.Raycast(origin + Vector3.up * 0.5f, Vector3.up, out RaycastHit hit, 12.0f, LayerMaskClass.HighPolyCollider | (1 << _foliageLayer)))
            {
                string hitName = hit.collider.gameObject.name.ToLower();
                if (hitName.Contains("tree") || hitName.Contains("leaf") || hitName.Contains("canopy") || hitName.Contains("branch"))
                {
                    return true;
                }
            }
            return false;
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
            int count = list.Count;
            for (int i = 0; i < count; i++)
            {
                Player p = list[i];
                if (p != null && !p.IsYourPlayer && (p.Position - playerPos).sqrMagnitude < minSqr)
                {
                    return true;
                }
            }
            return false;
        }

        private void DisableColliders()
        {
            if (_grassCollider != null) _grassCollider.enabled = false;
            if (_canopyCollider != null) _canopyCollider.enabled = false;
            IsConcealed = false;
            IsUnderCanopy = false;
        }

        public void Cleanup()
        {
            DisableColliders();
            if (_shieldObject != null)
            {
                Destroy(_shieldObject);
            }
            Instance = null;
        }

        private void OnDestroy()
        {
            Cleanup();
        }
    }
}
```

---

## 6. Checklist de Implementação

- [ ] Adicionar propriedades da seção 12 em `Configuration/ModConfig.cs`.
- [ ] Atualizar `PROPRIEDADES.md` com a seção 12 em pt-BR.
- [ ] Criar `Core/NaturalConcealmentManager.cs`.
- [ ] Integrar `NaturalConcealmentManager` no `PerformanceManager.cs` (Initialize, OnUpdate, OnGUI, Cleanup).
- [ ] Incrementar versão para `0.3.6` em `Plugin.cs`, `TRL-CoreSight.csproj` e `mod.json`.
- [ ] Compilar a build via `dotnet build` e isolar em `mods/TRL-CoreSight/builds/TRL-CoreSight.dll`.

---

## 7. Conformidade com Skills (Auto-Checklist)

| # | Check | Status | Evidência / Razão |
|---|---|:---:|---|
| 1 | Lifecycle de raid: start hook + stop hooks idempotentes — AP-01 | ✅ | Gerenciado pelo `PerformanceManager` e restaurado no `Cleanup()`. |
| 2 | Filtro MainPlayer/Fika em todo patch que reage a ação de player — AP-02 | ✅ | O escudo se movimenta com o `_mainPlayer` e monitora distância de bots pelo `AllAlivePlayersList`. |
| 3 | Alvos ofuscados/virtuais resolvidos por assinatura — AP-03 | ✅ | Opera sobre layers canônicos de física e Unity `CapsuleCollider`. |
| 4 | Mudança de estado via API canônica do EFT; side-effects mapeados — AP-04 | ✅ | Layer `Foliage` não bloqueia balas e nem colide com player; afeta unicamente IA. |
| 5 | Estado entre raids: raid1→exit→raid2 e alt-F4 cobertos | ✅ | GameObject e colisores destruídos no `Cleanup()`. |
| 6 | Semântica/defaults/faixas de cada ConfigEntry sem ambiguidade — AP-05 | ✅ | Configurações com tooltips e faixas numéricas definidas. |
| 7 | Todo patch-point reconfirmado no `.cs` do dump — AP-09 | ✅ | `LayerMaskClass.cs:103` e `VisionRaycastJob.cs:17` auditados. |

---

## Histórico

| Data | Evento |
|---|---|
| 2026-09-15 | Spec técnica criada com escudo de oclusão no layer Foliage, verificação de terreno e quebra por proximidade. |
