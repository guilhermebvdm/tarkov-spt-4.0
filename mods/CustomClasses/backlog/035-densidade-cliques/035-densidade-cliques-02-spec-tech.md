# 035 — Densidade global + redução de cliques · Spec Técnica

**Mod:** CustomClasses
**Spec funcional:** [035-densidade-cliques-01-spec.md](035-densidade-cliques-01-spec.md)
**Criado:** 2026-06-12

> Mod **server-only** (Blazor Server + MudBlazor embutido no host SPT via `IModWebMetadata`). Não há patch de `Assembly-CSharp`/Harmony — as seções "Pontos de patch" e "Propriedades F12 (BepInEx)" do template não se aplicam. Todo o trabalho é Razor/CSS/JS no próprio mod (`mods/CustomClasses/modded/Server/Web/...`). As únicas novidades de runtime são **JS interop** (1º uso no mod) para `localStorage` + atalho `Ctrl+S`.

## 1. Estratégia

Quatro frentes, nenhuma altera validação/custo/schema:

1. **(a) Densidade** — passada de `Dense`/`Margin.Dense` nos componentes Mud ainda airy (`ClassEdit` `MudTabs`/`MudGrid`/campos, diálogos de lifecycle, pickers). CSS do 033 (`.cc-dense`, paddings de tabela) permanece como está.
2. **(b) Lista + sidebar** — `MudTable` da `Classes.razor` ganha `SortLabel`/`SortBy` nas 3 colunas e um botão **Edit** por linha; `NavMenu` ganha um Edit por item (hover).
3. **(c) Edit** — `ClassEdit` lê/grava a aba ativa via preferência persistida; `Ctrl+S` chama `SaveAsync`. `NavMenu.NavigateToClass` passa a anexar a aba ativa na navegação edit→edit (preservar aba).
4. **(d) Persistência + atalho** — helper C# `UiPrefs` sobre `IJSRuntime` lendo/gravando `localStorage`; `wwwroot/js/customclasses.js` com `get/set/remove` + registro do listener `keydown` Ctrl+S (via `DotNetObjectReference`). Linkado no `BaseLayout` `<HeadContent>`. **(e)** Matriz: `SkillsMatrix.NavigateTo` → rota de edit + aba Skills.

### Premissas autônomas (revisão 2026-06-12, usuário ausente — não aprovável)

- **PA-035-01 (1º JS interop):** o mod não tem JS próprio hoje (confirmado: nenhum `IJSRuntime`/`InvokeVoidAsync`/`localStorage` em `modded/Server`; o único `<script>` é o `MudBlazor.min.js` do `BaseLayout:37`). Este item introduz `wwwroot/js/customclasses.js` servido pelo mount `/CustomClasses-Server/` (mesmo de css/icons — `CustomClassesMetadata` `IModWebMetadata`). É um `<script src>` plano (não-módulo) no `<HeadContent>`, igual ao padrão do MudBlazor (BaseLayout UI-03), expondo `window.ccPrefs`. **Não** usar `import()`/JS module isolation — o host não garante o pipeline de RCL JS modules para mods; `<script src>` plano é o padrão comprovado do repo.
- **PA-035-02 (interop só pós-circuito):** toda chamada a `IJSRuntime` ocorre em `OnAfterRenderAsync(firstRender)` ou em handlers de evento — **nunca** em `OnInitialized`/prerender (o prerender estático não tem JS; chamar lá lança `InvalidOperationException: JavaScript interop calls cannot be issued during prerendering`). Os componentes montam com os defaults atuais e **reconciliam** com a preferência no 1º after-render.
- **PA-035-03 (preferência não-bloqueante):** ler `localStorage` é assíncrono (interop). A UI nunca espera por ele para renderizar a 1ª vez; aplicar a preferência dispara um `StateHasChanged` adicional. Chave ausente/corrompida → `try/catch` → default. Sem persistência server-side.
- **PA-035-04 (aba na navegação edit→edit):** preservar a aba ativa NÃO depende exclusivamente do `localStorage` (que é assíncrono e poderia perder a corrida com o `OnParametersSet` do novo `ClassEdit`). A aba viaja **também** como query string na URL de navegação da sidebar (`?tab=N`), lida sincronamente em `OnParametersSet`. O `localStorage` é o fallback/persistência entre sessões; a query é o caminho síncrono confiável dentro da sessão. Decisão: **query é a fonte primária na troca de classe; localStorage é a fonte na 1ª montagem sem query**.
- **PA-035-05 (ícone já renderiza):** o kickoff afirma "ícone da classe na linha (hoje não renderiza)". O código atual **já renderiza** (`Classes.razor:57-63`, `img src=/CustomClasses-Server/icons/...`). Divergência consciente: nenhuma mudança necessária na renderização do ícone; o item só adiciona ordenação + Edit. Registrado para o code-review não "consertar" algo que já funciona.
- **PA-035-06 (Enter no picker fora de escopo):** ver spec funcional §Fora de escopo. Não implementar nesta wave.

