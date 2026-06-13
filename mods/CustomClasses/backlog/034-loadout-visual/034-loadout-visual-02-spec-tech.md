# 034 — Loadout visual · Especificação técnica

**Mod:** CustomClasses
**Status:** Backlog
**Criado:** 2026-06-12
**Spec funcional:** [034-loadout-visual-01-spec.md](./034-loadout-visual-01-spec.md)

## Contexto técnico (código real lido)

- **`ClassDetail.razor`** (item 033) já é o dashboard de 2 colunas. A coluna direita tem **dois pontos de extensão marcados** com contrato congelado:
  - `:218-219` — `EXTENSION POINT 034`, bloco `#cc-equipped`: trocar o conteúdo textual por `<GearPanel Equipped="@def.Loadout?.Equipped"/>`. Contrato: `def.Loadout?.Equipped` é `Dictionary<string, ItemSpec>?` (slot → spec).
  - `:238-239` — `EXTENSION POINT 034`, bloco `#cc-stash`: trocar a `MudTable` por `<StashPanel Lines="@_stashLines"/>`. Contrato: `_stashLines` é `List<LoadoutCostEntry>` (já filtrado `Context=="stash"` em `Reload`).
- **`ItemSpec`** (`ClassDefinition.cs:91-101`): `Tpl` (string?), `Preset` (string? — preset id OU tpl de arma), `Premium` (bool), `Count` (int, default 1), `Ammo`, `LoadedMag`, `Chambered`, `Mods`, `Contents`.
- **`LoadoutCostEntry`** (`CostService.cs:34-47`): `Tpl` (string), `Name` (string), `Context`, `Qty` (double), `UnitPrice` (double), `PriceSource` (string), `Subtotal` (double), `MissingPrice` (bool). **Não tem CategoryId nem dimensões** — o `StashPanel` resolve isso por tpl via `CatalogService`.
- **`CatalogService`** (singleton, `[Injectable]`): `GetItemName(MongoId, lang)`, `GetPrice(MongoId)→(double,string)`, `GetCategories(lang)→List<CatalogCategory>`, `Search(...)→List<CatalogItem>` (CatalogItem tem `CategoryId`). Índices lazy: `_handbookIndex` (`Dictionary<MongoId,string>` tpl→categoria id). **Não expõe**: dimensões do item nem resolução direta tpl→categoria id/nome.
- **Resolução de preset/tpl→item-raiz**: `ClassViewItemSpec.razor:97-125` faz preset > tpl via `Catalog.ResolveDefaultPreset/ResolvePremiumPreset` (internal) e `preset.Items.First().Template`. `GetItemDimensions` deve aceitar um tpl já resolvido — quem resolve o item-raiz de um `ItemSpec` é o componente (mesma ordem do `ClassViewItemSpec`).
- **TemplateItem.Properties** (`TemplateItem.cs:123-128`): `Width` e `Height` são `int?` (em células). Lidos via `itemHelper.GetItem(tpl)` → `.Value.Properties` (mesmo caminho do `GetTemplate` privado existente no CatalogService).
- **Ícone tarkov.dev**: `https://assets.tarkov.dev/{tpl}-icon.webp`, `onerror="this.style.display='none'"`, `loading="lazy"` — padrão de `ItemPicker.razor:69` e `ItemSpecEditor.razor:43`.
- **`ClassEdit.razor`**: aba Stash em `:431-475` (`@for` sobre `_model.Stash`, um `MudPaper` por linha com `ItemSpecEditor`). `_model.Stash` é `List<ItemSpec>` (ver `ClassEditModel.cs`). `ScheduleRecompute`/`_activePanelIndex` (037) — o filtro/agrupamento **não** deve chamar recompute.

## Arquivos a CRIAR / MODIFICAR

