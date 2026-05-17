# tarkov-itemdb

> **Para detalhes de como o SPT armazena/valida esses dados** (formato do `checks.dat`, comportamento do mod LiveFleaPrices, gotchas do trader assort, taxonomia de categorias), ver [docs/spt-internals.md](docs/spt-internals.md).

Base de dados unificada de itens do Tarkov, combinando 3 fontes:

- **SPT local** (instalação em `D:/SPT/SPT`) — preços base, flea atual, ofertas de trader
- **[tarkov.dev](https://tarkov.dev)** (GraphQL, sem chave) — preços flea PVE+regular, imagens, categorias, `sellFor`/`buyFor`
- **[tarkov-market.com](https://tarkov-market.com)** (REST, requer chave) — preços flea PVE alternativos

Saída: `data/items.json` normalizado por BSG Tpl, com bloco `consolidated` que serve como view direta para a tabela final (Item | Group | Trader | Flea SPT | Flea Dev | Flea Market).

## Setup em máquina nova

1. **Configurar `SPT_PATH`** (env var) apontando para a raiz do SPT install (a pasta que contém `SPT_Data/`). Exemplo Windows: `setx SPT_PATH "D:\SPT\SPT"`. Default: `D:/SPT/SPT`. O script aceita tanto a raiz quanto a subpasta `SPT_Data/`.
2. **Configurar `TARKOV_MARKET_API_KEY`** (env var) — obter chave em [tarkov-market.com](https://tarkov-market.com) (precisa de Patreon Tier 1+). Sem ela o `fetch-tarkov-market.js` aborta com erro claro.
3. **Verificar layout do SPT**: deve haver **apenas uma** pasta `user/mods/`. Em alguns installs aparece duplicidade (`<SPT>/user/` paralela à `<SPT>/SPT/user/`) — geralmente a externa é resíduo, pode deletar após confirmar com os outros.
4. **Rodar pipeline**: `node scripts/build.js` (~6s no primeiro run, mais rápido em re-runs com cache fresco).
5. **Abrir viewer**: `node viewer/serve.js [porta]` → `http://localhost:8080/viewer/`.

## Comandos

```bash
node scripts/build.js            # pipeline completo (usa cache se fresco)
node scripts/build.js --force    # ignora cache e re-baixa tudo
```

Etapas individuais:

```bash
node scripts/fetch-tarkov-dev.js [--force]      # → cache/tarkov-dev-raw.json
node scripts/fetch-tarkov-market.js [--force]   # → cache/tarkov-market-raw.json
node scripts/load-spt.js                        # → cache/spt-raw.json
node scripts/normalize.js                       # → data/{items,categories,meta}.json
```

## Variáveis de ambiente

| Variável | Default | Necessária para |
|---|---|---|
| `SPT_PATH` | `D:/SPT/SPT` | `load-spt.js` (aceita tanto raiz quanto subpasta `SPT_Data`) |
| `TARKOV_MARKET_API_KEY` | — | `fetch-tarkov-market.js` (obrigatória) |

## Estrutura

```text
tools/tarkov-itemdb/
├── scripts/      # fetch + load + normalize + build
├── cache/        # gitignored; outputs intermediários, regeneráveis
├── data/         # versionado; source of truth
│   ├── items.json       1 linha por Tpl, ordenado, ~14 MB
│   ├── categories.json  árvore única (dev backbone + handbook SPT anotado)
│   └── meta.json        timestamps + estatísticas por fonte
└── README.md
```

## Schema (`data/items.json`)

Objeto top-level chaveado por BSG Tpl. Cada item:

```jsonc
{
  "id": "<bsgTpl>",
  "name": "...",
  "shortName": "...",
  "wikiLink": "...",
  "image": { "icon": "...", "grid": "...", "large": "..." },  // null se item só no SPT
  "category": { "id", "name", "normalizedName", "path": [...] },
  "types": ["gun", "wearable", ...],
  "dims": { "weight", "width", "height" },
  "spt": {
    "basePrice": <RUB>,
    "fleaPrice": <RUB>,                                       // null se não está em prices.json
    "traders": [ { "name", "priceRUB", "currency", "loyaltyLevel", "unlimited", "stock", "questLocked" } ]
  },
  "tarkovDev": {
    "pve":     { "lastLow", "avg24h", "low24h", "high24h", "updated", "sellFor": [...], "buyFor": [...] },
    "regular": { ... }
  },
  "tarkovMarket": {
    "pve": { "avg24h", "avg7days", "price", "traderName", "traderPrice", "updated" }
  },
  "consolidated": {
    "group": "<category.name>",
    "conditionType": "uses" | "durability" | "resource" | "none" | "unknown",
    "priceTraderSell": { "value": <RUB>, "vendor": "..." },   // max sellFor entre vendors não-flea
    "priceFleaSpt":          <RUB>,
    "priceFleaDevLastLow":   <RUB>,
    "priceFleaDevAvg24h":    <RUB>,
    "priceFleaMarketAvg24h": <RUB>,
    "priceFleaCanonical":    <RUB>,                            // priority chain (ver abaixo)
    "priceFleaSource":       "tarkov-market-avg24h" | "tarkov.dev-avg24h" | "tarkov.dev-lastLow" | "spt"
  }
}
```

Campos nulos quando a fonte não cobre o item. **Sem blending entre fontes** — divergência visível é o ponto.

### Regra de prioridade do `priceFleaCanonical`

1. `tarkov-market avg24h` — fonte independente, métrica estável
2. `tarkov.dev avg24h` — fallback se item não está no tarkov-market
3. `tarkov.dev lastLow` — fallback se nem `avg24h` existe
4. `spt.fleaPrice` — último recurso

## Semânticas de preço — importante

- **`prices.json` (SPT)**: hoje é mantido pelo mod `DrakiaXYZ-LiveFleaPrices`. **Validação empírica deste pipeline mostrou divergência significativa de `dev.lastLow`** (M4A1: SPT=132k vs dev-lastLow=30k) — o mod aparenta não estar escrevendo `lastLow` puro (talvez aplica `priceMultiplier`, ou está stale, ou usa outra métrica). Tratar `spt.fleaPrice` como **valor canônico do SPT, semântica opaca**, não comparar 1:1 com `dev.lastLow`. Quando você desativar o mod e calibrar manualmente, vira valor autoral.
- **`avg24h`**: média ponderada das últimas 24h. Sempre maior que `lastLow` em itens líquidos.
- **`lastLow`**: menor listing ativo. Volátil.
- **`spt.traders[]`**: o que o player **paga** ao comprar do trader (buy-from-trader).
- **`consolidated.priceTraderSell`**: o que o trader **paga** ao comprar do player (sell-to-trader). Vem de `tarkov.dev sellFor`.

## Variância de condição (`conditionType`)

Chaves 1/10 vs 10/10, armaduras danificadas, meds parciais — as APIs públicas agregam **todas as condições**, então preços de itens com condição variável são "ruidosos".

| `conditionType` | Detectado por | Exemplos |
|---|---|---|
| `"uses"`       | `_props.MaximumNumberOfUsage > 0` | Chaves, keycards |
| `"durability"` | `_props.MaxDurability > 0` | Armas, armaduras, capacetes |
| `"resource"`   | `_props.MaxHpResource > 0` OU `_props.MaxResource > 0` | Meds, food, bag of bolts |
| `"none"`       | nenhum dos acima | Munição, currency, info, attachments |

Comparação justa só é segura entre itens `conditionType === "none"`. Filtro real "só 10/10" exigiria scrape de listings individuais (fora de escopo).

## Limitações conhecidas

- **Preset weapons**: ofertas de traders com presets (M4A1 já modificada) têm `priceRUB` refletindo o kit inteiro, não a arma "nua".
- **Barters**: ofertas que pedem item por item (`currency: "BARTER"`) ficam em `spt.traders[]` como informativas, mas `priceRUB` = `null`.
- **`barter_scheme` composto**: pegamos só o primeiro requisito (`[0][0]`). Offers que pedem "item + dinheiro" subestimam preço.
- **Items só no SPT** (mods custom): `image: null`, `category` com fallback fraco (id do handbook como name).
- **Câmbio USD/EUR → RUB**: lido do handbook do SPT (USD=120, EUR=133 atualmente). Não há refresh dinâmico — re-rode o build.
- **`prices.json` semantics presumida**: enquanto LiveFleaPrices estiver ativo, é `lastLow`. Validação empírica no log do normalize (top divergências SPT × dev-lastLow).

## Manutenção

`probe-schema.js` é one-shot — schema confirmado, pode ser deletado.

Cache TTL = 6h. Para forçar refetch após mudança no SPT (preços calibrados manualmente, novos mods):

```bash
node scripts/build.js --force
```

`load-spt` e `normalize` sempre rodam (são baratos); só os fetches honram TTL.

## Viewer & edição de preços

```bash
node viewer/serve.js [port]   # default 8080
```

Abrir `http://localhost:<port>/viewer/`. Features:

- Árvore de categorias (estilo flea-market), busca, filtros (conditionType, has flea, has trader, banidos)
- Indicadores ▲▼ % comparando Trader / Flea Dev / Flea Market vs Flea SPT (referência durante calibração)
- Click numa célula da coluna **Flea SPT** abre editor inline (Enter salva, Esc cancela)
- Edição dispara `POST /api/price` que:
  1. Escreve em `<SPT>/SPT_Data/database/templates/prices.json`
  2. Sincroniza `data/items.json` (mesmo schema, mesma serialização "1 linha por Tpl")
  3. Atualiza o hash MD5 do arquivo em `<SPT>/SPT_Data/checks.dat` para a validação de boot do SPT passar
  4. Anexa entrada em `logs/price-edits.jsonl` (audit log JSONL, append-only)
- Toast no canto superior direito confirma sucesso (verde 3s) ou erro (vermelho 5s)

**Backup**: na primeira execução o `serve.js` faz um diff dos hashes ao startar e loga `checks.dat refresh: [...]`. O backup `checks.dat.bak` é criado manualmente quando você reinstala/troca de máquina.

## Re-habilitar o mod LiveFleaPrices

Desativamos renomeando a DLL + pasta (sem deletar). Pra reativar:

```bash
mv "<SPT>/user/mods/DrakiaXYZ-LiveFleaPrices.disabled" "<SPT>/user/mods/DrakiaXYZ-LiveFleaPrices"
mv "<SPT>/user/mods/DrakiaXYZ-LiveFleaPrices/DrakiaXYZ-LiveFleaPrices.dll.disabled" "<SPT>/user/mods/DrakiaXYZ-LiveFleaPrices/DrakiaXYZ-LiveFleaPrices.dll"
```

**Atenção**: re-habilitar **vai sobrescrever** os preços que você editou via viewer no próximo boot do server (mod aplica o snapshot do repo do Drakia em memória). Mantenha desativado se a calibração autoral importa.

## Troubleshooting

- **Boot do SPT loga "validação de arquivo falhou para ./SPT_Data/database/templates/prices.json"**: hash do arquivo divergiu do `checks.dat`. Subir o `serve.js` uma vez resolve — ele rerresca o hash na inicialização. Inevitável depois de cada edição via viewer; o próprio `serve.js` reconcilia automaticamente.
- **`load-spt.js` lê 2k itens em vez de 3k+**: você está com o `prices.json` original (não substituído pelo snapshot PVE). Ver [docs/spt-internals.md](docs/spt-internals.md) seção "Como o `prices.json` foi calibrado pela primeira vez".
- **Viewer mostra "Falha ao carregar JSONs"**: `fetch()` não funciona em `file://`. Use `node viewer/serve.js`, não abra o HTML direto.
- **Endpoint `/api/price` retorna 500**: cheque `.serve.log` (criado quando inicia via PowerShell hidden) ou rode `node viewer/serve.js` em foreground.
- **`prices.json` voltou ao snapshot antigo**: SPT só persiste in-memory no shutdown limpo. Se rodou o server com LiveFleaPrices habilitado, o snapshot pode ter sido reescrito. Workflow seguro: editar via viewer com SPT desligado.
