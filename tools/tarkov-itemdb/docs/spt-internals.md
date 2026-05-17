# SPT 4.0 — Internals relevantes ao tarkov-itemdb

Notas técnicas dos pontos do SPT que o `tarkov-itemdb` lê/escreve. Validadas empiricamente em SPT 4.0.13.

## `SPT_Data/checks.dat` — manifest de integridade

Validado pelo SPT a cada boot. Cada arquivo `templates/*.json` (e vários `configs/*.json`) tem um hash esperado; divergência → log `Warn` no boot (não bloqueia, mas polui).

| Aspecto | Valor |
|---|---|
| Localização | `<SPT_DATA>/checks.dat` |
| Formato bruto | Base64 de UTF-8, terminado com `\n` |
| Conteúdo decodificado | JSON 2-space indent: `[ { "Path": "database/templates/items.json", "Hash": "C1BB93BA..." }, ... ]` |
| Algoritmo de hash | **MD5 hex maiúsculo** |
| Entradas | 316 arquivos (em SPT 4.0.13) |
| Logger que reclama | `SPTarkov.Server.Core.Utils.DatabaseImporter` |

Round-trip seguro: `Buffer.from(JSON.stringify(manifest, null, 2), 'utf8').toString('base64') + '\n'` — byte-for-byte igual ao original.

O `serve.js` mantém este arquivo sincronizado automaticamente após cada edit. Implementação em `updateSptChecks()`.

## `templates/prices.json` — semântica opaca

`{ "<bsgTpl>": <roublesInt> }`. **Não é uma média fixa** — o significado depende de quem escreveu o arquivo:

| Estado | Origem do número |
|---|---|
| SPT vanilla (fresh install) | Pré-gerado pela BSG, próximo ao avg histórico |
| Com LiveFleaPrices ativo, modo `regular` (PVP) | Snapshot do repo do Drakia, modo PVP |
| Com LiveFleaPrices ativo, modo `pve` | Snapshot do repo do Drakia, modo PVE |
| Após calibração manual via viewer | Valor autoral do player |

**Não tente comparar 1:1 com `tarkov.dev avg24h` ou `lastLow`** sem antes confirmar qual estado o arquivo está. O `consolidated.priceFleaSource` no `items.json` ajuda a diferenciar.

**Persistência do mod em runtime:** o LiveFleaPrices muta a tabela de preços **em memória** (`database.GetTables().Templates.Prices`). SPT só escreve isso de volta ao `prices.json` em shutdown limpo. `Ctrl+C` no console NÃO conta como shutdown limpo — perde mudanças in-memory.

## Mod `DrakiaXYZ-LiveFleaPrices` — comportamento upstream