## 2. Pontos de patch

Não aplicável — mod server-only, sem patch de `Assembly-CSharp`/Harmony. Ver nota de cabeçalho.

## 3. Novas propriedades F12 (BepInEx)

Não aplicável — sem `ConfigEntry` (mod server, não plugin BepInEx).

## 4. Arquivos do mod

| Arquivo | Ação | Resumo |
|---|---|---|
| [`modded/Server/Web/wwwroot/js/customclasses.js`](../../modded/Server/Web/wwwroot/js/customclasses.js) | **CRIAR** | `window.ccPrefs = { get(key), set(key,val), remove(key) }` sobre `localStorage` (com `try/catch`) + `registerSaveShortcut(dotNetRef)` que adiciona um `keydown` global capturando Ctrl/Cmd+S (`e.preventDefault()` + `dotNetRef.invokeMethodAsync('OnSaveShortcut')`) e `unregisterSaveShortcut()`. Servido em `/CustomClasses-Server/js/customclasses.js`. |
| [`modded/Server/Web/UiPrefs.cs`](../../modded/Server/Web/UiPrefs.cs) | **CRIAR** | Helper estático de interop: `Task<string?> GetAsync(IJSRuntime, string key)`, `Task SetAsync(...)`, `Task<int> GetIntAsync(..., int @default)`, `Task<bool> GetBoolAsync(...)`. Centraliza as chaves (`const string DrawerOpen = "cc.ui.drawerOpen"`, etc.) e o `try/catch` de interop (engole `JSException`/`InvalidOperationException` de prerender → retorna default). Sem estado próprio (sem DI). |
| [`modded/Server/Web/Layouts/BaseLayout.razor`](../../modded/Server/Web/Layouts/BaseLayout.razor) | MODIFICAR | `<HeadContent>` (`:18-23`): `<script src="/CustomClasses-Server/js/customclasses.js"></script>` (plano, após o link de css). `MudDrawer` (`:59-67`): **PA-R1-01** — NÃO bindar `Open` (o modo Mini + `OpenMiniOnHover` não tem estado de Open estável). Em vez disso, persistir um **pin** (`cc.ui.drawerPinned`): `Variant="@(_drawerPinned ? DrawerVariant.Persistent : DrawerVariant.Mini)"` reconciliado no `OnAfterRenderAsync(firstRender)`. (Botão de pin no AppBar grava a chave.) |
| [`modded/Server/Web/Pages/Classes.razor`](../../modded/Server/Web/Pages/Classes.razor) | MODIFICAR | (b) `MudTable` (`:43`): `<MudTh>` de Class/Skill cost/Loadout viram `<MudTh><MudTableSortLabel SortLabel=... SortBy=...>`; adicionar `@implements IDisposable`? não — usar `OnAfterRenderAsync` p/ ler `cc.ui.listSort` e `MudTable.SetSortLabel`. Coluna Actions (`:79-90`): adicionar `MudIconButton` Edit (`Icons.Material.Filled.Edit`) → `Nav.NavigateTo($".../{bare}/edit")`, `Disabled` quando `context.Entry.Definition is null`. `OnRowClick` mantém detalhe. |
| [`modded/Server/Web/Shared/NavMenu.razor`](../../modded/Server/Web/Shared/NavMenu.razor) | MODIFICAR | (b) item do sidebar (`:67-85`): `MudIconButton` Edit no hover (`opacity:0` → `:hover` 1 via classe css), `@onclick:stopPropagation` → navega pro edit. (c/PA-035-04) `NavigateToClass` (`:281-289`): no ramo edit→edit, anexar `?tab={ActiveTab}` à URL; `ActiveTab` lido de `cc.ui.editTab` (cacheado no `LoadRows`/after-render). (d) `_filter` persistido em `cc.ui.sidebarFilter` (opcional). |
| [`modded/Server/Web/Pages/ClassEdit.razor`](../../modded/Server/Web/Pages/ClassEdit.razor) | MODIFICAR | (c) novo `[Parameter, SupplyParameterFromQuery(Name="tab")] int? Tab`; `OnParametersSet` (`:557`) seta `ActivePanelIndex` por: query `Tab` → senão `cc.ui.editTab` (after-render) → senão 0. `ActivePanelIndex setter` (`:539`) grava `cc.ui.editTab`. `OnAfterRenderAsync(firstRender)`: `ccPrefs.registerSaveShortcut(_dotNetRef)`. `[JSInvokable] Task OnSaveShortcut()` → `if (!_saving) await SaveAsync()`. `Dispose` (`:867`): `unregisterSaveShortcut` + `_dotNetRef?.Dispose()`. (a) densidade nos `MudTextField`/`MudSelect` que ainda não têm `Margin.Dense`; `MudTabs` `PanelClass="pa-2"` (era `pa-4`). |
| [`modded/Server/Web/Pages/SkillsMatrix.razor`](../../modded/Server/Web/Pages/SkillsMatrix.razor) | MODIFICAR | (e) `NavigateTo(col)` (`:451`): destino vira `/customclasses/classes/{bare}/edit?tab={SkillsTabIndex}` **quando** `col.HasDefinition`, senão mantém o detalhe (fallback p/ classe inválida). `SkillsTabIndex = 1` (const, espelha o índice de aba do `ClassEdit`). (d) os 2 toggles (`_showDisabled`/`_showMultipliers`, `:34-35`) persistidos em `cc.ui.matrixToggles` (lidos no after-render, gravados no `@bind-Value:after`). |
| [`modded/Server/Web/Shared/ClassLifecycleCreateDialog.razor`](../../modded/Server/Web/Shared/ClassLifecycleCreateDialog.razor) | **VERIFICAR (provável no-op)** | (a/PA-R1-05) **JÁ denso**: `MudTextField` `Margin.Dense` (`:21`), `MudAlert Dense` (`:27,41`). Provável nenhuma mudança. |
| [`modded/Server/Web/Shared/ClassLifecycleDuplicateDialog.razor`](../../modded/Server/Web/Shared/ClassLifecycleDuplicateDialog.razor) | VERIFICAR | (a) ler antes; aplicar densidade só onde faltar. |
| [`modded/Server/Web/Shared/ClassLifecycleDeleteDialog.razor`](../../modded/Server/Web/Shared/ClassLifecycleDeleteDialog.razor) | VERIFICAR | (a) ler antes; aplicar densidade só onde faltar. |
| [`modded/Server/Web/Shared/ItemPicker.razor`](../../modded/Server/Web/Shared/ItemPicker.razor) | **VERIFICAR (provável no-op)** | (a/PA-R1-05) **JÁ denso** (`Margin.Dense`/`Dense` `:24-31,42`). Enter-select fora de escopo (PA-035-06). |
| [`modded/Server/Web/Shared/AmmoPicker.razor`](../../modded/Server/Web/Shared/AmmoPicker.razor), [`PresetPicker.razor`](../../modded/Server/Web/Shared/PresetPicker.razor), [`CustomizationPicker.razor`](../../modded/Server/Web/Shared/CustomizationPicker.razor), [`ItemSpecEditor.razor`](../../modded/Server/Web/Shared/ItemSpecEditor.razor) | MODIFICAR | (a) passada de densidade nos campos Mud ainda airy (verificar cada um; só onde a prop válida existir — PA-R: não reintroduzir `MUD0002`). |
| [`modded/Server/Web/wwwroot/css/customclasses.css`](../../modded/Server/Web/wwwroot/css/customclasses.css) | MODIFICAR | (b) `.cc-sidebar-edit` (botão Edit do item: `opacity:0; transition; .cc-sidebar-item:hover & { opacity:1 }`). Aditivo — não redefine classes 033/034. |
| [`docs/class-editor.md`](../../docs/class-editor.md) | MODIFICAR | Atualizar rotas/fluxos das waves 030–036 + atalhos/persistência do 035. **PRESERVAR frontmatter/cabeçalho** (hook bloqueia escrita sem ele) + linha no Histórico de Alterações. |

