# 001 — Crash flea oferta sem itens · Code Review 01

**Mod:** TRL-ItemsManagement
**Spec funcional:** [001-crash-flea-oferta-sem-itens-01-spec.md](001-crash-flea-oferta-sem-itens-01-spec.md)
**Spec técnica:** [001-crash-flea-oferta-sem-itens-02-spec-tech.md](001-crash-flea-oferta-sem-itens-02-spec-tech.md)
**Asbuild:** [001-crash-flea-oferta-sem-itens-05-asbuild.md](001-crash-flea-oferta-sem-itens-05-asbuild.md)
**Data:** 2026-09-23

> Análise crítica do código implementado por `/code-mod`. Cada achado recebe um ID `CR-01-MM` permanente. Resolver bloqueadores 🔴 via `/apply-code-review` antes de fechar o item.

## Resumo

> 🔴 Bloqueadores: 0 · 🟠 Fortes: 0 · 🟡 Médios: 0 · 🟢 Menores: 0 · ✅ Resolvidos: 2 · Total: 2

## Índice

| ID | Categoria | Impacto | Título | Status |
| --- | --- | --- | --- | --- |
| CR-01-01 | B | 🟠 | Outros acessores de leitura de `RagfairOfferHolder` continuam sem guarda (`GetOfferById`, `GetOffersByTemplate`, `GetOffersByTrader`) | ✅ Aplicado em 2026-09-23 |
| CR-01-02 | E | 🟢 | Comentário da classe reivindica proteção de "preço médio" que o código não cobre | ✅ Aplicado em 2026-09-23 |

## Categorias

- **A — Crítico** — bug grave, crash garantido, corrupção de estado, security issue.
- **B — Bug latente** — comportamento errado em cenário plausível, não acionado pelo caminho golden.
- **C — Gap vs. spec** — código não implementa critério de aceite, corner case, ou AC da spec.
- **D — Arquitetura** — viola padrões do repo, duplica código, leak de estado, abuso de reflection.
- **E — Legibilidade/manutenção** — nomes ruins, comentário "porquê" ausente, código morto, complexidade desnecessária.
- **F — Melhoria opcional** — refactor de qualidade, micro-otimização, simplificação.

## Impacto

- 🔴 **Bloqueador** — fix obrigatório antes de fechar o item.
- 🟠 **Forte** — fix recomendado; pode ser deferido para `06-fix-NN.md` futuro.
- 🟡 **Médio** — anotar, decidir caso a caso.
- 🟢 **Menor** — opcional.

---

## Pontos

### CR-01-01 · B — Bug latente · 🟠 Forte · ✅ Aplicado em 2026-09-23

**Outros acessores de leitura de `RagfairOfferHolder` continuam sem guarda — o mesmo crash pode acontecer por caminhos diferentes de `GetOffers()`**

