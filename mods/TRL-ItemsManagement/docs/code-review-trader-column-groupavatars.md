# Code review — TRADER column group-avatars + popover + Trader filter

> **Data:** 2026-07-19<br>
> **Status:** 🟢 Vivo<br>
> **Responsáveis:** Guilherme<br>
> **Referências:** [code-review-trader-availability-ux.md](./code-review-trader-availability-ux.md)<br>

---

**Escopo:** commits `763d2681` (conflito limpa no disable + filtro conflito + fixes de tooltip) e
`54242162` (group-avatars na coluna TRADER + popover no hover + filtro "Trader" + sync com disable).
100% frontend (`wwwroot/index.html` + `components.css`). Metodologia do `/code-review` (6 categorias ×
4 impactos); foco em correção JS, performance, escaping e consistência (o C# não mudou).

**Contadores:** 🔴 0 · 🟠 0 · 🟡 2 · 🟢 3

**Veredito:** sem bloqueadores. A feature está validada end-to-end; os achados são polimento de UX e
DRY/perf.

## Resolução (2026-07-19) — todos aceitos e aplicados

| ID | Resolução |
|---|---|
| **CR-T-01** | ✅ Removido o `data-tip` das linhas S/B (0 de 24 linhas o têm) — só o popover explica agora. |
| **CR-T-02** | ✅ (a) `positionPopover(pop, anchor)` extraído e usado nos dois popovers (reward + trader). (b) `sellSideRows`/`buySideRows` memoizados por `_dataVersion` (`_sideRowsMemo`) — validado: mesma ref no 2º call, invalida no bump, restaura no re-enable. |
| **CR-T-03** | ✅ Incorporado ao memo — o `_dataVersion++` do `refreshConflictFilter` passa a ser **necessário** (invalida o memo de sell/buy), então deixa de ser desperdício. |
| **CR-T-04** | ✅ `closeTraderPopover()` no início de `onTableClick` — qualquer clique na tabela (inclusive expandir) fecha o popover. |
| **CR-T-05** | ✅ Listener de `scroll` (ligado uma vez) fecha ambos os popovers no scroll de roda. |

## Verificações empíricas (sem defeito)

- **Fence:** nunca aparece em `sellSideRows` (0 de 6270 itens), então não há o risco de mostrar `[S] Fence`
  no group-avatar/filtro sem poder desabilitá-lo (o `StockApplier` pula Fence). O `[B] Fence` (buyback,
  4539 itens) é só filtro/leitura, não afetado por disable.
- **+N bem dimensionado:** máximo de 4 vendedores e 7 compradores por item → o "+N" só ativa no lado B
  (mostra 5, +2); o lado S nunca colapsa. O cap de 5 é adequado.
- **Escaping:** nenhum nome de trader tem caractere HTML-especial; ainda assim `traderAvatarImg`/popover
  usam `escapeAttr`/`escapeHtml`.
- **Consistência disable:** group-avatar, popover, filtro e count usam o MESMO `sellSideRows()` (exclui
  disabled), então tudo some junto. Validado: desabilitar Skier tira-o da linha e baixa `[S]` 420→419.
- **Sem código órfão:** o markup antigo (`traderBuy`/`traderSellValue`/`buyOverridden`/`avatarFor` local)
  foi removido sem referências pendentes.

---

## CR-T-01 · E/B — UX redundante · 🟡 Médio

**Tooltip da linha S/B e popover aparecem ao mesmo tempo**

**Local:** [`wwwroot/index.html`](../modded/Server/wwwroot/index.html) — `traderLineMarkup` (`data-tip` na
`.cell-trader__line`) + `bindTraderHover` (popover na `.cell-trader`)

**Problema:** cada linha S/B carrega `data-tip="S = traders that SELL this to you … hover for all prices"`
(tooltip CSS, aparece em ~250ms de hover) **e** a célula abre o popover completo (~300ms). Os dois
flutuantes surgem quase juntos, sobrepostos — o tooltip literalmente diz "hover for all prices" e o
popover é justamente a lista de preços. Ruído visual.

**Por que importa:** o operador vê dois balões concorrentes; o tooltip vira redundante assim que o
popover (mais completo) abre.

**Sugestão:** remover o `data-tip` das linhas S/B — o popover já é a explicação (com a legenda S/B nos
headers). Se quiser manter uma dica curta antes dos 300ms, encurtá-la para só "S · sells to you" sem o
"hover for all prices".

**Decisão:** `[ ]` Pendente · `[ ]` Aceitar · `[ ]` Rejeitar (dívida)

---

## CR-T-02 · D/F — DRY / perf · 🟡 Médio

**Posicionamento de popover duplicado + `buySideRows` recomputa `buildBuySourceRows`**

**Local:** [`wwwroot/index.html`](../modded/Server/wwwroot/index.html) — `openTraderPopover` (posicionamento
copiado de `openRewardPopover`) · `buySideRows` (chama `buildBuySourceRows` por item)

**Problema:** (a) o bloco de posicionar-e-flip do popover é idêntico entre `openTraderPopover` e
`openRewardPopover` — um `positionPopover(pop, anchor)` compartilhado evitaria a divergência futura.
(b) `buySideRows(it)` chama `buildBuySourceRows(it)` (itera `TRADERS` + `dev.sellFor`) a cada chamada;
no `buildTraderFilter` e no filtro isso roda O(itens) vezes. Medido ~18ms no build e ~8ms por filtro —
aceitável hoje, mas cresceria com o catálogo.

**Por que importa:** débito de manutenção (duplicação) + custo linear evitável.

**Sugestão:** extrair `positionPopover(pop, anchor)` e usar nos dois popovers; se a perf virar
problema, memoizar `buySideRows`/`sellSideRows` por `(tpl, _dataVersion)`.

**Decisão:** `[ ]` Pendente · `[ ]` Aceitar · `[ ]` Rejeitar (dívida)

---

## CR-T-03 · F — Invalidação agressiva · 🟢 Menor

`refreshConflictFilter` faz `_dataVersion++` a cada save de trader (preço/estoque/disable) via
`reflectAll`, invalidando o filter-cache mesmo num save de **preço** (que não muda quais itens os
filtros conflito/Trader retornam). Um re-filtro extra (~8ms) por save. Aceitável; se quiser, só
incrementar quando o `disabled` de fato mudou. [`index.html`](../modded/Server/wwwroot/index.html)

**Decisão:** `[ ]` Pendente · `[ ]` Aceitar · `[ ]` Rejeitar (dívida)

---

## CR-T-04 · E — Popover · 🟢 Menor

O popover não fecha ao **expandir a linha** (clique no item) — o expand não passa por `render()`, então
o popover fica aberto até o `mouseleave`. Cosmético. Fechar em `onTableClick` quando a linha é
expandida resolveria.

**Decisão:** `[ ]` Pendente · `[ ]` Aceitar · `[ ]` Rejeitar (dívida)

---

## CR-T-05 · B — Popover (herdado) · 🟢 Menor

Rolar a página **com a roda** sem mover o mouse deixa o popover no lugar antigo (posicionamento
`absolute` + `scrollY`, sem reposicionar no scroll). Mesmo comportamento do reward popover já aceito.
Fechar no evento `scroll` resolveria os dois de uma vez.

**Decisão:** `[ ]` Pendente · `[ ]` Aceitar · `[ ]` Rejeitar (dívida)

---

## Histórico de Alterações

| Data | Autor | Alteração |
|---|---|---|
| 2026-07-19 | Guilherme | Criação — review dos group-avatars + popover + filtro Trader (0 🔴/🟠, 2 🟡, 3 🟢). |
| 2026-07-19 | Guilherme | Todos os 5 achados (CR-T-01..05) aceitos, aplicados e validados via Chrome. |