> **Confirmar antes de editar (não assumir densidade default):** os pickers e o `ItemSpecEditor` já podem estar com `Margin.Dense` (a `ItemPicker` está — `:24-31,42`). A passada (a) é **incremental**: ler cada componente, aplicar a prop de densidade **válida** só onde falta. Referência de armadilha: code-review CR-01-02 do 034 removeu um `Dense` ilegal de `MudTextField` (`MUD0002`) — em `MudTextField`/`MudNumericField`/`MudSelect` o knob de densidade do input é `Margin="Margin.Dense"`; `Dense="true"` é válido em `MudTable`, `MudSelect` (lista do dropdown), `MudTabs`, `MudSimpleTable`, `MudChip`, `MudAlert`.

## 5. Stubs de código

> Server-only — sem `// ref: Assembly-CSharp`. Refs apontam para o próprio mod.

### (d) JS de preferências + atalho Ctrl+S

```javascript
// modded/Server/Web/wwwroot/js/customclasses.js
// Plain <script src> (NOT a module) — served at /CustomClasses-Server/js/customclasses.js by the
// same wwwroot mount as css/icons (CustomClassesMetadata : IModWebMetadata). Mirrors the MudBlazor
// plain-script pattern (BaseLayout UI-03). No bundler, no import().
window.ccPrefs = (function () {
    function get(key) { try { return window.localStorage.getItem(key); } catch { return null; } }
    function set(key, val) { try { window.localStorage.setItem(key, val); } catch { /* quota/denied */ } }
    function remove(key) { try { window.localStorage.removeItem(key); } catch { } }

    let saveHandler = null;
    function registerSaveShortcut(dotNetRef) {
        unregisterSaveShortcut();
        saveHandler = function (e) {
            // PA-R1-02: defesa em profundidade — mesmo um handler órfão (dispose vazou) NÃO
            // sequestra Ctrl+S fora da página de edit. O dispose do ClassEdit AINDA é obrigatório.
            if (!window.location.pathname.includes('/edit')) { return; }
            if ((e.ctrlKey || e.metaKey) && (e.key === 's' || e.key === 'S')) {
                e.preventDefault();                       // suppress the browser "save page"
                dotNetRef.invokeMethodAsync('OnSaveShortcut');
            }
        };
        window.addEventListener('keydown', saveHandler, true);   // capture: beat the browser default
    }
    function unregisterSaveShortcut() {
        if (saveHandler) { window.removeEventListener('keydown', saveHandler, true); saveHandler = null; }
    }
    return { get, set, remove, registerSaveShortcut, unregisterSaveShortcut };
})();
```

