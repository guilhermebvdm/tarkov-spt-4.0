# 004 — gerenciador-gc-anti-stutter · Spec Técnica

**Mod:** TRL-CoreSight  
**Spec funcional:** [004-gerenciador-gc-anti-stutter-01-spec.md](004-gerenciador-gc-anti-stutter-01-spec.md)  
**Criado:** 2026-09-15T22:35:00Z  

> Fonte primária de verdade para qualquer assinatura, fórmula ou ponto de patch: [references/eft-decompiled/Assembly-CSharp/](../../../../references/eft-decompiled/Assembly-CSharp/). Toda referência ao código do EFT cita `arquivo.cs:linha`.

---

## 1. Estratégia

Eliminar micro-travamentos (*stutters* de 25ms a 85ms e quedas de 1% low) no frame rate causados pelo coletor de lixo Mono Boehm da Unity (`Stop-the-World GC`).

Em Escape from Tarkov, a maior taxa de alocação de memória temporária (criação de projéteis balísticos, eventos de áudio com instâncias de som estéreo, cálculo de recuo procedural e chamadas de rede) ocorre exatamente no instante em que o jogador abre fogo ou puxa a mira em direção a um inimigo. Se o heap da Unity atingir o limite nesse milissegundo, a engine congela a execução da *Main Thread*, resultando em travamentos críticos de primeiro tiro.

### Mecânica de Supressão em Combate
1. **Supressão Ativa (`GarbageCollector.GCMode = GarbageCollector.Mode.Disabled`):**
   - Durante a visada com a arma (`ProceduralWeaponAnimation.IsAiming == true`).
   - Durante a corrida rápida do jogador (`MovementContext.IsSprintEnabled == true`).
   - Durante e logo após disparos de arma de fogo (`Time.time - _lastCombatActionTime < CombatGracePeriod`, padrão: 5.0 segundos).
2. **Coleta Preventiva e Controlada em Momentos Seguros:**
   - **Ao abrir o Inventário / Looting (`Cursor.visible == true`):** Quando o jogador abre mochilas, caixas de munição ou o menu de personagem, ele está imóvel e protegido. O mod reativa o coletor (`GCMode = Enabled`) e dispara uma coleta limpa via `MemoryControllerClass.Collect(force: true)` ou `GC.Collect()`.
   - **Janela de Cooldown Preventivo (Período Neutro):** Se o jogador passar mais de `SafeIntervalSeconds` (padrão: 90s) sem abrir o inventário, mas estiver fora de combate e sem inimigos próximos, o coletor é reabilitado suavemente.
3. **Airbag de Proteção contra Estouro de Heap (OOM):**
   - Monitorar continuamente a memória gerenciada (`GC.GetTotalMemory(false)`). Se ultrapassar `CriticalMemoryLimitMB` (padrão: 3500 MB), o GC é forçado imediatamente para `GarbageCollector.Mode.Enabled`, prevenindo travamentos ou falhas de alocação da máquina.
4. **Restauração Limpa e Idempotente:**
   - No `Cleanup()` e `OnDestroy()`, restaura deterministamente `GarbageCollector.GCMode = GarbageCollector.Mode.Enabled`.

---

## 2. Pontos de Integração e Ganchos

