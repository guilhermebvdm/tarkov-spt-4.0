# 032 — Matriz de skills (classes × skills, heatmap) — Spec técnica

**Mod:** CustomClasses
**Criado:** 2026-06-12
**Refs:** [01-spec](./032-matriz-skills-01-spec.md) · [00-kickoff](./032-matriz-skills-00-kickoff.md)

## Arquivos tocados

| Arquivo | Ação |
|---|---|
| `modded/Server/Web/Pages/SkillsMatrix.razor` | CRIADO — página `@page "/customclasses/skills"`, matriz heatmap skills×classes + rodapé de custo + 2 toggles |
| `modded/Server/Web/Shared/NavMenu.razor` | EDITADO — 1 `MudNavLink` "Skills matrix" → `/customclasses/skills`; nada mais na sidebar muda |

**Não tocados (só consumidos):** `SkillMaster.cs` (ordem/cores/labels das linhas), `ClassEditorService.cs` (`GetCachedEntries`), `CostService.cs` (`ComputeSkillCost`), `ClassDefinition.cs` (`Skills`, `SkillMultipliers`, `NameColor`, `IconFile`, `DisplayName`), `SkillCanonicalList.razor` (referência de chip/tier, não reusado como componente — ver decisão D2).

## Contratos reusados (assinaturas reais)

- **Ordem/identidade das linhas** — `SkillMaster` (`modded/Server/SkillMaster.cs`):
  - `public static IReadOnlyList<SkillMasterEntry> Entries { get; }` (`SkillMaster.cs:65`) — Ph→M→C→P→SpecialElite, derivada, sem hardcode.
  - `public sealed record SkillMasterEntry(SkillTypes Skill, string Name, SkillCategory Category)` (`SkillMaster.cs:16`).
  - `public static string ColorOf(SkillCategory category)` (`SkillMaster.cs:68`) e `LabelOf` (`SkillMaster.cs:79`) — cor/rótulo do separador de categoria.
- **Fonte das colunas** — `ClassEditorService` (`modded/Server/ClassEditorService.cs`):
  - `public IReadOnlyList<ClassFileEntry> GetCachedEntries()` (`ClassEditorService.cs:182`) — view de cache 037, **sem dry-run pesado** (faz scan de dir + CR-EP-06; chamar **1× por navegação**).
  - `public sealed record ClassFileEntry(string FileName, ClassDefinition? Definition, bool Enabled, bool Registered, List<ClassDiagnostic> Diagnostics)` (`ClassEditorService.cs:21`).
- **Custo do rodapé** — `CostService` (`modded/Server/CostService.cs`):
  - `public SkillCostBreakdown ComputeSkillCost(ClassDefinition def)` (`CostService.cs:106`) — barato (sem rebuild de loadout); chamar 1× por classe em `OnInitialized`.
  - `SkillCostBreakdown { double Total; bool WithinBudget; List<SkillCostEntry> Skills; List<string> Warnings }` (`CostService.cs:24`).
- **Dados da classe** — `ClassDefinition` (`modded/Server/ClassDefinition.cs`):
  - `Dictionary<string,int>? Skills` (`:45`), `Dictionary<string,double>? SkillMultipliers` (`:49`), `string? NameColor` (`:41`), `string? IconFile` (`:37`), `LocalizedText? DisplayName` (`:21`), `string? Name` (`:14`).
- **Padrão de consumo (a copiar)** — `NavMenu.razor:194-230` (`LoadRows()`: 1 `ComputeSkillCost` por classe, ícone via `$"/CustomClasses-Server/icons/{icon}"` `:220`, `NameStyle` `:287`), e `Classes.razor:1-2` (`@page`/`@layout BaseLayout`).

## Estrutura da página `SkillsMatrix.razor`

### Cabeçalho

```razor
@page "/customclasses/skills"
@layout BaseLayout

@using System.Globalization
@using CustomClasses.Web
@inject ClassEditorService EditorService
@inject CostService CostService
@inject NavigationManager Nav

<PageTitle>CustomClasses — Skills matrix</PageTitle>
```

### View model (projeção 1× por navegação)

