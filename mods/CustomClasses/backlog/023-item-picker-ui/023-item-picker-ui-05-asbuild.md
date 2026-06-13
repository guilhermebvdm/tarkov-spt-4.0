# 023 — Pickers de item (MudBlazor) — As-built

**Mod:** CustomClasses
**Criado:** 2026-06-10
**Origem:** [023-item-picker-ui-02-spec-tech.md](./023-item-picker-ui-02-spec-tech.md)

## Arquivos

| Arquivo | Ação | Conteúdo |
|---|---|---|
| `modded/Server/Web/Shared/ItemPicker.razor` | novo | busca debounced + categoria + `FilterTpls`, virtualizado, dual inline/diálogo, thumb tarkov.dev c/ fallback |
| `modded/Server/Web/Shared/PresetPicker.razor` | novo | presets por tpl com chips Default/Premium |
| `modded/Server/Web/Shared/AmmoPicker.razor` | novo | cartuchos por calibre da arma (dmg/pen/preço) |
| `modded/Server/Web/Shared/CustomizationPicker.razor` | novo | roupas upper/lower por facção, filtro + virtualize |
| `modded/Server/Web/Pages/PickerTest.razor` | novo | `@page "/customclasses/picker-test"` — harness dos 4 pickers |
| `modded/Server/CatalogService.cs` | editado | + `CatalogAmmo`, `GetCaliber`, `GetAmmoForWeapon`, `GetAmmoByCaliber` (seção "Ammo (item 023)") |

Não tocados (território do 024): `Web/Shared/NavMenu.razor`, `Web/Layouts/BaseLayout.razor`, `Web/Pages/Home.razor`, `Web/Pages/Classes*.razor`.

## Build

`dotnet build -c Release` (e `--no-incremental`) em `modded/Server/`: **0 erros, 0 warnings**. Rotas/tipos novos confirmados no binário (`CustomClasses-Server.dll` contém `picker-test`, `PickerTest`, `ItemPicker`). Nenhum erro observado em arquivos do item 024 (ainda não presentes no momento deste build).

## Como testar

1. Deploy do mod (fluxo padrão do repo) e server SPT no ar.
2. Abrir `http://127.0.0.1:6969/customclasses/picker-test` (sem link no NavMenu — URL direta).
3. **ItemPicker inline:** digitar "ak-74" → lista com thumb/nome/preço; selecionar → tpl aparece em "Selected". Trocar categoria no dropdown e repetir.
4. **Dialog:** "Open picker dialog" → mesmo picker em MudDialog; selecionar fecha e mostra o tpl. Botão "FilterTpls" demonstra o predicado (só tpls começando com `5a`).
5. **PresetPicker/AmmoPicker:** com o tpl default (M4A1) conferir chips Default/Premium e a lista 5.56x45 ordenada por pen; trocar o tpl de arma e ver ambos recarregarem. Tpl inválido/incompleto não pode derrubar a página.
6. **CustomizationPicker:** alternar Usec/Bear e Upper/Lower; filtrar por texto; selecionar.
7. Offline (ou tpl de mod terceiro): thumbs somem, linhas continuam utilizáveis (fallback texto).

## Pendências

- Validação visual no server rodando (build do deploy integrado é do orquestrador da wave).
- Remoção/manutenção da página `picker-test` — decisão do 029.
