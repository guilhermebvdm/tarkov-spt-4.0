# 003 — Fix rejeição de swap de magazine in-place no Headless (GClass1561) · Review Técnica 01

**Mod:** FIKA
**Spec técnica revisada:** [003-magazine-swap-inplace-fix-02-spec-tech.md](003-magazine-swap-inplace-fix-02-spec-tech.md)
**Data:** 2026-09-06

> Análise crítica da spec técnica. Cada ponto recebe um ID `PA-01-MM`. Resolver até zerar bloqueadores antes de `/code-mod`.

**Memória consultada:** snapshot de 2026-09-03 (Sessão 2) de `mods/FIKA/memory/sessions.md` · pendências que afetam: nenhuma.
**Docs técnicos:** `spt-antipatterns.md` relido (AP-03, AP-07, AP-09) · nenhum doc canônico contradiz a spec.

## Resumo

> 🔴 Bloqueadores: 0 · 🟡 Importantes: 0 · 🟢 Menores: 0 · ✅ Resolvidos: 5 · Total: 5

## Índice

| ID | Categoria | Impacto | Título | Status |
|---|---|---|---|---|
| PA-01-01 | C — Erro de lógica | 🔴 | Stub `IsSelfReferentialMagazineSwap` não compila (`this` em método `static`) | ✅ Resolvido em 2026-09-06 |
| PA-01-02 | A — Gap | 🔴 | Janela de graça não distingue swap de magazine de um saque/troca de arma real na mesma janela — viola corner case da spec funcional | ✅ Resolvido em 2026-09-06 |
| PA-01-03 | A — Gap | 🟡 | Spec não demonstra explicitamente qual checagem do laço protege o corner case "dois jogadores no mesmo item" | ✅ Resolvido em 2026-09-06 |
| PA-01-04 | C — Erro de lógica | 🟢 | Citação de linha do trecho `inOutHandsProcess` em `ObservedInventoryController.cs` está 1 linha à frente do real; blocos `#if DEBUG` omitidos no "antes" | ✅ Resolvido em 2026-09-06 |
| PA-01-05 | B — Edge case | 🟢 | Crescimento não-limitado (mas de baixo risco) do dicionário interno por arma se `Succeed` nunca chegar para uma arma específica | ✅ Resolvido em 2026-09-06 (risco aceito e documentado) |

## Categorias

- **A — Gaps de Especificação:** informações ausentes que ambiguam a implementação
- **B — Edge Cases:** cenários válidos não cobertos
- **C — Erros de Lógica:** pressupostos errados, contradições, código incompatível com SPT 4.0+

## Impacto

- 🔴 **Bloqueador** — impede implementar ou causa bug/crash garantido
- 🟡 **Importante** — pode causar comportamento errado em cenário relevante
- 🟢 **Menor** — qualidade/clareza, não bloqueia

---

## Pontos

### PA-01-01 · C — Erro de lógica · 🔴 ✅ Resolvido em 2026-09-06

**Stub `IsSelfReferentialMagazineSwap` não compila**

**Problema:** No §5 da spec técnica, o método é declarado `private static bool IsSelfReferentialMagazineSwap(Item item, GEventArgs17 inOutHandsProcess)`, mas seu corpo chama `InOutHandsProcessTimestampPatch.TryGetBeginElapsedSeconds(this, weapon, out var elapsed)`. Um método `static` não tem acesso a `this` — isso é erro de compilação (CS0026 "keyword 'this' is not valid in a static property, static method, or static field initializer").

**Por que importa:** A regra do `/create-technical-spec` é explícita: "Stubs devem compilar se copiados num projeto vazio". Como está, o `/code-mod` copiaria um erro de compilação direto para `ObservedInventoryController.cs`.

**Sugestão:** Remover o modificador `static` do método (`private bool IsSelfReferentialMagazineSwap(...)`), já que ele já é chamado a partir de um contexto de instância (dentro de `CheckItemAction`, que não é estático) e `this` resolve naturalmente para a própria `ObservedInventoryController` (que É um `TraderControllerClass`, o tipo esperado pelo primeiro parâmetro de `TryGetBeginElapsedSeconds`).

**Decisão:**
- `[x]` Aceitar sugestão

**Resolução:** Método `IsSelfReferentialMagazineSwap` reescrito como instância (sem `static`) em `003-magazine-swap-inplace-fix-02-spec-tech.md` §5.

---

### PA-01-02 · A — Gap · 🔴 ✅ Resolvido em 2026-09-06

**Janela de graça não distingue a própria troca de magazine de um saque de arma real ocorrendo por coincidência na mesma janela**

