# 023 — Pickers de item (MudBlazor) · Kickoff

**Mod:** CustomClasses · **Data:** 2026-06-09 · **Origem:** plano aprovado do editor web de classes (`~/.claude/plans/`, sessão 2026-06-09; renumerado 022→023)
**Wave:** W2 (paralelo ao 024 — `Web/Shared/` × `Web/Pages/`) · **Deps:** 020, 022

> Brief de kickoff — insumo para `/create-spec 023`. Não é a spec.

## Objetivo

Componentes de seleção reutilizáveis (consumidos por 025/026/027/028), sobre o `CatalogService` (022).

## Escopo

- **Item picker:** busca por nome/tpl com filtro por categoria e por slot compatível; **virtualização** MudBlazor (~4k+ templates — busca não pode travar o circuit do Blazor Server).
- **Preset picker:** presets do tpl (default/premium) com contagem de attachments.
- **Ammo picker:** filtrado por calibre da arma.
- **Customization picker:** upper/lower filtrado por facção (USEC/BEAR), cobrindo vanilla e "aparência direta" (padrão AllTheClothes).
- **Spike de imagem de item:** estratégia — estáticos do SPT (ImageRouter é key-value dinâmico; investigar o que o server expõe) vs URL tarkov.dev (requer internet) vs texto puro (fallback aceitável). **Decisão registrada na tech-spec.**
- Página de teste temporária (ou seção na home do 020) para exercitar os pickers isolados.

## Riscos / atenção

- Performance da busca no Blazor Server (latência por roundtrip WebSocket) — debounce + server-side filter.
- Imagem de item é nice-to-have; não bloquear a wave por ela (texto é aceitável).

## Refs

- `Web/Shared/` (020) — onde os componentes vivem
- `CatalogService` (022) — fonte de dados
- `mods/Skills-Extended/modded/Server/Web/` — exemplos de componentes MudBlazor no padrão SPT

## DoD (resumo)

- Pickers funcionam isolados na página de teste; busca responsiva com o catálogo completo.
- Decisão de imagem documentada na tech-spec.
