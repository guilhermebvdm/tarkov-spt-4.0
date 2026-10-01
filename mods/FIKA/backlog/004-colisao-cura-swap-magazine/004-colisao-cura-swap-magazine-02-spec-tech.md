# 004 — Swap de carregador rejeitado após curar aliado (colisão com transição de mãos não-magazine) · Spec Técnica

**Mod:** FIKA
**Spec funcional:** [004-colisao-cura-swap-magazine-01-spec.md](004-colisao-cura-swap-magazine-01-spec.md)
**Criado:** 2026-09-08

> Fonte primária de verdade para qualquer assinatura, fórmula ou ponto de patch: [references/eft-decompiled/Assembly-CSharp/](../../../../references/eft-decompiled/Assembly-CSharp/). Toda referência ao código do EFT deve citar `arquivo.cs:linha`. Wiki SPT e fontes externas só como complemento.

**Memória consultada:** snapshot de 2026-09-06 (Sessão 3) de `mods/FIKA/memory/sessions.md` + entradas que citam o item `004` (nenhuma própria ainda — item novo) · pendências que afetam: `P-3.1` (calibração `GraceWindowSeconds`, 🟡 débito técnico — reaproveitada sem recalibrar, ver §7) e `P-3.2` (limpeza patch órfão UIFixes, 🟢 ideia — não relacionada). Cross-mod: `mods/UIFixes/memory/sessions.md` Sessão 9b (2026-09-08) contém o diagnóstico completo da causa raiz, reconfirmado nesta sessão por leitura direta do código (não só relato de memória — AP-09).
**Docs técnicos lidos (gatilho disparado):** `spt-antipatterns.md` (sempre — AP-03/AP-08/AP-09 relevantes ao diagnóstico e ao desenho do fix). `spt4-vs-spt41-gclass-deobfuscation.md` não re-disparado como leitura nova: os únicos `GClassNNNN`/`GEventArgsNN` citados (`GClass1561`, `GEventArgs17`) já foram resolvidos na spec técnica do item `003` (§0 dela) e são reutilizados aqui sem mudança de assinatura. `fika-packet-desync-prevention-plan.md` não disparado: este item não declara nem toca nenhum `INetSerializable`. `spt4-items-inventory-hideout.md` não disparado: o fix é validação de colisão de operação (`inOutHandsProcess`), não manipulação de árvore de item/grade/preset.

## 0. Aliases 4.1 (deofuscação — rótulo, não pinado — AP-09)

Reutilizados sem alteração da spec técnica do item `003` (já confirmados por leitura direta):

| Nome 4.0 | Alias 4.1 (`consolidated-mappings.txt`) |
|---|---|
| `GEventArgs17` | `EFT.InventoryLogic.InOutHandsProcessEventArgs` |
| `GClass1561` | `EFT.InventoryLogic.PlayerIsBusyError` |

## 1. Estratégia

### 1.1 Causa raiz (já diagnosticada em sessão anterior — reconfirmada nesta sessão por leitura direta)

O diagnóstico completo já está registrado em `mods/UIFixes/memory/sessions.md` (Sessão 9b, 2026-09-08). Reconfirmado aqui, linha a linha:

1. `TRLImmersiveCombatMedicine` finaliza toda cura (fim normal do `HealRoutine`, cancelamento, ou drop de emergência) chamando `MedicHealPatch.ForceFinishAnimation()` ([`BandAidController.cs:625`](../../../TRL-ImmersiveCombatMedicine/modded-V3(review)/Patches/Medical/BandAidController.cs#L625), também linhas 487/612/743/953 para os outros caminhos de saída). `ForceFinishAnimation` invoca via reflexão `method_9` de `Player.MedsController.ObservedMedsControllerClass` ([`MedicHealPatch.cs:208-225`](../../../TRL-ImmersiveCombatMedicine/modded-V3(review)/Patches/Medical/MedicHealPatch.cs#L208), alvo resolvido em `MedicHealPatch.cs:93-101`).
2. `method_9` é o cleanup nativo da operação de meds — confirmado em [`Player.cs:19640-19660`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L19640): desinscreve eventos e invoca `Callback_0.Invoke(MedsController_0)` (linha 19655-19657), o callback que devolve o `HandsController` ao estado anterior — na prática, reequipando a arma que o médico tinha antes de curar.
3. Esse reequipe da arma percorre o mesmo pipeline de transição "entrando/saindo das mãos" (`GEventArgs17`/`inOutHandsProcess`, `CommandStatus.Begin`/`Succeed`) que qualquer outra operação de mãos do motor — o mesmo pipeline que o item `003` já instrumentou com `InOutHandsProcessTimestampPatch` (ver §1.2). No caso observado (jogador A cura jogador B, representado no Host por `ObservedInventoryController`/`ObservedMedsController`), esse `Begin` é registrado para a **arma** de A, mas o item que efetivamente o abriu (`movedItem`, ver §1.2) é o **item de cura** (bandagem/CMS) — nunca um carregador.
4. Se, dentro da janela de graça (`GraceWindowSeconds`, hoje 0.35s), o jogador A arrasta um carregador sobre a própria arma (swap 1-para-1, recurso do `UIFixes`), `ObservedInventoryController.CheckItemAction` ([`ObservedInventoryController.cs:70-209`](Fika-Plugin/Fika.Core/Main/ObservedClasses/ObservedInventoryController.cs#L70), lido nesta sessão no mod) encontra esse `Begin` pendente na mesma arma e chama `IsSelfReferentialMagazineSwap` (linhas 216-232, lido nesta sessão) para decidir se tolera a colisão.
5. `IsSelfReferentialMagazineSwap` (fix do item `003`) só tolera quando `movedItem is MagazineItemClass` (linha 231). Como `movedItem` aqui é o item de cura — não um `MagazineItemClass` — a condição falha, a colisão é marcada (`flag = true`), e o método retorna `new GClass1561(...)` ([`ObservedInventoryController.cs:201`](Fika-Plugin/Fika.Core/Main/ObservedClasses/ObservedInventoryController.cs#L201)) rejeitando o swap com `PlayerIsBusyError` de verdade — o cliente já tinha aplicado a troca localmente (Swap otimista), gerando a divergência client/server que trava o inventário do jogador pelo resto da raid.

**Isto é uma lacuna de escopo do fix do item `003`, não um bug novo:** a infraestrutura de correlação (`InOutHandsProcessTimestampPatch`) já captura corretamente QUALQUER item que abriu o `Begin` — a limitação está inteiramente na condição de tolerância dentro de `IsSelfReferentialMagazineSwap`, escrita estreita demais (só reconhece a metade gêmea de uma troca de magazine).

### 1.2 Mecanismo de correlação existente (item 003 — reconfirmado, sem alteração necessária)

`mods/FIKA/modded/Fika-Plugin/Fika.Core/Main/Patches/InventoryPatches/InOutHandsProcessTimestampPatch.cs` (lido integralmente nesta sessão) já:

- Captura o item efetivamente movido (`_pendingMovedItem`) em dois pontos: (a) Prefix Harmony em [`Player.TryRemoveFromHands`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L32223) (`CaptureMovedItemOnRemove`, linhas 70-92) para o lado "sair das mãos"; (b) chamada direta `SetPendingMovedItem(item)` a partir de `ObservedInventoryController.HandleInProcess` (linha 322 do mod, `ObservedInventoryController.cs`) para o lado "entrar nas mãos" — `InProcess` é sobrescrito nesta classe e nunca passa por `Player.TrySetInHands` (desvio já documentado no item 003).
- Registra `(controller, arma) → (movedItem, timestamp)` num `ConditionalWeakTable<TraderControllerClass, Dictionary<Item, Entry>>` (`_state`, linha 40) via Postfix em [`TraderControllerClass.RaiseInOutProcessEvents`](../../../../references/eft-decompiled/Assembly-CSharp/TraderControllerClass.cs#L1887) (`RecordBeginSucceed`, linhas 94-126): grava no `Begin`, remove no `Succeed`.
- **Já é inerentemente escopado por jogador e por arma**: a chave externa do `ConditionalWeakTable` é o `TraderControllerClass __instance` recebido no Postfix — ou seja, cada `ObservedInventoryController`/jogador tem sua própria entrada; não há como um `Begin` de um jogador aparecer na consulta de outro. Isso significa que **generalizar o `movedItem` aceito não introduz nenhum risco de tolerar colisão entre jogadores diferentes** — essa proteção nunca dependeu do tipo de `movedItem`, é estrutural ao dicionário.

**Conclusão:** nenhuma mudança é necessária em `InOutHandsProcessTimestampPatch.cs`. A captura já é genérica (qualquer `Item`, não só magazine) — a única lacuna está no consumidor (`IsSelfReferentialMagazineSwap`).

### 1.3 O que de fato precisa ser bloqueado (auditoria do "cenário protegido")

Antes de simplesmente remover a checagem `is MagazineItemClass`, é preciso confirmar o que ela protegia de verdade. Lendo `Player.TryRemoveFromHands` ([`Player.cs:32223-32263`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L32223)) nesta sessão:

- Quando o item removido **é literalmente o item já equipado nas mãos** (`HandsController.Item == item`, linha 32234), o método desvia para `SetControllerInsteadRemovedOne` (linha 32242) — que chama `TrySetLastEquippedWeapon` e **nunca levanta `GEventArgs17`/`Begin`**. Ou seja: um saque/guarda **completo** da arma (holster real) via este caminho **não gera entrada alguma** no dicionário de correlação — não é o cenário que colide.
- O ramo que efetivamente levanta `Begin`/`Succeed` (linha 32245-32257, `else if (HandsController.CanExecute(...))`) só é alcançado quando `item != HandsController.Item` — por construção, `movedItem` (o `item` capturado) e a arma-chave (`HandsController.Item`) **nunca são o mesmo objeto** nesse método. `method_35` ([`Player.cs:1471-1495`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L1471), reconfirmado) só devolve a própria arma como `item2` quando ela É o `ItemInHands` — que é exatamente o caso capturado pelo desvio acima.
- No lado "entrar" (`Player.TrySetInHands`, [`Player.cs:32294-32337`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L32294), e o override equivalente `ObservedInventoryController.HandleInProcess`), **não existe** o mesmo desvio — não há um `if (item == HandsController.Item)` especial. Portanto, teoricamente, se um jogador estivesse **sacando uma arma nova** (a própria arma entrando nas mãos, ainda sem outro item equipado) na mesma janela de um swap de carregador tentado nessa arma, `movedItem` poderia ser registrado como a própria arma.

**Decisão de design:** manter `movedItem != weapon` (identidade de referência, não `MagazineItemClass`) como o critério de bloqueio residual. Isso preserva por construção qualquer cenário em que a própria arma-alvo é o item que abriu a transição (saque real dessa arma) — o único caso em que `movedItem == weapon` pode acontecer, dado o comportamento assimétrico de `TryRemoveFromHands` vs `TrySetInHands` mapeado acima — e amplia a tolerância para **qualquer outro item** (carregador, item de cura, granada, faca, item de reanimação) que tenha aberto a transição na mesma arma, do mesmo jogador, dentro da janela de graça. Ver §8 para o item de validação in-game que fecha a incerteza residual sobre esse ramo específico (não há assembly disponível que prove em runtime, sem reproduzir, se `HandsController.Item` no momento exato do `Begin` de um saque de arma nova já reflete a arma entrando — só o teste em raid fecha isso com certeza).

### 1.4 Caminhos de correção descartados (com motivo)

- **Bypass amplo por tipo de item de cura (`item2 is MedsItemClass`/similar), replicando o padrão restrito de `003`.** Descartado: resolveria só o caso da cura, deixando granada/faca/reanimação com o mesmo bug (a spec funcional já generaliza o corner case). Cobrir caso a caso não escala e é o mesmo erro de design que originou este item.
- **Remover completamente a checagem `inOutHandsProcess` para o caso "mesma arma, mesmo jogador".** Descartado: eliminaria também a proteção real contra um saque de arma genuíno colidindo com um swap de carregador na mesma janela (corner case já coberto pela spec funcional/item 003), que o critério `movedItem != weapon` continua bloqueando.
- **Nova janela de graça independente para o caso "não-magazine".** Descartado: não há evidência de que o timing do cleanup de cura difira do timing already calibrado para o swap 1-para-1 (ambos passam pelo mesmo pipeline `Begin→Execute→Succeed`); duas constantes fariam duas coisas divergirem sem motivo. Reaproveita `GraceWindowSeconds` existente (0.35s, `TODO confirmar` herdado — `P-3.1`, fora de escopo aqui).

### 1.5 Abordagem escolhida

Edição pontual, só em `mods/FIKA/modded/Fika-Plugin/Fika.Core/Main/ObservedClasses/ObservedInventoryController.cs`, sem nenhum novo Harmony patch:

1. **Renomear** `IsSelfReferentialMagazineSwap` → `IsSelfReferentialHandsTransition` — o nome antigo fica enganoso (o método agora também tolera cura/granada/faca/reanimação, não só magazine).
2. **Broadening da condição de tolerância**: trocar `movedItem is MagazineItemClass` por `movedItem != null && movedItem != weapon` (comparação por referência, consistente com `item == geventArgs2.Item` já usado no mesmo arquivo, linha 182).
3. **Manter intocado** o resto do método: `item is not MagazineItemClass` continua exigido no item **sendo validado agora** (o parâmetro `item`) — este item não amplia o escopo para tolerar troca de OUTROS tipos de item além de carregador; só amplia o que pode ter aberto o `Begin` que colide com essa troca. Manter `TryGetPendingBegin`/`GraceWindowSeconds` sem alteração.

## 2. Pontos de patch

| Alvo (Assembly/mod) | Tipo | Motivo |
|---|---|---|
| [`ObservedInventoryController.cs:216-232`](Fika-Plugin/Fika.Core/Main/ObservedClasses/ObservedInventoryController.cs#L216) — `IsSelfReferentialMagazineSwap` | Edição direta (mod source) | Renomear + trocar `movedItem is MagazineItemClass` por `movedItem != null && movedItem != weapon` |
| [`ObservedInventoryController.cs:174`](Fika-Plugin/Fika.Core/Main/ObservedClasses/ObservedInventoryController.cs#L174) — chamada dentro de `CheckItemAction` | Edição direta (mod source) | Atualizar o nome do método chamado (rename) |

Nenhum ponto de patch no Assembly do EFT é tocado — `Player.cs:32223`/`TraderControllerClass.cs:1887` (usados por `InOutHandsProcessTimestampPatch`) já estão cobertos pelo item `003` e não mudam.

## 3. Novas propriedades F12 (BepInEx)

N/A — nenhuma `ConfigEntry` nova. Reaproveita a constante interna `GraceWindowSeconds` existente (item 003, `P-3.1` continua aberta e fora de escopo aqui).

## 4. Arquivos do mod

| Arquivo | Ação | Resumo |
|---|---|---|
| `mods/FIKA/modded/Fika-Plugin/Fika.Core/Main/ObservedClasses/ObservedInventoryController.cs` | MODIFICAR | Renomear `IsSelfReferentialMagazineSwap` → `IsSelfReferentialHandsTransition`; broadening da condição de tolerância (§1.5) |
| `mods/FIKA/modded/Fika-Plugin/Fika.Core/FikaPlugin.cs` | MODIFICAR | Bump `FikaVersion` (`2.3.14` → `2.3.15`) |
| `mods/FIKA/mod.json` | MODIFICAR | Bump do componente `plugin` (`2.3.14` → `2.3.15`) |

## 5. Stubs de código

```csharp
// Edição em mods/FIKA/modded/Fika-Plugin/Fika.Core/Main/ObservedClasses/ObservedInventoryController.cs
// Trecho ANTES (linhas 162-181 atuais, dentro de CheckItemAction):
//
//     lambda.inOutHandsProcess = geventArgs2 as GEventArgs17;
//     if (lambda.inOutHandsProcess != null)
//     {
//         bool collidesViaHands =
//             item.GetAllParentItemsAndSelf(false).Any(lambda.method_1) ||
//             location?.Container.ParentItem.GetAllParentItemsAndSelf(false).Any(lambda.method_1) == true;
//
//         // ref: PA-01-02 — só a metade "gêmea" da mesma troca de magazine em andamento é
//         // isenta (o item que abriu o Begin pendente precisa ser, ele também, um
//         // MagazineItemClass; um saque/guarda de arma grava a própria arma como item
//         // movido e nunca satisfaz essa condição). Qualquer outra colisão via
//         // inOutHandsProcess continua bloqueada.
//         if (collidesViaHands && !IsSelfReferentialMagazineSwap(item, lambda.inOutHandsProcess))
//         {
// #if DEBUG
//             FikaGlobals.LogError($"{item.LocalizedShortName()} failed inOutHandsProcess check (not a self-referential magazine swap)");
// #endif
//             flag = true;
//         }
//     }
//
// Trecho DEPOIS (blocos #if DEBUG preservados, só a mensagem de log e o nome do método mudam):

lambda.inOutHandsProcess = geventArgs2 as GEventArgs17;
if (lambda.inOutHandsProcess != null)
{
    bool collidesViaHands =
        item.GetAllParentItemsAndSelf(false).Any(lambda.method_1) ||
        location?.Container.ParentItem.GetAllParentItemsAndSelf(false).Any(lambda.method_1) == true;

    // ref: item 004 (colisão cura + swap magazine) — generaliza o fix do item 003 (PA-01-02):
    // tolera QUALQUER transição de mãos recente na MESMA arma/MESMO jogador (carregador,
    // item de cura, granada, faca, reanimação) que não seja, ela própria, um saque/guarda
    // real dessa arma (movedItem == weapon, único caso em que Player.TrySetInHands pode
    // gravar a própria arma como item movido — Player.TryRemoveFromHands nunca gera esse
    // caso, ver spec técnica do item 004 §1.3).
    if (collidesViaHands && !IsSelfReferentialHandsTransition(item, lambda.inOutHandsProcess))
    {
#if DEBUG
        FikaGlobals.LogError($"{item.LocalizedShortName()} failed inOutHandsProcess check (not a self-referential hands transition)");
#endif
        flag = true;
    }
}
```

```csharp
// Edição em mods/FIKA/modded/Fika-Plugin/Fika.Core/Main/ObservedClasses/ObservedInventoryController.cs
// Trecho ANTES (linhas 211-232 atuais):
//
//     private bool IsSelfReferentialMagazineSwap(Item item, GEventArgs17 inOutHandsProcess)
//     {
//         if (item is not MagazineItemClass || inOutHandsProcess?.Item is not Weapon weapon)
//         {
//             return false;
//         }
//
//         if (!InOutHandsProcessTimestampPatch.TryGetPendingBegin(this, weapon, out var movedItem, out var elapsed))
//         {
//             return false;
//         }
//
//         const float GraceWindowSeconds = 0.35f;
//         return movedItem is MagazineItemClass && elapsed <= GraceWindowSeconds;
//     }
//
// Trecho DEPOIS:

// ref: PA-01-01/PA-01-02 (item 003) + item 004 (colisão cura + swap magazine, generalização) —
// instância (não static, precisa de "this" como o TraderControllerClass dono de List_0/
// InOutHandsProcessTimestampPatch). Reconhece que uma colisão com inOutHandsProcess é uma
// transição de mãos recente e legítima do MESMO jogador na MESMA arma (troca de carregador,
// arma reequipada após cura/granada/faca/reanimação) — e não um saque/guarda real dessa arma
// nem concorrência de outro jogador (impossível aqui: TryGetPendingBegin já é escopado por
// TraderControllerClass = por jogador — ver spec técnica do item 004 §1.2).
private bool IsSelfReferentialHandsTransition(Item item, GEventArgs17 inOutHandsProcess)
{
    if (item is not MagazineItemClass || inOutHandsProcess?.Item is not Weapon weapon)
    {
        return false;
    }

    if (!InOutHandsProcessTimestampPatch.TryGetPendingBegin(this, weapon, out var movedItem, out var elapsed))
    {
        return false;
    }

    // TODO confirmar (P-3.1, herdada do item 003): janela de graça calibrada por instrumentação
    // temporária antes de fechar o item.
    const float GraceWindowSeconds = 0.35f;

    // movedItem == weapon é o único cenário que deve continuar bloqueado: um saque/guarda real
    // desta MESMA arma em voo (ver §1.3 da spec técnica — só ocorre pelo lado "entrar nas mãos",
    // Player.TrySetInHands/HandleInProcess; TryRemoveFromHands nunca produz esse caso porque
    // desvia para SetControllerInsteadRemovedOne antes de levantar o Begin).
    return movedItem != null && movedItem != weapon && elapsed <= GraceWindowSeconds;
}
```

## 6. Fluxo de dados

```
[A] TRL-ImmersiveCombatMedicine: HealRoutine termina → MedicHealPatch.ForceFinishAnimation()
      (mods/TRL-ImmersiveCombatMedicine/modded-V3(review)/Patches/Medical/BandAidController.cs:625)
[B] ForceFinishAnimation invoca via reflexão method_9 de Player.MedsController.ObservedMedsControllerClass
      (MedicHealPatch.cs:208-225 → Player.cs:19640-19660)
[C] method_9 → Callback_0.Invoke(MedsController_0) → motor do EFT devolve a arma equipada às mãos do médico
      → percorre TrySetInHands/HandleInProcess para a arma (Player.cs:32294 no cliente local;
        ObservedInventoryController.HandleInProcess:302-335 no Host/Headless, representando o médico)
      → SetPendingMovedItem(item de cura) [InOutHandsProcessTimestampPatch.cs:50-53]
      → RaiseInOutProcessEvents(Begin) na ARMA [ObservedInventoryController.cs:323]
      → RecordBeginSucceed.Postfix grava (arma, movedItem=item de cura, timestamp=agora) [InOutHandsProcessTimestampPatch.cs:110-117]
[D] Jogador (médico) arrasta um carregador sobre a própria arma (UIFixes, WeaponApplyPatch,
      fora deste mod) — dentro da janela de graça (~0.35s)
[E] Host: ObservedInventoryController.CheckItemAction roda para o swap do carregador
      → encontra o Begin pendente da mesma arma [ObservedInventoryController.cs:162-181]
      → chama IsSelfReferentialHandsTransition(carregador, inOutHandsProcess)
      → SEM a correção: movedItem (item de cura) is MagazineItemClass == false → flag = true
        → retorna GClass1561 (PlayerIsBusyError) → cliente já tinha aplicado o swap (Swap
          otimista) → divergência client/server → inventário travado pelo resto da raid
      → COM a correção: movedItem (item de cura) != weapon && != null && elapsed <= 0.35s → true
        → colisão não é marcada → segue para method_16/GClass1568 (sucesso)
[F] CENÁRIO AINDA PROTEGIDO (saque real da mesma arma na mesma janela): movedItem gravado
      seria a própria arma (== weapon) → IsSelfReferentialHandsTransition retorna false →
      GClass1561 continua sendo retornado (ver §1.3 — validar in-game, §8)
```

## 7. Riscos e dependências

- **Reaproveita 100% da infraestrutura do item `003`** (`InOutHandsProcessTimestampPatch`) sem modificá-la — nenhum risco de regressão nos Harmony patches existentes (`CaptureMovedItemOnRemove`, `RecordBeginSucceed`), que continuam intocados.
- **`GraceWindowSeconds` (P-3.1, aberta desde 2026-09-06)** continua com o mesmo valor não-calibrado (`0.35f`, `TODO confirmar`). Este item não recalibra — se a instrumentação futura de P-3.1 mudar o valor, afeta igualmente todos os casos tolerados por este método (magazine, cura, granada, faca, reanimação), não só o original.
- **`HandsAreNotBusy`** (`mods/HandsAreNotBusy/modded/HANB_FikaSync.cs:175-201`) — mesmo risco já aceito no item 003 (entrada "órfã" inofensiva se o HANB limpar um evento de `List_0` no meio da janela). Nenhuma mudança de comportamento introduzida por este item.
- **`TRL-ImmersiveCombatMedicine`** não precisa de nenhuma mudança — o fix é inteiramente do lado do FIKA, consistente com o diagnóstico da Sessão 9b (`mods/UIFixes/memory/sessions.md`).
- **`UIFixes` (`WeaponApplyPatch`, `SwapPatches.cs:840`)** não precisa de nenhuma mudança — já delega corretamente para o pipeline nativo de `Swap`, confirmado na Sessão 9b e reafirmado na spec funcional (§"Fora de escopo").
- **Incerteza residual sobre `movedItem == weapon` (§1.3):** a leitura estática do Assembly mostra que esse caso só pode originar do lado "entrar nas mãos" (`TrySetInHands`/`HandleInProcess`), nunca de `TryRemoveFromHands` (que desvia via `SetControllerInsteadRemovedOne`). Não há como provar em runtime, sem reproduzir em raid, que um saque de arma real efetivamente grava `movedItem == weapon` (dependeria do estado exato de `HandsController.Item` no instante do `Begin`, não determinável só por leitura estática). Mitigado por um item de validação in-game dedicado (§8) — se a validação mostrar que um saque de arma real NÃO é bloqueado, é preciso revisitar esta spec antes de fechar o item (não fazer esse ajuste "no escuro" durante o `/code-mod`).

## 8. Checklist de implementação

- [x] Renomear `IsSelfReferentialMagazineSwap` → `IsSelfReferentialHandsTransition` em `ObservedInventoryController.cs` (definição + único call site, linha 174) conforme stub §5.
- [x] Trocar a condição de retorno de `movedItem is MagazineItemClass && elapsed <= GraceWindowSeconds` para `movedItem != null && movedItem != weapon && elapsed <= GraceWindowSeconds` conforme stub §5.
- [x] Atualizar a mensagem de log `#if DEBUG` (linha ~177) de "not a self-referential magazine swap" para "not a self-referential hands transition".
- [x] Bump `FikaPlugin.cs:49` (`FikaVersion`: `2.3.14` → `2.3.15`) e `mods/FIKA/mod.json` (componente `plugin`: `2.3.14` → `2.3.15`).
- [ ] Compilar `Fika.Core.dll` via `dotnet build -c Release` (build isolado em `mods/FIKA/builds/` — **não usar `/compile-mod`**, instala automaticamente no jogo; usuário copia manualmente). Propagar para `Fika-Headless/References/Fika.Core.dll` e recompilar `Fika.Headless.dll` (mesmo procedimento dos itens 001/003). **Pendente — build.**
- [ ] Validar in-game o cenário principal: curar um aliado (Host/Headless dedicado) e, dentro da janela de graça, trocar o carregador da própria arma — sem rejeição, sem mão travada. **Pendente — in-game.**
- [ ] Validar in-game o corner case "self-heal" (curar a si mesmo) seguido de swap de carregador. **Pendente — in-game.**
- [ ] **(ref: PA-01-01)** Validar in-game a sequência repetida: curar 2+ aliados diferentes em sequência (ou o mesmo aliado 2+ vezes) trocando o carregador após cada cura, confirmando ausência de travamento/divergência em qualquer repetição — não só na primeira (critério de aceite obrigatório da spec funcional). **Pendente — in-game.**
- [ ] **(ref: PA-01-02)** Validar in-game, com um segundo jogador observando (B), que a sequência cura→swap de carregador de A não produz nenhuma anomalia visual na arma ou inventário de A do ponto de vista de B (critério padrão Fika/multiplayer da spec funcional). **Pendente — in-game.**
- [ ] **Validação dedicada à incerteza de §1.3/§7:** sacar uma arma nova (holster→draw completo, não reload) e, na mesma janela de graça, tentar um swap de carregador nessa arma — confirmar que a rejeição `GClass1561` **continua ocorrendo** (o cenário que `movedItem != weapon` deve continuar bloqueando). Se a rejeição NÃO ocorrer (ou seja, o saque de arma real também for tolerado indevidamente), a hipótese de §1.3 sobre `TrySetInHands` estava incompleta — revisitar a condição desta spec (ex.: um discriminador adicional) antes de fechar o item. **Pendente — in-game, bloqueador para fechar o item.**
- [ ] Validar in-game os demais corner cases da spec funcional que dependem de reprodução (granada, faca, reanimação) — na medida do praticável; alguns podem ficar como validação best-effort se a reprodução em raid for difícil de agendar. **Pendente — in-game.**
- [ ] Validar in-game o corner case "dois jogadores diferentes mexendo em armas diferentes ao mesmo tempo" (proteção estrutural via `ConditionalWeakTable` por `TraderControllerClass`, §1.2) — confirmar que não há vazamento cruzado. **Pendente — in-game.**

## 9. Conformidade com skills (auto-checklist)

| # | Check | Status | Evidência / razão |
|---|---|---|---|
| 1 | Lifecycle de raid: start hook + stop hooks idempotentes — AP-01 | N/A | Nenhum estado novo introduzido; reaproveita o `ConditionalWeakTable` do item 003 sem alteração (§1.2, §7) — já não fixa (pin) chaves, coletado junto com o jogador/controller ao fim da raid |
| 2 | Filtro MainPlayer/Fika em todo patch que reage a ação de player — AP-02 | N/A | A mudança é uma condição de validação dentro de um método já usado exclusivamente para jogadores observados pelo Host/Headless (`ObservedInventoryController`, nunca o cliente local) — filtro já é estrutural, herdado do item 003 |
| 3 | Alvos ofuscados/virtuais resolvidos por assinatura; TODOS os overrides auditados — AP-03 | ✅ | Nenhum novo alvo de patch. A única mudança é em código do próprio mod (`IsSelfReferentialHandsTransition`), não em método virtual do EFT. `TryRemoveFromHands`/`TrySetInHands`/`RaiseInOutProcessEvents` (usados pela infraestrutura reaproveitada) já foram auditados como não-virtuais no item 003 |
| 4 | Mudança de estado via API canônica do EFT; side-effects mapeados — AP-04 | ✅ | Não mutamos nenhum estado do EFT; só ampliamos a condição de um `flag` de validação já calculado, preservando o restante do pipeline (`method_16`/`GClass1568`) inalterado — mesma natureza do check equivalente no item 003 |
| 5 | Estado entre raids: raid1→exit→raid2 e alt-F4/morte/MIA cobertos | ✅ | Ver check 1 — nenhuma coleção nova; `ConditionalWeakTable` existente já garante que entradas não sobrevivem ao jogador/controller da raid anterior |
| 6 | Semântica/defaults/faixas de cada ConfigEntry sem ambiguidade — AP-05 | N/A | Nenhuma `ConfigEntry` nova (§3) |
| 7 | Re-invocação de método patcheado tem reentry-guard — AP-07 | N/A | Nenhum Postfix/Prefix novo; `IsSelfReferentialHandsTransition` é um método privado de instância chamado uma única vez por avaliação de `CheckItemAction`, sem recursão |
| 8 | Flags/caches de intercept validados contra o contexto atual após troca — AP-08 | ✅ | `TryGetPendingBegin` já valida por `(controller, arma)` exatos (item 003); a mudança desta spec só amplia QUAL `movedItem` é aceito como isento, sem alterar a chave de busca nem a janela de tempo — trocar de arma continua criando uma entrada nova, sem reaproveitar timestamp de arma anterior |
| 9 | Todo patch-point reconfirmado no `.cs` do dump — AP-09 | ✅ | `Player.cs:19441-19660` (`ObservedMedsControllerClass`/`method_9`), `Player.cs:32223-32263` (`TryRemoveFromHands`), `Player.cs:32294-32337` (`TrySetInHands`), `Player.cs:1441-1495` (`method_33/34/35`) lidos diretamente nesta sessão. `ObservedInventoryController.cs` e `InOutHandsProcessTimestampPatch.cs` (código do mod) lidos integralmente nesta sessão, não só citados do item 003. `BandAidController.cs`/`MedicHealPatch.cs` do `TRL-ImmersiveCombatMedicine` lidos diretamente. Incerteza residual sobre `movedItem == weapon` em runtime documentada explicitamente em §1.3/§7 (não afirmada como certeza sem prova) e endereçada com item de validação bloqueador em §8 |
| 10 | Skill EFT usada como lever confirmada não-inerte — AP-10 | N/A | Não usa skill do EFT como alavanca |
| 11 | Pacote FIKA próprio: envelope/`TryGet*`/`Valid`/main thread/registro por instância/zero `UnregisterPacket` — AP-11 | N/A | Não declara nenhum `INetSerializable` novo; não envia nada pela rede |

## Histórico

| Data | Evento |
|---|---|
| 2026-09-08 | Spec técnica criada via `/create-technical-spec` |
| 2026-09-08 | Revisão `/review-technical-spec` 01 — 3 pontos (0🔴/1🟡/2🟢) resolvidos: 2 itens de validação in-game adicionados ao §8 (sequência repetida/múltiplos aliados — PA-01-01; checagem do observador B — PA-01-02); comentários inline do stub §5 encurtados para um ID estável (PA-01-03) |
