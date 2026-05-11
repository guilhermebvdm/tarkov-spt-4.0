# As-Built — 001 Stamina e Velocidade por Postura

**Mod:** stancesAndCameraPositionSPT4.0.11  
**Data de entrega:** 2026-05-09  
**Relacionado a:** [spec](001-stamina-e-velocidade-01-spec.md) · [fix-01](001-stamina-e-velocidade-06-fix-01.md)

---

## Visão geral do que foi entregue

Sistema de stamina e velocidade por postura para o `MainPlayer`. Cada stance (0–3) tem um `StaminaMultiplier` (float) que controla drain ou recovery da `HandsStamina` em hipfire, além de um redutor de velocidade via `MovementContext.AddStateSpeedLimit`. Todos os efeitos são suspensos em ADS, opcionalmente em prone, e limpos ao fim da raid em qualquer caminho de saída.

O sistema passou por um redesign durante o desenvolvimento (fix-01): o modelo inicial `StaminaMode` (enum) + `StaminaIntensity` (float) foi substituído por `StaminaMultiplier` único porque Recovery dependia de `GetHandsRestorationFunc` que retorna 0 em hipfire. Ver [fix-01](001-stamina-e-velocidade-06-fix-01.md) para o raciocínio completo.

---

## Arquivos criados / modificados

| Arquivo | Tipo de mudança |
|---|---|
| `modded/StanceStaminaState.cs` | Novo (classe de estado estático compartilhado) |
| `modded/StanceConfig.cs` | Novo (holder de ConfigEntries por stance) |
| `modded/Patches/StanceStaminaRecoveryPatch.cs` | Novo (Harmony postfix) |
| `modded/StanceManager.cs` | Modificado — adicionados métodos de stamina/velocidade |
| `modded/Plugin.cs` | Modificado — bindings, defaults, subscriptions, registro de patches |
| `PROPRIEDADES.md` | Atualizado — 4 props por stance (total +16 no F12) |

---

## Arquitetura — componentes e responsabilidades

```
Plugin.Awake()
  └─ BindStance() × 4         → cria StanceConfig para cada stance (0–3)
  └─ SettingChanged            → MarkStaminaConfigDirty() ao mudar config em runtime
  └─ StanceStaminaRecoveryPatch.Enable()
  └─ GameWorldOnGameStartedPatch.Enable()
  └─ GameWorldOnDestroyPatch.Enable()

Plugin.Update() (todo frame)
  └─ StanceManager.TickStanceStamina()
  └─ StanceManager.EvaluateProneSuspensionTick()

StanceManager.CurrentStance setter
  └─ OnStanceChanged() → ApplyStaminaStance()

StanceStaminaRecoveryPatch (Harmony postfix — todo frame em que GetHandsRestorationFunc é chamado)
  └─ zera __result quando Multiplier ≠ 1.0 e não está em ADS
```

### `StanceStaminaState` — estado de leitura rápida

Classe estática que serve como cache de estado compartilhado entre o tick e o postfix Harmony, evitando lookups de config por frame.

```csharp
public static float Multiplier = 1f;          // <1.0 drain · 1.0 vanilla · >1.0 recovery
public static bool IsSuspendedByProne = false;
public static bool ShouldApplyStamina         // atalho: Multiplier ≠ 1.0 && !Prone
public static void Reset()                    // chamado em OnRaidStart e OnRaidEnd
```

`ShouldApplyStamina` usa `System.Math.Abs` (não `Mathf.Approximately`) para evitar dependência de `UnityEngine` neste arquivo de estado puro.

### `StanceConfig` — holder de ConfigEntries por stance

```csharp
ConfigEntry<float> StaminaMultiplier;         // range 0.0–3.0
ConfigEntry<bool>  ModifiesMovementSpeed;
ConfigEntry<int>   MovementSpeedMultiplier;   // range 50–100 (%)
ConfigEntry<bool>  ApplyWhenProne;
```

Indexado em `Plugin._stanceConfigs` pelo enum `Stance` (Default/Stance1/Stance2/Stance3).

### `StanceStaminaRecoveryPatch` — Harmony postfix

**Alvo:** `PlayerPhysicalClass.GetHandsRestorationFunc`  
**ref:** `Assembly-CSharp/PlayerPhysicalClass.cs:1022`  
**Prioridade:** `Priority.Low` (roda depois de outros mods de stamina)