### (d) Helper de interop C#

```csharp
// modded/Server/Web/UiPrefs.cs
using Microsoft.JSInterop;

namespace CustomClasses.Web;

/// <summary>
///     Thin wrapper over window.ccPrefs (localStorage) — see wwwroot/js/customclasses.js.
///     ALL calls must run after the interactive circuit connects (OnAfterRenderAsync(firstRender)
///     or an event handler), NEVER during prerender (PA-035-02): a prerender interop call throws
///     InvalidOperationException. Every method swallows that + JSException and falls back to the
///     default — a missing/denied/corrupted key never breaks the page (PA-035-03).
/// </summary>
public static class UiPrefs
{
    public const string DrawerPinned   = "cc.ui.drawerPinned";   // PA-R1-01: pin Mini↔Persistent
    // PA-R1-08: cc.ui.lastView removida do v1 (a vista já vem da URL na troca pela sidebar; sem consumidor).
    public const string EditTab        = "cc.ui.editTab";
    public const string ListSort       = "cc.ui.listSort";       // "<label>|asc" / "<label>|desc"
    public const string MatrixToggles  = "cc.ui.matrixToggles";  // "<showDisabled>|<showMultipliers>" e.g. "1|0"
    public const string SidebarFilter  = "cc.ui.sidebarFilter";

    public static async Task<string?> GetAsync(IJSRuntime js, string key)
    {
        try { return await js.InvokeAsync<string?>("ccPrefs.get", key); }
        catch (JSException) { return null; }
        catch (InvalidOperationException) { return null; }   // prerender / no JS yet
    }

    public static async Task SetAsync(IJSRuntime js, string key, string value)
    {
        try { await js.InvokeVoidAsync("ccPrefs.set", key, value); }
        catch (JSException) { } catch (InvalidOperationException) { }
    }

    public static async Task<int> GetIntAsync(IJSRuntime js, string key, int @default)
        => int.TryParse(await GetAsync(js, key), out var v) ? v : @default;

    public static async Task<bool> GetBoolAsync(IJSRuntime js, string key, bool @default)
        => (await GetAsync(js, key)) is { } s ? s == "1" : @default;
}
```

