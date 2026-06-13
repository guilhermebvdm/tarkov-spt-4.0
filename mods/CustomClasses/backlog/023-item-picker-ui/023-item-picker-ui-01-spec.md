# 023 — Pickers de item (MudBlazor) — Spec

**Mod:** CustomClasses
**Status:** Implementado (validação visual pendente — ver as-built)
**Criado:** 2026-06-10
**Origem:** [023-item-picker-ui-00-kickoff.md](./023-item-picker-ui-00-kickoff.md)

## Visão geral

Componentes de seleção reutilizáveis em `Web/Shared/` (consumidos por 025/026/027/028), sobre o `CatalogService` (022). Sem persistência — só seleção via callback.

## Comportamento desejado

- **`ItemPicker`**: busca de item com debounce (300ms), filtro opcional por categoria do handbook (dropdown em árvore indentada), predicado opcional `FilterTpls` (p/ o 026 restringir por slot compatível). Resultados virtualizados (MudVirtualize) com thumb 32px (tarkov.dev, fallback texto), nome (locale en), shortname/tpl, categoria e preço ₽. Funciona **inline** e como **MudDialog** (via `IDialogService`) — em modo diálogo, seleção fecha com `DialogResult.Ok(tpl)`.
- **`PresetPicker`**: recebe `WeaponTpl`; lista presets de `GetPresetsFor` com nome, nº de itens e chips **Default**/**Premium** (os mesmos que o `InventoryBuilder` resolve). Clique → callback com id do preset.
- **`AmmoPicker`**: recebe `WeaponTpl` ou `Caliber`; lista cartuchos do calibre da arma (match `_props.ammoCaliber` da arma × `_props.Caliber` da munição, baseclass AMMO), ordenados por penetração desc, com dano/pen/preço. Clique → callback com tpl do cartucho.
- **`CustomizationPicker`**: recebe `Side` (Usec/Bear) e `SlotKind` (Upper/Lower); lista roupas de `GetClothing` (mesmas regras do `OutfitBuilder`) com filtro por texto e lista virtualizada. Clique → callback com id da customization.
- **Página de teste** `/customclasses/picker-test`: exercita os 4 componentes isolados (sem link no NavMenu; remoção é decisão do 029).

## Critérios de aceite

- [x] Os 4 componentes compilam e são exercitáveis isolados na página de teste.
- [x] Busca não bloqueia o circuit (debounce + `Task.Run` + limite de resultados + virtualização).
- [x] ItemPicker abrível via `IDialogService` E utilizável inline (mesmo componente).
- [x] Decisão de imagem de item registrada na spec-tech com evidência.
- [x] `dotnet build -c Release` com 0 erros / 0 warnings.
- [ ] Validação visual no server rodando (pendência no as-built).

## Fora de escopo

- Persistência/edição de classe (025–028).
- Filtro real de compatibilidade por slot (o 026 passa o predicado; aqui só o hook `FilterTpls`).
- Link no NavMenu (NavMenu é território do 024/029).
