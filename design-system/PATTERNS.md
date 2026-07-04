# TRL Design System — Patterns

> **Data:** 2026-07-03<br>
> **Status:** 🟢 Vivo<br>
> **Responsáveis:** Guilherme<br>
> **Referências:** [CLAUDE.md](./CLAUDE.md)<br>

---

Parte 1 = **Regras** (normativas — MUST/NEVER, valem para todo editor).
Parte 2 = **Receitas** (composições prontas de componentes).

## Parte 1 — Regras

### R1. Disciplina do vermelho

O vermelho é a marca TRL — ele fala porque é raro. **Se o vermelho ocupa mais de ~5% da área da tela, está errado.**

Usos permitidos de `--trl-brand` (`#ff0000`), lista exaustiva:

| Uso | Componente |
|---|---|
| Laser divider (máx. **1 por view**) | `.trl-divider--laser` |
| Laser do topbar (vem de graça com o componente) | `.trl-topbar::after` |
| Dot "live/dirty" pulsante (estado não salvo) | `.trl-screen-bar__dot--live` |
| Dot de tag de marca | `.trl-tag--brand` |
| Logo TRL | `assets/` |

- `#ff0000` **nunca** colore texto nem preenche áreas — é luz (glow/laser), não pigmento.
- Texto de status de erro/perigo: **só** `--trl-danger` (`red-soft #d27a7a`).
- Ação destrutiva: `.trl-btn--danger` (`red-500`). Botão vermelho em ação **não destrutiva** é defeito.
- Ênfase pontual em ícone/valor: `--trl-red-300` (`#ff6b60`), com moderação.

### R2. Geometria

- `--trl-radius: 0` — **sempre**. Nenhum componente redondo (exceção: dots de status e radio, círculos por natureza).
- Bordas: `1px` **neutras** via `--trl-edge*` (a estrutura não é dourada). A moldura tan é `--trl-edge-accent`, reservada a elementos de accent (tag, badge). Nunca hex solto (`#444`).
- Barra de acento lateral: `2px` (`--trl-accent-bar-w`) — nav ativa, cards, alerts, toasts.
- **Chamfer** (`--trl-chamfer`): só em superfícies **sólidas sem borda** (`.trl-btn--primary`, `.trl-btn--danger`). `clip-path` corta a própria borda de 1px do elemento — chanfro em elemento com borda exige pseudo-elemento/gradiente e não faz parte do v1.

### R3. Tipografia

- Uppercase + letter-spacing (`--trl-track-*`) **só** em display: títulos, labels, chrome (screen-bar, tags, botões, th). Corpo de texto **nunca** — mais de ~4 palavras = corpo.
- Corpo é **Segoe UI** 12–13px, e não Bender: condensadas degradam leitura em tabelas densas. Bender fica no display. Refatorações futuras não devem "corrigir" isso para Bender-em-tudo.
- Todo número comparável/alinhável usa mono + `tabular-nums` (`.trl-u-num` ou `.num` em tabela).

### R4. Cor e tokens

- Labels de chrome (título de painel, label de field, chave de kv, thead) usam `--trl-fg-label` (neutro) — gold em label é defeito; gold marca **significado** (ativo, selecionado, primário, assinaturas).
- Produto consome **apenas tokens semânticos** (camada 2: `--trl-bg-*`, `--trl-fg-*`, `--trl-accent*`, `--trl-danger*`, `--trl-edge*`, `--trl-wash-*`). Primitivos (camada 1: ramps `tan-N`/`red-N`, surfaces) são internos do DS.
- Hover/seleção em CSS custom: usar `--trl-bg-hover`/`--trl-bg-active` — são os mesmos valores que os componentes do DS usam, por construção.
- **Hex hardcoded em editor é defeito de review.**
- Elevação = superfície mais clara (ladder `ground-deep → surface-4`), sombra reforça mas não substitui.

### R5. Acessibilidade (WCAG AA, valores medidos)

