# 030 — Sidebar persistente de classes — Spec técnica

**Mod:** CustomClasses
**Criado:** 2026-06-12
**Refs:** [01-spec](./030-sidebar-classes-01-spec.md) · [00-kickoff](./030-sidebar-classes-00-kickoff.md) · [037-spec](../037-performance-cache/037-performance-cache-01-spec.md) · [024-spec-tech](../024-class-viewer/024-class-viewer-02-spec-tech.md)

## Resumo da abordagem

O drawer (`MudDrawer` em `BaseLayout.razor:37-45` envolvendo `<NavMenu/>`) passa a hospedar uma **lista de classes** renderizada pelo `NavMenu.razor`. A lista vem de uma projeção leve por navegação (`ClassEditorService.ListClassSummaries()`, novo — projeta a view leve da cache do 037, sem `dry-run`). Cada item: ícone tingido + nome na `nameColor` + custo de skills compacto + dot de status. 1 clique navega preservando a vista (detail↔edit) via `NavigationManager`. O guard de unsaved-changes é um `<NavigationLock>` renderizado no `BaseLayout` (circuit-scoped) que consulta um estado de "sujo" exposto pelo formulário de edição via `CascadingValue` (ver §5 — único acoplamento cross-item, documentado).

## Arquivos a criar / modificar

| Arquivo | Ação | Detalhe |
|---|---|---|
| `modded/Server/ClassEditorService.cs` | MODIFICAR | Adicionar `public IReadOnlyList<ClassSummary> ListClassSummaries()` + o record `ClassSummary`. Projeta `GetCachedEntries()` (`ClassEditorService.cs:182`) — leitura da cache do 037, **sem novo `dry-run`** (território compartilhado aceitável; ver §1). |
| `modded/Server/CostService.cs` | (consumir) | `ComputeSkillCost(ClassDefinition)` (`CostService.cs:106`) chamado UMA vez por classe dentro de `ListClassSummaries` para derivar `SkillCostTotal`/`WithinBudget`. Cálculo barato (sem reconstrução de loadout / sem `dry-run`). Não modificar. |
| `modded/Server/Web/Shared/NavMenu.razor` | MODIFICAR (território) | Lista de classes + filtro + dot de status + navegação 1-clique preservando vista. Substitui o `MudNavMenu` atual (`NavMenu.razor:5-16`), mantendo Home/Classes como utilitários no topo. |
| `modded/Server/Web/Layouts/BaseLayout.razor` | MODIFICAR (território) | Hospeda o `<NavigationLock>` (guard) + provê o `CascadingValue<EditGuardState>` consumido pelo `NavMenu` e pelo formulário de edição. Drawer permanece `Variant=Mini`/`OpenMiniOnHover` (`BaseLayout.razor:37-45`). |
| `modded/Server/Web/Pages/ClassEdit.razor` | MODIFICAR (cross-item, §5) | Marcar `EditGuardState.IsDirty` quando o `ClassEditModel` muda e expor `SaveAsync`/`Discard` ao guard. Mínimo necessário — território do 025/026, documentado como dependência. |

> Nenhuma mudança em `ClassRegistrar`, builders, registries, csproj. `ListClassFiles`/`GetCachedEntries`/cache do 037 não são alterados — só consumidos.

## §1 — Contrato 037 → 030: `ListClassSummaries()`

O gancho exposto pelo 037 é **`ClassEditorService.GetCachedEntries()`** (`ClassEditorService.cs:182`), cujo XML-doc já reserva o nome `ListClassSummaries()` para o item 030 e prescreve a projeção. Assinatura nova:

```csharp
/// <summary>One lightweight per-class row for the sidebar (item 030). Projected from the
/// 037 entry cache (GetCachedEntries) — NO extra dry-run. SkillCostTotal/WithinBudget come
/// from CostService.ComputeSkillCost (cheap; no loadout rebuild). Loadout ₽ is intentionally
/// out (too costly per row — stays on the detail/edit pages).</summary>
public sealed record ClassSummary(
    string FileName,            // bare file name (route key via Path.GetFileNameWithoutExtension)
    string DisplayName,         // def.Name ?? FileName  (header label; same as the list)
    string? DisplayNameEn,      // def.DisplayName?.En   (filter also matches this)
    string? NameColor,          // def.NameColor (null → theme default)
    string? IconUrl,            // "/CustomClasses-Server/icons/{iconFile}" or null
    bool Enabled,               // entry.Enabled
    bool HasError,              // def is null || Diagnostics has any Error
    bool HasDefinition,         // entry.Definition is not null  (drives edit→detail fallback)
    double SkillCostTotal,      // 0 when no def / no skills
    bool WithinBudget,          // SkillCostTotal in [BudgetMin, BudgetMax]; true when total==0 (neutral)
    SidebarStatus Status);      // computed enum (see §3)

public enum SidebarStatus { Healthy, Invalid, Disabled, OverBudget }

public IReadOnlyList<ClassSummary> ListClassSummaries();
```

Implementação (esboço, dentro do `ClassEditorService`, já injeta `CostService`? **não** — ver §1a):

```csharp
public IReadOnlyList<ClassSummary> ListClassSummaries()
{
    var summaries = new List<ClassSummary>();
    foreach (var e in GetCachedEntries())   // ClassEditorService.cs:182 — hot cache, no dry-run
    {
        var def = e.Definition;
        var hasError = def is null || e.Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error);
        var skillCost = def is null ? null : _costService.ComputeSkillCost(def);   // cheap
        var total = skillCost?.Total ?? 0;
        var within = skillCost is null || total == 0 || skillCost.WithinBudget;
        var status =
            hasError    ? SidebarStatus.Invalid    :
            !e.Enabled  ? SidebarStatus.Disabled   :
            !within     ? SidebarStatus.OverBudget :
                          SidebarStatus.Healthy;
        summaries.Add(new ClassSummary(
            e.FileName,
            def?.Name ?? e.FileName,
            def?.DisplayName?.En,
            def?.NameColor,
            def?.IconFile is { Length: > 0 } icon ? $"/CustomClasses-Server/icons/{icon}" : null,
            e.Enabled, hasError, def is not null, total, within, status));
    }
    return summaries;
}
```

### §1a — Dependência `CostService` no `ClassEditorService`

`ClassEditorService` hoje **não** injeta `CostService` (construtor em `ClassEditorService.cs:65-72`). Há duas opções:

- **(A) Injetar `CostService` no `ClassEditorService`** — ambos são `[Injectable(InjectionType.Singleton)]` (`CostService.cs:96`, `ClassEditorService.cs:64`); `CostService` depende de `CatalogService`/helpers, sem ciclo de volta para `ClassEditorService` (verificado: `CostService.cs:97-101` injeta `CatalogService, ItemHelper, InventoryHelper, DatabaseService` — nenhum referencia `ClassEditorService`). Sem ciclo de DI.
- **(B)** `ListClassSummaries()` projeta só os campos do `ClassFileEntry` (sem custo) e o **`NavMenu` chama `CostService.ComputeSkillCost`** por classe. O `NavMenu` já pode `@inject CostService` (padrão de `Classes.razor:14`).

**Decisão:** **(B)**. Mantém `ClassEditorService` sem nova dependência e mantém o cálculo de custo na camada de UI (igual a `Classes.razor:132`, que já faz `CostService.ComputeSkillCost` por linha). `ListClassSummaries()` então retorna os campos do entry (sem `SkillCostTotal`/`WithinBudget`/`Status` que dependam de custo); o `NavMenu` computa custo/budget/status uma vez por navegação ao montar suas linhas de view-model. Reflete o `ClassSummary` revisado abaixo.

```csharp
// (B) — projeção pura sobre a cache, ZERO custo/dry-run no service:
public sealed record ClassSummary(
    string FileName, string Name, string? DisplayNameEn,
    string? NameColor, string? IconUrl,
    bool Enabled, bool HasError, bool HasDefinition,
    ClassDefinition? Definition);   // p/ o NavMenu chamar ComputeSkillCost 1× por classe

public IReadOnlyList<ClassSummary> ListClassSummaries() =>
    GetCachedEntries().Select(e =>
    {
        var def = e.Definition;
        return new ClassSummary(
            e.FileName, def?.Name ?? e.FileName, def?.DisplayName?.En,
            def?.NameColor,
            def?.IconFile is { Length: > 0 } icon ? $"/CustomClasses-Server/icons/{icon}" : null,
            e.Enabled,
            def is null || e.Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error),
            def is not null, def);
    }).ToList();
```