Quando `Multiplier ≠ 1.0` e não está em ADS e não está em prone-suspenso, zera `__result` para que o vanilla não some um delta indesejado ao tick do mod. Guards aplicados em ordem:

1. `Singleton<GameWorld>.Instance?.MainPlayer == null` → return
2. `MainPlayer is HideoutPlayer && !_DebugApplyInHideout` → return
3. `__instance.Player_0 != gw.MainPlayer` → return (ignora bots)
4. `Math.Abs(mult - 1.0f) <= 1e-5f` → return (vanilla — não interferir)
5. `!IsSuspendedByProne && !IsAiming` → `__result = 0f`

### `StanceManager` — métodos de stamina/velocidade

#### `OnRaidStart()`

Chamado pelo patch `GameWorldOnGameStartedPatch` (postfix em `GameWorld.OnGameStarted`).

- Marca `_raidEnded = false`
- Cacheia `_cachedAimDrainRate` de `BackendConfigSettingsClass.Stamina.AimDrainRate` (constante imutável em runtime; fallback = `3f`)
- Chama `StanceStaminaState.Reset()` + `ApplyStaminaStance(Stance.Default)`

#### `OnRaidEnd()`

Chamado pelo patch `GameWorldOnDestroyPatch` (postfix em `GameWorld.OnDestroy`). Idempotente via guard `if (_raidEnded) return`.

- Remove speed limit via `mc.RemoveStateSpeedLimit(StanceSpeedLimitCause)`
- Chama `StanceStaminaState.Reset()`
- Chama `ResetState()` (limpa CurrentStance, tac sprint, caches de frame)

> **Nota:** `BaseLocalGame.Stop` não é patchável diretamente (open generic). `GameWorld.OnDestroy` cobre os 3 caminhos de saída (Left/Killed/MIA).

#### `ApplyStaminaStance(Stance stance)`

Chamado em `OnRaidStart`, `OnStanceChanged` e pelo tick (quando `_staminaConfigDirty`).

1. Guard `!IsActiveContext()` → return
2. Lookup `Plugin._stanceConfigs[stance]`
3. `RemoveStateSpeedLimit` + seta `_lastAppliedSpeedLimit = -1f`
4. `StanceStaminaState.Multiplier = cfg.StaminaMultiplier.Value`
5. Calcula `IsSuspendedByProne = player.IsInPronePose && !cfg.ApplyWhenProne.Value`
6. Se `ModifiesMovementSpeed && !IsSuspendedByProne`: calcula `fraction * mc.MaxSpeed` e chama `AddStateSpeedLimit`

#### `TickStanceStamina()`

Chamado por `Plugin.Update()` todo frame.

1. Se `_staminaConfigDirty`: re-aplica config (responde a mudança de F12 no mesmo frame)
2. Guards: `IsActiveContext`, `ShouldApplyStamina`, `!IsAiming`, `hands != null`, `hands.Multiplier > 0`, `!hands.ForceMode`
3. Fórmula: `delta = _cachedAimDrainRate × (Multiplier - 1.0f) × hands.Multiplier × Time.deltaTime`
4. Sanidade: `float.IsNaN`, `float.IsInfinity`, `Mathf.Abs(delta) < 0.0001f` → return
5. `target = Mathf.Clamp(prev + delta, 0f, (float)hands.TotalCapacity)`
6. Se `Mathf.Abs(target - prev) < 0.0001f` → return (evita notify desnecessário)
7. `hands.Current = target`
8. `NotifyHandsStaminaChanged(hands, prev)` — dispara eventos da HUD
9. Se drain e atingiu zero: `hands.HandleExpiration()` (replica comportamento vanilla)

#### `EvaluateProneSuspensionTick()`

Chamado por `Plugin.Update()` todo frame. Detecta entrada/saída de prone e:

- Se mudou `wasSuspended → isSuspended`: atualiza `IsSuspendedByProne`; em prone, remove speed limit
- Re-aplica speed limit defensivamente se `target` calculado divergiu do aplicado por mais de `0.001f` (cobre staleness de `MaxSpeed` que varia com skill Strength)

#### `NotifyHandsStaminaChanged(GClass774 hands, float prevValue)`

