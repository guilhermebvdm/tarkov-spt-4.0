# 001 — Crash flea oferta sem itens · Review Técnica 01

**Mod:** TRL-ItemsManagement
**Spec técnica revisada:** [001-crash-flea-oferta-sem-itens-02-spec-tech.md](001-crash-flea-oferta-sem-itens-02-spec-tech.md)
**Data:** 2026-09-23

> Análise crítica da spec técnica. Cada ponto recebe um ID `PA-01-MM`. Resolver até zerar bloqueadores antes de `/code-mod`.

## Resumo

> 🔴 Bloqueadores: 0 · 🟡 Importantes: 0 · 🟢 Menores: 0 · ✅ Resolvidos: 3 · Total: 3

## Índice

| ID | Categoria | Impacto | Título | Status |
|---|---|---|---|---|
| PA-01-01 | A/C | 🔴 | Ponto de patch não cobre todos os crashes possíveis — mover para `RagfairOfferService.GetOffers()` | ✅ Resolvido em 2026-09-23 |
| PA-01-02 | A | 🟡 | Checklist de validação não dá um caminho concreto de teste | ✅ Resolvido em 2026-09-23 |
| PA-01-03 | B | 🟢 | Crescimento ilimitado de `LoggedOfferIds` não documentado como premissa | ✅ Resolvido em 2026-09-23 |

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

### PA-01-01 · A/C — Gap + Erro de lógica · ✅ Resolvido em 2026-09-23

**Patchear só `RagfairCategoriesService.GetCategoriesFromOffers` não satisfaz o critério de aceite "a busca nunca falha" — existe pelo menos mais um caminho de crash não coberto**

