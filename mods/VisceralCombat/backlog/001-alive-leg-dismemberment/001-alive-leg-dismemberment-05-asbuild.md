# 001 — Desmembramento de Perna em Bots Vivos · As-Built

**Mod:** VisceralCombat
**Spec funcional:** [001-alive-leg-dismemberment-01-spec.md](001-alive-leg-dismemberment-01-spec.md)
**Spec técnica:** [001-alive-leg-dismemberment-02-spec-tech.md](001-alive-leg-dismemberment-02-spec-tech.md)
**Última review técnica:** [001-alive-leg-dismemberment-03-spec-tech-review-01.md](001-alive-leg-dismemberment-03-spec-tech-review-01.md)
**Build inicial:** 2026-08-11

> Documentação **pós-implementação**. Reflete o estado real do código entregue pelo `/code-mod` e atualizado por `/apply-code-review`. Quando o conteúdo aqui diverge da spec técnica, este documento ganha — a spec é planejamento, o asbuild é o que foi feito.

## Arquivos alterados (build inicial)

| Ação | Path | Resumo |
| --- | --- | --- |
| CRIADO | `mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Dismemberment.Classes/LivingDismembermentController.cs` | Controlador de bots vivos desmembrados (prone, sangramento 10 HP/s, vozes, decalques) |
| MODIFICADO | `mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Combined.Patches/KillPatch.cs` | Anexo do `LivingDismembermentController` em desmembramento de perna em bot vivo |
| MODIFICADO | `mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Dismemberment.Patches/BleedPatch.cs` | Parentar efeitos visuais de sangue na raiz do jogador com localScale Vector3.one |
| MODIFICADO | `mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Ragdolls.Classes/RagdollHelperClass.cs` | Escala limbSize ajustada para (0.1f, 0.1f, 0.1f) evitando zero-vector warnings |
| MODIFICADO | `mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/BodiesImpulsePatch.cs` | Força de impacto universal p = m * v em N.s para todas as munições em ragdolls |
| MODIFICADO | `mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/LimbKillPatch.cs` | Removida força duplicada raw-speed para prevenir arremessos irrealistas |

## PA-NN-MM resolvidos durante o build

| ID | Categoria · Impacto | Resumo da resolução |
| --- | --- | --- |
| PA-01-01 | B — Edge Case · 🟢 | Definida escala `limbSize` para `(0.1f, 0.1f, 0.1f)` prevenindo aviso C++ `LookRotation` |

## Mudanças posteriores

(vazio inicialmente — preenchido por `/apply-code-review`)

## Histórico

| Data | Evento |
| --- | --- |
| 2026-08-11 | Build concluído via `/code-mod` |