### (c) ClassEdit — aba persistida + Ctrl+S

```razor
@* modded/Server/Web/Pages/ClassEdit.razor — additions to the existing @code *@
@inject IJSRuntime JS

@code {
    // PA-035-04: query is the SYNCHRONOUS primary source on a class switch; localStorage is the
    // 1st-mount / cross-session fallback. SupplyParameterFromQuery binds ?tab=N from NavMenu.
    [Parameter, SupplyParameterFromQuery(Name = "tab")] public int? Tab { get; set; }

    private DotNetObjectReference<ClassEdit>? _dotNetRef;
    private bool _tabReconciled;        // applied the persisted (localStorage) tab once
    private bool _tabFromQueryApplied;  // PA-R1-03: applied the ?tab query ONCE (don't clobber clicks)

    // existing OnParametersSet (:557) — append:
    //   PA-R1-03: ?tab fica na URL; OnParametersSet re-roda em qualquer re-render. Aplicar UMA vez,
    //   senão a aba "gruda" na query e o usuário não consegue trocar de aba de forma estável.
    //   ClassEdit re-monta a cada {FileName} diferente na rota → o flag reinicia por classe nova.
    // if (Tab is { } t && !_tabFromQueryApplied) { _tabFromQueryApplied = true; _activePanelIndex = ClampTab(t); }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender) return;
        _dotNetRef = DotNetObjectReference.Create(this);
        await JS.InvokeVoidAsync("ccPrefs.registerSaveShortcut", _dotNetRef);

        // No query tab → reconcile from localStorage exactly once (PA-035-02/03).
        if (Tab is null && !_tabReconciled)
        {
            _tabReconciled = true;
            var saved = await UiPrefs.GetIntAsync(JS, UiPrefs.EditTab, _activePanelIndex);
            var clamped = ClampTab(saved);
            if (clamped != _activePanelIndex) { _activePanelIndex = clamped; StateHasChanged(); }
        }
    }

    [JSInvokable]
    public async Task OnSaveShortcut()
    {
        // PA-R1-06: [JSInvokable] roda fora do contexto de render → mutações de _saving/_saveDiagnostics
        // precisam de InvokeAsync para renderizar o feedback (botão "Saving…", banner, snackbar) igual ao clique.
        await InvokeAsync(async () =>
        {
            if (_saving) return;             // corner case: don't queue a 2nd save
            await SaveAsync();               // reuses the button path → form validation + Error gate
        });
    }

    private static int ClampTab(int t) => t is >= 0 and <= StashTabIndex ? t : 0;   // 0..6, else General

    // ActivePanelIndex setter (:539) — after assigning _activePanelIndex, persist:
    //   _ = UiPrefs.SetAsync(JS, UiPrefs.EditTab, value.ToString());   // fire-and-forget

    // Dispose (:867) — append:
    //   try { _ = JS.InvokeVoidAsync("ccPrefs.unregisterSaveShortcut"); } catch { }
    //   _dotNetRef?.Dispose();
}
```

> `StashTabIndex = 6` já existe (`ClassEdit.razor:531`) — reusar como teto do clamp (General=0..Stash=6). O `SkillsTabIndex` da matriz (=1) deve **espelhar** esse mesmo mapa de abas; documentar a dependência num comentário em ambos os arquivos (mudou a ordem das `MudTabPanel` → atualizar as duas constantes).

### (b) Classes.razor — colunas ordenáveis + Edit