A página projeta uma `ClassColumn` por entry de cache em `OnInitialized` — **uma** chamada `GetCachedEntries()` e **uma** `ComputeSkillCost` por classe (espelho exato de `NavMenu.LoadRows`, `NavMenu.razor:194-230`):

```csharp
private sealed record ClassColumn(
    string BareName,            // Path.GetFileNameWithoutExtension(entry.FileName) — rota destino
    string DisplayName,         // def?.Name ?? entry.FileName
    string? NameColor,          // def?.NameColor
    string? IconUrl,            // def?.IconFile -> $"/CustomClasses-Server/icons/{icon}"
    bool Enabled,
    bool HasDefinition,         // def is not null
    IReadOnlyDictionary<string,int> Levels,         // case-insensitive; def?.Skills ?? empty
    IReadOnlyDictionary<string,double> Multipliers, // case-insensitive; def?.SkillMultipliers ?? empty
    double SkillCostTotal,      // ComputeSkillCost(def).Total ; 0 quando def null
    bool WithinBudget,          // ComputeSkillCost(def).WithinBudget ; false quando def null
    bool HasCost);              // def != null && total > 0  (controla "—" vs valor no rodapé)

private List<ClassColumn> _columns = [];
private List<string> _overflowSkills = [];   // skills definidas por alguma classe e fora de SkillMaster.Entries
private bool _showDisabled = true;            // toggle "Mostrar desabilitadas" (default on)
private bool _showMultipliers = false;        // toggle "Multiplicadores XP" (default off)

protected override void OnInitialized() => LoadColumns();
```

`LoadColumns()`:
1. `foreach (var entry in EditorService.GetCachedEntries())` — projeta uma `ClassColumn`.
2. Dicionários de níveis/multiplicadores construídos **case-insensitive** (`StringComparer.OrdinalIgnoreCase`) — mesmo motivo do 031 (`SkillCanonicalList.razor:116-141`): "endurance" casa "Endurance", chave faltante → 0, nunca `KeyNotFoundException`.
3. Custo: `var cost = def is null ? null : CostService.ComputeSkillCost(def); total = cost?.Total ?? 0; within = cost?.WithinBudget ?? false; hasCost = cost is not null && total > 0;` (espelha `NavMenu.razor:203-205`).
4. Overflow: união (case-insensitive) das chaves de `Levels` de todas as classes que **não** estão em `SkillMaster.Entries` (set via `SkillMaster.Entries.Select(e => e.Name)`), preservando ordem de primeira aparição (espelha `SkillCanonicalList.BuildOverflowEntries`, `:163-191`).

`VisibleColumns()` (chamado no render): `_showDisabled ? _columns : _columns.Where(c => c.Enabled).ToList()` — ordem preservada (P5).

### Render — tabela heatmap

`<table class="cc-skill-matrix">` dentro de `<div class="cc-matrix-wrap">` (scroll horizontal). Estrutura portada de `profiles-skills.js:78-92` + CSS de `profiles-skills.css:25-127`:

- **thead:** 1ª célula vazia (canto, `cc-skill-name-header`); depois 1 `<th class="cc-skill-col-header @(col.Enabled ? null : "cc-cell--disabled")" @onclick="@(() => NavigateTo(col))">` por coluna visível. O `@onclick` fica no `<th>` inteiro (cobre ícone + nome). **R2** — ícone e nome são elementos SEPARADOS: o ícone vai numa `<div class="cc-col-icon">` (`writing-mode: horizontal-tb`, NÃO herda a rotação), e só o nome vai num `<div class="cc-skill-col-header__name" style="@NameStyle(col)">` com `writing-mode: vertical-rl` (CSS `:43-54`). Pôr `<img>` dentro do bloco rotacionado o deixaria deitado/de cabeça para baixo.
- **tbody:** para cada `entry` de `SkillMaster.Entries`, emitir um separador quando a categoria muda (espelha `SkillCanonicalList.razor:36-46`), com `<td colspan="@ColumnSpan" ...>` — **R1**, colspan DINÂMICO; depois uma `<tr class="cc-skill-row">` com:
  - `<td class="cc-skill-name">@entry.Name</td>`
  - por coluna visível: `@Cell(col, entry.Name)`.
