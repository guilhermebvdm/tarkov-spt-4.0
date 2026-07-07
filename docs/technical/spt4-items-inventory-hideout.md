---
title: Estrutura de Itens, Inventário e Hideout no SPT 4.0
date: 2026-06-07
status: 🟢 Vivo
authors: Guilherme
---

# Estrutura de Itens, Inventário e Hideout no SPT 4.0

> **Doc canônica.** Sempre que for mexer com **itens, equipamento, inventário, contêineres, armas compostas, presets ou hideout** (server ou client), consulte este documento. As skills `spt-mod-best-practices` e `csharp-mod-best-practices` referenciam este arquivo.
>
> Evidência do servidor: [references/spt-source/](../../references/spt-source/). Cada afirmação cita `arquivo.cs:linha`.

---

## 1. Modelo de item — lista flat + árvore por `parentId`/`slotId`

O inventário de um perfil é uma **lista flat** de itens (`Inventory.Items`); a hierarquia (arma com mods, rig com conteúdo) é expressa por **referências de pai/slot**, não por aninhamento. Cada item ([Item.cs:8](../../references/spt-source/Libraries/SPTarkov.Server.Core/Models/Eft/Common/Tables/Item.cs#L8)):

| Campo (JSON) | C# | Significado |
|---|---|---|
| `_id` | `Id` (`MongoId`, **required**) | ID **único da instância** (24-hex). Não é o tipo do item. |
| `_tpl` | `Template` (`MongoId`) | ID do **template** (o "tipo" do item no DB). |
| `parentId` | `ParentId` (`string?`) | `_id` do item-pai (contêiner/arma/equipment root). `null` só para a raiz. |
| `slotId` | `SlotId` (`string?`) | **onde** no pai: nome do slot de equipamento, do mod, da grade, etc. |
| `location` | `Location` (`object?`) | posição: `ItemLocation {x,y,r}` em grades; ausente em slots simples. |
| `upd` | `Upd?` | estado: `StackObjectsCount`, durabilidade, etc. |

`MongoId ↔ string` tem conversão implícita (visto em [CreateProfileService.cs](../../references/spt-source/Libraries/SPTarkov.Server.Core/Services/CreateProfileService.cs)); `new MongoId()` gera um id novo; `new MongoId("...")` cria a partir de string.

## 2. Raízes do inventário

[Inventory (BotBase.cs)](../../references/spt-source/Libraries/SPTarkov.Server.Core/Models/Eft/Common/Tables/BotBase.cs#L368): `Items` (lista flat), `Equipment` (`MongoId?` — raiz do equipamento), `Stash` (`MongoId?` — raiz do stash), além de `QuestRaidItems`, `SortingTable`, `HideoutCustomizationStashId`.

- **Equipado:** `parentId = Inventory.Equipment`, `slotId = <EquipmentSlots>`.
- **Stash (solto):** `parentId = Inventory.Stash`, `slotId = "hideout"` ([InventoryHelper.cs:367](../../references/spt-source/Libraries/SPTarkov.Server.Core/Helpers/InventoryHelper.cs#L367)), `location = {x,y,r}`.

## 3. Slots de equipamento (`EquipmentSlots`)

[EquipmentSlots.cs](../../references/spt-source/Libraries/SPTarkov.Server.Core/Models/Enums/EquipmentSlots.cs#L3) — o `slotId` do item equipado é **exatamente** o nome do enum:

`Headwear · Earpiece · FaceCover · ArmorVest · Eyewear · ArmBand · TacticalVest · Pockets · Backpack · SecuredContainer · FirstPrimaryWeapon · SecondPrimaryWeapon · Holster · Scabbard`

> Slots simples (não-grade) **não** usam `location`. `Pockets`/`SecuredContainer` normalmente já existem no template base — cuidado para não duplicar.

## 4. Contêineres (rig, mochila) — grades

Um contêiner tem **grades** definidas no template ([TemplateItem.cs:353 `Grids`](../../references/spt-source/Libraries/SPTarkov.Server.Core/Models/Eft/Common/Tables/TemplateItem.cs#L353)). Itens dentro dele:
- `parentId = <_id do contêiner>`
- `slotId = <nome da grade>` — geralmente `"main"` ([ItemHelper.cs:1135](../../references/spt-source/Libraries/SPTarkov.Server.Core/Helpers/ItemHelper.cs#L1135)); contêineres maiores podem ter várias grades nomeadas.
- `location = {x, y, r}` na grade.

Tamanho do item: `_props.Width`/`_props.Height` ([TemplateItem.cs:124/128](../../references/spt-source/Libraries/SPTarkov.Server.Core/Models/Eft/Common/Tables/TemplateItem.cs#L124)). Tamanho da grade: `Grid` (cells H×V). O packing precisa casar dims do item com a grade (first-fit + rotação).

## 5. `location` — `{x, y, r}` e rotação

[ItemLocation (Item.cs:81)](../../references/spt-source/Libraries/SPTarkov.Server.Core/Models/Eft/Common/Tables/Item.cs#L81): `X` (int), `Y` (int), `R` (`ItemRotation`), `IsSearched` (bool). [`ItemRotation`](../../references/spt-source/Libraries/SPTarkov.Server.Core/Models/Eft/Common/Tables/Item.cs#L103) = `Horizontal | Vertical`.

- `x`/`y`: célula superior-esquerda do item na grade (0-based).
- `r = Vertical`: item rotacionado (troca W↔H).
- Em **slot simples** (equipamento, mods), `location` é omitido.

## 6. Armas compostas (arma + mods + munição)

- **Mods:** `parentId = <_id da arma/mod-pai>`, `slotId = "mod_magazine" | "mod_scope" | "mod_muzzle" | "mod_stock" | "mod_pistol_grip" | …` (definidos em `_props.Slots` do template — [TemplateItem.cs:357](../../references/spt-source/Libraries/SPTarkov.Server.Core/Models/Eft/Common/Tables/TemplateItem.cs#L357)).
- **Carregador carregado:** o cartucho é **filho do carregador**: `parentId = <_id do mag>`, `slotId = "cartridges"`, `upd.StackObjectsCount = N`. Capacidade do mag: `_props.Cartridges[0]._max_count` ([TemplateItem.cs:560/1733](../../references/spt-source/Libraries/SPTarkov.Server.Core/Models/Eft/Common/Tables/TemplateItem.cs#L560)).
- **Bala na câmara:** `parentId = <_id da arma>`, `slotId = "patron_in_weapon"` (algumas armas usam `patron_in_weapon_000`/`_001`) — [BotWeaponGenerator.cs:213/245](../../references/spt-source/Libraries/SPTarkov.Server.Core/Generators/BotWeaponGenerator.cs#L213).
- **Presets (atalho):** em vez de montar a árvore à mão, use [PresetHelper](../../references/spt-source/Libraries/SPTarkov.Server.Core/Helpers/PresetHelper.cs#L124): `GetPreset(id)` (preset específico) ou `GetDefaultPreset(tpl)` (preset default da arma). O `Preset` ([Globals.cs:4393](../../references/spt-source/Libraries/SPTarkov.Server.Core/Models/Eft/Common/Globals.cs#L4393)) traz `Items` (a árvore pronta) e `Parent` (tpl da raiz). Clone `Preset.Items`, **re-id** e re-raiz no slot.

## 7. Armadura com placas

Placas são filhas da armadura: `parentId = <_id da armadura>`, `slotId = "front_plate" | "back_plate" | "left_side_plate" | "right_side_plate" | "soft_armor_*"` (conforme `_props.Slots` do template da armadura).

## 8. `Upd` — estado do item

[Upd (Item.cs:112)](../../references/spt-source/Libraries/SPTarkov.Server.Core/Models/Eft/Common/Tables/Item.cs#L112): `StackObjectsCount` (quantidade da pilha — [:133](../../references/spt-source/Libraries/SPTarkov.Server.Core/Models/Eft/Common/Tables/Item.cs#L133)), `Repairable` (durabilidade), `FireMode`, `Foldable`, etc.

- **Stack-aware:** itens empilháveis com `Count > stackMax` viram **N entradas** (cada uma `StackObjectsCount ≤ stackMax`). `stackMax` por tpl: tarkov-itemdb / `_props.StackMaxSize`.

## 9. IDs únicos e re-id no perfil

Todos os `_id` devem ser **únicos dentro do perfil**. Ao construir um **template de perfil**, basta serem únicos no template: o servidor **re-id** tudo na criação do perfil ([CreateProfileService.cs:94 `ReplaceProfileInventoryIds`](../../references/spt-source/Libraries/SPTarkov.Server.Core/Services/CreateProfileService.cs#L94)). Ao clonar uma árvore (ex.: preset), construa um mapa `oldId→newId` e reescreva `Id` **e** `ParentId` por ele, senão os filhos ficam órfãos.

## 10. Hideout

[PmcData.Hideout (BotBase.cs:706)](../../references/spt-source/Libraries/SPTarkov.Server.Core/Models/Eft/Common/Tables/BotBase.cs#L706) → `Areas` (`List<BotHideoutArea>`). [BotHideoutArea (BotBase.cs:828)](../../references/spt-source/Libraries/SPTarkov.Server.Core/Models/Eft/Common/Tables/BotBase.cs#L828): `Type` (`HideoutAreas`), `Level` (`int?`), `Active`, `CompleteTime`, `Constructing`.

- Setar nível: achar a área por `Type` e ajustar `Level` (lembre: `int?` → `Level ?? 0`).
- [`HideoutAreas`](../../references/spt-source/Libraries/SPTarkov.Server.Core/Models/Enums/Hideout/HideoutAreas.cs) (namespace `...Enums.Hideout`): `Stash, Generator, Heating, Security, Workbench, MedStation, RestSpace, WaterCollector, Vents, IntelligenceCenter, ShootingRange, Library, ScavCase, …`.
- **Pré-requisitos:** algumas estações dependem de outras (ex.: ShootingRange/IntelligenceCenter). Dar nível a uma estação com pré-requisito não-atendido pode ficar inconsistente — preferir estações sem pré-requisito (lição do RZCustomProfiles).

## 11. Helpers do servidor (reusar — não reinventar)

- [`PresetHelper`](../../references/spt-source/Libraries/SPTarkov.Server.Core/Helpers/PresetHelper.cs) — presets de arma/armadura.
- [`ItemHelper`](../../references/spt-source/Libraries/SPTarkov.Server.Core/Helpers/ItemHelper.cs) — templates, baseclass, dims, helpers de árvore.
- [`InventoryHelper`](../../references/spt-source/Libraries/SPTarkov.Server.Core/Helpers/InventoryHelper.cs) — adicionar itens, encontrar slot livre, mapa da grade (placement no stash, slot default `"hideout"`).
- `ICloner` — **clone profundo** antes de mutar templates compartilhados.

## 12. Pegadinhas

- `_tpl` ≠ `_id` — confundir quebra tudo.
- Esquecer de re-id ao clonar → filhos órfãos / colisão.
- `slotId` errado (ex.: `"main"` onde devia ser `"cartridges"`) → item não aparece / inválido.
- `location` faltando em item de grade → sobreposição / sumiço.
- `Location` é `object?` (pode ser `ItemLocation` **ou** número) — em grades, use `ItemLocation`.
- `HideoutArea.Level` é `int?` — null-check.
- `Inventory.Equipment/Stash/Items` são nullable — checar antes de usar.

## Histórico de Alterações

| Data | Autor | Alteração |
|---|---|---|
| 2026-06-07 | Guilherme | Criação — estrutura de item (_id/_tpl/parentId/slotId/location/upd), equipamento, contêineres/grades, armas compostas/presets/munição, hideout, helpers e pegadinhas. Motivada pelo item 003 do mod CustomClasses. |
| 2026-06-08 | Guilherme | build: extend compile-mod for hybrid C# mods + ignore build artifacts |
| 2026-07-06 | Guilherme | chore(launcher): remove empty placeholder diff.txt |
