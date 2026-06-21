# 047 — Roster 11→6 (aplicar matriz) · As-Built

**Mod:** CustomClasses
**Spec funcional:** [047-roster-6-classes-01-spec.md](047-roster-6-classes-01-spec.md)
**Spec técnica:** [047-roster-6-classes-02-spec-tech.md](047-roster-6-classes-02-spec-tech.md)
**Última review técnica:** [047-roster-6-classes-03-spec-tech-review-01.md](047-roster-6-classes-03-spec-tech-review-01.md)
**Build inicial:** 2026-06-21

> Documentação **pós-implementação**. Reflete o estado real do código entregue pelo `/code-mod`. Onde diverge da spec técnica, este documento ganha.

## ⚠️ Divergência da spec (decisão do usuário, 2026-06-21)

O **`OrphanEditionSaveLoadRouter` NÃO foi construído** (e o config `orphanEditionFallback` não foi adicionado). Decisão do usuário: o server não roda oficialmente ainda, então perfis de classes removidas perdidos/quebrados **não são problema** — a rede de segurança vira over-engineering. Isso torna **moot** os pontos PA-01-01/02/03 (todos sobre o router). O 047 fica **config-only + sync do `SkillWeights.cs`**.

## Arquivos alterados (build inicial)

| Ação | Path | Resumo |
| --- | --- | --- |
| MODIFICADO | `modded/Server/config/classes/medicoDeCombate.jsonc` | matriz Médico (skills/mults) — custo 31.87, net +6.12 |
| MODIFICADO | `modded/Server/config/classes/fuzileiro.jsonc` | matriz Fuzileiro — custo 30.51, net +6.27 |
| MODIFICADO | `modded/Server/config/classes/cacador.jsonc` | matriz Caçador + hideout ShootingRange (dropou Heating) — custo 31.40, net +6.21 |
| MODIFICADO | `modded/Server/config/classes/saqueador.jsonc` | matriz Saqueador + hideout ScavCase — custo 28.23, net +4.09 |
| CRIADO | `modded/Server/config/classes/fantasma.jsonc` | nova classe Ghost + gear placeholder (clone do furtivo) + hideout WaterCloset — custo 30.14, net +6.12 |
| CRIADO | `modded/Server/config/classes/tanque.jsonc` | nova classe Tank + gear placeholder (clone do tático) + hideout RestSpace — custo 30.29, net +4.28 |
| DELETADO | `modded/Server/config/classes/{armeiro,batedor,gerenteDeOperacoes,operadorFurtivo,operadorTatico,sobrevivencialista}.jsonc` | 6 classes aposentadas removidas |
| MODIFICADO | `modded/Server/SkillWeights.cs` | +3 categorias de gem: `UsecArsystems`/`BearAksystems`→C, `Shadowconnections`→P |

**Validação (repo):** `check-skill-costs.mjs` — as 6 em [28,32] (Naked isento); custos batem com `class-matrix.mjs` (cross-check ✅). Avisos de "categories without coverage" são os não-bloqueantes esperados.
**NÃO feito (fora deste passo):** `/compile-mod` (build + install), validação in-game (item 052). Identidade visual (ícones próprios de fantasma/tanque) usa placeholder (PNG do furtivo/tático) — curar depois.

## PA-NN-MM resolvidos durante o build

| ID | Categoria · Impacto | Resumo da resolução |
| --- | --- | --- |
| PA-01-01 | B · 🟡 | **Moot** — router descopado (sem perfis ao vivo). |
| PA-01-02 | A · 🟡 | **Moot** — sem router, sem `orphanEditionFallback`. |
| PA-01-03 | A · 🟡 | **Moot** — idem. |
| PA-01-04 | A · 🟢 | Gear placeholder por clone: fantasma ← operadorFurtivo, tanque ← operadorTatico (antes de deletar). |
| PA-01-05 | C · 🟢 | Âncora ProfileHelper corrigida na spec (não afeta código — router descopado). |
| PA-01-06 | A · 🟢 | Confirmado `Commit` escreve `GetProfileTemplates()[plan.Name]` (ClassRegistrar.cs:282). |

## Mudanças posteriores

(vazio inicialmente — preenchido por `/apply-code-review`)

## Histórico

| Data | Evento |
| --- | --- |
| 2026-06-21 | Build concluído via `/code-mod` — config-only + SkillWeights sync; router descopado por decisão do usuário. |