Dispara os três eventos do `GClass774` que a HUD escuta:

| Evento | Campo backing | Condição |
|---|---|---|
| `OnValueChanged` | `action_3` | sempre |
| `OnChanged` | `InvokeChangedAction()` (método público) | sempre |
| `OnThresholdPass` | `action_1` | só ao cruzar threshold de 15f |

Os backing fields `action_3` e `action_1` são resolvidos por lista de candidatos via `ResolveBackingFieldByCandidates` (nome público + nome decompilador) em campos `static readonly`. Falha de resolução é detectada no Awake e logada como `LogWarning` — drain continua funcional, apenas os eventos de HUD ficam silenciosos.

#### `IsActiveContext()`

Guard central. Retorna `true` somente se:
- `Singleton<GameWorld>.Instance != null`
- `gw.MainPlayer != null`
- Não é `HideoutPlayer` (a menos que `_DebugApplyInHideout = true`)

---

## Sistema de velocidade

**Causa registrada:** `Plugin.StanceSpeedLimitCause = (Player.ESpeedLimit)9001`

O valor `9001` está deliberadamente fora dos valores oficiais de `ESpeedLimit` (0–8) para não colidir com limites vanilla. Documentado no README.

**Fluxo:** `ApplyStaminaStance` chama `RemoveStateSpeedLimit` antes de qualquer `AddStateSpeedLimit`, garantindo que ao trocar de stance o limite antigo seja sempre retirado antes do novo ser aplicado.

**Re-aplicação defensiva em `EvaluateProneSuspensionTick`:** `mc.MaxSpeed` varia com a skill Strength em runtime. O tick re-aplica o limit se a diferença entre o target calculado e o último aplicado for > `0.001f`, sem chamar `OnCharacterControllerSpeedLimitChanged` à toa.

---

## Configuração exposta no F12

4 propriedades × 4 stances = **16 entradas** adicionadas ao backlog 001, organizadas nas seções `Stance 0 - Vanilla`, `Stance 1 - Ready Up`, `Stance 2 - Ready Down`, `Stance 3 - Custom`.

| ConfigEntry | Tipo | Faixa | Order |
|---|---|---|---|
| `Stance N Stamina Multiplier` | float | 0.0 – 3.0 | 5 |
| `Stance N Modifies Movement Speed` | bool | — | 3 |
| `Stance N Movement Speed Multiplier` | int | 50 – 100 | 2 (Avançado) |
| `Stance N Apply When Prone` | bool | — | 1 (Avançado) |

Mudança em qualquer dessas entradas via F12 dispara `OnStanceConfigChanged` → `MarkStaminaConfigDirty()`. O tick re-aplica no próximo frame.

### Defaults implementados

| Stance | StaminaMultiplier | ModSpeed | SpeedMult | ApplyProne |
|---|---|---|---|---|
| 0 - Vanilla | `0.5` (drain suave) | `true` | `90%` | `false` |
| 1 - Ready Up | `1.5` (recovery suave) | `true` | `95%` | `false` |
| 2 - Ready Down | `2.0` (recovery pleno) | `true` | `100%` | `false` |
| 3 - Custom | `1.0` (vanilla) | `true` | `90%` | `false` |

---

## Lifecycle de raid — diagrama

```
Boot (Awake)
  └─ patches registrados, configs bound, subscriptions ativas

Início de raid (GameWorld.OnGameStarted postfix)
  └─ OnRaidStart(): cacheia AimDrainRate, Reset(), ApplyStaminaStance(Default)

Em raid (Update, todo frame)
  └─ TickStanceStamina()          ← drain/recovery hipfire
  └─ EvaluateProneSuspensionTick() ← prone detection + speed limit refresh

Troca de stance (CurrentStance setter)
  └─ OnStanceChanged() → ApplyStaminaStance(novaStance)
     └─ atualiza Multiplier, IsSuspendedByProne, speed limit

Mudança de config no F12
  └─ MarkStaminaConfigDirty()
     └─ TickStanceStamina() re-aplica no próximo frame

Fim de raid (GameWorld.OnDestroy postfix — cobre Left/Killed/MIA)
  └─ OnRaidEnd(): remove speed limit, Reset(), ResetState()
     └─ idempotente: guard _raidEnded
```

