# 047 — Roster 11→6 (aplicar matriz recalibrada) · Kickoff

**Mod:** CustomClasses · **Data:** 2026-06-20 · **Origem:** redesign 11→6 (handoff `customclasses-class-redesign`; matriz aprovada/recalibrada na Fase 4 — [class-levers.md](../../docs/class-levers.md) §4)
**Wave:** R-W0 — **PRIMEIRO** (a camada 🎯 funciona sozinha → valida cedo no editor/launcher) · **Deps:** —

> Brief de kickoff — insumo para `/create-spec 047`. Não é a spec.

## Objetivo

Substituir as 11 classes antigas pelas **6 do redesign** (Médico/Fuzileiro/Caçador/Fantasma/Saqueador/Tanque + Peladão isenta), aplicando nos `.jsonc` a matriz recalibrada (skills + skillMultipliers) + loadout inicial 🎒 + hideout 🏠. **Só a camada 🎯** (XP-mult); patches e skills custom vêm nos itens 048–051.

## Escopo

- **Rewrite (4):** `medicoDeCombate.jsonc`, `fuzileiro.jsonc`, `cacador.jsonc`, `saqueador.jsonc` com a matriz nova.
- **Criar (2):** `fantasma.jsonc`, `tanque.jsonc`.
- **Remover (6 aposentadas):** armeiro, batedor, gerenteDeOperacoes, operadorFurtivo, operadorTatico, sobrevivencialista. **Manter** peladao (`noBaseline`).
- skills + skillMultipliers **exatamente** como em [`scripts/class-matrix.mjs`](../../scripts/class-matrix.mjs) (cross-check ✅).
- loadout 🎒 (1 estação inicial) + hideout 🏠 (1 estação −50%) por classe ([class-levers.md](../../docs/class-levers.md) §5).
- **Sincronizar `SkillWeights.cs`** (ponta solta #5): adicionar Categories `ShadowConnections→P`, `UsecArsystems→C`, `BearAksystems→C` (já no `skill-weights.mjs`, faltam no `.cs`).
- IDs = enum `SkillTypes`: `Lockpicking`, `UsecArsystems`, `BearAksystems`, `ShadowConnections`, `SilentOps`, `ProneMovement` (**não** "LockPicking").

## Riscos / atenção

- **Coordenar com a sessão do editor web** — fonte de verdade = `.jsonc` do install. Aplicar via `build-class-jsons.js --force` + `/sync-classes`; **não clobberar** edições (memória `feedback_serve_inventory_clobber`).
- SMG e AttachedLauncher foram **removidos** (inertes no globals) — não reintroduzir.
- Após aplicar: `class-matrix.mjs` + `check-skill-costs.mjs` sem flags (custo [28,32]).

## Refs

- [../../docs/class-levers.md](../../docs/class-levers.md) §4/§5 · [../../docs/class-overview.md](../../docs/class-overview.md)
- [../../scripts/class-matrix.mjs](../../scripts/class-matrix.mjs) — matriz fonte
- [../../modded/Server/config/classes/](../../modded/Server/config/classes/) — destino · [../../scripts/build-class-jsons.js](../../scripts/build-class-jsons.js) + skill `/sync-classes`

## DoD (resumo)

- 6 classes (+Peladão) no editor e no launcher com a matriz recalibrada; 6 antigas removidas.
- `class-matrix.mjs` e `check-skill-costs.mjs` sem flags; `SkillWeights.cs` sincronizado.
- Smoke: criar perfil de cada classe; skills/mults corretos na tela de Skills.
