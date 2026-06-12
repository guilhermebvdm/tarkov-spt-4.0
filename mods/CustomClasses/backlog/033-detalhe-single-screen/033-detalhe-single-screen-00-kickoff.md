# 033 — Detalhe single-screen (dashboard da classe) · Kickoff

**Mod:** CustomClasses · **Data:** 2026-06-10 · **Origem:** comparação de UX com `tools/tarkov-itemdb/viewer/profiles.html`
**Épico:** UX do editor (030–035) · **Wave:** UX-W2 (paralelo ao 032) · **Deps:** 030 (sidebar), 031 (componente de skills)

> Brief de kickoff — insumo para `/create-spec 033`. Não é a spec.

## Problema (UX)

No viewer antigo, **tudo de um perfil cabia numa tela**: header com badges (custo ponderado, ₽), coluna esquerda estreita com skills+hideout, coluna direita com equipado+stash — densidade 12-14px, padding mínimo, zero cliques pra ver qualquer coisa. O `ClassDetail` atual usa MudExpansionPanels empilhados (seções fechadas/scroll longo, padding generoso do MudBlazor) — ver skills + equipado + custo exige expandir/rolar.

## Escopo

- **Redesign do `ClassDetail.razor`** abandonando expansion panels: layout 2 colunas (port do `profiles.css:72-86`):
  - **Header compacto:** nome na cor da classe + ícone + displayName + badges (custo de skills c/ budget, loadout ₽, status, baseEdition, enabled) + descrição 1 linha (tooltip pro resto) + botões Edit/Duplicate/Delete.
  - **Coluna esquerda (~300px):** `SkillCanonicalList` (031, read-only) + multiplicadores embutidos como chips ±% na própria linha + hideout como badges (`Heating L1`) — sem painel separado.
  - **Coluna direita (flex):** Equipado em cima (lista compacta por slot — visual rico fica pro 034), Stash embaixo agrupado, com subtotais.
  - **Diagnostics** (se houver) como MudAlert fino no topo.
- **Densidade:** `Dense=true` em todos os Mud components da página + classe CSS local (`wwwroot/css/customclasses.css` novo, importado no layout) com tipografia 12-14px/line-height 1.3 e paddings 4-8px inspirados em `tokens.css` do viewer antigo. NÃO criar design system completo — só o necessário pro dashboard.
- **0 cliques** pra ver skills+equipado+stash+custos (nada atrás de expansão). Nota de expectativa (review #2): com o stash ainda **textual**, a meta "tudo numa tela ≤1 scroll" só fecha por completo no **034** (grids de ícones) — o 033 entrega a ESTRUTURA (2 colunas, zero expansões, densidade); o 034 entrega a compactação visual final.

## Refs

- `tools/tarkov-itemdb/viewer/profiles.js:95-111` (renderMain), `profiles.css:72-86` (layout), `:152-214` (densidade)
- `SkillCanonicalList` (031), `CostService` breakdowns, `ClassViewItemSpec.razor` (árvore existente — reaproveitar na lista compacta)
- **Território (review #3):** o 033 toca `ClassDetail.razor` + `wwwroot/css/customclasses.css` (novo) + a linha de import no `BaseLayout`. NavMenu/Layout são território do **030** (W1, já mergeado quando o 033 rodar); na UX-W2 o **032** adiciona apenas 1 link no NavMenu — o 033 NÃO toca NavMenu.

## DoD (resumo)

- Zero expansion panels: header+skills+mults+hideout+equipado+stash+custos todos visíveis sem cliques; estrutura 2 colunas densa (single-screen completo é DoD do 034).
- Trocar de classe pelo sidebar mantém o layout — comparação visual imediata.
