# 001 — Otimização do Limitador de IA por Distância (AI Limiter + LOD) · As-Built

**Mod:** SAIN
**Spec funcional:** [001-perf-limitador-ia-lod-01-spec.md](001-perf-limitador-ia-lod-01-spec.md)
**Spec técnica:** [001-perf-limitador-ia-lod-02-spec-tech.md](001-perf-limitador-ia-lod-02-spec-tech.md)
**Última review técnica:** [001-perf-limitador-ia-lod-03-spec-tech-review-01.md](001-perf-limitador-ia-lod-03-spec-tech-review-01.md)
**Build inicial:** 2026-09-19

> Documentação **pós-implementação**. Reflete o estado real do código entregue pelo `/code-mod`. Quando o conteúdo aqui diverge da spec técnica, este documento ganha — a spec é planejamento, o asbuild é o que foi feito.

## Arquivos alterados (build inicial)

| Ação | Path | Resumo |
| --- | --- | --- |
| MODIFICADO | `mods/SAIN/modded-multithread/SAIN/Components/BotComponent.cs` | Remove as 4 `const` privadas de LOD; `GetMinDistanceToHumanPlayer()` reusa `AILimit.ClosestPlayerDistanceSqr` (linear, apesar do nome) com fallback síncrono; adiciona `_wasCloseToHuman` e dead-band na fronteira Tier0/Tier1 |
| MODIFICADO | `mods/SAIN/modded-multithread/SAIN/Preset/GlobalSettings/Categories/General/AILimitSettings.cs` | Adiciona 5 campos públicos novos (`LODCloseDistance`, `LODMidDistance`, `LODCloseDistanceMargin`, `LODMidIntervalSeconds`, `LODFarIntervalSeconds`), editáveis no F6 — só adição, nada removido |
| MODIFICADO | `mods/SAIN/docs/modded-multithread/01-arquitetura-multithread-e-lod.md` | Nova nota no "Ponto 6" documentando a coexistência entre `SAINAILimit` (sensorial) e o LOD (tick-rate) |
| MODIFICADO | `mods/SAIN/PROPRIEDADES.md` | 3 linhas novas catalogando as propriedades do LOD |
| **NÃO TOCADO** | `mods/SAIN/modded-multithread/SAIN/Classes/Bot/SAINAILimit.cs` | Revertido em relação à primeira versão da spec técnica (`PA-01-01`) — fonte de dado sensorial permanece exatamente como estava |

## PA-01-MM resolvidos durante o build

> Pontos da Review Técnica 01 que foram resolvidos na própria spec técnica antes do código (não houve achado novo durante a implementação em si).

| ID | Categoria · Impacto | Resumo da resolução |
| --- | --- | --- |
| PA-01-01 | C — Erro de Lógica · 🔴 | Direção da unificação invertida — LOD lê do `SAINAILimit`, não o contrário; armadilha do nome `ClosestPlayerDistanceSqr` (é linear) documentada e respeitada no código (sem `Sqrt` nesse caminho) |
| PA-01-02 | B — Edge Case · 🟡 | Fallback explícito (`aiLimitDist >= 0f`) implementado em `GetMinDistanceToHumanPlayer()` |
| PA-01-03 | C — Erro de Lógica · 🟢 | N/A para o código (era só redação da spec) |
| PA-01-04 | C — Erro de Lógica · 🟢 | Atributo real confirmado como `[Advanced]` (não `[IsAdvanced]` como o stub sugeria) durante a implementação — aplicado corretamente nos 3 campos avançados |

## Descoberta durante a implementação (fora do escopo de achado novo — aplicada direto)

Ao verificar os atributos reais em `ConfigAttributes.cs` antes de escrever o código, confirmou-se que o atributo de "avançado" se chama `[Advanced]` (classe `AdvancedAttribute`), não `[IsAdvanced]` como os stubs da spec técnica (baseados em suposição, não em leitura prévia desse arquivo específico) sugeriam. Corrigido silenciosamente por ser uma correção de nome de atributo real, sem impacto de design — mesma intenção (marcar os 3 campos de ajuste fino como avançados), só o nome certo do atributo.

## Mudanças posteriores

| Data | Evento |
| --- | --- |
| 2026-09-20 | `/code-review` (rodada 01): 2 achados (`CR-01-01` 🟡, `CR-01-02` 🟢). `CR-01-01` aplicado via `/apply-code-review` — `BotComponent.cs:239` trocado de `>= 0f` para `> 0f` (guard contra o default não-inicializado `0f` de `SAINAILimit.ClosestPlayerDistanceSqr`). `CR-01-02` é só recomendação de sequenciamento de validação in-game, sem mudança de código. Recompilado com 0 erros. |

## Histórico

| Data | Evento |
| --- | --- |
| 2026-09-19 | Build concluído via `/code-mod` — compilado com 0 erros (6 avisos pré-existentes, não relacionados a este item) |
| 2026-09-20 | Code review 01 aplicada (`CR-01-01`); recompilado com 0 erros |
