# 030 — Sidebar persistente de classes — As-built

**Mod:** CustomClasses
**Data:** 2026-06-12
**Refs:** [01-spec](./030-sidebar-classes-01-spec.md) · [02-spec-tech](./030-sidebar-classes-02-spec-tech.md) · [00-kickoff](./030-sidebar-classes-00-kickoff.md) · [037-asbuild](../037-performance-cache/037-performance-cache-05-asbuild.md)

> Mod server-only (Blazor Server + serviços DI SPT). Sem patch Harmony. Implementação autônoma (usuário ausente — premissas registradas abaixo, não houve aprovação interativa). `dotnet build` NÃO executado (estágio dedicado faz).

## Escopo de território (restrição desta sessão)

Esta implementação tocou **APENAS** `NavMenu.razor` e `BaseLayout.razor` (território do 030). A spec técnica (§1a/§5) previa também tocar `ClassEditorService.cs` (adicionar `ListClassSummaries()`) e `ClassEdit.razor` (set do dirty-flag + handlers Save/Discard). Esses dois arquivos são território de outros itens (037 / 025-026) e **não foram modificados** — ver "Desvios da spec técnica" e "Dependências cross-item pendentes".

## Arquivos entregues

| Arquivo | Conteúdo |
|---|---|
| `modded/Server/Web/Shared/NavMenu.razor` | REESCRITO — drawer vira **sidebar persistente de classes**. Mantém Home/Classes como `MudNavLink` utilitários no topo + `MudDivider`. Abaixo: `MudTextField` de filtro (`Immediate`, `DebounceInterval=120`, `Clearable`) e a lista de classes. Fonte de dados: **`EditorService.GetCachedEntries()`** (gancho 037 — view leve da cache, **sem dry-run por render**). `LoadRows()` projeta cada `ClassFileEntry` num `SidebarRow` e deriva custo+status **uma vez por navegação** via `CostService.ComputeSkillCost` (1 call/classe, sem reconstrução de loadout — espelha `Classes.razor:121-146`). Item: dot de status + ícone tingido (`/CustomClasses-Server/icons/{iconFile}`, fallback = inicial do nome) + nome na `nameColor` + custo compacto. `MudTooltip` (nome+status+custo) para o estado mini do drawer (spec §6). Filtro casa `Name` e `DisplayName.En` (OrdinalIgnoreCase). `@implements IDisposable` + `Nav.LocationChanged` (re-projeta + re-destaca a cada navegação; cobre Save/Delete que invalidam a cache do 037). `DetectView`/`NavigateToClass` preservam a vista (detail↔edit; inválida cai no detail — `HasDefinition`). `<style>` scoped: strip lateral + hover dark (espelha `profiles.css:21-60`). |
| `modded/Server/Web/Layouts/BaseLayout.razor` | EDITADO — hospeda o **guard de unsaved-changes**. `@inject IDialogService`. `EditGuardState` (POCO **nested public** no `@code`) instanciado **uma vez por circuito** (`_guard`) e provido via `<CascadingValue IsFixed="true">` ao redor de `<NavMenu/>` + `@Body`. `<NavigationLock OnBeforeInternalNavigation="OnBeforeNavAsync" ConfirmExternalNavigation="@_guard.IsDirty"/>`. `OnBeforeNavAsync`: se `!IsDirty` deixa navegar; se sujo abre `DialogService.ShowMessageBox` de 3 botões (**Save**=yes / **Discard**=no / **Cancel**) — Cancel/dismiss → `ctx.PreventNavigation()`; Save → `await _guard.SaveAsync()` e só navega se retornou `true` (senão `PreventNavigation`); Discard → `_guard.Discard()` + `Reset()` e segue. Drawer permanece `Variant=Mini`/`OpenMiniOnHover` intocado. |

> Nenhuma mudança em `ClassEditorService`, `CostService`, `ClassRegistrar`, builders, registries, csproj.

## Decisões de implementação

