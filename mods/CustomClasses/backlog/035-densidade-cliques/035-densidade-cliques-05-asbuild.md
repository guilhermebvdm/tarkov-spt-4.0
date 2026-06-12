# 035 — Densidade global + redução de cliques — As-built

**Mod:** CustomClasses
**Data:** 2026-06-12
**Refs:** [00-kickoff](./035-densidade-cliques-00-kickoff.md) · [01-spec](./035-densidade-cliques-01-spec.md) · [02-spec-tech](./035-densidade-cliques-02-spec-tech.md)

> Mod **server-only** (Blazor Server + MudBlazor embutido no host SPT). Sem patch Harmony — todo o trabalho é Razor/CSS/JS no próprio mod. Implementação **autônoma** (usuário ausente — premissas registradas abaixo, não houve aprovação interativa). 1º uso de **JS interop** no mod (`window.ccPrefs` + Ctrl+S).

## Arquivos entregues

| Arquivo | Ação | Conteúdo |
|---|---|---|
| `modded/Server/Web/wwwroot/js/customclasses.js` | **CRIADO** | `window.ccPrefs` (IIFE, plain `<script src>`, NÃO módulo): `get/set/remove` sobre `localStorage` com `try/catch` (PA-035-03); `registerSaveShortcut(dotNetRef)` adiciona um `keydown` global em **capture** que captura Ctrl/Cmd+S, faz `preventDefault()` e chama `dotNetRef.invokeMethodAsync('OnSaveShortcut')` — com guard de pathname `/edit` como backstop (PA-R1-02); `unregisterSaveShortcut()`. |
| `modded/Server/Web/UiPrefs.cs` | **CRIADO** | Helper estático de interop (`namespace CustomClasses.Web`, sem DI/estado). Chaves `const`: `DrawerPinned`, `EditTab`, `ListSort`, `MatrixToggles`, `SidebarFilter`. `GetAsync/SetAsync/GetIntAsync/GetBoolAsync` engolem `JSException`+`InvalidOperationException` (prerender) → default (PA-035-02/03). |
| `modded/Server/Web/_imports.razor` | EDITADO | Adicionado `@using Microsoft.JSInterop` (necessário p/ `IJSRuntime`/`DotNetObjectReference`/`[JSInvokable]` nas razor — não estava importado). |
| `modded/Server/Web/Layouts/BaseLayout.razor` | EDITADO | (d) `<script src="/CustomClasses-Server/js/customclasses.js">` no `<HeadContent>` (após o css). (a/PA-R1-01) **pin** do drawer via `Variant` Mini↔Persistent (`_drawerPinned`) — NÃO binda `Open`; `OpenMiniOnHover="@(!_drawerPinned)"`. Botão de pin no AppBar (`PushPin`/`PushPin` outlined) → `ToggleDrawerPin` grava `cc.ui.drawerPinned`. `OnAfterRenderAsync(firstRender)` reconcilia o pin do `localStorage`. `@inject IJSRuntime JS` + `@using CustomClasses.Web`. |
| `modded/Server/Web/Pages/Classes.razor` | EDITADO | (b) 3 colunas ordenáveis (`MudTableSortLabel` Class/Skill cost/Loadout; null/sem-definição → `double.MaxValue` ordena por último; Loadout usa `HasError` não o proxy null — PA-R1-07). Botão **Edit** por linha (`Disabled` sem definição) → `EditClass`. Persiste a ordenação via `SortDirectionChanged`→`OnSortChanged` (`cc.ui.listSort` = `<label>\|asc/desc`); restaura no `OnAfterRenderAsync` via `@ref` por label + `ToggleSortDirection()` (1×=Asc, 2×=Desc). `@inject IJSRuntime JS`. |
| `modded/Server/Web/Shared/NavMenu.razor` | EDITADO | (b) ação **Edit** por item (hover, `.cc-sidebar-edit`, `stopPropagation`, só `HasDefinition`) → `EditClass`. (c/PA-035-04) `NavigateToClass` e `EditClass` anexam `?tab={_editTab}` no ramo edit→edit; `_editTab` cacheado do `localStorage` no after-render e **re-lido a cada `LocationChanged`** (`RefreshEditTabAsync`) p/ não ficar stale. (d) filtro persistido em `cc.ui.sidebarFilter` (`OnFilterChanged`). `@inject IJSRuntime JS` + `@using CustomClasses.Web`. CSS `.cc-sidebar-edit` no `<style>` scoped. |
| `modded/Server/Web/Pages/ClassEdit.razor` | EDITADO | (c) `[SupplyParameterFromQuery(Name="tab")] int? Tab`. `OnParametersSet` aplica `?tab` **uma vez** por instância (PA-R1-03, flag `_tabFromQueryApplied`) + marca `_pendingTabPersist`. `OnAfterRenderAsync(firstRender)`: registra Ctrl+S (`ccPrefs.registerSaveShortcut`), persiste o tab vindo de query, e — sem query — reconcilia do `cc.ui.editTab` (uma vez). Setter de `ActivePanelIndex` grava `cc.ui.editTab` quando muda. `[JSInvokable] OnSaveShortcut` → `InvokeAsync(SaveAsync)` (no-op se `_saving`, PA-R1-06). `Dispose` desregistra o listener + `_dotNetRef.Dispose()`. `ClampTab(0..StashTabIndex)`. (a) `MudTabs PanelClass="pa-2"` (era `pa-4`). `@inject IJSRuntime JS`. |
| `modded/Server/Web/Pages/SkillsMatrix.razor` | EDITADO | (e) `NavigateTo(col)` → `/edit?tab=1` quando `HasDefinition`, senão detalhe (fallback p/ classe inválida); `SkillsTabIndex=1` espelha o mapa de abas do `ClassEdit`. (d) toggles `_showDisabled`/`_showMultipliers` persistidos em `cc.ui.matrixToggles` (`PersistToggles` no `@bind-Value:after`; restaura no `OnAfterRenderAsync`). `@inject IJSRuntime JS`. |
| `mods/CustomClasses/docs/class-editor.md` | EDITADO | Frontmatter/cabeçalho **preservado**. Tabela de rotas + §sidebar + §7 já refletiam o 035 (waves 030–036); adicionada **linha no Histórico de Alterações** registrando a entrega do código 035. |
| `modded/Server/Web/wwwroot/css/customclasses.css` | NÃO MODIFICADO | A regra `.cc-sidebar-edit` ficou no `<style>` scoped do `NavMenu.razor` (onde vivem as outras `.cc-sidebar-*`), não no css global — coesão com as classes irmãs. Densidade dos componentes já coberta pelo 033; nenhuma regra global nova foi necessária. |

