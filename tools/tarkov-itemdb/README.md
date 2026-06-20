# tarkov-itemdb

> **Para detalhes do SPT internals** (checks.dat, handbook/prices semântica, assort gotchas, categorias, flea blacklist) ver [docs/spt-internals.md](docs/spt-internals.md).

Base de dados unificada de itens do Tarkov, combinando 3 fontes:

- **SPT local** (`D:/SPT/SPT`) — template de item, preços base + flea (derivados do handbook), assort de traders, itens de mods
- **[tarkov.dev](https://tarkov.dev)** (GraphQL, sem chave) — preços flea PVE+regular, imagens, categorias, `sellFor`/`buyFor`
- **[tarkov-market.com](https://tarkov-market.com)** (REST, requer chave) — preços flea PVE alternativos

Saída: `data/items.json` normalizado por BSG Tpl, com bloco `consolidated` que serve como view direta para a tabela (Item | Group | Trader | Flea SPT | Flea Dev | Flea Market).

Inclui viewer com edição de preços ao vivo (override aditivo p/ itens vanilla, multiplicador p/ itens de mod) e ban/unban de itens no flea.

---

## Setup em máquina nova

1. **`SPT_PATH`** (env var) — raiz do SPT install (a pasta que contém `SPT_Data/`). Default: `D:/SPT/SPT`. O script aceita tanto a raiz quanto a subpasta `SPT_Data/`.
2. **`TARKOV_MARKET_API_KEY`** (env var) — chave de [tarkov-market.com](https://tarkov-market.com) (requer Patreon Tier 1+). Sem ela `fetch-tarkov-market.js` aborta com erro claro.
3. **Rodar pipeline**: `cd tools/tarkov-itemdb && node scripts/build.js` (~6s no primeiro run, rápido em re-runs com cache fresco).
4. **Abrir viewer**: `node viewer/serve.js [porta]` → `http://localhost:8080/TRLItemsManagement/`.

---

## Comandos

```bash
node scripts/build.js             # pipeline completo (usa cache se fresco)
node scripts/build.js --force     # refetch tudo + regenera data/
```

Etapas individuais (úteis para debug):

```bash
node scripts/fetch-tarkov-dev.js [--force]      # → cache/tarkov-dev-raw.json
node scripts/fetch-tarkov-market.js [--force]   # → cache/tarkov-market-raw.json
node scripts/load-spt.js                        # → cache/spt-raw.json
node scripts/normalize.js                       # → data/{items,categories,meta}.json
```

`load-spt` e `normalize` sempre rodam mesmo sem `--force` (são rápidos; seus inputs mudam com calibração manual).

---

## Variáveis de ambiente

| Variável | Default | Necessária para |
| --- | --- | --- |
| `SPT_PATH` | `D:/SPT/SPT` | `load-spt.js` — aceita raiz ou subpasta `SPT_Data` |
| `TARKOV_MARKET_API_KEY` | — | `fetch-tarkov-market.js` (obrigatória) |

---

## Estrutura de arquivos

```text
tools/tarkov-itemdb/
├── scripts/
│   ├── build.js                  orquestrador: chama os 4 em sequência
│   ├── fetch-tarkov-dev.js       GraphQL tarkov.dev → cache/
│   ├── fetch-tarkov-market.js    REST tarkov-market → cache/
│   ├── load-spt.js               lê D:/SPT → cache/spt-raw.json
│   └── normalize.js              merge das 3 fontes → data/
├── cache/                        gitignored; regeneráveis
│   ├── tarkov-dev-raw.json
│   ├── tarkov-market-raw.json
│   └── spt-raw.json
├── data/                         versionado; source of truth
│   ├── items.json                ~14 MB, 1 linha por Tpl, ordenado por Tpl
│   ├── categories.json           árvore única (tarkov.dev + handbook SPT)
│   ├── meta.json                 timestamps + estatísticas por fonte
│   ├── traders.json              metadados dos traders (nome, avatar URL)
│   └── handbook-prices-log.json  histórico de edições de preço via viewer
├── viewer/
│   ├── serve.js                  servidor HTTP + APIs de escrita (/api/price, /api/ban, /api/flea-min-level)
│   ├── index.html                tabela principal
│   └── components.css / tokens.css
├── docs/
│   └── spt-internals.md          internals do SPT relevantes ao pipeline
└── logs/
    ├── price-edits.jsonl         audit log de edições de preço (append-only)
    └── ban-edits.jsonl           audit log de ban/unban (append-only)
```

---

## Schema (`data/items.json`)

Objeto top-level `{ "<bsgTpl>": { ... } }` para lookup O(1). 1 linha por Tpl (diffs legíveis).

```jsonc
{
  "id": "5447a9cd4bdc2dbd208b4567",
  "name": "Colt M4A1 5.56x45 assault rifle",
  "shortName": "M4A1",
  "wikiLink": "https://...",
  "image": { "icon": "...", "grid": "...", "large": "..." },  // null se item só no SPT/mods
  "category": { "id", "name", "normalizedName", "path": ["Weapon", "Assault rifle"] },
  "types": ["gun", "wearable"],
  "dims": { "weight": 3.7, "width": 1, "height": 1 },
  "grids": null,                            // ou [{ name, cellsH, cellsV }] para containers
  "modSource": null,                        // nome do mod (ex: "WTT-PackNStrap") ou null se item base
  "spt": {
    "basePrice": 18397,                     // handbook.Items[].Price
    "fleaPrice": 27596,                     // bonus ADITIVO = basePrice × fleaMultiplier (o viewer subtrai isto)
    "fleaFloor": 18397,                     // piso = basePrice × K_trader (oferta não desce abaixo)
    "fleaCeiling": null,                    // teto = basePrice × mult (Weapon Mod ×6 / Electronics ×11), senão null
    "fleaMultiplier": 1.5,                  // M: 1.5/2.3 (craft) ou 1.8/2.5 (override tpl/tipo)
    "isHideoutCraftItem": false,            // true se é ingrediente em alguma receita
    "fleaOverride": null,                   // número em ragfair.json:itemPriceOverrideRouble[tpl], senão null
    "effectiveFleaPrice": 27596,            // = clamp((fleaOverride ?? prices.json ?? 0) + fleaPrice, fleaFloor, fleaCeiling)
    "fleaBanned": false,
    "fleaBanReasons": [],                   // [] | ["bsg"] | ["custom"] | ["bsg","custom"]
    "questRewards": [{ "questId", "name", "trader", "count" }],  // [] se não é reward
    "traders": [
      { "name": "Mechanic", "priceRUB": 18397, "currency": "RUB",
        "loyaltyLevel": 1, "unlimited": false, "stock": 3, "questLocked": false }
    ]
  },
  "tarkovDev": {
    "pve":     { "lastLow", "avg24h", "low24h", "high24h", "updated", "sellFor": [...], "buyFor": [...] },
    "regular": { "lastLow", "avg24h", "low24h", "high24h", "updated" }
  },
  "tarkovMarket": {
    "pve": { "avg24h", "avg7days", "price", "traderName", "traderPrice", "updated" }
  },
  "consolidated": {
    "group": "Assault rifle",
    "conditionType": "durability",
    "priceTraderSell": { "value": 10302, "vendor": "Mechanic" },
    "priceFleaSpt":          27596,         // = spt.effectiveFleaPrice (vanilla ou override)
    "priceFleaDevLastLow":   30000,
    "priceFleaDevAvg24h":    41000,
    "priceFleaMarketAvg24h": 40500,
    "priceFleaCanonical":    40500,
    "priceFleaSource":       "tarkov-market-avg24h"
  }
}
```

Campos nulos quando a fonte não cobre o item. **Sem blending entre fontes** — divergência visível é o ponto.

---

## Semântica de preço — crítico

> Validada vs código-fonte (`references/spt-source/`) + 7 cenários in-game. Detalhe completo do boot em [docs/flea-formula-validation.md](docs/flea-formula-validation.md) e [docs/flea-override-plan.md](docs/flea-override-plan.md).

A fórmula real do flea (oferta base, qualidade cheia):

```text
offerBase = clamp( (override ?? prices.json[tpl] ?? 0) + bonus ,  fleaFloor ,  fleaCeiling )
oferta    = offerBase × variância(0.8..1.2)
```

### Campos derivados

| Campo | Fórmula | Papel |
|---|---|---|
| `fleaPrice` | `round(basePrice × M)` | **bonus aditivo** — o que o viewer subtrai. `M` = 1.5/2.3 (craft) ou 1.8/2.5 (override de tpl/tipo no `ragfair.json:generateBaseFleaPrices`) |
| `fleaFloor` | `round(basePrice × K_trader)` | **piso** — oferta não desce abaixo (via `useTraderPriceForOffersIfHigher`). `K_trader = max(100 − buy_price_coef[LL0])/100` ≈ 1.0 → piso ≈ handbook |
| `fleaCeiling` | `round(basePrice × mult)` ou `null` | **teto** — `unreasonableModPrices` capa Weapon Mods (×6) e Electronics (×11); `null` no resto |
| `fleaOverride` | valor em `ragfair.json` ou `null` | override compensado escrito pelo viewer |
| `effectiveFleaPrice` | `clamp((fleaOverride ?? prices.json ?? 0) + fleaPrice, fleaFloor, fleaCeiling)` | preço que o flea usa de fato |

### O ponto crítico: o override é ADITIVO, não substitui

No boot, `ApplyFleaPriceOverrides` faz `Prices[tpl] = override` (assignment, **antes**), e `ReplaceFleaBasePrices` faz `Prices.AddOrUpdate(tpl, bonus)` = **`+=`** (**depois**). Resultado: `base = override + bonus`. Por isso o viewer grava o **override compensado**:

**`override = preçoDesejado − fleaPrice(bonus)`**  →  `base = (X − bonus) + bonus = X`.

### Edição de preço via viewer

O viewer exibe `effectiveFleaPrice` na coluna "Flea SPT" (badge **OVR** se há override). Ao editar para o preço `X`:

1. Valida `fleaFloor ≤ X ≤ fleaCeiling` (senão `422` com o limite).
2. Escreve `override = X − bonus` em `ragfair.json:dynamic.itemPriceOverrideRouble[tpl]`.
3. Recalcula MD5 → `checks.dat`.
4. Sync `data/items.json`: `fleaOverride`, `effectiveFleaPrice = X`.
5. No próximo boot do SPT: `base = override + bonus = X`; ofertas em `X × 0.8..1.2`.

**Tradeoff/limites:** handbook in-game não muda (só o flea); Electronics/Weapon Mods têm teto (`X ≤ handbook × 11/6`); nada desce abaixo do piso. Botão **Restaurar default** (↺) deleta a key → volta ao vanilla.

### `consolidated.priceTraderSell` vs `spt.traders[]`

- **`spt.traders[]`**: o que o player **paga** ao comprar do trader (buy-from-trader). Vem de `assort.json`.
- **`consolidated.priceTraderSell`**: o que o trader **paga** ao comprar do player (sell-to-trader). Vem de `tarkovDev.pve.sellFor`.

### Regra de prioridade do `priceFleaCanonical`

1. `tarkov-market avg24h` — fonte independente, métrica estável
2. `tarkov.dev avg24h` — fallback
3. `tarkov.dev lastLow` — fallback se não há avg24h
4. `spt.fleaPrice` — último recurso (circular durante calibração, mas útil para itens raros)

---

## Variância de condição (`conditionType`)

APIs públicas agregam **todas as condições** — chave 1/10 e 10/10 no mesmo avg24h.

| `conditionType` | Detectado por | Exemplos |
| --- | --- | --- |
| `"uses"`       | `_props.MaximumNumberOfUsage > 0` | Chaves, keycards |
| `"durability"` | `_props.MaxDurability > 0` | Armas, armaduras, capacetes |
| `"resource"`   | `_props.MaxHpResource > 0` ou `MaxResource > 0` | Meds, food |
| `"none"`       | nenhum dos acima | Munição, currency, attachments |
| `"unknown"`    | item só de mod sem `_props` base | — |

Comparação de preços é segura apenas entre `conditionType === "none"`.

---

## Itens de mods (`modSource != null`)

`load-spt.js` escaneia `<SPT_PATH>/user/mods/*/db/CustomItems/*.json(c)` após o passo de flea blacklist (passo 4c). Todos os mods que adicionam itens usam esse padrão.

**Mods atualmente instalados com itens customizados:**

| Mod | Itens | Observação |
| --- | --- | --- |
| `WTT-PackNStrap` | 44 | Belts e containers de cintura (slot ArmBand) |
| `TEP300Backport` | 1 | Headset Peltor TEP-300 |
| `LoadAmmoAnimServer` | 1 | Item interno de animação (QuestItem, sem flea) |

**Schema do arquivo de mod** (`db/CustomItems/*.json`):

- Chave = BSG Tpl ID
- `overrideProperties` → props do item (Width, Height, Weight, Grids, CanSellOnRagfair…)
- `locales.en.{name,shortName}` → nome exibido (inline, não usa en.json do SPT)
- `handbookPriceRoubles` → `spt.basePrice`
- `fleaPriceRoubles` → `spt.fleaPrice` (direto, não usa multiplicador handbook)
- `traders.TRADERNAME.assortId.{barterSettings,barters}` → `spt.traders[]`
  - `"MONEY_ROUBLES"` no `_tpl` de barters = moeda RUB
- `parentId` → `parentClassId`; `handbookParentId` → `handbookCategoryId`

Mod items têm campo `modSource` com o nome da pasta do mod.

---

## Viewer — funcionalidades

```bash
node viewer/serve.js [port]   # default 8080
# Abrir: http://localhost:8080/TRLItemsManagement/
```

| Feature | Como funciona |
|---|---|
| Árvore de categorias (sidebar) | Click filtra por categoria; "Todos os itens" reseta |
| Busca | Filtra por `name` ou `shortName` com debounce 300ms |
| Dropdowns (Group, Condition, Ban, **Override**, **Mod**) | Multi-select; estado persistido no `localStorage`. **Override** filtra itens com/sem override de flea; **Mod** filtra itens customizados por `modSource` |
| **Flea Level widget (topbar)** | Botão "Flea Lvl N+" no topbar; click abre editor inline; salva em `globals.json` via `/api/flea-min-level`; **não** está embutido no `<th>` da tabela |
| Indicadores ▲▼ % | Comparam Trader / Flea Dev / Flea Market vs Flea SPT (referência de calibração) |
| **Atualizar preço (por item)** | Click na célula **Flea tarkov.dev** ou **Flea tarkov-market** → re-busca aquele item naquela fonte (spinner na célula + toast "era X → Y"). Via `/api/refresh-dev` / `/api/refresh-market` |
| **Atualizar todos (topbar)** | Botões **↻ dev** / **↻ market** → modal de confirmação → re-baixa o dump inteiro da fonte e re-mescla (`/api/refresh-all`). Não é item-a-item (tarkov-market = 5 req/min). UI fica busy (botões off + tabela dimmed), toast início/fim, recarrega ao terminar |
| **Edição de preço** | Click na célula Flea SPT → menu (Edit price / **Restaurar default** se há override / Ban); Edit abre input inline (mín/máx = piso/teto), grava override compensado em `ragfair.json`; badge **OVR** marca itens com override; **Restaurar default** (menu ou ↺ no form) remove o override → vanilla |
| **Ban/Unban** | Click na célula → menu (Edit / Ban / Unban); confirmação com botão "×" para cancelar |
| **Reward popover** | Badge de reward: hover por 300ms abre tooltip com lista de quests; mouse leave fecha (grace 80ms para mover ao popover) |
| Ordenação | Click no `<th>` ordena; segundo click inverte |
| Toast | Sucesso (verde 3s) / Erro (vermelho 5s) no canto superior direito |

### APIs do servidor

| Endpoint | Método | O que faz |
| --- | --- | --- |
| `GET /data/:file` | — | Serve arquivos de `data/` |
| `GET /spt-images/*` | — | Proxy de imagens do SPT (avatars, etc.) |
| `POST /api/price` | JSON `{ tpl, price }` | Grava `override = price − bonus` em `ragfair.json:itemPriceOverrideRouble` (rejeita `price` fora de `[fleaFloor, fleaCeiling]` com `422`), atualiza `data/items.json`, refresha `checks.dat`, grava em `logs/price-edits.jsonl`. Mutex + escrita atômica. |
| `DELETE /api/price` | JSON `{ tpl }` | Remove o override (restaura vanilla `prices.json + bonus`, com clamp), atualiza `data/items.json`, refresha `checks.dat` |
| `GET /api/overrides` | — | Mapa `itemPriceOverrideRouble` atual do `ragfair.json` |
| `POST /api/ban` | JSON `{ tpl, banned }` | Togla `CanSellOnRagfair` em `items.json` do SPT, atualiza `data/items.json`, refresha hash em `checks.dat`, grava em `logs/ban-edits.jsonl` |
| `POST /api/flea-min-level` | JSON `{ minUserLevel }` | Edita `globals.json:config.RagFair.minUserLevel`, refresha hash |
| `POST /api/refresh-dev` | JSON `{ tpl }` | Re-busca 1 item no tarkov.dev (GraphQL pve+regular), atualiza `tarkovDev` + `consolidated`, grava `logs/price-history.jsonl` |
| `POST /api/refresh-market` | JSON `{ tpl }` | Re-busca 1 item no tarkov-market (`/pve/item?q=<name>` filtrado por bsgId; requer `TARKOV_MARKET_API_KEY`), atualiza `tarkovMarket` + `consolidated` |
| `POST /api/refresh-all` | JSON `{ source: 'dev'\|'market' }` | Bulk: roda `fetch(--force) → load-spt → normalize` (child processes, sob mutex) e reconstrói `data/items.json`. Retorna `{ itemCount, durationMs }`. O cliente recarrega ao terminar |

### Logs de auditoria (`logs/`, gitignored)

Append-only JSONL, um evento por linha:

- `logs/price-edits.jsonl` — override set/delete (`action`, `tpl`, `desiredFlea`, `bonus`, `floor`, `override`, `previousOverride`).
- `logs/ban-edits.jsonl` — ban/unban (`action`, `tpl`, `method: CanSellOnRagfair`).
- `logs/price-history.jsonl` — refreshes de preço (dev/market) com `previous`/`current`, fácil de plotar depois.

> O antigo `data/handbook-prices-log.json` foi descontinuado quando o editor migrou de back-calc no handbook para override em `ragfair.json`.

---

## Limitações conhecidas

- **Preset weapons**: traders vendem M4A1 já modificada — `priceRUB` reflere o kit inteiro, não a arma "nua".
- **Barters complexos**: pegamos só `barter_scheme[id][0][0]`. Offers "item + dinheiro" subestimam preço.
- **`barter_scheme` de mods** com múltiplos requisitos: pulados (`continue`).
- **Imagens de itens de mods**: `image: null` (assets são bundles Unity, não URLs de CDN).
- **Câmbio USD/EUR → RUB**: lido do `handbook.json` do SPT (USD=120, EUR=133 em SPT 4.0.13). Re-rodar `build.js` atualiza.
- **`conditionType: "unknown"`**: itens de mods sem `_props` base no SPT.
- **Quest-locked de mods**: sempre `false` (mods definem isso em runtime, fora do `questassort.json`).
- **Filtro "só 10/10"**: exigiria scrape de listings individuais. `conditionType` é a única mitigação.

---

## Escrita em arquivos do SPT — QA obrigatório

Toda mudança no fluxo de escrita (`/api/price`, `/api/ban`, `load-spt.js`) precisa de spot-check **no jogo**, não só por write+hash. SPT silenciosamente ignora campos JSON fora do schema esperado — write bem-sucedido + hash correto em `checks.dat` não garante efeito real.

**Checklist mínimo após mexer em ban/price:**

1. Reload do server SPT.
2. Boot limpo (sem "validação de arquivo falhou").
3. **Ban**: tentar listar o item no flea → deve aparecer "Item is prohibited".
4. **Unban**: confirmar que o item volta ao flea.
5. **Edição de preço**: confirmar que o preço novo aparece no flea (delay de 1-2 min).

A lição foi vivida: ban via `dynamic.blacklist.custom` "funcionou" por dias até a descoberta in-game de que não tinha efeito real (SPT 4.0 dropou esse campo). Ver [memory/feedback_spt_validation.md](../../memory/feedback_spt_validation.md).

---

## Manutenção

### Re-habilitar LiveFleaPrices (se desativado)

```bash
mv "<SPT>/user/mods/DrakiaXYZ-LiveFleaPrices.disabled" "<SPT>/user/mods/DrakiaXYZ-LiveFleaPrices"
```

**Atenção**: re-habilitar vai **sobreescrever `prices.json` em memória** no próximo boot, mas o pipeline agora **ignora `prices.json`** — a calibração via `handbook.json` permanece intacta. O LiveFleaPrices também desabilita `useHandbookPrice` em memória, o que pode subverter os preços do flea. Mantenha desativado se a calibração importa.

### Novos mods com itens

Se instalar um novo mod, rodar `node scripts/load-spt.js` é suficiente (sem refetch das APIs). O scan de `user/mods/*/db/CustomItems/` é automático.

### Re-pipeline completo

```bash
node scripts/build.js --force
```

Cache TTL = 6h. Só os fetches honram TTL; `load-spt` e `normalize` sempre rodam.

---

## Troubleshooting

| Sintoma | Causa | Solução |
| --- | --- | --- |
| Boot SPT: "validação de arquivo falhou para handbook.json" | `serve.js` não rodou desde a última edição | Subir `serve.js` uma vez — ele refresha os hashes na inicialização |
| Viewer: "Falha ao carregar JSONs" | `fetch()` não funciona em `file://` | Usar `node viewer/serve.js`, não abrir HTML direto |
| `/api/price` retorna 500 | Erro no `serve.js` | Ver `.serve.log` ou rodar em foreground |
| Mod items não aparecem | Pasta do mod não tem `db/CustomItems/` | Verificar se o mod segue o padrão WTT/SPT 4.0 |
| `spt.fleaPrice` zerado em itens de mod | `fleaPriceRoubles: 0` no JSON do mod | Normal para itens internos (ex: LoadAmmoAnimServer) |
| Preço editado não muda o flea in-game | SPT server não reiniciado | Reiniciar o SPT server após editar |
