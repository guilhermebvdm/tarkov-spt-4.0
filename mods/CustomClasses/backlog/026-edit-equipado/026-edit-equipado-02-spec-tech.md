# 026 — Editor de loadout equipado — Spec técnica

**Mod:** CustomClasses
**Criado:** 2026-06-10
**Refs:** [01-spec](./026-edit-equipado-01-spec.md) · [018 class-schema.md §3] · [022 CatalogService] · [023 pickers] · [025 ClassEditModel/ClassEdit]

## Estratégia de slot-filter (com evidência do DB)

**Decisão: um único mecanismo para slots de equipamento E slots de mod — `_props.Slots[]._props.filters[0].Filter` do DB vivo.**

Evidência (SPT 4.0, `spt-source/Libraries/SPTarkov.Server.Assets/SPT_Data/database/templates/items.json`): o template **"Default Inventory"** (`55d7217a4bdc2d86028b456d`, o personagem-como-item) declara os slots de equipamento em `_props.Slots`, com `_name` **idêntico** aos nomes do enum `EquipmentSlots` e filter por baseclass:

| Slot (`_name`) | filter | exemplo de entrada |
|---|---|---|
| FirstPrimaryWeapon / SecondPrimaryWeapon | 25 entradas | `5447b5fc4bdc2d87278b4567` (AssaultCarbine) … |
| Holster | 6 | `5447b5cf4bdc2d65278b4567` (Pistol) … |
| TacticalVest | 1 | `5448e5284bdc2dcb718b4567` (= baseclass VEST) |
| ArmorVest / Backpack / Headwear / … | 1 cada | baseclass correspondente |

O fallback pragmático por categoria handbook (cogitado no kickoff) **não foi necessário** — o template cobre os 14 slots do enum (há um 15º slot `Dogtag` no template, fora do enum e fora do editor).

**Resolução de herança:** candidato permitido ⇔ `filter.Contains(tpl)` **ou** `ItemHelper.IsOfBaseclass(tpl, entrada)` para alguma entrada do filter (mesmo padrão de baseclass-check do código existente — `InventoryBuilder`/`CatalogService`).

**Leniência (decisão):** `IsAllowedInSlot` retorna `true` quando **não consegue avaliar** (id malformado, template pai desconhecido/moddado) — só responde `false` quando o slot existe e o filter exclui o candidato. Warning-grade por design (kickoff: espelhar o loader); o dry-run do Save é a palavra final. Filter declarado mas vazio (raro) → permitido.

## `CatalogService` — seção "Slots (item 026)" (read-only)

| Membro | Contrato |
|---|---|
| `const DefaultInventoryTpl` | `55d7217a4bdc2d86028b456d` (evidência acima, documentada no XML doc) |
| `GetSlotsOf(tpl)` | `List<CatalogSlotInfo>` (`Id` = `_name`, `Required` = `_required`); desconhecido → vazio |
| `GetSlotFilter(parentTpl, slotId)` | `filters[0].Filter` como `List<string>`; slot/tpl ausente → vazio |
| `IsAllowedInSlot(parentTpl, slotId, tpl)` | regra de herança + leniência acima; match de slot case-insensitive (consistente com o parse do `InventoryBuilder`) |
| `GetEquipmentSlotFilter(slot)` / `IsAllowedInEquipmentSlot(slot, tpl)` | delegam com `DefaultInventoryTpl` |
| `TemplateExists(tpl)` / `HasGrids(tpl)` / `IsWeapon(tpl)` | suporte do editor (unresolved / seção contents / seção ammo + restrição do picker de armas) |

APIs em `string` cru (razor-friendly): parse guardado por `MongoId.IsValidMongoId` — nunca lança.

## View-model (`Web/ClassEditModel.cs`)

- **`ItemSpecModel` / `ModSpecModel`** — espelhos mutáveis dos records `ItemSpec`/`ModSpec` (init-only), com `FromSpec`/`ToSpec`; vazios → `null` no round-trip (arquivo salvo próximo do manual). `Count` clampa em ≥1.
- **`EquippedSlotRow`** (`Slot` + `ItemSpecModel`) — `ClassEditModel.Equipped` substitui o pass-through do `Loadout`; ordem do arquivo preservada (diffs mínimos), slots novos no fim.
- **`Stash` continua pass-through** (referência intacta de `def.Loadout?.Stash` — item 028). `BuildLoadout()`: equipped rows → dict (`TryAdd`, primeira vence — UI só oferece slots livres) + stash as-is; ambos vazios → `loadout` omitido.

