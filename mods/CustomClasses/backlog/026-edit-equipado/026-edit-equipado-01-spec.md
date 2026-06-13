# 026 — Editor de loadout equipado — Spec

**Mod:** CustomClasses
**Status:** Implementado (validado no browser — ver as-built)
**Criado:** 2026-06-10
**Origem:** [026-edit-equipado-00-kickoff.md](./026-edit-equipado-00-kickoff.md)

## Visão geral

Aba **Equipped** da página de edição (shell do 025) deixa de ser placeholder: cada slot do personagem (enum `EquipmentSlots`) vira um card com um **`ItemSpecEditor`** — componente recursivo compartilhado que o item 028 reusa para o stash. Compatibilidade item×slot e mod×slot vem dos filters do DB vivo (`_props.Slots`), como **aviso** (nunca bloqueia — o dry-run do Save decide).

## Comportamento desejado

- **Aba Equipped:** um card por slot presente no `loadout.equipped` (header = nome do slot + remover com confirmação); "Add slot" oferece só os `EquipmentSlots` ainda não usados (ordem do enum). Caption fixa avisa que `count` é ignorado em equipado e que compatibilidade é warning-only.
- **`ItemSpecEditor`** (1 `ItemSpec`):
  - **Modo "Item" (tpl):** ItemPicker (dialog) restrito pelo filter do slot de equipamento; **árvore de mods** colapsável — um row por slot do template (`_props.Slots`, chip `required`), Set/Replace via ItemPicker filtrado pelo filter daquele slot, remove, recursão nos sub-slots do mod montado; entradas com `slotId` órfão (não existe no template) aparecem com warning + remove.
  - **Modo "Preset":** ItemPicker restrito a armas (baseclass WEAPON) → semântica `preset: <tpl da arma>` (default/premium) OU preset explícito via PresetPicker (chips Default/Premium do 023); switch `premium`; botão de voltar pro default. Trocar de modo limpa os campos do modo abandonado.
  - **Ammo (arma em qualquer modo):** AmmoPicker por calibre (colapsável); switches `loadedMag`/`chambered` **desabilitados sem ammo** (hint cita PA-01-03); limpar o ammo reseta os dois switches.
  - **`count`:** visível só com `AllowCount=true` (equipado NÃO; stash/contents sim).
  - **Contents (contêiner com `_props.Grids`):** lista recursiva de `ItemSpecEditor` com `AllowCount=true`; warning "no grids" quando há contents mas o template atual não tem grade.
  - **Avisos inline:** `unresolved` (tpl/preset fora do DB vivo) e `not allowed in slot` (fora do filter) — nunca bloqueiam.
  - **Limite de recursão:** `MaxDepth = 6` (contents e árvore de mods); além disso o nível é preservado no save mas não editável.
- **Custo ao vivo:** total ₽ do loadout na toolbar recalcula a cada mudança (add/remove de slot, troca de item/preset/ammo/mod/content).
- **Save:** mesmo pipeline do 025 (`ClassEditorService.Save` → dry-run `ValidateAndBuild` → `.bak` → write → hot-apply). `loadout.stash` passa **intacto** (028).

## Critérios de aceite

- [x] Montar arma por preset (default/premium/explícito) E por árvore manual de mods; ammo coerente com o calibre (AmmoPicker resolve pelo `ammoCaliber` da arma).
- [x] Regra "ammo obrigatório com loadedMag/chambered" imposta na UI (switches desabilitados; clear reseta).
- [x] Compatibilidade item×slot e mod×slot via `_props.Slots` filters do DB vivo (warning, não bloqueia).
- [x] Custo do loadout recalcula on-change (validado no browser: remover Backpack → 4.770.122 → 4.651.624 ₽).
- [x] Save de loadout passa pelo dry-run; `stash` preservado intacto no round-trip (27 linhas).
- [x] Save real no browser: `.bak1` + arquivo reescrito + hot-apply (re-registro logado) — DoD do kickoff.

## Fora de escopo

- Edição do stash (028 — reusa o `ItemSpecEditor` com `AllowCount=true`).
- Validação bloqueante de compatibilidade (decisão do kickoff: espelhar a leniência do loader).
- Grid visual / posicionamento (packing é runtime — `GridPacker`).
