# 015 — Bloquear mount ativo em Stance 1/2/3 · Spec Técnica

**Mod:** stancesAndCameraPositionSPT4.0.11
**Spec funcional:** [015-bloquear-mount-ativo-stances-01-spec.md](015-bloquear-mount-ativo-stances-01-spec.md)
**Criado:** 2026-07-09

> Fonte primária: [references/eft-decompiled/Assembly-CSharp/](../../../../references/eft-decompiled/Assembly-CSharp/). Investigação por 2 sub-agents (Assembly EFT + fork `modded/` + Fika).

## 1. Estratégia

Dois mecanismos complementares, ambos **locais** (só o MainPlayer):

1. **Bloquear a ativação** — `Prefix` em `EFT.Player.TryMountWeapon()` ([Player.cs:26218](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L26218)), o **ponto único** por onde o input de mount de superfície passa (tecla `ECommand.WeaponMounting`). Retornar `false` quando o jogador local está em **Stance 1/2/3 sem ADS** impede o mount antes da detecção de ponto do componente externo `GClass2667`. O **bipé não passa por este método** (é `FirearmController.Class1270`/`BipodState`), então fica de fora automaticamente — atende à decisão "bipé é exceção".

2. **Desmontar ao entrar em Stance 1/2/3 (ou soltar ADS montado)** — um **tick por frame** no `Plugin.Update` (`StanceManager.TickActiveMountGuard`) que, se o MainPlayer está montado em **superfície** (`ProceduralWeaponAnimation.IsMountedState`, que **exclui** o bipé) e está em Stance 1/2/3 sem ADS, chama `MovementContext.StartExitingMountedState()` ([MovementContext.cs:2985](../../../../references/eft-decompiled/Assembly-CSharp/EFT/MovementContext.cs#L2985)) — o caminho de saída **vanilla**, que anima e **replica no Fika** (termina em `ExitMountedState` → `OnMounting(EMountingCommand.Exit)`). Um flag de idempotência evita re-disparar a cada frame.

**Alternativas descartadas:** (a) Prefix em `MovementContext.EnterMountedState()` — mais invasivo e **também chamado para observados** pelo `MountingPacket.Execute` do Fika ([MountingPacket.cs:58](../../../../references/fika-plugin/Fika.Core/Networking/Packets/Player/Common/SubPackets/MountingPacket.cs#L58)); bloquear ali arriscaria quebrar o mount de peers. Fica como rede-de-segurança **opcional** (ver Riscos), sempre com gate `IsYourPlayer`. (b) Mexer em `InMountedState` diretamente — dessincroniza o coop (não emite `OnMounting`).

## 2. Pontos de patch

| Alvo (Assembly) | Tipo | Motivo |
|---|---|---|
| [`EFT/Player.cs:26218`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L26218) `TryMountWeapon()` | **Prefix** | Bloquear a ativação do mount de superfície em Stance 1/2/3 sem ADS (retorna `false`). |
| [`EFT/MovementContext.cs:2985`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/MovementContext.cs#L2985) `StartExitingMountedState()` | **Chamado** (não patcheado) | Desmontar de forma grácil e replicada quando já montado e entra em Stance 1/2/3 / solta ADS. `public`, no-op se não montado. |

Leituras (sem patch): `ProceduralWeaponAnimation.IsMountedState` (mount de superfície — distinto de `IsBipodUsed`; ver [modded/StanceManager.cs:157](../../modded/StanceManager.cs#L157)), `ProceduralWeaponAnimation.IsAiming`, `StanceManager.CurrentStance`, `Player.IsYourPlayer`, `Player.MovementContext`.

## 3. Novas propriedades F12 (BepInEx)

| Seção | Nome (EN) | Tipo | Padrão | Faixa | Avançado | Tooltip (pt-BR) |
|---|---|---|---|---|---|---|
| `Weapon Mounting` | `Block Active Mount In Stance` | bool | `true` | — | Não | Impede apoiar a arma em superfícies (mount) enquanto estiver em Stance 1/2/3 sem mirar. Em Stance 0 ou mirando, o mount funciona normalmente. |

> **Nota (checklist):** reusar a **seção exata** que o item 011 já usa para o mount (evitar criar seção nova). Confirmar o nome no `code-mod` — o histórico do mod usa `Weapon Mounting`; alinhar com `PROPRIEDADES.md`.

## 4. Arquivos do mod

| Arquivo | Ação | Resumo |
|---|---|---|
| `modded/Patches/BlockActiveMountPatch.cs` | CRIAR | Prefix em `Player.TryMountWeapon` — bloqueia a ativação em Stance 1/2/3 sem ADS (gate `IsYourPlayer`). |
| `modded/StanceManager.cs` | MODIFICAR | Novo `TickActiveMountGuard()` — desmonta via `StartExitingMountedState()` quando montado (superfície) + Stance 1/2/3 sem ADS; flag de idempotência. |
| `modded/Plugin.cs` | MODIFICAR | `SafeEnable("BlockActiveMountPatch", …)` no bloco de mount (junto do 011, ~linha 330-334); chamar `StanceManager.TickActiveMountGuard()` no `Update`; bind da ConfigEntry `_BlockActiveMountInStance`. |
| `PROPRIEDADES.md` | MODIFICAR | Documentar a nova ConfigEntry. |

## 5. Stubs de código

```csharp
// modded/Patches/BlockActiveMountPatch.cs
using System;
using System.Reflection;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace CameraRotationMod.Patches
{
    /// <summary>
    /// Item 015: bloqueia a ATIVAÇÃO do mount de superfície (mount vanilla) quando o jogador LOCAL está em
    /// Stance 1/2/3 sem mirar. Prefix em Player.TryMountWeapon — ponto único de ativação (input WeaponMounting),
    /// antes da detecção de ponto do GClass2667. O bipé NÃO passa por aqui (Class1270/BipodState), logo fica de
    /// fora naturalmente (decisão "bipé é exceção").
    /// </summary>
    public class BlockActiveMountPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
            // ref: Assembly-CSharp/EFT/Player.cs:26218
            => AccessTools.Method(typeof(Player), "TryMountWeapon");

        [PatchPrefix]
        private static bool Prefix(Player __instance)
        {
            try
            {
                if (Plugin._BlockActiveMountInStance != null && !Plugin._BlockActiveMountInStance.Value)
                    return true;                                   // feature desligada no F12 → vanilla
                if (__instance == null || !__instance.IsYourPlayer) return true;   // AP-02: só o MainPlayer local

                var pwa = __instance.ProceduralWeaponAnimation;    // ref: Player.ProceduralWeaponAnimation
                bool isAiming = pwa != null && pwa.IsAiming;       // ref: ProceduralWeaponAnimation.IsAiming
                // Stance 1/2/3, sem ADS e sem prone. PA-01-01: em prone o mount é legítimo (o 011 também cede
                // ao vanilla em prone — PassiveMountDetectPatch.cs:56). Mesma família de guard do 011.
                if (StanceManager.CurrentStance != Stance.Default && !isAiming && !__instance.IsInPronePose)
                    return false;                                  // pula o TryMountWeapon original → não monta
            }
            catch (Exception ex) { Plugin.Logger.LogError($"[Mount015] BlockActiveMount {ex.Message}"); }
            return true;
        }
    }
}
```

```csharp
// modded/StanceManager.cs — novo método (chamado 1x/frame no Plugin.Update)

// Idempotência: StartExitingMountedState inicia uma transição que mantém IsMountedState=true por alguns
// frames; sem este flag, re-dispararíamos a saída a cada frame.
private static bool _mountGuardExiting;

/// <summary>
/// Item 015: desmonta o mount de SUPERFÍCIE quando o jogador local está em Stance 1/2/3 sem ADS.
/// Cobre "entrou em stance montado" e "soltou o ADS montado em stance". Usa o caminho vanilla de saída
/// (replica no Fika via OnMounting(Exit)). O bipé (IsBipodUsed) NÃO é afetado — usamos IsMountedState.
/// </summary>
public static void TickActiveMountGuard()
{
    try
    {
        if (Plugin._BlockActiveMountInStance != null && !Plugin._BlockActiveMountInStance.Value) return;

        var gw = GetCachedGameWorld();
        var player = gw?.MainPlayer;
        if (player == null) return;

        var pwa = player.ProceduralWeaponAnimation;
        // IsMountedState = mount de SUPERFÍCIE (bipé usa IsBipodUsed → excluído). ref: StanceManager.cs:157
        bool mountedSurface = pwa != null && pwa.IsMountedState;
        bool isAiming = pwa != null && pwa.IsAiming;

        // PA-01-01: prone é exceção (mount deitado é legítimo, igual ao 011).
        if (mountedSurface && CurrentStance != Stance.Default && !isAiming && !player.IsInPronePose)
        {
            if (!_mountGuardExiting)
            {
                _mountGuardExiting = true;
                // ref: Assembly-CSharp/EFT/MovementContext.cs:2985 — public, replica no Fika (OnMounting(Exit)).
                player.MovementContext.StartExitingMountedState();
            }
        }
        else
        {
            _mountGuardExiting = false;   // saiu do cenário → rearmado
        }
    }
    catch (Exception ex) { Plugin.Logger.LogError($"[StanceManager.TickActiveMountGuard] {ex}"); }
}
```

```csharp
// modded/Plugin.cs — dentro do Awake, no bloco de mount (junto do 011, ~linha 330-334):
SafeEnable("BlockActiveMountPatch", () => new Patches.BlockActiveMountPatch());   // item 015

// no Update(), junto dos outros ticks de StanceManager:
StanceManager.TickActiveMountGuard();                                             // item 015

// no bind de ConfigEntries (seção Weapon Mounting):
_BlockActiveMountInStance = Config.Bind("Weapon Mounting", "Block Active Mount In Stance", true,
    new ConfigDescription("Impede apoiar a arma em superfícies (mount) enquanto estiver em Stance 1/2/3 sem mirar. " +
                          "Em Stance 0 ou mirando, o mount funciona normalmente."));
```

## 6. Fluxo de dados

```
Bloqueio:
[A] jogador aciona a tecla dedicada de mount (input externo)
      → [B] Player.TryMountWeapon()  ── Prefix BlockActiveMountPatch ──►  Stance≠0 && !ADS && !prone && IsYourPlayer?
                                                                            ├─ sim → return false (não monta)
                                                                            └─ não → original → GClass2667 detecta → EnterMountedState

Desmontar:
[A] jogador já montado (Stance 0/ADS) troca p/ Stance 1/2/3  OU solta o ADS em Stance 1/2/3
      → [B] Plugin.Update → StanceManager.TickActiveMountGuard()
             → pwa.IsMountedState && CurrentStance≠0 && !IsAiming && !prone ?
                → [C] MovementContext.StartExitingMountedState()  (MovementContext.cs:2985)
                       → idle.StartExiting() → … → ExitMountedState() → OnMounting(Exit)  (replica no Fika)
```

Refs: input `ECommand.WeaponMounting` ([EGameKey.cs:120](../../../../references/eft-decompiled/Assembly-CSharp/EFT/InputSystem/EGameKey.cs#L120)); ativação [Player.cs:26218](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L26218); estado montado `IsMountedState` (mod usa em [StanceManager.cs:157](../../modded/StanceManager.cs#L157)); saída [MovementContext.cs:2985](../../../../references/eft-decompiled/Assembly-CSharp/EFT/MovementContext.cs#L2985)/[:2996](../../../../references/eft-decompiled/Assembly-CSharp/EFT/MovementContext.cs#L2996); replicação Fika [MountingPacket.cs](../../../../references/fika-plugin/Fika.Core/Networking/Packets/Player/Common/SubPackets/MountingPacket.cs).

## 7. Riscos e dependências

- **Patches existentes (`modded/`):** o item **011** (`PassiveMountDetectPatch`) usa a MESMA condição (`CurrentStance != Default && !IsAiming`) para suprimir o passivo — consistência garantida, sem conflito (o 011 hooka `FirearmController.method_11`, o 015 hooka `Player.TryMountWeapon`). O item **013** força Stance 0 em arma **stationary/turret** (`IsMountedState || IsBipodUsed`) — não conflita: turret já força Stance 0, então o guard do 015 nem dispara. O **012** (`StaminaController`) lê `IsMountedState` para o cenário "Active Mount" — inalterado.
- **Bipé:** garantido como exceção por construção — `TryMountWeapon` não é o caminho do bipé, e o desmontar usa `IsMountedState` (não `IsBipodUsed`). **Não** tocar em `Class1270`/`BipodState`/`SetBipod`/`OnBipodToggleEvent`.
- **Coop/Fika (AP-02):** bloqueio é local (gate `IsYourPlayer`); desmontar usa `StartExitingMountedState` (emite `OnMounting(Exit)` → replica). **Testar como CLIENTE**, não só host (solo=host mascara — `feedback_coop_multiplayer_sync`). Peers montam/desmontam via `MountingPacket` sem passar por `TryMountWeapon`.
- **`StartExitingMountedState` não completa sozinho?** Se o teste mostrar que a arma fica "meio-desmontando", trocar para `MovementContext.ExitMountedState()` ([:2996](../../../../references/eft-decompiled/Assembly-CSharp/EFT/MovementContext.cs#L2996)) — hard cut, também emite `OnMounting(Exit)`. Documentado como fallback.
- **Rede-de-segurança opcional:** se algum caminho montar sem passar por `TryMountWeapon` (improvável), adicionar Prefix em `MovementContext.EnterMountedState` **com gate `IsYourPlayer`** (nunca bloquear observados). Não incluído na 1ª entrega.
- **Ordem de init:** `BlockActiveMountPatch` via `SafeEnable` (isolado); o tick depende de `StanceManager.CurrentStance` já inicializado (garantido — roda no `Update`, pós-`Awake`).

## 8. Checklist de implementação

- [x] Criar `modded/Patches/BlockActiveMountPatch.cs` (Prefix `Player.TryMountWeapon`, gate `IsYourPlayer` + `_BlockActiveMountInStance` + `!IsInPronePose`).
- [x] Adicionar `StanceManager.TickActiveMountGuard()` + flag `_mountGuardExiting`.
- [x] `Plugin.cs`: `SafeEnable` do patch; chamada do tick no `Update`; bind `_BlockActiveMountInStance` na seção nova `Weapon Mount (Active)` (PA-01-02 — o 011 usa `Weapon Mount (Passive)`; criada a paralela `(Active)`).
- [x] Declarar o campo `public static ConfigEntry<bool> _BlockActiveMountInStance;` em `Plugin.cs`.
- [x] Atualizar `PROPRIEDADES.md` com a nova ConfigEntry.
- [x] Build 0 erros; instalado em `RealisticMobility/` (hash `cc33e8d1b113`).

## 9. Conformidade com skills (auto-checklist)

| # | Check | Status | Evidência / razão |
|---|---|---|---|
| 1 | Lifecycle de raid: start/stop idempotentes — AP-01 | ✅ | Sem estado de raid próprio; o tick lê `MainPlayer` por frame (null-safe) e o único flag (`_mountGuardExiting`) é auto-rearmado. Sem subscrição de evento a limpar. |
| 2 | Filtro MainPlayer/Fika — AP-02 | ✅ | Prefix: `!__instance.IsYourPlayer → return true` (stub §5). Tick: opera só em `gw.MainPlayer`. Desmontar via caminho que replica (`OnMounting`). §7. |
| 3 | Alvos ofuscados/virtuais por assinatura; overrides — AP-03 | ✅ | `TryMountWeapon` e `StartExitingMountedState` são métodos **não-virtuais** de `Player`/`MovementContext` ([Player.cs:26218](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L26218), [MovementContext.cs:2985](../../../../references/eft-decompiled/Assembly-CSharp/EFT/MovementContext.cs#L2985)); resolvidos por nome via `AccessTools.Method`. Sem overrides. |
| 4 | Mudança de estado via API canônica; side-effects — AP-04 | ✅ | Desmontar usa `StartExitingMountedState()` (API vanilla, emite rede + reseta animação/ergonomia); não mexe em `InMountedState` cru. §1, §7. |
| 5 | Estado entre raids: raid1→exit→raid2, alt-F4/morte | ✅ | Sem estático persistente além de `_mountGuardExiting` (bool auto-rearmado); nada a resetar entre raids. Tick é null-safe quando não há `MainPlayer`. |
| 6 | Semântica/defaults/faixas da ConfigEntry — AP-05 | ✅ | `Block Active Mount In Stance` bool default `true` (feature on); off = 100% vanilla. §3. |
| 7 | Re-invocação do método patcheado / reentry — AP-07 | ✅ | O Prefix não re-chama `TryMountWeapon`; o tick chama `StartExitingMountedState` (método distinto, não patcheado). Sem recursão. |
| 8 | Flags/caches validados contra contexto após troca — AP-08 | ✅ | `_mountGuardExiting` é reavaliado por frame contra o estado atual (`IsMountedState`/stance/ADS); troca de arma já desmonta via `HandsChangingEvent` vanilla ([MovementContext.cs:3001](../../../../references/eft-decompiled/Assembly-CSharp/EFT/MovementContext.cs#L3001)). |

## Histórico

| Data | Evento |
|---|---|
| 2026-07-09 | Spec técnica criada via `/create-technical-spec` (2 sub-agents: Assembly EFT + fork modded + Fika). |
| 2026-07-09 | Review 01 aplicado: PA-01-01 (guard `!IsInPronePose` no Prefix e no tick — prone é exceção); PA-01-05 (cosmético §6). PA-01-02 (seção F12) resolver no `/code-mod`; PA-01-03/04 viram checklist de validação in-game. 0 🔴. |