- **Seção overflow** (se `_overflowSkills` não vazio): separador "Outside canonical (loader ignores / unmapped)" com `colspan="@ColumnSpan"` (espelha `SkillCanonicalList.razor:292-301`) + 1 linha por skill de overflow.
- **tfoot (rodapé de custo):** `<td class="cc-skill-name">Skill cost</td>` + por coluna: `@CostFooterCell(col)`.

**R1 — colspan dinâmico:** `private int ColumnSpan => 1 + VisibleColumns().Count;` (coluna de nomes + colunas visíveis), recomputado por render — igual ao `ColumnCount` de `SkillCanonicalList.razor:413`. Garante separador alinhado quando o toggle "Mostrar desabilitadas" muda o número de colunas.

**R3 — esmaecimento POR CÉLULA, não por coluna:** CSS não seleciona "a N-ésima célula de cada linha por estado de dado", e `nth-child` quebra quando o toggle filtra colunas. Cada `Cell`/`CostFooterCell`/header recebe a classe `cc-cell--disabled` quando `!col.Enabled`, avaliada no laço de render a partir do dado (`col.Enabled`). Robusto à filtragem/reordenação.

### Fragmentos

```csharp
// Célula de nível — heatmap por tier (porta de profiles-skills.js:60-68)
private RenderFragment Cell(ClassColumn col, string skillName) => __builder => { ... }
//   level = col.Levels.TryGetValue(skillName, out var l) ? l : 0;
//   level <= 0  -> <td class="cc-skill-cell cc-skill-cell--empty" @onclick=NavigateTo(col)></td>
//                  (clicável mesmo vazia — navega para a classe; chip de multiplicador pode aparecer, CC8)
//   tier = TierOf(level); <td class="cc-skill-cell cc-skill-cell--{tier}" @onclick=...>
//            <span class="cc-skill-cell__val">@level</span> @if(_showMultipliers) @MultiplierChip(...)
private static string TierOf(int level) => level <= 3 ? "low" : level <= 6 ? "mid" : "high"; // js:64

private RenderFragment MultiplierChip(double factor) // copia a lógica de SkillCanonicalList.razor:374-393
//   |factor-1| < 0.0001 -> nada ; factor>1 -> MudChip Success "+N%" ; senão MudChip Error "−N%"

private RenderFragment CostFooterCell(ClassColumn col) => __builder => { ... }
//   !col.HasCost -> "—" ; senão @col.SkillCostTotal.ToString("0", InvariantCulture)
//   com classe cc-cost--ok quando col.WithinBudget, cc-cost--over caso contrário

private void NavigateTo(ClassColumn col) =>
    Nav.NavigateTo($"/customclasses/classes/{Uri.EscapeDataString(col.BareName)}");   // P7, detalhe

private static string? NameStyle(ClassColumn col) =>
    string.IsNullOrWhiteSpace(col.NameColor) ? null : $"color:{col.NameColor};";       // NavMenu.razor:287
```

`MultiplierOf` lookup: case-insensitive sobre `col.Multipliers` (mesma técnica de `SkillCanonicalList.razor:257-273`) — nunca indexar direto.

### Toggles

```razor
<MudStack Row="true" AlignItems="AlignItems.Center" Spacing="4" Class="mb-3">
    <MudSwitch T="bool" @bind-Value="_showDisabled" Color="Color.Default" Label="Mostrar desabilitadas"/>
    <MudSwitch T="bool" @bind-Value="_showMultipliers" Color="Color.Primary" Label="Multiplicadores XP"/>
</MudStack>
```

Alternar um toggle só re-renderiza (Blazor) — **não** chama `LoadColumns()` (zero recomputo de custo, CA7). `VisibleColumns()` e a presença do chip leem o estado em tempo de render.

### Vazio

`@if (_columns.Count == 0) { <MudAlert Severity="Severity.Info" Dense="true">No class files found in config/classes/.</MudAlert> }` antes da tabela (corner case 1).

### CSS scoped

