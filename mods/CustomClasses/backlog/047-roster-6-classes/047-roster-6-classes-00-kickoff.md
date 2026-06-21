# 047 — Roster 11→6 (aplicar matriz recalibrada) · Kickoff

**Mod:** CustomClasses · **Data:** 2026-06-20 · **Origem:** redesign 11→6 (handoff `customclasses-class-redesign`; matriz aprovada/recalibrada na Fase 4 — [class-levers.md](../../docs/class-levers.md) §4)
**Wave:** R-W0 — **PRIMEIRO** (a camada 🎯 funciona sozinha → valida cedo no editor/launcher) · **Deps:** —

> Brief de kickoff — insumo para `/create-spec 047`. Não é a spec.

## Objetivo

Substituir as 11 classes antigas pelas **6 do redesign** (Médico/Fuzileiro/Caçador/Fantasma/Saqueador/Tanque + Peladão isenta), aplicando nos `.jsonc` a matriz recalibrada (skills + skillMultipliers) + loadout inicial 🎒 + hideout 🏠. **Só a camada 🎯** (XP-mult); patches e skills custom vêm nos itens 048–051.

## Escopo

**(a) Matriz 🎯 (server-side; editável no editor web, aplica em perfil novo/restart):**
- **Rewrite (4):** `medicoDeCombate.jsonc`, `fuzileiro.jsonc`, `cacador.jsonc`, `saqueador.jsonc` com a matriz nova.
- **Criar (2):** `fantasma.jsonc`, `tanque.jsonc`.
- **Remover (6 aposentadas):** armeiro, batedor, gerenteDeOperacoes, operadorFurtivo, operadorTatico, sobrevivencialista. **Manter** peladao (`noBaseline`).
- skills + skillMultipliers **exatamente** como em [`scripts/class-matrix.mjs`](../../scripts/class-matrix.mjs) (cross-check ✅). IDs = enum `SkillTypes` (`Lockpicking`/`UsecArsystems`/`BearAksystems`/`ShadowConnections`/`SilentOps`/`ProneMovement` — **não** "LockPicking").
- Saqueador usa Lockpicking/Strength ×3 (acima do teto ×2.0) — **intencional** (ressalva de viabilidade peso-baixo, balance-model §2); não "corrigir".

**(b) Loadout 🎒 (gear) + hideout 🏠 por classe** ([class-levers.md](../../docs/class-levers.md) §5):
- hideout: 1 estação inicial + 1 estação −50%.
- gear (`equipped` + `stash`): as 4 mantidas já têm; **as 2 novas (fantasma/tanque) precisam de gear autorado** — usar `extract-from-profile.mjs` (item 046) a partir de um profile de referência, com merge cirúrgico (preserva skills/mults). **O `/create-spec 047` escolhe/nomeia o profile-fonte de cada classe nova** (ou decide autorar o gear à mão).

**(c) Sub-tarefa — sync `SkillWeights.cs`** (ponta solta #5; mudança C# **separada** da matriz, só afeta o warning de custo do editor): adicionar Categories `ShadowConnections→P`, `UsecArsystems→C`, `BearAksystems→C` (já no `skill-weights.mjs`).

**(d) Decisão — bug do Círculo de Cultistas** (ShadowConnections do Saqueador, [class-skill-catalog.md](../../docs/class-skill-catalog.md) §5.1): aceitar o efeito instantâneo **ou** corrigir o `NormalizeToPercentage()` no server antes de ativar.

## Riscos / atenção

- **Coordenar com a sessão do editor web** — fonte de verdade = `.jsonc` do install. Aplicar via `build-class-jsons.js --force` + `/sync-classes`; **não clobberar** edições (memória `feedback_serve_inventory_clobber`).
- SMG e AttachedLauncher foram **removidos** (inertes no globals) — não reintroduzir.
- `check-skill-costs.mjs` vai avisar **"categories without coverage"** (vários cards têm skills *iniciais* só em 2 categorias — ex.: Médico P/Ph). É warning **não-bloqueante**; documentar quais são aceitos.
- Após aplicar: `class-matrix.mjs` + `check-skill-costs.mjs` sem flags de **custo** (todas em [28,32]).

## Refs

- [../../docs/class-levers.md](../../docs/class-levers.md) §4/§5/§6.4 · [../../docs/class-overview.md](../../docs/class-overview.md) · [../../docs/class-skill-catalog.md](../../docs/class-skill-catalog.md) §5.1
- [../../scripts/class-matrix.mjs](../../scripts/class-matrix.mjs) — matriz fonte · [../../scripts/extract-from-profile.mjs](../../scripts/extract-from-profile.mjs) — gear (item 046)
- [../../modded/Server/config/classes/](../../modded/Server/config/classes/) — destino · [../../scripts/build-class-jsons.js](../../scripts/build-class-jsons.js) + skill `/sync-classes`

## DoD (resumo)

- 6 classes (+Peladão) no editor e no launcher com a matriz recalibrada; 6 antigas removidas.
- gear das 2 novas (fantasma/tanque) presente; `class-matrix.mjs` e `check-skill-costs.mjs` sem flag de custo.
- `SkillWeights.cs` sincronizado (sub-tarefa c); decisão do bug do Círculo registrada (sub-tarefa d).
- Smoke: criar perfil de cada classe; skills/mults corretos na tela de Skills.