| Par | Razão | Veredito |
|---|---|---|
| `ink` sobre qualquer superfície | 11.7–15.0:1 | ✓ AAA — corpo padrão |
| `ink-muted` sobre qualquer superfície | 5.2–6.6:1 | ✓ AA — secundário + labels de chrome (`--trl-fg-label`) |
| `tan-300` sobre qualquer superfície | 7.1–9.1:1 | ✓ AA+ |
| `red-soft` sobre qualquer superfície | 4.7–6.0:1 | ✓ AA — único vermelho para texto |
| `fg-on-accent` sobre `tan-300` / branco sobre `red-500` | 9.1 / 4.9:1 | ✓ AA |
| `tan-500` (dim) | 5.0 ground · 4.7 surface-1 · **3.9–4.3 surface-2/3** | ◇ só labels uppercase ≥11px; nunca conteúdo essencial em superfície elevada |
| `ink-faint` | 2.9–3.7:1 | ◇ decorativo (captions, placeholders, separadores) |

- Critério: ≥4.5:1 texto normal; ≥3:1 texto ≥18px/bold e componentes de UI.
- `:focus-visible` vem de graça no escopo `.trl-app` (ring tan de 2 camadas) — não remover outline sem substituto.
- Animações desligam sozinhas com `prefers-reduced-motion` — não criar animação fora dos keyframes `trl-*` sem cobrir esse caso.
- **Comportamento (aria/teclado) é responsabilidade do consumidor** — o DS é CSS-only. Dropdown custom e tabs precisam de roles/teclado no app; para seleção simples, prefira o `.trl-select` nativo (acessível de graça).
- **Exceção de game-chrome:** os labels do paper doll usam 8px (fiéis à tela Gear do EFT), abaixo da diretriz de ≥11px — a cor é `--trl-fg-muted` (AA) e a informação completa do slot deve estar disponível via tooltip/aria no consumidor.

### R6. Tipografia em px (decisão registrada)

A escala tipográfica usa **px por decisão**: ferramenta densa desktop, o mecanismo de ampliação suportado é o **zoom do browser** (funciona em tudo); a preferência de tamanho de fonte do SO não escala a UI. Migração para `rem` é candidata a v2 (MAJOR) — não "corrigir" pontualmente.

### R7. Densidade

- Alturas de controle: 30px (padrão) / 24px (`--sm`). Linha de tabela ~36px.
- Paddings internos: `space-2`/`space-3`. Gutters de página: `space-6`.
- Editores são ferramentas de dados: densidade é feature, espaçamento generoso é para o showcase/hero, não para o CRUD.

### R8. I18n (PT-BR/EN)

- **Orçamento de expansão: PT ≈ +30% sobre EN.** Todo rótulo com `white-space: nowrap` tem contenção no DS (ellipsis) — mas telas novas devem ser **testadas com strings PT reais**, não lorem/EN.
- Números, moeda (₽/$/€) e datas: formatação com o locale do jogo é responsabilidade do consumidor; o DS garante alinhamento (`tabular-nums`).
- Sem texto em imagem; uppercase CSS preserva acentos.
- Fora de escopo por decisão: RTL, framework de pluralização/ICU, tema claro.

### R9. Assets e scripts

- Nenhum recurso externo (CDN, Google Fonts) — editores rodam offline/localhost.
- Se o DS for copiado para o `wwwroot` de um mod: **nenhum `.js` solto** (o ModValidator do SPT rejeita) — scripts ficam inline no HTML.

## Parte 2 — Receitas

### Shell de editor completo

Não duplicar markup: partir de [`templates/editor-starter.html`](./templates/editor-starter.html) — topbar com brand+laser, sidebar nav, workspace 3 colunas com screen-bars, form, kv, progress e mount de toasts, tudo funcional.

### Screen-bar + estado dirty

O painel que edita dados troca o dot conforme o estado:

```html
<!-- sincronizado -->
<span class="trl-screen-bar__dot trl-screen-bar__dot--ok"></span> … <span class="trl-screen-bar__meta">synced</span>
<!-- com edição pendente (dot vermelho pulsante = momento de marca legítimo) -->
<span class="trl-screen-bar__dot trl-screen-bar__dot--live"></span> … <span class="trl-screen-bar__meta">unsaved changes</span>
```

### Página de formulário

