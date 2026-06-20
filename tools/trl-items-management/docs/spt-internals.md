# SPT 4.0 — Internals relevantes ao trl-items-management

Notas técnicas dos pontos do SPT que o `trl-items-management` lê/escreve. Validadas empiricamente em SPT 4.0.13.

## `SPT_Data/checks.dat` — manifest de integridade

Validado pelo SPT a cada boot. Cada arquivo `templates/*.json` (e vários `configs/*.json`) tem um hash esperado; divergência → log `Warn` no boot (não bloqueia, mas polui).

| Aspecto | Valor |
| --- | --- |
| Localização | `<SPT_DATA>/checks.dat` |
| Formato bruto | Base64 de UTF-8, terminado com `\n` |
| Conteúdo decodificado | JSON 2-space indent: `[ { "Path": "database/templates/handbook.json", "Hash": "C1BB93BA..." }, ... ]` |
| Algoritmo de hash | **MD5 hex maiúsculo** |
| Entradas | 316 arquivos (em SPT 4.0.13) |
| Logger que reclama | `SPTarkov.Server.Core.Utils.DatabaseImporter` |

Round-trip seguro: `Buffer.from(JSON.stringify(manifest, null, 2), 'utf8').toString('base64') + '\n'` — byte-for-byte igual ao original.

O `serve.js` mantém este arquivo sincronizado automaticamente após cada edit via `updateSptChecks()`. Arquivos rastreados que o pipeline escreve:

- `database/templates/handbook.json` — editado por `/api/price`
- `database/templates/items.json` — editado por `/api/ban`
- `database/globals.json` — editado por `/api/flea-min-level`

## Flea — como o SPT 4.0 calcula o preço de oferta

Validado contra o código fonte vendored em `references/spt-source/` (SHA `c87cc3c6...`) e 12 cenários empíricos em [flea-formula-validation.md](flea-formula-validation.md).

### Variáveis

| Símbolo | Significado |
|---|---|
| `P_disco(tpl)` | `prices.json[tpl]` no disco antes do boot (ou ausente) |
| `H(tpl)` | `handbook.Items[tpl].Price` |
| `m` | `priceMultiplier` (default 1.5) |
| `c(tpl)` | `0.8` se `tpl` é ingrediente em ≥1 receita do hideout, senão `0` |
| `T(tpl)` | trader buy price (só se `PreventPriceBeingBelowTraderBuyPrice=true`) |
| `O(tpl)` | `itemPriceOverrideRouble[tpl]` em `ragfair.json` (ou ausente) |

### Fórmula em 3 passos no boot do server

> ⚠️ **Ordem corrigida (2026-06-07, validada in-game):** o override entra **ANTES** do bonus e é **somado**, não sobrescreve. A versão anterior deste doc dizia "passo 3 sobrescreve total" — **errado**.

```text
Passo A — PostDbLoadService.ApplyFleaPriceOverrides  (roda PRIMEIRO):
  se O(tpl) existe: Templates.Prices[tpl] = O(tpl)     ← assignment (substitui prices.json)

Passo B — RagfairPriceService.ReplaceFleaBasePrices  (roda DEPOIS):
  bonus(tpl) = H(tpl) × (M(tpl) + c(tpl))              M = tplOverride|tipoOverride|m(1.5) ; c = 0.8 se craft
  se PreventPriceBeingBelowTraderBuyPrice e T(tpl) > bonus: bonus ← T(tpl)
  Templates.Prices.AddOrUpdate(tpl, bonus)             ← += (key já existe via Passo A / prices.json)

⇒ base_mem(tpl) = (O(tpl) ?? P_disco(tpl) ?? 0) + bonus(tpl)
```