> O `NavMenu` deriva `SidebarStatus` + custo em um único `LoadRows()` (mesmo molde de `Classes.razor:121-146`), chamado em `OnInitialized`/em location-change — **uma vez por navegação**, nunca por item num loop de render.

## §2 — NavMenu: lista, filtro, navegação

- **Fonte:** `@inject ClassEditorService EditorService` + `@inject CostService CostService` + `@inject NavigationManager Nav`. Em `OnInitialized` e ao detectar mudança de localização (ver §4), montar `List<SidebarRow>` a partir de `EditorService.ListClassSummaries()` + `CostService.ComputeSkillCost(summary.Definition)`.
- **Render:** manter Home (`/customclasses`) e Classes (`/customclasses/classes`) como `MudNavLink` no topo (utilitários). Abaixo, um `MudTextField` de filtro (`Immediate=true`) e a lista de classes (um elemento clicável por linha — `MudNavLink` com `Href` calculado, ou `div`/`MudListItem` com `OnClick="() => NavigateToClass(row)"`).
- **Item ativo:** comparar `row.FileName` (extensionless) com o segmento de classe da URL atual (parse de `Nav.Uri`, §4). Ativa → classe CSS com strip lateral + fundo (padrão do viewer antigo; cores do tema dark MudBlazor).
- **Ícone tingido + nome:** `<img src="@row.IconUrl">` quando há; nome em `<span style="color:@row.NameColor">` com fallback do tema quando `NameColor` é null/inválida (espelha `Classes.razor:214` `NameStyle`). Custo compacto: `row.SkillCostTotal.ToString("0")` num chip pequeno.
- **Dot de status:** `<span>` colorido por `SidebarStatus` (vermelho/cinza/laranja/none) — paleta alinhada ao `StatusChip` de `ClassDetail.razor:532-550` / `Classes.razor:217-238`.
- **Filtro:** `rows.Where(r => r.Name.Contains(f, OrdinalIgnoreCase) || (r.DisplayNameEn?.Contains(...) ?? false))`; `f` vazio → todas. Lista vazia → texto neutro "no classes match".

```csharp
private sealed record SidebarRow(
    ClassSummary Summary, double SkillCostTotal, SidebarStatus Status)
{
    public string FileName => Summary.FileName;
    public string BareName => System.IO.Path.GetFileNameWithoutExtension(Summary.FileName);
    public string Name => Summary.Name;
    public string? DisplayNameEn => Summary.DisplayNameEn;
}
```

## §3 — Mapeamento de status (dot)

Espelha exatamente a árvore de decisão de `Classes.razor:217-238` / `ClassDetail.razor:532-550`, acrescentando o estado **OverBudget** (laranja) que o kickoff pede:

| Condição (na ordem) | `SidebarStatus` | Cor do dot |
|---|---|---|
| `def is null` OU qualquer `Diagnostic.Error` | `Invalid` | vermelho (`Color.Error`) |
| `!Enabled` | `Disabled` | cinza (`Color.Default`) |
| custo de skills `> 0` e fora de `[SkillWeights.BudgetMin, BudgetMax]` | `OverBudget` | laranja (`Color.Warning`) |
| caso contrário (válida, habilitada, dentro do budget, ou custo 0) | `Healthy` | sem dot de alerta |

`SkillWeights.BudgetMin/Max` (28/32) já é a fonte usada em `Classes.razor:253`. Custo 0 (ex.: Peladão) é neutro, não OverBudget (paridade com `Classes.razor:246-250`).

## §4 — Preservação de vista (detail↔edit) e item ativo

A vista corrente é derivada de `Nav.Uri` (relativo a `Nav.BaseUri`). Rotas reais:
- Detalhe: `@page "/customclasses/classes/{FileName}"` (`ClassDetail.razor:1`).
- Edição: `@page "/customclasses/classes/{FileName}/edit"` (`ClassEdit.razor:1`).

`FileName` na rota é **sem extensão** (decisão do 024 — `024-class-viewer-02-spec-tech.md:17`; `Classes.razor:157` navega com `Path.GetFileNameWithoutExtension` + `Uri.EscapeDataString`).

