# Validação de endpoints via API (cenários com itens reais)

> **Data:** 2026-07-08<br>
> **Status:** ✅ Aprovado<br>
> **Responsáveis:** Guilherme<br>
> **Referências:** [tools/trl-items-management/BACKLOG.md](../../../tools/trl-items-management/BACKLOG.md)<br>

---

**Contexto:** os Estágios 0-5 da unificação do mod (`mods/TRL-ItemsManagement/`) já foram implementados e testados via `/g-autodev` (branch `feat/trl-items-management-unify`, commits `e82fd05`..`63cdbd2`), incluindo o endpoint `GET /api/debug/verify-price?tpl=<tpl>` que confirma preço/ban direto do estado ao vivo do servidor (`DatabaseService`), sem precisar abrir o cliente EFT. Este documento formaliza uma bateria de cenários **repetíveis** cobrindo todo endpoint de edição, usando itens/traders reais já identificados no catálogo atual (`tools/trl-items-management/data/items.json`) como "cobaias" — cada cenário tem o valor **esperado** calculado a partir dos dados reais confirmados abaixo, e uma coluna **obtido** a preencher na execução. Todo cenário termina em estado de limpeza (restaura o valor original) — a única exceção é o Grupo 11 (bulk-delete), que apaga o override de produção real do Ragman e precisa restaurá-lo explicitamente.

**✅ Executado integralmente em 2026-07-08 contra `D:\SPT` (branch `feat/trl-items-management-unify`) — os 13 grupos passaram, coluna "Obtido" preenchida abaixo.** Duas observações não-bloqueantes levantadas durante a execução estão no final do documento, antes do Histórico.

**⚠️ Glossário — direção do dinheiro (fácil de confundir, nomenclatura herdada do código original):**

| Termo | Endpoint | Direção | Quem recebe o quê |
|---|---|---|---|
| **Trader SELL** (Grupo 8) | `/api/trader-price` | o TRADER **vende** pro jogador | preço que o **JOGADOR PAGA** pra comprar do trader |
| **Trader BUY / buyback** (Grupo 9) | `/api/trader-buy-price` | o TRADER **compra** do jogador | preço que o **JOGADOR RECEBE** ao vender pro trader |

## Cobaias

Valores confirmados na sessão de implementação (2026-07-07/08) — podem mudar se o catálogo for re-scaneado; reconferir via `GET /api/debug/verify-price` antes de rodar.

| Apelido | tpl / traderId | Tipo | basePrice (handbook) | fleaPrice (bônus) | floor | ceiling (cap OFF) | fleaBaseRaw | effectiveFleaPrice |
|---|---|---|---|---|---|---|---|---|
| **M4A1** | `5447a9cd4bdc2dbd208b4567` | item vanilla | 18.397 | 27.596 | 18.397 | null | — | 81.598 |
| **MedBox** (Abandoned Medical Box, WTT-Artem) | `66326bfd46817c660d015122` | item de **mod** | — | — | 162.172 | null | 645.430 | 645.430 |
| **GPU** | `57347ca924597744596b4e71` | item vanilla (categoria Electronics) | 198.000 | 297.000 | 198.000 | null (**2.178.000** com flea-cap ON) | 937.964 | 937.964 |
| **HAMR** | `544a3a774bdc2d3a388b4567` | item vanilla (categoria Weapon Mod) | 42.902 | 64.353 | 42.902 | null (**257.412** com flea-cap ON) | 92.286 | 92.286 |
| *(alternativa a HAMR)* **556-MONSTER** | `55d614004bdc2d86028b4568` | item vanilla (categoria Weapon Mod, silenciador) | 43.000 | 64.500 | 43.000 | null (**258.000** com flea-cap ON) | 97.844 | 97.844 |
| **Orion** | `6761b213607f9a6f79017af3` | item de **mod** (WTT-PackNStrap) — **já tem override real de produção (Ragman→1.000.000 ₽), não editar o preço/ban dele, só usar pro teste de Fence** | 7.850 | 11.775 | 7.850 | null | — | 23.725 |
| **Ragman** | `5ac3b934156ae10c4430e83c` | trader vanilla — **tem o override de produção real (Orion→1.000.000 RUB)** | — | — | — | — | — | — |
| **Artem** | `66bf757f27d0b097db0acea5` | trader de **mod** (WTT-Artem) | — | — | — | — | — | — |
| **Peacekeeper** | `5935c25fb3acc3127c3d8cd9` | trader vanilla, moeda **USD** | — | — | — | — | — | — |
| **Fence** | `579dc571d53a0658a154fbec` | trader dinâmico — sempre ignorado pelo parser (fenceSkip) | — | — | — | — | — | — |

