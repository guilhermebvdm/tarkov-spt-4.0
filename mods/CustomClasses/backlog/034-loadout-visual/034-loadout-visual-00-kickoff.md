# 034 — Loadout visual (gear slots + stash com ícones e tooltip) · Kickoff

**Mod:** CustomClasses · **Data:** 2026-06-10 · **Origem:** comparação de UX com o painel de loadout do viewer antigo
**Épico:** UX do editor (030–035) · **Wave:** UX-W3 · **Deps:** 033 (coluna direita do dashboard)

> Brief de kickoff — insumo para `/create-spec 034`. Não é a spec.

## Problema (UX)

O viewer antigo mostrava o equipado como **slots visuais estilo Tarkov** (2 linhas: HEADWEAR|ARMOR|RIG|BACKPACK / ON BACK|HOLSTER|SHEATH, ícone dimensionado pelo tamanho real do item, label) e o stash como **grid de ícones agrupado por categoria** com badge de quantidade — e **hover tooltip** com nome/tamanho/peso/preço (sem clique). O detalhe atual é texto puro.

## Escopo

- **`GearPanel.razor`:** slots do equipado em grid visual (port do layout `profiles.css:256-495`): label do slot, ícone do item (tarkov.dev `https://assets.tarkov.dev/{tpl}-icon.webp`, decisão do 023; `onerror` → fallback texto), dimensionado por `_props.Width/Height` (expor no `CatalogService` se faltar), nome curto truncado. Slots vazios esmaecidos.
- **`StashPanel.razor`:** itens agrupados por categoria do handbook (Weapons/Armor/Mags/Ammo/Meds/...), grid de ícones proporcionais, **badge de qty**, subtotal ₽ por grupo.
- **Tooltip de item (hover, sem clique):** nome, categoria, tamanho em slots, preço flea, qty — MudTooltip ou popover custom (port `profiles.css` tooltip). Reusar nos pickers (023) se sair barato.
- Adoção no `ClassDetail` (coluna direita do 033). Aba Equipado/Stash do **edit** ganha os ícones nas linhas (sem redesign do editor — os forms ficam).
- **Aba Stash do edit: agrupamento + filtro (review #12):** hoje são ~27 cards na ordem do JSON — achar um item é scroll cego. Agrupar os cards por categoria do handbook (mesma taxonomia do `StashPanel` read-only) com headers colapsáveis + campo de filtro por nome no topo da aba. Add item continua igual.
- Fecha a meta single-screen do dashboard (DoD herdado do 033: classe completa numa tela 1080p com ≤1 scroll).
- Offline: sem internet os ícones somem e sobra o texto (comportamento já aceito no 023).

## Refs

- `tools/tarkov-itemdb/viewer/profiles.js:243-320` (renderLoadout), `profiles.css:256-495` (gear/stash/tooltip)
- `CatalogService.cs` (nome/preço/categoria; + dims se faltar), `ClassViewItemSpec.razor`
- Território: `Web/Shared/GearPanel.razor`/`StashPanel.razor` (novos), `ClassDetail.razor` (pós-033), toques pontuais em `ItemSpecEditor.razor`

## DoD (resumo)

- Equipado e stash legíveis "de relance" (ícones+grupos+qty) e tooltip no hover com preço — 0 cliques.
- Sem internet, degrada pra texto sem quebrar layout.
