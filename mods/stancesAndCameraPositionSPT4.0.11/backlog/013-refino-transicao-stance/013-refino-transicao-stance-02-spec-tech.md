# 013 — Refinamentos de transição de stance · Spec Técnica

**Mod:** stancesAndCameraPositionSPT4.0.11
**Spec funcional:** [013-refino-transicao-stance-01-spec.md](013-refino-transicao-stance-01-spec.md)
**Criado:** 2026-06-21

> Refs verificadas via 2 sub-agents de pesquisa (stationary weapon no Assembly + sprint/stance no mod).

## 1. Estratégia

Três ajustes pequenos, todos no jogador local, **sem novas configs F12**:

1. **Stationary → Mount Active:** `StaminaController` detecta `MovementContext.IsStationaryWeaponInHands` e resolve `ActiveStance0` (reusa o multiplicador Active Mount). Detecção **contínua** (sem flag) — sai limpo ao largar a arma.
2. **Forçar Stance 0 ao entrar:** o `StanceManager.Update` **já** força Stance 0 em `isNativeMounting || isInProne` ([StanceManager.cs:173-184](../../modded/StanceManager.cs)); basta **incluir `isStationary`** na condição. O `SetStance(Default)` só dispara se `CurrentStance != Default` (evento único, no-op se já em 0).
3. **Sprint sem flash da Stance 0:** o flash é o **spring** ([ApplyComplexRotationPatch.cs:264](../../modded/Patches/ApplyComplexRotationPatch.cs)) animando os offsets `Stance1→0` quando o sprint força `SetStance(Default)` ([StanceManager.cs:199](../../modded/StanceManager.cs)). Fix: ao forçar Default no sprint, **snap instantâneo** dos offsets para neutro (`SnapToNeutral()`), pulando a animação — a corrida assume a pose nativa sem "passar pela Stance 0". Ao sair do sprint, o restore atual (`SetStance(_preSprintStance)`) re-anima suave.

Não é preciso cercar stance/ADS/breath em stationary — o jogo bloqueia nativamente ([Player.cs:32113](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L32113)).

## 2. Pontos de patch / referências