### Densidade (a) — verificação por componente

A passada (a) era **incremental** ("aplicar só onde falta"). Auditados todos os `MudTable`/`MudSelect`/`MudTextField`/`MudNumericField` da árvore `Web/`:

- **JÁ densos (no-op confirmado):** `ItemPicker`, `AmmoPicker`, `PresetPicker`, `CustomizationPicker`, `ItemSpecEditor`, `SkillCanonicalList`, `ClassLifecycleCreate/Duplicate/DeleteDialog`, `Classes` (`MudTable Dense`), `Home`, e todos os campos do `ClassEdit` (`Margin.Dense` / `Dense` conforme o knob válido de cada componente).
- **Única mudança de densidade:** `ClassEdit` `MudTabs PanelClass` `pa-4`→`pa-2`.
- `MUD0002` evitado: nenhum `Dense` ilegal reintroduzido em `MudTextField`/`MudNumericField` (CR-01-02 do 034 respeitado).

## Decisões de implementação

- **Restauração da ordenação via `ToggleSortDirection()`** (não `SetSortLabel`): a inspeção do `MudBlazor.xml` 8.13.0 (NuGet cache) confirmou que `MudTable<T>` **não** expõe `SetSortLabel` público; `MudTableSortLabel<T>.SetSortDirection` "não atualiza a ordem da tabela". O método público que **re-ordena** é `ToggleSortDirection()` (`AllowUnsorted=false` → Asc↔Desc; labels começam sem ordenação). Restauração: `@ref` por label + 1 toggle (Asc) ou 2 (Desc). Envolto em `try/catch` → degrada p/ ordem de arquivo. Os toggles re-disparam `SortDirectionChanged` (re-persiste o mesmo valor — idempotente).
- **Tab como query síncrona + localStorage assíncrono** (PA-035-04): `?tab=N` é a fonte síncrona confiável na troca de classe; `cc.ui.editTab` é a persistência cross-sessão. `_editTab` no NavMenu é re-lido em cada `LocationChanged` p/ refletir a aba que o usuário acabou de escolher.
- **Persistir o tab vindo de query** (`_pendingTabPersist`): aplicar `?tab` no `OnParametersSet` é set direto no campo (sem re-entrar no setter durante params), então a gravação em `cc.ui.editTab` é feita no after-render — assim a matriz→Skills propaga o tab pro NavMenu nas trocas seguintes.
- **Pin do drawer, não Open** (PA-R1-01): o drawer Mini + `OpenMiniOnHover` não tem estado de `Open` estável p/ bindar. Persistimos o **pin** (Mini↔Persistent). Default `false` = Mini = comportamento de hoje.
- **`.cc-sidebar-edit` no `<style>` do NavMenu**: coeso com as outras `.cc-sidebar-*` (todas scoped no componente), em vez de poluir o css global do 033.