```csharp
private enum CurrentView { Other, Detail, Edit }

private CurrentView DetectView(out string? activeBare)
{
    activeBare = null;
    var rel = Nav.ToBaseRelativePath(Nav.Uri).Split('?', '#')[0].Trim('/');
    // expects: customclasses/classes/{bare}    or    customclasses/classes/{bare}/edit
    var parts = rel.Split('/');
    if (parts.Length >= 3 && parts[0] == "customclasses" && parts[1] == "classes")
    {
        activeBare = Uri.UnescapeDataString(parts[2]);
        return parts.Length >= 4 && parts[3] == "edit" ? CurrentView.Edit : CurrentView.Detail;
    }
    return CurrentView.Other;
}

private void NavigateToClass(SidebarRow row)
{
    var bare = Uri.EscapeDataString(row.BareName);
    var view = DetectView(out _);
    // edit→edit só se a classe destino tem definição parseável; senão cai no detail (01-spec / fallback)
    var target = (view == CurrentView.Edit && row.Summary.HasDefinition)
        ? $"/customclasses/classes/{bare}/edit"
        : $"/customclasses/classes/{bare}";
    Nav.NavigateTo(target);   // o guard (§5) intercepta se a edição atual estiver suja
}
```

- **Atualizar a lista/destaque por navegação:** assinar `Nav.LocationChanged` (`+= OnLocationChanged` em `OnInitialized`, `-=` em `Dispose` — `NavMenu` passa a `@implements IDisposable`), recomputando o item ativo (e, se necessário, re-derivando custo/status só se a navegação foi um Save/Delete — caso contrário a cache do 037 mantém os números). `DetectView` é chamado tanto para o highlight quanto para `NavigateToClass`.
- **Clique na própria classe ativa, mesma vista:** `NavigateToClass` gera a mesma URL → `NavigateTo` é no-op de roteamento; não reabre guard (o guard só dispara em mudança real de localização e só se sujo).

## §5 — Guard de unsaved changes (CRÍTICO)

**Restrição de arquitetura (achado):** o host (`SPTWeb.InitializeSptBlazor`, ver `020-infra-web-blazor-02-spec-tech.md:30-33`) é quem registra os serviços Blazor; o mod **não** registra serviços scoped por circuito — só usa singletons SPT `[Injectable]` via `@inject`. Um `[Injectable(Singleton)]` é **process-wide** (compartilhado entre todos os circuitos) → **inadequado** para o flag "sujo" (vazaria entre abas/usuários). Portanto o estado do guard precisa viver **no circuito**, não num singleton de DI.

**Solução (dentro do território + 1 toque cross-item documentado):**

1. `BaseLayout.razor` (território) instancia um `EditGuardState` simples (POCO, um por circuit — o layout é único por página/circuito) e o provê via `<CascadingValue Value="_guard" IsFixed="true">` ao redor de `<NavMenu/>` e `@Body`. Também renderiza o interceptador:

```razor
@code {
    private readonly EditGuardState _guard = new();
}
<CascadingValue Value="_guard" IsFixed="true">
    <NavigationLock OnBeforeInternalNavigation="OnBeforeNavAsync"
                    ConfirmExternalNavigation="@_guard.IsDirty" />
    ...drawer com <NavMenu/> ... <MudMainContent>@Body</MudMainContent>
</CascadingValue>
```

`EditGuardState` (novo, em `Web/`):
```csharp
public sealed class EditGuardState
{
    public bool IsDirty { get; set; }
    /// <summary>Set by the edit page so the guard can offer "Save" (returns true on success).</summary>
    public Func<Task<bool>>? SaveAsync { get; set; }
    /// <summary>Set by the edit page so the guard can offer "Discard" (reverts the form in place).</summary>
    public Action? Discard { get; set; }
    public void Reset() { IsDirty = false; SaveAsync = null; Discard = null; }
}
```

