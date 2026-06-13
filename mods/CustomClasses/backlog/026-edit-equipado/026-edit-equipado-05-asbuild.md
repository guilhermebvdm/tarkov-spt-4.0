# 026 — Editor de loadout equipado — As-built

**Mod:** CustomClasses
**Data:** 2026-06-10
**Refs:** [01-spec](./026-edit-equipado-01-spec.md) · [02-spec-tech](./026-edit-equipado-02-spec-tech.md)

## Arquivos entregues

| Arquivo | Conteúdo |
|---|---|
| `modded/Server/CatalogService.cs` | EDITADO — seção "Slots (item 026)", read-only: `CatalogSlotInfo`, `DefaultInventoryTpl` (`55d7217a4bdc2d86028b456d` — slots de equipamento do personagem, `_name` == enum `EquipmentSlots`, evidência no XML doc), `GetSlotsOf`, `GetSlotFilter`, `IsAllowedInSlot` (filter contém tpl OU `ItemHelper.IsOfBaseclass` por entrada; **leniente** quando não consegue avaliar), `GetEquipmentSlotFilter`/`IsAllowedInEquipmentSlot`, `TemplateExists`, `HasGrids`, `IsWeapon`. APIs string-cru com parse guardado (`MongoId.IsValidMongoId`). |
| `modded/Server/Web/ClassEditModel.cs` | EDITADO — `ItemSpecModel`/`ModSpecModel` (espelhos mutáveis dos records, `FromSpec`/`ToSpec`, vazios→null), `EquippedSlotRow`; `Equipped` editável no `ClassEditModel`; `Stash` segue **pass-through intacto** (028); `BuildLoadout()` no `ToDefinition`. |
| `modded/Server/Web/Shared/ItemSpecEditor.razor` | NOVO — editor recursivo de 1 `ItemSpec` (028 reusa): modos Item/Preset (toggle limpa o modo abandonado), pickers do 023 (ItemPicker em dialog c/ `FilterTpls`; Preset/Ammo inline em `MudCollapse`), switch premium, seção ammo (switches loadedMag/chambered desabilitados sem ammo + hint PA-01-03; clear reseta), árvore de mods recursiva por `_props.Slots` (chip required, Set/Replace filtrado pelo slot filter, órfãos com warning), contents recursivo (`AllowCount=true`) p/ templates com grids, chips `unresolved`/`not allowed in slot` (warning-only), `MaxDepth=6`. |
| `modded/Server/Web/Pages/ClassEdit.razor` | EDITADO — aba Equipped real (card por slot: header + remover c/ confirmação `ShowMessageBox` + `ItemSpecEditor` c/ `TplFilter` do slot), "Add slot" (enum menos usados), `RecomputeLoadoutCost` on-change (load, add/remove, todo `OnChanged`), label "Loadout total", placeholder do Stash lê `_model.Stash`, inject `IDialogService`. |

Não tocados (território do 027): `Web/Pages/Classes.razor`, `ClassEditorService.cs`, diálogos `ClassLifecycle*`.

## Build

- `dotnet build -c Release` — **0 erros, 0 warnings** (1 warning CS8625 na primeira tentativa, corrigido: cast `(MongoId?)null`).
- `compile-mod.sh CustomClasses` — instalado em `D:/SPT`; guard de config: "install matches repo — no divergence".

## Evidências (server real SPT 4.0 + browser via Chrome DevTools MCP)

Server up, **`Loaded 11 class(es), skipped 0`**. Página `https://…:6969/customclasses/classes/cacador/edit`:

- **curl GET → HTTP 200** (36 KB); labels `Equipped (6)` / `Stash (27)` no HTML (contagens vêm do novo modelo). Conteúdo do painel não aparece via curl — MudTabs só pré-renderiza a aba ativa → validação de conteúdo feita no **browser real** (circuito SignalR vivo):
- **Aba Equipped renderizada:** 6 cards (FirstPrimaryWeapon SV-98 preset premium + ammo LPS + loadedMag/chambered ON; Holster PM default + loadedMag; ArmorVest 6B2 c/ 2 slots de mod `required`; Headwear LShZ c/ 6 slots; TacticalVest WARTECH c/ 4 slots + Contents; Backpack Pilgrim + Contents). Screenshot: `evidence-equipped-tab.png`.
- **Custo on-change:** remover Backpack (diálogo de confirmação OK) → `Equipped (6)→(5)`, total **4.770.122 → 4.651.624 ₽**; Discard restaurou tudo (6 slots, custo e switches originais).
- **Save real (DoD do kickoff):** toggle `premium` no Holster → Save → snackbar "Class saved and hot-applied" + banner de limites; no disco: `cacador.jsonc` reescrito com `"premium": true` no Holster, **27 linhas de stash intactas** (pass-through), `cacador.jsonc.bak1` criado, linha no `_audit.log`, hot-apply re-registrou 'Caçador' no log do server.
- Pós-teste: install restaurado do repo (`md5` idêntico), `.bak1` de teste removido — **repo==install ao final**. Server finalizado (`Stop-Process`); porta 6969 livre.

Nota de cert: o MCP Chrome (`--isolated`) bloqueia o cert self-signed do server (`ERR_CERT_AUTHORITY_INVALID`) — bypass via `thisisunsafe` no interstitial funcionou.

## Observações / pendências

- [ ] Console do browser tem 1 erro pré-existente, **não relacionado ao 026**: `Identifier 'MudPointerEventsNone' has already been declared` (script do MudBlazor carregado 2× — infra do 020; circuito funciona normalmente). Candidato a item de housekeeping/029.
- [ ] Validar em jogo que um perfil novo nasce com loadout editado (a cadeia editor→arquivo→hot-apply→`InventoryBuilder` está coberta; o passo launcher→jogo segue manual — memória `feedback_spt_validation`).
- Contents de contêiner não filtram pelo filter das grades (decisão registrada na spec-tech — warning fica pro jogo/dry-run).
- `count > 1` em equipado: UI não mostra o campo (`AllowCount=false`) — espelha PA-02-05.

## Pós-review (2026-06-10, CR-EP-01)

O code review do épico ([epico-editor-04-code-review-01.md](../029-docs-e-fechamento/epico-editor-04-code-review-01.md)) estendeu o `InventoryBuilder.PackSpecsIntoGrids`: linhas de **stash/contents agora honram `preset`/`mods`/`ammo`/`contents`** (mesma semântica do slot equipado). O `ItemSpecEditor` completo usado em contents (e no stash do 028) deixou de expor campos que o builder ignorava — paridade UI×builder×custo restaurada. `FilterTpls` do `ItemPicker` agora roda DENTRO do `CatalogService.Search`, antes do cap (CR-EP-07).
