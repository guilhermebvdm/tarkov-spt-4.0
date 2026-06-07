# Plano — Migração do viewer para `itemPriceOverrideRouble` (override-only)

> **Este documento é a fonte de verdade do trabalho de correção do flea price.**
> Originalmente vivia em `~/.claude/plans/curried-dreaming-marble.md` (fora do repo).
> Copiado para cá para permitir continuar o trabalho em outra máquina.

## TL;DR — onde paramos (2026-05-19)

A fórmula real do flea do SPT 4.0 foi descoberta (análise do código-fonte + 12 testes empíricos). A decisão arquitetural é: **o viewer passa a escrever apenas em `ragfair.json:dynamic.itemPriceOverrideRouble[tpl]`**, em vez de `handbook.json`.

### Status de execução — ✅ CONCLUÍDO (2026-06-07, commits `140c016` + `bbce32c`)

| Ação | Estado | Nota |
| --- | --- | --- |
| **0 — Smoke test do override** | ✅ Rodado | Override **funciona**, mas a premissa "sobrescreve" foi **falsificada** — é **aditivo** (override + bonus), com **piso** e **teto**. Ver fórmula corrigida abaixo. |
| **1 — `load-spt.js`** | ✅ Feita | Computa `fleaMultiplier` (M real, incl. overrides tpl/tipo), `fleaPrice` (bonus = handbook×M), `fleaFloor` (handbook×K_trader), `fleaCeiling` (handbook×unreasonableMult), `fleaOverride`, `effectiveFleaPrice` (clamp). |
| **2 — `normalize.js` + `items.json`** | ✅ Feita | Propaga os 7 campos; `consolidated.priceFleaSpt = effectiveFleaPrice`. |
| **3 — `serve.js` `/api/price`** | ✅ Feita | `POST` grava `override = X − bonus` em `ragfair.json` (rejeita X<floor e X>ceiling), `DELETE` remove, `GET /api/overrides`, mutex, escrita atômica TAB, refresh checks.dat. |
| **4 — `index.html` UI** | ✅ Feita | Badge **OVR**, botão **Restaurar default** (↺), hint mín/máx, toast com transição. |
| **5 — Docs** | ✅ Feita | Este arquivo + `flea-formula-validation.md` + `flea-override-validation.md` + `README.md` + `spt-internals.md`. |
| **6 — Memory** | ✅ Feita | `project_flea_price_formula.md` reescrita com a fórmula aditivo+piso+teto. |

**Lição (atualiza a regra antiga):** a Ação 0 era bloqueante e cumpriu o papel — **falsificou** a premissa central do plano (override "sobrescreve"). A análise de código inicial estava certa sobre os passos, mas errada sobre a **ordem** (override entra ANTES do `AddOrUpdate +=`, não depois) e omitia o piso de trader e o teto de `unreasonableModPrices`. Ver §"A fórmula real" abaixo, reescrita.

### Estado do SPT install

`D:/SPT/SPT/SPT_Data/` em **vanilla** (5 overrides default no `ragfair.json`, handbook/prices intactos). Os edits de teste foram revertidos. O viewer (`serve.js`) escreve **apenas** em `ragfair.json:itemPriceOverrideRouble` + refresca `checks.dat`.

---

## A fórmula real do flea (SPT 4.0)

Validada contra `references/spt-source/Libraries/SPTarkov.Server.Core/` e 12 cenários em [flea-formula-validation.md](flea-formula-validation.md).

### Variáveis

| Símbolo | Significado |
| --- | --- |
| `P_disco(tpl)` | `prices.json[tpl]` no disco antes do boot (ou ausente se a key não existe) |
| `H(tpl)` | `handbook.Items[tpl].Price` |
| `m` | `priceMultiplier` do `ragfair.json` (default 1.5) |
| `c(tpl)` | `0.8` se `tpl` aparece como ingrediente em ≥1 receita do hideout, senão `0` |
| `T(tpl)` | `highestSellToTraderPrice(tpl)` — só se `PreventPriceBeingBelowTraderBuyPrice=true` |
| `O(tpl)` | `ragfair.json:dynamic.itemPriceOverrideRouble[tpl]` (ou ausente) |

### Boot do server — ordem REAL (validada por código + 7 cenários in-game)

> ⚠️ Corrigido: o override **NÃO sobrescreve**. Ele entra no dicionário de prices **antes** do bonus, e o bonus é **somado** por cima.

