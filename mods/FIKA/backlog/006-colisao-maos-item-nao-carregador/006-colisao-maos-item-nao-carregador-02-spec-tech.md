# 006 — Trava de mãos ao equipar arma/faca/granada após ação recente (item não-carregador) · Spec Técnica

**Mod:** FIKA
**Spec funcional:** [006-colisao-maos-item-nao-carregador-01-spec.md](006-colisao-maos-item-nao-carregador-01-spec.md)
**Criado:** 2026-09-10

> Fonte primária de verdade para qualquer assinatura, fórmula ou ponto de patch: [references/eft-decompiled/Assembly-CSharp/](../../../../references/eft-decompiled/Assembly-CSharp/). Toda referência ao código do EFT cita `arquivo.cs:linha`, reconfirmada por leitura direta nesta sessão (não herdada só do relatório de auditoria).

## ⚠️ Restrição crítica — superfície pública do FIKA

`mods/FIKA/modded/` é consumido como biblioteca por dezenas de mods **já presentes neste repositório e por qualquer mod externo que venha a ser instalado no futuro** — a lista de dependentes mapeada por `grep` neste repo (~20 mods) é só uma amostra do risco conhecido, não o universo de risco real. Nenhuma das quatro correções deste item altera nome, assinatura ou modificador de acesso de nenhum membro público. Três delas (Fix 1a, Fix 1b e o airbag de pacote) tocam código atrás de um método público **já existente e herdado do EFT** (`CheckItemAction`, `override` de `TraderControllerClass`) — não criam superfície nova, mas mudam **comportamento observável** desse método (uma operação antes rejeitada passa a ser aceita em dois casos específicos, um por cada Fix). Ver §7 "Riscos e dependências" para o detalhamento desse risco residual não-relacionado a assinatura.

## 1. Estratégia

> **Revisão pós-`PA-01-01` (2026-09-11):** a review técnica 01 encontrou, por leitura do Assembly, que a causa raiz assumida (`SetEmptyHands`/`TrySetLastEquippedWeapon` colidindo via `GEventArgs17`/`inOutHandsProcess`) não se sustentava — esses dois métodos usam um pipeline diferente (`Proceed`/`Process<>`/`DropCurrentController`), que nunca levanta `GEventArgs17`. O usuário validou empiricamente (build Debug, log do Headless em raid real reproduzindo o bug do `SPT-ContinuousLoadAmmo`) qual bloco realmente dispara — ver §1.1 abaixo. **A causa raiz real é outra, e o Fix 1 foi redesenhado** (agora "Fix 1a" + "Fix 1b"). Fixes 2 e 3 (rede/logging) não são afetados por essa revisão. Ver `006-colisao-maos-item-nao-carregador-03-spec-tech-review-01.md`, ponto `PA-01-01`, para a evidência completa e o rastreamento no Assembly que embasa esta reescrita.

### 1.1 Causa raiz confirmada por evidência empírica direta (log dirigido)

Build `Fika.Core` Debug reproduzido em raid Headless real (município de vários carregadores em sequência via `SPT-ContinuousLoadAmmo`) capturou, no momento exato da trava:

```
[Error : Fika.Core] [CheckItemAction]: item was same as GEventArgs2.Item
[Error : Fika.Core] [CheckItemAction]: Flag hit, gevent was GEventArgs10
```