## Premissas autônomas (usuário ausente)

- **PA-AB-035-01:** a "passada de regressão visual Chrome MCP + re-medição dos tempos do 037" do kickoff **NÃO** faz parte desta entrega — o orquestrador a executa na validação final com o server real. Esta entrega é só **código + docs** (conforme instrução explícita da tarefa).
- **PA-AB-035-02 (API de sort):** mecanismo de restauração escolhido (`ToggleSortDirection`) a partir da inspeção do `MudBlazor.xml` 8.13.0; o `try/catch` garante que qualquer drift de API degrade p/ ordem de arquivo sem quebrar a página. Confirmar visualmente na validação runtime.
- **PA-AB-035-03 (`@using Microsoft.JSInterop`):** o `_imports.razor` não importava `Microsoft.JSInterop`; adicionado lá (em vez de por arquivo) p/ as 4 páginas com interop. `MudBlazor`/`Microsoft.AspNetCore.Components` (p/ `[SupplyParameterFromQuery]`) já resolviam.
- **PA-AB-035-04 (densidade já presente):** os pickers/diálogos já estavam densos (confirmando PA-R1-05 da spec-tech). A passada (a) reduziu-se a `PanelClass pa-4→pa-2`. Nenhuma regra css global nova.
- **PA-AB-035-05 (Dispose obrigatório do listener):** `ClassEdit.Dispose` chama `ccPrefs.unregisterSaveShortcut` + `_dotNetRef.Dispose()`. O guard de pathname no JS é backstop, não substituto (PA-R1-02).
- **PA-035-05 (ícone já renderiza):** confirmado — `Classes.razor` e a sidebar já renderizam o ícone; nenhuma mudança de renderização de ícone.

## Build

- `dotnet build` — **NÃO executado** (estágio dedicado faz o build, conforme instrução). TODO validar 0 erros / 0 warnings (atenção: `ToggleSortDirection` deve compilar como `Task`; `MudIcon` sem `title` — título movido p/ o `<span>` wrapper).

## Verificação funcional (TODO — validação runtime no server real)

> Memória `feedback_spt_validation`: escrita+hash não basta — validar na UI/jogo.

- [ ] Densidade sem `MUD0002` na build.
- [ ] Lista: colunas ordenam; ordenação persiste no reload; Edit por linha navega pro edit.
- [ ] Sidebar: Edit no hover navega; troca de classe edit→edit preserva a aba.
- [ ] Matriz: célula/header abre o edit na aba Skills (classe inválida → detalhe).
- [ ] `Ctrl+S`/`Cmd+S` salva (mesma validação do botão; bloqueio por Error) e suprime o "salvar página" nativo; só na página de edit.
- [ ] Preferências (pin do drawer, aba, ordenação, toggles da matriz, filtro da sidebar) persistem entre reloads; 1ª visita = defaults.

## Histórico

| Data | Evento |
|---|---|
| 2026-06-12 | As-built criado via `/code-mod` (autônomo — usuário ausente). 035 é a passada de polimento sobre as waves 030–037 já commitadas. |
