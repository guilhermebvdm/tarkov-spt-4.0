# TRL Design System

> **Data:** 2026-07-03<br>
> **Status:** 🟢 Vivo<br>
> **Responsáveis:** Guilherme<br>
> **Referências:** [PATTERNS.md](./PATTERNS.md)<br>

---

Linguagem visual padrão dos **editores web dos mods TRL** (Tarkov Red Line · SPT 4.0 · Fika Coop): base **grafite neutra** (como a UI real do EFT), tipografia **Bender** (a fonte oficial do jogo), **tan/gold** como accent de significado e **vermelho** disciplinado como cor de marca (laser da logo).

**Veja tudo vivo:** abra [`design-system.html`](./design-system.html) no browser (funciona via `file://`).

## Quickstart

```html
<link rel="stylesheet" href="design-system/fonts/bender.css">  <!-- opcional -->
<link rel="stylesheet" href="design-system/tokens.css">
<link rel="stylesheet" href="design-system/components.css">
<link rel="stylesheet" href="design-system/utilities.css">
<style>html, body { margin: 0; height: 100% }</style>  <!-- .trl-shell usa 100vh -->

<body class="trl-app"> … </body>
```

Editor novo? Copie [`templates/editor-starter.html`](./templates/editor-starter.html) e comece de lá.

## Arquivos

| Arquivo | Conteúdo |
|---|---|
| [`tokens.css`](./tokens.css) | Design tokens `--trl-*` em 3 camadas (primitivos → semânticos → assinaturas) |
| [`components.css`](./components.css) | Componentes `.trl-*`: shell, nav, forms (incl. multi-select de filtros), tabelas, cards, tags, progress, toasts, modal + **game data** (cell, grid2d, doll, mod-tree, heatmap, facções) |
| [`utilities.css`](./utilities.css) | Utilitários mínimos `.trl-u-*` |
| [`design-system.html`](./design-system.html) | Showcase premium navegável (cores, tipo, componentes, do's & don'ts) |
| [`templates/editor-starter.html`](./templates/editor-starter.html) | Shell de editor funcional para copiar |
| [`PATTERNS.md`](./PATTERNS.md) | Regras normativas (disciplina do vermelho, radius 0, AA) + receitas |
| [`CLAUDE.md`](./CLAUDE.md) | Contexto de uso para agentes |
| [`fonts/`](./fonts/) | Bender woff2 + `@font-face` + nota de licença |
| [`assets/`](./assets/) | Logo TRL (`trl-logo-dark` hero · `trl-logo-mark` topbar) + `bg-ambient.jpg` (key art p/ `.trl-app--photo`/hero) + `icons.svg` (sprite Tabler requadrada, 55 ícones) — PROVENANCE.txt |

## Versionamento

Semver em [`CHANGELOG.md`](./CHANGELOG.md):

- **MAJOR** — remove/renomeia token ou classe (quebra consumidores)
- **MINOR** — componente ou token novo (aditivo)
- **PATCH** — ajuste visual sem mudança de API

## Histórico de Alterações

| Data | Autor | Alteração |
|---|---|---|
| 2026-07-03 | Guilherme | Criação (v1.0.0) |