| Ação | Arquivo | Conteúdo / contrato | Refs reais |
|---|---|---|---|
| CRIAR | `Web/Shared/GearPanel.razor` | Painel de slots do equipado. Param: `[Parameter] Dictionary<string, ItemSpec>? Equipped`. Renderiza só slots presentes (PA-034-01). Por slot: célula `cc-gear-slot` com label, ícone dimensionado, nome curto, `ItemTooltip`. | contrato `ClassDetail.razor:219`; layout `profiles.css:256-340` |
| CRIAR | `Web/Shared/StashPanel.razor` | Painel do stash agrupado. Param: `[Parameter] List<LoadoutCostEntry> Lines`. Agrupa por categoria (via `Catalog`), grid de ícones com badge qty + subtotal ₽ por grupo + `ItemTooltip`. | contrato `ClassDetail.razor:239`; `CostService.cs:34` |
| CRIAR | `Web/Shared/ItemTooltip.razor` | Conteúdo de tooltip reutilizável. Params: tpl, nome, categoria, `Width`, `Height`, preço, fonte, qty, missingPrice. Renderiza dentro de `MudTooltip` (`ChildContent` = a célula). | `MudTooltip` (usado em `ClassDetail.razor:133`) |
| MODIFICAR | `Web/Pages/ClassDetail.razor` | Trocar **só** o conteúdo dos blocos `#cc-equipped` (`:220-236`) e `#cc-stash` (`:240-278`) pelos componentes novos. `_stashLines`/`_loadoutCost`/`def` inalterados. `MissingPriceBadge` pode migrar para o `StashPanel` (ou ser mantido). | pontos de extensão `:218`/`:238` |
| MODIFICAR | `Web/Pages/ClassEdit.razor` | Aba Stash (`:431-475`): adicionar campo de filtro por nome + agrupar os cards por categoria. **Opera sobre `List<ItemSpecModel>` `_model.Stash`** (não `LoadoutCostEntry` — CR-034-01): resolve nome+categoria por linha via `Catalog.GetItemName(rootTpl)`/`GetCategoryId(rootTpl)` (rootTpl = preset>tpl). Não reordenar `_model.Stash`; não chamar recompute no filtro. | aba `:431-475`; `_model.Stash` (`ClassEditModel.cs`, `ItemSpecModel`) |
| MODIFICAR | `Server/CatalogService.cs` | Adicionar `GetItemDimensions(string tpl)→(int W, int H)` (lê `_props.Width/Height`, default 1×1) e `GetCategoryName(string tpl, lang)→string?` (tpl→`_handbookIndex`→nome via `GetCategories`/locale). Corrigir o comentário do método existente se necessário; **não** alterar índices existentes. | `GetTemplate` `:801`; `_handbookIndex` `:106`; `GetCategories` `:381` |
| MODIFICAR | `Web/wwwroot/css/customclasses.css` | **Adicionar** classes `cc-gear-*`, `cc-stash-*`, `cc-item-cell*`. Não remover/redefinir classes do 033. | arquivo do 033 (47 linhas) |

## Assinaturas novas (CatalogService)

```csharp
/// <summary>
///     Item dimensions in inventory cells (_props.Width × _props.Height). Unknown/malformed tpl or
///     missing props default to 1×1 (corner case 034 — never zero, never throws). Read-only over the
///     live DB via the same GetTemplate path as the rest of the service.
/// </summary>
public (int Width, int Height) GetItemDimensions(string tpl)
{
    var id = TryParseMongoId(tpl);                       // existing private helper :786
    var props = id is null ? null : GetTemplate(id.Value)?.Properties;   // :801
    var w = props?.Width ?? 1;
    var h = props?.Height ?? 1;
    return (w > 0 ? w : 1, h > 0 ? h : 1);
}

/// <summary>
///     Localized handbook category NAME for a tpl, or null when the tpl is not in the handbook
///     (corner case 034 — caller falls back to an "Other" group). Reuses the lazy _handbookIndex
///     (tpl → category id) + GetCategories (id → localized name). lang: "en" | "pt".
/// </summary>
public string? GetCategoryName(string tpl, string lang = "en")
{
    var id = TryParseMongoId(tpl);
    if (id is null || !_handbookIndex.Value.TryGetValue(id.Value, out var catId))
    {
        return null;
    }
    // GetCategories builds a List once per call; the panel calls this per line, so the StashPanel
    // builds an id→name map ONCE from GetCategories and resolves locally (see StashPanel grouping
    // note) — GetCategoryName stays the single-tpl convenience used by GearPanel/ItemTooltip.
    var cats = GetCategories(lang);
    return cats.FirstOrDefault(c => string.Equals(c.Id, catId, StringComparison.Ordinal))?.Name;
}

/// <summary>tpl → handbook category id (null when absent). Cheap O(1) over the lazy index.</summary>
public string? GetCategoryId(string tpl)
{
    var id = TryParseMongoId(tpl);
    return id is not null && _handbookIndex.Value.TryGetValue(id.Value, out var catId) ? catId : null;
}
```

