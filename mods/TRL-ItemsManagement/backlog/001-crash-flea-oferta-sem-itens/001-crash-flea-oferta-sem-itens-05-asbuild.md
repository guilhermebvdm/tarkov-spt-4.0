# 001 — Crash flea oferta sem itens · As-Built

**Mod:** TRL-ItemsManagement
**Spec funcional:** [001-crash-flea-oferta-sem-itens-01-spec.md](001-crash-flea-oferta-sem-itens-01-spec.md)
**Spec técnica:** [001-crash-flea-oferta-sem-itens-02-spec-tech.md](001-crash-flea-oferta-sem-itens-02-spec-tech.md)
**Última review técnica:** [001-crash-flea-oferta-sem-itens-03-spec-tech-review-01.md](001-crash-flea-oferta-sem-itens-03-spec-tech-review-01.md)
**Build inicial:** 2026-09-23

> Documentação **pós-implementação**. Reflete o estado real do código entregue pelo `/code-mod` e atualizado por `/apply-code-review`. Quando o conteúdo aqui diverge da spec técnica, este documento ganha — a spec é planejamento, o asbuild é o que foi feito.

## Arquivos alterados (build inicial)

| Ação | Path | Resumo |
| --- | --- | --- |
| CRIADO | `mods/TRL-ItemsManagement/modded/Server/Ragfair/RagfairEmptyOfferGuardPatch.cs` | Harmony Postfix em `RagfairOfferService.GetOffers()` que remove ofertas com `Items` nulo/vazio do resultado e loga a primeira ocorrência de cada uma (dedup via `ConcurrentDictionary<MongoId, byte>`), evitando o `NullReferenceException` na busca da flea. |
| MODIFICADO (CR-01-01/02) | `mods/TRL-ItemsManagement/modded/Server/Ragfair/RagfairEmptyOfferGuardPatch.cs` | Lógica de filtro extraída para `RagfairOfferGuardCore` (compartilhada); adicionados patches para `RagfairOfferHolder.GetOfferById`, `GetOffersByTemplate` e `GetOffersByTrader` (checagem de preço médio, oferta específica, ofertas por trader); comentário de cobertura corrigido. |

Nenhum outro arquivo do mod foi modificado — o patch é descoberto automaticamente pelo `PatchAll(typeof(TraderPriceOnLoad).Assembly)` já existente em `TraderPriceOnLoad.OnLoad()` ([TraderPriceOnLoad.cs:L74](../../../modded/Server/Pricing/TraderPriceOnLoad.cs#L74)), e o logging reutiliza o campo estático `TraderPriceOnLoad.Log` (mesmo padrão de `FleaFloorOverridePatch`).

## PA-NN-MM resolvidos durante o build

> Pontos da última review técnica que foram **aplicados como parte da implementação** (não como `/apply-code-review` posterior — foram incorporados na própria spec técnica antes do build, e o código já nasceu refletindo-os).

| ID | Categoria · Impacto | Resumo da resolução |
| --- | --- | --- |
| PA-01-01 | A/C — Gap + Erro de lógica · 🔴 | Alvo do patch é `RagfairOfferService.GetOffers()` (não `RagfairCategoriesService.GetCategoriesFromOffers`), cobrindo os dois caminhos de crash identificados (categorias + `GetValidOffers`/`PassesSearchFilterCriteria`) num único ponto, sem interferir no ciclo de expiração de `RagfairOfferHolder`. |
| PA-01-02 | A — Gap · 🟡 | Checklist de validação (§8 da spec técnica) reescrito com um caminho concreto de duas camadas (leitura de código + monitoramento de log em produção). |
| PA-01-03 | B — Edge case · 🟢 | `LoggedOfferIds` documentado em §7 da spec técnica como assumindo volume raro de ofertas quebradas. |

## Mudanças posteriores

> Atualizado por `/apply-code-review` a cada rodada. Cada entrada lista os achados aplicados/rejeitados/pulados naquela rodada e os arquivos tocados.

**Rodada 01 (2026-09-23):** CR-01-01 (🟠, aplicado) e CR-01-02 (🟢, aplicado) — ver [001-crash-flea-oferta-sem-itens-04-code-review-01.md](001-crash-flea-oferta-sem-itens-04-code-review-01.md). Nenhum achado rejeitado ou pulado.

**Build manual (2026-09-23):** `dotnet build TRLItemsManagement.csproj -c Release -p:SkipDeploy=true` (sem instalar no jogo, a pedido do usuário). Achado e corrigido em compilação real: faltava `using SPTarkov.Server.Core.Extensions;` para `IsTraderOffer()`/`IsFakePlayerOffer()` — erro que a leitura estática do código não pegou. `References/0Harmony.dll` populado manualmente a partir do `.spt-path` (`E:/Tarkov Red Line/BepInEx/core/0Harmony.dll`) — `/compile-mod` ainda não resolve isso automaticamente para o tipo `server-csharp`. Build final: 0 erros, 0 warnings.

**Fix 01 (2026-09-23):** Validação em produção (`D:\SPT 4.0`, outro computador) mostrou o crash persistindo mesmo com o patch confirmadamente executando (log de diagnóstico temporário provou isso). Causa: `IsBroken` só checava `Items.Count`, mas o caso real era uma lista **não-vazia com o primeiro elemento nulo** — corrigido pra `offer.Items?.FirstOrDefault() is null`, espelhando exatamente a expressão vanilla que quebra. Ver [001-crash-flea-oferta-sem-itens-06-fix-01.md](001-crash-flea-oferta-sem-itens-06-fix-01.md). Validação em produção ainda pendente (aguardando novo deploy).

## Histórico

| Data | Evento |
| --- | --- |
| 2026-09-23 | Build concluído via `/code-mod` |
| 2026-09-23 | Aplicação de 2 achados de code-review 01 via `/apply-code-review` — IDs: CR-01-01, CR-01-02 |
| 2026-09-23 | Fix 01 aplicado — `IsBroken` corrigido após validação em produção revelar caso não coberto (lista não-vazia, primeiro elemento nulo) |
| 2026-09-23 | Fix 02 aplicado — crash persistiu após Fix 01; `IsBroken`/`LogBrokenOfferOnce` blindados contra a entrada da lista em si ser nula (mesma mensagem/stack do NRE, causa distinta) |
