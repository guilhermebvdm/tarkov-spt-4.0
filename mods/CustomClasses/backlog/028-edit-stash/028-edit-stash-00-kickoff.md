# 028 — Editor de inventário (stash) · Kickoff

**Mod:** CustomClasses · **Data:** 2026-06-09 · **Origem:** plano aprovado do editor web de classes (`~/.claude/plans/`, sessão 2026-06-09; renumerado 027→028)
**Wave:** W5 (paralelo ao 029) · **Deps:** 026 (ItemSpecEditor), 025

> Brief de kickoff — insumo para `/create-spec 028`. Não é a spec.

## Objetivo

Aba "Stash": itens iniciais do inventário da classe. **Sem grid visual** — o stash do schema é lista plana (`tpl`+`count`); o `GridPacker` posiciona em runtime.

## Escopo

- Aba Stash: lista de `ItemSpec` (reusa o `ItemSpecEditor` do 026) com `count`, `contents` recursivo (itens dentro de mochila/rig no stash), mags com `ammo`/`loadedMag`.
- **Dry-run do `GridPacker`** (stateless por instância — instanciar com as dimensões do stash da `baseEdition` + grids de containers) para **aviso de capacidade** quando o conteúdo não couber. **Não bloqueia** o save — o loader já trata overflow (itens que não couberam são pulados com warning), e a dimensão real depende da stash do template.
- Considerar `StackMaxSize` (stack-split) no dry-run, como o packing real faz.

## Refs

- [modded/Server/GridPacker.cs](../../modded/Server/GridPacker.cs) — first-fit + rotação, stateless
- [modded/Server/InventoryBuilder.cs](../../modded/Server/InventoryBuilder.cs) — `PackSpecsIntoGrids` (referência do dry-run: dimensão montada via `InventoryHelper.GetItemSize`)
- `ItemSpecEditor` (026), shell de abas (025)

## DoD (resumo)

- Stash editado persiste e aparece no perfil novo (sem overflow inesperado).
- Aviso de capacidade dispara em stash propositalmente lotado e some ao reduzir.
