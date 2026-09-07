# 003 — Fix rejeição de swap de magazine in-place no Headless (GClass1561) · Spec Técnica

**Mod:** FIKA
**Spec funcional:** [003-magazine-swap-inplace-fix-01-spec.md](003-magazine-swap-inplace-fix-01-spec.md)
**Criado:** 2026-09-06
**Revisado:** 2026-09-06 (aplica resoluções de [003-magazine-swap-inplace-fix-03-spec-tech-review-01.md](003-magazine-swap-inplace-fix-03-spec-tech-review-01.md) — PA-01-01 a PA-01-05)

> Fonte primária de verdade para qualquer assinatura, fórmula ou ponto de patch: [references/eft-decompiled/Assembly-CSharp/](../../../../references/eft-decompiled/Assembly-CSharp/). Toda referência ao código do EFT deve citar `arquivo.cs:linha`. Wiki SPT e fontes externas só como complemento.

**Memória consultada:** snapshot de 2026-09-03 (Sessão 2) de `mods/FIKA/memory/sessions.md` · pendências que afetam: nenhuma.
**Docs técnicos lidos (gatilho disparado):** `spt-antipatterns.md` (sempre — AP-03 é o núcleo do diagnóstico) · `spt4-vs-spt41-gclass-deobfuscation.md` (spec cita `GClass1561`/`GEventArgs17`/`GClass3380`/`GClass3475`/`GStruct155` — aliases resolvidos abaixo). `fika-packet-desync-prevention-plan.md` não disparado: este item não declara nenhum `INetSerializable` novo nem envia pacote próprio.

## 0. Aliases 4.1 (deofuscação — rótulo, não pinado — AP-09)

| Nome 4.0 | Alias 4.1 (`consolidated-mappings.txt`) |
|---|---|
| `GEventArgs17` | `EFT.InventoryLogic.InOutHandsProcessEventArgs` |
| `GEventArgs1` | `EFT.InventoryLogic.ItemEventArgs` |
| `GClass1561` | `EFT.InventoryLogic.PlayerIsBusyError` |
| `GClass3380` | `EFT.InventoryLogic.ItemExtensions` |
| `GClass3475` | `EFT.InventoryLogic.Operations.AbstractAsyncOperation` |
| `GStruct155` | `Diz.LanguageExtensions.Option` |

## 1. Estratégia

### 1.1 Causa raiz confirmada (evidência lida diretamente nesta sessão)

