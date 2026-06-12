# 034 — Loadout visual · As-build

**Mod:** CustomClasses
**Status:** Implementado (não compilado — `dotnet build` fora do escopo desta sessão)
**Data:** 2026-06-12
**Spec técnica:** [034-loadout-visual-02-spec-tech.md](./034-loadout-visual-02-spec-tech.md)

## O que foi feito

Painéis visuais de loadout (gear slots + stash em grid de ícones + tooltip de hover) na coluna
direita do dashboard `ClassDetail` (033) e agrupamento/filtro por categoria na aba Stash do editor
`ClassEdit`. Ícones via tarkov.dev (`https://assets.tarkov.dev/{tpl}-icon.webp`), dimensionados pelo
tamanho real do item (`_props.Width/Height`). Degrada para texto offline (sem ícone), sem quebrar layout.

## Arquivos criados

| Arquivo | Conteúdo |
|---|---|
| `Server/Web/Shared/ItemTooltip.razor` | Tooltip reutilizável: envolve uma célula (`ChildContent`) num `MudTooltip` cujo `TooltipContent` mostra nome (negrito), categoria, `W×H cells`, preço `N0 ₽` + fonte (ou `⚠ no price`), e qty quando `>1`. |
| `Server/Web/Shared/GearPanel.razor` | Painel de equipado. Param `Dictionary<string, ItemSpec>? Equipped`. Renderiza só slots presentes (PA-034-01). Por slot: label, célula `cc-item-cell` dimensionada (`GetItemDimensions`), `<img>` tarkov.dev (`onerror` esconde), nome curto sempre visível sob a célula (CR-034-05), `ItemTooltip`. RootTpl = preset(default/premium)>tpl, igual ao `ClassViewItemSpec`. Vazio → "No equipped items." |
| `Server/Web/Shared/StashPanel.razor` | Painel do stash. Param `List<LoadoutCostEntry> Lines`. Agrupa por categoria do handbook; por grupo: header (nome + subtotal ₽) e grid de células com badge de qty (`>1`) e badge `⚠` (missing price). `GetCategories()` chamado UMA vez em `OnParametersSet` (id→nome), agrupamento por `GetCategoryId` O(1) (CR-034-03). |

## Arquivos modificados

| Arquivo | Mudança |
|---|---|
| `Server/CatalogService.cs` | Adicionados 3 getters read-only sobre a DB live / `_handbookIndex` (037), sem novo `Lazy<T>`: `GetItemDimensions(string)→(int,int)` (default 1×1, nunca zero/throw), `GetCategoryId(string)→string?` (O(1)), `GetCategoryName(string,lang)→string?` (id→nome via `GetCategories`). Índices existentes inalterados. |
| `Server/Web/Pages/ClassDetail.razor` | Bloco `#cc-equipped`: lista textual `ClassViewItemSpec` → `<GearPanel Equipped="@def.Loadout?.Equipped"/>`. Bloco `#cc-stash`: `MudTable` → `<StashPanel Lines="@_stashLines"/>`. Warnings de custo e aviso "stash sem linha precificada" permanecem no `ClassDetail` (dependem de `_loadoutCost`/`def`). `_stashLines`/`_loadoutCost`/`def`/`Reload` inalterados. `MissingPriceBadge` mantido (não mais referenciado — dead code inócuo, sem warning-as-error no csproj). |
| `Server/Web/Pages/ClassEdit.razor` | Aba Stash: campo de filtro (`MudTextField`, nome/shortname/tpl) + cards agrupados por categoria. Opera sobre `List<ItemSpecModel>` `_model.Stash` (CR-034-01). Helpers novos: `BuildStashGroups()` (bucketiza preservando ordem; #N = índice original 1-based estável), `StashRootTpl()` (preset>tpl), `StashFilterChanged()` (só `StateHasChanged()`, NUNCA recompute — PA-037-04). Adicionado `@using SPTarkov.Server.Core.Models.Common` (MongoId). |
| `Server/Web/wwwroot/css/customclasses.css` | ADICIONADAS classes `cc-item-cell*`, `cc-gear-*`, `cc-stash-*`, `cc-item-tip*`. Unidade da célula via `--cc-cell-unit:36px`. Classes do 033 não removidas/redefinidas (comentário da `cc-equip-slot` ajustado de "extension point" para uso textual residual). |

## Premissas registradas (decididas autonomamente)

- **PA-034-A — texto in-cell do stash atrás do ícone:** no `StashPanel` o `<span cc-item-cell__txt>`
  vem ANTES do `<img>` no DOM (mesmo stacking context, ambos `position:absolute`), então o ícone
  carregado pinta por cima; offline (`onerror` esconde o img) o texto fica visível. No `GearPanel`
  o nome fica num label SOB a célula (`cc-item-cell__name`), sempre visível (CR-034-05) — não há
  texto dentro da célula do gear.
- **PA-034-B — fonte do "missing price":** célula/tooltip marcam missing quando `GetPrice` retorna
  source `"missing"` (gear) ou `LoadoutCostEntry.MissingPrice` (stash). Consistente com o 022/033.
- **PA-034-C — categoria no editor:** usa `GetCategoryName` per-line (poucas linhas no editor, ~27),
  diferente do read-only `StashPanel` que usa o mapa local (037-aware). Aceito: o editor não é hot path.
- **PA-034-D — `--cc-cell-unit` 36px** como no spec; cores via `var(--mud-palette-*)` com fallback
  literal (tokens `--accent`/`--space-*` do viewer não existem neste contexto MudBlazor — CR-034-04).
- **PA-034-E — slots vazios:** v1 não desenha grid fixo de slots vazios (PA-034-01); `cc-gear-slot--empty`
  fica reservada/sem uso para um grid futuro.

## Não verificado nesta sessão

- `dotnet build`/`/compile-mod` (fora do escopo do prompt). Compilação client+server acoplada — ver
  memory `project_customclasses_session_split` antes de compilar.
- Render real no jogo / offline (DevTools block tarkov.dev) e meta single-screen 1080p — plano de
  verificação §1-5 da spec técnica pendente.

## Histórico

| Data | Evento |
|---|---|
| 2026-06-12 | Implementação autônoma dos 3 componentes + getters do CatalogService + adoção no ClassDetail/ClassEdit + CSS. Sem compilação. |
