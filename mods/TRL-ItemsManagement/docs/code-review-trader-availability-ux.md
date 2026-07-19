# Code review — trader availability UX overhaul + disable-sale + conflict flag

> **Data:** 2026-07-19<br>
> **Status:** 🟢 Vivo<br>
> **Responsáveis:** Guilherme<br>
> **Referências:** [code-review-b5-b6-flea-floor-stock.md](./code-review-b5-b6-flea-floor-stock.md)<br>

---

**Escopo:** revisão das alterações dos commits `00cffab9` (reforma de UX do painel de trader + disable-sale
+ flag de inconsistência flea/trader + reorder do detalhe) e `d64e174a` (quebra de linha nos tooltips).
Metodologia do `/code-review` (6 categorias × 4 impactos); checklists `spt-mod-best-practices` +
`csharp-mod-best-practices`.

**Arquivos:** `Pricing/StockOverride.cs`, `Pricing/StockApplier.cs`, `Api/StockController.cs`,
`wwwroot/index.html`, `wwwroot/components.css`.

**Contadores:** 🔴 0 · 🟠 0 · 🟡 2 · 🟢 3

**Veredito:** sem bloqueadores. Todos os critérios de aceite atendidos e validados (ver abaixo). Os
achados são robustez de dados e melhorias de fluxo.

## Resolução (2026-07-19) — todos aceitos e aplicados

| ID | Resolução |
|---|---|
| **CR-U-01** | ✅ `StockController` normaliza: `disabled:true` grava só `{disabled:true}`; `disabled:false`/ausente nunca grava a chave; `disabled:false` sozinho → 400. Validado via API (PATCH `{stock:5,disabled:true}` → gravou `{disabled:true}`). |
| **CR-U-02** | ✅ Tooltip do ⚠ acrescenta "(quest-locked — only appears after the quest)" quando todos os vendedores por dinheiro são quest-locked. Validado. |
| **CR-U-03** | ✅ Item disabled não renderiza badge `OVR` nem ↺ inline (só o preço riscado). Validado. |
| **CR-U-04** | ✅ Clamp de tooltip via `--tip-shift` (JS `clampTooltip` no mouseover/focusin + `calc()` no transform). Validado: shift −30px num tip colado na borda, 0 no centro. |
| **CR-U-05** | ✅ ⚠ clicável → `openConflictResolver` expande o item e abre o editor no primeiro trader vendedor, pronto pra desabilitar. Validado. |

## Critérios de aceite — validados

| Pedido | Estado |
|---|---|
| Tooltips quebram em várias linhas | ✅ `white-space:normal` + `max-width:280px` (screenshot: 4 linhas) |
| Quantidades (stock/buy-cycle) à esquerda do preço, sem abrir editor | ✅ `.trader-avail` na linha (`54 stock ∞/cyc`) |
| Editor sem scroll/mal posicionado | ✅ Accordion full-width abaixo da linha (`display:contents` no `<tr>`) |
| Popover de clareza em cada campo | ✅ 4× `.hint-q (?)` com a semântica stock×buy-cycle |
| Desabilitar venda por trader = remover do assort | ✅ `StockApplier` via `RemoveItemFromAssort`; **in-game: Artem removido do MAG5-60 após restart** |
| Alerta de inconsistência flea-banned + vendido por trader | ✅ ⚠ na listagem (185 itens; MAG5-60 incluso) |
| Reorder: Trader sell → buy → SPT/handbook → SPT → dev → market | ✅ confirmado no DOM |

## Verificações empíricas (sem defeito)

- **Critério de root do disable:** `SlotId=="hideout"` ≡ `ParentId=="hideout"` no assort real (Prapor:
  420/420, 0 mismatch) — meu `SlotId` casa os mesmos roots que o helper do SPT usa.
- **Conflito × barter:** barter carrega `price`/`priceRUB` nulos, então itens só-barter **não** disparam o
  flag (0 falsos positivos). O filtro `price!=null || priceRUB!=null` está correto.
- **Transições enable↔disable no Save:** revisadas — desabilitar sobrescreve stock override por
  `{disabled:true}`; reabilitar com campos vazios faz DELETE; undo do audit restaura o estado exato.

---

## CR-U-01 · E/D — Robustez de dados · 🟡 Médio

**`StockController` grava `disabled` sem normalizar contra `stock`/`buyLimit`**

