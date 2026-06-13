# 039 — Fundação do balanceamento

Item de **fundação** do épico de balance (039–045). Diferente dos itens de feature, não tem spec/código em `modded/` — os entregáveis são **docs + scripts**, já concluídos:

| Entregável | Path |
|---|---|
| Modelo de balance (2 orçamentos, meta ~+6, anti-furo, ressalva de viabilidade) | [docs/balance-model.md](../../docs/balance-model.md) |
| Arquétipos das 11 classes (conjunto plausível por classe) | [docs/class-archetypes.md](../../docs/class-archetypes.md) |
| Tabela de peso única (lado JS, espelha `SkillWeights.cs`) | [scripts/skill-weights.mjs](../../scripts/skill-weights.mjs) |
| Snapshot de baseline (custo + netMult + flags) | [scripts/class-balance-snapshot.mjs](../../scripts/class-balance-snapshot.mjs) |
| Paridade de custo (28–32) | [scripts/check-skill-costs.mjs](../../scripts/check-skill-costs.mjs) |

**Meta travada (usuário, 2026-06-13):** `netMult ~+6` para todas; **Médico de Combate = padrão intacto**; diferença entre classes vem de *quais* skills cada uma acelera. Rodadas 040–045 aplicam isso por grupo de arquétipos.

**Baseline 2026-06-13** (`node scripts/class-balance-snapshot.mjs`): Médico +6.17 (padrão); demais entre +1.36 (Saqueador) e +3.43 (Sobrevivencialista) — todas a subir. Peladão isento (`noBaseline`).
