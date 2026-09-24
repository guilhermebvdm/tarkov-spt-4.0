# 001 — Fix 02 · Entrada nula na lista de ofertas não coberta pelo Fix 01

**Mod:** TRL-ItemsManagement
**Item raiz:** [001-crash-flea-oferta-sem-itens-01-spec.md](001-crash-flea-oferta-sem-itens-01-spec.md)
**Asbuild:** [001-crash-flea-oferta-sem-itens-05-asbuild.md](001-crash-flea-oferta-sem-itens-05-asbuild.md)
**Criado:** 2026-09-23
**Disparado por:** Validação em produção do Fix 01 — crash idêntico persistiu mesmo com `IsBroken` corrigido pra checar `Items.FirstOrDefault()`.

## Contexto

Depois do [Fix 01](001-crash-flea-oferta-sem-itens-06-fix-01.md) (checagem trocada de `Items.Count` pra `Items.FirstOrDefault()`), o usuário reinstalou no mesmo servidor (`D:\SPT 4.0`) e o crash aconteceu de novo, stack trace idêntico, sem nenhuma linha nova de log (nem `"descartada"` nem `"guard failed"` no trecho compartilhado).

## Causa raiz

Hipótese: a entrada da **lista** pode ser `null` — ou seja, não é "uma oferta com `Items` ruim", é "a própria oferta é `null`" dentro do `List<RagfairOffer>` retornado por `RagfairOfferService.GetOffers()`. `x.Items` num `x` nulo (linha do `.GroupBy(x => x.Items.FirstOrDefault().Template)`, [RagfairCategoriesService.cs:66](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Services/RagfairCategoriesService.cs#L66)) dá **exatamente a mesma mensagem e o mesmo stack trace** que os outros dois casos — não dá pra diferenciar só pelo log de erro do usuário.

Efeito colateral crítico: como o `IsBroken` do Fix 01 não tinha proteção própria contra receber um `offer` nulo, `offer.Items` dentro dele **também lançaria** — e como não havia `try/catch` ali, a exceção subia pro `Postfix`, era pega pelo `catch` de fora (que logaria `"guard failed"`), e a lista **nunca era reatribuída**. Resultado: saía sem filtro nenhum, silenciosamente — o mesmo sintoma de antes de qualquer fix.

Não é possível confirmar com 100% de certeza que ESSE é o mecanismo exato sem ver a linha `"guard failed"` do log real (não compartilhada até agora) — mas é a explicação mais simples e completa disponível, e a correção é estritamente mais segura que o estado anterior de qualquer forma (nunca faz mal tratar entrada nula com segurança).

## Mudanças aplicadas

| Arquivo | Mudança |
|---|---|
| `modded/Server/Ragfair/RagfairEmptyOfferGuardPatch.cs` | `IsBroken` ganhou seu próprio `try/catch` — qualquer exceção ao inspecionar uma oferta agora é tratada como "quebrada" em vez de propagar. `IsBroken`/`LogBrokenOfferOnce` agora aceitam `RagfairOffer?` e tratam `offer is null` explicitamente (log próprio, sem `Id` disponível pra dedupe, uma vez por processo). Isso isola cada item da lista — uma entrada problemática não derruba mais o filtro inteiro. |

## Checklist de validação (obrigatório antes de marcar o fix como entregue)

- [x] Compila sem erros (`dotnet build TRLItemsManagement.csproj -c Release -p:SkipDeploy=true` — 0 erros, 0 warnings)
- [ ] **Em produção:** busca na flea não quebra mais no servidor real (`D:\SPT 4.0`) — pendente de novo deploy + teste
- [ ] Se ainda quebrar: pedir o log completo (não só o trecho do crash) pra confirmar se aparece `"guard failed"` (confirmaria a hipótese deste fix) ou nada (indicaria uma quarta causa ainda não mapeada)
- [ ] Memória do mod atualizada (`/update-memory`) com a lição: erro idêntico (mesma mensagem/stack) pode ter múltiplas causas distintas na mesma linha (`Items` vazio, `Items` não-vazio com primeiro elemento nulo, oferta em si nula) — não assumir que a primeira explicação plausível é a única

## Histórico

| Data | Evento |
|---|---|
| 2026-09-23 | Fix criado — hipótese de entrada nula na lista, guard blindado contra qualquer exceção de inspeção; aguardando validação em produção |