`trl-panel` (**sem `--flush`** — formulário precisa do respiro do `__body`) + `trl-screen-bar` no topo, `trl-form-grid` no body (campos `trl-field` com label/hint/error; `is-invalid` no field marca o input), ações no rodapé alinhadas à direita: ghost à esquerda do primário.

### Página de tabela de dados

`trl-toolbar` (search à esquerda, filtros, tag de status à direita) + `trl-panel--flush` com `trl-table` (thead sticky funciona dentro de container com scroll) + `trl-pagination` no rodapé. Colunas numéricas levam `class="num"`. Linha selecionada: `is-selected`.

### Barra de filtros

`trl-toolbar` com `trl-search` + um `trl-multiselect` por dimensão (trader, categoria…): trigger mostra o rótulo da dimensão + `__count` da seleção; painel fica **aberto entre cliques** (toggle de `is-checked` é JS do consumidor) e fecha no clique fora; `__actions` traz Select all/Clear. Lista longa (categorias, 100+ opções)? `__search` sticky no topo do painel (filtragem é JS do consumidor). Abaixo da toolbar, os filtros aplicados viram uma linha de `trl-filter-chip` (`dimensão: <b>valor</b>` + `__remove`) com um botão ghost "Clear all" no fim — o usuário vê e desfaz o estado do filtro sem abrir dropdown. Para seleção única, use `trl-dropdown`; para booleano, `trl-switch`.

### Dashboard de stats

Grid de `trl-stat` (destaque com `--hi`, indisponível com `--dim` + `trl-badge` explicando por quê), deltas com `trl-chip--up/--down`, barras `trl-progress` com variante de cor por significado — nunca por estética.

### Modal de confirmação

`trl-modal-overlay[hidden]` + `trl-modal` (head com dot de contexto, close ×; actions: ghost "cancelar" + primário ou danger). Overlay fecha no clique fora **e no Esc** (JS do consumidor — ver demo do showcase); `hidden` controla visibilidade. Destrutivo = botão danger + verbo explícito ("Discard", "Delete"), nunca "OK". Painel que hospeda **formulário nunca usa `--flush`** — o respiro vem do padding do `__body` (`--flush` é para listas/tabelas coladas na borda).

### Toast programático

Container fixo `#toasts` (`.trl-toast-container`) já no starter; o app appenda:

```js
function toast(kind, title, msg) {  // kind: ok | warn | error
  const el = document.createElement('div');
  el.className = 'trl-toast trl-toast--' + kind;
  el.innerHTML = '<div class="trl-toast__title"></div><div class="trl-toast__msg"></div>';
  el.children[0].textContent = title;
  el.children[1].textContent = msg;
  document.getElementById('toasts').append(el);
  setTimeout(() => el.remove(), 4000);
}
```

### Receitas game data (bloco H)

O bloco H fornece a **casca visual** dos componentes de domínio Tarkov; todo comportamento (DnD, rotação, validação de colisão, fallback de ícone) é do consumidor — no CustomClasses hoje isso vive em JS interop (`ccGridDnd`) e C# (`CanPlace`).

**Item cell** — dimensiona em unidades de jogo via CSS vars inline:

```html
<div class="trl-cell is-editable" style="--w:2;--h:1">
  <div class="trl-cell__icon"><img src="…" alt=""></div>
  <span class="trl-cell__qty">×30</span>        <!-- stack -->
  <span class="trl-cell__contents">▤4</span>    <!-- container c/ 4 itens -->
</div>
```

Estados: `is-empty` · `is-editable` · `is-invalid` (unresolved) · `is-dragging` (origem esmaecida durante drag). Cadeia de fallback de ícone (preset → tpl → ícone de categoria → `__label` texto) é lógica do consumidor.

**Stash grid 2D** — `--cols` define a largura; itens entram como `.trl-grid2d__item` com `grid-column/grid-row` inline (start/span). Durante o drag: classe `is-dragging` no grid (outline tracejado), hints `.trl-grid2d__hint--ok/--bad` posicionados na mesma grid-area do alvo, e `.trl-grid2d__ghost` (position:fixed) seguindo o cursor via JS.