---

## Referências EFT

| Símbolo | Arquivo | Linha | Uso |
|---|---|---|---|
| `PlayerPhysicalClass.GetHandsRestorationFunc` | `PlayerPhysicalClass.cs` | 1022 | alvo do postfix |
| `Float_7[(int)Epose_0]` | `PlayerPhysicalClass.cs` | 1027 | retorna 0 em hipfire (causa raiz do bug de Recovery) |
| `BackendConfigSettingsClass.Stamina.AimDrainRate` | — | — | taxa base de drain (default 3f/s) |
| `GClass774` | — | — | tipo de `HandsStamina` |
| `GClass774.TotalCapacity` | — | — | `GClass848<float>`, castável para `float` |
| `GClass774.Multiplier` (= `Float_1`) | — | — | multiplicador da skill Endurance |
| `GClass774.ForceMode` | — | — | quando `true`, EFT pula `Consume()` — respeitado pelo tick |
| `GClass774.HandleExpiration()` | — | — | dispara evento `OnExpired` ao atingir zero |
| `GClass774.OnValueChanged` (backing `action_3`) | — | — | sinal de atualização para a HUD |
| `GClass774.OnThresholdPass` (backing `action_1`) | — | — | som "tired" ao cruzar 15f |
| `MovementContext.AddStateSpeedLimit` | — | — | aplica redutor de velocidade |
| `MovementContext.RemoveStateSpeedLimit` | — | — | remove redutor ao trocar de stance ou fim de raid |
| `GameWorld.OnGameStarted` | `GameWorld.cs` | 2584 | hook de início de raid |
| `GameWorld.OnDestroy` | `GameWorld.cs` | 2111 | hook de fim de raid |

---

## Decisões de implementação notáveis

**`_cachedAimDrainRate` em vez de lookup por frame**  
`BackendConfigSettingsClass` é um singleton imutável em runtime. Cachear `AimDrainRate` em `OnRaidStart` evita lookup via Singleton todo frame no tick.

**`(float)hands.TotalCapacity` — cast explícito**  
`GClass774.TotalCapacity` é `GClass848<float>`. O cast para `float` é válido e consistente com o próprio código EFT (`NormalValue = Current / (float)TotalCapacity`).

**Idem para `hands.Multiplier <= 0f`**  
Guard explícito antes da fórmula para não dividir por zero em cenários de exaustão extrema.

**`_raidEnded = true` como default inicial**  
O campo começa em `true` (não há raid ativa no Awake). Isso defende contra `OnRaidEnd` disparando antes de qualquer `OnRaidStart`, ex.: BepInEx reload no hideout.

**`StanceSpeedLimitCause = (Player.ESpeedLimit)9001`**  
Valor arbitrário fora dos 0–8 oficiais. Sem colisão conhecida. Se um mod externo usar o mesmo valor, os speed limits se sobrescreveriam mutuamente — documentado como incompatibilidade potencial.

**Reflection cacheada para events de `GClass774`**  
`action_3` e `action_1` são backing fields privados resolvidos por lista de candidatos (`nome público → nome ILSpy`) em campos `static readonly`. BSG pode renomear esses campos — nesse caso, drain continua funcional mas a HUD para de atualizar. O Awake detecta e loga um `LogWarning` com os campos não resolvidos.

---

## Limitações conhecidas e fora de escopo

- **Aumento de velocidade (> 100%):** `AddStateSpeedLimit` só reduz — o sistema de `min` do EFT impede aceleração.
- **Bots e network players:** apenas `MainPlayer` afetado. Bots passam pelo guard `Player_0 != gw.MainPlayer`.
- **Fika (multiplayer):** não testado. Comportamento em multiplayer é "best effort".
- **Persistência entre raids:** cada raid começa na `Stance.Default` com `Multiplier = 1f` (vanilla). Preferência de stance do jogador não é persistida.
- **`BaseLocalGame.Stop` não patchado:** open generic impede patch direto. `GameWorld.OnDestroy` cobre todos os caminhos de saída na prática.
- **Composição com bipod:** `BipodAimDrainRateMultiplier` é aplicado antes do nosso postfix — a multiplicação é `vanilla × mod`, o que é o comportamento correto (não testado formalmente).