> **Decisão de performance (037-aware):** `GetCategories()` reconstrói a lista a cada chamada. O `StashPanel` agrupa N linhas — chamar `GetCategoryName` por linha custaria N×O(cats). Para não regredir o 037, o `StashPanel` chama `Catalog.GetCategories()` **uma vez** no `OnParametersSet`, monta um `Dictionary<string,string>` (id→nome) local, e usa `GetCategoryId(tpl)` (O(1)) por linha. `GetCategoryName` fica como conveniência single-tpl para o `GearPanel`/`ItemTooltip` (poucas chamadas). Registrado em CR-034-03.

## Contrato dos componentes

### GearPanel.razor
```csharp
[Parameter] public Dictionary<string, ItemSpec>? Equipped { get; set; }
```
- `OnParametersSet`: se `Equipped` nulo/vazio → renderiza `<MudText Typo="Caption">No equipped items.</MudText>` (mantém a msg do 033).
- Por `(slot, spec)`: resolve o **tpl do item-raiz** com a MESMA ordem do `ClassViewItemSpec` (preset > tpl). Para preset usa `Catalog.ResolveDefaultPreset/ResolvePremiumPreset` (internal — `GearPanel` está no mesmo assembly `CustomClasses`, ok) → `preset.Items.First().Template`. Dimensões via `Catalog.GetItemDimensions(rootTpl)`; nome via `Catalog.GetItemName`; preço via `Catalog.GetPrice`.
- Célula: `div.cc-item-cell` com `width/height` = `W*UNIT`/`H*UNIT` (UNIT = var CSS, ex. 36px) via `style`. Dentro: `<img>` tarkov.dev (`{rootTpl}-icon.webp`, `onerror` esconde). O `<div class="cc-gear-slot__name">{shortName}</div>` é renderizado **sempre** como label sob a célula (CR-034-05), com ellipsis — quando a img carrega, vê-se ícone + label; offline, só o label. Sem JS para detectar `onerror`. Tooltip envolve a célula.

> **Editor (CR-034-01):** distinto do read-only acima — a aba Stash do `ClassEdit` agrupa/filtra `List<ItemSpecModel>` `_model.Stash`. Por linha, resolve o rootTpl (preset>tpl) e usa `Catalog.GetItemName`/`GetCategoryId`. O filtro casa contra nome/shortname/tpl resolvidos. Não há `LoadoutCostEntry` nesse caminho; o agrupamento é só de exibição (ordem de `_model.Stash` preservada) e não chama `ScheduleRecompute`.

### StashPanel.razor
```csharp
[Parameter, EditorRequired] public List<LoadoutCostEntry> Lines { get; set; } = [];
```
- `OnParametersSet`: monta `_catNames` (id→nome) de `Catalog.GetCategories()` uma vez; agrupa `Lines` por `Catalog.GetCategoryId(line.Tpl)` (null → grupo `"Other"`); ordena grupos por nome; calcula subtotal ₽ por grupo (`Σ line.Subtotal`).
- Por grupo: header `cc-stash-group__title` (nome + subtotal). Grid de células: ícone dimensionado (`GetItemDimensions(line.Tpl)`), badge `cc-item-cell__qty` quando `line.Qty > 1`, badge ⚠ quando `line.MissingPrice`, tooltip.
- Warnings (`_loadoutCost.Warnings`) e o aviso "stash sem linha precificada" **permanecem no `ClassDetail`** (acima do `<StashPanel>`), porque dependem de `_loadoutCost`/`def` que o painel não recebe. O painel só renderiza `Lines`.

