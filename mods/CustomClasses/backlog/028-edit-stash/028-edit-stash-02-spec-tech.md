# 028 — Editor de inventário (stash) — Spec técnica

**Mod:** CustomClasses
**Status:** Implementado
**Criado:** 2026-06-10
**Origem:** [028-edit-stash-01-spec.md](./028-edit-stash-01-spec.md)

## Arquitetura

| Camada | Arquivo | Mudança |
|---|---|---|
| Model | `Web/ClassEditModel.cs` | `Stash: List<ItemSpec>?` (pass-through) → `List<ItemSpecModel>` (editável). `FromDefinition` materializa via `ItemSpecModel.FromSpec`; `BuildLoadout` reconstrói via `ToSpec()` (vazia → `null`). |
| Service | `CostService.cs` | Seção **"Stash capacity (item 028)"**: `StashCapacityResult` + `CheckStashCapacity(ClassDefinition)` + `ResolveBaseStashGrids` + `TryPlace`. Ctor ganhou `InventoryHelper` e `DatabaseService`. |
| UI | `Web/Pages/ClassEdit.razor` | Aba Stash real: cards com `ItemSpecEditor AllowCount=true`, add (ItemPicker)/duplicar/remover, totais + valor ₽ do stash, `StashCapacityAlert()`. `RecomputeLoadoutCost` agora também computa `_stashCost` e `_stashCapacity`; setter de `BaseEditionSelect` dispara recompute. |

Componente novo `Stash*` não foi necessário — o `ItemSpecEditor` (026) cobre a linha inteira; o que restou (header do card + alert) é markup local da página.

## Estratégia do dry-run de capacidade

### 1. Resolução da grade da stash

Mesmo caminho que `InventoryBuilder.Apply` percorre em runtime, só que partindo do **template de perfil da `baseEdition`** (em registro o personagem já é o clone desse template):

```
DatabaseService.GetProfileTemplates()[baseKey]      // baseKey = def.BaseEdition ?? ClassRegistrar.DefaultBaseEdition
  → sides.Usec?.Character ?? sides.Bear?.Character
  → character.Inventory.Stash                       // MongoId do item-contêiner da stash
  → Inventory.Items.First(i => i.Id == Stash)       // o item stash em si
  → ItemHelper.GetItem(item.Template).Properties.Grids   // _props.Grids → cellsH × cellsV
```

**Evidência (DB do install, `SPT_Data/database/templates/profiles.json` + `items.json`):** `SPT Zero to hero` (default do mod) → stash tpl `566abbc34bdc2d92178b4576` = **"Standard stash 10x30"**, 1 grade `hideout` com `cellsH=10, cellsV=30` → **300 células**; a stash base nasce **vazia** (0 filhos).

Cada passo irresolúvel degrada para warning + "capacity not checked" (alerta info na UI) — nunca erro.

### 2. Materialização (espelha `InventoryBuilder.PackSpecsIntoGrids`)

Para cada `ItemSpec` do stash:

- **Sem `tpl`** → warning "not packed (the builder skips it too)" — o packing real só usa `tpl` (linhas preset-only são puladas pelo builder).
- **Composto** (`CatalogService.ResolveStashPreset(tpl)` retorna preset — mirror já existente do 022): uma colocação **por unidade** (`count`), dimensão da árvore **montada** via `InventoryHelper.GetItemSize(root.Template, root.Id, tree)` (considera `ExtraSize` dos mods). Nota: o builder injeta óptica mínima (`EnsureMinimumOptic`) antes de medir; ópticas não carregam `ExtraSize`, footprint idêntico — passo **não espelhado** (comentário no código).
- **Simples:** stack-split por `_props.StackMaxSize` (mesma expressão do builder), uma colocação por stack, dimensão via `GetItemSize` com item-sonda único.
- **Contents NÃO consomem células** da stash (ficam dentro do contêiner) — só a raiz tem footprint; o builder real também não os empacota na grade da stash.
- Exceção por linha capturada como warning (mirror do try/catch do builder).

### 3. Colocação e resultado

`GridPacker(cellsH, cellsV)` **vazio** por grade — exatamente como o builder (que ignora itens pré-existentes da stash base; quando a `baseEdition` já traz itens, ex. "Standard", o dry-run emite warning de overlap possível). `Place(w, h)` first-fit + rotação; falha → linha entra em `Unplaced` ("Nome ×unidades restantes") e o restante daquela linha é abortado (mirror do `break` do builder).

`StashCapacityResult`: `GridResolved`, `BaseEdition`, `GridSizes` ("10×30"), `TotalCells`, `UsedCells` (Σ w×h colocados), `OccupancyPercent`, `PlacedCount`, `Unplaced`, `Warnings`.

### Limites conhecidos

- First-fit não é ótimo: um arranjo manual poderia caber mais — mas o dry-run usa **o mesmo algoritmo** do runtime, então o aviso reflete o que de fato acontece.
- `EnsureMinimumOptic` não espelhado (footprint inalterado na prática — ver §2).
- Itens pré-existentes da stash base não pré-ocupam a grade (paridade com o builder; warning informativo).
- Linhas com `tpl` mal-formado/desconhecido não são empacotadas (warning), igual ao builder.

## Custo do stash (subtotal)

`_stashCost = CostService.ComputeLoadoutCost(new ClassDefinition { Loadout = new Loadout { Stash = … } })` — definição contendo só o stash, porque filtrar `_loadoutCost.Items` por `Context == "stash"` perderia contents/ammo (contexts "contents"/"ammo" são compartilhados com o equipado).

## Decisões

- Dry-run dentro do `CostService` (opção A do kickoff): já é singleton com `CatalogService`/`ItemHelper` e o mirror de presets de stash mora lá desde o 022 — service novo duplicaria DI sem ganho.
- `GridPacker` reusado direto (internal, mesmo assembly) — zero duplicação do algoritmo.
- Recompute síncrono no circuito (packing de dezenas de itens numa grade ≤ 10×72 é trivial — sem `Task.Run`).
