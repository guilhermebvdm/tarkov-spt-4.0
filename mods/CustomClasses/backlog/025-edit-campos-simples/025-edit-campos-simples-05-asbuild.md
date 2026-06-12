# 025 — Edição de campos simples + outfit — As-built

**Mod:** CustomClasses
**Data:** 2026-06-10
**Refs:** [01-spec](./025-edit-campos-simples-01-spec.md) · [02-spec-tech](./025-edit-campos-simples-02-spec-tech.md)

## Arquivos entregues

| Arquivo | Conteúdo |
|---|---|
| `modded/Server/Web/ClassEditModel.cs` | NOVO — view-model mutável (`ClassDefinition` é record `init`-only): `FromDefinition`/`ToDefinition`, rows `SkillLevelRow`/`SkillFactorRow`/`HideoutRow`, outfit em 4 strings, `Loadout` pass-through (026/028), vazios → null no round-trip. |
| `modded/Server/Web/Pages/ClassEdit.razor` | NOVO — `@page "/customclasses/classes/{FileName}/edit"` (resolução sem extensão = ClassDetail). Shell `MudTabs`: General (name read-only+tooltip, displayName/description en-pt, nameColor hex+swatch+validação, enabled switch, baseEdition vanilla-only via `ClassVisualRegistry`, iconFile enumerado do install via `ModHelper`+preview+aviso de degradação), Skills (níveis 0–51, peso/origem/custo por linha ao vivo, add de TODO o enum `SkillTypes` alfabético, total+budget+warnings), Multipliers (double ≥0 step 0.1, verde/vermelho, badge SE + alert se SE ausente), Hideout (`HideoutAreas` sem `NotSet`, nível ≥1), Outfit (4× `CustomizationPicker` 023 + nome resolvido + clear), Equipped/Stash placeholders (026/028). Toolbar sticky: Save/Discard + custo de skills ao vivo + loadout ₽. Save = `ClassEditorService.Save(file, def, hotApply:true)` em `Task.Run`; sucesso → snackbar + banner dos limites do hot-apply; falha → diagnostics `[Code] Message`, nada salvo. Discard recarrega do disco. |
| `modded/Server/Web/Pages/ClassDetail.razor` | EDITADO — botão "Edit" no header → `/customclasses/classes/{bare}/edit`. |

Não tocados: `ClassEditorService.cs`, `ClassRegistrar.cs`, `CostService.cs`, `CatalogService.cs`, builders, registries, csproj, pickers do 023.

## Build

- `dotnet build -c Release` — **0 erros, 0 warnings** (primeira tentativa).
- `compile-mod.sh CustomClasses` — instalado em `D:/SPT` sem disparar o guard de config ("install matches repo — no divergence").

## Evidências (server real, SPT 4.0.13, 11 classes registradas)

- `GET /customclasses/classes/cacador/edit` → **HTTP 200** (36 KB pré-renderizados): título "Edit — Caçador", 7 abas (General/Skills/Multipliers/Hideout/Outfit/Equipped/Stash), toolbar com Save/Discard/"Weighted skill cost"/"Loadout total", 4 referências a `cacador.png` (header/preview/select).
- `GET /customclasses/classes/cacador` → 200 com link `classes/cacador/edit` (botão Edit).
- `GET /customclasses/classes/peladao/edit` → 200, chip "no skills" (classe vazia OK).
- `GET /customclasses/classes/naoexiste/edit` → 200 com alert "No editable class file matching…" (caminho de erro OK).
- **Fluxo de divergência (teste mais valioso sem browser):** edição manual de `displayName.en`/`description.en` no `cacador.jsonc` do INSTALL → `sync-classes.sh --dry-run` detectou "1 changed" com o diff exato → install restaurado → dry-run voltou a "already in sync". repo==install ao final.
- Server finalizado; porta 6969 livre (verificado via `Get-NetTCPConnection`).

## Pendências (validar no browser — curl não executa o circuito SignalR/Blazor)

- [ ] **Save-roundtrip real**: editar um campo na UI → Save → conferir `.bak1` + arquivo reescrito + snackbar/banner → perfil novo no launcher reflete sem restart (DoD do kickoff).
- [ ] Save inválido (ex.: `baseEdition` apontado pra edition inexistente via arquivo) bloqueia com diagnostics na UI.
- [ ] Interações: add/remove de skill recalcula custo; pickers de outfit selecionam/limpam; validação visual do nameColor.
- [ ] Lembrete pós-save no browser: rodar `sync-classes --yes` antes de qualquer rebuild (comentários JSONC são perdidos no re-serialize — caveat do 021; `.bak1` preserva).
