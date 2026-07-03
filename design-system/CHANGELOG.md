# Changelog — TRL Design System

Semver: **MAJOR** quebra token/classe · **MINOR** componente/token novo · **PATCH** ajuste visual.

## 1.0.0 — 2026-07-03

Primeira versão.

- `tokens.css` — namespace `--trl-*` em 3 camadas: primitivos (surfaces oliva, ramps tan/red/status, ink, edges, washes, tipo, spacing 4px, sombras/glows, z-index, motion), semânticos (`--trl-bg-*`, `--trl-fg-*`, `--trl-accent*`, `--trl-brand`, `--trl-danger*`, focus ring) e assinaturas (laser, texturas, progress fill, chamfer).
- `components.css` — blocos A–H: base `.trl-app` (texturas + scrollbars + focus-visible), shell (`trl-shell/topbar/screen-bar/panel/toolbar/workspace`), navegação (`trl-nav/tree/tabs/breadcrumb/pagination`), forms (`trl-btn` + variantes, `trl-field/input/select/checkbox/radio/switch/search/dropdown/slider/form-grid`), data display (`trl-table/kv/card/tag/chip/badge/stat/progress/eyebrow/sec/divider/h1-h3/lede/code/mono`), feedback (`trl-toast/alert/spinner/skeleton/empty` + tooltip `[data-trl-tip]`), overlays (`trl-modal/popover/menu`), motion (keyframes `trl-*` + reduced-motion).
- `utilities.css` — `.trl-u-*` (num/mono/cores/truncate/flex/gap/margens/sr-only).
- `design-system.html` — showcase navegável com scroll-spy, copy de tokens, contraste AA medido e do's & don'ts.
- `templates/editor-starter.html` — shell de editor funcional.
- `fonts/` — Bender Regular/Bold woff2 (origem tarkov.dev; ver LICENSE-NOTE.txt) com fallback Bahnschrift.
- `assets/` — logo TRL integral + marca compacta.
- Docs: `README.md`, `CLAUDE.md`, `PATTERNS.md` (regras R1–R7 + receitas).