```razor
@inject IJSRuntime JS
@* HeaderContent (:45-55): wrap the sortable columns. SortBy projects the Row field. *@
<MudTh><MudTableSortLabel T="Row" SortLabel="name" SortBy="@(r => r.Name)">Class</MudTableSortLabel></MudTh>
@* numeric columns: null (no definition) sorts LAST in both directions — stable, no NRE (corner case) *@
<MudTh Style="text-align:right">
    <MudTableSortLabel T="Row" SortLabel="skillCost"
        SortBy="@(r => r.SkillCost?.Total ?? double.MaxValue)">Skill cost</MudTableSortLabel></MudTh>
@* PA-R1-07: usar o sinal semântico HasError (Row já tem, :108), não o proxy `SkillCost is null`. *@
<MudTh Style="text-align:right">
    <MudTableSortLabel T="Row" SortLabel="loadout"
        SortBy="@(r => r.HasError ? double.MaxValue : r.LoadoutTotal)">Loadout</MudTableSortLabel></MudTh>

@* Actions cell (:79-90): add Edit before Duplicate. *@
<MudIconButton Icon="@Icons.Material.Filled.Edit" Size="Size.Small" title="Edit this class"
               Disabled="@(context.Entry.Definition is null)"
               OnClick="@(() => EditClass(context))"/>

@code {
    private MudTable<Row>? _table;   // @ref on the MudTable for SetSortLabel on restore

    private void EditClass(Row row)
    {
        var bare = Path.GetFileNameWithoutExtension(row.Entry.FileName);
        Nav.NavigateTo($"/customclasses/classes/{Uri.EscapeDataString(bare)}/edit");
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender) return;
        // Restore persisted sort: "<label>|asc|desc". MudTable exposes the sort via the SortLabel API.
        if (await UiPrefs.GetAsync(JS, UiPrefs.ListSort) is { } s && s.Split('|') is [var label, var dir] && _table is not null)
        {
            var d = dir == "desc" ? SortDirection.Descending : SortDirection.Ascending;
            // MudTableSortLabel binding restore — call the table's sort API (see MudBlazor MudTable.SetSortLabel).
            // Persist on change via OnSortLabelChanged callback on the MudTable.
        }
    }

    private void OnSort(string label, SortDirection dir)
        => _ = UiPrefs.SetAsync(JS, UiPrefs.ListSort, $"{label}|{(dir == SortDirection.Descending ? "desc" : "asc")}");
}
```

> **Nota de API (PA-R candidato):** a forma exata de restaurar a ordenação programaticamente no `MudTable` (`SetSortLabel`/`OnSortLabelChanged` vs. bind no `MudTableSortLabel.SortDirectionChanged`) deve ser confirmada contra a versão de MudBlazor do host no `/code-mod` (ver §7). O contrato funcional (persistir coluna+direção, restaurar no reload) não muda; só o mecanismo Mud.

### (e) SkillsMatrix — célula → edit na aba Skills

```razor
@code {
    private const int SkillsTabIndex = 1;   // MUST mirror ClassEdit tab order (General=0, Skills=1, …)

    private void NavigateTo(ClassColumn col)
    {
        var bare = Uri.EscapeDataString(col.BareName);
        // HasDefinition: an invalid/unparseable class has no edit form → fall back to the detail.
        Nav.NavigateTo(col.HasDefinition
            ? $"/customclasses/classes/{bare}/edit?tab={SkillsTabIndex}"
            : $"/customclasses/classes/{bare}");
    }
}
```

### (a) BaseLayout — script + drawer persistido

