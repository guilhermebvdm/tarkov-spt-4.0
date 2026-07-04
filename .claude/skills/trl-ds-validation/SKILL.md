---
name: trl-ds-validation
description: Valida telas/componentes contra o TRL Design System com 4 lentes — readability, a11y (WCAG AA + targets 2.2), i18n PT-BR/EN e dataviz (paletas validadas por script). Use ao revisar ou refatorar qualquer editor web de mod (/code-review de UI, refatoração visual, tela nova), quando o user pedir "valide o design/a11y/leiturabilidade", ou antes de mergear mudanças em design-system/.
---

# TRL DS — lentes de validação

Contexto fixo: editores web **desktop, dark-only, offline** (servidos pelo SPT server), comunidade **PT-BR/EN**. Fora de escopo por decisão: tema claro, mobile/touch, RTL, framework de i18n (pluralização/ICU é do consumidor).

Fonte de verdade dos tokens/regras: `design-system/` (ler `CLAUDE.md` + `PATTERNS.md` antes).

## Lente 1 — Readability

- Corpo 12–13px Segoe UI; uppercase+tracking **só** display/labels (>4 palavras = corpo). Prosa ≤72ch.
- Números comparáveis: mono + `tabular-nums` (`.trl-u-num` / `.num`), alinhados à direita.
- Hierarquia por elevação (superfície mais clara) + eyebrow/sec — não por tamanho de fonte inflado.
- Texto em px por decisão (ferramenta densa; zoom do browser cobre) — não "corrigir" para rem sem decisão registrada.

## Lente 2 — A11y (WCAG AA + 2.2)

- Contraste medido (não estimado): `ink` AAA, `ink-muted` AA em toda superfície; `tan-500` só labels uppercase ≥11px em ground/surface-1; `ink-faint` decorativo. **Labels de chrome = `--trl-fg-label` (neutro)** — gold em label é defeito (gold marca significado). Números completos: PATTERNS R5.
- `:focus-visible` vem do escopo `.trl-app` — **verificar por tab real** que todo interativo é alcançável e mostra ring.
- **Targets ≥24×24px** (WCAG 2.2): botões `--sm` 24 ✓; ícones de fechar/limpar precisam de 24; área de clique de links de toolbar via padding. Checkbox/switch: o `<label>` inteiro é o alvo (ok por construção).
- `prefers-reduced-motion` desliga keyframes `trl-*` — animação nova fora deles é defeito.
- Comportamento (aria/roles/teclado de dropdown, tabs, modal, DnD) é **do consumidor** — a lente cobra que exista, não que o CSS o forneça. Esc fecha overlays (receita do modal).

## Lente 3 — I18n (PT-BR/EN)

- **Orçamento de expansão: PT ≈ +30% sobre EN.** Todo rótulo com `white-space: nowrap` precisa de contenção (`min-width:0` + `overflow:hidden` + `text-overflow:ellipsis`) — testar com strings PT reais, não lorem.
- Sondar no browser: trocar `textContent` por versões PT longas e medir `scrollWidth`/vazamento (screen-bar title, doll labels, tabs, kv keys).
- Números/moeda/data formatados pelo consumidor com locale do jogo; DS garante só o alinhamento (tabular-nums).
- Uppercase CSS preserva acentos ✓; não usar texto em imagem.

## Lente 4 — Dataviz (skill `dataviz` + validador)

Regra de ouro: **cor de UI ≠ cor de série. Rodar o validador, não estimar.**

```
node <dataviz-skill>/scripts/validate_palette.js "<hex,...>" --mode dark --surface "#1b1b1d" [--ordinal]
```

Parâmetros TRL validados (2026-07-03, surface-1 `#1b1b1d`):

| Slot | Valor | Status |
|---|---|---|
| Categórica (ordem FIXA, nunca ciclar) | `#4a86c8` blue → `#b8892e` gold → `#4f9a6a` green → `#cf4b3c` red | PASS (pior ΔE adjacente 17.0) |
| Sequencial (magnitude) | tan `#6b6247 → #8f8560 → #ab9a71 → #c7b48a` (600→300) | PASS · **nunca** começar em tan-700/800 (reprovam contraste 2:1) |
| Divergente (polaridade) | `#4f9a6a` ↔ neutro `#7a786e` ↔ `#cf4b3c` | midpoint neutro por design (não validar chroma nele) |
| Status | green/amber/red-soft do DS — **reservados**, nunca série | — |

- 5ª série não existe: vira "Other" (cinza), small multiples ou encoding composto.
- **Nunca dual-axis**; sequencial = 1 hue monotônica; heat/chips sempre com o valor visível (número/±%) — cor nunca sozinha.
- ≥2 séries = legenda sempre; texto usa tokens de texto, nunca a cor da série.

## Procedimento

1. Ler `design-system/PATTERNS.md` (regras R1–R9) — esta skill NÃO as repete.
2. Rodar as sondas de browser (lentes 2–3) na tela alvo via Chrome DevTools MCP.
3. Gráfico novo? Seguir a skill `dataviz` (forma → cor → **validador** → marks → hover → a11y) com os parâmetros TRL acima.
4. Reportar por lente; fixes de contraste/paleta exigem número medido, não opinião.