**Problema:** `GEventArgs17` (`InOutHandsProcessEventArgs`) carrega apenas `Item` (a arma — sempre `HandsController.Item`), `Location`, `Status`, `EventId` e `OwnerId` (confirmado em `GEventArgs1.cs:11-20`, base de `GEventArgs17`). O `Item` do evento é **sempre a arma**, tanto quando a transição pendente é um **saque/guarda de arma** (`method_33`/`method_34` chamados com `item` = a própria arma, `Player.cs:1441-1464` + `method_35`, `Player.cs:1471-1495`, retornando o próprio item quando ele É `ItemInHands`) quanto quando é um **swap de magazine dentro da arma já empunhada** (mesmo `method_35` retornando o item quando o endereço está apenas *aninhado* em `ItemInHands`). A spec técnica (§1.4, §5) propõe isentar a colisão sempre que "o Begin daquela arma foi aberto há menos que uma janela de graça curta" — mas essa condição, como desenhada (`InOutHandsProcessTimestampPatch` grava o timestamp a partir de `RaiseInOutProcessEvents(GEventArgs17 args)`, que só recebe a arma, nunca o item efetivamente movido), **não tem como saber se o `Begin` pendente é a própria troca de magazine ou um saque de arma genuíno que por coincidência aconteceu dentro da mesma janela de 0.35s**.

Isso é exatamente o corner case que a spec funcional exige preservar: *"Troca disparada logo após o jogador sacar a arma (transição de mãos anterior — troca de arma — ainda não confirmada pelo Headless) — verificar que a correção não libera indevidamente uma colisão real com essa transição diferente"* (`003-magazine-swap-inplace-fix-01-spec.md` §Corner cases). Como desenhada, a correção **libera exatamente esse caso indevidamente**, porque não existe, no par `(arma, timestamp)`, nenhuma informação sobre qual item foi originalmente movido para abrir aquele `Begin`.

**Por que importa:** Sem essa distinção, um jogador que sacar uma arma e imediatamente (dentro de ~350ms) arrastar um carregador sobre ela teria a troca aplicada no Headless mesmo com a transição de saque ainda em voo — o comportamento que o corner case da spec funcional explicitamente proíbe. Isso não é hipotético: times com binds rápidos ou macros de "sacar + recarregar" cairiam nessa janela facilmente.

**Sugestão:** Mover o ponto de captura do timestamp de `TraderControllerClass.RaiseInOutProcessEvents` (que só recebe a arma) para `Player.TryRemoveFromHands(Item item, ...)` / `Player.TrySetInHands(Item item, ItemAddress to, ...)` (`Player.cs:32223` e `Player.cs:32294`), que recebem o **item efetivamente movido** (`item2`/`item` nos parâmetros — a arma no caso de saque/guarda, ou o carregador no caso do swap in-place). Um `[HarmonyPrefix]` nesses dois métodos, gravando `(arma = HandsController.Item, itemMovido, Time.time)` — capturado ANTES do `Begin` ser levantado — permite ao `IsSelfReferentialMagazineSwap` checar não só a janela de tempo, mas também que `itemMovido is MagazineItemClass` (nunca a própria arma). Isso torna a isenção estruturalmente impossível de disparar para um saque/guarda de arma, porque nesse caso `itemMovido` é sempre a arma, nunca um `MagazineItemClass`. Ajustar §1.4, §2, §4, §5 e §6 da spec técnica para refletir o novo ponto de patch (`TryRemoveFromHands`/`TrySetInHands` em vez de `RaiseInOutProcessEvents`) e o campo adicional (`itemMovido`) no estado gravado.

**Decisão:**
- `[x]` Aceitar sugestão

**Resolução:** Ponto de captura movido de `RaiseInOutProcessEvents` para `TryRemoveFromHands`/`TrySetInHands` (recebem o item efetivamente movido). Exceção agora exige `movedItem is MagazineItemClass` — um saque/guarda de arma sempre grava a própria arma como `movedItem`, nunca satisfazendo a condição. Ver `003-magazine-swap-inplace-fix-02-spec-tech.md` §1.4, §2, §5, §6.

---

### PA-01-03 · A — Gap · 🟡 ✅ Resolvido em 2026-09-06

**Spec não demonstra qual checagem protege o corner case "dois jogadores no mesmo item"**

**Problema:** O critério de aceite Fika/multiplayer da spec funcional exige que "um segundo jogador tentando mutar o mesmo item... enquanto a operação do primeiro ainda está em andamento continua sendo corretamente recusado". A spec técnica (§1.4) só demonstra, com evidência, que a isenção proposta é escopada ao bloco `inOutHandsProcess` (linhas ~160-177 atuais) — mas não mapeia explicitamente qual das **outras** checagens do mesmo laço de `ObservedInventoryController.CheckItemAction` (`GEventArgs7`/`GEventArgs8`/`GEventArgs2`/`GEventArgs3`, a colisão direta `item == geventArgs2.Item`, ou `smethod_0`) é a que efetivamente bloquearia esse cenário — nem confirma que `List_0` é realmente o mecanismo relevante para uma colisão *entre dois jogadores diferentes* (o item pode ter dois "donos" possíveis dependendo do contexto de troca/loot).

