# 018 — Doc canônica do schema de classe · Kickoff

**Mod:** CustomClasses · **Data:** 2026-06-09 · **Origem:** plano aprovado do editor web de classes (`~/.claude/plans/`, sessão 2026-06-09; renumerado 017→018 por colisão com o bug 017)
**Wave:** W0 (primeiro de todos — alimenta as specs de 019–029) · **Deps:** —

> Brief de kickoff — insumo para `/create-spec 018`. Não é a spec.

## Objetivo

Criar `mods/CustomClasses/docs/class-schema.md`: a referência canônica do schema JSON de classe, usada por todas as specs do editor web (019–029) e pela validação do pipeline (021).

## Escopo

- **100% dos campos** do `ClassDefinition`: `name` (chave da edition), `enabled`, `baseEdition` (default `"SPT Zero to hero"`), `displayName {en,pt}`, `description {en,pt}` (+ forma legada string), `iconFile`, `nameColor`, `skills` (SkillTypes → 0..51), `skillMultipliers` (fator XP ≥ 0), `hideout` (HideoutAreas → nível), `outfit {usec,bear}.{upper,lower}`, `loadout {equipped: slot→ItemSpec, stash: ItemSpec[]}` com `ItemSpec` (`tpl|preset`, `premium`, `count`, `ammo`, `loadedMag`, `chambered`, `contents[]`, `mods[]`) e `ModSpec` (`slotId`, `tpl`, `mods[]` recursivo).
- **Semântica dos builders:** `InventoryBuilder` (remoção de ocupante de slot, preset default vs premium vs árvore manual, mag+câmara com `ammo` obrigatório, contents→grids), `GridPacker` (posiciona stash em **runtime** — stash é lista plana, sem posição no JSON), `HideoutBuilder`, `OutfitBuilder` (vanilla vs "aparência direta", validação de facção).
- **Regras de validação do loader** com refs **por símbolo/método** (`CustomClassesMod.RegisterClass`, `CustomClassesMod.ApplySkills`, `InventoryBuilder.Apply`, …) — não por linha (o 021 refatora esses arquivos).
- **Limites conhecidos:** 4 skills dependem do Skills-Extended (`FirstAid`, `FieldMedicine`, `BearRawpower`, `UsecNegotiations` — soft-detect em `SkillsExtendedCompat`); skill/estação desconhecida = warning + ignorada; classe inválida = pulada sem derrubar as demais.
- Reconciliar `modded/Server/config/classes/_docs/exampleClass.jsonc` com a doc (corrigir divergências).

## Refs

- [modded/Server/ClassDefinition.cs](../../modded/Server/ClassDefinition.cs) — DTO completo
- [modded/Server/CustomClassesMod.cs](../../modded/Server/CustomClassesMod.cs) — loader + validações
- [modded/Server/InventoryBuilder.cs](../../modded/Server/InventoryBuilder.cs), [GridPacker.cs](../../modded/Server/GridPacker.cs), [HideoutBuilder.cs](../../modded/Server/HideoutBuilder.cs), [OutfitBuilder.cs](../../modded/Server/OutfitBuilder.cs)
- [config/classes/_docs/exampleClass.jsonc](../../modded/Server/config/classes/_docs/exampleClass.jsonc)
- Doc de inventário: `docs/technical/inventario-itens-spt4.md` (raiz do repo)

## DoD (resumo)

- Doc cobre todos os campos do `ClassDefinition` (incl. `displayName`), cada regra de validação com ref de símbolo.
- `exampleClass.jsonc` consistente com a doc.