- **Fonte de dados = `GetCachedEntries()` direto, sem `ListClassSummaries()`** (desvio da spec §1a-B). O gancho 037 já é público e retorna `IReadOnlyList<ClassFileEntry>` com tudo que a projeção do `SidebarRow` precisa (`FileName`, `Enabled`, `Diagnostics`, `Definition?`). Como `ClassEditorService.cs` está fora do território desta sessão, o `NavMenu` projeta o `SidebarRow` localmente em `LoadRows()` (mesma forma que a spec previa para a opção B — só sem o passo intermediário do record `ClassSummary`). Resultado funcional idêntico; **zero dry-run por render** mantido (o XML-doc de `GetCachedEntries` autoriza exatamente esse consumo).
- **Status (dot) — árvore de decisão** espelha `Classes.razor:217-238` + estado **OverBudget** do kickoff: `Invalid` (def null OU qualquer `Diagnostic.Error`) → vermelho; `Disabled` (`!Enabled`) → cinza; `OverBudget` (skill cost > 0 e fora de `[BudgetMin,BudgetMax]`) → laranja; senão `Healthy` (sem dot). Custo 0 (ex.: Peladão) é **neutro**, não OverBudget (paridade `Classes.razor:246-250`). Cores via CSS vars do tema (`--mud-palette-error/text-disabled/warning`) em vez de `MudChip` (dot é um `<span>` puro — mais leve por linha).
- **`EditGuardState` é nested public no `BaseLayout`** (não um arquivo próprio em `Web/` como a spec §5 sugeria) — criar `Web/EditGuardState.cs` está fora do território (só posso editar os 2 arquivos). O `CascadingValue` funciona hoje; quando o 025/026 wirar o `ClassEdit`, o tipo deve ser **promovido a `Web/EditGuardState.cs`** (top-level) para um `[CascadingParameter] EditGuardState` limpo — ver pendência cross-item.
- **Diálogo do guard = `ShowMessageBox` 3-botões** (yes=Save / no=Discard / cancel=Cancel) em vez de um dialog custom — é o caminho MudBlazor mais simples que entrega os 3 desfechos sem novo componente. `ShowMessageBox` retorna `bool?`: `true`=Save, `false`=Discard, `null`=Cancel/dismiss.
- **`NavigateToClass` emite a URL e deixa o `NavigationLock` arbitrar.** Clique na própria classe/vista ativa gera a mesma URL → no-op de roteamento, guard não dispara. O guard só intercepta mudança real de localização E só quando `IsDirty`.
- **Re-projeção em `LocationChanged`** (não em cada render): cobre o caso Save/Delete (a cache do 037 já reflete) e re-destaca o item ativo. `GetCachedEntries()` quente é dir-scan + leitura de dict (barato), nunca dry-run.

## Desvios da spec técnica

- **`ListClassSummaries()` NÃO foi adicionado ao `ClassEditorService`** (spec §1a-B previa). Motivo: território. O `NavMenu` consome `GetCachedEntries()` diretamente — o record intermediário `ClassSummary` virou o `SidebarRow` privado do componente. Se no futuro outro consumidor precisar da projeção, mover para o service como `ListClassSummaries()` continua sendo a evolução natural (o XML-doc do gancho 037 já reserva o nome).
- **Guard NÃO ligado ao `ClassEdit.razor`.** A infra do guard (estado por circuito + `NavigationLock` + diálogo) está **completa e inerte por padrão**: `IsDirty` nasce `false`, então o `NavigationLock` é no-op (navegação nunca bloqueada) até o `ClassEdit` setar o flag. Isso é um default **seguro** (nunca bloqueia indevidamente), mas significa que o DoD "sair do edit com mudanças pendentes SEMPRE pergunta" **ainda não está satisfeito** — depende da pendência cross-item abaixo.

## Dependências cross-item pendentes (para fechar o DoD)

Em `ClassEdit.razor` (território 025/026 — NÃO tocado aqui):
1. `[CascadingParameter] BaseLayout.EditGuardState Guard { get; set; }` (ou tipo promovido a `Web/EditGuardState.cs`).
2. Setar `Guard.IsDirty = true` em qualquer mutação não persistida do `_model` (ganchos existentes: `RecomputeSkillCost`/`ScheduleRecompute`/`bind:after`). `IsDirty = false` após Save bem-sucedido (`SaveAsync`, ~`:730`) e em `Discard` (`:642`).
3. Em `OnInitialized`: `Guard.SaveAsync = async () => { await SaveAsync(); return _savedOnce; }` (`_savedOnce` já vira `true` só quando `result.Success` — `ClassEdit.razor:728-731`) e `Guard.Discard = Discard`.
4. Em `Dispose` (`:896`): `Guard.Reset()`.

