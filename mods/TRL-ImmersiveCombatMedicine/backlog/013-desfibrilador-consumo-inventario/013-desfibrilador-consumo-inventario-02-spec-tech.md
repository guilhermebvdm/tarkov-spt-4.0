# 013 — Spec técnica: consumo do desfibrilador

**Mod:** TRL-ImmersiveCombatMedicine
**Spec funcional:** [013-desfibrilador-consumo-inventario-01-spec.md](./013-desfibrilador-consumo-inventario-01-spec.md)
**Criado:** 2026-07-26

## 1. Causa-raiz

O consumo vive em `FikaRevivePlayerPatch.ConsumeDefibrillator` — [FikaRevivePatch.cs:98-102](../../modded/Patches/Trauma/FikaRevivePatch.cs#L98-L102):

```csharp
var discardResult = InteractionsHandlerClass.Discard(defib, inventoryController);   // sem simulate
if (discardResult.Succeeded)
    _ = inventoryController.TryRunNetworkTransaction(discardResult);               // sem callback
```

É o padrão que o sistema de cura **abandonou** em CR-04/CR-05, com o diagnóstico registrado em [MedicalLogic.cs:496-504](../../modded/Patches/Medical/MedicalLogic.cs#L496-L504):

> *"O que quebrava tudo era o DESCARTE: `Discard(simulate:false)` destacava o item na hora, silenciosamente, e a `RemoveOperation` seguinte lançava em `Item.Parent` → host com espelho fantasma, client com slot morto, mão travada."*

### Por que o item pisca (mecanismo no Assembly)

`ItemView` combina quatro flags e liga o blink com o resultado — `EFT.UI.DragAndDrop/ItemView.cs:578,596`:

```csharp
ibindable_0 = GClass1641.Combine(IsBeingAdded, IsBeingRemoved, IsBeingExamined, IsBeingLoadedAmmo, (a,r,e,l) => a||r||e||l);
CompositeDisposable.BindState(ibindable_0, Animator.SetBlinkingState);
```

`ItemViewAnimation.SetBlinkingState` (`EFT.UI/ItemViewAnimation.cs:61-71`) roda um loop de alpha 1↔0 a cada 0,5 s **enquanto a flag estiver ligada**, e `ItemView.IsInteractive` (`:686`) exclui essas mesmas flags — daí "pisca **e** não pode ser usado".

`SlotView.OnRemoveFromSlot` / `GridView` ligam `IsBeingRemoved` em `CommandStatus.Begin` e só limpam em `Succeed`/`Failed` (`SlotView.cs:555-573`, `GridView.cs:394-406`). Uma operação que recebe `Begin` e nunca recebe desfecho deixa a flag ligada para sempre — que é exatamente o estado produzido acima.

Para contraste, o caminho vanilla correto emite os dois eventos em sequência (`GClass3017.RemoveItem`, `GClass3017.cs:31-32`): `RaiseEvents(controller, Begin)` **e** `RaiseEvents(controller, Succeed)`.

## 2. Segundo defeito: consumo antes da confirmação

O Prefix roda em `ReviveInteractable.RevivePlayer(bool success)` e hoje só checa `success` — [FikaRevivePatch.cs:64](../../modded/Patches/Trauma/FikaRevivePatch.cs#L64). Mas o corpo do Fika aborta em três condições (`references/fika-plugin/Fika.Core/Main/Components/ReviveInteractable.cs:221,229,231`):

```csharp
if (!success || !_localPlayer.HealthController.IsAlive) { ...; return; }
if (_localPlayer != null) { if (_observedPlayer != null) { ...revive... } }
```

Ou seja: reanimador morto no último instante, ou alvo já destruído, abortam o revive **depois** de o nosso Prefix ter cobrado o item. O guard do mod tem de espelhar as três condições.

## 3. Mudança

### 3.1 Reusar o descarte diferido

`MedicalLogic.DiscardItemNetworked` ([MedicalLogic.cs:561-574](../../modded/Patches/Medical/MedicalLogic.cs#L561-L574)) é hoje `private`. Promover a `public static` e chamá-lo do revive, em vez de duplicar a decisão "controller disponível → coroutine com retry; senão → tentativa única".

O que se ganha, por dentro:

| Etapa | Onde | Efeito |
|---|---|---|
| `Discard(..., simulate: true)` | [MedicalLogic.cs:608](../../modded/Patches/Medical/MedicalLogic.cs#L608) | valida sem destacar o item; a mutação real acontece dentro da operação de rede |
| `TryRunNetworkTransaction(result, callback)` | [MedicalLogic.cs:615-621](../../modded/Patches/Medical/MedicalLogic.cs#L615-L621) | o desfecho é observado (`DiscardWatch`), não presumido |
| guard `item.CurrentAddress == null` | [MedicalLogic.cs:601](../../modded/Patches/Medical/MedicalLogic.cs#L601) | torna o retry seguro contra double-discard, e nunca toca `item.Parent` (o getter lança) |
| até 4 tentativas + espera de callback | [BandAidController.cs:983-1013](../../modded/Patches/Medical/BandAidController.cs#L983-L1013) | cobre a validação assíncrona do Fika |
| coroutine amarrada ao `GameWorld` de nascimento | [BandAidController.cs:970,985](../../modded/Patches/Medical/BandAidController.cs#L970) | fim de raid aborta sem mutar item morto |
| falha final logada como erro | [BandAidController.cs:1014-1015](../../modded/Patches/Medical/BandAidController.cs#L1014-L1015) | satisfaz o AC de "não passa em silêncio" |
| dedup por `item.Id` | [BandAidController.cs:948](../../modded/Patches/Medical/BandAidController.cs#L948) | dois revives seguidos não colidem |

**Decisão registrada — a espera de mãos é no-op benigno aqui.** O passo 1 da coroutine aguarda `doctor.HandsController is Player.MedsController` (desenhado para a animação de cura). No revive o reanimador está em `CurrentManagedState.Plant(...)`, não em `MedsController`, então a condição já é falsa na entrada e o `while` não itera: sobra a folga de 0,2 s antes da 1ª tentativa. Nenhuma alteração na coroutine — reusar como está é preferível a criar uma variante por chamador.

### 3.2 Espelhar os guards de abort do Fika

No Prefix, antes de consumir: `success`, `_localPlayer != null`, `_localPlayer.HealthController.IsAlive`, `_observedPlayer != null` — este último lido por reflection, como `_localPlayer` já é.

## 4. Riscos e não-riscos

- **Não** mexe em `FikaReviveGetActionsPatch` (o gate de quem pode reviver) — fora de escopo.
- **Não** altera a coroutine nem o `StartDiscardAttempt`: só promove a visibilidade de um método e adiciona um chamador. Zero risco de regressão no caminho da cura, que é o validado em 2 PCs.
- Promover `private` → `public static` num assembly de mod único é mudança de superfície interna, sem impacto de wire format nem de config.
- Corner de item empilhável (`StackObjectsCount > 1`) **não** é tratado, igual ao resto do mod: o vanilla usaria `SplitToNowhere` nesse caso (`GClass3017.cs:7-35`). O desfibrilador (`5c052e6986f7746b207bc3c9`) não empilha, e tratar aqui criaria divergência com o caminho da cura. Registrado, não implementado.
- O consumo continua dentro de `try/catch` (CR-01-04): uma exceção aqui cancelaria o revive inteiro.

## 5. Verificação

Cenário **C1** do [roteiro happy-flow](../../docs/happy-flow-test-plan.md), nos dois papéis (reanimador host e reanimador client). Log esperado: `Descarte agendado (aguarda mãos livres)` seguido de `Descarte confirmado pelo pipeline (CR-04-02)`. Ausência da linha de confirmação, ou presença de `TODAS as tentativas falharam`, reprova.