O erro `GClass1561` ("item is currently being modified") vem de `TraderControllerClass.CheckItemAction` — declarado `public virtual GStruct155 CheckItemAction(Item item, ItemAddress location)` em [`TraderControllerClass.cs:1098`](../../../../references/eft-decompiled/Assembly-CSharp/TraderControllerClass.cs#L1098). A checagem relevante (colisão com um evento pendente `GEventArgs17`/`inOutHandsProcess`) está duplicada em três lugares com comportamento distinto:

1. **Base** — [`TraderControllerClass.cs:1163-1174`](../../../../references/eft-decompiled/Assembly-CSharp/TraderControllerClass.cs#L1163): checagem original.
2. **`Player.PlayerInventoryController.CheckItemAction`** ([`Player.cs:1432-1439`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L1432)) — override que faz uma checagem extra (`IsInventoryBlocked()`) e então **chama `base.CheckItemAction(item, location)`** (linha 1438), ou seja, cai na checagem original.
3. **`ObservedInventoryController.CheckItemAction`** ([`ObservedInventoryController.cs:69-206`](../../../../mods/FIKA/modded/Fika-Plugin/Fika.Core/Main/ObservedClasses/ObservedInventoryController.cs#L69)) — override que **reimplementa a checagem inteira sem nunca chamar `base.CheckItemAction()`** (confirmado: `diff` contra `mods/FIKA/original` mostra o arquivo idêntico — não foi alterado por nós, é comportamento upstream do Fika).

O jogador local no FIKA usa `ClientInventoryController : BaseInventoryController` ([`ClientInventoryController.cs:22`](../../../../mods/FIKA/modded/Fika-Plugin/Fika.Core/Main/ClientClasses/ClientInventoryController.cs#L22)), que **não sobrescreve `CheckItemAction`** — cai na base (item 1). O jogador **observado pelo Host/Headless** usa `ObservedInventoryController` (item 3). São dois caminhos de código totalmente distintos.

O patch existente do UIFixes (`FikaActiveWeaponMagSwapPatch`, fora de escopo deste item) intercepta via Harmony `TraderControllerClass.CheckItemAction` — o alvo do item 1. Por despacho virtual, essa interceptação nunca executa para instâncias de `ObservedInventoryController`, porque o override do item 3 tem corpo IL próprio e nunca invoca a base. **Isto é exatamente o antipattern AP-03** (alvo virtual patcheado sem auditar todos os overrides) — auditoria completa feita acima.

### 1.2 Por que a colisão acontece de verdade (não é falso-positivo aleatório)

A checagem de `inOutHandsProcess` não é uma peculiaridade do Fika nem do caso observado — é o motor do próprio EFT protegendo qualquer item aninhado dentro da arma empunhada (`ItemInHands`) enquanto uma transição de "entrar/sair das mãos" está em voo:

- Toda operação de inventário (mover, trocar, remover) passa por `IItemOwner.OutProcess`/`InProcess`. Na implementação de `Player` ([`Player.cs:1110-1136`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L1110)), `OutProcess`/`InProcess` chamam `method_34`/`method_33` ([`Player.cs:1441-1464`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L1441)), que por sua vez chamam `Player_0.TryRemoveFromHands(item2, ...)` / `Player_0.TrySetInHands(item, to, ...)`.
- **`item2`/`item` passados para `TryRemoveFromHands`/`TrySetInHands` não são sempre a arma.** `method_35` ([`Player.cs:1471-1495`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L1471)) decide o valor: se o item movido **é** a própria arma empunhada, `item2` = a arma (caso saque/guarda de arma). Se o item movido só está **aninhado** dentro da arma (endereço de origem/destino filho de `ItemInHands`, via `GClass3380.IsChildOf`, linhas 1481/1485 — `GClass3380` = `ItemExtensions`), `item2` = o **item original movido** (ex.: o carregador), não a arma. **Essa distinção é o discriminador que faltava na primeira versão desta spec (PA-01-02) — ver §1.4.**
- Quando dispara, `Player.TryRemoveFromHands` ([`Player.cs:32223-32263`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L32223)) e `TrySetInHands` ([`Player.cs:32294-32337`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L32294)) abrem `RaiseInOutProcessEvents(new GEventArgs17(HandsController.Item, CommandStatus.Begin, ...))` (linhas 32248/32313 — o `Item` do evento é **sempre a arma**, `HandsController.Item`, independente de `item2`/`item` ser a arma ou o carregador), executam `HandsController.Execute(operation, callback)`, e só fecham com `CommandStatus.Succeed` (linhas 32255/32320) **dentro do callback de conclusão de `Execute`**.
- Uma troca 1-para-1 (`InteractionsHandlerClass.Swap`) move **dois** itens (o carregador que sai e o que entra), ambos endereçados dentro da mesma arma empunhada — logo, cada metade abre sua própria janela Begin→Succeed sobre a **mesma arma**. Se a segunda metade é validada por `CheckItemAction` antes da primeira ter recebido seu `Succeed`, a colisão é **genuína pela definição do motor** — só que é a própria troca colidindo consigo mesma, não uma concorrência real de outro jogador ou de outra ação (como um saque de arma).
- `ObservedFirearmController` (Headless) **não é um stub** — estende `FirearmController` real e delega `ReloadMag`/`QuickReloadMag` direto para `CurrentOperation` ([`ObservedFirearmController.cs:183-192`](../../../../mods/FIKA/modded/Fika-Plugin/Fika.Core/Main/ObservedClasses/HandsControllers/ObservedFirearmController.cs#L183)), sem overrides de `CanExecute`/`Execute`. Isso descarta a hipótese "stub incompleto" (como no item 001, DropBackpack) — a janela Begin→Succeed aqui é a mesma FSM real, só que sua confirmação de conclusão no Headless está sujeita a uma latência que não existe (ou é desprezível) no caminho 100% local.

### 1.3 Caminhos de correção descartados (com motivo)

- **Correlacionar por `EventId`/`OwnerId` do `GEventArgs17`.** Descartado: `GEventArgs1` ([`GEventArgs1.cs:6-30`](../../../../references/eft-decompiled/Assembly-CSharp/GEventArgs1.cs#L6)) gera `EventId` de um contador estático global (`Int_0++`, linha 27) sem relação entre as duas metades de um mesmo swap, e `OwnerId` é o ID do `InventoryController` — igual para **todos** os eventos de um mesmo jogador, então não discrimina "minha própria troca" de "outra operação minha". Nenhum dos dois campos serve de correlação.
- **Bypass amplo por tipo (`item is MagazineItemClass`), como o UIFixes tenta hoje.** Descartado: enfraquece a proteção também para uma colisão genuína (ex.: segunda tentativa de outro jogador no mesmo item, corner case já coberto na spec funcional).
- **Janela de graça baseada só em tempo, sem saber o que abriu o `Begin` (versão original desta spec, revisada em PA-01-02).** Descartado: `GEventArgs17` só carrega a arma (`HandsController.Item`), nunca o item efetivamente movido — uma janela de graça pura-tempo não distingue a própria troca de magazine de um saque de arma real que por coincidência caiu na mesma janela, violando o corner case da spec funcional. Ver §1.4 para o desenho corrigido.
- **Reestruturar `InteractionsHandlerClass.Swap` para abrir uma única janela atômica para as duas metades.** Adiado: `InteractionsHandlerClass.cs` está marcado `// DECOMPILE-ERROR` no dump ([`InteractionsHandlerClass.cs:2`](../../../../references/eft-decompiled/Assembly-CSharp/InteractionsHandlerClass.cs#L2)) — exigiria re-decompile pontual via `ilspycmd -t` (AGENTS.md) antes de qualquer stub confiável. Risco/custo desproporcional para este item; revisitar só se a abordagem escolhida abaixo não bastar na validação in-game.

### 1.4 Abordagem escolhida (revisada — PA-01-01, PA-01-02)

Patch cirúrgico em duas partes, ambas em `mods/FIKA/modded/`:

1. **Correlação por item efetivamente movido, não só por tempo** — três Harmony patches num único arquivo:
   - `[HarmonyPrefix]` em `Player.TryRemoveFromHands(Item item, GInterface438 abstractOperation, Callback callback)` ([`Player.cs:32223`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L32223)) e em `Player.TrySetInHands(Item item, ItemAddress to, GInterface438 operation, Callback originalCallback)` ([`Player.cs:32294`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L32294)): gravam, num campo estático "ambiente" (`_pendingMovedItem`), o parâmetro `item` recebido — que é a **arma** quando a transição é um saque/guarda de arma, ou o **item aninhado** (o carregador, no caso do swap) quando é um reload/swap. Ambos os métodos são chamados só a partir da main thread (fluxo de input/rede do EFT), sem concorrência a proteger.
   - `[HarmonyPostfix]` em `TraderControllerClass.RaiseInOutProcessEvents(GEventArgs17 args)` ([`TraderControllerClass.cs:1887`](../../../../references/eft-decompiled/Assembly-CSharp/TraderControllerClass.cs#L1887) — método **não-virtual**, sem risco de AP-03): quando `args.Status == CommandStatus.Begin`, lê o `_pendingMovedItem` gravado pelo Prefix acima (ainda válido, porque `RaiseInOutProcessEvents` é chamado **de dentro** do corpo de `TryRemoveFromHands`/`TrySetInHands`, antes do Postfix desses métodos limpar o campo) e grava `(controller, arma = args.Item, itemMovido, Time.time)`. Em `CommandStatus.Succeed`, remove a entrada daquela arma.
   - `[HarmonyPostfix]` em `TryRemoveFromHands`/`TrySetInHands`: limpa `_pendingMovedItem = null` — evita que uma leitura tardia (de um `RaiseInOutProcessEvents` chamado por outro caminho não coberto) reaproveite um valor obsoleto. Se `_pendingMovedItem` estiver `null` no momento do `Begin`, a entrada é gravada sem `itemMovido` (fallback seguro: nunca isenta).
2. **Exceção escopada dentro do override que realmente roda** — editar diretamente `ObservedInventoryController.CheckItemAction` (não um Harmony patch — é código do mod, editável) para, **somente** quando o item em conflito é um `MagazineItemClass` colidindo via `inOutHandsProcess` com a arma empunhada, **e** o registro da janela mostra que o `Begin` daquela arma foi aberto por outro `MagazineItemClass` (nunca pela própria arma) há **menos que uma janela de graça curta** (marcada abaixo como `TODO confirmar` — calibrar por instrumentação temporária antes de fechar o item), deixar de marcar a colisão. Todas as outras checagens de colisão do método (`GEventArgs7/8/2/3`, colisão direta de item, `smethod_0`) permanecem intocadas e continuam bloqueando qualquer conflito genuíno — **incluindo, por construção, um saque de arma real na mesma janela: nesse caso `itemMovido` gravado é a própria arma, não um `MagazineItemClass`, então a condição de isenção nunca é satisfeita** (fecha PA-01-02).

**Por que isso cobre o corner case "dois jogadores no mesmo item" (PA-01-03):** a isenção acima só desativa o ramo específico da checagem `inOutHandsProcess` (linhas 160-177 atuais). As demais checagens do mesmo laço permanecem ativas sem nenhuma alteração — em particular `if (item == geventArgs2.Item) { flag = true; }` ([`ObservedInventoryController.cs:178`](../../../../mods/FIKA/modded/Fika-Plugin/Fika.Core/Main/ObservedClasses/ObservedInventoryController.cs#L178) — leitura direta confirmada nesta revisão), que dispara sempre que o item sendo validado é a mesma instância de `Item` referenciada por **qualquer** evento pendente em `List_0`, independente de tipo. Como `List_0` pertence ao `InventoryController` dono do item, uma segunda tentativa de mexer no mesmo item (de qualquer jogador, enquanto a primeira operação ainda está pendente) continua caindo nessa checagem, que este item não toca.

Isso resolve a causa raiz (a própria troca colidindo consigo mesma) sem tocar a proteção do motor contra concorrência real ou contra saques de arma reais, e sem depender de reescrever `InteractionsHandlerClass`.

## 2. Pontos de patch

| Alvo (Assembly/mod) | Tipo | Motivo |
|---|---|---|
| [`Player.cs:32223`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L32223) — `TryRemoveFromHands(Item, GInterface438, Callback)` | Prefix (novo) | Gravar o item efetivamente movido (`_pendingMovedItem`) antes do `Begin` |
| ~~`Player.cs:32294` — `TrySetInHands(...)`~~ | ~~Prefix + Postfix~~ | **Descartado (CR-01-02):** `ObservedInventoryController.InProcess` nunca chama `TrySetInHands` — ver [`ObservedInventoryController.cs:290-296`](../../../../mods/FIKA/modded/Fika-Plugin/Fika.Core/Main/ObservedClasses/ObservedInventoryController.cs#L290) |
| [`ObservedInventoryController.cs:290-296`](../../../../mods/FIKA/modded/Fika-Plugin/Fika.Core/Main/ObservedClasses/ObservedInventoryController.cs#L290) — `HandleInProcess` | Edição direta (mod source) | Chamar `InOutHandsProcessTimestampPatch.SetPendingMovedItem(item)` antes do `Begin` — cobre o lado "inserir" que `InProcess` sobrescreve |
| [`TraderControllerClass.cs:1887`](../../../../references/eft-decompiled/Assembly-CSharp/TraderControllerClass.cs#L1887) — `RaiseInOutProcessEvents(GEventArgs17)` | Postfix (novo) | Registrar `(arma, itemMovido, timestamp)` no `Begin`; limpar no `Succeed` |
| [`ObservedInventoryController.cs:160-177`](../../../../mods/FIKA/modded/Fika-Plugin/Fika.Core/Main/ObservedClasses/ObservedInventoryController.cs#L160) — bloco `inOutHandsProcess` dentro de `CheckItemAction` | Edição direta (mod source) | Suprimir a colisão só quando for a própria troca de magazine em andamento na mesma arma, dentro da janela de graça, e nunca quando o `Begin` foi aberto pela arma em si (saque/guarda) |

## 3. Novas propriedades F12 (BepInEx)

N/A — nenhuma `ConfigEntry` nova. A janela de graça é uma constante interna (ver §7/§8); não expor no F12 até haver evidência de que precisa ser ajustável por usuário.

## 4. Arquivos do mod

| Arquivo | Ação | Resumo |
|---|---|---|
| `mods/FIKA/modded/Fika-Plugin/Fika.Core/Main/Patches/InventoryPatches/InOutHandsProcessTimestampPatch.cs` | CRIAR | 2 Prefix + 2 Postfix (ver §2) + API estática de consulta. Auto-registrado pelo `_patchManager.EnablePatches()` já existente ([`FikaPlugin.cs:179`](../../../../mods/FIKA/modded/Fika-Plugin/Fika.Core/FikaPlugin.cs#L179)) — confirmado que **nenhum** `ModulePatch` deste mod é habilitado via `.Enable()` manual fora dos dois casos condicionais em `FikaConfig.cs:910-911`; a descoberta de tipos é automática, então não é necessário editar `FikaPlugin.cs` para registrar o novo patch. |
| `mods/FIKA/modded/Fika-Plugin/Fika.Core/Main/ObservedClasses/ObservedInventoryController.cs` | MODIFICAR | Adicionar a condição de exceção escopada dentro do laço existente de `CheckItemAction` (linhas 160-177 atuais, preservando os blocos `#if DEBUG` existentes) |
| `mods/FIKA/modded/Fika-Plugin/Fika.Core/FikaPlugin.cs` | MODIFICAR | Bump de `FikaVersion` (linha 48, atualmente `"2.3.13"` — **nota:** já mais nova que o `2.3.11` registrado em `mods/FIKA/mod.json`, drift pré-existente não relacionado a este item, só alinhar no bump) |
| `mods/FIKA/mod.json` | MODIFICAR | Bump do componente `plugin` para acompanhar `FikaVersion` |

## 5. Stubs de código

```csharp
// mods/FIKA/modded/Fika-Plugin/Fika.Core/Main/Patches/InventoryPatches/InOutHandsProcessTimestampPatch.cs
using System.Reflection;
using System.Runtime.CompilerServices;
using EFT;
using EFT.InventoryLogic;
using HarmonyLib;
using SPT.Reflection.Patching;
using UnityEngine;

namespace Fika.Core.Main.Patches.InventoryPatches;

/// <summary>
/// Correlaciona cada janela "entrando/saindo das mãos" (GEventArgs17/Begin-Succeed) com o item
/// que efetivamente a abriu — a própria arma (saque/guarda) ou um item aninhado nela (ex.: um
/// carregador, num reload/swap). Consumido por ObservedInventoryController.CheckItemAction para
/// reconhecer, dentro de uma janela curta, que a colisão detectada é a própria troca de magazine
/// em andamento (as duas metades do swap abrem/fecham essa janela na mesma arma) — nunca um
/// saque de arma real ou outra operação concorrente genuína.
/// </summary>
public static class InOutHandsProcessTimestampPatch
{
    private sealed class Entry
    {
        public Item MovedItem;
        public float Timestamp;
    }

    // ref: Assembly-CSharp/TraderControllerClass.cs:1887 — RaiseInOutProcessEvents(GEventArgs17 args)
    private static readonly ConditionalWeakTable<TraderControllerClass, System.Collections.Generic.Dictionary<Item, Entry>> _state = new();

    // Campo "ambiente": item efetivamente passado para TryRemoveFromHands/TrySetInHands
    // (a arma, num saque/guarda; um item aninhado — ex. carregador — num reload/swap).
    // Válido só durante a execução síncrona desses dois métodos (main thread only).
    private static Item _pendingMovedItem;

    /// <summary>Se há um Begin pendente para essa arma, diz há quanto tempo e qual item o abriu.</summary>
    public static bool TryGetPendingBegin(TraderControllerClass controller, Item weapon, out Item movedItem, out float elapsedSeconds)
    {
        movedItem = null;
        elapsedSeconds = 0f;
        if (weapon == null || !_state.TryGetValue(controller, out var perWeapon) || !perWeapon.TryGetValue(weapon, out var entry))
        {
            return false;
        }

        movedItem = entry.MovedItem;
        elapsedSeconds = Time.time - entry.Timestamp;
        return true;
    }

    public class CaptureMovedItemOnRemove : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            // ref: Assembly-CSharp/EFT/Player.cs:32223
            return AccessTools.Method(typeof(Player), nameof(Player.TryRemoveFromHands));
        }

        [PatchPrefix]
        public static void Prefix(Item item)
        {
            _pendingMovedItem = item;
        }

        [PatchPostfix]
        public static void Postfix()
        {
            _pendingMovedItem = null;
        }
    }

    // ref: CR-01-02 — DESCARTADO durante o /code-mod. Leitura direta de ObservedInventoryController.cs
    // mostrou que InProcess é sobrescrito ali e chama RaiseInOutProcessEvents diretamente, nunca
    // passando por Player.TrySetInHands — este patch seria código morto para o caminho Headless que
    // este item resolve. Substituído por uma chamada direta a SetPendingMovedItem(item) dentro de
    // ObservedInventoryController.HandleInProcess (sem Harmony). Bloco abaixo preservado só como
    // registro do que foi planejado; NÃO existe em InOutHandsProcessTimestampPatch.cs.
    // Ver 003-magazine-swap-inplace-fix-05-asbuild.md § "Desvio confirmado durante o build".
    //
    // public class CaptureMovedItemOnSet : ModulePatch
    // {
    //     protected override MethodBase GetTargetMethod()
    //     {
    //         // ref: Assembly-CSharp/EFT/Player.cs:32294
    //         return AccessTools.Method(typeof(Player), nameof(Player.TrySetInHands));
    //     }
    //
    //     [PatchPrefix]
    //     public static void Prefix(Item item)
    //     {
    //         _pendingMovedItem = item;
    //     }
    //
    //     [PatchPostfix]
    //     public static void Postfix()
    //     {
    //         _pendingMovedItem = null;
    //     }
    // }

    public class RecordBeginSucceed : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            // ref: Assembly-CSharp/TraderControllerClass.cs:1887
            return AccessTools.Method(typeof(TraderControllerClass), nameof(TraderControllerClass.RaiseInOutProcessEvents));
        }

        [PatchPostfix]
        public static void Postfix(TraderControllerClass __instance, GEventArgs17 args)
        {
            if (args?.Item == null)
            {
                return;
            }

            var perWeapon = _state.GetOrCreateValue(__instance);
            if (args.Status == CommandStatus.Begin)
            {
                perWeapon[args.Item] = new Entry { MovedItem = _pendingMovedItem, Timestamp = Time.time };
            }
            else if (args.Status == CommandStatus.Succeed)
            {
                perWeapon.Remove(args.Item);
            }
        }
    }
}
```

```csharp
// Edição em mods/FIKA/modded/Fika-Plugin/Fika.Core/Main/ObservedClasses/ObservedInventoryController.cs
// dentro do laço "foreach (var geventArgs2 in List_0)" de CheckItemAction (linhas 120-200 atuais).
// Trecho ANTES (linhas 160-177 atuais — blocos #if DEBUG existentes preservados, omitidos abaixo só por brevidade):
//
//     lambda.inOutHandsProcess = geventArgs2 as GEventArgs17;
//     if (lambda.inOutHandsProcess != null)
//     {
//         if (item.GetAllParentItemsAndSelf(false).Any(lambda.method_1)) { /* #if DEBUG log #endif */ flag = true; }
//         if (location?.Container.ParentItem.GetAllParentItemsAndSelf(false).Any(lambda.method_1) == true) { /* #if DEBUG log #endif */ flag = true; }
//     }
//
// Trecho DEPOIS (blocos #if DEBUG existentes devem ser preservados nas mesmas posições):

lambda.inOutHandsProcess = geventArgs2 as GEventArgs17;
if (lambda.inOutHandsProcess != null)
{
    bool collidesViaHands =
        item.GetAllParentItemsAndSelf(false).Any(lambda.method_1) ||
        location?.Container.ParentItem.GetAllParentItemsAndSelf(false).Any(lambda.method_1) == true;

    // ref: PA-01-02 — só a metade "gêmea" da mesma troca de magazine em andamento é isenta
    // (itemMovido precisa ser um MagazineItemClass; um saque/guarda de arma grava a própria
    // arma como itemMovido e nunca satisfaz essa condição). Qualquer outra colisão via
    // inOutHandsProcess continua bloqueada.
    if (collidesViaHands && !IsSelfReferentialMagazineSwap(item, lambda.inOutHandsProcess))
    {
        flag = true;
    }
}

// Novo método privado de instância na mesma classe (NÃO static — precisa de "this", ref: PA-01-01):
private bool IsSelfReferentialMagazineSwap(Item item, GEventArgs17 inOutHandsProcess)
{
    if (item is not MagazineItemClass || inOutHandsProcess?.Item is not Weapon weapon)
    {
        return false;
    }

    if (!Fika.Core.Main.Patches.InventoryPatches.InOutHandsProcessTimestampPatch
            .TryGetPendingBegin(this, weapon, out var movedItem, out var elapsed))
    {
        return false;
    }

    // TODO confirmar: janela de graça calibrada por instrumentação temporária (log do
    // elapsedSeconds real observado numa rejeição reproduzida em Headless) antes de fechar o item.
    const float GraceWindowSeconds = 0.35f;
    return movedItem is MagazineItemClass && elapsed <= GraceWindowSeconds;
}
```

## 6. Fluxo de dados

```
[A] Jogador arrasta carregador do rig sobre a arma empunhada (UIFixes, fora deste mod)
      → InteractionsHandlerClass.Swap(mag_novo, endereço_arma, mag_velho, endereço_rig, ...)
[B] Cliente: cada metade do swap passa por Player.OutProcess/InProcess (Player.cs:1110-1136)
      → method_35 detecta endereço aninhado em ItemInHands e retorna o CARREGADOR (não a arma) — Player.cs:1471-1495
      → TryRemoveFromHands/TrySetInHands recebem o carregador como "item"; Prefix grava _pendingMovedItem = carregador
      → RaiseInOutProcessEvents abre GEventArgs17 Begin na ARMA (Player.cs:32248/32313)
      → Postfix de RecordBeginSucceed grava (arma, itemMovido=carregador, timestamp=agora)
[C] InventoryPacket enviado ao Headless (ReliableOrdered) — cliente já refletiu o swap otimisticamente
[D] Headless aplica a operação via ObservedInventoryController (representa este jogador no servidor)
      → CheckItemAction roda para a 2ª metade do swap ANTES do Succeed da 1ª metade ter fechado
      → sem a correção: List_0 ainda tem o Begin da 1ª metade → GClass1561 (ObservedInventoryController.cs:160-198)
      → com a correção: TryGetPendingBegin confirma Begin da MESMA arma, itemMovido=carregador, há < 0.35s
        → IsSelfReferentialMagazineSwap retorna true → colisão não é marcada → segue para GClass1568 (sucesso)
      → CENÁRIO PROTEGIDO (saque de arma real na mesma janela): itemMovido gravado seria a própria ARMA,
        não um MagazineItemClass → IsSelfReferentialMagazineSwap retorna false → GClass1561 continua sendo retornado
[E] OperationCallbackPacket(Succeed) volta ao cliente — sem WaitingForCallback preso, sem pente piscando
```

## 7. Riscos e dependências

- **Patch atual do UIFixes** (`FikaActiveWeaponMagSwapPatch`, `mods/UIFixes/modded/src/Patches/SwapPatches.cs:891-924`, fora de escopo/Trilha B) intercepta `TraderControllerClass.CheckItemAction` no cliente — método diferente do editado aqui (`ObservedInventoryController.CheckItemAction`, override distinto). Sem sobreposição de patch; convivem sem conflito até a Trilha B revisar/remover o do UIFixes.
- **`HandsAreNotBusy`** (`mods/HandsAreNotBusy/modded/HANB_FikaSync.cs:175-201`) chama `InventoryController.RemoveActiveEvent` diretamente em `List_0` como mecanismo manual de emergência. Não há conflito de código (arquivos diferentes, sem chamada cruzada), mas se HANB limpar um `Begin` no meio da janela de graça, nosso dicionário de correlação fica com uma entrada "órfã" até o próprio `Succeed` nunca chegar a limpá-la — inofensivo: `TryGetPendingBegin` só é consultado quando `List_0` já sinaliza colisão; se HANB já removeu o evento de `List_0`, não há colisão a suprimir.
- **Item 002 (watchdog de timeout)** continua como rede de segurança para qualquer caso não coberto pela janela de graça (ex.: latência real acima de 0.35s).
- **`SPT-MagCheckInterrupt`/`LoadAmmoAnim`** consomem o mesmo pipeline de `ReloadMag` mas não geram `GEventArgs17` nem tocam `CheckItemAction` diretamente (confirmado por grep nesta investigação) — risco residual baixo; corner case de regressão já coberto na spec funcional para teste manual.
- **Ordem de inicialização:** nenhuma — o novo patch é descoberto automaticamente por `_patchManager.EnablePatches()`, sem exigir edição de `FikaPlugin.cs` além do bump de versão.
- **Drift de versão pré-existente:** `mods/FIKA/mod.json` (`2.3.11`) já estava desatualizado em relação a `FikaPlugin.cs` (`2.3.13`) antes deste item — não investigado a fundo (fora de escopo), só alinhado no bump feito aqui.
- **Crescimento não-limitado de baixo risco (PA-01-05, aceito):** se um `Succeed` nunca chegar para uma arma específica (ex.: desconexão no meio da transição), a entrada correspondente permanece no dicionário interno pelo resto da raid. Escopo limitado ao número de armas distintas que aquele jogador empunhou na raid (baixo); `ConditionalWeakTable` já garante que isso não sobrevive entre raids (chave = `TraderControllerClass`, coletado com o jogador). Risco aceito sem mitigação adicional nesta versão.

## 8. Checklist de implementação

- [x] Criar `InOutHandsProcessTimestampPatch.cs` conforme stub §5, **com 1 desvio confirmado durante o `/code-mod`**: `CaptureMovedItemOnSet` (Prefix em `Player.TrySetInHands`) foi **descartado** — leitura direta de `ObservedInventoryController.cs` mostrou que `InProcess` é sobrescrito ali (linhas 260-274/276-305) e chama `RaiseInOutProcessEvents` diretamente, nunca passando por `TrySetInHands`. Substituído por uma chamada direta a `InOutHandsProcessTimestampPatch.SetPendingMovedItem(item)` dentro de `HandleInProcess` (sem Harmony, é código do próprio mod). Mantido: `CaptureMovedItemOnRemove` (Prefix+Postfix em `Player.TryRemoveFromHands`, cobre o lado `OutProcess`, que não é sobrescrito) e `RecordBeginSucceed` (Postfix em `RaiseInOutProcessEvents`). Ver `003-magazine-swap-inplace-fix-05-asbuild.md`.
- [ ] Adicionar instrumentação temporária (log `elapsedSeconds` real, e `movedItem?.GetType().Name`) antes de fixar `GraceWindowSeconds` — reproduzir a rejeição em Headless dedicado + 1 cliente, capturar 5-10 amostras, escolher o valor final com margem (ver `spt-performance-analysis` § instrumentação temporária). **Pendente — depende de `/compile-mod` + sessão in-game, fora do escopo do `/code-mod`.**
- [x] Editar `ObservedInventoryController.CheckItemAction` conforme stub §5 — método `IsSelfReferentialMagazineSwap` como **instância** (sem `static`, PA-01-01). Blocos `#if DEBUG` das duas checagens `Any(lambda.method_1)` **consolidados em um único log** (`"failed inOutHandsProcess check (not a self-referential magazine swap)"`), já que a decisão de isenção agora avalia as duas juntas via `collidesViaHands` — não é uma preservação literal 1:1 dos dois logs originais (PA-01-04), mas cobre o mesmo evento. As demais checagens do laço (`GEventArgs7/8/2/3`, colisão direta, `smethod_0`) permanecem intocadas.
- [ ] Teste manual dedicado ao corner case "dois jogadores no mesmo item" (PA-01-03) confirmando que `item == geventArgs2.Item` (`ObservedInventoryController.cs:178`) continua bloqueando, já que esse ramo não foi tocado. **Pendente — in-game.**
- [ ] Teste manual dedicado ao corner case "swap logo após sacar a arma" (spec funcional) confirmando que a rejeição continua ocorrendo quando o `Begin` pendente foi aberto por um saque de arma (não por um `MagazineItemClass`). **Pendente — in-game.**
- [ ] Remover a instrumentação temporária (ou rebaixar para `LogDebug` gated) antes de fechar o item. **N/A nesta rodada — nenhuma instrumentação temporária foi adicionada ainda (ver item acima); revisitar quando a calibração da janela de graça for feita.**
- [x] Bump `FikaPlugin.cs:48` (`FikaVersion`: `2.3.13` → `2.3.14`) e `mods/FIKA/mod.json` (componente `plugin`: `2.3.11` → `2.3.14`, alinhando o drift pré-existente).
- [ ] Compilar `Fika.Core.dll`; propagar para `Fika-Headless/References/Fika.Core.dll`; recompilar `Fika.Headless.dll` (mesmo procedimento do item 001). **Pendente — `/compile-mod`.**
- [ ] Validar in-game os 4 cenários da spec funcional (singleplayer, Host, Headless dedicado, corner case de concorrência real ainda bloqueada) + os 2 corner cases adicionais acima. **Pendente — in-game.**

## 9. Conformidade com skills (auto-checklist)

| # | Check | Status | Evidência / razão |
|---|---|---|---|
| 1 | Lifecycle de raid: start hook + stop hooks idempotentes — AP-01 | N/A | Sem estado por raid a inicializar/encerrar: `ConditionalWeakTable<TraderControllerClass, ...>` não fixa (pin) as chaves — entradas são coletadas junto com o `TraderControllerClass`/jogador quando a raid termina (§5, §7 PA-01-05) |
| 2 | Filtro MainPlayer/Fika em todo patch que reage a ação de player — AP-02 | N/A | Os patches de captura/timestamp são observação pura, rodam para qualquer controller sem alterar comportamento; a única mudança de comportamento fica isolada dentro do override `ObservedInventoryController.CheckItemAction`, usado só para jogadores observados pelo Host/Headless — filtro já é estrutural |
| 3 | Alvos ofuscados/virtuais resolvidos por assinatura; TODOS os overrides auditados — AP-03 | ✅ | §1.1: 3 overrides de `CheckItemAction` auditados (base `TraderControllerClass.cs:1163`, `Player.PlayerInventoryController` que chama base `Player.cs:1438`, `ObservedInventoryController` que não chama base) — edição feita direto no override que roda no caminho problemático, não via Harmony na base. `RaiseInOutProcessEvents`, `TryRemoveFromHands`, `TrySetInHands` confirmados não-virtuais/instância direta de `Player` (não há override a auditar) |
| 4 | Mudança de estado via API canônica do EFT; side-effects mapeados — AP-04 | ✅ | Não mutamos item/endereço diretamente; só suprimimos condicionalmente um `flag` de validação já calculado pelo motor, preservando o restante do pipeline (`method_16`/`GClass1568`) inalterado |
| 5 | Estado entre raids: raid1→exit→raid2 e alt-F4/morte/MIA cobertos | ✅ | Ver check 1 — `ConditionalWeakTable` limpa junto com o controller; sem coleção estática acumulando entre raids |
| 6 | Semântica/defaults/faixas de cada ConfigEntry sem ambiguidade — AP-05 | N/A | Nenhuma `ConfigEntry` nova (§3) |
| 7 | Re-invocação de método patcheado tem reentry-guard — AP-07 | N/A | Nenhum Postfix/Prefix re-invoca o próprio método patcheado ou `CheckItemAction`; sem recursão possível |
| 8 | Flags/caches de intercept validados contra o contexto atual após troca — AP-08 | ✅ | Entrada do dicionário é por `(controller, arma)` e removida no `Succeed`; trocar de arma cria uma entrada nova para a nova arma, nunca reaproveita timestamp/itemMovido de uma arma anterior. Campo ambiente `_pendingMovedItem` é limpo no Postfix de `TryRemoveFromHands`/`TrySetInHands`, evitando leitura obsoleta por um `RaiseInOutProcessEvents` de outra origem |
| 9 | Todo patch-point reconfirmado no `.cs` do dump — AP-09 | ✅ | Todas as citações desta spec (incluindo as adicionadas nesta revisão) foram lidas diretamente nesta sessão em `TraderControllerClass.cs`, `Player.cs`, `GEventArgs1.cs`, `GClass3380.cs`, `ObservedFirearmController.cs`, `ObservedInventoryController.cs` — nenhuma citação vem só de relato de subagente sem reconferência |
| 10 | Skill EFT usada como lever confirmada não-inerte — AP-10 | N/A | Não usa skill do EFT como alavanca |
| 11 | Pacote FIKA próprio: envelope/`TryGet*`/`Valid`/main thread/registro por instância/zero `UnregisterPacket` — AP-11 | N/A | Não declara nenhum `INetSerializable` novo; não envia nada pela rede |

## Histórico

| Data | Evento |
|---|---|
| 2026-09-06 | Spec técnica criada via `/create-technical-spec` |
| 2026-09-06 | Revisão `/review-technical-spec` 01 — 2 bloqueadores (PA-01-01, PA-01-02) + 1 importante (PA-01-03) + 2 menores (PA-01-04, PA-01-05) resolvidos: método de instância corrigido; correlação por item-efetivamente-movido substitui janela pura-tempo; corner case "dois jogadores" documentado com evidência; citações de linha corrigidas; crescimento de baixo risco aceito e documentado |
| 2026-09-06 | `/code-mod` executado — desvio confirmado contra o código real: `ObservedInventoryController.InProcess` é sobrescrito e nunca chama `Player.TrySetInHands`, então o patch `CaptureMovedItemOnSet` planejado em §5 foi substituído por uma chamada direta em `HandleInProcess` (sem Harmony). Ver `003-magazine-swap-inplace-fix-05-asbuild.md` para o detalhe do que foi de fato implementado. |