```text
Passo A — PostDbLoadService.ApplyFleaPriceOverrides  (roda PRIMEIRO):
  se O(tpl) existe: Templates.Prices[tpl] = O(tpl)     ← assignment (substitui prices.json)

Passo B — RagfairPriceService.ReplaceFleaBasePrices  (roda DEPOIS):
  bonus(tpl) = H(tpl) × (M(tpl) + c(tpl))               M = tplOverride|tipoOverride|1.5 ; c = 0.8 se craft
  se PreventPriceBeingBelowTraderBuyPrice e T(tpl) > bonus: bonus = T(tpl)
  Templates.Prices.AddOrUpdate(tpl, bonus)              ← += (key já existe via Passo A ou prices.json)

⇒ base_mem(tpl) = (O(tpl) ?? P_disco(tpl) ?? 0) + bonus(tpl)
```

**Gotcha central:** `AddOrUpdate` ([DictionaryExtensions.cs:12-19](../../../references/spt-source/Libraries/SPTarkov.Server.Core/Extensions/DictionaryExtensions.cs#L12-L19)) faz `+=` se a key existe. Como o Passo A já pôs o override na key, o Passo B **soma** o bonus.

### Geração de oferta (runtime) — piso e teto

```text
price = GetFleaPriceForItem(tpl) = base_mem(tpl)
se useTraderPriceForOffersIfHigher e traderSell > price: price = traderSell   ← PISO (= H × K_trader ≈ H)
se tpl ∈ unreasonableModPrices e price > H × overMult:   price = H × newMult   ← TETO (mods ×6, electronics ×11)
(adjustPriceWhenBelowHandbookPrice está OFF neste install)
price ×= ItemPriceMultiplier[tpl]   (mapa manual, 2 tpls)
price ×= qualityModifier            (se não em IgnoreQualityPriceVarianceBlacklist)
oferta = price × variância(0.8..1.2, bias 2,2; clamp RÍGIDO — re-rola fora do range)
```

### Fórmula consolidada (o que o viewer usa)

```text
offerBase = clamp( (override ?? prices.json ?? 0) + bonus ,  floor ,  ceiling )
  bonus   = H × M
  floor   = H × K_trader        (K_trader = max(100 − buy_price_coef[LL0])/100 ; =1.0 → floor ≈ handbook)
  ceiling = H × unreasonableMult (Weapon Mod ×6, Electronics ×11; senão ∞)
```

Para cravar o preço `X`: **`override = X − bonus`**, válido para `floor ≤ X ≤ ceiling`.

### Por que os testes 6/7 pareciam mostrar 2 padrões

Leitura empírica inicial sugeriu "prices=0 → k≈2.0" e "prices missing → k≈1.5". A causa real era se o item é **craft do hideout** (`c=0.8` → 2.3 vs 1.5). `prices=0` vs `missing` tem efeito idêntico.

---

## Decisão arquitetural

**Viewer escreve apenas em `ragfair.json:dynamic.itemPriceOverrideRouble[tpl]`** (override **compensado** = X − bonus). Não toca `handbook.json` nem `prices.json`.

### Por quê

- Mantém handbook/prices.json intactos; reversível deletando a key.
- A compensação (`X − bonus`) faz `base_mem = X` exato; piso/teto são respeitados pelo viewer (rejeita X fora de `[floor, ceiling]`).

### Tradeoffs / limites

- **Handbook in-game (menu do EFT) NÃO reflete o preço** — só o flea.
- **Teto:** Electronics/Weapon Mods não passam de `H × 11` / `H × 6` (cap do SPT, não contornável por override).
- **Piso:** nada desce abaixo de `H × K_trader` (≈ handbook).
- Se LiveFleaPrices for reativado, ele muta `prices.json` em memória no boot — mas o override (Passo A) ainda vira a base e o piso/teto continuam valendo.

---

## Ação 0 — Smoke test (PRÉ-REQUISITO BLOQUEANTE)

Validar manualmente que `itemPriceOverrideRouble` funciona em SPT 4.0.13. Fecha o risco da ordem de boot (`PostDbLoadService` vs `RagfairPriceService.Load`).

### Procedimento

```bash
node tools/tarkov-itemdb/scripts/action0-override-smoke-test.js prep
```

O script faz backup de `ragfair.json`, injeta 2 overrides (Bolts `57347c5b245977448d35f6e1` → `123456`; GPU `57347ca924597744596b4e71` → `654321`), refresca `checks.dat`.

Depois: restart full do SPT, abrir flea, filtrar cada item, registrar ofertas min/max.

### Critério

- **Sucesso**: ofertas em `override × 0.8..1.2` (Bolts ≈98K-148K, GPU ≈523K-785K). Prossegue para Ação 1+.
- **Falha A**: ofertas batem fórmula vanilla → override ignorado. Replanejar.
- **Falha B**: ofertas em `(override + bonus_vanilla) × variance` → ordem invertida. Replanejar.
- **Falha C**: inconsistente → investigar interferência.

Preencher resultado em [flea-override-validation.md](flea-override-validation.md). Reverter com `action0-override-smoke-test.js revert`.

---

## Ação 1 — Pipeline (`scripts/load-spt.js`) ✅ FEITA

Já implementado. Para referência do que foi feito:

- Lê `<SPT_DATA>/database/hideout/production.json`, monta Set de craft items (tpls em `recipes[].requirements[]` com `type==="Item"`).
- Gera `data/hideout-crafts.json` (audit trail).
- Por item do handbook calcula:
  - `fleaMultiplier = 1.5 + (0.8 se craft, senão 0)`
  - `fleaPrice = round(basePrice × fleaMultiplier)` (vanilla)
  - `fleaOverride = ragfair.json:dynamic.itemPriceOverrideRouble[tpl] ?? null`
  - `effectiveFleaPrice = fleaOverride ?? fleaPrice`

### Validação manual (cruzar com os 12 tpls)

| Tpl | Item | `isHideoutCraftItem` esperado |
| --- | --- | --- |
| `660bbc47c38b837877075e47` | Encrypted flash drive | true |
| `590c678286f77426c9660122` | IFAK | true |
| `544fb45d4bdc2dee738b4568` | Salewa | false |
| `5d80c62a86f7744036212b3f` | RB-VO marked key | false |
| `5c0530ee86f774697952d952` | LEDX | false |
| `5755356824597772cb798962` | AI-2 medkit | true |
| `5751a25924597722c463c472` | Army bandage | true |
| `57347ca924597744596b4e71` | Graphics card | false |
| `57347c5b245977448d35f6e1` | Bolts | true |
| `5c12620d86f7743f8b198b72` | Tetriz | false |
| `5c06779c86f77426e00dd782` | Bundle of wires | true |
| `5c12613b86f7743bbe2c3f76` | Intelligence folder | true |

⚠️ Esta tabela é induzida das predições da conversa, não do código. Se a coluna real do `hideout-crafts.json` divergir, replanejar (a hipótese de "qual é craft" estaria errada).

---

## Ação 2 — Schema (`scripts/normalize.js`) ❌ PENDENTE

No bloco de output `spt` do `normalize.js` (~linhas 344-351), propagar:

```js
const spt = s ? {
  basePrice:           s.basePrice ?? null,
  fleaPrice:           s.fleaPrice ?? null,           // vanilla (handbook × multiplier)
  fleaMultiplier:      s.fleaMultiplier ?? null,      // 1.5 ou 2.3
  isHideoutCraftItem:  s.isHideoutCraftItem ?? false,
  fleaOverride:        s.fleaOverride ?? null,        // valor em ragfair.json, ou null
  effectiveFleaPrice:  s.effectiveFleaPrice ?? null,  // fleaOverride ?? fleaPrice
  fleaBanned:          s.fleaBanned === true,
  fleaBanReasons:      s.fleaBanReasons || [],
  // ... resto
};
```

⚠️ **Verificar o `ITEM_KEYS` / `orderedObject`** no `normalize.js` — campos novos precisam estar na lista de chaves senão são descartados na serialização (bug que já aconteceu com `modSource`).

---

## Ação 3 — Viewer back-end (`viewer/serve.js`) ❌ PENDENTE

### 3.1 Reescrever `PATCH /api/price`

Parar de escrever `handbook.json`. Passar a escrever `ragfair.json`:

```js
const ragfairPath = path.join(SPT_DATA, 'configs/ragfair.json');
const ragfair = JSON.parse(fs.readFileSync(ragfairPath, 'utf8'));
ragfair.dynamic.itemPriceOverrideRouble = ragfair.dynamic.itemPriceOverrideRouble || {};
const previousOverride = ragfair.dynamic.itemPriceOverrideRouble[tpl] ?? null;
ragfair.dynamic.itemPriceOverrideRouble[tpl] = price;
fs.writeFileSync(ragfairPath, JSON.stringify(ragfair, null, 4), 'utf8');  // escrita atômica tmp+rename
updateSptChecks({ 'configs/ragfair.json': ragfairPath });
// sync data/items.json: items[tpl].spt.fleaOverride = price; effectiveFleaPrice = price;
```

### 3.2 Adicionar `DELETE /api/price`

Remove o override, restaura vanilla:

```js
delete ragfair.dynamic.itemPriceOverrideRouble[tpl];
fs.writeFileSync(...);  // atômica
updateSptChecks(...);
// sync items.json: fleaOverride = null; effectiveFleaPrice = fleaPrice (vanilla)
```

### 3.3 Adicionar `GET /api/overrides`

Retorna o mapa `{ tpl: price }` de `ragfair.json:dynamic.itemPriceOverrideRouble` — para a UI listar/bulk-delete.

### 3.4 Audit log

Renomear `logs/price-edits.jsonl` → `logs/override-edits.jsonl` (ou manter nome + campo `mode`). Registrar `{ timestamp, tpl, action: 'set'|'delete', previousOverride, newOverride, vanillaFleaPrice, isHideoutCraftItem }`.

### 3.5 `checks.dat` cobre `configs/ragfair.json`?

Confirmar lendo `checks.dat`. Se não cobrir, é só warning no boot (não fatal). Adicionar à lista vigiada no `updateSptChecks` se necessário.

### 3.6 Mutex de escrita

Lock simples em-process para evitar race em `PATCH` simultâneos (leem-modificam-escrevem o mesmo `ragfair.json`).

### NÃO modificar

- `handbook.json` — não toca mais
- `prices.json` — não toca mais
- `/api/ban` — **continua igual** (escreve `CanSellOnRagfair` em `database/templates/items.json`)
- `/api/flea-min-level` — continua igual (escreve `globals.json`)

---

## Ação 4 — Viewer UI (`viewer/index.html`) ❌ PENDENTE

1. **Display do preço**: mostrar `effectiveFleaPrice` como valor primário ("Flea (live)"). Se `fleaOverride != null` → badge "Override" + valor vanilla em cinza. Senão → badge "Vanilla". Badge informativo "Hideout craft (×2.3)" / "Standard (×1.5)".
2. **Dialog de edit**: campo "Preço desejado no flea"; tooltip explicando que grava em `ragfair.json:itemPriceOverrideRouble` e sobrescreve a fórmula vanilla, com variância ±20% ainda aplicável. Botão "Salvar" → `PATCH`. Botão "Restaurar default" (só se há override) → `DELETE` + confirmação.
3. **Filtro opcional**: checkbox "Mostrar só items com override".
4. **Compatibilidade**: tratar `fleaOverride === undefined` como `null` (items.json antigo).

---

## Ação 5 — Documentação 🟡 PARCIAL

- **5a `spt-internals.md`** ✅ — fórmula em 3 passos documentada, refs ao código-fonte.
- **5b `README.md`** ✅ — schema do `items.json` atualizado com os campos novos.
- **5c `flea-formula-validation.md`** ❌ — adicionar seção "Conclusão" com fórmula validada, tabela predição×observado dos 12 cenários, refutação das hipóteses A/B.
- **5d `flea-override-validation.md`** 🟡 — criado; preencher resultados após a Ação 0.

⚠️ **Nota**: o `spt-internals.md` atualizado referencia `mods/server-csharp/original/...` mas o código-fonte real foi vendorizado em `references/spt-source/`. Corrigir os paths das refs (ou mover/symlink) numa próxima passada.

---

## Ação 6 — Memory persistente ❌ PENDENTE

- **`memory/project_flea_price_formula.md`**: reescrever com a fórmula vanilla em 3 passos, decisão override-only, gotcha do `AddOrUpdate +=`, refs ao server-csharp.
- **`memory/feedback_spt_validation.md`**: ajustar o caso `blacklist.custom` ("comportamento mudou entre versões; 4.0.13 desserializa"); adicionar o caso da fórmula `×1.5` que tinha exceção escondida (craft items). Lição: matriz de testes com classes variadas > generalizar de 1-2 casos.

> Memory vive em `~/.claude/projects/.../memory/` — **não vai para o outro computador**. Após reescrever lá, considerar copiar o conteúdo relevante para um doc no repo (ex: este arquivo ou `spt-internals.md`).

---

## Ordem de execução

1. **Ação 0** — smoke test (BLOQUEANTE)
2. **Ação 2** — schema no normalize.js (Ação 1 já feita)
3. Validação: rodar `load-spt.js` + `normalize.js`, checar `isHideoutCraftItem` dos 12 tpls
4. **Ação 3** — viewer back-end
5. **Ação 4** — viewer UI
6. **Ação 5** — docs (paralelizável com 3+4)
7. **Ação 6** — memory

---

## Verificação end-to-end

### Pipeline

```bash
node tools/tarkov-itemdb/scripts/load-spt.js
node tools/tarkov-itemdb/scripts/normalize.js
```

`data/items.json`: campos novos presentes; 12 tpls com `isHideoutCraftItem` correto; `data/hideout-crafts.json` coerente.

### Viewer back-end

```bash
node tools/tarkov-itemdb/viewer/serve.js
```

- `PATCH /api/price {tpl:"57347c5b245977448d35f6e1",price:100000}` → `ragfair.json` override = 100000, `checks.dat` atualizado, `items.json` viewer-side com `fleaOverride:100000`
- `DELETE /api/price` mesmo tpl → key removida, `fleaOverride:null`
- `GET /api/overrides` → mapa
- Audit log tem ambos os eventos

### In-game smoke test

Editar 3 items (craft handbook baixo, craft handbook alto, non-craft com prices.json não-zero), restart SPT (LiveFleaPrices OFF), confirmar `override × 0.8..1.2`. Deletar overrides, restart, confirmar volta ao vanilla.

### Regressão

`/api/ban` e `/api/flea-min-level` continuam funcionando. `items.json` antigo (sem `fleaOverride`) ainda renderiza no viewer.

---

## Riscos / pontos abertos

- **Ordem de boot** (resolvido pela Ação 0) — `PostDbLoadService` vs `RagfairPriceService.Load`. Se Ação 0 falhar, replanejar tudo.
- **`checks.dat` cobre `ragfair.json`?** — confirmar; se não, só warning no boot.
- **LiveFleaPrices runtime** — muta `prices.json` em memória no boot. Se rodar DEPOIS de `ApplyFleaPriceOverrides`, sobrescreve overrides. Smoke test obrigatório com LiveFleaPrices ON antes de fechar como "compatível".
- **Schema antigo do `items.json`** — viewer deve tolerar campos ausentes (fallback graceful).
- **`PreventPriceBeingBelowTraderBuyPrice`** afeta a fórmula vanilla mas não o override (passo 3 atribui direto). Doc only.
- **Edits históricos no handbook** — se houver edits prévios via viewer no `handbook.json`, eles continuam afetando `bonus` mas somem do fluxo do viewer. Migração opcional (Ação 7, provavelmente descartada — estado já foi limpo).

---

## Arquivos relevantes

### Código a modificar

- `tools/tarkov-itemdb/scripts/load-spt.js` ✅ feito
- `tools/tarkov-itemdb/scripts/normalize.js` ❌ Ação 2
- `tools/tarkov-itemdb/viewer/serve.js` ❌ Ação 3
- `tools/tarkov-itemdb/viewer/index.html` ❌ Ação 4

### Docs

- `tools/tarkov-itemdb/docs/spt-internals.md` ✅
- `tools/tarkov-itemdb/docs/flea-formula-validation.md` 🟡 (falta conclusão)
- `tools/tarkov-itemdb/docs/flea-override-validation.md` 🟡 (falta resultado Ação 0)
- `tools/tarkov-itemdb/docs/flea-override-plan.md` — **este arquivo**
- `tools/tarkov-itemdb/README.md` ✅

### Estado de teste (versionado)

- `tools/tarkov-itemdb/.revert-state.json` — valores originais dos 12 tpls
- `tools/tarkov-itemdb/.test-validation-state.json` — backups detalhados por teste
- `tools/tarkov-itemdb/data/hideout-crafts.json` — Set de craft items gerado pela Ação 1
- `tools/tarkov-itemdb/scripts/action0-override-smoke-test.js` — script da Ação 0

### Referências (só leitura) — código-fonte SPT vendorizado

- `references/spt-source/Libraries/SPTarkov.Server.Core/Services/RagfairPriceService.cs` — passo 2 (`ReplaceFleaBasePrices`), quality modifier
- `references/spt-source/Libraries/SPTarkov.Server.Core/Extensions/DictionaryExtensions.cs` — `AddOrUpdate +=` (o gotcha)
- `references/spt-source/Libraries/SPTarkov.Server.Core/Services/PostDbLoadService.cs` — passo 3 (`ApplyFleaPriceOverrides`)
- `references/spt-source/Libraries/SPTarkov.Server.Assets/SPT_Data/database/hideout/production.json` — receitas do hideout
- `references/spt-source/Libraries/SPTarkov.Server.Assets/SPT_Data/configs/ragfair.json` — config default (tem `itemPriceOverrideRouble`)

### Estado do SPT install (máquina atual)

`D:/SPT/SPT/SPT_Data/` — revertido ao vanilla nos 12 tpls de teste. Backups em `*.test-backup`. Em outra máquina, o caminho do SPT muda — ajustar `SPT_PATH` env var.