O log `"failed inOutHandsProcess check"` (que confirmaria a hipótese original) **não apareceu em nenhuma tentativa**. Quem disparou foi o bloco genérico `if (item == geventArgs2.Item) { flag = true; }` ([`ObservedInventoryController.cs:182-188`](../modded/Fika-Plugin/Fika.Core/Main/ObservedClasses/ObservedInventoryController.cs#L182)) — que roda para **qualquer** tipo de evento em `List_0`, não só `GEventArgs17` — contra um `GEventArgs10` pendente.

Rastreamento do mecanismo real no Assembly (lido diretamente nesta sessão):

- `GEventArgs10` = alias 4.1 `EFT.InventoryLogic.RemoveFromHandsEventArgs` ([`GEventArgs10.cs:1`](../../../../references/eft-decompiled/Assembly-CSharp/GEventArgs10.cs#L1) — comentário de alias já injetado no dump). É levantado por `Class1312.vmethod_0()`/`vmethod_1()` ([`Player.cs:22234-22243`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L22234)) via `InventoryController.RaiseEvent(new GEventArgs10(item, status, controller))` — `Begin` no `vmethod_0` (chamado de dentro do construtor via `Execute()`), `Succeed`/`Failed` no `vmethod_1` (chamado por `.Confirm(succeed)`).
- `Class1312` é criado e **executado imediatamente** por `Player.method_138(Item item)` ([`Player.cs:32383-32393`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L32383)) — o próprio jogo rotula essa operação como `"BeginRemoveFromHands"` no log de fallback (linha 32391, `Debug.LogWarning`). Existe um par simétrico, `Class1311`/`GEventArgs9`/`method_137` (`Player.cs:32371-32381`, `"BeginSetInHands"`), para o lado "entrando nas mãos".
- **Quem chama `method_138` e nunca confirma a tempo:** `FirearmController.Drop(...)` ([`Player.cs:13506-13524`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L13506)) chama `_player.method_138(Item)` **antes** de iniciar `CurrentOperation.HideWeapon(onHidden, fastDrop, nextControllerItem)`, e só chama `inventoryOperation.Confirm()` **dentro do callback `onHidden`**, ou seja, **quando a animação/operação de esconder a arma termina de verdade**. Existe uma janela real — do tamanho dessa animação — em que a arma tem um `GEventArgs10` `Begin` pendente em `List_0`.
- `Player.SetEmptyHands(callback)` ([`Player.cs:31704-31711`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L31704), usado por `SPT-ContinuousLoadAmmo` pra tirar a arma da mão antes de carregar munição fora do inventário) → `Proceed(withNetwork: true, callback)` → `Process<>.Execute()` → `Player.DropCurrentController(...)` ([`Player.cs:31690-31693`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L31690)) → `HandsController.Drop(1f, callback, fastDrop, nextControllerItem)` → cai exatamente no `FirearmController.Drop` acima. Confirmado nesta sessão que `ObservedFirearmController` (`mods/FIKA/modded/Fika-Plugin/Fika.Core/Main/ObservedClasses/HandsControllers/ObservedFirearmController.cs`) **não sobrescreve `Drop`** — o Headless roda exatamente o mesmo código-fonte, com seu próprio timer de animação/rede, para o jogador observado.
- **Por que colide:** `SPT-ContinuousLoadAmmo` (`mods/SPT-ContinuousLoadAmmo/modded/Controllers/LoadAmmoController.cs:511-538`) chama `SetEmptyHands` no CLIENTE local e prossegue com base no callback/estado local (`IsLoadAmmoAnimActiveOrPending`, `HandsIsEmpty`) — sem nenhum conhecimento de quando o Headless (rodando sua própria cópia server-authoritative da mesma sequência, sujeita a latência de rede e ao próprio timer de animação do servidor) efetivamente chama `.Confirm()` no `GEventArgs10` dele. Quando o próximo carregador do lote dispara um novo `SetEmptyHands`/`TrySetLastEquippedWeapon` antes do Headless ter confirmado o anterior, `CheckItemAction` no Headless encontra a arma ainda com um `GEventArgs10` `Begin` pendente e rejeita.
- **Por que é seguro tolerar (sem risco de concorrência real entre jogadores):** `List_0` é um campo de instância do `TraderControllerClass`/`ObservedInventoryController` — **um por jogador**. Qualquer evento encontrado no `foreach (var geventArgs2 in List_0)` dentro de `CheckItemAction` já pertence, por construção, ao mesmo jogador que está sendo validado agora. Não existe cenário em que um `GEventArgs9`/`GEventArgs10` de OUTRO jogador apareça aqui — a mesma garantia estrutural que os itens 003/004 já usam para a tabela de correlação do `GEventArgs17`.

### 1.2 Estratégia revisada (Fix 1a + Fix 1b + Fixes 2/3 inalterados)

Quatro correções independentes, nenhuma usando Harmony em alvo virtual/ofuscado por nome literal exceto onde inevitável (Fix 1b, ver nota AP-09 em §2):

1. **Fix 1a — `IsSelfReferentialHandsTransition`** (mantido do desenho original, `ObservedInventoryController.cs:218-237`) — remover a restrição `item is not MagazineItemClass`, mantendo o bloqueio real (`item == weapon && movedItem == weapon`). Continua tendo valor **próprio**: cobre o corner case cura→faca/granada/troca-de-arma da spec funcional (que passa pelo pipeline `GEventArgs17`, não pelo `GEventArgs10`) — só não resolve, sozinho, o bug relatado do `ContinuousLoadAmmo`.
2. **Fix 1b (NOVO) — tolerância de auto-colisão para `GEventArgs9`/`GEventArgs10`** — novo Harmony Postfix em `TraderControllerClass.method_19(GEventArgs1 args)` (o hub confirmado de Add/Remove de `List_0` para **todos** os tipos de evento — `RaiseInOutProcessEvents`/`GEventArgs17` e `RaiseEvent(GEventArgs13)` já confirmadamente delegam pra ele, `Player.cs:1887` e `:1969-1971`), filtrando só `GEventArgs9`/`GEventArgs10`, que registra `(controller, item, timestamp)` num `ConditionalWeakTable` novo e independente do `InOutHandsProcessTimestampPatch` do item 003 (não compartilha estado — mecanismo diferente, sem motivo pra acoplar). `ObservedInventoryController.CheckItemAction` ganha uma nova exceção escopada no bloco genérico `item == geventArgs2.Item` (linha 182 hoje): tolera quando `geventArgs2` é `GEventArgs9`/`GEventArgs10` **e** a entrada de correlação mostra que o evento é recente. **Janela própria, não reaproveitada:** `GraceWindowSeconds` (0.35f, item 003/004) limita a sobreposição quase instantânea de duas metades de um swap de carregador; aqui a janela precisa cobrir uma animação inteira de guardar/sacar arma (tipicamente > 1s) — nova constante `HandsBookkeepingGraceWindowSeconds` (2.5f, teto conservador, `TODO confirmar` por instrumentação, mesmo espírito de `P-3.1` mas item separado).
3. **Fix 2 — Airbag central de pacote desconhecido** — inalterado no desenho, **com a correção de `PA-01-02`**: `catch (Exception ex)` em vez de `catch (ParseException ex)`, pra fechar também a causa raiz 5 do guia canônico (assimetria `Serialize`/`Deserialize` de terceiro mal implementado), não só hash desconhecido.
4. **Fix 3 — Logging nos dois `catch (Exception) { }` vazios** de `ClientInventoryOperationHandler.cs` — inalterado.

## 2. Pontos de patch

Um novo Harmony `ModulePatch` (Fix 1b); os demais são edições diretas em código já pertencente ao fork:

| Alvo (código do mod / EFT p/ contexto) | Tipo de mudança | Motivo |
|---|---|---|
| [`ObservedInventoryController.cs:220`](../modded/Fika-Plugin/Fika.Core/Main/ObservedClasses/ObservedInventoryController.cs#L220) | Edição de guarda condicional | Fix 1a — remove restrição a `MagazineItemClass`, mantém bloqueio de saque real |
| [`TraderControllerClass.method_19`](../../../../references/eft-decompiled/Assembly-CSharp/TraderControllerClass.cs#L1822) — `method_19(GEventArgs1 args)` | Postfix (novo) | Fix 1b — registrar `(controller, item, timestamp)` no `Begin` de `GEventArgs9`/`GEventArgs10`; limpar no `Succeed`/`Failed`. **Nome obfuscado resolvido por string literal (`"method_19"`), não por assinatura — ver nota AP-09 abaixo.** |
| [`ObservedInventoryController.cs:182-188`](../modded/Fika-Plugin/Fika.Core/Main/ObservedClasses/ObservedInventoryController.cs#L182) — bloco `if (item == geventArgs2.Item)` | Edição direta (mod source) | Fix 1b — tolerar quando `geventArgs2` é `GEventArgs9`/`GEventArgs10` recente do mesmo jogador/item, via nova correlação |
| [`FikaClient.cs:500`](../modded/Fika-Plugin/Fika.Core/Networking/FikaClient.cs#L500) | Substituição de chamada (`ReadAllPackets` → `TryReadAllPackets`) + novo método privado | Fix 2 — airbag central com throttle (AP-11), `catch (Exception)` (correção `PA-01-02`) |
| [`FikaServer.cs:946`](../modded/Fika-Plugin/Fika.Core/Networking/FikaServer.cs#L946) | Idem, espelhado | Idem, lado servidor/Headless |
| [`ClientInventoryOperationHandler.cs:63-65`](../modded/Fika-Plugin/Fika.Core/Main/ClientClasses/ClientInventoryOperationHandler.cs#L63-L65) | `catch` vazio → log | Fix 3 — não mascarar falha silenciosa de `RaiseRefreshEvent` |
| [`ClientInventoryOperationHandler.cs:114-116`](../modded/Fika-Plugin/Fika.Core/Main/ClientClasses/ClientInventoryOperationHandler.cs#L114-L116) | `catch` vazio → log | Fix 3 — não mascarar falha silenciosa de `Operation.Dispose()` |

**Nota AP-09 sobre o alvo do Fix 1b (`method_19`):** é um método obfuscado (nome `method_NN`, não coberto pela tabela de deofuscação — que cobre só tipos, não membros). Diferente do `GEventArgs17`, não existe um wrapper público com nome real que delegue especificamente pra `GEventArgs9`/`GEventArgs10` (`RaiseEvent(GEventArgs13 args) { method_19(args); }`, [`TraderControllerClass.cs:1969-1971`](../../../../references/eft-decompiled/Assembly-CSharp/TraderControllerClass.cs#L1969), e `RaiseInOutProcessEvents` para `GEventArgs17` confirmam o padrão de delegação, mas nenhum overload nomeado equivalente existe para `GEventArgs9`/`GEventArgs10` — o call site em `Class1311`/`Class1312.vmethod_0/1` resolve por um caminho que não foi possível fechar 100% nesta sessão). Resolver por string literal (`AccessTools.Method(typeof(TraderControllerClass), "method_19")`) é a mesma prática já usada em `TRL-ImmersiveCombatMedicine` (`method_9`, `method_5`, `method_8`) — aceita neste ecossistema para membros sem nome real, mas frágil a renumeração entre builds do EFT. Mitigação: **validar em runtime** que o `MethodBase` resolvido tem a assinatura exata `void method_19(GEventArgs1)` antes de habilitar o patch (falhar alto/logar erro, nunca patchear silenciosamente o método errado) — ver stub §5.2.

**⚠️ Achado durante a redação desta spec (verificado por investigação dedicada, não suposição):** `GetTargetMethod()` retornando `null` **lança uma exceção** (`PatchException`/equivalente) dentro do `Enable()` do `ModulePatch` — não é um "falhar graciosamente" por si só. O comportamento depois disso depende de **como o patch é registrado**: (a) via `_patchManager.EnablePatches()` (auto-discovery em massa, `FikaPlugin.cs:180`) — cada patch é habilitado dentro do próprio `try/catch` do `PatchManager`, uma falha não derruba os outros; (b) via `_patchManager.EnablePatch(new X())` individual (o padrão usado pelos 2 patches aninhados do item 003, `FikaPlugin.cs:186-187`, **sem** `try/catch` no call site) — uma exceção aqui propaga **sem proteção** e pode abortar o resto do `Awake()`, derrubando o plugin inteiro por causa de UM patch. Como `RecordHandsBookkeepingEvent` é uma classe aninhada (mesma situação do item 003 — auto-discovery não confirmado pra tipos aninhados, CR-01-01 do item 003), o registro **precisa** ser explícito — então o `try/catch` precisa ficar no **call site do registro em `FikaPlugin.cs`**, não só dentro do `GetTargetMethod()`. Ver stub §5.3 (novo) e checklist §8.

Contexto de referência no Assembly (para provar que o bloqueio residual continua correto — não são pontos de patch, são evidência):
- [`Player.cs:32223`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L32223) — `TryRemoveFromHands`: confirma que um saque real nunca abre `Begin`/`Succeed` por esse lado (desvia via `SetControllerInsteadRemovedOne`, `:32242`).
- [`Player.cs:32294`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L32294) — `TrySetInHands`: ramo B (`:32294-32337`) é o único que levanta `Begin` com `.Item = HandsController.Item` (o item que **já** ocupa as mãos) — confirma que `item == weapon && movedItem == weapon` só ocorre por reinserção da própria arma (Fold ou reentrada da mesma referência), nunca por um saque de arma diferente. (Contexto do Fix 1a.)
- [`Player.cs:13506-13524`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L13506) — `FirearmController.Drop`: origem confirmada do `GEventArgs10` pendente no cenário `ContinuousLoadAmmo`. (Contexto do Fix 1b, §1.1.)

## 3. Novas propriedades F12 (BepInEx)

N/A — este item não introduz nenhuma `ConfigEntry`.

## 4. Arquivos do mod

| Arquivo | Ação | Resumo |
|---|---|---|
| `modded/Fika-Plugin/Fika.Core/Main/ObservedClasses/ObservedInventoryController.cs` | MODIFICAR | Fix 1a: `IsSelfReferentialHandsTransition` generalizada pro lado `item` (linha 220). Fix 1b: novo bloco de tolerância em `CheckItemAction` (linha 182-188) |
| `modded/Fika-Plugin/Fika.Core/Main/Patches/InventoryPatches/HandsBookkeepingTimestampPatch.cs` | CRIAR | Fix 1b: novo Postfix em `method_19`, correlação `(controller, item) → timestamp` para `GEventArgs9`/`GEventArgs10` |
| `modded/Fika-Plugin/Fika.Core/Networking/FikaClient.cs` | MODIFICAR | Fix 2: novo método privado `TryReadAllPackets` (`catch (Exception)`) + troca do call-site em `OnNetworkReceive` |
| `modded/Fika-Plugin/Fika.Core/Networking/FikaServer.cs` | MODIFICAR | Fix 2: idem, espelhado |
| `modded/Fika-Plugin/Fika.Core/Main/ClientClasses/ClientInventoryOperationHandler.cs` | MODIFICAR | Fix 3: 2 blocos `catch` vazios ganham log |
| `modded/Fika-Plugin/Fika.Core/FikaPlugin.cs` | MODIFICAR | Registro explícito e **protegido por try/catch** de `HandsBookkeepingTimestampPatch.RecordHandsBookkeepingEvent` (stub §5.3) + bump SemVer (seguir convenção dos itens 001-005) |
| `mod.json` | MODIFICAR | Bump do componente `plugin` em paralelo |

## 5. Stubs de código

### 5.1 Fix 1a — `IsSelfReferentialHandsTransition` — guarda generalizada

```csharp
// modded/Fika-Plugin/Fika.Core/Main/ObservedClasses/ObservedInventoryController.cs
// (método já existe nesta classe — mostrando o corpo completo pós-mudança, linhas 218-237 hoje)

// ref: AUD-01-01 (docs/relatorio-auditoria-codigo-01.md) — remove a restrição a MagazineItemClass
// que o item 004 não tinha tocado. Continua tolerando só transições recentes (GraceWindowSeconds)
// do MESMO jogador na MESMA arma, e continua bloqueando o único caso que deve permanecer rejeitado:
// a própria arma reentrando nas mãos enquanto o Begin pendente também foi aberto por ela mesma
// (Fold de coronha, ou saque real concorrente — ver Player.cs:32294-32337 na spec técnica §2).
private bool IsSelfReferentialHandsTransition(Item item, GEventArgs17 inOutHandsProcess)
{
    if (inOutHandsProcess?.Item is not Weapon weapon)
    {
        return false;
    }

    if (!InOutHandsProcessTimestampPatch.TryGetPendingBegin(this, weapon, out var movedItem, out var elapsed))
    {
        return false;
    }

    // TODO confirmar (P-3.1, herdada do item 003): janela de graça calibrada por
    // instrumentação temporária antes de fechar o item.
    const float GraceWindowSeconds = 0.35f;

    if (movedItem == null || elapsed > GraceWindowSeconds)
    {
        return false;
    }

    // Único cenário que deve continuar bloqueado: a própria arma reentrando nas mãos
    // enquanto o Begin pendente também foi aberto por ela mesma (Fold, ou saque real
    // concorrente — ver Player.cs:32294-32337).
    if (item == weapon && movedItem == weapon)
    {
        return false;
    }

    return true;
}
```

### 5.2 Fix 1b — novo patch de correlação (`HandsBookkeepingTimestampPatch.cs`)

```csharp
// modded/Fika-Plugin/Fika.Core/Main/Patches/InventoryPatches/HandsBookkeepingTimestampPatch.cs
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using EFT;
using EFT.InventoryLogic;
using Fika.Core.Main.Utils;
using HarmonyLib;
using SPT.Reflection.Patching;
using UnityEngine;

namespace Fika.Core.Main.Patches.InventoryPatches;

/// <summary>
/// Correlaciona o Begin/Succeed de GEventArgs9 ("BeginSetInHands", Class1311) e GEventArgs10
/// ("BeginRemoveFromHands", Class1312) com um timestamp, por (controller, item). Consumido por
/// ObservedInventoryController.CheckItemAction para tolerar, dentro de uma janela curta, uma
/// colisão genérica (item == geventArgs2.Item) contra um evento de bookkeeping recente do MESMO
/// jogador no MESMO item — nunca concorrência real (List_0 já é escopado por jogador, item 004
/// §1.2) nem qualquer outro tipo de evento (GEventArgs2/3/7/8/17, que continuam bloqueando como
/// hoje). Mecanismo INDEPENDENTE de InOutHandsProcessTimestampPatch (item 003) — GEventArgs9/10
/// não têm o conceito de "item efetivamente movido" que GEventArgs17 tem (o item já É a própria
/// chave); ver spec técnica do item 006 §1.1.
///
/// Causa raiz que este patch endereça: FirearmController.Drop (Player.cs:13506-13524) abre um
/// GEventArgs10 Begin ANTES da animação de esconder a arma começar e só confirma quando ela
/// termina — usado por SetEmptyHands (Player.cs:31704), chamado pelo SPT-ContinuousLoadAmmo entre
/// cada carregador. O cliente local avança pro próximo carregador com base no SEU PRÓPRIO timing;
/// o Headless roda sua própria cópia dessa mesma animação/confirmação, sujeita a latência de rede
/// — se o próximo pedido chega no Headless antes do Confirm() anterior, a arma ainda tem um
/// GEventArgs10 pendente e o pedido é rejeitado (GClass1561).
/// </summary>
public static class HandsBookkeepingTimestampPatch
{
    // ref: Assembly-CSharp/TraderControllerClass.cs:1822 — method_19(GEventArgs1 args), hub
    // confirmado de Add/Remove de List_0 para TODOS os tipos de evento (RaiseInOutProcessEvents e
    // RaiseEvent(GEventArgs13) delegam pra ele — TraderControllerClass.cs:1887 e :1969-1971).
    private static readonly ConditionalWeakTable<TraderControllerClass, System.Collections.Generic.Dictionary<Item, float>> _state = new();

    /// <summary>Se há um evento de bookkeeping (GEventArgs9/10) pendente pra esse item, há quanto tempo.</summary>
    public static bool TryGetPendingTimestamp(TraderControllerClass controller, Item item, out float elapsedSeconds)
    {
        elapsedSeconds = 0f;
        if (item == null || !_state.TryGetValue(controller, out var perItem) || !perItem.TryGetValue(item, out var timestamp))
        {
            return false;
        }

        elapsedSeconds = Time.time - timestamp;
        return true;
    }

    public class RecordHandsBookkeepingEvent : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            // ref: Assembly-CSharp/TraderControllerClass.cs:1822 — method_19(GEventArgs1 args).
            // TODO confirmar (AP-09): nome obfuscado sem wrapper nomeado equivalente ao
            // RaiseInOutProcessEvents pra GEventArgs9/10 (ver spec técnica §2). Validação de
            // assinatura abaixo evita patchear silenciosamente o método errado se renumerado.
            var candidates = AccessTools.GetDeclaredMethods(typeof(TraderControllerClass))
                .Where(m => m.Name == "method_19"
                    && m.GetParameters().Length == 1
                    && m.GetParameters()[0].ParameterType == typeof(GEventArgs1)
                    && m.ReturnType == typeof(void))
                .ToList();

            if (candidates.Count != 1)
            {
                FikaGlobals.LogError(
                    $"HandsBookkeepingTimestampPatch: esperava exatamente 1 candidato pra " +
                    $"TraderControllerClass.method_19(GEventArgs1), achou {candidates.Count}. " +
                    "Patch NÃO habilitado — Fix 1b do item 006 fica inerte até isso ser corrigido.");
                return null;
            }

            return candidates[0];
        }

        [PatchPostfix]
        public static void Postfix(TraderControllerClass __instance, GEventArgs1 args)
        {
            if (args is not (GEventArgs9 or GEventArgs10) || args.Item == null)
            {
                return;
            }

            if (args.Status == CommandStatus.Begin)
            {
                _state.GetOrCreateValue(__instance)[args.Item] = Time.time;
            }
            else if (_state.TryGetValue(__instance, out var perItem))
            {
                perItem.Remove(args.Item);
            }
        }
    }
}
```

```csharp
// Edição em modded/Fika-Plugin/Fika.Core/Main/ObservedClasses/ObservedInventoryController.cs
// Trecho ANTES (linha 182-188 atuais, dentro do foreach de CheckItemAction):
//
//     if (item == geventArgs2.Item)
//     {
// #if DEBUG
//         FikaGlobals.LogError($"{item.LocalizedShortName()} item was same as GEventArgs2.Item");
// #endif
//         flag = true;
//     }
//
// Trecho DEPOIS:

// ref: item 006 (Fix 1b) — tolera colisão genérica só quando geventArgs2 é um evento de
// bookkeeping (GEventArgs9/10) recente do PRÓPRIO jogador no MESMO item (nunca concorrência real
// — List_0 é por jogador). Qualquer outro tipo de evento (GEventArgs2/3/7/8/17, ou um GEventArgs9/
// 10 fora da janela de graça) continua marcando flag = true normalmente.
if (item == geventArgs2.Item && !IsRecentSelfHandsBookkeepingEvent(item, geventArgs2))
{
#if DEBUG
    FikaGlobals.LogError($"{item.LocalizedShortName()} item was same as GEventArgs2.Item");
#endif
    flag = true;
}

// Novo método privado de instância na mesma classe (mesmo padrão de IsSelfReferentialHandsTransition):
private bool IsRecentSelfHandsBookkeepingEvent(Item item, GEventArgs1 geventArgs)
{
    if (geventArgs is not (GEventArgs9 or GEventArgs10))
    {
        return false;
    }

    if (!HandsBookkeepingTimestampPatch.TryGetPendingTimestamp(this, item, out var elapsed))
    {
        return false;
    }

    // ref: item 006 §1.3 — NÃO reaproveita GraceWindowSeconds (0.35f) do item 003/004: aquela
    // constante limita a sobreposição quase instantânea das duas metades de UM swap de carregador.
    // Aqui a janela é o tempo real de uma animação de guardar/sacar arma inteira (GEventArgs9/10,
    // FirearmController.Drop → HideWeapon, Player.cs:13506-13524) — tipicamente > 1s no EFT,
    // variando por arma/perícia/postura. TODO confirmar (instrumentação temporária, mesmo padrão de
    // P-3.1): valor abaixo é um teto conservador de segurança, não calibrado por medição real ainda.
    const float HandsBookkeepingGraceWindowSeconds = 2.5f;
    return elapsed <= HandsBookkeepingGraceWindowSeconds;
}
```

### 5.3 Fix 1b — registro protegido em `FikaPlugin.cs`

```csharp
// modded/Fika-Plugin/Fika.Core/FikaPlugin.cs
// Junto dos outros _patchManager.EnablePatch(...) individuais (linhas 186-187 hoje, item 003):

// ref: item 006 — GetTargetMethod() de RecordHandsBookkeepingEvent pode retornar null (alvo
// obfuscado resolvido por predicado, sem garantia de encontrar exatamente 1 candidato — ver spec
// técnica §2 nota AP-09). Diferente dos patches do item 003 (alvo por nome real, nunca falha),
// aqui um Enable() com alvo null LANÇA uma exceção — sem este try/catch, isso abortaria o resto
// do Awake() e derrubaria o plugin inteiro por causa de um único patch defensivo.
try
{
    _patchManager.EnablePatch(new HandsBookkeepingTimestampPatch.RecordHandsBookkeepingEvent());
}
catch (Exception ex)
{
    FikaGlobals.LogError(
        "Falha ao habilitar HandsBookkeepingTimestampPatch.RecordHandsBookkeepingEvent — " +
        "Fix 1b do item 006 (tolerância GEventArgs9/10) fica inativo, resto do plugin segue normal: " +
        ex.Message);
}
```

### 5.4 Airbag central de pacote desconhecido (`FikaClient.cs`)

```csharp
// modded/Fika-Plugin/Fika.Core/Networking/FikaClient.cs
// Novo campo privado, junto dos outros campos de instância da classe:
private int _unknownPacketCount;

// Novo método privado:

// ref: AUD-01-02 (docs/relatorio-auditoria-codigo-01.md) + AP-11 (docs/technical/spt-antipatterns.md,
// "causa raiz 4" em docs/technical/fika-packet-desync-prevention-plan.md §2) — GetCallbackFromData
// (NetPacketProcessor.cs:83-91) lança ParseException sem barreira nenhuma até aqui quando o hash do
// pacote não está registrado (mod ausente no peer, ou registrado com atraso/versão diferente). Sem
// este catch, LiteNetManager.PollEvents (LiteNetManager.cs:1436-1441) descarta TODOS os eventos de
// rede já enfileirados no mesmo frame, de todos os peers — não só o pacote ruim. Throttle de log
// segue o padrão exigido pelo guia canônico (§4 regra 4): stack completo na 1ª ocorrência, resumo
// a cada N depois, para não inundar o console num mod desatualizado enviando em alta frequência.
// ref: PA-01-02 — catch (Exception), não só ParseException: NetPacketProcessor.ReadAllPackets/
// ReadPacket não têm try/catch interno nenhum, então também fecha a "causa raiz 5" do guia canônico
// (assimetria Serialize/Deserialize de um pacote de terceiro mal implementado usando Get* cru em vez
// de TryGet*, que lança em payload truncado) — não só hash nunca registrado.
private void TryReadAllPackets(NetDataReader reader, object userData)
{
    try
    {
        _packetProcessor.ReadAllPackets(reader, userData);
    }
    catch (Exception ex)
    {
        _unknownPacketCount++;
        if (_unknownPacketCount == 1 || _unknownPacketCount % 50 == 0)
        {
            _logger.LogWarning($"[CLIENT] Dropping malformed/unknown packet (#{_unknownPacketCount} so far): {ex.Message}");
        }
    }
}

// Call-site em OnNetworkReceive (FikaClient.cs:497-501), troca de uma linha:
public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channelNumber, DeliveryMethod deliveryMethod)
{
    switch (reader.GetEnum<EPacketType>())
    {
        case EPacketType.Serializable:
            TryReadAllPackets(reader, peer); // era: _packetProcessor.ReadAllPackets(reader, peer);
            break;
        // ... demais cases inalterados
    }
}
```

### 5.5 Espelho em `FikaServer.cs`

Mesma forma exata de `TryReadAllPackets` (com `catch (Exception ex)`, PA-01-02), campo `_unknownPacketCount` próprio (instância separada, sem estado compartilhado entre client e server), só troca a tag de log de `"[CLIENT]"` para `"[SERVER]"` (consistente com o padrão já usado em `_logger.LogError("[SERVER] error " + ...)`, `FikaServer.cs:793`). Call-site trocado em `FikaServer.cs:945-947` (dentro do mesmo `case EPacketType.Serializable:`).

### 5.6 Logging em `ClientInventoryOperationHandler.cs`

```csharp
// modded/Fika-Plugin/Fika.Core/Main/ClientClasses/ClientInventoryOperationHandler.cs
// Linhas 58-66 hoje (bloco TRL-Fixes #2, dentro de ReceiveStatusFromServer):
if (Operation is MoveOperationClass moveOp)
{
    try
    {
        moveOp.From?.Container?.ParentItem?.RaiseRefreshEvent(true, true);
        moveOp.To?.Container?.ParentItem?.RaiseRefreshEvent(true, true);
    }
    catch (Exception ex) // era: catch (Exception)
    {
        // ref: AUD-01-04 (docs/relatorio-auditoria-codigo-01.md)
        FikaGlobals.LogError($"{InventoryController?.ID} - RaiseRefreshEvent falhou após rejeição do servidor: {ex}");
    }
}

// Linhas 106-117 hoje (dentro de HandleResult):
if (Operation != null)
{
    try
    {
        Operation.Dispose();
    }
    catch (Exception ex) // era: catch (Exception)
    {
        // ref: AUD-01-04 (docs/relatorio-auditoria-codigo-01.md)
        FikaGlobals.LogError($"{InventoryController?.ID} - Operation.Dispose() falhou: {Operation?.Id} - {ex}");
    }
}
```

## 6. Fluxo de dados

**Fix 1a — tolerância `GEventArgs17` (cura/granada/faca/troca-de-arma → carregador ou item genérico):**
```
[A] Jogador equipa arma / saca faca / arremessa granada logo após outra transição de mãos recente
  → [B] Player.TrySetLastEquippedWeapon / HandsController.Execute → Item.CheckAction (Item.cs:657/664)
  → [C] owner.CheckItemAction → ObservedInventoryController.CheckItemAction (:70-209)
  → [D] IsSelfReferentialHandsTransition (:218-237) — agora tolera item != MagazineItemClass
  → [E] GClass1568 (sucesso) em vez de GClass1561 (rejeição "is currently being modified")
```

**Fix 1b — tolerância `GEventArgs9`/`GEventArgs10` (cenário real do `ContinuousLoadAmmo`):**
```
[A] SPT-ContinuousLoadAmmo (cliente local): terminou de municiar um carregador, chama
      Player.SetEmptyHands(...) de novo pro PRÓXIMO carregador — decisão baseada só no
      timing/estado LOCAL (LoadAmmoController.cs:511-538), sem saber se o Headless já confirmou
      o Drop anterior daquela mesma arma
[B] Cliente local: SetEmptyHands → Proceed → Process<>.Execute → DropCurrentController
      → HandsController.Drop (FirearmController.Drop, Player.cs:13506) → method_138(weapon)
      → GEventArgs10 Begin LOCAL (rápido — sem latência de rede a esperar)
[C] Headless (jogador observado, ObservedFirearmController — NÃO sobrescreve Drop): recebe o
      comando de rede equivalente, roda a MESMA sequência Drop → method_138(weapon) → GEventArgs10
      Begin, só que sujeita à latência de rede e ao timer de animação do PRÓPRIO servidor
      → HandsBookkeepingTimestampPatch.RecordHandsBookkeepingEvent grava (controller, weapon,
        timestamp=agora) [Fix 1b, novo]
[D] Se o Confirm() do Headless (dentro do callback onHidden de HideWeapon, Player.cs:13519-13524)
      ainda não rodou quando o PRÓXIMO SetEmptyHands/TrySetLastEquippedWeapon chega pro Headless
      (client já avançou, servidor ainda processando o anterior):
      → ObservedInventoryController.CheckItemAction roda pra esse novo pedido de equipar a arma
      → encontra o GEventArgs10 Begin ainda pendente pra mesma weapon em List_0 (:182-188)
      → SEM a correção: item == geventArgs2.Item → flag = true → GClass1561 → mão trava
      → COM a correção: IsRecentSelfHandsBookkeepingEvent confirma GEventArgs9/10 + elapsed <=
        HandsBookkeepingGraceWindowSeconds (2.5s) → colisão não é marcada → segue pra
        method_16/GClass1568 (sucesso)
[E] CENÁRIO AINDA PROTEGIDO: qualquer OUTRO tipo de evento em List_0 (GEventArgs2/3/7/8/17) ou um
      GEventArgs9/10 fora da janela de graça (Drop genuinamente travado/anormalmente longo, >
      2.5s — não o caso normal de uma animação de guardar arma) continua rejeitando normalmente —
      IsRecentSelfHandsBookkeepingEvent só isenta o caso específico deste fix
```

**Fix 2 — airbag de pacote desconhecido:**
```
[A] Peer remoto envia pacote de tipo X (registrado nele, ausente/desalinhado no receptor) OU um
      pacote malformado (Deserialize de terceiro usando Get* cru, payload truncado — PA-01-02)
  → [B] FikaClient/FikaServer.OnNetworkReceive → TryReadAllPackets
  → [C] NetPacketProcessor.ReadAllPackets → lança ParseException (hash desconhecido) OU qualquer
        outra exceção (Deserialize malformado — PA-01-02)
  → [D] catch (Exception) loga (com throttle) e retorna — não propaga
  → [E] LiteNetManager.PollEvents continua processando os demais eventos do mesmo lote/frame normalmente
```

## 7. Riscos e dependências

- **Patches existentes:** `InOutHandsProcessTimestampPatch` (item 003) **não é alterado** por este item — só o consumidor (`IsSelfReferentialHandsTransition`, Fix 1a) muda. `HandsBookkeepingTimestampPatch` (Fix 1b, novo) é independente, sem estado compartilhado com o mecanismo do item 003 — mesma razão de design: `GEventArgs9`/`GEventArgs10` não têm o conceito de "item efetivamente movido" que `GEventArgs17` tem.
- **Incerteza residual — é seguro tolerar a colisão de `GEventArgs9`/`GEventArgs10`?** A leitura estática confirma que `List_0` é escopado por jogador (sem risco de concorrência real entre jogadores diferentes — §1.1) e que `Class1311`/`Class1312` são **bookkeeping** (marcam "este item está em trânsito"), não o gate real de execução (`AbstractHandsController.CanExecute()`/`Execute()` são os gates reais que decidem se a operação de mãos em si pode prosseguir). Não foi possível confirmar por leitura estática, nesta sessão, se bypassar o bloqueio de `CheckItemAction` aqui pode deixar a operação avançar num estado que `CanExecute()` ainda rejeitaria de forma inconsistente (ex.: uma segunda operação sendo aceita pelo `CheckItemAction` mas depois falhando de um jeito diferente dentro do próprio `HandsController.Execute()`, produzindo um sintoma novo em vez do `GClass1561` atual). **Mitigado, não eliminado:** (a) o item de checklist §8 marcado bloqueador exige validação in-game repetida antes de fechar; (b) mesmo se algo ainda falhar ocasionalmente, o watchdog do item `002` (`FikaPlayer.WaitingForCallback`, timeout de 5s) continua como rede de segurança — não há regressão pra "trava permanente" mesmo no pior caso.
- **Mudança de comportamento observável em método público herdado (não é quebra de assinatura, mas é quebra de contrato comportamental potencial):** `CheckItemAction` é `public override` de `TraderControllerClass`/`Player.PlayerInventoryController` — já existia, sua assinatura não muda. Mas seu **comportamento** muda: uma operação que hoje é rejeitada (`GClass1561`) numa janela de ~0.35s após outra transição `GEventArgs17` (Fix 1a) **ou** numa janela de ~2.5s após um evento de bookkeeping `GEventArgs9`/`GEventArgs10` (Fix 1b — janela maior, cobre a duração de uma animação de guardar/sacar arma) no mesmo item passa a ser aceita, desde que não seja a própria arma reentrando (Fix 1a). Isso é o efeito **desejado** para os cenários reportados, mas **qualquer mod externo (presente no repo ou não) que chame `CheckItemAction` diretamente numa `ObservedInventoryController` e dependa da rejeição estrita anterior** veria esse comportamento mudar. Nenhum mod deste repo foi encontrado fazendo isso (`grep` não achou chamador externo a `CheckItemAction` em `mods/*/modded/`), mas como não há como garantir isso para mods desconhecidos, **isso é sinalizado aqui para decisão humana explícita antes do `/code-mod`**.
- **Compatibilidade com mods relacionados:** `SPT-ContinuousLoadAmmo` (gatilho direto do bug relatado — Fix 1b), `TRL-ImmersiveCombatMedicine` (gatilho do item 004 — Fix 1a), `UIFixes` (magazine swap, item 003) — nenhum precisa de mudança própria; todos se beneficiam das duas generalizações sem qualquer ação de sua parte.
- **Fragilidade do alvo `method_19` (Fix 1b, AP-09):** nome obfuscado sem wrapper público nomeado equivalente ao `RaiseInOutProcessEvents`. `GetTargetMethod` retorna `null` e loga erro se não achar exatamente 1 candidato, em vez de patchear o método errado silenciosamente — **mas retornar `null` por si só lança exceção dentro de `Enable()`** (achado verificado nesta sessão, não suposição — ver nota em §2). Por isso o registro em `FikaPlugin.cs` **precisa** do `try/catch` do stub §5.3: com ele, se um futuro update do EFT renumerar `method_19`, o patch fica inerte (log denuncia, resto do plugin segue normal) — sem ele, a mesma situação **derrubaria o `Awake()` inteiro**. Este risco só fica mitigado se o passo do checklist §8 (registro protegido) for aplicado exatamente como especificado — não é uma garantia automática do design do `ModulePatch`.
- **`TryReadAllPackets` não é API pública nova exposta a terceiros** — é `private` em `FikaClient`/`FikaServer`, não faz parte de `IFikaNetworkManager`. Mods externos que chamam `Singleton<IFikaNetworkManager>.Instance.SendData`/`RegisterPacket` (a API pública real de rede do FIKA) não são afetados.
- **Ordem de inicialização:** `HandsBookkeepingTimestampPatch.RecordHandsBookkeepingEvent` é uma classe aninhada — mesma situação do item 003 (auto-discovery do `_patchManager.EnablePatches()` não confirmada pra tipos aninhados, `CR-01-01` do item 003) — por isso o registro é **explícito e protegido por try/catch** (§5.3), não deixado pro auto-discovery em massa. As demais correções não introduzem novo registro/`Awake`/evento de ciclo de vida.
- **Regressão de rede:** rodar `node scripts/check-packet-hashes.js` após a mudança é parte do checklist de implementação (não deve haver diferença, já que nenhum tipo `INetSerializable` novo é criado por este item — é só validação de não-regressão).

## 8. Checklist de implementação

- [x] Editar `IsSelfReferentialHandsTransition` (`ObservedInventoryController.cs:220`) — remover a guarda `item is not MagazineItemClass`, manter e comentar o bloqueio `item == weapon && movedItem == weapon` (stub §5.1, Fix 1a).
- [x] Criar `HandsBookkeepingTimestampPatch.cs` (stub §5.2) — resolver `method_19` por predicado + validação de assinatura, Postfix filtrando `GEventArgs9`/`GEventArgs10`.
- [x] Editar `ObservedInventoryController.CheckItemAction` (linha 182-188 atuais) — novo método `IsRecentSelfHandsBookkeepingEvent` e a condição atualizada no bloco `item == geventArgs2.Item` (stub §5.2, Fix 1b).
- [x] Registrar `HandsBookkeepingTimestampPatch.RecordHandsBookkeepingEvent` em `FikaPlugin.cs` via `_patchManager.EnablePatch(...)` **dentro de um `try/catch` próprio** (stub §5.3) — obrigatório: (a) classe aninhada, mesmo caso do item 003 (auto-discovery não confirmado pra tipos aninhados), precisa de registro explícito; (b) sem o `try/catch`, um `GetTargetMethod()` retornando `null` (alvo obfuscado não encontrado) derruba o `Awake()` inteiro do plugin, não só este patch.
- [x] Adicionar campo `_unknownPacketCount` e método `TryReadAllPackets` (`catch (Exception)`, PA-01-02) em `FikaClient.cs`; trocar o call-site em `OnNetworkReceive` (stub §5.4).
- [x] Espelhar em `FikaServer.cs` com tag de log `"[SERVER]"` (stub §5.5).
- [x] Adicionar log em `ClientInventoryOperationHandler.cs:63-65` (stub §5.6).
- [x] Adicionar log em `ClientInventoryOperationHandler.cs:114-116` (stub §5.6).
- [x] Rodar `node scripts/check-packet-hashes.js` — confirmar 0 colisões (não deve mudar, é validação de não-regressão). **Confirmado: "✓ Nenhuma colisão de hash CRC-16" (85 tipos distintos, 34 do FIKA). Os 4 avisos de duplicata reportados são pré-existentes — Band-Aid vs. TRL-ImmersiveCombatMedicine, não relacionados a este item.**
- [x] Bump de versão SemVer em `FikaPlugin.cs` + `mod.json` (seguir convenção dos itens 001-005). `2.3.16` → `2.3.17`.
- [x] Compilar (`dotnet build -c Release`, **não** `/compile-mod` — regra do usuário, instala automaticamente no jogo) e confirmar 0 erros/avisos nos módulos afetados (`Fika.Core`, `Fika.Headless` se aplicável). **0 erros, 1 warning pré-existente (conflito de versão do `System.Runtime.CompilerServices.Unsafe`, não relacionado a este item). Build em `mods/FIKA/builds/Fika.Core-260911-0541.dll`. `Fika.Headless.dll` não recompilado nesta sessão — não há mudança em `Fika-Headless/`.**
- [x] **Validação in-game bloqueadora (fecha a incerteza de §7):** reproduzir o cenário original (municiar vários carregadores em sequência via `SPT-ContinuousLoadAmmo` em raid Headless real) e confirmar ausência de travamento em pelo menos 10-15 ciclos seguidos — **(ref: PA-02-02)** testar em pelo menos dois ritmos: municiamento rápido (clique repetido) e municiamento normal (esperando cada carregador terminar antes do próximo), confirmando que nenhum dos dois trava. **✅ Validado em 2026-09-11 pelo usuário, em raid Headless real com o build `Fika.Core-260911-0541.dll`: cancelou o municiamento no meio, encheu vários carregadores em sequência, esvaziou, encheu de novo um por um — nenhuma trava em nenhum ciclo. Cenário bloqueador fechado.**
- [ ] Validar in-game que uma colisão genuína continua bloqueada: dois jogadores tentando pegar a mesma arma física ao mesmo tempo (não uma transição de mãos própria) continua rejeitado com `GClass1561`. **Pendente — in-game, não-bloqueador (proteção estrutural já garantida por análise estática — `List_0` por jogador, §1.1).**
- [ ] Validar in-game o corner case cura→faca/granada/troca-de-arma (Fix 1a, ainda não tinha validação própria). **Pendente — in-game, não-bloqueador.**
- [ ] **(ref: PA-02-01)** Validar in-game algum cenário que dispare `GEventArgs9` especificamente (ex.: sacar uma arma repetidamente em sucessão rápida, sem passar pelo `ContinuousLoadAmmo`) e confirmar que a tolerância desse lado não introduz nenhum comportamento estranho — mesmo rigor dado ao cenário `GEventArgs10`, que foi o único empiricamente confirmado nesta sessão. **Pendente — in-game, não-bloqueador.**

## 9. Conformidade com skills (auto-checklist)

| # | Check | Status | Evidência / razão |
|---|---|---|---|
| 1 | Lifecycle de raid: start hook + stop hooks idempotentes — AP-01 | N/A | Nenhuma das 4 correções introduz estado raid-scoped novo. `IsSelfReferentialHandsTransition` (Fix 1a) reaproveita o `ConditionalWeakTable` já existente do item 003. `HandsBookkeepingTimestampPatch` (Fix 1b) usa seu **próprio** `ConditionalWeakTable<TraderControllerClass, ...>` — mesma garantia estrutural: chave = `TraderControllerClass`, coletado junto com o jogador/controller ao fim da raid, sem pin. `_unknownPacketCount` é um contador de instância (vida do plugin, não do raid) — não precisa de teardown. |
| 2 | Filtro MainPlayer/Fika em todo patch que reage a ação de player — AP-02 | ✅ | `ObservedInventoryController` só existe para jogadores observados/Headless, nunca para `MainPlayer` local (§1 desta spec, `ObservedInventoryController.cs:16`). `HandsBookkeepingTimestampPatch` é observação pura (grava timestamp pra QUALQUER `TraderControllerClass`, sem distinguir player — a única mudança de comportamento fica isolada em `ObservedInventoryController`, mesmo raciocínio do item 003). O airbag de pacote roda na camada de transporte, abaixo da distinção de jogador. |
| 3 | Alvos ofuscados/virtuais resolvidos por assinatura; overrides auditados — AP-03 | ✅ | `method_19` (Fix 1b) é **não-virtual** (instância direta de `TraderControllerClass`, sem override a auditar) — confirmado por `RaiseInOutProcessEvents`/`RaiseEvent(GEventArgs13)` delegarem pra ele sem `virtual`/`override` na assinatura (`TraderControllerClass.cs:1887`, `:1969`). Resolvido por predicado de assinatura + validação em runtime (não por nome/índice cego), ver §2 nota AP-09. |
| 4 | Mudança de estado via API canônica; side-effects mapeados — AP-04 | ✅ | Nenhuma das 4 correções escreve campo interno do EFT diretamente — Fix 1a/1b só ajustam uma condição booleana já existente (o `flag` de validação calculado pelo próprio motor); o airbag só envolve uma chamada já existente (`ReadAllPackets`) em try/catch; o logging só formata exceções já capturadas. |
| 5 | Estado entre raids: raid1→exit→raid2 e alt-F4/morte/MIA cobertos | ✅ | Ver check 1 — nenhum estado estático acumulando entre raids em nenhuma das 4 correções. `_unknownPacketCount` sobrevive entre raids por design (contador cumulativo de diagnóstico, não estado de gameplay) — aceitável e documentado no critério "estado entre raids" da spec funcional. |
| 6 | Semântica/defaults/faixas de cada ConfigEntry — AP-05 | N/A | Nenhuma `ConfigEntry` nova. |
| 7 | Reentry-guard em re-invocação de método patcheado — AP-07 | N/A | Nenhum Postfix/edição direta re-invoca o próprio método patcheado/editado; sem recursão possível. |
| 8 | Flags/caches de intercept validados contra contexto atual — AP-08 | ✅ | `HandsBookkeepingTimestampPatch` (Fix 1b): entrada é por `(controller, item)` exato e removida no `Succeed`/`Failed` — trocar de item/arma cria uma entrada nova, nunca reaproveita timestamp de um item anterior. |
| 9 | Todo patch-point reconfirmado no `.cs` do dump — AP-09 | ✅ | Todas as linhas citadas nesta spec (`ObservedInventoryController.cs`, `NetPacketProcessor.cs`, `FikaClient.cs`, `FikaServer.cs`, `ClientInventoryOperationHandler.cs`, `Player.cs:13506-13524`/`22234-22243`/`32371-32393`, `TraderControllerClass.cs:1822-1838`/`1887`/`1969-1971`) foram lidas diretamente nesta sessão via `Read`/`Grep`, incluindo reconfirmação empírica via log dirigido em raid real (§1.1) — não herdadas só do relatório de auditoria. Incerteza residual sobre `method_19` (nome obfuscado sem wrapper nomeado equivalente) documentada explicitamente em §2/§7, não afirmada como certeza sem prova. |
| 10 | Skill EFT usada como lever confirmada não-inerte — AP-10 | N/A | Não aplicável, item não usa skill do EFT. |
| 11 | Pacote FIKA próprio: envelope, `TryGet*`, `Valid`, main thread, registro por instância, zero `UnregisterPacket`, airbag com throttle — AP-11 | ✅ | Este item não cria pacote `INetSerializable` novo (não se aplica envelope/`TryGet*`/`Valid`/registro), mas implementa exatamente o requisito de **"airbag com throttle"** do guia (§4 regra 4) no ponto mais central possível — antes de qualquer callback de pacote específico rodar, fechando a "causa raiz 4" **e** a "causa raiz 5" (`catch (Exception)`, PA-01-02) do guia. `node scripts/check-packet-hashes.js` faz parte do checklist de implementação (§8) para confirmar não-regressão. |

## Histórico

| Data | Evento |
|---|---|
| 2026-09-10 | Spec técnica criada via `/create-technical-spec` |
| 2026-09-11 | Revisão `/review-technical-spec` 01 — `PA-01-01` (🔴) e `PA-01-02` (🟡) resolvidos: hipótese original rejeitada por evidência empírica direta (log dirigido em raid real); causa raiz real identificada (`GEventArgs9`/`GEventArgs10`, não `GEventArgs17`). Spec reescrita: Fix 1 dividido em Fix 1a (mantido, valor próprio pro corner case cura→faca/granada/arma) + Fix 1b (novo, endereça o bug relatado do `ContinuousLoadAmmo`). Fix 2 incorpora `catch (Exception)` (não só `ParseException`, `PA-01-02`). Duas correções adicionais feitas durante a própria redação (não geradas por rodada de review formal): (1) janela de graça do Fix 1b trocada de `GraceWindowSeconds` (0.35f, reaproveitada por engano do item 003/004) pra uma constante própria (`HandsBookkeepingGraceWindowSeconds`, 2.5f) — o evento que o Fix 1b tolera é limitado pela duração de uma animação inteira de guardar arma, não pela sobreposição quase instantânea de duas metades de swap de carregador; (2) `GetTargetMethod()` retornando `null` (alvo obfuscado não encontrado) foi verificado, por investigação dedicada no `references/spt-source/`, que **lança exceção** — o registro do novo patch em `FikaPlugin.cs` precisa de `try/catch` próprio (stub §5.3), senão uma falha nesse patch específico derrubaria o `Awake()` inteiro do plugin. |
| 2026-09-11 | Revisão `/review-technical-spec` 02 — 2 pontos (0🔴/1🟡/1🟢) resolvidos sobre o desenho reescrito: `PA-02-01` (validação in-game de `GEventArgs9` adicionada ao §8, já que só `GEventArgs10` foi empiricamente confirmado nesta sessão) e `PA-02-02` (cláusula de variação de ritmo adicionada ao item de validação principal). |
| 2026-09-11 | `/code-mod` executado — código implementado sem desvios em relação aos stubs (Fix 1a, Fix 1b, Fix 2, Fix 3). Build `dotnet build -c Release` verificado: 0 erros. `check-packet-hashes.js` confirmou 0 colisões. Ver `006-colisao-maos-item-nao-carregador-05-asbuild.md`. |
| 2026-09-11 | **Validação in-game bloqueadora confirmada pelo usuário** em raid Headless real (build `Fika.Core-260911-0541.dll`): municiamento cancelado no meio, vários carregadores em sequência, esvaziar e reencher um por um — nenhuma trava em nenhum ciclo. Item funcionalmente fechado; validações não-bloqueadoras restantes (concorrência entre jogadores, corner case de cura, cenário `GEventArgs9`) seguem como débito de validação, não como bloqueio. |
