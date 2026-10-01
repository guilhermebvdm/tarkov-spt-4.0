# 004 — Swap de carregador rejeitado após curar aliado (colisão com transição de mãos não-magazine) · As-Built

**Mod:** FIKA
**Spec funcional:** [004-colisao-cura-swap-magazine-01-spec.md](004-colisao-cura-swap-magazine-01-spec.md)
**Spec técnica:** [004-colisao-cura-swap-magazine-02-spec-tech.md](004-colisao-cura-swap-magazine-02-spec-tech.md)
**Última review técnica:** [004-colisao-cura-swap-magazine-03-spec-tech-review-01.md](004-colisao-cura-swap-magazine-03-spec-tech-review-01.md)
**Build inicial:** 2026-09-08

> Documentação **pós-implementação**. Reflete o estado real do código entregue pelo `/code-mod` e atualizado por `/apply-code-review`. Quando o conteúdo aqui diverge da spec técnica, este documento ganha — a spec é planejamento, o asbuild é o que foi feito.

## Arquivos alterados (build inicial)

| Ação | Path | Resumo |
| --- | --- | --- |
| MODIFICADO | `mods/FIKA/modded/Fika-Plugin/Fika.Core/Main/ObservedClasses/ObservedInventoryController.cs` | Renomeado `IsSelfReferentialMagazineSwap` → `IsSelfReferentialHandsTransition`; condição de tolerância ampliada de `movedItem is MagazineItemClass` para `movedItem != null && movedItem != weapon` — tolera qualquer transição de mãos recente do mesmo jogador na mesma arma (carregador, item de cura, granada, faca, reanimação), preservando o bloqueio de um saque/guarda real da própria arma |
| MODIFICADO | `mods/FIKA/modded/Fika-Plugin/Fika.Core/FikaPlugin.cs` | Bump `FikaVersion`: `2.3.14` → `2.3.15` |
| MODIFICADO | `mods/FIKA/mod.json` | Bump do componente `plugin`: `2.3.14` → `2.3.15` |

Implementação seguiu o stub da spec técnica §5 sem desvios — nenhuma divergência entre o planejado e o código real (diferente do item `003`, que teve um desvio confirmado durante o build). Nenhum novo Harmony patch foi criado: a infraestrutura de correlação (`InOutHandsProcessTimestampPatch.cs`) do item `003` já capturava genericamente qualquer item que abrisse a transição — a lacuna estava inteiramente na condição de tolerância consumidora, agora corrigida.

## PA-NN-MM resolvidos durante o build

> Pontos da última review técnica que foram **aplicados como parte da implementação** (não como /apply-code-review posterior).

| ID | Categoria · Impacto | Resumo da resolução |
| --- | --- | --- |
| PA-01-03 | C — Convenção · 🟢 | Comentários inline do método renomeado reescritos para citar `// ref: item 004 (colisão cura + swap magazine)` em vez do caminho completo do arquivo de spec técnica |

PA-01-01 e PA-01-02 (itens de validação in-game adicionados ao checklist da spec técnica) foram resolvidos como edição da própria spec técnica **antes** deste build (não geram mudança de código) — permanecem como itens de checklist pendentes de validação in-game, listados na spec técnica §8.

## Mudanças posteriores

> Atualizado por `/apply-code-review` a cada rodada. Cada entrada lista os achados aplicados/rejeitados/pulados naquela rodada e os arquivos tocados.

### Rodada 01 (2026-09-08) — [004-colisao-cura-swap-magazine-04-code-review-01.md](004-colisao-cura-swap-magazine-04-code-review-01.md)

- **Aplicado:** `CR-01-01` — comentário de documentação do método `IsSelfReferentialHandsTransition` (`ObservedInventoryController.cs:217`) reescrito de `"Ver 004-colisao-cura-swap-magazine-02-spec-tech.md §1.3."` para `"Ver spec técnica do item 004 §1.3."`, fechando a aplicação de `PA-01-03` (item 003) nos 3 pontos do arquivo onde a referência aparecia.
- Rejeitados: nenhum. Pulados: nenhum.

## Histórico

| Data | Evento |
| --- | --- |
| 2026-09-08 | Build concluído via `/code-mod` |
| 2026-09-08 | Aplicação de 1 achado de code-review 01 via `/apply-code-review` — ID: CR-01-01 |