```razor
@* <HeadContent> (:18-23) — after the css link: *@
<script src="/CustomClasses-Server/js/customclasses.js"></script>

@* MudDrawer (:59-67) — PA-R1-01: NÃO bindar Open (Mini + OpenMiniOnHover não tem Open estável).
   Persistir o PIN via Variant Mini↔Persistent. Open="true" continua literal (mini sempre montado). *@
<MudDrawer Open="true" Width="250px" ClipMode="DrawerClipMode.Always" Elevation="5"
           Variant="@(_drawerPinned ? DrawerVariant.Persistent : DrawerVariant.Mini)"
           OpenMiniOnHover="@(!_drawerPinned)" Breakpoint="Breakpoint.None">

@code {
    [Inject] private IJSRuntime JS { get; set; } = default!;
    private bool _drawerPinned;   // default false = Mini (= comportamento de hoje)

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender) return;
        var pinned = await UiPrefs.GetBoolAsync(JS, UiPrefs.DrawerPinned, _drawerPinned);
        if (pinned != _drawerPinned) { _drawerPinned = pinned; StateHasChanged(); }
    }

    // Botão de pin no AppBar → toggla _drawerPinned + grava:
    //   _drawerPinned = !_drawerPinned;
    //   _ = UiPrefs.SetAsync(JS, UiPrefs.DrawerPinned, _drawerPinned ? "1" : "0");
}
```

## 6. Fluxo de dados

```
Boot / 1ª render (prerender estático)
  → componentes montam com DEFAULTS (drawer aberto, aba General, ordem de arquivo, toggles on/off)
  → NENHUM acesso a localStorage (PA-035-02: JS indisponível no prerender)

Circuito interativo conecta → OnAfterRenderAsync(firstRender) de cada componente
  → UiPrefs.Get* (ccPrefs.get) → reconcilia estado (drawer/aba/ordenação/toggles) → StateHasChanged
  → ClassEdit: ccPrefs.registerSaveShortcut(_dotNetRef)

Troca de classe pela sidebar (edit→edit)
  → NavMenu.NavigateToClass → /classes/{bare}/edit?tab={ActiveTab}   (PA-035-04: query síncrona)
  → ClassEdit.OnParametersSet lê Tab (query) → ActivePanelIndex (aba preservada)

Clique numa célula da matriz
  → SkillsMatrix.NavigateTo → /classes/{bare}/edit?tab=1  (Skills) | fallback detalhe se inválida

Ctrl/Cmd+S na página de edit
  → window keydown (capture) → e.preventDefault() → dotNetRef.OnSaveShortcut → SaveAsync (se !_saving)

Mudança de preferência (drawer/aba/ordenação/toggle)
  → handler grava ccPrefs.set (fire-and-forget) → próxima sessão relê
```

## 7. Riscos e dependências

- **Prerender x interop (PA-035-02):** o maior risco. Qualquer `IJSRuntime` no `OnInitialized`/`OnParametersSet` lança no prerender. Mitigação: interop só em `OnAfterRenderAsync(firstRender)` e handlers; `UiPrefs` engole `InvalidOperationException`/`JSException`. A reconciliação pós-circuito causa um "flash" do default → valor salvo (aceito — single-user local).
- **DotNetObjectReference vazamento:** `ClassEdit` cria `DotNetObjectReference.Create(this)` e registra o listener global de keydown. **Obrigatório** `unregisterSaveShortcut` + `_dotNetRef.Dispose()` no `Dispose()` (a página já é `@implements IDisposable`, `:3`) — senão o handler de uma página de edit anterior continua vivo e dispara `OnSaveShortcut` num componente disposto. O `keydown` é registrado em `capture` para preceder o atalho nativo do browser.
- **Ordem das abas acoplada em 2 lugares:** `ClassEdit.StashTabIndex`/clamp e `SkillsMatrix.SkillsTabIndex` codificam o mapa de abas. Reordenar as `MudTabPanel` exige atualizar ambas as constantes — documentar com comentário cruzado (mitiga o acoplamento; não há fonte única hoje).
- **API de ordenação do MudTable:** o mecanismo exato de restaurar/persistir a ordenação programaticamente depende da versão de MudBlazor do host. Confirmar no `/code-mod` (`MudTableSortLabel.SortDirectionChanged` + `MudTable.SetSortLabel`/`@bind`); o contrato (persistir coluna+direção) é estável.
- **Contrato 037→030/032 intocado:** lista/sidebar/matriz seguem consumindo `EditorService.GetCachedEntries()` uma vez por navegação (não por item de render). A ordenação da lista opera sobre as `Row`s já projetadas em memória (sort client-side do `MudTable`), **não** re-consulta o cache — zero impacto no 037.
- **`MUD0002` (analyzer):** densidade aplicada só com a prop válida de cada componente (ver nota da §4). Reintroduzir `Dense` em `MudTextField` quebra a build (analyzer). Code-review deve checar.
- **JS servido pelo mount do mod:** depende de `CustomClassesMetadata : IModWebMetadata` montar `wwwroot/` em `/CustomClasses-Server/` (já comprovado p/ css/icons). Em dev/local o `/compile-mod` instala `wwwroot/` sempre (doc class-editor §5.2).
- **Concorrência cross-tab:** múltiplas abas → última escrita no `localStorage` vence (PA-035-03). Aceito (single-user). Sem `storage` event listener (não vale a complexidade).