Bloco `<style>` na própria página (mesmo padrão do `NavMenu.razor:89-144`), portando `profiles-skills.css`:
- `.cc-matrix-wrap { overflow-x:auto; }` + `.cc-skill-matrix { border-collapse:collapse; width:max-content; }` (css `:26-33`).
- header `.cc-skill-col-header { cursor:pointer; vertical-align:bottom; }`; nome vertical `.cc-skill-col-header__name { writing-mode:vertical-rl; transform:rotate(180deg); white-space:nowrap; }` (css `:42-54`); ícone `.cc-col-icon { writing-mode:horizontal-tb; }` (R2 — fora da rotação).
- tiers: `--empty` transparente, `--low/--mid/--high` com os `rgba` de `profiles-skills.css:96-107`; `__val` de `:109-118`.
- `.cc-skill-row:hover .cc-skill-cell { background: var(--mud-palette-action-default-hover); }` (porta do hover `:76-78`).
- `.cc-cell--disabled { opacity:.4; }` (R3 — aplicada por célula `th`/`td` quando `!col.Enabled`, não por seletor de coluna).
- `.cc-cost--ok { color: var(--mud-palette-success); }` / `.cc-cost--over { color: var(--mud-palette-warning); }`.

Tokens do viewer (`--accent-bright`, `--fg-dim`, etc.) **não existem** no editor — substituir por variáveis MudBlazor (`--mud-palette-*`) ou hex literais; as cores de categoria vêm de `SkillMaster.ColorOf` (não de classes `cat-header-row--Ph`).

## `NavMenu.razor` — link

Inserir **um** `MudNavLink` no `<MudNavMenu>`, junto dos links utilitários do topo (após "Classes", antes do `<MudDivider/>` da linha 34), sem tocar em mais nada:

```razor
<MudNavLink Href="/customclasses/skills" Match="NavLinkMatch.Prefix" Icon="@Icons.Material.Filled.GridOn">
    Skills matrix
</MudNavLink>
```

`Match="NavLinkMatch.Prefix"` para destacar quando ativo; ícone `GridOn` (matriz) — cosmético, ajustável.

## Decisões técnicas

- **D1 — Tabela HTML crua, não `MudTable`:** a matriz é densa, com header rotacionado e heatmap por célula; o controle fino de CSS portado do viewer é mais fiel e mais simples que dobrar um `MudTable`. Toggles e chips continuam MudBlazor (consistência). (Spec P8.)
- **D2 — Não reusar `SkillCanonicalList` como componente:** aquele componente é **uma classe por render** (linhas = skills de UMA classe, com campo de edição/barra/delta). A matriz é N classes × skills com heatmap por célula — layout fundamentalmente diferente. Reuso correto = **`SkillMaster`** (ordem/cores) + **a lógica do chip de multiplicador** (copiada, ~15 linhas) + **o padrão de projeção do `NavMenu`**. Reaproveitar a tabela do 031 forçaria um terceiro modo que distorceria o componente.
- **D3 — Custo 1× por navegação:** `ComputeSkillCost` é chamado em `LoadColumns` (OnInitialized), nunca por render/célula. `GetCachedEntries` idem. Toggles não recarregam. (CA7.)
- **D4 — Sem `LocationChanged`:** ao contrário da sidebar (que precisa re-highlightar a cada navegação), a matriz é uma página de destino — não re-projeta em navegação. Se o usuário voltar à matriz após editar, o Blazor re-monta a página → `OnInitialized` roda de novo com o cache 037 já atualizado (Save invalida a entry). Sem necessidade de assinar `Nav.LocationChanged` (sem `IDisposable`).

## Histórico de Alterações

| Data | Autor | Alteração |
|---|---|---|
| 2026-06-12 | Guilherme | Criação da spec técnica (tabela CRIAR/EDITAR, contratos reusados com refs arquivo:linha, view model, fragmentos, CSS portado, link no NavMenu, 4 decisões). |
| 2026-06-12 | Guilherme | Auto-review 03: resolvido R1 (overflow no `tfoot`/colspan), R2 (CategoryHeader colspan dinâmico), R3 (ícone no header rotacionado). |