**Problema:** Segui a cadeia de chamadas completa de `/client/ragfair/find` além do que a spec técnica documentou, e `RagfairController.GetOffers` ([RagfairController.cs:L80-L99](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Controllers/RagfairController.cs#L80-L99)) constrói **dois conjuntos de ofertas independentes** a partir da mesma fonte, por dois caminhos diferentes:

1. `result.Offers` (os itens realmente exibidos na busca) vem de `GetOffersForSearchType` → `RagfairOfferHelper.GetValidOffers` ([RagfairOfferHelper.cs:L58-L109](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Helpers/RagfairOfferHelper.cs#L58-L109)), que faz `offer.Items.FirstOrDefault()` na linha 74 e passa o resultado (`null` para uma oferta quebrada) para `PassesSearchFilterCriteria`.
2. `result.Categories` (o que a spec técnica atual protege) só é calculado quando `searchRequest.UpdateOfferCount == true` ([RagfairController.cs:L96-L99](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Controllers/RagfairController.cs#L96-L99)), e dentro de `GetSpecificCategories` ([RagfairController.cs:L256-L282](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Controllers/RagfairController.cs#L256-L282)) — para qualquer busca que **não** seja "linked"/"required" search (a maioria) — o pool de ofertas usado é **descartado e reobtido do zero** via `ragfairOfferService.GetOffers()` (linha 268), ignorando completamente `result.Offers` já filtrado.

O caminho 1 (`GetValidOffers`) **também tem um ponto de crash real** com uma oferta de `Items` vazio: `PassesSearchFilterCriteria` ([RagfairOfferHelper.cs:L874-L977](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Helpers/RagfairOfferHelper.cs#L874-L977)) recebe `offerRootItem` (pode ser `null`) e o deref sem guard em `offerRootItem.Upd.StackObjectsCount` nas linhas 902 e 908 — **se a busca do jogador usar filtro de quantidade** (`QuantityFrom`/`QuantityTo` no request), isso lança NRE ali, **antes mesmo** de chegar em `GetCategoriesFromOffers`. Esse caminho não passa pelo patch atual em nenhuma hipótese, porque `GetValidOffers` roda sempre (toda busca), independente de `UpdateOfferCount`.

Não vimos esse segundo crash nos logs de produção porque provavelmente nenhuma busca até agora usou filtro de quantidade — mas ele é real e vai acontecer assim que alguém usar esse filtro numa busca que toque a oferta quebrada. A spec funcional (critério de aceite #1) exige "nunca retorna erro genérico ao jogador, mesmo havendo uma oferta ativa sem itens" — sem qualificar por tipo de filtro — então esse caminho conta.

**Por que importa:** Implementar exatamente como a spec técnica atual descreve entrega uma correção **incompleta**: resolve o crash já observado, mas deixa uma segunda forma de crashar a mesma busca aberta, que vai aparecer como um bug "novo" (stack trace diferente) assim que alguém usar o filtro de quantidade — provavelmente lido como "o fix não funcionou" quando na verdade é uma lacuna de escopo do próprio design.

**Sugestão:** Mover o ponto de patch de `RagfairCategoriesService.GetCategoriesFromOffers` para **`RagfairOfferService.GetOffers()`** ([RagfairOfferService.cs:L42-L44](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Services/RagfairOfferService.cs#L42-L44) — passthrough de 1 linha para `ragfairOfferHolder.GetOffers()`). Esse é o ponto de origem comum usado por **todos** os 6 consumidores relevantes que constroem dado pra exibir ao jogador (confirmado por grep, todos os call-sites reais):
- `RagfairController.cs:268` (a própria categoria — cobre o crash já observado)
- `RagfairController.cs:1142`
- `RagfairOfferHelper.cs:70` (`GetValidOffers` — cobre o segundo crash encontrado aqui, `offerRootItem` nunca mais será `null` porque a oferta quebrada já não estará na lista)
- `RagfairRequiredItemsService.cs:54`
- `RagfairServer.cs:111` e `:131`

Importante: **não** patchear `RagfairOfferHolder.GetOffers()` diretamente (a camada abaixo) — o próprio `RagfairOfferHolder.FlagExpiredOffersAfterDate` ([RagfairOfferHolder.cs:L393-L420](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Utils/RagfairOfferHolder.cs#L393-L420)) chama `GetOffers()` **internamente, na mesma classe** (linha 397) pra decidir quais ofertas expirar. Se o filtro entrasse aí, uma oferta quebrada nunca seria flagada como expirada nem removida de `_offersById` — viraria uma "zumbi" permanente em vez de eventualmente ser limpa pelo ciclo de vida natural da SPT (que já tem seu próprio guard em `GetExpiredOfferItems`, [RagfairOfferHolder.cs:L366-L370](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Utils/RagfairOfferHolder.cs#L366-L370)). Patchear na camada do `Service` (não do `Holder`) preserva esse ciclo de vida intacto — `FlagExpiredOffersAfterDate` continua vendo (e eventualmente removendo) a oferta quebrada, enquanto todo consumidor "de apresentação" passa a recebê-la já filtrada.

Consequências a incorporar na spec (e revisar com o usuário se aceitas):
- Troca de `[HarmonyPrefix]` com `ref IEnumerable<RagfairOffer> offers` para **`[HarmonyPostfix]` com `ref List<RagfairOffer> __result`** — mais simples, e é o mesmo idioma já usado em `FleaFloorOverridePatch.cs:L30-L38` (`Postfix(MongoId tpl, ref double __result)`), evitando introduzir uma técnica de Harmony (reescrita de argumento por `ref`) sem precedente no mod.
- §1, §2, §4 (nome do arquivo pode continuar `RagfairEmptyOfferGuardPatch.cs`, só muda o alvo), §5 (stub) e §6 (fluxo de dados) da spec técnica precisam ser reescritos para refletir o novo alvo — o diagrama atual também não menciona a bifurcação linked/required-search da §256-269 do `RagfairController`, que é o motivo real da intermitência por jogador observada em produção (filtro `OfferOwnerType` dentro do `.Where()` de `GetCategoriesFromOffers`/`GetValidOffers` decide se a oferta quebrada entra na busca daquele jogador específico).
- Documentar explicitamente esta troca de camada como decisão consciente (com a razão do `FlagExpiredOffersAfterDate`) — não é óbvio de fora, e um leitor futuro pode "corrigir" pra `Holder` achando ser mais direto.

**Decisão:**
- `[x]` Aceitar sugestão

**Resolução:** Usuário aprovou ("Sim"). Spec técnica ([02-spec-tech.md](001-crash-flea-oferta-sem-itens-02-spec-tech.md)) reescrita com alvo `RagfairOfferService.GetOffers()`, Postfix em `ref List<RagfairOffer> __result`, e nota explícita sobre por que não patchear `RagfairOfferHolder.GetOffers()`.

---

### PA-01-02 · A — Gap · ✅ Resolvido em 2026-09-23

**Checklist de implementação não dá um caminho concreto pra validar o guard antes de considerar o item pronto**

**Problema:** O item "Validar em ambiente de teste: se possível, forçar uma oferta com `Items` vazio (via ferramenta de debug/perfil)..." (§8 da spec técnica) não diz **como** de fato construir esse cenário. Já ficou estabelecido (spec funcional + spec técnica §2, evidência de `RagfairOfferGenerator.CreateOffer`) que uma oferta não nasce vazia — ela fica vazia por um mecanismo ainda desconhecido, então não existe um jeito óbvio de "forçar" o cenário via UI normal do jogo. O mod não tem projeto de testes (`mods/TRL-ItemsManagement/modded/Server` não tem pasta `tests/`, diferente do CompoundingPerf que tem), então "escrever um teste" não é uma ação trivial de apontar.

**Por que importa:** Sem um caminho concreto, esse item do checklist tende a ser marcado como feito sem validação real, e a única validação de fato vai ser esperar o bug ocorrer de novo em produção — o que é aceitável, mas deveria ser uma decisão explícita, não uma lacuna silenciosa.

**Sugestão:** Trocar o item do checklist por uma validação em duas camadas, explícitas:
1. **Validação por leitura de código** (imediata, sem precisar do jogo): confirmar manualmente que o método patcheado (`RagfairOfferService.GetOffers`, ou o alvo que for decidido em PA-01-01) tem uma oferta com `Items` vazio corretamente excluída do `List<RagfairOffer>` retornado — pode ser feito com um pequeno programa `dotnet-script`/console standalone referenciando o DLL compilado, chamando o método do patch diretamente (ele é `internal static`, então precisa ficar num projeto com `InternalsVisibleTo` ou virar `internal` acessível via `friend assembly` temporário) com uma lista `List<RagfairOffer>` montada à mão contendo uma entrada com `Items = new List<Item>()`.
2. **Validação em produção/staging** (a validação real): após deploy, monitorar o log por `"[TRLItemsManagement] ragfair offer sem itens descartada da busca"` — a primeira ocorrência sem nenhum crash acompanhado na mesma janela de tempo é a confirmação de que o guard funcionou pra valer.

**Decisão:**
- `[x]` Aceitar sugestão

**Resolução:** Usuário aprovou ("Sim"). Checklist da spec técnica (§8) reescrito com as duas camadas de validação sugeridas.

---

### PA-01-03 · B — Edge case · ✅ Resolvido em 2026-09-23

**Crescimento do dicionário `LoggedOfferIds` assume que ofertas quebradas são raras, mas isso não está escrito em lugar nenhum**

**Problema:** `LoggedOfferIds` (um `ConcurrentDictionary<MongoId, byte>` estático) só cresce, nunca é limpo (além de resetar num restart do servidor). Isso é uma escolha correta e proporcional **se** ofertas quebradas forem raras (a premissa de todo o item) — mas se a investigação de causa raiz que este item habilita revelar que o problema é frequente (não raro), esse dicionário cresce sem limite pela vida do processo. Não é um vazamento de memória grave (cada entrada é minúscula), mas é uma premissa não declarada.

**Por que importa:** Um leitor futuro debugando um crescimento de memória inesperado não teria como saber, sem ler o código, que esse dicionário existe e por quê ele pode crescer.

**Sugestão:** Adicionar uma frase na seção "Riscos e dependências" (§7) da spec técnica: "`LoggedOfferIds` assume que ofertas quebradas são raras (premissa central deste item). Se a causa raiz revelar alta frequência, revisitar com uma política de expiração (ex: `ConcurrentDictionary` com timestamp + limpeza periódica) — não necessário no design atual porque o volume esperado é próximo de zero."

**Decisão:**
- `[x]` Aceitar sugestão

**Resolução:** Usuário aprovou ("Sim"). Frase adicionada em §7 da spec técnica.

## Histórico

| Data | Evento |
|---|---|
| 2026-09-23 | Review 01 criada |