## 8. Checklist de implementação

- [ ] **(d)** Criar `wwwroot/js/customclasses.js` (`ccPrefs` get/set/remove + register/unregisterSaveShortcut, capture).
- [ ] **(d)** Criar `Web/UiPrefs.cs` (chaves const + Get/Set/GetInt/GetBool com `try/catch` de prerender/JSException).
- [ ] **(d/PA-R1-01)** `BaseLayout`: `<script src>` no `<HeadContent>`; **pin** do drawer via `Variant` Mini↔Persistent (`_drawerPinned`), NÃO bindar `Open`; botão de pin no AppBar; reconciliar/gravar `cc.ui.drawerPinned` no after-render.
- [ ] **(a)** Passada de densidade incremental: `ClassEdit` (`MudTabs PanelClass="pa-2"`, campos faltantes `Margin.Dense`), diálogos de lifecycle, pickers, `ItemSpecEditor` — **ler cada um, aplicar só a prop válida** (não reintroduzir `MUD0002`).
- [ ] **(b)** `Classes.razor`: `MudTableSortLabel` em Class/Skill cost/Loadout (null→`double.MaxValue` p/ ordenar por último); botão Edit por linha (`Disabled` sem definição); persistir/restaurar `cc.ui.listSort` (`@ref` no MudTable).
- [ ] **(b)** `NavMenu`: botão Edit por item (hover, `stopPropagation`), css `.cc-sidebar-edit`.
- [ ] **(c)** `ClassEdit`: `[SupplyParameterFromQuery] Tab`; `OnParametersSet` aplica query→localStorage→0; setter de `ActivePanelIndex` grava `cc.ui.editTab`; `registerSaveShortcut` + `[JSInvokable] OnSaveShortcut` (no-op se `_saving`); `Dispose` desregistra + `_dotNetRef.Dispose()`.
- [ ] **(c/PA-035-04)** `NavMenu.NavigateToClass` anexa `?tab={ActiveTab}` no ramo edit→edit.
- [ ] **(e)** `SkillsMatrix.NavigateTo` → `/edit?tab=1` quando `HasDefinition`, senão detalhe; `SkillsTabIndex=1` espelhando o mapa de abas.
- [ ] **(d)** `SkillsMatrix`: persistir/restaurar `cc.ui.matrixToggles`; (opcional) `NavMenu` persiste `cc.ui.sidebarFilter`.
- [ ] Atualizar `docs/class-editor.md` (rotas/fluxos 030–036 + atalhos/persistência 035) **preservando frontmatter** + linha no Histórico.
- [ ] Verificação funcional in-game (memory `feedback_spt_validation`): salvar produz mesmo `.jsonc`/diagnósticos/audit; preferências persistem entre reloads; `Ctrl+S` salva e bloqueia o save nativo; aba preservada na troca de classe; densidade sem `MUD0002`.

## Histórico

| Data | Evento |
|---|---|
| 2026-06-12 | Spec técnica criada via `/create-technical-spec` (autônoma — usuário ausente). Premissas PA-035-01..06 registradas (1º JS interop do mod, interop só pós-circuito, query como fonte síncrona da aba, ícone já renderiza, Enter-no-picker fora de escopo). |
| 2026-06-12 | Review técnica 01 (autônoma): 3 🔴 resolvidos in-place — PA-R1-01 (drawer Mini não persiste `Open`; trocado por pin `Variant` Mini↔Persistent), PA-R1-02 (guard de pathname no listener Ctrl+S + dispose obrigatório), PA-R1-03 (`?tab` aplicado uma vez via flag). Refinamentos PA-R1-04..09 anotados (sort restore, escopo de densidade, `InvokeAsync` no OnSaveShortcut, proxy de sort, `lastView` removida, flash aceito). Ver review-01. |
