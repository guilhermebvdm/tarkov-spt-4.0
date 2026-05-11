# 002 — Fix 01 · F4 patch target + swap Stance 2 ↔ Stance 3

**Mod:** stancesAndCameraPositionSPT4.0.11
**Item raiz:** [002-ciclo-linear-hotkeys-snap-fogo-01-spec.md](002-ciclo-linear-hotkeys-snap-fogo-01-spec.md)
**Asbuild:** [002-ciclo-linear-hotkeys-snap-fogo-05-asbuild.md](002-ciclo-linear-hotkeys-snap-fogo-05-asbuild.md)
**Data:** 2026-05-10
**Disparado por:** Feedback do usuário pós-teste in-raid do item 002.

## Contexto

Após o `/code-mod` original do item 002 + a rodada `/apply-code-review` 01, o usuário testou as 5 features in-raid:

| Feature | Status reportado |
| --- | --- |
| F1 — Include Stance 0 in Cycle | Não testado ainda |
| F2 — Mouse Wheel Scroll Mode | ✓ Funcionando |
| F3 — Hotkeys dedicadas | ✓ Funcionando |
| F4 — Snap to Stance 0 on Fire | ❌ **Não funciona** |
| F5 — Start In Low Ready On Raid Begin | ✓ Funcionando |

Adicionalmente, o usuário observou inconsistência de nomenclatura: queria que **Stance 2 = Low Ready** e **Stance 3 = Custom** (oposto do que tinha ficado no item 002 original).

## Mudanças aplicadas

### 1. F4 patch target trocado (Fase A do plano)

**Bug raiz:** No item 002, o `SnapFireTriggerPatch` patcheava a virtual base aninhada em `Player.FirearmController` ([`Player.cs:3810`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L3810) — `OperationBase.SetTriggerPressed`). A premissa era que todas as operações concretas (DefaultWeaponOperation, ReloadOperation, etc.) chamariam `base.SetTriggerPressed(pressed)` no início do override, fazendo o Prefix patcheado disparar.

**Refutação por evidência:** Auditoria do Assembly mostrou que **apenas 1 de 14 overrides chama `base.SetTriggerPressed()` ([`Player.cs:3184`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L3184))**. Os outros 13 — incluindo o **DefaultWeaponOperation** em [`Player.cs:2712`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L2712) (o caminho mais comum de fogo: `FirearmController_0.IsTriggerPressed = pressed;` sem chamar base) — pulam a base. C# virtual dispatch executa o IL do override diretamente; Harmony patches só interceptam o método cujo IL foi reescrito. Portanto, na esmagadora maioria dos cenários reais, o Prefix nunca disparava → F4 silenciosamente off.

**Fix:** O patch target mudou para [`Player.cs:13668`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L13668) — o método de roteamento na **própria `Player.FirearmController`**:

```csharp
public virtual void SetTriggerPressed(bool pressed)
{
    CurrentOperation.SetTriggerPressed(pressed && method_53());
}
```

Este método é chamado pelo input pipeline ANTES da virtual dispatch para a operação concreta. Patchear aqui captura todos os caminhos de fire, independente da operation ativa.

Arquivos tocados:

| Arquivo | Mudança |
| --- | --- |
| [`Plugin.cs`](../../modded/Plugin.cs) | `ResolveFirearmOperationBase()` substituída por `ResolveFirearmControllerSetTrigger()` — simplificada (sem busca reflexiva de nested types nem `GetBaseDefinition` fallback). Property `OperationOriginalSetTrigger` substituída por `FirearmControllerSetTrigger`. `CurrentOperationGetter` removida (não precisa mais — staleness check vira `MainPlayer.HandsController == fc` direto). |
| [`Patches/SnapFireTriggerPatch.cs`](../../modded/Patches/SnapFireTriggerPatch.cs) | Reescrita: `Prefix(Player.FirearmController __instance, bool pressed)` tipado; remove cache de nested operation type (`_cachedOperationType`/`_cachedFcField`); `RaiseSyntheticTrigger(Player.FirearmController fc, bool pressed)` simplificado. Reentry guard `[ThreadStatic] _inSyntheticCall` preservado. Filtro CR-01-01 (MainPlayer) preservado. |
| [`StanceManager.cs`](../../modded/StanceManager.cs) | Signatures atualizadas: `TryInterceptTriggerDown(Player.FirearmController fc)`, `OnTriggerUpAfterIntercept(Player.FirearmController fc, MethodBase originalMethod)`. Campos renomeados: `_interceptOperationInstance` → `_interceptFc`, `_pendingResurrectInstance` → `_pendingResurrectFc`, `_pendingResetInstance` → `_pendingResetFc`. `IsOperationStillCurrent` → `IsFirearmControllerStillCurrent` (mais simples: compara `gw.MainPlayer.HandsController == fc`). |

A lógica de design (2-frame pulse, reentry guard, MainPlayer filter, anti-swap via `_interceptFc`, stale timeout, double-tap natural-pressed guard) **permanece igual** — apenas o nível de patch foi corrigido.

### 2. Swap Stance 2 ↔ Stance 3 (Fase B do plano)

Trocadas as identidades semânticas e os defaults das stances 2 e 3:

