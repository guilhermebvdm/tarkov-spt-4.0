# 054 — Rename Ghost/Fantasma → Stealth/Furtivo · As-built

**Mod:** CustomClasses · **Data:** 2026-06-22 · **Status:** 🟡 código feito + **propagação pro install CONFIRMADA em 2026-07-04** (`D:/SPT/.../config/classes/furtivo.jsonc` com `name: Stealth`, displayName Stealth/Furtivo; `fantasma` só como .bak) — pendente APENAS validação in-game (launcher mostra Furtivo/Stealth; perfil novo cria a edição certa; gating do 050 na chave `Stealth`)
**Ref:** [00-kickoff](./054-furtivo-rename-00-kickoff.md) · [class-design.md §Contrato de gating](../../docs/class-design.md)

## O que foi feito

| Arquivo | Mudança |
|---|---|
| `modded/Server/config/classes/fantasma.jsonc` → **`furtivo.jsonc`** | `git mv` (renomeado). `name` `Ghost`→**`Stealth`** · `displayName` {en `Ghost`→**`Stealth`**, pt `Fantasma`→**`Furtivo`**} · comentário + `description` (en/pt) atualizados. Parse ✅. |
| `scripts/class-matrix.mjs` | key `fantasma:`→**`furtivo:`** (MATRIX + APPROVED) · `file`→`furtivo.jsonc`. Cross-check `node class-matrix.mjs` ✅ (Furtivo custo 30.14 / net +6.12, sem flags). |

## Verificação

- `furtivo.jsonc` parseia (name=`Stealth`, displayName {en:`Stealth`, pt:`Furtivo`}). ✅
- `class-matrix.mjs` cross-check ✅ (transcrição fiel).
- **Sem referências hardcoded** a "Ghost"/"Fantasma" no código C# do server (`ClassRegistrar`/`CustomClassesMod` leem de config; único hit é o comentário "skill fantasma" = phantom, não a classe). ✅
- Gating: `Info.GameVersion` = `displayName[lang]` → chave estável agora é `Stealth` (en/name) / `Furtivo` (pt). O 050 deve gatear no valor pós-rename.

## Pendente (gate humano/externo)

- **Re-sync do server** (restart/hot-apply) p/ registrar a edition renomeada.
- **Validação in-game:** launcher mostra **Furtivo (pt) / Stealth (en)**; perfil novo cria com a edition certa; viewer do editor reflete o novo `name` (write+hash não basta — validar no jogo).
- ⚠️ Mexeu em `modded/Server/` → conferir que o editor web não estava ativo (risco de clobber).

## Histórico

| Data | Autor | Alteração |
|---|---|---|
| 2026-06-22 | Guilherme | Rename executado (config + class-matrix). Pendente re-sync + validação in-game. |