**Por que importa:** Sem essa demonstração explícita, o critério "concorrência real continua bloqueada" fica sustentado por suposição, não por evidência — exatamente o tipo de lacuna que a §9 (check 3/AP-03) pede para ser fechada antes do código.

**Sugestão:** Adicionar um parágrafo em §1.4 (ou uma subseção nova) explicando, com o `arquivo.cs:linha` da checagem específica, qual dos outros ramos do laço (provavelmente a colisão direta `item == geventArgs2.Item`, já que ambos os jogadores estariam operando sobre a mesma instância de `Item`) permanece intocado pela mudança e cobre esse corner case. Se não for possível confirmar estaticamente, marcar como `TODO confirmar` explícito e adicionar ao checklist de `/code-mod` um teste manual dedicado a esse corner case antes de fechar o item (a spec funcional já lista isso como corner case obrigatório, então o teste deveria existir de qualquer forma — só falta a spec técnica citar a base de código que sustenta a expectativa).

**Decisão:**
- `[x]` Aceitar sugestão

**Resolução:** Adicionado parágrafo dedicado em `003-magazine-swap-inplace-fix-02-spec-tech.md` §1.4 ("Por que isso cobre...") citando `ObservedInventoryController.cs:178` (`item == geventArgs2.Item`) como a checagem intocada que protege esse corner case; teste manual dedicado adicionado ao checklist §8.

---

### PA-01-04 · C — Erro de lógica · 🟢 ✅ Resolvido em 2026-09-06

**Citação de linha do bloco `inOutHandsProcess` está deslocada em 1 linha; blocos `#if DEBUG` omitidos no "antes"**

**Problema:** A spec cita "`ObservedInventoryController.cs:161-178`" para o bloco `lambda.inOutHandsProcess = ...` em §1.4, §2 e §5. Conferindo o arquivo real (lido nesta review), o bloco começa em `lambda.inOutHandsProcess = geventArgs2 as GEventArgs17;` na linha **160** e o `if (item == geventArgs2.Item)` seguinte está na linha **178** — ou seja, o bloco em si é `160-177`, não `161-178`. Além disso, o trecho "ANTES" mostrado no stub §5 omite os blocos `#if DEBUG ... #endif` que envolvem os `FikaGlobals.LogError(...)` de diagnóstico existentes no código real entre as duas checagens `Any(lambda.method_1)`.

**Por que importa:** Não bloqueia a lógica da correção, mas se o `/code-mod` copiar o trecho "DEPOIS" do stub literalmente por cima do range de linha errado, ou esquecer de preservar os blocos `#if DEBUG`, perde-se instrumentação de debug já existente sem necessidade.

**Sugestão:** Ajustar as três citações para `160-177`, e no stub §5 explicitar `// (blocos #if DEBUG existentes entre as duas checagens Any(...) devem ser preservados — omitidos aqui só por brevidade)`.

**Decisão:**
- `[x]` Aceitar sugestão

**Resolução:** Citações corrigidas para `160-177` em todo o documento; nota explícita sobre preservar os blocos `#if DEBUG` adicionada ao stub §5 e ao checklist §8 em `003-magazine-swap-inplace-fix-02-spec-tech.md`.

---

### PA-01-05 · B — Edge case · 🟢 ✅ Resolvido em 2026-09-06

**Crescimento não-limitado (baixo risco) do dicionário interno por arma**

**Problema:** `PerControllerState.BeginTimestamps` (ou, após PA-01-02, a estrutura equivalente em `TryRemoveFromHands`/`TrySetInHands`) só remove uma entrada quando o `Succeed` correspondente chega. Se, por qualquer motivo, um `Succeed` nunca chegar para uma arma específica (ex.: jogador desconecta no meio da transição), a entrada permanece na memória pelo resto da raid. Como o dicionário é por-controller e as chaves são instâncias de `Item` (armas), o crescimento é limitado ao número de armas distintas que aquele jogador empunhou na raid — baixo, mas não zero.

**Por que importa:** Não é um leak entre raids (a §9 check 1 já cobre isso via `ConditionalWeakTable` não fixar o controller), mas dentro de uma única raid longa com muita troca de arma, o dicionário interno cresce sem nunca encolher no pior caso.

**Sugestão:** Opcional — não bloqueante. Se quiser fechar de vez, adicionar um limite simples (ex.: ao ultrapassar N entradas, remover a mais antiga) ou aceitar o risco documentando-o explicitamente na spec como aceito (dado o tamanho tipicamente pequeno do conjunto de armas por jogador por raid).

**Decisão:**
- `[x]` Aceitar sugestão (risco documentado, sem mitigação adicional)

**Resolução:** Risco aceito e documentado explicitamente em `003-magazine-swap-inplace-fix-02-spec-tech.md` §7 ("Crescimento não-limitado de baixo risco (PA-01-05, aceito)"), justificando pelo escopo limitado ao número de armas distintas por raid e pela limpeza garantida pelo `ConditionalWeakTable` entre raids.