| Aspecto | Antes (item 002) | Depois (06-fix-01) |
| --- | --- | --- |
| Stance 2 nome | "Stance 2 - Custom" | "Stance 2 - Low Ready" |
| Stance 2 Pitch | 0° | **30°** (cano desce) |
| Stance 2 Yaw | -30° | 0° |
| Stance 2 Forward | 0 | **0.03** |
| Stance 2 Stamina | 2.0 | **1.0** |
| Stance 2 Speed | 100 | **90** |
| Stance 2 SnapOnFire | true | **false** |
| Stance 3 nome | "Stance 3 - Low Ready" | "Stance 3 - Custom" |
| Stance 3 Pitch | 30° | **0°** |
| Stance 3 Yaw | 0° | **-30°** (lateral) |
| Stance 3 Forward | 0.03 | **0** |
| Stance 3 Stamina | 1.0 | **2.0** |
| Stance 3 Speed | 90 | **100** |
| Stance 3 SnapOnFire | false | **true** |

**F5 target:** [`RaidLifecyclePatches.cs:29`](../../modded/Patches/RaidLifecyclePatches.cs#L29) — `Stance.Stance3` → `Stance.Stance2` (porque "Low Ready" agora é Stance 2).

**Linear scroll:** SEM mudança de código (`HandleLinearScroll` já usava `Stance.Stance2` como bottom do eixo e `Stance.Stance3` como off-axis). Após o swap, isso passa a fazer sentido semanticamente: Linear axis = Stance 1 (High Ready) ↔ Stance 0 ↔ Stance 2 (Low Ready), e Stance 3 (Custom) fica off-axis.

Arquivos tocados:

| Arquivo | Mudança |
| --- | --- |
| [`Plugin.cs`](../../modded/Plugin.cs) L57-60 | `Stance2Section`/`Stance3Section` constants swap |
| [`Plugin.cs`](../../modded/Plugin.cs) L43-46 | `_stanceDefaults` swap (stamina/speed/snap defaults) |
| [`Plugin.cs`](../../modded/Plugin.cs) L735-840 | Hand rotation defaults swap (Pitch/Yaw/Forward em Stance 2 e Stance 3) |
| [`Patches/RaidLifecyclePatches.cs`](../../modded/Patches/RaidLifecyclePatches.cs#L29) | F5 target → `Stance.Stance2` |
| [`PROPRIEDADES.md`](../../PROPRIEDADES.md) | Section headers swap; 12 default values swap nas tabelas; 2º aviso de breaking change |
| [`002-…-01-spec.md`](002-ciclo-linear-hotkeys-snap-fogo-01-spec.md) | Visão geral, ACs, F12 layout, hotkey tooltips, corner cases — todos com nomes swapped. Entrada de Histórico nova. |

### 3. F12 ordering — aceitar alfabético (Fase C)

Sem mudança de código. Após o swap, a ordem F12 fica naturalmente:

```text
Stance 0 - Vanilla
Stance 1 - High Ready
Stance 2 - Low Ready
Stance 3 - Custom
```

O usuário aceitou que "Low Ready" não fique acima de "High Ready" (BepInEx ConfigurationManager sorta seções alfabeticamente; alternativas como prefixo "02" foram avaliadas e descartadas como hack).

## Migração de `.cfg`

Breaking change adicional: usuários com `BepInEx/config/shwng.camerarotation.cfg` modificado nas seções `Stance 2 - Custom` ou `Stance 3 - Low Ready` (nomes do item 002 original) terão essas entries ficando órfãs após o primeiro boot pós-fix. As novas seções `Stance 2 - Low Ready` e `Stance 3 - Custom` serão criadas com os defaults swapped acima.

**Para preservar customizações antigas:** copiar manualmente as linhas das seções antigas para as novas no `.cfg` antes do primeiro boot.

## O que NÃO mudou

- F1 (Include Stance 0 in Cycle): código inalterado.
- F2 (Mouse Wheel Scroll Mode): código inalterado. Linear axis preservado.
- F3 (Hotkeys): código inalterado (defaults dos hotkeys ainda apontam para o slot `Stance.Stance3` = `O`, agora referindo a "Custom" em vez de "Low Ready" — usuário pode mudar via F12).
- F4 (Snap on Fire): mesmo design (2-frame pulse, reentry guard, etc.), apenas patch target corrigido + signatures simplificadas.
- F5 (Start In Low Ready): mesma feature, apenas alvo mudou de `Stance.Stance3` para `Stance.Stance2`.
- F1/F2/F3/F5 ACs da spec funcional: preservados; apenas rótulos atualizados.

## Como verificar

1. **F4 funciona após fix:** raid, Stance 1 (High Ready), pressionar e segurar gatilho ≥ 200ms — esperado: snap para Stance 0 + 1 tiro disparado no release+1 frame.
2. **F4 multiplayer (Fika):** stance local não muda quando outros atiram — guard `__instance != MainPlayer.HandsController` no Prefix preservado.
3. **F5 + swap:** raid começa em Stance 2 - Low Ready (Pitch 30° visível na arma).
4. **F12 layout:** seções na ordem Stance 0 → Stance 1 (High Ready) → Stance 2 (Low Ready) → Stance 3 (Custom).
5. **Stance 2 axis:** F12 mostra Pitch 30°, Yaw 0°.
6. **Stance 3 axis:** F12 mostra Pitch 0°, Yaw -30°.

## Histórico

| Data | Evento |
| --- | --- |
| 2026-05-10 | Fix 01 criado — F4 patch target corrigido + swap Stance 2 ↔ Stance 3 + ajustes em PROPRIEDADES e 01-spec. Disparado por feedback do usuário in-raid (F4 não funcionou no item 002 original; usuário também pediu swap dos rótulos). |
