# Code review — B-9 filtro Vanilla · B-10 filtro Trader exclusivo · B-11 loyalty level · B-12 chevron

> **Data:** 2026-07-26<br>
> **Status:** 🟢 Vivo<br>
> **Responsáveis:** Guilherme<br>
> **Referências:** [code-review-trader-column-groupavatars.md](./code-review-trader-column-groupavatars.md)<br>

---

**Escopo:** revisão dos quatro itens de backlog desenvolvidos via `/g-autodev`:

- **B-9** — pseudo-opção "Vanilla (no mod)" no filtro Mod (só itens sem `modSource`).
- **B-10** — toggle "only these" (exclusivo) no filtro Trader: alterna entre modo OR e modo exclusivo (item aparece só se TODOS os seus ofertantes de cada lado S/B estão na seleção).
- **B-11** — editar o loyalty level (1-4) em que um item desbloqueia por trader; persiste em `config/stock-overrides.json` e muta `TraderAssort.LoyalLevelItems` no boot.
- **B-12** — chevron da árvore lateral só expande; a seleção ocorre só no rótulo.

**Arquivos:** `wwwroot/index.html`, `wwwroot/components.css` (B-9/B-10/B-12 + UI do B-11); `Pricing/StockOverride.cs`, `Pricing/StockApplier.cs`, `Api/StockController.cs`, `Api/DebugController.cs` (backend do B-11).

**Metodologia:** `/code-review` (6 categorias × 4 impactos), executada por **3 sub-agents adversariais independentes** (contexto limpo, não quem escreveu): (A) B-9/B-10/B-12, (B) backend B-11, (C) UI B-11. Validação end-to-end via Chrome DevTools MCP + `/debug/verify-price` no install local D:\SPT.

**Contadores:** 🔴 0 · 🟠 1 · 🟡 5 · 🟢 7

**Veredito:** sem bloqueadores. **Todos os achados acionáveis foram aplicados e revalidados** — exceto CR-A-04 (a11y da árvore), deferido por decisão do usuário como dívida pré-existente. Um bug 🟠 de reversão de estado (CR-A-01) e um bug 🟡 de cache stale (encontrado na validação, antes do review) foram corrigidos e provados com teste end-to-end.

## Bug pré-review encontrado na validação (corrigido)

Durante a validação via Chrome, o **modo exclusivo do B-10 não surtia efeito ao ser ligado**: `filterStateKey()` (a cache key de `_filterCache`) incluía `selectedTraders` mas **não** `traderExclusive`, então togglar o exclusivo sem mudar a seleção retornava a lista cacheada do modo OR. Fix: adicionar `traderExclusive` à cache key (`index.html:1382`). Revalidado: exclusivo ON = 0 violações (só o trader selecionado vende cada item); OFF = itens multi-seller reaparecem. Os 3 revisores confirmaram o fix presente, correto e completo.

## Resolução (2026-07-26) — todos aplicados e revalidados, exceto CR-A-04 (deferido)

| ID | Impacto | Resolução |
|---|---|---|
| **CR-A-01** | 🟠 | ✅ `URL_INITIAL.delete('trExcl')` consome o param na 1ª leitura de `buildTraderFilter` (espelha `urlListOverride`). **Validado:** desligar o toggle após vir de `?trExcl=1` persiste através de um edit de trader (rebuild real) — não re-liga. |
| **CR-A-02** | 🟡 | ✅ **Decisão do usuário: ignorar a Fence** no modo exclusivo. `subsetIgnoringFence` descarta a Fence dos ofertantes salvo se explicitamente selecionada. **Validado:** `[B]:Mechanic` exclusivo mostra 60/60 itens co-comprados pela Fence (antes: excluídos → filtro inútil). |
| **CR-A-03** | 🟢 | ⚪ Aceito como limitação do icon-rail (<1000px esconde label/count/chevron juntos). Documentado, sem código. |
| **CR-A-04** | 🟡 | ⏸️ **Deferido (decisão do usuário)** — a11y por teclado da árvore é dívida pré-existente da sidebar inteira, escopo do `trl-ds-validation` (role=treeitem + navegação por setas WCAG 2.2). B-12 não regrediu a11y. |
| **CR-A-05** | 🟢 | ✅ Marcador `is-exclusive` no trigger fechado (badge acentuado + tooltip); sincronizado no `syncEx` (o toggle não passa por `refresh()`). **Validado:** marker no load/off/on. |
| **CR-B-01** | 🟡 | ✅ `StockApplier` valida o LL cru em 1-4 (config é hand-editável) — fora da faixa → `badLoyalty++`, não aplica. Evita o item sumir silenciosamente (LL 99 → gate inalcançável). |
| **CR-B-02** | 🟡 | ✅ Mitigado pelo hard-cap 1-4 no applier (traders vanilla têm 4 níveis). Limitação de custom traders com < 4 níveis documentada aqui. |
| **CR-B-03** | 🟢 | ✅ Contador `loyaltyNoDict` quando o trader não tem `LoyalLevelItems` (log de boot) — antes o override sumia sem feedback. |
| **CR-B-04** | 🟢 | ✅ Docstring corrigida: `ResetExpiredTrader` **não clona** `LoyalLevelItems` — só reatribui `Assort.Items`; o dict mutado é retido e as chaves (root Ids) continuam batendo. |
| **CR-B-05** | 🟢 | ✅ Audit `after` agora reflete o **entry normalizado** (não o request cru) — `{disabled:true, loyaltyLevel:3}` loga só `{disabled:true}`. |
| **CR-B-06** | 🟢 | ✅ Helpers `ReadDouble/Int/Bool` toleram hand-edit de tipo errado (`"3"`/`3.5`) sem 500 na leitura do entry anterior. |
| **CR-C-01** | 🟡 | ✅ `_dataVersion++` antes de `rerenderMainRow` (em `reflectAll` e `undoApplyStock`) — o group-avatar reflete o LL novo na hora, sem discordar da detail-row. **Validado.** |
| **CR-C-02** | 🟢 | ✅ Picker de LL monta as opções de `[1,2,3,4] ∪ {vanillaLL, effLLInit}` — a opção selecionada sempre existe; sem override espúrio de LL1 quando `vanillaLL ∉ 1-4`. |
| **CR-C-03** | 🟢 | ✅ `showLL = vanillaLL != null || curOvrLL != null` — override de LL sem LL vanilla na cache continua editável pelo picker. |
| **CR-C-04** | 🟢 | ✅ Group-avatar da main-row marca o LL como override (`is-capped` + tooltip) via `loyaltyOverridden`. **Validado:** `LL1` acentuado + `L1⃥2⃥` na detail-row. |

