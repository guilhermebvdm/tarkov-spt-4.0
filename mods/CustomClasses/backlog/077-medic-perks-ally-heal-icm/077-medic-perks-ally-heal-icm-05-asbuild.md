# 077 — Médico: perks de tempo/movimento valem na cura de aliado do ICM · As-Built

**Mod:** CustomClasses (+ TRL-ImmersiveCombatMedicine)
**Spec funcional:** [077-medic-perks-ally-heal-icm-01-spec.md](077-medic-perks-ally-heal-icm-01-spec.md)
**Spec técnica:** [077-medic-perks-ally-heal-icm-02-spec-tech.md](077-medic-perks-ally-heal-icm-02-spec-tech.md)
**Última review técnica:** [077-medic-perks-ally-heal-icm-03-spec-tech-review-01.md](077-medic-perks-ally-heal-icm-03-spec-tech-review-01.md)
**Build inicial:** 2026-07-19

> Documentação pós-implementação. Reflete o código real entregue pelo `/code-mod`.

## Arquivos alterados (build inicial)

| Ação | Path | Resumo |
| --- | --- | --- |
| CRIADO | `mods/CustomClasses/modded/Client/CombatMedicAllyPerks.cs` | Fachada pública `AllyHealTimeMult(bool)` + `AllyMobileSurgeon()` (gate `IsLocalClass`, fail-safe) |
| MODIFICADO | `mods/CustomClasses/modded/Client/Patches/ClassMedicPatches.cs` | `MedicTiming.FactorFor(bool)` overload (fonte única da lógica de tempo do 072; `FactorFor(Item)` redireciona) |
| MODIFICADO | `mods/TRL-ImmersiveCombatMedicine/modded/Patches/Medical/CustomClassesBridge.cs` | Resolve `CombatMedicAllyPerks` + wrappers reflection `AllyHealTimeMult` (fail-open 1f) / `AllyMobileSurgeon` (fail-safe false) |
| MODIFICADO | `mods/TRL-ImmersiveCombatMedicine/modded/Patches/Medical/MedicHealPatch.cs` | Campo `AllyAnimSpeedMult` + multiplicado nos 3 `SetUseTimeMultiplier` (paths remoto/null/sucesso) |
| MODIFICADO | `mods/TRL-ImmersiveCombatMedicine/modded/Patches/Medical/BandAidController.cs` | `HealRoutine`: computa `allyTimeMult`, liga `HealingLegs` na cirurgia (exceto Médico+Mobile Surgery), `totalUseTime *= allyTimeMult`, seta `AllyAnimSpeedMult`; helper `ReleaseSurgeryImmobilize` em 5 cleanups + `CleanupHealState` + `ResetAllState` |

## PA-NN-MM resolvidos durante o build

| ID | Categoria · Impacto | Resumo da resolução |
| --- | --- | --- |
| PA-01-01 | B — Edge · 🟡 | Aceleração de tempo plena só no path de aliado remoto; parcial no bot local (MedEffect nativo com `UseTimeFor` que o guard do 072 não acelera). Documentado como limitação na §7; imobilização vale nos dois. |
| PA-01-02 | A — Gap · 🟢 | Ref `FirearmsAnimator.cs:465` adicionada à spec técnica; comentários `// 077` nos 3 pontos de `SetUseTimeMultiplier`. |
| PA-01-03 | B — Edge · 🟢 | `AllyAnimSpeedMult` setado no início de CADA `HealRoutine` (antes de `SetInHands`) — invariante; não depende do reset anterior. |

## Notas de implementação

- **Gate local (não coop-sync):** o operador é sempre o `MainPlayer`, então `CombatMedicAllyPerks` usa `SkillMultipliers.IsLocalClass` — sem mapa 057 nem packet (diferente do 076). Movimento/animação replicam via Fika nativo.
- **Fail-safe assimétrico:** `AllyHealTimeMult` fail-**open** (1f = tempo normal); `AllyMobileSurgeon` fail-**safe** (false = imobiliza). Sem CustomClasses, o ICM imobiliza todos na cirurgia de aliado (melhoria vs. hoje) e usa tempo padrão.
- **Guard `BandAidIsRedirecting` (072) mantido:** impede o 072 de encurtar o `UseTimeFor` vanilla em paralelo; o `MedAnimSpeedPatch` (072) fica desarmado (`Armed=false`) durante o redirect, então o valor `base * AllyAnimSpeedMult` passa intacto pelo Prefix.
- **Lock de movimento pareado:** `HealingLegs` é solto por `ReleaseSurgeryImmobilize` em todos os pontos que soltam `UsingMeds` (EmergencyDrop, paciente-morto, fim-normal, CancelHeal, DeactivateMedicMode) + reset do mult em `CleanupHealState` (médico-morto) e `ResetAllState` (mudança de raid). Reset incondicional é seguro.

## Mudanças posteriores

(vazio inicialmente — preenchido por `/apply-code-review`)

## Histórico

| Data | Evento |
| --- | --- |
| 2026-07-19 | Build concluído via `/code-mod` (5 arquivos; SDD completo — spec/tech/review-01 sem bloqueadores) |
