# design-system/ — contexto para agentes

**TRL Design System v1.0.0** — linguagem visual padrão de **todo editor web** de mod deste repo (comunidade Tarkov Red Line · SPT 4.0 · Fika Coop). Dark militar, accent tan/gold de trabalho, vermelho reservado à marca.

## Mapa

| Arquivo | Papel |
|---|---|
| `tokens.css` | 3 camadas: primitivos → **semânticos (consuma estes)** → assinaturas. Namespace `--trl-*` |
| `components.css` | Componentes `.trl-*` (BEM; estados `.is-*`). Blocos: base, shell, nav, forms, data, feedback, overlays, **game data** (cell/grid2d/doll/mod-tree/heat/faction — casca visual; comportamento é do consumidor), motion |
| `utilities.css` | Utilitários mínimos `.trl-u-*` |
| `fonts/bender.css` | `@font-face` Bender (opcional — fallback Bahnschrift). Licença: `fonts/LICENSE-NOTE.txt` |
| `design-system.html` | Showcase navegável — **abra para ver tudo vivo**; é também o teste de integração |
| `templates/editor-starter.html` | Ponto de partida copy-paste de editor novo |
| `PATTERNS.md` | **Regras normativas (R1–R9) + receitas** — leia antes de estilizar qualquer editor |
| `CHANGELOG.md` | Semver: MAJOR quebra token/classe · MINOR componente novo · PATCH ajuste visual |

Ordem de consumo:

```html
<link rel="stylesheet" href="design-system/fonts/bender.css">  <!-- opcional -->
<link rel="stylesheet" href="design-system/tokens.css">
<link rel="stylesheet" href="design-system/components.css">
<link rel="stylesheet" href="design-system/utilities.css">
<style>html, body { margin: 0; height: 100% }</style>  <!-- .trl-shell usa 100vh -->
<body class="trl-app"> … </body>
```

## Regras de ouro (detalhes em PATTERNS.md)

1. Consumir **só tokens semânticos da camada 2** (`--trl-bg-*`, `--trl-fg-*`, `--trl-accent*`, `--trl-danger*`, `--trl-edge*`, `--trl-wash-*`). Hex hardcoded em editor = defeito.
2. **Vermelho é raro** (R1): laser divider, dot live/dirty, botão destrutivo. `#ff0000` nunca em texto/fill. >~5% da tela em vermelho = errado.
3. `--trl-radius: 0`, bordas 1px `--trl-edge*`. Uppercase+tracking só em display/labels.
4. Mudanças no DS são **aditivas**; breaking = MAJOR no CHANGELOG. Componente novo entra no showcase **na mesma mudança**.
5. Estilo novo específico de um mod fica no CSS do mod (consumindo tokens) — só entra aqui o que ≥2 editores usariam.
6. Se copiar esta pasta para `wwwroot` de mod: **nenhum `.js` solto** (ModValidator rejeita) — scripts inline.
7. Ao revisar/refatorar telas, validar com a skill do repo **`trl-ds-validation`** (lentes readability/a11y/i18n/dataviz + paletas de gráfico validadas). Gráficos usam os tokens `--trl-viz-*` — cores de UI reprovam como série.

## Consumidores e fases futuras

- **Editores novos**: partir de `templates/editor-starter.html`.
- **CustomClasses** (Blazor + MudBlazor): refatoração futura via adapter `bridge/trl-mudblazor.css` (mapear `--mud-palette-*` → tokens; **ainda não existe**).
- **trl-items-management viewer**: proto-DS legado (`tools/trl-items-management/viewer/*.css`, sem namespace) — não tocar; migração futura via shim de compat. Não copiar padrões de lá para cá.