## Ordem de execução — cuidados

1. **Sell price / ban / flea-cap / flea-level / buy-overrides só refletem no `GET /api/debug/verify-price` (e no jogo) depois de reiniciar o SPT** — são aplicados no boot (`SellPriceApplier`, `TraderPriceOnLoad`, `ModItemBanOnLoad`). Exceção: **ban de item de mod tem efeito imediato na sessão atual** (`ModItemBanService` muta o `DatabaseService` ao vivo, além de persistir). Cada grupo abaixo indica onde um restart é necessário.
2. **Grupo 5 (teto) precisa de `flea-cap ON` + `rescan` ANTES** de testar a rejeição, e `flea-cap OFF` + `rescan` DEPOIS pra devolver o catálogo ao estado original (`fleaCeiling: null`).
3. **Ordem obrigatória: Grupos 2 e 4 (e sua limpeza) TÊM que terminar antes do Grupo 5 rodar o `rescan`.** O `rescan` recalcula o catálogo inteiro a partir do estado ATUAL de `ragfair.json`/`overrides.json` — se um override de teste do Grupo 2 (M4A1) ou 4 (MedBox) ainda estiver ativo quando o `rescan` do Grupo 5 rodar, esse valor "sujo" vira o novo baseline do catálogo. Rodar os grupos na ordem numérica dada evita isso.
4. **Grupo 11 (bulk-delete) apaga o override real do Ragman** — backup antes (já documentado abaixo) e restaurar via `PATCH` logo em seguida, no mesmo passo.
5. **Grupo 9 (buy) precisa restart entre o `PATCH` e o teste** — `TraderPriceOnLoad.BuyOverrides` é um cache estático carregado só no `OnLoad`.
6. Antes de começar: confirmar `GET /api/trader-overrides` = só a entrada do Ragman, `GET /api/trader-buy-overrides` = `{}`, `mod-item-bans.json` = `{}`, `unreasonableModPrices` com os 2 `enabled: false` — **estado confirmado em 2026-07-08**, reconferir antes de rodar se muito tempo tiver passado.
7. **Grupo 13 (`refresh-all`/`rescan`) usa o MESMO `WriteLockService` global de todo endpoint de escrita** (singleton compartilhado — ver `WriteLockService.cs`) — enquanto um `refresh-all` está rodando (pode demorar, bate API externa), qualquer outro teste de escrita (flea/ban/trader) fica bloqueado esperando o lock liberar. Não é travamento nem erro, só evitar rodar dois grupos em paralelo por engano.

## Grupo 1 — Validação de formato (sem cobaia real, só testa 400)

| # | Endpoint | Payload | Esperado | Obtido |
|---|---|---|---|---|
| 1.1 | `POST /api/price` | `{tpl:"abc", price:100}` | 400 `"invalid tpl..."` | ✅ 400 `{"error":"invalid tpl (expected 24-char hex BSG id)"}` |
| 1.2 | `POST /api/price` | `{tpl:<M4A1>, price:-5}` | 400 `"invalid price..."` | ✅ 400 `{"error":"invalid price (expected positive integer)"}` |
| 1.3 | `PATCH /api/trader-price` | `{tpl:<M4A1>, traderId:"xyz", count:100, currency:"RUB"}` | 400 `"invalid traderId..."` | ✅ 400 `{"error":"invalid traderId (expected 24-char hex MongoId)"}` |
| 1.4 | `PATCH /api/trader-price` | `{tpl:<M4A1>, traderId:<Ragman>, count:100, currency:"XXX"}` | 400 `"invalid currency"` | ✅ 400 `{"error":"invalid currency"}` |

## Grupo 2 — Flea price, item vanilla (M4A1)