O passo B **soma** por causa do `AddOrUpdate` em [`DictionaryExtensions.cs:12-19`](../../../references/spt-source/Libraries/SPTarkov.Server.Core/Extensions/DictionaryExtensions.cs#L12-L19) — se a key existe faz `dict[key] += value`. Como o Passo A já pôs o override na key, o bonus é somado por cima.

### Geração de oferta (runtime) — com piso e teto

```text
price = base_mem(tpl)
se useTraderPriceForOffersIfHigher e T(tpl) > price: price = T(tpl)            ← PISO (T = H × K_trader ≈ H)
se tpl ∈ unreasonableModPrices e price > H × overMult: price = H × newMult     ← TETO (mods ×6, electronics ×11)
(adjustPriceWhenBelowHandbookPrice = OFF neste install)
price ×= ItemPriceMultiplier[tpl]   (mapa manual)
price ×= qualityModifier            (se não em IgnoreQualityPriceVarianceBlacklist)
range    = priceRanges.default (0.8..1.2) | preset (0.95..1.05) | pack (0.75..0.96)
variance = GetBiasedRandomNumber(min,max,2,2) — re-rola fora do range (clamp RÍGIDO)
oferta   = price × variance
```

**Consolidado:** `offerBase = clamp((override ?? prices.json ?? 0) + bonus, floor, ceiling)`, com `floor = H × K_trader`, `ceiling = H × unreasonableMult` (ou ∞).

### Referências no código fonte

| Componente | Arquivo |
|---|---|
| Soma do passo 2 | [Extensions/DictionaryExtensions.cs:12-19](../../../references/spt-source/Libraries/SPTarkov.Server.Core/Extensions/DictionaryExtensions.cs#L12-L19) |
| Passo 2 (ReplaceFleaBasePrices) | [Services/RagfairPriceService.cs:73-103](../../../references/spt-source/Libraries/SPTarkov.Server.Core/Services/RagfairPriceService.cs#L73-L103) |
| Passo 3 (overrides) | [Services/PostDbLoadService.cs:122,789-796](../../../references/spt-source/Libraries/SPTarkov.Server.Core/Services/PostDbLoadService.cs#L789-L796) |
| Multiplier por baseclass / per-item | [Services/RagfairPriceService.cs:148-166](../../../references/spt-source/Libraries/SPTarkov.Server.Core/Services/RagfairPriceService.cs#L148-L166) |
| Trader-buy-floor (bonus, Passo B) | [Services/RagfairPriceService.cs:90-99](../../../references/spt-source/Libraries/SPTarkov.Server.Core/Services/RagfairPriceService.cs#L90-L99) |
| Piso de oferta (useTraderPriceForOffersIfHigher) | [Services/RagfairPriceService.cs:316-323](../../../references/spt-source/Libraries/SPTarkov.Server.Core/Services/RagfairPriceService.cs#L316-L323) |
| Teto (AdjustUnreasonablePrice / unreasonableModPrices) | [Services/RagfairPriceService.cs:389-407](../../../references/spt-source/Libraries/SPTarkov.Server.Core/Services/RagfairPriceService.cs#L389-L407) |
| `GetHighestSellToTraderPrice` (K_trader) | [Helpers/TraderHelper.cs:485-520](../../../references/spt-source/Libraries/SPTarkov.Server.Core/Helpers/TraderHelper.cs#L485-L520) |
| Variância (clamp rígido) | [Utils/RandomUtil.cs GetBiasedRandomNumber](../../../references/spt-source/Libraries/SPTarkov.Server.Core/Utils/RandomUtil.cs) |
| Quality modifier | [Services/RagfairPriceService.cs:298-380](../../../references/spt-source/Libraries/SPTarkov.Server.Core/Services/RagfairPriceService.cs#L298-L380) |
| Set de craft items do hideout | `database/hideout/production.json` (campos `recipes[].requirements[]` onde `type === "Item"`) |

### Caminhos para editar preço de flea programaticamente

1. **Override compensado (usado pelo viewer)** — escrever `ragfair.json:dynamic.itemPriceOverrideRouble[tpl] = X − bonus`. O boot soma o bonus de volta → `base = X`; oferta = `X × variance`. Válido para `floor ≤ X ≤ ceiling`. Reversível removendo a key. **Não muda handbook in-game.**
2. **Handbook + zerar prices.json** — escrever `handbook[tpl] = X / (m + c)` E garantir `prices.json[tpl]` ausente ou 0. Resultado: oferta ≈ `X × variance` (sem quality, se aplicável). Muda handbook in-game.
3. **Handbook puro (sem zerar prices.json)** — risco silencioso: se `prices.json[tpl]` tem valor não-trivial, a soma do passo 2 mantém o legado e a edição vira `X + legado`. Resultado imprevisível. **Não recomendado.**

### `templates/prices.json` — como é usado pelo SPT

Read no passo 1 e populado em memória pelo passo 2 (handbook × multiplier). NÃO é reescrito em disco automaticamente pelo SPT vanilla — o disco fica como o user/LiveFleaPrices deixou. Edições manuais em `prices.json[tpl]` persistem entre boots no disco mas em runtime são SOMADAS ao bonus do handbook (passo 2).

Histórico de semânticas do arquivo (para referência):

| Estado | Origem do número |
| --- | --- |
| SPT vanilla (fresh install) | Pré-gerado pela BSG |
| Com LiveFleaPrices ativo | Snapshot do repo DrakiaXYZ, modo pve ou regular |
| Após calibração manual | Valor autoral — mas o pipeline não usa mais esse arquivo |

**Persistência do mod em runtime:** o LiveFleaPrices muta a tabela em memória (`database.GetTables().Templates.Prices`). SPT só persiste em shutdown limpo. `Ctrl+C` no console NÃO conta.

## Mod `DrakiaXYZ-LiveFleaPrices` — comportamento upstream

Fonte: [github.com/DrakiaXYZ/SPT-LiveFleaPrices-CSharp](https://github.com/DrakiaXYZ/SPT-LiveFleaPrices-CSharp).

**Não consulta tarkov.dev diretamente.** Baixa JSON pré-agregado diário:

- `https://raw.githubusercontent.com/DrakiaXYZ/SPT-LiveFleaPriceDB/main/prices-pve.json`
- `https://raw.githubusercontent.com/DrakiaXYZ/SPT-LiveFleaPriceDB/main/prices-regular.json`

### Lógica do `OnLoad()`

```text
1. Lê config.json + blacklist.json do próprio mod
2. Desabilita: ragfair.Dynamic.GenerateBaseFleaPrices.UseHandbookPrice = false   ← subverte calibração
3. Clona priceTable atual em memória (snapshot pra clamp via maxIncreaseMult)
4. Se now > config.nextUpdate AND !disablePriceFetching → fetch
5. UpdatePrices(fetch): GET prices-{mode}.json → aplica em memória com cap opcional
6. Inicia background task: loop infinito Sleep(1h) → UpdatePrices()
```

**Atenção**: o passo 2 desabilita `useHandbookPrice`, subvertendo a calibração via handbook mesmo que o arquivo `handbook.json` tenha sido editado. Manter o mod desativado enquanto a calibração importa.

### Config flags relevantes

| Flag | Efeito |
| --- | --- |
| `nextUpdate` | Timestamp Unix do próximo fetch. `0` = força fetch no boot |
| `pvePrices` | `true` → modo pve; `false` → modo regular (PVP) |
| `disablePriceFetching` | `true` → aplica cache local mas não busca nem inicia loop |
| `maxIncreaseMult` | Cap multiplicativo sobre basePrice do handbook. Default 10 |

## Trader `assort.json` — gotchas

`<SPT_DATA>/database/traders/<traderId>/assort.json`:

```jsonc
{
  "items":             [ { "_id": "<assortInstance>", "_tpl": "<bsgTpl>", "parentId": "hideout"|<parentAssortId>, ... } ],
  "barter_scheme":     { "<assortInstance>": [[{ "_tpl": "<currencyTplOrItemTpl>", "count": <n> }]] },
  "loyal_level_items": { "<assortInstance>": 1|2|3|4 }
}
```

1. **Chaves do `barter_scheme` e `loyal_level_items` são `_id` (instância), não `_tpl` (template).** Construir map `assortId → tpl` antes de resolver preços.
2. **Filtrar `parentId === "hideout"`** — só itens raiz. Filhos (mods de arma de preset) não são vendidos individualmente.
3. **Presets de armas**: `priceRUB` do barter_scheme reflete o kit inteiro, não a arma nua.
4. **Moedas reconhecidas:**
   - `5449016a4bdc2d6f028b456f` → RUB
   - `5696686a4bdc2da3298b456a` → USD
   - `569668774bdc2da2298b4568` → EUR
5. **Conversão USD/EUR → RUB**: lida do `handbook.Items[].Price` para os tpls de moeda. **Não está em `ragfair.json`** apesar do nome sugerir.
6. **Barters complexos**: `barter_scheme[id]` pode ter múltiplos requisitos AND. Pegamos só `[0][0]`.
7. **Dedup intra-trader**: mesmo `_tpl` pode aparecer N vezes. Manter entrada com menor `loyaltyLevel`, em empate menor preço.

### Quest-locked

`questassort.json` (se existir): `{ "success": { "<assortId>": "<questId>" }, "started": {...}, "fail": {...} }`. Qualquer `assortId` nessas 3 chaves → `questLocked: true`. Não resolvemos qual quest.

## Trader assort de mods (`user/mods/*/db/CustomItems/`)

Mods como WTT-PackNStrap **não escrevem em `assort.json`** — injetam as ofertas em memória via servidor Node.js no boot. O pipeline lê os preços diretamente do JSON do mod (campo `traders.TRADERNAME.assortId.barters`).

Mapeamento de moeda em mods: `"MONEY_ROUBLES"` no `_tpl` de barters = RUB. Conversão USD/EUR usa as mesmas taxas do handbook.

## Flea blacklist — múltiplas fontes

Validado no código fonte vendored em `references/spt-source/` ([RagfairServerHelper.cs:35-87](../../../references/spt-source/Libraries/SPTarkov.Server.Core/Helpers/RagfairServerHelper.cs#L35-L87)).

Item banido na geração de oferta dinâmica quando satisfaz qualquer:

1. **Per-item BSG flag**: `items.json._props.CanSellOnRagfair === false`. Gate ativo enquanto `configs/ragfair.json:dynamic.blacklist.enableBsgList = true` (default).
2. **Custom server-side**: `configs/ragfair.json:dynamic.blacklist.custom` — array de Tpls.
3. **Por categoria**: `configs/ragfair.json:dynamic.blacklist.customItemCategoryList` quando `enableCustomItemCategoryList = true`.
4. **Quest items**: ativo quando `enableQuestList = true`.
5. **Damaged ammo packs**: ammo box com `_damaged` no nome quando `damagedAmmoPacks = true`.

### `blacklist.custom` em 4.0 — dupla semântica

Diferente de versões anteriores (onde foi documentado como "não desserializa"), no SPT 4.0.13 o campo **funciona e tem dois efeitos**:

- **Gate negativo** ([RagfairServerHelper.cs:96](../../../references/spt-source/Libraries/SPTarkov.Server.Core/Helpers/RagfairServerHelper.cs#L96)): bane o tpl da geração de oferta dinâmica.
- **Exceção em `SetAllDbItemsAsSellableOnFlea`** ([PostDbLoadService.cs:768-781](../../../references/spt-source/Libraries/SPTarkov.Server.Core/Services/PostDbLoadService.cs#L768-L781)): o boot percorre itens com `CanSellOnRagfair=false` e os promove a `true`, **a menos que** estejam em `blacklist.custom`. Inversão de semântica — aqui o campo funciona como "lista de exceções à promoção em massa".

Para banir um item com certeza: adicionar a `blacklist.custom`. Para garantir que continue vendável: ele precisa ter `CanSellOnRagfair=true` em `items.json` E não estar em `blacklist.custom`.

Não implementado no pipeline (limitação):

- `enableQuestList: true` — exigiria cruzar com `quests.json` + estado do perfil
- `traderItems: false` — exigiria resolver exclusividade de trader

## Categorias — três taxonomias diferentes

| Taxonomia | Onde vive | Para que serve |
| --- | --- | --- |
| **tarkov.dev category** | GraphQL `items.category { id name parent { id } }` | UI tipo handbook. **Usada no viewer.** |
| **`items.json._parent`** | Item template SPT | Hierarquia de **classe BSG** (herança de props). **Não usar para UI.** |
| **handbook.json `Items[].ParentId`** | Handbook | Hierarquia in-game do handbook. IDs distintos dos outros dois. |

`normalize.js` prefere tarkov.dev (mais rico). Items só-no-SPT ou de mods usam fallback de `handbookParentId` → nome via locale.

## Locale-resolved names

`<SPT_DATA>/database/locales/global/en.json` — formato `{ "<tpl> Name": "Display Name", "<tpl> ShortName": "..." }` (chave tem **espaço** entre tpl e atributo). `_props.Name` é só a chave i18n, não o display name.

Itens de mods têm locale inline (`def.locales.en.name`) — não usam `en.json` do SPT.

## Resumo do fluxo de dados

```text
tarkov.dev GraphQL ──┐
                     ├─ fetch ──► cache/*-raw.json ──┐
tarkov-market REST ──┘                                │
                                                      ├─ normalize.js ──► data/items.json
SPT (D:/SPT/SPT) ────► load-spt.js ──► spt-raw.json ─┘
  ├─ items.json (template + conditionType + ban flag)
  ├─ handbook.json (basePrice; fleaPrice vanilla derivado de basePrice × multiplier)
  ├─ hideout/production.json (set de craft items para detectar isHideoutCraftItem)
  ├─ globals.json (flea minUserLevel)
  ├─ traders/*/assort.json (buy-from-trader prices)
  ├─ traders/*/questassort.json (quest-locked flags)
  ├─ locales/global/en.json (display names)
  ├─ configs/ragfair.json (blacklist config + itemPriceOverrideRouble → fleaOverride)
  └─ user/mods/*/db/CustomItems/*.json(c) (mod-added items, passo 4c)

Viewer ──► PATCH /api/price ──► ragfair.json (itemPriceOverrideRouble[tpl] = desiredFlea)
                           └──► checks.dat (hash refresh)
                           └──► data/items.json (sync fleaOverride + effectiveFleaPrice)
                           └──► logs/override-edits.jsonl (audit)

       ──► DELETE /api/price ──► ragfair.json (remove key)
                            └──► checks.dat (hash refresh)
                            └──► data/items.json (sync)
                            └──► logs/override-edits.jsonl (audit)

       ──► GET /api/overrides ──► retorna mapa { tpl: price } atual

       ──► POST /api/ban ──► items.json SPT (CanSellOnRagfair toggle)
                        └──► checks.dat (hash refresh)
                        └──► data/items.json (sync)
                        └──► logs/ban-edits.jsonl (audit)

       ──► POST /api/flea-min-level ──► globals.json (RagFair.minUserLevel)
                                   └──► checks.dat (hash refresh)
```

### Schema de cada item em `data/items.json` (campo `spt`)

| Campo | Origem | Significado |
|---|---|---|
| `basePrice` | `handbook.Items[tpl].Price` | Preço base vanilla (não muda em runtime) |
| `fleaPrice` | computado | `basePrice × fleaMultiplier` — preço vanilla esperado |
| `fleaMultiplier` | computado | `1.5` ou `2.3` (se hideout craft) |
| `isHideoutCraftItem` | `production.json` | `true` se aparece como ingrediente em ≥1 receita |
| `fleaOverride` | `ragfair.json:itemPriceOverrideRouble[tpl]` | Valor escrito pelo viewer, ou `null` |
| `effectiveFleaPrice` | computado | `fleaOverride ?? fleaPrice` — o preço que o flea de fato vai usar |

**Source of truth para flea SPT (com override aplicado)**: o passo 3 do boot. Editável via viewer escrevendo em `ragfair.json:dynamic.itemPriceOverrideRouble[tpl]`. Não modifica handbook (item continua aparecendo no menu Handbook do EFT com o preço original).
