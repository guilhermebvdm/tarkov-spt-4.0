# 014 — Corrigir sync visual de stances no Fika · Spec Técnica

**Mod:** stancesAndCameraPositionSPT4.0.11
**Spec funcional:** [014-sync-stances-fika-01-spec.md](014-sync-stances-fika-01-spec.md)
**Criado:** 2026-06-22

> Diagnóstico por 3 sub-agents (Sessão 6): o local aplica o offset de stance em `HandsContainer.WeaponRootAnim` (braço+arma juntos); o remoto aplicava em `PlayerBones.Spine3` (só tronco). Networking (`StanceSyncPacket`/`FikaSyncManager`) está correto.

## 1. Estratégia

**Aplicar o offset de stance do jogador remoto no mesmo transform e da mesma forma que o local** — `ProceduralWeaponAnimation.HandsContainer.WeaponRootAnim`, aditivo sobre `weaponPosition`/`weapRotation` (que já contêm lean, troca de ombro e mira). O ponto de aplicação é o `Postfix` de `ProceduralWeaponAnimation.ApplyComplexRotation` (que **já roda para o observed player**, dentro do `ProcessEffectors`, **antes** de o Fika copiar `WeaponRootAnim` → `PlayerBones.Offset/DeltaRotation`): basta deixar de barrar o observed no gate `!IsYourPlayer` e, para ele, aplicar o offset usando o **stance sincronizado** + um **spring state por-player** guardado no `ObservedStanceAnimator`. O `Spine3`/`LateUpdate` do animator são removidos.

Por que coexiste com lean/shoulder de graça: o offset é multiplicado **por cima** de `weapRotation` (pose nativa já com lean/ombro/mira) — exatamente como no local; nada do vanilla é sobrescrito.

## 2. Pontos de patch / transforms