## `Web/Shared/ItemSpecEditor.razor` — contratos

| Parâmetro | Significado |
|---|---|
| `Spec` (required) | `ItemSpecModel` editado **in place** |
| `TplFilter` (`Func<string,bool>?`) | restrição do picker da RAIZ (filter do slot de equipamento); null = livre |
| `AllowCount` (default false) | mostra `count` — equipado não; contents/stash (028) sim |
| `Depth` | 0 na raiz; contents aninham `Depth+1` (cap `MaxDepth=6` + indentação) |
| `OnChanged` (`EventCallback`) | borbulha TODA mutação (própria ou aninhada) — página recalcula custo |

Decisões internas:

- **Estado derivado cacheado** (`Refresh()`): `Spec` é mutado in place, então `OnParametersSet` sozinho não cobre as mutações dos próprios handlers — `NotifyChangedAsync` = `Refresh()` + `OnChanged`. Evita `GetPresetsFor`/`GetSlotsOf` repetidos por render.
- **Tpl raiz efetivo:** modo item = `Spec.Tpl`; modo preset = root do preset resolvido por `Catalog.ResolveDefaultPreset` (aceita preset-id OU tpl de arma — mesma resolução do `InventoryBuilder`). É a base de ammo (calibre), contents (grids) e do warning de slot.
- **Pickers em dialog** (`IDialogService.ShowAsync<ItemPicker>` + `FilterTpls`, padrão do PickerTest 023) — evita N pickers inline pesados; Preset/Ammo pickers ficam inline em `MudCollapse`.
- **Árvore de mods = `RenderFragment` recursivo** (`ModSlotRows(parentTpl, mods, depth)`), só no modo item (precedência do builder: `preset > mods > tpl` — mods com preset seriam ignorados). Trocar a raiz limpa `Mods` (árvore pertencia ao template anterior) e o trio ammo (calibre pode mudar). Trocar um mod limpa os sub-mods dele.
- **Troca de modo limpa o modo abandonado** (tpl+mods ou preset+premium) — sem estado morto persistido no arquivo.

## Aba Equipped (`Web/Pages/ClassEdit.razor`)

- Card por row de `_model.Equipped` (`@key=row`): header (nome + remover com `DialogService.ShowMessageBox`) + `ItemSpecEditor` com `TplFilter = t => Catalog.IsAllowedInEquipmentSlot(row.Slot, t)` e `OnChanged = RecomputeLoadoutCost`.
- "Add slot": `Enum.GetNames<EquipmentSlots>()` (ordem do enum — layout do personagem) menos os usados.
- `RecomputeLoadoutCost()` = `CostService.ComputeLoadoutCost(_model.ToDefinition())` — chamado no load, em add/remove de slot e em todo `OnChanged` (síncrono: walk do DB é barato pro caso local).
- Toolbar: label "Loadout total" (era "read-only here"); placeholder do Stash agora lê `_model.Stash`.

## Limites

- `MaxDepth = 6` (contents e mods) — além disso, conteúdo é preservado no save mas não editável na UI.
- Contents de contêiner sem filtro de grade (grids têm filters próprios no EFT; não aplicado — pragmático, warning fica pro dry-run/jogo).
- Compatibilidade de slot de equipamento NÃO é validada pelo `InventoryBuilder` no build real (ele equipa o que vier) — o warning da UI é a única sinalização antes do jogo.

## Arquivos

| Arquivo | Ação |
|---|---|
| `modded/Server/CatalogService.cs` | EDITADO — seção "Slots (item 026)" + record `CatalogSlotInfo` (read-only) |
| `modded/Server/Web/ClassEditModel.cs` | EDITADO — `ItemSpecModel`/`ModSpecModel`/`EquippedSlotRow`; `Equipped` editável; `Stash` pass-through; `BuildLoadout()` |
| `modded/Server/Web/Shared/ItemSpecEditor.razor` | NOVO — editor recursivo compartilhado (028 reusa) |
| `modded/Server/Web/Pages/ClassEdit.razor` | EDITADO — aba Equipped real; custo on-change; inject `IDialogService` |