**Local:** [`mods/TRL-ItemsManagement/modded/Server/Ragfair/RagfairEmptyOfferGuardPatch.cs:L19`](../../modded/Server/Ragfair/RagfairEmptyOfferGuardPatch.cs#L19)

**Problema:** O patch só cobre `RagfairOfferService.GetOffers()`. Fui conferir se havia outro método na mesma classe/subsistema com o mesmo tipo de acesso desprotegido e achei três:

1. `RagfairOfferService.GetOffersOfType(templateId)` ([RagfairOfferService.cs:L57-L60](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Services/RagfairOfferService.cs#L57-L60)) chama `ragfairOfferHolder.GetOffersByTemplate(templateId)` — **não** `GetOffers()`. É usado por `RagfairController.GetItemMinAvgMaxFleaPriceValues` → `GetAveragePriceFromOffers`, cuja linha `offer.Items.First().Upd?.StackObjectsCount` ([RagfairController.cs:L397](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Controllers/RagfairController.cs#L397)) crasha exatamente como a busca original crashava, se a oferta quebrada aparecer entre os resultados. Consulta de "preço médio de um item" é uma ação de UI comum (passar o mouse/checar preço), não rara.
2. `RagfairOfferHolder.GetOfferById(id)` ([RagfairOfferHolder.cs:L54-L57](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Utils/RagfairOfferHolder.cs#L54-L57)) — usado por `RagfairController.GetOfferByInternalId` ([RagfairController.cs:L1140-L1146](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Controllers/RagfairController.cs#L1140-L1146)), chamado quando o jogador clica numa oferta específica.
3. `RagfairOfferHolder.GetOffersByTrader(traderId)` ([RagfairOfferHolder.cs:L94-L102](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Utils/RagfairOfferHolder.cs#L94-L102)) — listagem de ofertas por trader.

Nenhum dos três passa por `RagfairOfferService.GetOffers()`, então nenhum é filtrado pelo patch atual.

Confirmei que patchear os três na camada do `Holder` (diferente da recomendação da review técnica de **não** patchear `RagfairOfferHolder.GetOffers()`) é seguro aqui: o único método que `FlagExpiredOffersAfterDate` chama internamente é `GetOffers()` ([RagfairOfferHolder.cs:L397](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Utils/RagfairOfferHolder.cs#L397)) — `GetOffersByTemplate`/`GetOffersByTrader` não são chamados por nenhum mecanismo interno de expiração/limpeza. `GetOfferById` **é** chamado internamente por `GetExpiredOfferItems` ([RagfairOfferHolder.cs:L359](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Utils/RagfairOfferHolder.cs#L359)), mas o efeito de filtrá-lo ali é inofensivo: o offer cai no branch `if (offer is null)` (linha 360-364, loga "not found, skipping") em vez do branch `if (offer.Items?.Count == 0)` (linha 366-370, loga "has no items, skipping") — mesmo resultado prático (a oferta é pulada da regeneração), só muda a mensagem de log.

**Por que importa:** O item resolve o crash de busca já observado em produção, mas deixa pelo menos um caminho igualmente comum (checar preço de um item na flea) igualmente quebrado — só que com um stack trace diferente, que vai reaparecer como "o fix não funcionou completamente" assim que alguém checar o preço do item da oferta quebrada.

**Sugestão:** Extrair a lógica de filtro (`for` + `ConcurrentDictionary` dedup + log) de `Postfix` para um método estático compartilhado `FilterBrokenOffers(List<RagfairOffer>? offers)` (ou `IEnumerable<RagfairOffer>?` conforme o tipo de retorno de cada alvo) dentro da mesma classe, e adicionar mais três `[HarmonyPatch]`/`[HarmonyPostfix]` (podem ficar na mesma classe ou em classes irmãs no mesmo arquivo/pasta) para `RagfairOfferHolder.GetOfferById`, `RagfairOfferHolder.GetOffersByTemplate` e `RagfairOfferHolder.GetOffersByTrader`, reaproveitando `LoggedOfferIds`. Ajustar o comentário de classe (ver CR-01-02) para refletir a cobertura real após a extensão.

**Decisão:**
- `[x]` Aceitar sugestão

**Resolução:** Sugestão aplicada conforme proposto.
**Aplicação:** `mods/TRL-ItemsManagement/modded/Server/Ragfair/RagfairEmptyOfferGuardPatch.cs` reestruturado — lógica de filtro extraída para `RagfairOfferGuardCore` (compartilhada), e adicionados 3 novos `[HarmonyPatch]`/`[HarmonyPostfix]`: `RagfairOfferHolderGetOffersByTemplateGuardPatch`, `RagfairOfferHolderGetOffersByTraderGuardPatch`, `RagfairOfferHolderGetOfferByIdGuardPatch`. Todos reaproveitam `LoggedOfferIds`/`LogBrokenOfferOnce` do core.

---

### CR-01-02 · E — Legibilidade · 🟢 Menor · ✅ Aplicado em 2026-09-23

**Comentário da classe reivindica proteção de "preço médio" que o código atual não cobre**

**Local:** [`mods/TRL-ItemsManagement/modded/Server/Ragfair/RagfairEmptyOfferGuardPatch.cs:L12-L13`](../../modded/Server/Ragfair/RagfairEmptyOfferGuardPatch.cs#L12-L13)

**Problema:**
```csharp
///     ser a origem comum de todos os consumidores de apresentação da flea (busca, categorias, preço
///     médio) SEM tocar no ciclo de expiração interno de <c>RagfairOfferHolder</c>...
```
"Preço médio" (`GetItemMinAvgMaxFleaPriceValues`/`GetAveragePriceFromOffers`) não passa por `RagfairOfferService.GetOffers()` — passa por `GetOffersOfType`/`GetOffersByTemplate` (ver CR-01-01). O comentário afirma uma cobertura que não existe.

**Por que importa:** Um leitor futuro (inclusive o próprio autor, em outra sessão) confiaria nessa afirmação sem reconferir o código, e poderia descartar "preço médio quebra com oferta vazia" como já resolvido quando não está.

**Sugestão:** Se CR-01-01 for aceito: atualizar o comentário para listar os 4 métodos cobertos após a extensão. Se CR-01-01 for deferido: remover "preço médio" da lista e adicionar uma linha `<c>TODO</c>`/nota explícita: "GetOffersOfType/GetOffersByTemplate (preço médio) e GetOfferById/GetOffersByTrader continuam desprotegidos — ver CR-01-01."

**Decisão:**
- `[x]` Aceitar sugestão

**Resolução:** Sugestão aplicada conforme proposto.
**Aplicação:** Comentário antigo (que reivindicava "preço médio") removido de `RagfairEmptyOfferGuardPatch`; substituído por um comentário de cobertura precisa em `RagfairOfferGuardCore` (topo do arquivo) listando os 4 métodos realmente cobertos após CR-01-01, e cada classe de patch individual ganhou seu próprio comentário focado só no que ela cobre.

---

## Histórico

| Data | Evento |
| --- | --- |
| 2026-09-23 | Code review 01 criada via `/code-review` |
| 2026-09-23 | Aplicação automática de 2 achados via `/apply-code-review` — IDs aplicados: CR-01-01, CR-01-02 |
