# 006 — Estômago: agachar probabilístico · As-Built

**Mod:** TRL-ImmersiveCombatMedicine
**Spec funcional:** [006-estomago-agachar-01-spec.md](006-estomago-agachar-01-spec.md)
**Spec técnica:** [006-estomago-agachar-02-spec-tech.md](006-estomago-agachar-02-spec-tech.md)
**Última review técnica:** [006-estomago-agachar-03-spec-tech-review-01.md](006-estomago-agachar-03-spec-tech-review-01.md)
**Build inicial:** 2026-07-19

> Documentação **pós-implementação**. Reflete o estado real do código entregue pelo `/code-mod` e atualizado por `/apply-code-review`. Quando o conteúdo aqui diverge da spec técnica, este documento ganha — a spec é planejamento, o asbuild é o que foi feito.

## Arquivos alterados (build inicial)

| Ação | Path | Resumo |
| --- | --- | --- |
| CRIADO | `mods/TRL-ImmersiveCombatMedicine/modded/Patches/Trauma/TraumaStomachConsumer.cs` | Consumidor de estômago: roll p=75%/25% (pk latched D8) na entrada real de `StomachZeroed`, cooldown compartilhado com reserva atômica, agachar via chamada DIRETA em `TraumaPose` (humano/bot), pumps próprios. |
| MODIFICADO | `mods/TRL-ImmersiveCombatMedicine/modded/Patches/Trauma/TraumaPose.cs` | `KindWord`/`AbsorbIfCycleEngaged`/`CancelKind`/`BotCrouchDip` ganham `TraumaRegion`; dedup do `Defer` casa por `(player, kind, region)`; call sites de log NOOP/EXECUTED/DEFERRED/CANCELED/ABSORB usam o word por região. |
| MODIFICADO | `mods/TRL-ImmersiveCombatMedicine/modded/Patches/Trauma/TraumaLegsConsumer.cs` | Call site único: `CancelKind(InvoluntaryCrouch, TraumaRegion.Legs, "toggle-off")`. |
| MODIFICADO | `mods/TRL-ImmersiveCombatMedicine/modded/Patches/Trauma/HealthPatches.cs` | Bloco legado "sem ar" de estômago (stamina zerada + pose forçada + voz "Gut", inclusive bots) removido por inteiro; comentário-lápide D10 no lugar. |
| MODIFICADO | `mods/TRL-ImmersiveCombatMedicine/modded/TRLImmersiveCombatMedicinePlugin.cs` | Versão 1.7.0; tooltip INERTE em `Sistema de Estomago`; rename-at-delivery `Stomach Effects (item 006)` → `Stomach Effects` (ON); 2 novos binds seção 10 (chances de agachar sem/com analgésico); 4º bloco de `MigrateOrphanedConfigKeys`; `AddComponent<TraumaStomachConsumer>()`. |
| MODIFICADO | `mods/TRL-ImmersiveCombatMedicine/PROPRIEDADES.md` | Seção 10 nova (2 entries); INERTE na seção 2; tooltip real na seção 6; linha na tabela Renomeadas; Histórico de Alterações. |

## PA-NN-MM resolvidos durante o build

> Pontos da última review técnica que foram **aplicados como parte da implementação** (não como /apply-code-review posterior).

| ID | Categoria · Impacto | Resumo da resolução |
| --- | --- | --- |
| PA-01-01 | B — Edge Case · 🟡 | Resolvido **na spec técnica ANTES deste build** (não durante o build em si): a reserva atômica do cooldown (`TraumaEngine.ReportOneShotExecuted` chamado IMEDIATAMENTE após o pré-check de cooldown passar, ANTES de invocar `TryInvoluntaryCrouch`/`BotCrouchDip`) já veio corrigida no stub §5 da spec técnica lida por este `/code-mod`. Implementado exatamente como a spec corrigida descreve, em `TraumaStomachConsumer.OnTransitionCore` — sem desvio. |
| PA-01-02 | C — Erro de Lógica · 🟢 | Resolvido **na spec técnica ANTES deste build**: a citação de `Random.value` foi corrigida para apontar só `VoiceAndHealthUtils.cs:51` (com `MedicalLogic.cs:366` anotado como `Random.Range`, gênero diferente). O código implementado usa `Random.value` — comentário inline no `TraumaStomachConsumer.cs` reflete a citação já corrigida. |

## Mudanças posteriores

> Atualizado por `/apply-code-review` a cada rodada. Cada entrada lista os achados aplicados/rejeitados/pulados naquela rodada e os arquivos tocados.

(vazio inicialmente — preenchido por `/apply-code-review`)

## Histórico

| Data | Evento |
| --- | --- |
| 2026-07-19 | Build concluído via `/code-mod`. Quarto consumidor do motor 002 (após 003/004/005) — zero patch Harmony novo, zero mudança no motor (`TraumaEngine`/`TraumaEngineState`/`TraumaMatrixResolver`). 1 arquivo criado + 4 modificados + `PROPRIEDADES.md`. Versão 1.6.1 → 1.7.0. Call sites existentes de `TraumaPose` (003/004) confirmados compilando: `TraumaLegsConsumer.cs:134` (`BotCrouchDip(p)`, região default `Legs` inalterada), `TraumaLegsConsumer.cs:137` (`TryInvoluntaryCrouch(p, TraumaRegion.Legs, kind)`, assinatura inalterada), `TraumaLegsConsumer.cs:212` (`CancelKind` atualizado explicitamente para `(kind, TraumaRegion.Legs, reason)` — único call site que precisava de edição por ganhar parâmetro obrigatório novo). `TraumaFallCycleConsumer.cs` não toca nenhuma das APIs modificadas (usa `CancelFallsFor`, que manteve assinatura). Nenhum desvio da spec técnica. |
