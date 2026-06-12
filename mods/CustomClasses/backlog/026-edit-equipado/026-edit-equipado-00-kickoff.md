# 026 — Editor de loadout equipado · Kickoff

**Mod:** CustomClasses · **Data:** 2026-06-09 · **Origem:** plano aprovado do editor web de classes (`~/.claude/plans/`, sessão 2026-06-09; renumerado 025→026)
**Wave:** W4 (paralelo ao 027 — arquivos novos × diálogos na lista) · **Deps:** 023, 025, 021

> Brief de kickoff — insumo para `/create-spec 026`. Não é a spec. Item mais gordo de UI — por isso Equipado (aqui) e Stash (028) foram separados.

## Objetivo

Aba "Equipado": configurar os slots do personagem com itens compostos válidos.

## Escopo

- **`ItemSpecEditor`** — componente compartilhado (reusado pelo 028): edita um `ItemSpec` completo — `tpl`|`preset` (pickers do 023), `premium`, `count`, `ammo` + `loadedMag` + `chambered` (regra: `ammo` obrigatório quando loadedMag/chambered), `mods[]` (árvore recursiva de `ModSpec` slotId→tpl) e `contents[]` (recursivo, p/ rig/mochila).
- **Aba Equipado:** slot (enum `EquipmentSlots` completo — FirstPrimaryWeapon, SecondPrimaryWeapon, Holster, Headwear, Earpiece, FaceCover, Eyewear, ArmBand, ArmorVest, TacticalVest, Backpack, Scabbard, SecuredContainer, Pockets…) → `ItemSpec`; adicionar/remover slot.
- **Validação de compatibilidade:** item permitido no slot e mod permitido no slot do pai via `_props.Slots` filters do DB vivo (mesma fonte que o jogo usa); inválido = aviso, espelhando a leniência do loader (que pula slot inválido com warning).
- **Dry-run do pipeline (021)** antes do save — diagnósticos do `InventoryBuilder` aparecem no UI.

## Riscos / atenção

- Árvore de mods recursiva é o componente mais complexo do editor — manter o `ItemSpecEditor` autocontido e testável.
- `count > 1` é ignorado em equipped (só stash) — refletir no UI.

## Refs

- [modded/Server/InventoryBuilder.cs](../../modded/Server/InventoryBuilder.cs) — semântica de preset/mods/ammo/ocupante
- Doc do 018 (`docs/class-schema.md`) — `ItemSpec`/`ModSpec`
- Pickers (023), shell de abas (025), dry-run (021)

## DoD (resumo)

- Montar arma por preset E por árvore manual de mods; ammo coerente com o calibre.
- Save de loadout inválido bloqueado com diagnóstico; classe salva nasce equipada corretamente em perfil novo.