### ItemTooltip.razor
```csharp
[Parameter, EditorRequired] public RenderFragment ChildContent { get; set; } = default!; // a célula
[Parameter] public string Name { get; set; } = "";
[Parameter] public string? Category { get; set; }
[Parameter] public int Width { get; set; } = 1;
[Parameter] public int Height { get; set; } = 1;
[Parameter] public double Price { get; set; }
[Parameter] public string? PriceSource { get; set; }
[Parameter] public double? Qty { get; set; }
[Parameter] public bool MissingPrice { get; set; }
```
- Renderiza `<MudTooltip>` com `TooltipContent` = bloco formatado (nome em negrito, categoria, `W×H`, preço `N0 ₽` + fonte ou "⚠ no price", qty quando `Qty>1`) e `ChildContent` = a célula passada.

## CSS a adicionar (nomes estáveis, prefixo cc-)

```css
/* ── Item 034: gear/stash visual cells ─────────────────────────────────────── */
.cc-item-cell        { /* base cell: relative, bg grid, border, --cc-cell-unit:36px */ }
.cc-item-cell img    { position:absolute; inset:0; width:100%; height:100%; object-fit:contain; }
.cc-item-cell__name  { /* visible fallback name, ellipsis, 9px */ }
.cc-item-cell__qty   { /* corner qty badge */ }
.cc-item-cell__warn  { /* corner ⚠ missing-price badge */ }

.cc-gear-slots       { display:flex; flex-wrap:wrap; gap:12px; }
.cc-gear-slot        { display:flex; flex-direction:column; align-items:center; gap:4px; }
.cc-gear-slot__label { font-size:9px; text-transform:uppercase; opacity:.7; }
.cc-gear-slot--empty { /* reserved for a future fixed grid (PA-034-01) — unused in v1 */ }

.cc-stash-group        { margin-bottom:12px; }
.cc-stash-group__title { /* category name + subtotal, like cc-section__title */ }
.cc-stash-grid         { display:flex; flex-wrap:wrap; gap:8px; }
```
> Tokens do viewer (`--accent`, `--border-subtle`, etc.) **não existem** no contexto MudBlazor desta app — usar variáveis MudBlazor (`var(--mud-palette-*)`) ou valores literais, como o 033 já fez. Não copiar `var(--space-*)` cegamente. Registrado em CR-034-04.

## Pontos de atenção (csharp/spt best-practices)

- **Sem novo índice / sem mutação de DB:** os getters novos são read-only sobre a DB live, reusando `GetTemplate` e `_handbookIndex` (037). Não criar `Lazy<T>` novo, não eager no ctor (premissa 037).
- **Concorrência (037 PA-037-03):** `_handbookIndex`/`GetCategories` já são thread-safe; os getters novos só leem. Os componentes Blazor são por-circuito — sem estado estático mutável.
- **`internal` ResolveDefaultPreset/ResolvePremiumPreset:** `GearPanel` está no assembly `CustomClasses` → acesso `internal` ok (mesmo que `ClassViewItemSpec` já faz).
- **Degradação offline:** nenhuma chamada de rede no servidor; o ícone é um `<img>` client-side com `onerror`. Tooltip/nome/preço vêm do servidor.
- **Não tocar `ItemSpecEditor`, `CostService`, índices do 037, header do 033/036.**

## Plano de verificação

1. `/compile-mod CustomClasses` — compila client+server (cuidado com acoplamento, ver memory `project_customclasses_session_split`).
2. Servir o editor (`/serve-inventory` ou host do mod), abrir `ClassDetail` de uma classe com equipado + stash variados: conferir ícones dimensionados, agrupamento, tooltip no hover, subtotais batendo com o 033.
3. Simular offline (DevTools block `assets.tarkov.dev`): confirmar degradação para nome+qty sem quebra de layout.
4. Aba Stash do editor: filtrar por nome, conferir que grupos somem/voltam e que `_model.Stash` não reordena; digitar no filtro não dispara recompute (log 037 silencioso).
5. Single-screen 1080p: classe completa com ≤1 scroll.

## Histórico

| Data | Evento |
|---|---|
| 2026-06-12 | Spec técnica criada via `/create-technical-spec` (autônoma); getters de dimensão/categoria definidos sobre o CatalogService do 037 |
| 2026-06-12 | Auto-review `/review-technical-spec` aplicado — ver 03 (CR-034-01..05) |