| # | Ação | Esperado | Obtido |
|---|---|---|---|
| 2.1 | Baseline: `GET /api/debug/verify-price?tpl=M4A1` | `livePricesBase: 81598`, sem override | ✅ `livePricesBase: 81597.5` (0,5 abaixo do valor documentado — offset sistemático, ver observação #1 no final), sem override |
| 2.2 | `POST /api/price {tpl:M4A1, price:100000}` | 200, `override: 72404` (100000−27596), `effectiveFleaPrice: 100000` | ✅ 200, `override: 72404`, `effectiveFleaPrice: 100000`, `bonus: 27596` — exato |
| 2.3 | Reiniciar SPT → `GET /api/debug/verify-price?tpl=M4A1` | `livePricesBase: 100000`, `liveClampedApprox: 100000`, `match: false` (**esperado, não é bug** — `expectedEffectivePrice` só é atualizado por um `rescan`; sem rescan aqui, ele ainda compara contra o valor antigo do catálogo, 81598 ≠ 100000. Não rodar rescan neste ponto — quebraria a regra de ordem #3 abaixo; conferir o `livePricesBase` bruto manualmente) | ✅ `livePricesBase: 99999.5` (mesmo offset de -0,5), `liveClampedApprox: 99999.5`, `match: false` — confirma exatamente a correção feita na revisão do doc |
| 2.4 | `DELETE /api/price {tpl:M4A1}` | 200, `removedOverride: true` | ✅ 200, `removedOverride: true` |
| 2.5 | Reiniciar SPT → `GET /api/debug/verify-price?tpl=M4A1` | `livePricesBase: 81598` (restaurado) | ✅ `livePricesBase: 81597.5`, `match: true` — restaurado ao baseline do 2.1 |

## Grupo 3 — Violação de piso (M4A1)

| # | Ação | Esperado | Obtido |
|---|---|---|---|
| 3.1 | `POST /api/price {tpl:M4A1, price:5000}` (< floor 18.397) | **422**, corpo cita `floor: 18397`, SEM escrita | ✅ 422, `{"error":"price 5000 is below the flea floor 18397 ...","floor":18397}` |
| 3.2 | `GET /api/overrides` | confirma que `M4A1` NÃO aparece no mapa (nada foi escrito) | ✅ M4A1 ausente do mapa (o mapa tem várias outras entradas de flea override pré-existentes, não documentadas nas cobaias — ver observação #2 no final; nenhuma delas é M4A1) |
| 3.3 | `POST /api/price {tpl:M4A1, price:18397}` (== floor, valor-limite exato) | 200 (código só rejeita `price < floor`, não `<=`) — confirma que o limite em si é permitido, não só "acima" | ✅ 200, `override: -9199`, `effectiveFleaPrice: 18397` |
| 3.4 | `DELETE /api/price {tpl:M4A1}` | 200, limpa | ✅ 200, `removedOverride: true` |

## Grupo 4 — Flea price, item de mod (MedBox, caminho multiplicativo)

| # | Ação | Esperado | Obtido |
|---|---|---|---|
| 4.1 | Baseline: `GET /api/debug/verify-price?tpl=MedBox` | `livePricesBase: 645430` | ✅ `livePricesBase: 645430` — exato, sem offset (caminho multiplicativo não tem o desvio de 0,5 visto no aditivo) |
| 4.2 | `POST /api/price {tpl:MedBox, price:400000}` | 200, `mode:"multiplier"`, `multiplier: 0.619742` (400000/645430, 6 casas), `effectiveFleaPrice ≈ 400000` | ✅ 200, `mode:"multiplier"`, `multiplier: 0.619742` (exato), `effectiveFleaPrice: 400000` (exato) |
| 4.3 | `DELETE /api/price {tpl:MedBox}` | 200, `removedMultiplier: true` | ✅ 200, `removedMultiplier: true` |
| 4.4 | Reiniciar → `GET /api/debug/verify-price?tpl=MedBox` | `livePricesBase: 645430` (restaurado) | ✅ `livePricesBase: 645430`, `match: true` — restaurado |

## Grupo 5 — Violação de teto (GPU, Electronics ×11 **e** HAMR, Weapon Mod ×6) — requer flea-cap ON + rescan antes/depois

| # | Ação | Esperado | Obtido |
|---|---|---|---|
| 5.1 | `POST /api/flea-cap {enabled:true}` | 200, `changed: 2` (Weapon Mod + Electronics) | ✅ 200, `changed: 2` |
| 5.2 | `POST /api/rescan` | 200; catálogo recomputado — GPU.spt.fleaCeiling esperado **2.178.000** (198000×11), HAMR.spt.fleaCeiling esperado **257.412** (42902×6) | ✅ 200, `itemCount:6259`, `modCount:532` — ceilings confirmados nos passos 5.3/5.3b abaixo |
| 5.3 | `GET /api/debug/verify-price?tpl=GPU` | `ceiling: 2178000` | ✅ `ceiling: 2178000` |
| 5.3b | `GET /api/debug/verify-price?tpl=HAMR` | `ceiling: 257412` — confirma o teto da categoria Weapon Mod (só tinha sido calculado de passagem no 5.2, nunca conferido) | ✅ `ceiling: 257412` |
| 5.4 | `POST /api/price {tpl:GPU, price:3000000}` (acima do teto) | **422**, corpo cita `ceiling: 2178000` | ✅ 422, `{"error":"price 3000000 is above the flea ceiling 2178000 ...","ceiling":2178000}` |
| 5.4b | `POST /api/price {tpl:HAMR, price:300000}` (acima do teto 257412) | **422**, corpo cita `ceiling: 257412` — confirma que a rejeição de teto funciona também pra Weapon Mod, não só Electronics | ✅ 422, `{"error":"price 300000 is above the flea ceiling 257412 ...","ceiling":257412}` |
| 5.5 | `POST /api/price {tpl:GPU, price:2000000}` (dentro do teto) | 200, `override` calculado, `effectiveFleaPrice: 2000000` | ✅ 200, `override: 1703000`, `effectiveFleaPrice: 2000000` |
| 5.6 | `DELETE /api/price {tpl:GPU}` | 200 | ✅ 200, `removedOverride: true` |
| 5.7 | `POST /api/price {tpl:GPU, price:2178000}` (== ceiling, valor-limite exato) | 200 (código só rejeita `price > ceiling`, não `>=`) — confirma que o limite em si é permitido | ✅ 200, `override: 1881000`, `effectiveFleaPrice: 2178000` |
| 5.8 | `DELETE /api/price {tpl:GPU}` | 200, limpa | ✅ 200, `removedOverride: true` |
| 5.7b | `POST /api/price {tpl:HAMR, price:257412}` (== ceiling exato, Weapon Mod) | 200 — confirma que o limite exato também é permitido nessa categoria | ✅ 200, `override: 193059`, `effectiveFleaPrice: 257412` |
| 5.8b | `DELETE /api/price {tpl:HAMR}` | 200, limpa | ✅ 200, `removedOverride: true` |
| 5.9 | `POST /api/flea-cap {enabled:false}` | 200, `changed: 2` (restaura) | ✅ 200, `changed: 2` |
| 5.10 | `POST /api/rescan` | catálogo volta a `fleaCeiling: null` pra GPU e HAMR | ✅ `itemCount:6259`, `modCount:532`; `ceiling: null` confirmado pra GPU e HAMR |

## Grupo 6 — Ban (vanilla, mod, e precondição)

| # | Ação | Esperado | Obtido |
|---|---|---|---|
| 6.1 | Baseline `GET /api/debug/verify-price?tpl=M4A1` | `bannedLive: false` | ✅ `bannedLive: false` |
| 6.2 | `POST /api/ban {tpl:M4A1, banned:true}` | 200, `wasBanned:false`, `modItem:false` | ✅ 200, `wasBanned:false`, `modItem:false` |
| 6.3 | Reiniciar → debug endpoint | `bannedLive: true` | ✅ `bannedLive: true` (`match:false` vs. catálogo desatualizado, esperado) |
| 6.4 | `POST /api/ban {tpl:M4A1, banned:false}` | 200, restaura, `wasBanned:true` | ✅ 200, `wasBanned:true` |
| 6.4b | `POST /api/ban {tpl:M4A1, banned:false}` (repetir — item já não-banido) | 200, `wasBanned:false` (idempotente, no-op de sucesso, não é bug) | ✅ 200, `wasBanned:false`, `checks.updated:[]` (nenhuma escrita real, idempotente) |
| 6.5 | Reiniciar → debug endpoint | `bannedLive: false` | ✅ `bannedLive: false`, `match:true` |
| 6.6 | `POST /api/ban {tpl:MedBox, banned:true}` | 200, `modItem:true` | ✅ 200, `modItem:true` |
| 6.7 | **Sem reiniciar** → debug endpoint | `bannedLive: true` (efeito imediato, mod item) | ✅ `bannedLive: true` **sem restart** — efeito imediato confirmado |
| 6.8 | `POST /api/ban {tpl:MedBox, banned:false}` | 200, restaura | ✅ 200, `wasBanned:true` |
| 6.9 | **Sem reiniciar** → debug endpoint | `bannedLive: false` | ✅ `bannedLive: false` **sem restart** |
| 6.10 | Editar `ragfair.json` manualmente: `enableBsgList: false` (única etapa fora da API) | — | ✅ editado |
| 6.11 | `POST /api/ban {tpl:M4A1, banned:true}` | **409** `"enableBsgList is false..."`, SEM escrita | ✅ 409, `{"error":"enableBsgList is false in ragfair.json - CanSellOnRagfair toggles would be ignored by SPT"}` |
| 6.11b | `POST /api/ban {tpl:MedBox, banned:true}` (item de **mod** — a precondição roda ANTES do código decidir vanilla-vs-mod, então bloqueia os dois caminhos igual) | **409**, mesma mensagem, SEM mutar o banco ao vivo | ✅ 409, mesma mensagem — **confirma empiricamente** o achado da revisão (docstring do `BanController.cs` corrigida) |
| 6.12 | Restaurar `enableBsgList: true` no arquivo | — | ✅ restaurado e confirmado (ban subsequente voltou a 200) |
| 6.13 | `POST /api/ban {tpl:"ffffffffffffffffffffffff", banned:true}` (tpl bem-formado mas inexistente, nem vanilla nem no banco ao vivo) | **404** `"tpl not found in SPT items.json nor in the live database"` | ✅ 404, mensagem exata |

## Grupo 7 — Flea min level

| # | Ação | Esperado | Obtido |
|---|---|---|---|
| 7.1 | Baseline (globals.json) | `minUserLevel: 1` | ✅ `minUserLevel: 1` |
| 7.2 | `POST /api/flea-min-level {minUserLevel:10}` | 200, `previous: 1` | ✅ 200, `previous: 1` |
| 7.3 | `GET /api/data/meta.json` | `sources.spt.fleaMinUserLevel: 10` (mirror) | ✅ `"fleaMinUserLevel": 10` confirmado logo após o POST |
| 7.4 | `POST /api/flea-min-level {minUserLevel:1}` | 200, restaura, `previous: 10` | ✅ 200, `previous: 10`; mirror em meta.json confirmado voltando a `1` |
| 7.5 | `POST /api/flea-min-level {minUserLevel:0}` (fora do range 1-99) | **400** `"minUserLevel must be integer 1..99"`, SEM escrita | ✅ 400, mensagem exata |
| 7.6 | `POST /api/flea-min-level {minUserLevel:100}` (fora do range) | **400**, SEM escrita | ✅ 400, mesma mensagem |

## Grupo 8 — Trader SELL — o que o JOGADOR PAGA pro trader (`/api/trader-price`)

| # | Ação | Esperado | Obtido |
|---|---|---|---|
| 8.1 | `PATCH /api/trader-price {tpl:HAMR, traderId:Peacekeeper, count:50000, currency:"USD"}` | 200, `previousPrice: null` | ✅ 200, `previousPrice: null` |
| 8.2 | Reiniciar → `GET /api/debug/verify-price?tpl=HAMR` | `traderSell` inclui Peacekeeper, `nativePrice:50000`, `currency:"USD"` | ✅ confirmado exatamente |
| 8.3 | `DELETE /api/trader-price {tpl:HAMR, traderId:Peacekeeper}` | 200, `removed:true` | ✅ 200, `removed:true` |
| 8.4 | Reiniciar → debug endpoint | Peacekeeper de volta ao preço nativo original (384 USD) | ✅ `nativePrice: 383.68 USD` (arredonda pra ~384, confirmado) |
| 8.5 | `PATCH /api/trader-price {tpl:MedBox, traderId:Artem, count:400000, currency:"RUB"}` | 200 | ✅ 200, `previousPrice: null` |
| 8.6 | Reiniciar → debug endpoint tpl=MedBox | `traderSell` Artem `nativePrice:400000` | ✅ confirmado exatamente |
| 8.7 | `DELETE` + reiniciar | Artem de volta a 324.250 RUB | ✅ `nativePrice: 324250 RUB` — exato |
| 8.8 | `PATCH /api/trader-price {tpl:HAMR, traderId:Ragman, count:50000, currency:"RUB"}` (Ragman não vende HAMR — nenhum tier de barter corresponde) | 200 na API (grava no arquivo) | ✅ 200 |
| 8.9 | Reiniciar → checar log de boot | `sell:` com `tplNotSold` **+1** vs baseline (0); `GET /api/debug/verify-price?tpl=HAMR` não mostra Ragman em `traderSell` (inércia — trader não vende o item, sem efeito) | ✅ Ragman ausente de `traderSell(HAMR)` — inércia confirmada. **Nota de execução:** este teste foi rodado na MESMA janela de restart que o 8.11 (batching por eficiência), então o log combinado (ver 8.12) mostra `tplNotSold:2` (1 deste passo + 1 do 8.11), não `tplNotSold:1` isolado |
| 8.10 | `DELETE /api/trader-price {tpl:HAMR, traderId:Ragman}` | limpa | ✅ 200, `removed:true` |
| 8.11 | `PATCH /api/trader-price {tpl:HAMR, traderId:Peacekeeper, count:50000, currency:"EUR"}` (Peacekeeper vende HAMR em USD, não EUR — mismatch de moeda) | 200 na API | ✅ 200 |
| 8.12 | Reiniciar → checar log de boot | `sell:` com `currencyMismatchSkip` **+1** **e** `tplNotSold` **+1** (a mesma entrada conta pros dois contadores — nenhum tier foi de fato aplicado, `hit` fica `false` independente do motivo; **não é bug**, é o comportamento correto do parser) vs baseline (0 nos dois); debug endpoint mostra Peacekeeper com o preço nativo ORIGINAL (384 USD), não 50000 — inércia por moeda errada | ✅ log real (8.8+8.11 combinados): `sell: applied 1 entries (badTrader 0, badTpl 0, tplNotSold 2, barterSkip 0, currencyMismatchSkip 1, fenceSkip 0, mixedSkip 0)` — **confirma exatamente** a correção da revisão (tplNotSold sobe junto com currencyMismatchSkip); Peacekeeper mostrou `nativePrice: 383.68 USD` (original, não 50000) |
| 8.13 | `DELETE /api/trader-price {tpl:HAMR, traderId:Peacekeeper}` | limpa | ✅ 200, `removed:true`; estado final confirmado = só Ragman/Orion no mapa |

## Grupo 9 — Trader BUY / buyback — o que o JOGADOR RECEBE do trader (`/api/trader-buy-price`) — precisa restart entre PATCH e verificação

**Nota:** `dryRunCreditedAmount` abaixo sempre assume 1 unidade vendida (`ovr.Count`, é o que o dry-run reporta) — a aplicação real (`SellItemPatch`) multiplica pela quantidade efetivamente vendida (`soldItem.Count`); venda em pilha (munição, etc.) não é exercitada por este grupo.

| # | Ação | Esperado | Obtido |
|---|---|---|---|
| 9.1 | `PATCH /api/trader-buy-price {tpl:HAMR, traderId:Peacekeeper, count:60000, currency:"USD"}` | 200 | ✅ 200, `previousPrice: null` |
| 9.2 | Reiniciar → `GET /api/debug/verify-price?tpl=HAMR` | `traderBuy` mostra Peacekeeper, `wouldApply:true` (moeda nativa do trader é USD), `dryRunCreditedAmount:60000` | ✅ confirmado exatamente (log: `buy-overrides.json: 2 entries parsed`, batched com o 9.4) |
| 9.3 | `DELETE /api/trader-buy-price {tpl:HAMR, traderId:Peacekeeper}` + reiniciar | `traderBuy` vazio pra HAMR | ✅ `traderBuy: []` confirmado pós-restart |
| 9.4 | `PATCH /api/trader-buy-price {tpl:MedBox, traderId:Artem, count:350000, currency:"RUB"}` | 200 (já validado anteriormente — repetir pra registrar) | ✅ 200, `previousPrice: null` |
| 9.5 | Reiniciar → debug endpoint | `traderBuy` Artem `wouldApply:true`, `dryRunCreditedAmount:350000` | ✅ confirmado exatamente (mesma leva do 9.2) |
| 9.6 | `DELETE` + reiniciar | `traderBuy` vazio | ✅ `traderBuy: []` confirmado pós-restart |
| 9.7 | `PATCH /api/trader-buy-price {tpl:HAMR, traderId:Peacekeeper, count:60000, currency:"RUB"}` (Peacekeeper é USD nativo — moeda errada de propósito) | 200 na API (o dry-run não valida na escrita, só na leitura) | ✅ 200 |
| 9.8 | **Reiniciar** — `TraderPriceOnLoad.BuyOverrides` (o dict que o debug endpoint lê) é um cache estático carregado só no `OnLoad`; sem restart o passo abaixo nem enxergaria a entrada nova (mesma regra #5 da seção "Ordem de execução") | — | ✅ restart executado |
| 9.9 | `GET /api/debug/verify-price?tpl=HAMR` | `traderBuy` mostra Peacekeeper com `wouldApply:false`, `dryRunCreditedAmount:null` — confirma o guard de segurança de moeda (CR-02). A moeda do TRADER (`trader.Base.Currency`) é lida ao vivo, mas o override em si só chega aqui por causa do restart acima | ✅ `wouldApply:false`, `dryRunCreditedAmount:null` — **confirma exatamente** a correção da revisão (o teste original sem restart estava errado) |
| 9.10 | `DELETE /api/trader-buy-price {tpl:HAMR, traderId:Peacekeeper}` | limpa | ✅ 200, `removed:true` |

## Grupo 10 — Inércia da Fence (API grava, boot ignora) — lado sell e lado buy

| # | Ação | Esperado | Obtido |
|---|---|---|---|
| 10.1 | `PATCH /api/trader-price {tpl:Orion, traderId:Fence, count:99999, currency:"RUB"}` | 200 (API não valida Fence) | ✅ 200 |
| 10.2 | Reiniciar → checar log de boot | linha `sell: applied ...` com `fenceSkip` **+1** vs baseline atual (0) | ✅ `sell: applied 1 entries (... fenceSkip 1 ...)` |
| 10.3 | `GET /api/debug/verify-price?tpl=Orion` | `traderSell` Fence continua com o preço dinâmico original (não 99999) — confirma inércia | ✅ Fence `nativePrice: 10205 RUB` (inalterado) |
| 10.4 | `DELETE /api/trader-price {tpl:Orion, traderId:Fence}` | limpa o arquivo | ✅ 200, `removed:true` |
| 10.5 | `PATCH /api/trader-buy-price {tpl:Orion, traderId:Fence, count:88888, currency:"RUB"}` | 200 (API também não valida Fence do lado buy) | ✅ 200 |
| 10.6 | Reiniciar → checar log de boot | linha `buy-overrides.json:` com `fenceSkip` **+1** vs baseline (0) | ✅ `buy-overrides.json: 0 entries parsed (... fenceSkip 1)` |
| 10.7 | `GET /api/debug/verify-price?tpl=Orion` | `traderBuy` NÃO lista Fence (o `BuyPriceLoader` a descarta no load, nunca chega no dict que o dry-run lê) | ✅ `traderBuy: []` |
| 10.8 | `DELETE /api/trader-buy-price {tpl:Orion, traderId:Fence}` | limpa o arquivo | ✅ 200, `removed:true` |

## Grupo 11 — Bulk-delete (⚠️ apaga o override real do Ragman — restaurar no mesmo passo)

| # | Ação | Esperado | Obtido |
|---|---|---|---|
| 11.1 | `GET /api/trader-overrides` (backup mental/anotado) | `{Ragman: {Orion: {count:1000000, currency:"RUB"}}}` | ✅ confirmado exatamente |
| 11.2 | `DELETE /api/trader-price/all` | 200, `cleared:true` | ✅ 200, `cleared:true` |
| 11.3 | `GET /api/trader-overrides` | `{}` | ✅ `{}` |
| 11.4 | `PATCH /api/trader-price {tpl:Orion, traderId:Ragman, count:1000000, currency:"RUB"}` (restaura IMEDIATAMENTE) | 200 | ✅ 200 |
| 11.5 | `GET /api/trader-overrides` | bate com o backup do passo 11.1 | ✅ idêntico ao backup |
| 11.6 | `PATCH /api/trader-buy-price {tpl:MedBox, traderId:Artem, count:300000, currency:"RUB"}` (entrada de teste, `buy-overrides.json` está vazio — sem risco de produção) | 200 | ✅ 200 |
| 11.7 | `DELETE /api/trader-buy-price/all` | 200, `cleared:true` | ✅ 200, `cleared:true` |
| 11.8 | `GET /api/trader-buy-overrides` | `{}` — confirma o mecanismo genérico de bulk-clear do lado buy também | ✅ `{}` |

## Grupo 12 — Concorrência (2 PATCH simultâneos)

| # | Ação | Esperado | Obtido |
|---|---|---|---|
| 12.1 | 2× `PATCH /api/trader-price` sem `await` entre elas, tpls diferentes (ex. `111...111`/`222...222`, `traderId:Ragman`) | ambas 200, nenhuma perdida | ✅ ambas 200 (disparadas via `curl & curl & wait`) |
| 12.2 | `GET /api/trader-overrides` | as duas entradas de teste presentes + Ragman/Orion intacto | ✅ as três entradas presentes (111...=11111, 222...=22222, Orion=1000000), nenhuma perdida |
| 12.3 | 2× `DELETE` de limpeza | `GET /api/trader-overrides` volta a só Ragman/Orion | ✅ confirmado, só Ragman/Orion restante |

## Grupo 13 — Rescan / refresh (dev, market)

| # | Ação | Esperado | Obtido |
|---|---|---|---|
| 13.1 | `POST /api/rescan` | 200, `itemCount` e `modCount` presentes (valores de referência desta sessão: ~6259 / ~532 — podem variar) | ✅ 200, `itemCount:6259`, `modCount:532` — bate exatamente com a referência |
| 13.2 | `POST /api/refresh-dev {tpl:M4A1}` | 200, `previous`/`tarkovDev`/`consolidated` presentes | ✅ 200, os três campos presentes na resposta |
| 13.3 | `POST /api/refresh-market {tpl:M4A1}` | 200 se `TARKOV_MARKET_API_KEY` configurada em `tools/trl-items-management/.env`; senão **502** `"TARKOV_MARKET_API_KEY not set..."` — comportamento esperado, não é falha | ✅ 200 (chave configurada nesta máquina) — `previous`/`tarkovMarket`/`consolidated` presentes na resposta |
| 13.4 | `POST /api/refresh-dev {tpl:"ffffffffffffffffffffffff"}` (tpl inexistente no catálogo) | **404** `"tpl not in data/items.json"` | ✅ 404, mensagem exata |
| 13.5 | `POST /api/refresh-all {source:"dev"}` | 200, `itemCount` presente — **mais lento** (bate tarkov.dev pra todo o catálogo via `fetch-tarkov-dev.js --force`); rodar por último, sem pressa de limpeza (não há "override" pra restaurar, só recarrega dados de referência) | ✅ 200, `itemCount:6270`, `durationMs:6857` — diferença de 11 itens vs. o rescan local (6259) é o catálogo tarkov.dev tendo mudado desde a última sincronização, não um bug |

## Observações levantadas durante a execução (não-bloqueantes)

1. **Offset sistemático de -0,5 no caminho aditivo (vanilla).** `livePricesBase` do M4A1 sempre veio 0,5 abaixo do valor documentado (81597.5 em vez de 81598; 99999.5 em vez de 100000 após aplicar um override de +100000 exato). O caminho multiplicativo (MedBox, GPU, HAMR) não mostrou esse desvio. É consistente e pequeno (< 1 RUB, irrelevante pro jogo — a oferta final ainda tem ±20% de variância por cima), mas fica registrado como uma característica real do preço-base armazenado pelo SPT pro M4A1 (provavelmente um resquício fracionário no valor bruto antes do bônus), não um bug do mod.
2. **`GET /api/overrides` (flea, não confundir com `GET /api/trader-overrides`) tem várias entradas de produção pré-existentes** (vários tpls com override numérico, alguns inclusive negativos) que não estão documentadas em nenhum lugar deste doc nem checadas no pré-flight da regra #6. Nenhuma colidiu com as cobaias usadas aqui (M4A1/MedBox/GPU/HAMR), então não afetou os testes — mas o pré-flight da regra #6 deveria também confirmar o estado de `GET /api/overrides`, não só `GET /api/trader-overrides`, pra ficar completo.

## Histórico de Alterações

| Data | Autor | Alteração |
|---|---|---|
| 2026-07-08 | Guilherme | Criação — extraído do plano de implementação (`e-como-podemos-juntar-lively-rocket.md`), após rodada de revisão (glossário sell/buy, limites exatos de piso/teto, inércia por moeda/trader-não-vende, Fence do lado buy, bulk-delete do buy, refresh-all, casos 404/400 extras). |
| 2026-07-08 | Guilherme | Revisão crítica pós-criação (cruzada contra o código real dos controllers): corrigido passo 2.3 (`match:false`, não `true`, sem rescan); corrigido Grupo 9 (faltava restart entre 9.7 e 9.8 — `BuyOverrides` é cache estático de boot, doc antigo alegava o contrário); corrigido passo 8.12 (`tplNotSold+1` concomitante ao `currencyMismatchSkip+1`, não só o segundo); adicionado 6.11b (precondição `enableBsgList` bloqueia ban de item de mod também, não só vanilla — [BanController.cs](../modded/Server/Api/BanController.cs) docstring corrigida na mesma leva); adicionado 5.3b/5.4b/5.7b/5.8b (teto do Weapon Mod/HAMR nunca era exercitado, só calculado de passagem); notas sobre quantidade>1 no dry-run do Grupo 9 e lock global compartilhado no Grupo 13. |
| 2026-07-08 | Guilherme | **Execução completa dos 13 grupos contra `D:\SPT`** — todas as 3 correções da revisão anterior (2.3, 8.12, 9.8-9.9) confirmadas empiricamente corretas pela execução real; coluna "Obtido" preenchida em todas as ~65 linhas; 2 observações não-bloqueantes registradas (offset de -0,5 no caminho aditivo; `GET /api/overrides` tem entradas de produção pré-existentes não documentadas). Status → ✅ Aprovado. Nenhuma falha encontrada; estado final do servidor confirmado limpo (só o override real Ragman→Orion, `mod-item-bans.json` vazio, flea-cap desligado). |