| Alvo | Arquivo:linha | Uso |
|---|---|---|
| `MovementContext.IsStationaryWeaponInHands` (bool) | [MovementContext.cs:1446](../../../../references/eft-decompiled/Assembly-CSharp/EFT/MovementContext.cs#L1446) | Detecção de arma montada (ajustes 1 e 2) |
| `StanceManager.Update` força-Default | [StanceManager.cs:154-184](../../modded/StanceManager.cs) | Incluir `isStationary` na condição (ajuste 2) |
| `StanceManager` sprint force-zero | [StanceManager.cs:191-217](../../modded/StanceManager.cs) | Snap ao forçar Default no sprint (ajuste 3) |
| `ApplyComplexRotationPatch` spring + `CurrentEuler`/`CurrentPosition`/`_rotVelocity`/`_posVelocity` | [ApplyComplexRotationPatch.cs:259-267](../../modded/Patches/ApplyComplexRotationPatch.cs) | Novo `SnapToNeutral()` (ajuste 3) |
| `StaminaController.Resolve`/`Tick` | [StaminaController.cs](../../modded/StaminaController.cs) | Mapear stationary → ActiveStance0 (ajuste 1) |

Nenhum **patch Harmony novo** — só edição de código do mod (a detecção de stationary é leitura de propriedade pública, não-virtual; sem AP-03).

## 3. Novas propriedades F12

Nenhuma.

## 4. Arquivos do mod

| Arquivo | Ação | Resumo |
|---|---|---|
| `modded/StaminaController.cs` | MODIFICAR | `Tick` permite stationary no gate; `Resolve` → `ActiveStance0` quando `IsStationaryWeaponInHands`. |
| `modded/StanceManager.cs` | MODIFICAR | `Update`: `isStationary` na condição de força-Default (ajuste 2); `SnapToNeutral()` após o `SetStance(Default)` do sprint (ajuste 3). |
| `modded/Patches/ApplyComplexRotationPatch.cs` | MODIFICAR | Novo `public static void SnapToNeutral()` (zera offsets + velocidades do spring). |

## 5. Stubs de código

```csharp
// StaminaController.cs — Tick (gate permite stationary)
Player p = Singleton<GameWorld>.Instance?.MainPlayer;
if (p == null) { SetScenario(StaminaScenario.Inactive); ControllingHands = false; return; }
bool stationary = p.MovementContext != null && p.MovementContext.IsStationaryWeaponInHands;  // ref: MovementContext.cs:1446
GClass774 hands = p.Physical?.HandsStamina;
if (hands == null || (!stationary && !(p.HandsController is Player.FirearmController)))
{ SetScenario(StaminaScenario.Inactive); ControllingHands = false; return; }
```

```csharp
// StaminaController.cs — Resolve (stationary → Mount Active, item 013)
private static StaminaScenario Resolve(Player p)
{
    if (p == null) return StaminaScenario.Inactive;
    // Arma montada do cenário (stationary) = Mount Active. Sem ADS/stance reais (bloqueio nativo do EFT).
    if (p.MovementContext != null && p.MovementContext.IsStationaryWeaponInHands)
        return StaminaScenario.ActiveStance0;
    EFT.Animations.ProceduralWeaponAnimation pwa = p.ProceduralWeaponAnimation;
    // ... resto inalterado (Active mount > Passive > Prone > Stand) ...
}
```

```csharp
// StanceManager.cs — Update: detectar stationary (ajuste 2)
bool isNativeMounting = false, isAiming = false, isInProne = false, isStationary = false;
if (gameWorld?.MainPlayer != null)
{
    var pwa = gameWorld.MainPlayer.ProceduralWeaponAnimation;
    if (pwa != null) { isNativeMounting = pwa.IsMountedState || pwa.IsBipodUsed; isAiming = pwa.IsAiming; }
    isInProne = gameWorld.MainPlayer.IsInPronePose;
    var mc = gameWorld.MainPlayer.MovementContext;
    isStationary = mc != null && mc.IsStationaryWeaponInHands;   // ref: MovementContext.cs:1446 (item 013)
}
...
// força Stance 0 ao montar / deitar / ENTRAR EM ARMA MONTADA (item 013)
if (isNativeMounting || isInProne || isStationary)
{
    if (_isActionStanceActive) EndActionStance(forceCancel: true);
    if (CurrentStance != Stance.Default) SetStance(Stance.Default);
    return;
}
```

```csharp
// StanceManager.cs — sprint force-zero: snap instantâneo (ajuste 3, elimina o flash)
if (!_isTacSprintActive && !CanDoTacSprint(gameWorld.MainPlayer))
{
    if (!_wasSprintingForceZero)
    {
        _wasSprintingForceZero = true;
        if (CurrentStance != Stance.Default)
        {
            _preSprintStance = CurrentStance;
            SetStance(Stance.Default);
            Patches.ApplyComplexRotationPatch.SnapToNeutral();   // item 013: pula o spring 1→0 (sem flash)
        }
        else _preSprintStance = Stance.Default;
    }
}
```

```csharp
// ApplyComplexRotationPatch.cs — snap dos offsets para neutro (item 013)
public static void SnapToNeutral()
{
    CurrentEuler = Vector3.zero;
    CurrentPosition = Vector3.zero;
    _rotVelocity = Vector3.zero;
    _posVelocity = Vector3.zero;
    CurrentRotation = Quaternion.identity;
}
```

## 6. Fluxo de dados

```
ARMA MONTADA (stationary):
  MovementContext.IsStationaryWeaponInHands == true
   ├─ StaminaController.Resolve → ActiveStance0 (Mount Active)            [ajuste 1]
   └─ StanceManager.Update → (isStationary) → SetStance(Default) 1×       [ajuste 2]
  ao largar: IsStationaryWeaponInHands == false → estado/stance normalizam (sem flag preso)

SPRINT (sem TacSprint) a partir de Stance 1/2/3:
  IsSprintEnabled → SetStance(Default) + ApplyComplexRotationPatch.SnapToNeutral()  [ajuste 3]
   └─ offsets vão a 0 instantâneo (sem spring) → pose de sprint nativa, sem "flash"
  parar sprint → SetStance(_preSprintStance) → spring re-anima 0→stance (suave)
```

## 7. Riscos e dependências

- **🟡 Ajuste 3 é visual (validar in-game):** o `SnapToNeutral` elimina a animação do spring; se ainda houver um salto perceptível, calibrar (ex.: snap só da rotação, manter posição). Com TacSprint ativo, o caminho NÃO é tocado (gate `!_isTacSprintActive && !CanDoTacSprint`) — TacSprint preservado.
- **Armas grandes no sprint:** ao snap para neutro (offsets 0), a arma usa a pose de sprint nativa — sem clipping (o risco apontado seria manter offsets, o que NÃO fazemos).
- **AP-02/Fika:** `StanceManager.Update` e `StaminaController.Tick` já operam só sobre `MainPlayer`; `IsStationaryWeaponInHands` é lido do MainPlayer. Peers intactos.
- **Lifecycle:** stationary é **detecção contínua** (sem flag persistente) → sair da arma/raid limpa naturalmente; o `StaminaController.Reset` (item 012) cobre o fim de raid.
- **Stationary + HandsController:** confirmado que continua `FirearmController` (sub-agent) — o gate do Tick permite stationary explicitamente, então não depende disso.

## 8. Checklist de implementação

- [ ] `StaminaController`: gate do `Tick` permite stationary; `Resolve` mapeia stationary → `ActiveStance0`.
- [ ] `StanceManager.Update`: `isStationary` + incluir na condição de força-Default.
- [ ] `ApplyComplexRotationPatch`: `SnapToNeutral()`.
- [ ] `StanceManager` sprint: chamar `SnapToNeutral()` após o `SetStance(Default)` do sprint.
- [ ] `/compile-mod` 0 erros.

## 9. Conformidade com skills (auto-checklist)

| # | Check | Status | Evidência |
|---|---|---|---|
| 1 | Lifecycle de raid — AP-01 | ✅ | Detecção contínua de stationary (sem flag); `StaminaController.Reset` (item 012) cobre fim de raid |
| 2 | MainPlayer/Fika — AP-02 | ✅ | `StanceManager.Update` (GetCachedGameWorld().MainPlayer) e `StaminaController.Tick` (Singleton MainPlayer) — só local |
| 3 | Alvos ofuscados/virtuais — AP-03 | ✅ N/A | Sem patch Harmony novo; `IsStationaryWeaponInHands` é propriedade pública não-virtual |
| 4 | Mudança de estado via API canônica — AP-04 | ✅ | Stance forçada via `SetStance`/`ApplyUserStance` (aplica offsets); snap reusa os campos do spring existente |
| 5 | Estado entre raids | ✅ | Sem estado estático novo; `_wasSprintingForceZero`/`_preSprintStance` já resetam no fluxo de sprint |
| 6 | ConfigEntry — AP-05 | ✅ N/A | Nenhuma config nova |
| 7 | Reentry / recursão — AP-07 | ✅ N/A | Sem re-invocação de método patcheado |
| 8 | Stale state após troca de contexto — AP-08 | ✅ | Stationary re-amostrado por frame; sprint snap não deixa offsets presos (restore re-anima) |

## Histórico

| Data | Evento |
|---|---|
| 2026-06-21 | Spec técnica criada via `/create-technical-spec` (refs via 2 sub-agents) |