Sem (1)-(4) o guard fica inerte. Com eles, o contrato exigido pelo 030 é: `Guard.IsDirty == true` sse há mudança não persistida.

## Premissas autônomas (usuário ausente)

- **PA-030-01:** `NavigationLock` / `LocationChangingContext` (`Microsoft.AspNetCore.Components.Routing`, .NET 7+) estão disponíveis no host Blazor do SPT 4.0 (net9.0, `Microsoft.NET.Sdk.Web` + `AddInteractiveServerComponents`). Não há `_Imports.razor` no mod; o Razor SDK do `Sdk.Web` já injeta os `@using` default (`Microsoft.AspNetCore.Components.*`, `.Routing`, `.Web`) — por isso os componentes existentes usam `NavigationManager`/MudBlazor sem `@using` explícito. Mesma cobertura vale para `NavigationLock`/`LocationChangedEventArgs`. TODO validar no build/runtime.
- **PA-030-02:** `EditGuardState` ficou **nested** no `BaseLayout` por restrição de território (não posso criar `Web/EditGuardState.cs`). Premissa: o `CascadingValue<EditGuardState>` resolve por tipo no consumidor independentemente de ser nested. Quando o 025/026 wirar, promover a top-level.
- **PA-030-03:** dot de status como `<span>` com CSS var do tema (não `MudChip`) — escolha de UI para densidade na sidebar; sem aprovação interativa.
- **PA-030-04:** `DebounceInterval=120` ms no filtro (inline) — responsivo sem flood de `StateHasChanged`. Não exposto como config (mod server).
- **PA-030-05:** ícone-fallback = primeira letra do nome num quadrado tingido (degradação para texto, paridade com a lista que mostra só o nome sem ícone — `Classes.razor:58-63`).
- **PA-030-06:** custo compacto exibido só quando `> 0` (classe vazia/Peladão não polui a linha).

## Build

- `dotnet build` — NÃO executado neste estágio (estágio dedicado faz). TODO validar 0 erros / 0 warnings (atenção a: disponibilidade de `NavigationLock`/`LocationChangingContext` — PA-030-01; nullability de `EditGuardState.SaveAsync?`/`Discard?`).

## Verificação funcional (TODO — validação runtime, memória `feedback_spt_validation`)

- [ ] Sidebar lista TODAS as classes (incl. disabled/invalid) com ícone+cor+custo+dot.
- [ ] Trocar de classe = **1 clique** preservando a vista (detail→detail, edit→edit; inválida cai no detail).
- [ ] Filtro por nome / display name (en) narrows em tempo real; lista vazia → "No classes match".
- [ ] Item ativo destacado (strip lateral + fundo) acompanha a navegação (`LocationChanged`).
- [ ] Estado mini do drawer mostra só dot+ícone; tooltip traz nome+status.
- [ ] Após Save/Delete a sidebar reflete custo/status novos sem reload manual.
- [ ] **Guard** (depende da pendência cross-item): sair do `/edit` sujo SEMPRE pergunta Save/Discard/Cancel; Cancel mantém na edição; Save bloqueado por Error mantém na edição + snackbar; Discard segue perdendo mudanças; refresh/close-tab dispara o prompt nativo do browser quando sujo.

## Histórico

| Data | Evento |
|---|---|
| 2026-06-12 | As-built criado via `/code-mod` (autônomo). Sidebar (lista+filtro+dot+nav 1-clique preservando vista) + infra do guard (EditGuardState por circuito + NavigationLock + diálogo) entregues em NavMenu.razor/BaseLayout.razor. `ListClassSummaries()` substituído por consumo direto de `GetCachedEntries()` (território). Wiring do guard no ClassEdit fica como dependência cross-item (025/026). |