## Verificações empíricas (sem defeito — confirmadas pelos revisores)

- **B-11 backend end-to-end:** editar LL3→LL1 gravou `{loyaltyLevel:1}`; o boot logou `3 loyalty-level entries applied` (o tpl tem 3 root offers); `/debug/verify-price` confirmou `LoyalLevelItems` live = 1 nos 3 offers. Persistência através do refresh: `ResetExpiredTrader` só reatribui `Assort.Items`, nunca toca `LoyalLevelItems` (revisor B leu o SPT source).
- **B-11 preserva stock ao editar só o LL:** item `{stock:5}` + mudar só o LL → `{stock:5, loyaltyLevel:3}` (o input de stock é pré-preenchido com `curOvrStock` e reentra no `desired`). O cenário crítico de perda de dados **não ocorre** (revisor C traçou).
- **B-11 disable × LL:** `Disabled=true` faz `continue` antes do branch LL; o controller normaliza para `{disabled:true}` (descarta LL); `RemoveItemFromAssort` também remove a entry de loyalty — cobertura em três camadas.
- **B-9:** condição de match (`!it.modSource`) idêntica à contagem (`else` de `if (it.modSource)`) — `undefined`/`null`/`""` caem juntos. Sem XSS: label passa por `escapeHtml`, value só vai para `dataset`/`URLSearchParams`.
- **B-10 semântica AND-entre-lados:** lado sem seleção → `null` → não restringe; lado com seleção exige `≥1 ofertante real dentro da seleção` (evita `every` vacuamente-true).
- **B-12 chevron:** `.chev` é `<span>` vazio (seta via `::before`), então `e.target.closest('.chev')` nunca falha por filho interno. Clique no chevron expande sem selecionar; clique no rótulo seleciona (`?cat=...`) sem alterar expansão. Ambos validados.
- **B-11 concorrência:** PATCH/DELETE em `writeLock.RunAsync` (SemaphoreSlim(1,1)); `RawConfigStore.Write` é tmp+rename.

## Limitações conhecidas (não-bloqueadoras, documentadas)

- **Custom traders com < 4 loyalty levels (CR-B-02):** o picker oferece 1-4 e o applier aceita 1-4; setar um LL acima do máximo real de um custom trader gate o item atrás de um nível inalcançável. Traders vanilla têm 4 níveis, então não afeta o uso normal.
- **a11y da árvore por teclado (CR-A-04):** deferido; a sidebar é mouse-only (dívida pré-existente).
- **Icon-rail <1000px (CR-A-03):** categorias não expandem nesse breakpoint (o chevron some junto com label/count).

---

## CR-A-01 · B-10 · Categoria A/C · 🟠 Forte — ✅ Aplicado

**Toggle "only these" re-ligava sozinho após rebuild do filtro (param `trExcl` da URL nunca consumido)**

**Local:** [`wwwroot/index.html`](../modded/Server/wwwroot/index.html) — `buildTraderFilter` (leitura de `URL_INITIAL.get('trExcl')`).

**Problema:** `URL_INITIAL` retinha `trExcl`, e `buildTraderFilter` (re-executado a cada edit de trader via `refreshConflictFilter`) o relia com prioridade sobre a memória — reintroduzindo o modo exclusivo mesmo após o usuário desligá-lo, se a sessão veio de `?trExcl=1`. Exatamente a classe de bug que `urlListOverride` já previne com `URL_INITIAL.delete(paramKey)`.