**Paper doll** — espelho da tela **Gear** do EFT: grade de slots **sobre** a `__silhouette` (SVG decorativa) + `__carry` (coluna de carga separada por hairline: tactical rig, pockets via `__cells`, backpack, pouch). Slots usam áreas nomeadas 1:1 com os equipment slots do jogo: `__slot--earpiece/--headwear/--facecover` (linha 1), `--armband/--bodyarmor/--eyewear` (linha 2), `--onsling` (largo, cruza 2 colunas) `/--holster` (linha 3), `--onback/--scabbard` (linha 4). Células escalam em `cqw` (7cqw/unidade — o doll acompanha a coluna). `is-required-missing` marca slot obrigatório vazio. Slot custom ou posição pixel-accurate: `style="position:absolute;left…"`.

**Mod tree** — linhas recursivas com `__slot` (id mono lowercase: `mod_magazine`), `__name`, badges reutilizados (`trl-badge--green` required, `--red` unknown slot) e `__actions` que aparecem no hover. Aninhamento via `__children` (indent + hairline, mesmo pattern do `.trl-tree`).

**Heatmap** — buckets monocromáticos tan em células de tabela: `trl-heat--none/low/mid/high` (0 / 1–3 / 4–6 / 7+). Nunca usar cores de status para heat — heat é intensidade, não semântica. **A célula sempre exibe o valor** — cor nunca é o único encoding (mesma regra dos chips ±%).

**Categorias de skill** (Physical/Mental/Combat/Practical/Elite) — mapear para as cores de status existentes no consumidor (ex.: Physical=green, Practical=amber, Combat=red-soft, Mental=blue-400, Elite=tan). Não criar novas hues.

**Facções** — `trl-card--usec` / `trl-card--bear` (tokens `--trl-faction-*`). Azul `--trl-blue-400` existe para facção USEC e usos informacionais — não vira accent de UI.

**Fullscreen** — `.trl-fullscreen` ( `hidden` controla) + `.trl-fullscreen__exit`; usado para maximizar matrizes/tabelas densas.

### Gráficos (dataviz)

Cores de UI **reprovam** como cores de série (validado por script — banda de luminosidade, chroma, CVD). Usar os slots `--trl-viz-*` do tokens.css:

- **Categórica** (identidade): `viz-cat-1..4` em **ordem fixa** — nunca ciclar, nunca repintar séries sobreviventes ao filtrar; 5ª série vira "Other" (`viz-neutral`), small multiples ou encoding composto.
- **Sequencial** (magnitude): `viz-seq-1..4` (ramp tan 600→300; 700/800 reprovam o piso de contraste 2:1 — não estender).
- **Divergente** (polaridade): `viz-cat-3` ↔ `viz-neutral` ↔ `viz-cat-4` (midpoint neutro por design).
- **Status é reservado** (success/warning/danger) — nunca vira série.
- **Nunca dual-axis** (2 escalas Y): duas medidas = dois gráficos ou indexação a base comum. ≥2 séries = legenda sempre; texto (valores/labels) usa tokens de texto, nunca a cor da série. Heat/chips sempre com valor visível.
- Paleta nova ou mudança nos slots: **rodar o validador** da skill `dataviz` contra `--trl-surface-1` (procedimento na skill do repo `trl-ds-validation`) — nunca aprovar no olho.

### Consumo em Blazor/MudBlazor (fase futura)

O adapter oficial (`bridge/trl-mudblazor.css`, mapeando `--mud-palette-*` → tokens TRL) **não faz parte do v1** — será criado na refatoração do CustomClasses. Até lá: páginas Blazor podem usar componentes `.trl-*` em ilhas com `class="trl-app"` no wrapper, sem conflito de namespace com o Mud.

## Histórico de Alterações

| Data | Autor | Alteração |
|---|---|---|
| 2026-07-03 | Guilherme | Criação (v1.0.0) — regras R1–R7 + receitas iniciais |
| 2026-07-03 | Guilherme | Recalibração "grafite + chrome neutro": base neutra, edges neutras (+`--trl-edge-accent`), labels via `--trl-fg-label`; R2/R4/R5 atualizadas |
| 2026-07-03 | Guilherme | Review por lentes: R6 (px por decisão) e R8 (i18n) inseridas — Densidade→R7, Assets→R9; receita "Gráficos (dataviz)" com tokens `--trl-viz-*` validados; heat exige valor visível |
