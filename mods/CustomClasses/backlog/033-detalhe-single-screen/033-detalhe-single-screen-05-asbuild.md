# 033 — Detalhe single-screen (dashboard) — As-build

**Mod:** CustomClasses
**Data:** 2026-06-12
**Refs:** [02-spec-tech](./033-detalhe-single-screen-02-spec-tech.md) · [00-kickoff](./033-detalhe-single-screen-00-kickoff.md)

## O que foi implementado

Redesign do `ClassDetail.razor` abandonando `MudExpansionPanels`: dashboard de 2 colunas,
header compacto com badges, densidade 12-14px via nova folha `customclasses.css`. Zero
expansões/cliques para ver skills + multiplicadores + hideout + outfit + equipado + stash + custos.

## Arquivos tocados

| Arquivo | Ação |
|---|---|
| `modded/Server/Web/wwwroot/css/customclasses.css` | **CRIADO** — folha de densidade (árvore `wwwroot/css/` criada do zero) |
| `modded/Server/Web/Layouts/BaseLayout.razor` | **MODIFICADO** — 1 linha `<link>` estático no `<HeadContent>`, após o CSS do MudBlazor |
| `modded/Server/Web/Pages/ClassDetail.razor` | **MODIFICADO** — header + badges + grid 2 colunas; removido o `MudExpansionPanels` inteiro |

## Detalhes por arquivo

### `customclasses.css` (novo)
Porta os tokens do viewer antigo (`profiles.css:72-86,152-214`). Classes estáveis e reusadas pelo 034:
`.cc-dash` / `.cc-dash__left` (320px) / `.cc-dash__right` (flex), `.cc-dense` (+ override de padding de td/th
com `!important`, escopo restrito a `.cc-dense` — PT-2), `.cc-section` / `.cc-section__title`, `.cc-badges` /
`.cc-badge*`, `.cc-desc` (ellipsis 1 linha), `.cc-hideout-grid`, `.cc-equip-slot*`. Servido em
`/CustomClasses-Server/css/customclasses.css` pelo mesmo mount de `wwwroot/` que já serve os ícones.

### `BaseLayout.razor`
Uma linha `<link href="/CustomClasses-Server/css/customclasses.css" rel="stylesheet" />` no `<HeadContent>`,
logo após o link do MudBlazor. Nada mais do que o 030 montou (drawer, appbar, guard, `CascadingValue`) foi tocado.

### `ClassDetail.razor`
- `MudContainer` passou de `MaxWidth.Large` para `MaxWidth.False` (full-width, P4) — coluna direita respira.
- Header `MudStack` (back / ícone / nome colorido / `StatusChip` / Edit / Duplicate / Delete) **intacto** —
  handlers e navegação preservados.
- Nova faixa de badges (`.cc-badges`): Skill cost (+ `SkillTotalChip`), Loadout ₽, Base edition, e
  Icon / Name color condicionais. Descrição EN em 1 linha com `MudTooltip` + ellipsis (`.cc-desc`).
- Grid `.cc-dash .cc-dense`:
  - **Esquerda:** `<SkillCanonicalList ... Editable="false"/>` (a MESMA tag adotada pelo 031, apenas movida —
    componente não foi reescrito) + linha "Weighted/budget" + aviso SE-ausente; hideout como `MudChip`
    (`Station L<n>`); outfit como `MudSimpleTable` densa (`ClothingLabel` intacto).
  - **Direita:** dois `cc-section` nomeados (`id="cc-equipped"`, `id="cc-stash"`), cada um com comentário
    `░░ EXTENSION POINT 034 ░░`. Equipado reusa `ClassViewItemSpec` por slot; stash reusa a `MudTable` de
    `_stashLines` (6 colunas). Warnings de loadout (`_loadoutCost.Warnings`) migraram para cima da tabela de stash.
- Removido o `MudExpansionPanels` inteiro (General / Skills / XP multipliers / Hideout / Outfit / Equipped /
  Stash / Cost summary). Conteúdo redistribuído (comentário in-file documenta o mapeamento).
- `@code` intacto exceto: removido `FactorColor` (única chamadora era a tabela de multiplicadores removida —
  zero callers restantes, conforme spec). `Reload()`, handlers de lifecycle, `_stashLines`, `_loadoutCost`,
  `_clothingNames`, `ClothingLabel`, `FormatRub`, `StatusChip`, `SkillTotalChip`, `MissingPriceBadge` preservados.

## Contrato 033 → 034 entregue

1. Dois blocos isolados `id="cc-equipped"` e `id="cc-stash"` dentro de `.cc-dash__right`, cada um com marcador
   `░░ EXTENSION POINT 034 ░░` indicando a troca por `<GearPanel>` / `<StashPanel>`.
2. Tipos de dados estáveis: `def.Loadout?.Equipped` (`IReadOnlyDictionary<string, ItemSpec>`) e `_stashLines`
   (`List<LoadoutCostEntry>`, já filtrado `Context=="stash"`). Documentados nos comentários dos extension points.
3. CSS de densidade/layout (`.cc-dash*`, `.cc-dense`, `.cc-section*`, `.cc-equip-slot*`) vive em
   `customclasses.css` e é consumido pelo 034 — o 034 só adiciona regras de grid de ícones.

## Premissas registradas (decididas autonomamente)

- **PA-033-A:** Adicionados badges condicionais Icon e Name color ao header (não estavam explícitos na spec, mas
  a spec removeu o painel "General" que os mostrava). Mantém a info visível sem expansão. Cosmético, ajustável no 035.
- **PA-033-B:** `MaxWidth.False` escolhido como "container largo / full-width" (P4 deixou em aberto entre
  largo e full-width). Full-width maximiza espaço da coluna direita textual; reversível.
- **PA-033-C:** `@media (max-width: 960px)` empilha as colunas em telas estreitas (não exigido pela spec, mas
  evita squeeze ilegível). Puramente defensivo.
- O override `!important` de padding ficou escopado a `.cc-dense` (PT-2 da spec) — não vaza para o editor.

## Pendências / fora de escopo

- Single-screen completo (≤1 scroll) é DoD do **034** (grids de ícones compactam equipado/stash). O 033 entrega
  a estrutura 2 colunas + densidade que tornam isso possível; a coluna direita textual ainda pode gerar scroll.
- **Não** foi rodado `dotnet build` (estágio dedicado faz). Validação visual no servidor Blazor pendente.

## Histórico de Alterações

| Data | Autor | Alteração |
|---|---|---|
| 2026-06-12 | Guilherme | As-build do 033 (dashboard 2 colunas, customclasses.css, badges, remoção dos expansion panels). |
