# 027 — Criar / duplicar / deletar classe — As-built

**Mod:** CustomClasses
**Data:** 2026-06-10
**Refs:** [01-spec](./027-criar-duplicar-deletar-01-spec.md) · [02-spec-tech](./027-criar-duplicar-deletar-02-spec-tech.md)

## Arquivos entregues

| Arquivo | Conteúdo |
|---|---|
| `modded/Server/ClassEditorService.cs` | EDITADO — seção "Lifecycle (item 027)": `CreateResult`, `ExistingEditionNames()` (templates ∪ names dos arquivos, case-insensitive), `ValidateNewClassName()`, `Create()` (template mínimo + `Save(hotApply:true)` + audit `create`), `Duplicate()` (`Load` + record `with` + audit `duplicate`), `ProfilesUsingEdition()` (`../../profiles` via `ModHelper`, `JsonDocument` streaming em `info.edition`/`info.username`, skip de arquivo ruim), `Slugify`/`UniqueClassFileName`. Usings novos: `System.Globalization`, `System.Text`, `System.Text.Json`. |
| `modded/Server/ClassRegistrar.cs` | EDITADO — `DefaultBaseEdition` `private`→`public const` (1 linha; o Create escreve o default explícito). |
| `modded/Server/ClassDiagnostic.cs` | EDITADO — código novo `DiagnosticCodes.InvalidClassName`. |
| `modded/Server/Web/Shared/ClassLifecycleCreateDialog.razor` | NOVO — input + validação live + aviso "nasce sem ícone" + diagnostics; `Ok(fileName)`. |
| `modded/Server/Web/Shared/ClassLifecycleDuplicateDialog.razor` | NOVO — novo nome p/ cópia verbatim; `Ok(newFileName)`. |
| `modded/Server/Web/Shared/ClassLifecycleDeleteDialog.razor` | NOVO — varredura de perfis (`Task.Run`) + aviso forte + Delete file / Disable instead / Cancel; `Ok("deleted"|"disabled")`. Disable = `Save(enabled:false, hotApply:true)` — o próprio Save hot-aplica como `Remove` (021), sem chamada extra. |
| `modded/Server/Web/Pages/Classes.razor` | EDITADO — toolbar "New class", coluna Actions (stopPropagation vs. row-click), `LoadRows()` p/ reload, handlers (create→edit, duplicate→detail, delete/disable→reload+snackbar). |
| `modded/Server/Web/Pages/ClassDetail.razor` | EDITADO — botões Duplicate/Delete no header (mesmos dialogs), `Reload()` extraído de `OnParametersSet`, injects `IDialogService`/`ISnackbar`/`NavigationManager`. |

Não tocados (território do 026 em paralelo): `Web/Shared/ItemSpec*.razor`, `Web/Pages/ClassEdit.razor`, `Web/ClassEditModel.cs`, `CatalogService.cs`.

## Decisões

- Colisão de nome **case-insensitive** e incluindo classes só-em-arquivo — mais estrito que o registrar (ordinal), pré-validação de UX; o `Save` continua sendo a autoridade final.
- Duplicar preserva `enabled` do fonte (cópia fiel); duplicata de classe desabilitada não registra (warning inócuo do `Remove`).
- Match de edition nos perfis é **ordinal** (launcher grava verbatim — evidência no spec-tech, 36 perfis reais de `D:/SPT/SPT/user/profiles`).
- Sem rota HTTP nova — tudo via DI nos componentes Blazor (mesmo padrão do 024/025).

## Build / validação

- **NÃO buildado aqui** — exclusividade de `dotnet build` do item 026; o orquestrador roda o build integrado depois. Símbolos verificados manualmente contra os fontes reais (`ClassEditorService`/`ClassRegistrar`/`FileUtil`/padrões MudBlazor 8 do 023: `IMudDialogInstance`, `DialogParameters<T>`, `ShowAsync(title, parameters, options)`).
- Evidência do campo de perfil colhida do install real (script Python sobre os 36 `*.json`).

## Pendências (orquestrador)

- [ ] `dotnet build` integrado (026+027) + `compile-mod.sh CustomClasses`.
- [ ] Smoke no server real: criar classe → launcher sem restart → editar → duplicar → deletar (perfil de teste usando a edition pra ver o aviso) → disable.
- [ ] Pós-smoke: `sync-classes` (arquivos novos/criados no install precisam voltar pro repo).