**Local:** [`Api/StockController.cs:72-76`](../modded/Server/Api/StockController.cs#L72)

**Problema:** o PATCH grava cada campo presente de forma independente:
```csharp
if (body.Stock is { } st) entry["stock"] = st;
if (body.BuyLimit is { } lim) entry["buyLimit"] = lim;
if (body.Disabled is { } dis) entry["disabled"] = dis;
```
Dois estados inconsistentes são graváveis: (a) `disabled:false` → entry-lixo `{disabled:false}` (que
`stockOverrideFor` na UI retorna como override, embora inócuo); (b) `{stock:5, disabled:true}` →
`StockApplier` prioriza `disabled` e ignora `stock`, mas o config carrega um `stock` órfão que confunde
uma leitura/edição futura do arquivo.

**Por que importa:** a UI atual nunca gera esses estados (o `desired` é `{disabled:true}` OU
`{stock,buyLimit}`, exclusivos), mas a API é a fronteira e não deve depender do cliente para manter o
config coerente. Um hand-edit ou um cliente futuro pode gravar lixo.

**Sugestão:** normalizar antes de gravar — se `disabled == true`, gravar **só** `{disabled:true}`; se
`disabled` é `false`/ausente, não gravar a chave `disabled`. (Mesmo espírito do no-op guard dos outros
writers.)

**Decisão:** `[ ]` Pendente · `[ ]` Aceitar · `[ ]` Rejeitar (dívida)

---

## CR-U-02 · C — Gap de sinalização · 🟡 Médio

**O flag de conflito não distingue vendedores quest-locked**

**Local:** [`wwwroot/index.html`](../modded/Server/wwwroot/index.html) — `renderRow`, `soldByTrader`

**Problema:** dos 185 itens flagados, **31** são vendidos **apenas** por traders quest-locked. O item só
aparece na loja depois de completar a quest, mas o ⚠ trata igual a uma venda imediata. Tecnicamente
ainda é uma inconsistência (o item é vendível), mas o operador não sabe que é gated por quest ao olhar
a listagem.

**Por que importa:** um operador pode "corrigir" (desabilitar) uma venda que na prática só existe após
uma quest, sem essa informação. Não é um falso positivo, é falta de contexto.

**Sugestão:** enriquecer o `data-tip` do ⚠ com "(sold by quest-locked trader)" quando **todos** os
vendedores por dinheiro forem `questLocked`, ou um segundo glifo/cor. Opcional.

**Decisão:** `[ ]` Pendente · `[ ]` Aceitar · `[ ]` Rejeitar (dívida)

---

## CR-U-03 · E — Redundância visual · 🟢 Menor

Um item **disabled** que também tem override de preço mostra o preço riscado (`is-off`) **e** o badge
`OVR` na mesma célula — dois marcadores para um item que nem está à venda. Considerar esconder o `OVR`/↺
inline quando `disabled`. [`renderTraderRow`](../modded/Server/wwwroot/index.html)

**Decisão:** `[ ]` Pendente · `[ ]` Aceitar · `[ ]` Rejeitar (dívida)

---

## CR-U-04 · F — Melhoria (pré-existente) · 🟢 Menor

Tooltips continuam ancorados em `left:50%` sem clamp de viewport — um tip de 280px perto da borda
direita (ex.: campos do accordion na coluna direita) pode cortar na tela. Era pior com `nowrap`; um
clamp por JS resolveria de vez. [`components.css:1772`](../modded/Server/wwwroot/components.css#L1772)

**Decisão:** `[ ]` Pendente · `[ ]` Aceitar · `[ ]` Rejeitar (dívida)

---

## CR-U-05 · F — Melhoria de fluxo · 🟢 Menor

O ⚠ de conflito é informativo mas resolver exige expandir o item → achar o trader → desabilitar. Uma
ação rápida "disable trader sale" a partir do próprio alerta (ou do menu de ação da célula) fecharia o
loop achar→corrigir. Ideia para depois.

**Decisão:** `[ ]` Pendente · `[ ]` Aceitar · `[ ]` Rejeitar (dívida)

---

## Histórico de Alterações

| Data | Autor | Alteração |
|---|---|---|
| 2026-07-19 | Guilherme | Criação — review da reforma de UX + disable-sale + conflito + tooltip (0 🔴/🟠, 2 🟡, 3 🟢). |
| 2026-07-19 | Guilherme | Todos os 5 achados (CR-U-01..05) aceitos, aplicados e validados (API + Chrome). |