Fonte: [github.com/DrakiaXYZ/SPT-LiveFleaPrices-CSharp](https://github.com/DrakiaXYZ/SPT-LiveFleaPrices-CSharp).

**Não consulta tarkov.dev diretamente.** Baixa um JSON pré-agregado:

- `https://raw.githubusercontent.com/DrakiaXYZ/SPT-LiveFleaPriceDB/main/prices-pve.json`
- `https://raw.githubusercontent.com/DrakiaXYZ/SPT-LiveFleaPriceDB/main/prices-regular.json`

Atualizado **diariamente** pelo Drakia (commits em `SPT-LiveFleaPriceDB`).

### Lógica do `OnLoad()`

```
1. Lê config.json + blacklist.json do próprio mod
2. Desabilita SPT internal price gen: ragfair.Dynamic.GenerateBaseFleaPrices.UseHandbookPrice = false
3. Clona priceTable atual em memória (snapshot pra clamp via maxIncreaseMult)
4. Se now > config.nextUpdate AND !disablePriceFetching → fetch
5. UpdatePrices(fetch):
   - mode = pvePrices ? "pve" : "regular"
   - GET https://.../prices-{mode}.json (5 retries)
   - Salva em mod_config/prices-{mode}.json (cache local)
   - Atualiza priceTable em memória aplicando maxIncreaseMult cap se maxLimiter=true
   - Seta config.nextUpdate = now + 3600 (1h)
6. Inicia background task: loop infinito Sleep(1h) → UpdatePrices()
```

### Config flags

| Flag | Efeito |
|---|---|
| `nextUpdate` | Timestamp Unix do próximo fetch permitido. `0` = força fetch no boot. |
| `pvePrices` | `true` → modo `pve`; `false` → modo `regular` (PVP) |
| `disablePriceFetching` | `true` → mod aplica cache local em memória mas não busca nem inicia o loop de refresh |
| `maxIncreaseMult` | Cap multiplicativo sobre o `basePrice` do handbook. Default 10 |
| `maxLimiter` | `true` ativa o cap; `false` aceita qualquer preço fetchado |

### Implicações práticas

- "Desativar mod" e "manter snapshot atual" são duas coisas diferentes (ver README seção "Re-habilitar"). Renomear `.dll` é o gate seguro porque o loader procura `*.dll` literal.
- `prices-pve.json` no cache local do mod **só é criado após primeiro fetch bem-sucedido**. Se a pasta `config/` só tem `config.json` + `blacklist.json`, o fetch nunca completou — pode baixar manualmente do upstream pra forçar.
- Modo `regular` deixa resíduo: se você trocou de regular pra pve, o `prices-regular.json` antigo permanece no cache. Inofensivo.

## Trader `assort.json` — gotchas

`<SPT_DATA>/database/traders/<traderId>/assort.json` tem 3 partes:

```jsonc
{
  "items":             [ { "_id": "<assortInstance>", "_tpl": "<bsgTpl>", "parentId": "hideout"|<parentAssortId>, ... } ],
  "barter_scheme":     { "<assortInstance>": [[{ "_tpl": "<currencyTplOrItemTpl>", "count": <n> }]] },
  "loyal_level_items": { "<assortInstance>": 1|2|3|4 }
}
```

### Pegadinhas

1. **Chaves do `barter_scheme` e `loyal_level_items` são `_id` (instância), não `_tpl` (template).** Construir map `assortId → tpl` antes de resolver preços.
2. **Filtrar `parentId === "hideout"`** — só itens "raiz" da assort. Filhos (mods de arma, peças de preset) são parte do offer composto, não vendidos individualmente.
3. **Presets de armas** (M4A1 já configurada): `priceRUB` do `barter_scheme` reflete o **kit inteiro**, não a arma "nua". Limitação aceita — comparação direta com flea fica enviesada para esses itens.
4. **Moedas reconhecidas no `_tpl` do barter_scheme**:
   - `5449016a4bdc2d6f028b456f` → RUB
   - `5696686a4bdc2da3298b456a` → USD
   - `569668774bdc2da2298b4568` → EUR
5. **Conversão USD/EUR → RUB**: lida do `handbook.Items[].Price` para os tpls de moeda (USD=120, EUR=133 em SPT 4.0.13). **Não está em `ragfair.json`** apesar do nome sugerir.
6. **Barters complexos**: `barter_scheme[id]` pode ter múltiplos requisitos AND (`[[req1, req2]]`). Pegamos só `[0][0]` por simplicidade — offers "item + dinheiro" subestimam preço.
7. **Dedup intra-trader**: mesmo `_tpl` pode aparecer N vezes (stacks paralelos). Convenção: manter entrada com **menor `loyaltyLevel`**, em empate menor preço (deal mais acessível).

### Quest-locked

`<SPT_DATA>/database/traders/<traderId>/questassort.json` (se existir):

```jsonc
{ "success": { "<assortId>": "<questId>" }, "started": {...}, "fail": {...} }
```

Qualquer `assortId` nessas 3 chaves → offer é gated por quest. Resolvemos só como flag `questLocked: true`, não qual quest (o que exigiria cruzar com `quests.json` + estado do perfil — fora de escopo).

## Flea blacklist — duas fontes

Item banido do flea quando satisfaz qualquer:

1. **Per-item BSG flag**: `items.json._props.CanSellOnRagfair === false` (649 itens em 4.0.13). Ativo enquanto `configs/ragfair.json:dynamic.blacklist.enableBsgList = true` (default).
2. **Custom server-side**: `configs/ragfair.json:dynamic.blacklist.custom` = array de Tpls.

Não implementado (limitação documentada):

- `enableQuestList: true` baniria itens de quests ativas — exigiria cruzar com `quests.json` + estado do perfil.
- `traderItems: false` baniria itens só-vendidos-por-trader.

## Categorias — `tarkov.dev` ≠ `items._parent` ≠ `handbook.Categories`

Existem **3 taxonomias diferentes** no Tarkov, com IDs diferentes:

| Taxonomia | Onde vive | Para que serve |
|---|---|---|
| **tarkov.dev category** | GraphQL `items.category { id name parent { id } }` | UI tipo handbook (Weapon > Assault rifle > ...). É o que usamos no viewer. |
| **`items.json._parent`** | Item template | Hierarquia de **classe BSG** (ex: "AssaultRifle base class" como parent de qualquer rifle). Usada pelo engine pra herança de props. **Não usar para UI.** |
| **handbook.json `Items[].ParentId`** | Handbook | Outra hierarquia, com IDs `5b47574386f774...`. Usada pela UI in-game do handbook. Próxima da tarkov.dev mas com IDs distintos. |

`normalize.js` prefere tarkov.dev (mais rico). Items só-no-SPT (mods) usam fallback de handbook (nome via locale `en.json`, key = id da categoria).

## Locale-resolved names

`<SPT_DATA>/database/locales/global/en.json` — formato `{ "<tpl> Name": "Display Name", "<tpl> ShortName": "...", "<tpl> Description": "..." }` (nota: chave tem **espaço** entre tpl e atributo). Também tem nomes de categorias: `locale["<categoryId>"] = "Display Name"`. `_props.Name` no items.json é só a **chave i18n** ("weapon_izhmash_akm_762x39"), não o display name.

## Resumo do fluxo de dados

```
tarkov.dev GraphQL ──┐
                     ├─ fetch ──► cache/*-raw.json ──┐
tarkov-market REST ──┘                                │
                                                      ├─ normalize.js ──► data/items.json
SPT (D:/SPT/SPT) ────► load-spt.js ──► spt-raw.json ─┘
  ├─ items.json (template)
  ├─ prices.json (flea)
  ├─ handbook.json (base + cats)
  ├─ traders/*/assort.json
  ├─ locales/global/en.json
  └─ configs/ragfair.json (blacklist)

Viewer ──► POST /api/price ──► prices.json (escrita)
                          └──► checks.dat (hash refresh)
                          └──► data/items.json (sync)
                          └──► logs/price-edits.jsonl (audit)
```

Source of truth para flea: `prices.json` no disco. Cache do mod (`mod_config/prices-pve.json`) é fonte secundária — útil quando você ainda usa LiveFleaPrices, irrelevante após calibração autoral.