| Alvo | Arquivo | Papel |
|---|---|---|
| `ProceduralWeaponAnimation.ApplyComplexRotation(float)` | [ApplyComplexRotationPatch.cs:134](../../modded-beta/Patches/ApplyComplexRotationPatch.cs#L134) | Postfix; ponto de aplicação (local **e** observed) |
| `HandsContainer.WeaponRootAnim.SetPositionAndRotation` | [ApplyComplexRotationPatch.cs:280](../../modded-beta/Patches/ApplyComplexRotationPatch.cs#L280) | Transform da arma+braço (local) — replicar no observed |
| `ObservedPlayer` cópia `WeaponRootAnim`→`PlayerBones.Offset/DeltaRotation` → `ShiftWeaponRoot` | [ObservedPlayer.cs:1851-1853, 1876](../../../../references/fika-plugin/Fika.Core/Main/Players/ObservedPlayer.cs#L1851) | Confirma que o offset no `WeaponRootAnim` é renderizado no remoto |
| `FikaSyncManager` / `StanceSyncPacket` | [Networking/](../../modded-beta/Networking/) | Já enviam `ProfileId`+`Stance`+`IsAiming` por player (mantidos) |

## 3. Novas propriedades F12

Nenhuma. (Reusa os `Stance X Hands Pitch/Yaw/Roll` e os offsets de posição já existentes, via `StanceManager`.)

## 4. Arquivos do mod

| Arquivo | Ação | Resumo |
|---|---|---|
| `modded-beta/Networking/ObservedStanceAnimator.cs` | MODIFICAR | Vira **state-holder por player**: guarda stance/isAiming + spring (euler/pos/velocities). Remove `LateUpdate`/Spine3. Novo `ApplyTo(pwa, weaponPosition, weapRotation, dt)` que avança o spring e escreve o `WeaponRootAnim`. |
| `modded-beta/Patches/ApplyComplexRotationPatch.cs` | MODIFICAR | No gate, se `!IsYourPlayer`: buscar o `ObservedStanceAnimator` do player e chamar `ApplyTo(...)`, depois `return` (sem rodar kick/holdbreath). Expor `SpringLerpAngle`/`SpringLerp` (reuso). |
| `modded-beta/StanceManager.cs` | MODIFICAR | Overloads `GetTargetRotation(Stance, bool)` e `GetTargetPosition(Stance, bool)` (recebem o stance por parâmetro; os atuais delegam com `CurrentStance`). |

> _PA-01-03: a edição em `RaidLifecyclePatches` foi removida — o estado vive no component `ObservedStanceAnimator` (destruído no despawn), sem dict estático a limpar._

## 5. Stubs de código

```csharp
// StanceManager.cs — overloads parametrizados (os existentes passam a delegar)
public static Vector3 GetTargetRotation(bool isAiming) => GetTargetRotation(CurrentStance, isAiming);
public static Vector3 GetTargetRotation(Stance stance, bool isAiming)
{
    RebuildCachedStanceValues();
    if (isAiming && (Plugin._ResetOnADS?.Value ?? false)) return _cachedADSRotation;
    return stance switch
    {
        Stance.Stance1 => _cachedStance1Rotation,
        Stance.Stance2 => _cachedStance2Rotation,
        Stance.Stance3 => _cachedStance3Rotation,
        _ => Vector3.zero
    };
}
public static Vector3 GetTargetPosition(bool isAiming) => GetTargetPosition(CurrentStance, isAiming);
public static Vector3 GetTargetPosition(Stance stance, bool isAiming) { /* idem, _cachedStanceXPosition */ }
```

```csharp
// Networking/ObservedStanceAnimator.cs — state-holder por observed player (sem LateUpdate/Spine3)
using EFT;
using EFT.Animations;
using Fika.Core.Main.Players;
using UnityEngine;
using CameraRotationMod.Patches;

namespace CameraRotationMod.Networking
{
    public class ObservedStanceAnimator : MonoBehaviour
    {
        private ObservedPlayer _observedPlayer;
        private int _stance;
        private bool _isAiming;
        private Vector3 _euler, _pos, _rotVel, _posVel;

        public void Init(ObservedPlayer p) => _observedPlayer = p;
        public void SetStance(int stance, bool isAiming) { _stance = stance; _isAiming = isAiming; }

        // Chamado pelo ApplyComplexRotationPatch (Postfix) para o player observado, no MESMO ponto/forma do local.
        public void ApplyTo(ProceduralWeaponAnimation pwa, Vector3 weaponPosition, Quaternion weapRotation, float dt)
        {
            if (pwa?.HandsContainer?.WeaponRootAnim == null) return;

            bool inStance = _stance > 0 && !(_observedPlayer != null && _observedPlayer.IsInPronePose);
            Vector3 targetEuler = inStance ? StanceManager.GetTargetRotation((Stance)_stance, _isAiming) : Vector3.zero;
            Vector3 targetPos   = inStance ? StanceManager.GetTargetPosition((Stance)_stance, _isAiming) : Vector3.zero;

            float speedMult = Plugin._StanceTransitionSpeed?.Value ?? 1f;
            float stiffness = 150f * speedMult;
            float damping   = Plugin._StanceOvershootDamping?.Value ?? 12f;
            _euler = ApplyComplexRotationPatch.SpringLerpAngle(_euler, targetEuler, ref _rotVel, stiffness, damping, dt);
            _pos   = ApplyComplexRotationPatch.SpringLerp(_pos, targetPos, ref _posVel, stiffness, damping, dt);

            Vector3 oriented = weapRotation * _pos;
            pwa.HandsContainer.WeaponRootAnim.SetPositionAndRotation(
                weaponPosition + oriented, weapRotation * Quaternion.Euler(_euler));   // ref: ApplyComplexRotationPatch.cs:280
        }
    }
}
```

```csharp
// Patches/ApplyComplexRotationPatch.cs — desviar o observed para ApplyTo, antes do bloco MainPlayer
[PatchPostfix]
private static void Postfix(EFT.Animations.ProceduralWeaponAnimation __instance, float dt)
{
    var firearmController = (Player.FirearmController)_firearmControllerField.GetValue(__instance);
    if (firearmController == null) return;
    Player player = Traverse.Create(firearmController).Field<Player>("_player").Value;
    if (player == null) return;

    // Campos da pose nativa (já com lean/ombro/mira) — lidos para ambos os caminhos
    Vector3 weaponPosition = (Vector3)_weapTempPositionField.GetValue(__instance);
    Quaternion weapRotation = (Quaternion)_weapTempRotationField.GetValue(__instance);
    if (weapRotation.w == 0 && weapRotation.x == 0 && weapRotation.y == 0 && weapRotation.z == 0) return; // safeguard
    if (float.IsNaN(dt) || dt <= 0f || dt > 1f) return;

    // Observed (Fika): aplica o stance SINCRONIZADO no WeaponRootAnim, sem kick/holdbreath (só local).
    if (!player.IsYourPlayer)
    {
        var animator = player.gameObject.GetComponent<Networking.ObservedStanceAnimator>();
        animator?.ApplyTo(__instance, weaponPosition, weapRotation, dt);
        return;
    }

    // ... bloco MainPlayer atual (kick, hold-breath, stance via StanceManager, estado estático) inalterado ...
}

// tornar reusáveis:
public static Vector3 SpringLerpAngle(Vector3 cur, Vector3 target, ref Vector3 vel, float k, float d, float dt) { /* já existe */ }
public static Vector3 SpringLerp(Vector3 cur, Vector3 target, ref Vector3 vel, float k, float d, float dt) { /* já existe */ }
```

## 6. Fluxo de dados

```
[Local troca stance] → StanceManager.OnStanceChanged → FikaSyncManager.SendStance(stance, ads)
   → StanceSyncPacket{ProfileId, Stance, IsAiming} (ReliableOrdered)

[Remoto recebe] → FikaSyncManager.OnStanceSyncPacketReceived
   → acha ObservedPlayer por ProfileId → GetComponent<ObservedStanceAnimator> (cria se falta)
   → animator.SetStance(stance, ads)

[Render do observed, por frame] ObservedPlayer.ProcessEffectors (ObservedPlayer.cs:1851)
   └─ ProceduralWeaponAnimation.ApplyComplexRotation → **Postfix**:
        !IsYourPlayer → animator.ApplyTo(pwa, weaponPosition, weapRotation, dt)
          → spring → WeaponRootAnim.SetPositionAndRotation (offset por cima do lean/ombro/mira)
   └─ (segue) copia WeaponRootAnim → PlayerBones.Offset/DeltaRotation (1852) → ShiftWeaponRoot (1876) → renderiza
```

## 7. Riscos e dependências

- **🟡 Timing (a validar in-game):** assume-se que o `Postfix` de `ApplyComplexRotation` roda **antes** da cópia `WeaponRootAnim`→`PlayerBones.Offset` (ObservedPlayer.cs:1852). Se a cópia ocorrer antes, o offset não renderiza → alternativa: aplicar diretamente em `PlayerBones.Offset/DeltaRotation` ou via Postfix em `ProcessEffectors`. **Validar com 2 clientes.**
- **🟡 Coexistência lean/ombro:** depende de `weaponPosition`/`weapRotation` já conterem lean/ombro no momento do Postfix (são a pose nativa pós-efetores). Confirmar visualmente as combinações.
- **AP-02:** o caminho observed só roda para `!IsYourPlayer`; o caminho MainPlayer permanece intacto (kick/holdbreath nunca rodam no observed).
- **Prone:** em prone, `targetEuler/targetPos = 0` (cede ao vanilla), como hoje.
- **Lifecycle:** o `ObservedStanceAnimator` é um component no GameObject do observed player → destruído no despawn/morte/fim de raid (sem órfão). `SetStance` é idempotente.
- **Perf:** custo extra por observed player por frame = 1 spring + 1 SetPositionAndRotation (trivial).
- **Estado:** `_euler/_pos/velocities` são por-instância (component), não estáticos — múltiplos remotos isolados.

## 8. Checklist de implementação

- [ ] `StanceManager`: overloads `GetTargetRotation(Stance,bool)` / `GetTargetPosition(Stance,bool)`; os existentes delegam.
- [ ] `ApplyComplexRotationPatch`: tornar `SpringLerpAngle`/`SpringLerp` públicos; desviar `!IsYourPlayer` para `ObservedStanceAnimator.ApplyTo` antes do bloco MainPlayer.
- [ ] `ObservedStanceAnimator`: remover `LateUpdate`/Spine3; virar state-holder + `ApplyTo` (spring + WeaponRootAnim).
- [ ] `/compile-mod` 0 erros.

## 9. Conformidade com skills (auto-checklist)

| # | Check | Status | Evidência |
|---|---|---|---|
| 1 | Lifecycle de raid — AP-01 | ✅ | `ObservedStanceAnimator` é component do observed → destruído no despawn; sem estado estático persistente |
| 2 | MainPlayer/Fika — AP-02 | ✅ | caminho observed só `!IsYourPlayer`; kick/holdbreath só no local; cada remoto tem seu component (por `ProfileId`) |
| 3 | Alvos ofuscados/virtuais — AP-03 | ✅ N/A | `ApplyComplexRotation`/`HandsContainer.WeaponRootAnim`/`ObservedPlayer` são nomeados (não ofuscados); patch já existente |
| 4 | API canônica; side-effects — AP-04 | ✅ | aplica no mesmo `WeaponRootAnim` que o local; aditivo sobre a pose nativa (não sobrescreve lean/ombro/mira) |
| 5 | Estado entre raids | ✅ | spring por-component; reset implícito no despawn; sem dict estático persistente (limpeza defensiva no §4) |
| 6 | ConfigEntry — AP-05 | ✅ N/A | sem novas configs (reusa offsets de stance existentes) |
| 7 | Reentry-guard — AP-07 | ✅ N/A | Postfix não re-invoca o método; `ApplyTo` só escreve transform |
| 8 | Stale state — AP-08 | ✅ | stance vem do último pacote; spring re-avalia por frame; por-player isolado |

## Histórico

| Data | Evento |
|---|---|
| 2026-06-22 | Spec técnica criada via `/create-technical-spec` (diagnóstico de 3 sub-agents) |
