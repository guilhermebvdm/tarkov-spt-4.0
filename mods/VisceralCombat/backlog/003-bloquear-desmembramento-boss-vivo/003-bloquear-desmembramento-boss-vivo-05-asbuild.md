# 003 — Bloquear Desmembramento de Perna em Boss/Escolta Vivos · As-Built

**Mod:** VisceralCombat
**Spec funcional:** [003-bloquear-desmembramento-boss-vivo-01-spec.md](003-bloquear-desmembramento-boss-vivo-01-spec.md)
**Spec técnica:** [003-bloquear-desmembramento-boss-vivo-02-spec-tech.md](003-bloquear-desmembramento-boss-vivo-02-spec-tech.md)
**Última review técnica:** [003-bloquear-desmembramento-boss-vivo-03-spec-tech-review-01.md](003-bloquear-desmembramento-boss-vivo-03-spec-tech-review-01.md)
**Build inicial:** 2026-09-10

## Arquivos alterados (build inicial)

| Ação | Path | Resumo |
| --- | --- | --- |
| MODIFICADO | `mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/LimbKillPatch.cs` | Gate "vivo" em `ProcessLimbKill` (linhas 68-77) passa a excluir Boss/escolta via `WildSpawnType.IsBossOrFollower()`, com defesa contra `Profile`/`Info`/`Settings` nulo |
| MODIFICADO | `mods/VisceralCombat/modded/VisceralCombat/VisceralCombat/VisceralEntry.cs` | Versão do plugin 3.9.11 → 3.9.12 |

## PA-NN-MM resolvidos durante o build

| ID | Categoria · Impacto | Resumo da resolução |
| --- | --- | --- |
| PA-01-01 | C — Erro de Lógica · 🟢 | Cosmético (faixa de linha aproximada) — nenhuma mudança de código necessária |

## Mudanças posteriores

(vazio inicialmente — preenchido por `/apply-code-review`)

## Pendências antes de marcar 🟢 Entregue

- [ ] Validação solo: Boss vivo (ex.: Killa) não desmembra a perna sob tiro; Scav comum continua desmembrando normalmente (30%); Boss morto desmembra pós-morte normalmente.
- [ ] Validação de escolta: seguidor de boss vivo (ex.: `followerBully`) também bloqueado.

## Histórico

| Data | Evento |
| --- | --- |
| 2026-09-10 | Build concluído via `/code-mod` — código implementado e compilado, aguardando validação in-game antes de fechar o item |
| 2026-09-10 | `/code-review` rodada 01: 0 achados — implementação limpa, bate com a spec técnica sem desvios |