2. `OnBeforeNavAsync(LocationChangingContext ctx)` no `BaseLayout`: se `!_guard.IsDirty` → retorna (deixa navegar). Se sujo → abre o diálogo MudBlazor de 3 botões (`IDialogService` já está disponível no host de providers do layout; padrão `DialogService.ShowMessageBox`/dialog custom com 3 ações):
   - **Cancelar** → `ctx.PreventNavigation()` (permanece na edição, mudanças intactas).
   - **Descartar** → `_guard.Reset()`; deixa a navegação seguir (perde mudanças).
   - **Salvar** → `await _guard.SaveAsync?.Invoke()`. Se `true` → `_guard.Reset()`, deixa navegar. Se `false` (Save bloqueado por Error de validação — `ClassEdit.SaveAsync` retorna sem sucesso) → `ctx.PreventNavigation()` + snackbar (corner case "Salvar falha" do 01-spec; espelha `ClassEdit.razor:728-737`).

3. **Toque cross-item em `ClassEdit.razor` (território do 025/026 — documentado, mínimo):**
   - `[CascadingParameter] EditGuardState Guard { get; set; }`.
   - Em `OnAfterRender`/nos `bind:after` que já recomputam custo (ex.: `RecomputeSkillCost` em `ClassEdit.razor:238`, `ScheduleRecompute` em `ClassEdit.razor:845`), setar `Guard.IsDirty = true` (qualquer mutação do `_model`). Alternativa robusta: comparar um snapshot `ToDefinition()` serializado vs o carregado (dirty real), evitando falso-positivo de re-render; decisão de implementação fica para o 025/026, mas o **contrato** que o 030 exige é: `Guard.IsDirty == true` sse há mudança não persistida.
   - Em `OnInitialized`: `Guard.SaveAsync = async () => { await SaveAsync(); return _saveDiagnostics.All(d => d.Severity != DiagnosticSeverity.Error) && _savedOnce; }` e `Guard.Discard = Discard`.
   - Em `SaveAsync` bem-sucedido e em `Discard` (`ClassEdit.razor:642`,`:685`): `Guard.IsDirty = false`.
   - No `Dispose` (`ClassEdit.razor:896`): `Guard.Reset()` (sair da edição limpa o estado para a próxima vista — detalhe/lista não têm form sujo).

> **Premissa:** `NavigationLock` (`Microsoft.AspNetCore.Components`, .NET 7+) está disponível no host Blazor do SPT 4.0 (net9.0, `AddInteractiveServerComponents` — `020-...-02:30`). Cobre navegação interna (links/`NavigateTo`) via `OnBeforeInternalNavigation` e navegação externa (refresh/fechar aba) via prompt nativo do browser quando `ConfirmExternalNavigation=true` — esta última é só o prompt genérico do browser, **não** o diálogo Save/Discard/Cancel (fora de escopo no 01-spec).

## §6 — Responsividade

Drawer permanece `Variant=Mini` + `OpenMiniOnHover` (`BaseLayout.razor:41-42`). No estado mini só o ícone tingido + dot aparecem; `title`/`MudTooltip` no item traz nome + status. Classe sem `iconFile`: glifo/inicial de fallback (degradação para texto, paridade com a lista que mostra só o nome quando `IconUrl is null` — `Classes.razor:58-63`).

## §7 — Concorrência / cache

`ListClassSummaries` → `GetCachedEntries` → `ListClassFiles` é thread-safe (cache `ConcurrentDictionary`, `ClassEditorService.cs:94`). A projeção é leitura; `ClassSummary`/`SidebarRow` são imutáveis. `ComputeSkillCost` é puro (não muta estado compartilhado). O `EditGuardState` é por-circuito (não compartilhado) → sem corrida entre circuitos.

## Decisões registradas

- Custo de loadout (₽) **fora** da sidebar (caro por linha); só custo de skills (barato, sem `dry-run`).
- `ListClassSummaries()` projeta a cache do 037 (`GetCachedEntries`), **sem** novo `dry-run`; custo/status derivados na UI (opção B, §1a) — `ClassEditorService` não ganha dependência de `CostService`.
- Guard via `NavigationLock` no `BaseLayout` + `EditGuardState` por circuito (CascadingValue), porque o host não permite registrar serviço scoped do mod e singleton SPT vazaria entre circuitos.
- O acoplamento com `ClassEdit.razor` (set do dirty-flag + handlers Save/Discard) é o único toque fora do território 030; documentado como dependência cross-item (025/026). Sem ele, o guard não tem sinal de "sujo" — ver review §🔴-1.

## Histórico

| Data | Evento |
|---|---|
| 2026-06-12 | Spec técnica criada via `/create-technical-spec` |
