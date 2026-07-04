# Changelog — TRL Design System

Semver: **MAJOR** quebra token/classe · **MINOR** componente/token novo · **PATCH** ajuste visual.

## 1.0.0 — 2026-07-03

Primeira versão. Inclui o bloco **H. Game data** (primitivos de domínio Tarkov, derivados do inventário dos editores existentes): `trl-cell` (célula de item em unidades de jogo, qty/contents/label, estados empty/invalid/editable/dragging), `trl-grid2d` (stash 2D com hints de drop e ghost de drag), `trl-doll` (paper doll com slots em cqw), `trl-mod-tree` (árvore recursiva de slots de arma), `trl-heat--*` (heatmap tan p/ matrizes), `trl-card--usec/--bear` (facções, tokens `--trl-faction-*` + primitivo `--trl-blue-400`), `trl-crest`, `trl-fullscreen`. Comportamento (DnD/rotação/validação) permanece no consumidor — receitas no PATTERNS.md.

Filtros: `trl-multiselect` (dropdown de checkboxes com count no trigger, counts por opção, `__search` sticky p/ listas longas, Select all/Clear; painel persiste entre cliques) + `trl-filter-chip` (chips de filtros aplicados com remove) — receita "Barra de filtros" no PATTERNS.md. Dropdown e multiselect compartilham a casca (trigger/caret/painel/option) em seletores agrupados; diferenças intencionais são overrides explícitos comentados.

Iconografia oficial: Tabler Icons (MIT) requadrada para o hard-edges (caps butt/joins miter, stroke 1.75) — sprite curado `assets/icons.svg` (55 ícones UI + domínio: crosshair/target/shield/radar/swords/backpack…), componente `.trl-icon` (13/16/20px, currentColor), grade com click-copy no showcase, receita "Ícones" no PATTERNS; starter usa refresh/device-floppy nos botões do topbar.

Fundo ambiente com a key art oficial (escolha do user entre 3 protótipos ao vivo): `assets/bg-ambient.jpg` (1920w/317KB, proveniência documentada) + tokens `--trl-photo`/`--trl-scrim` + modifier `.trl-app--photo` (foto fixa sob scrim 86–93%; painéis opacos seguram AA). Editor-starter usa o ambiente; hero do showcase usa a banda com fade — receita "Fundo ambiente & hero" no PATTERNS.

Recalibração "grafite + chrome neutro" (feedback do user: gold constante na tela inteira): superfícies e ink neutros (o calor vem da vignette e dos accents), edges neutras com `--trl-edge-accent` reservada a tag/badge, labels de chrome via `--trl-fg-label`, botão base neutro, kv value promovido a `fg`. Gold agora só marca significado (ativo, selecionado, primário, assinaturas). Contrastes remedidos (muted 5.2–6.6 AA) e paleta viz revalidada contra a surface nova — tudo PASS.

Review por lentes (readability/a11y/i18n/dataviz — skill do repo `trl-ds-validation`): tokens `--trl-viz-*` de gráfico validados por script (categórica fixa 4 hues ΔE≥17, sequencial tan-600→300, divergente c/ neutro) + receita "Gráficos" no PATTERNS; screen-bar e doll label contidos p/ títulos PT longos (ellipsis); targets WCAG 2.2 ≥24px (search clear, modal close, breadcrumb via hit-area); regras novas R6 (px por decisão) e R8 (i18n PT-BR/EN); heat exige valor visível na célula.

Auditoria funcional via DevTools: fix de grid blowout no `trl-kv` (`minmax(0,1fr)` + `overflow-wrap` — ids longos quebravam o layout <900px); botão × do `trl-search` funcional nos templates; Esc fecha modal e fullscreen (demos); starter sem `--flush` no painel de formulário (respiro do `__body`).

Passe de polimento premium/fluidez: `trl-doll` espelha a tela Gear do EFT — grade de slots com áreas nomeadas 1:1 (`__slot--earpiece` … `--scabbard`, armas cruzando 2 colunas) sobre a silhueta SVG de soldado + coluna `__carry` (rig/pockets/backpack/pouch), em vez de posicionamento absoluto obrigatório; tabs com underline animada (scaleX); modal com entrada `trl-pop` + fade no overlay; `trl-cell` com transições e lift no hover editável; botões sólidos com luz superior sutil; `trl-card--interactive` (lift + `is-selected` p/ pickers); transições consistentes em table rows, menu, pagination, closes.

- `tokens.css` — namespace `--trl-*` em 3 camadas: primitivos (surfaces oliva, ramps tan/red/status, ink, edges, washes, tipo, spacing 4px, sombras/glows, z-index, motion), semânticos (`--trl-bg-*`, `--trl-fg-*`, `--trl-accent*`, `--trl-brand`, `--trl-danger*`, focus ring) e assinaturas (laser, texturas, progress fill, chamfer).
- `components.css` — blocos A–H: base `.trl-app` (texturas + scrollbars + focus-visible), shell (`trl-shell/topbar/screen-bar/panel/toolbar/workspace`), navegação (`trl-nav/tree/tabs/breadcrumb/pagination`), forms (`trl-btn` + variantes, `trl-field/input/select/checkbox/radio/switch/search/dropdown/slider/form-grid`), data display (`trl-table/kv/card/tag/chip/badge/stat/progress/eyebrow/sec/divider/h1-h3/lede/code/mono`), feedback (`trl-toast/alert/spinner/skeleton/empty` + tooltip `[data-trl-tip]`), overlays (`trl-modal/popover/menu`), motion (keyframes `trl-*` + reduced-motion).
- `utilities.css` — `.trl-u-*` (num/mono/cores/truncate/flex/gap/margens/sr-only).
- `design-system.html` — showcase navegável com scroll-spy, copy de tokens, contraste AA medido e do's & don'ts.
- `templates/editor-starter.html` — shell de editor funcional.
- `fonts/` — Bender Regular/Bold woff2 (origem tarkov.dev; ver LICENSE-NOTE.txt) com fallback Bahnschrift.
- `assets/` — logo TRL integral + marca compacta.
- Docs: `README.md`, `CLAUDE.md`, `PATTERNS.md` (regras R1–R9 + receitas).