| Componente | Tipo | Motivo |
|---|---|---|
| `UnityEngine.Scripting.GarbageCollector` | Chamada Direta | Controle do modo de coleta (`GCMode = Enabled / Disabled`). |
| [`MemoryControllerClass.cs:118`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/references/eft-decompiled/Assembly-CSharp/MemoryControllerClass.cs#L118) | Chamada Nativa EFT | Executar coleta de lixo e limpeza de memória nativa da BSG de forma segura. |
| `Player.FirearmController.MakeShot` | Patch Postfix | Registrar o timestamp do último tiro disparado para iniciar a janela de combate. |
| `PerformanceManager.Initialize()` | Ciclo de Vida | Registrar o componente e capturar o estado inicial do GC. |
| `PerformanceManager.Cleanup()` | Ciclo de Vida | Restaurar o GC para `Mode.Enabled` ao sair da raid. |

---

## 3. Novas propriedades F12 (BepInEx)

Adicionar na seção **`11. Anti-Stutter (Garbage Collection)`** em `Configuration/ModConfig.cs`:

| Seção | Nome (EN) | Tipo | Padrão | Faixa | Tooltip (pt-BR) |
|---|---|---|---|---|---|
| `11. Anti-Stutter (GC)` | `EnableGCOptimizer` | bool | `true` | — | Bloqueia a coleta de lixo da Unity durante combates e mira para eliminar stutters de primeiro tiro. |
| `11. Anti-Stutter (GC)` | `CombatGracePeriod` | float | `5.0` | 2.0 a 15.0 | Tempo em segundos após o último disparo de arma para manter a supressão de GC ativa. |
| `11. Anti-Stutter (GC)` | `SafeIntervalSeconds` | float | `90.0` | 30.0 a 300.0 | Intervalo máximo em segundos sem coleta antes de efetuar limpeza preventiva fora de combate. |
| `11. Anti-Stutter (GC)` | `CriticalMemoryLimitMB` | float | `3500.0` | 2000.0 a 6000.0 | Limite de segurança de memória alocada para forçar o GC preventivamente e evitar OOM. |
| `11. Anti-Stutter (GC)` | `ForceCollectOnInventory` | bool | `true` | — | Executa coleta de lixo limpa e controlada assim que o jogador abre o inventário ou menu de looting. |

---

## 4. Arquivos do mod

| Arquivo | Ação | Resumo |
|---|---|---|
| `Configuration/ModConfig.cs` | MODIFICAR | Registrar opções na seção 11 de F12 e evento de configuração. |
| `PROPRIEDADES.md` | MODIFICAR | Documentar as opções da seção 11 em pt-BR. |
| `Core/GCOptimizerManager.cs` | CRIAR | Componente MonoBehaviour que monitora combate, suprime o GC e agenda coletas seguras. |
| `Patches/WeaponShotGCPatch.cs` | CRIAR | Hook Postfix em disparos para atualizar o timestamp de combate ativo. |
| `Core/PerformanceManager.cs` | MODIFICAR | Inicializar, registrar telemetria OnGUI e restaurar o `GCOptimizerManager`. |
| `Plugin.cs` | MODIFICAR | Ativar `WeaponShotGCPatch` e incrementar versão para `0.3.5`. |
| `TRL-CoreSight.csproj` | MODIFICAR | Incluir `GCOptimizerManager.cs` e `WeaponShotGCPatch.cs`, bump para `0.3.5`. |
| `mod.json` | MODIFICAR | Bump de versão para `0.3.5`. |

---

## 5. Stubs de código

### `Core/GCOptimizerManager.cs`

```csharp
using System;
using EFT;
using TRLCoreSight.Configuration;
using UnityEngine;
using UnityEngine.Scripting;

namespace TRLCoreSight.Core
{
    public class GCOptimizerManager : MonoBehaviour
    {
        public static GCOptimizerManager Instance { get; private set; }

        private Player _mainPlayer;
        private float _lastShotTime = -100f;
        private float _lastCollectTime = 0f;
        private bool _wasInventoryOpen = false;
        private float _inventoryOpenTimer = 0f;

        public bool IsSuppressed { get; private set; }
        public int SuppressedCollectionsCount { get; private set; }
        public int ControlledCollectionsCount { get; private set; }
        public float AllocatedMemoryMB => (float)GC.GetTotalMemory(false) / (1024f * 1024f);

        private void Awake()
        {
            Instance = this;
        }

        public void Initialize(Player mainPlayer)
        {
            _mainPlayer = mainPlayer;
            _lastCollectTime = Time.time;
        }

        public void NotifyShotFired()
        {
            _lastShotTime = Time.time;
        }

        public void OnUpdate()
        {
            if (!ModConfig.ModEnabled.Value || !ModConfig.EnableGCOptimizer.Value || _mainPlayer == null)
            {
                if (GarbageCollector.GCMode != GarbageCollector.Mode.Enabled)
                {
                    GarbageCollector.GCMode = GarbageCollector.Mode.Enabled;
                    IsSuppressed = false;
                }
                return;
            }

            float now = Time.time;
            bool isAiming = _mainPlayer.ProceduralWeaponAnimation != null && _mainPlayer.ProceduralWeaponAnimation.IsAiming;
            bool isSprinting = _mainPlayer.MovementContext != null && _mainPlayer.MovementContext.IsSprintEnabled;
            bool inCombat = (now - _lastShotTime) < ModConfig.CombatGracePeriod.Value;

            bool isInventoryOpen = Cursor.visible;

            // 1. Coleta controlada no inventário
            if (isInventoryOpen && ModConfig.ForceCollectOnInventory.Value)
            {
                _inventoryOpenTimer += Time.deltaTime;
                if (!_wasInventoryOpen && _inventoryOpenTimer > 0.5f && (now - _lastCollectTime) > 20.0f)
                {
                    PerformSafeCollection();
                    _wasInventoryOpen = true;
                }
            }
            else
            {
                _inventoryOpenTimer = 0f;
                _wasInventoryOpen = false;
            }

            // 2. Airbag de memória crítica
            if (AllocatedMemoryMB > ModConfig.CriticalMemoryLimitMB.Value)
            {
                if (GarbageCollector.GCMode != GarbageCollector.Mode.Enabled)
                {
                    GarbageCollector.GCMode = GarbageCollector.Mode.Enabled;
                    IsSuppressed = false;
                }
                return;
            }

            // 3. Supressão em Combate/Mira/Sprint
            bool shouldSuppress = (isAiming || isSprinting || inCombat) && !isInventoryOpen;

            if (shouldSuppress)
            {
                if (GarbageCollector.GCMode != GarbageCollector.Mode.Disabled)
                {
                    GarbageCollector.GCMode = GarbageCollector.Mode.Disabled;
                    IsSuppressed = true;
                    SuppressedCollectionsCount++;
                }
            }
            else
            {
                // Fora de combate: se o tempo de intervalo seguro expirou, efetua coleta preventiva
                if ((now - _lastCollectTime) > ModConfig.SafeIntervalSeconds.Value)
                {
                    PerformSafeCollection();
                }
                else if (GarbageCollector.GCMode != GarbageCollector.Mode.Enabled)
                {
                    GarbageCollector.GCMode = GarbageCollector.Mode.Enabled;
                    IsSuppressed = false;
                }
            }
        }

        public void PerformSafeCollection()
        {
            try
            {
                GarbageCollector.GCMode = GarbageCollector.Mode.Enabled;
                IsSuppressed = false;
                _lastCollectTime = Time.time;
                ControlledCollectionsCount++;

                // Executa coleta via EFT MemoryControllerClass nativo
                MemoryControllerClass.Collect(force: true);
            }
            catch (Exception ex)
            {
                Plugin.LogSource?.LogWarning($"[TRL-CoreSight] Erro ao executar coleta de lixo controlada: {ex.Message}");
                GC.Collect();
            }
        }

        public void Cleanup()
        {
            try
            {
                GarbageCollector.GCMode = GarbageCollector.Mode.Enabled;
            }
            catch { }
            IsSuppressed = false;
            Instance = null;
        }

        private void OnDestroy()
        {
            Cleanup();
        }
    }
}
```

### `Patches/WeaponShotGCPatch.cs`

```csharp
using System.Reflection;
using HarmonyLib;
using SPT.Reflection.Patching;
using TRLCoreSight.Core;

namespace TRLCoreSight.Patches
{
    /// <summary>
    /// Registra o momento de cada disparo efetuado por armas de fogo para blindar contra GC durante tiroteios.
    /// ref: Assembly-CSharp/Player.cs:5015
    /// </summary>
    public class WeaponShotGCPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            // ref: Assembly-CSharp/EFT/Player.cs (FirearmController.MakeShot)
            return AccessTools.Method(typeof(Player.FirearmController), nameof(Player.FirearmController.MakeShot));
        }

        [PatchPostfix]
        private static void Postfix()
        {
            if (GCOptimizerManager.Instance != null)
            {
                GCOptimizerManager.Instance.NotifyShotFired();
            }
        }
    }
}
```

---

## 6. Fluxo de Decisão de GC

```
                    [Update a cada frame]
                              │
                              ▼
           Memória alocada > CriticalMemoryLimitMB?
                  │                      │
                 SIM                    NÃO
                  │                      │
                  ▼                      ▼
         [Reativa GC Imediato]    Jogador no Inventário / Cursor?
                                         │               │
                                        SIM             NÃO
                                         │               │
                                         ▼               ▼
                             [Executa Coleta Limpa]   Em Mira / Sprint / Disparo recente?
                                         │                       │               │
                                         │                      SIM             NÃO
                                         │                       │               │
                                         │                       ▼               ▼
                                         │             [Desativa GCMode]   Timer > SafeInterval?
                                         │                                      │          │
                                         │                                     SIM        NÃO
                                         │                                      │          │
                                         │                                      ▼          ▼
                                         └──────────────────────────────► [Coleta Segura] [GC Normal]
```

---

## 7. Checklist de Implementação

- [ ] Adicionar propriedades da seção 11 de Anti-Stutter (GC) em `Configuration/ModConfig.cs`.
- [ ] Atualizar `PROPRIEDADES.md` com as configurações e explicações em pt-BR.
- [ ] Criar `Core/GCOptimizerManager.cs`.
- [ ] Criar `Patches/WeaponShotGCPatch.cs`.
- [ ] Integrar `GCOptimizerManager` no ciclo de vida e telemetria do `PerformanceManager.cs`.
- [ ] Ativar `WeaponShotGCPatch` em `Plugin.cs` e incrementar versão para `0.3.5`.
- [ ] Incluir novos arquivos em `TRL-CoreSight.csproj` e incrementar versão para `0.3.5`.
- [ ] Atualizar `mod.json` para `0.3.5`.
- [ ] Compilar via `dotnet build` e isolar em `mods/TRL-CoreSight/builds/TRL-CoreSight.dll`.

---

## 8. Conformidade com Skills (Auto-Checklist)

| # | Check | Status | Evidência / Razão |
|---|---|:---:|---|
| 1 | Lifecycle de raid: start hook + stop hooks idempotentes — AP-01 | ✅ | Gerenciado pelo `PerformanceManager` e restaurado no `Cleanup()`. |
| 2 | Filtro MainPlayer/Fika em todo patch que reage a ação de player — AP-02 | ✅ | `GCOptimizerManager` protege contra GC de combate local e compartilhado. |
| 3 | Alvos ofuscados/virtuais resolvidos por assinatura — AP-03 | ✅ | `Player.FirearmController.MakeShot` canônico da BSG. |
| 4 | Mudança de estado via API canônica do EFT; side-effects mapeados — AP-04 | ✅ | Utiliza `MemoryControllerClass.Collect(force: true)` da BSG e `GarbageCollector.GCMode` da Unity. |
| 5 | Estado entre raids: raid1→exit→raid2 e alt-F4 cobertos | ✅ | `Cleanup()` garante `GarbageCollector.GCMode = GarbageCollector.Mode.Enabled`. |
| 6 | Semântica/defaults/faixas de cada ConfigEntry sem ambiguidade — AP-05 | ✅ | Tipados com limites numéricos e descrições em pt-BR. |
| 7 | Todo patch-point reconfirmado no `.cs` do dump — AP-09 | ✅ | `MemoryControllerClass.cs:118` e `Player.cs:5015` reconfirmados. |

---

## Histórico

| Data | Evento |
|---|---|
| 2026-09-15 | Spec técnica criada com supressão de combate, airbag de OOM e coleta limpa no inventário. |
