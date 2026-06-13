# 028 — Editor de inventário (stash) — Spec

**Mod:** CustomClasses
**Status:** Implementado (validado no browser — ver as-built)
**Criado:** 2026-06-10
**Origem:** [028-edit-stash-00-kickoff.md](./028-edit-stash-00-kickoff.md)

## Visão geral

Aba **Stash** da página de edição deixa de ser placeholder: o `loadout.stash` (lista plana de `ItemSpec` — o `GridPacker` posiciona em runtime, **sem grid visual**) vira uma lista editável de cards, cada um com o **`ItemSpecEditor`** do 026 em modo `AllowCount=true` (count, contents recursivos p/ mochila/rig, ammo/loadedMag p/ armas). Um **dry-run do `GridPacker`** contra a grade da stash da `baseEdition` avisa quando o conteúdo não cabe — **warning only**, nunca bloqueia o save (o loader já pula overflow com warning próprio).

## Comportamento desejado

- **Lista de linhas:** um card por entrada do stash (header `#N` + duplicar + remover com confirmação) com `ItemSpecEditor` (`AllowCount=true`): modos Item/Preset, count, ammo/loadedMag/chambered, árvore de mods, contents recursivos.
- **Add item:** botão abre o `ItemPicker` (sem filtro — stash aceita qualquer template) e cria a linha já com o `tpl` escolhido.
- **Duplicar:** deep-copy via round-trip `ToSpec()` → `FromSpec()`, inserida logo abaixo da original.
- **Totais ao vivo:** nº de linhas, nº de unidades (Σ `count` das raízes) e **valor ₽ só do stash** (recomputado junto com o "Loadout total" da toolbar a cada mudança).
- **Dry-run de capacidade (`CostService.CheckStashCapacity`):**
  - Resolve a grade da stash da `baseEdition` pelo mesmo caminho do `InventoryBuilder.Apply` (template de perfil → `Inventory.Stash` → item → `_props.Grids`).
  - Materializa a lista como o packing real (`PackSpecsIntoGrids`): preset de stash p/ compostos, stack-split por `StackMaxSize` p/ simples, dimensão montada via `InventoryHelper.GetItemSize`, first-fit + rotação via `GridPacker.Place`.
  - **MudAlert:** verde "Fits — X% occupied (used/total cells, stash W×H of base '…')" ou laranja "N item line(s) won't fit: … — the server will skip them with a warning". Grade não resolvida → alerta info "capacity not checked" (nunca erro).
  - Recalcula a cada mudança da aba **e ao trocar a `baseEdition`** (a grade depende dela).
- **Round-trip:** `ClassEditModel.Stash` vira lista de `ItemSpecModel` (mesmo espelho mutável do 026); `BuildLoadout` reconstrói `List<ItemSpec>` no save (lista vazia → `null`, omitida na serialização).
- **Save:** pipeline inalterado do 025 (dry-run `ValidateAndBuild` → `.bak` → write → hot-apply).

## Critérios de aceite

- [x] Linhas do stash renderizam com `ItemSpecEditor` completo (count, contents, ammo) e add/duplicar/remover funcionam.
- [x] Custo do stash e total do loadout recalculam on-change.
- [x] Dry-run de capacidade: verde quando cabe (com % de ocupação), laranja listando o que não coube — sem bloquear nada.
- [x] Aviso some/dispara ao reduzir/lotar o stash (validado no browser com count alto).
- [x] Round-trip preserva o stash (load → save sem mudança ≈ arquivo equivalente; normalizações do 026 aplicam: vazios→null, count<1→1).

## Fora de escopo

- Grid visual / posicionamento manual (packing é runtime — decisão do kickoff).
- Bloqueio por capacidade (o loader trata overflow; a dimensão real depende do template da stash).
- Pré-ocupação da grade com itens que a `baseEdition` já traz (o builder real também ignora — vira warning informativo no dry-run).
- README/docs do editor (item 029, paralelo).