**Sugestão aplicada:** `const exFromUrl = URL_INITIAL.get('trExcl'); URL_INITIAL.delete('trExcl');` e fallback para localStorage só quando o param está ausente.

**Decisão:** `[x]` Aceitar sugestão — validado end-to-end (toggle OFF persiste após rebuild real).

---

## CR-A-02 · B-10 · Categoria C/A · 🟡 Médio — ✅ Aplicado (decisão do usuário)

**Fence entrava nos conjuntos S/B do modo exclusivo, penalizando itens co-ofertados por ela**

**Local:** `filterItems` branch `traderExclusive` + `sellSideRows`/`buySideRows`.

**Problema:** `regularMoneySellers` exclui a Fence, mas `sellSideRows`/`buySideRows` não. Como a Fence compra ~4539 itens, o modo exclusivo no lado `[B]` ficava quase sempre vazio (todo item co-comprado pela Fence era excluído).

**Decisão do usuário (múltipla escolha):** **ignorar a Fence** na avaliação de exclusividade — consistente com o resto do app, que a trata como mercado dinâmico. `subsetIgnoringFence` descarta a Fence dos ofertantes salvo se explicitamente selecionada. Validado: `[B]:Mechanic` exclusivo mostra 60/60 itens co-comprados pela Fence.

**Decisão:** `[x]` Aceitar com modificação: ignorar Fence (não removê-la das opções de filtro).

---

## CR-B-01 · B-11 · Categoria B · 🟡 Médio — ✅ Aplicado

**StockApplier aplicava `LoyaltyLevel` cru; valor fora de 1-4 (hand-edit) sumia com o item silenciosamente**

**Local:** [`Pricing/StockApplier.cs`](../modded/Server/Pricing/StockApplier.cs) — branch B-11.

**Problema:** o fence 1-4 existia só no controller; o StockApplier lê o JSON cru (`DeserializeFromFile`) e confiava. Um `loyaltyLevel: 99` hand-editado → `LoyalLevelItems[id]=99` → o consumidor (`StripLockedLoyaltyAssort`) remove o item para todos, na loja E no mirror do flea, sem flag `disabled` (phantom-bug).

**Sugestão aplicada:** validar `ll` em 1-4 no applier (uma vez por tpl); fora da faixa → `badLoyalty++`, não aplica; sem `LoyalLevelItems` → `loyaltyNoDict++` (CR-B-03). Ambos no log de boot.

**Decisão:** `[x]` Aceitar sugestão.

---

## CR-C-01 · B-11 · Categoria C · 🟡 Médio — ✅ Aplicado

**Group-avatar da main-row lia `sellSideRows` do memo antes do `_dataVersion` bumpar → LL antigo até re-render**

**Local:** `openTraderEdit` `reflectAll` + `undoApplyStock`.

**Problema:** `rerenderMainRow` (que lê o `sellSideRows` memoizado, agora com o LL efetivo do group-avatar) rodava antes do único `_dataVersion++` do caminho (na 1ª linha de `refreshConflictFilter`). O avatar colapsado mostrava o LL antigo enquanto a detail-row já mostrava o novo — dois valores conflitantes até um scroll/filtro forçar re-render.

**Sugestão aplicada:** `_dataVersion++` no início de `reflectAll` e antes do `rerenderMainRow` em `undoApplyStock`.

**Decisão:** `[x]` Aceitar sugestão — validado (group-avatar e detail-row concordam na hora).

---

## Achados menores aplicados (🟢)

- **CR-A-05** — marcador `is-exclusive` no trigger fechado (badge + tooltip), sincronizado no `syncEx`. Validado.
- **CR-B-03** — contador `loyaltyNoDict` quando o trader não tem `LoyalLevelItems`.
- **CR-B-04** — docstring de persistência corrigida (StockApplier + DebugController): `ResetExpiredTrader` não clona `LoyalLevelItems`.
- **CR-B-05** — audit `after` do entry normalizado, não do request cru.
- **CR-B-06** — leitura tolerante (`ReadDouble/Int/Bool`) do entry anterior contra hand-edit de tipo errado.
- **CR-C-02** — opções do picker de LL de `[1,2,3,4] ∪ {vanillaLL, effLLInit}` (opção selecionada sempre existe).
- **CR-C-03** — `showLL` também quando há override de LL sem LL vanilla na cache.
- **CR-C-04** — group-avatar marca o LL como override (`is-capped` + tooltip). Validado.

## Histórico de Alterações

| Data | Autor | Alteração |
|---|---|---|
| 2026-07-26 | Guilherme | Criação. Review adversarial (3 sub-agents) dos itens B-9/B-10/B-11/B-12. 0 bloqueadores; 1🟠 + 5🟡 + 7🟢. Todos aplicados e revalidados, exceto CR-A-04 (a11y deferida). Inclui fix pré-review do cache stale do B-10 (`filterStateKey`). |
