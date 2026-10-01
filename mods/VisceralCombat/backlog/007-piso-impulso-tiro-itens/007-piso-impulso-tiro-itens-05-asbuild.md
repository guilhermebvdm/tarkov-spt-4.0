# 007 — Impulso de Tiro Menos Dependente do Peso do Item · As-Built

**Mod:** VisceralCombat
**Spec funcional:** [007-piso-impulso-tiro-itens-01-spec.md](007-piso-impulso-tiro-itens-01-spec.md)
**Spec técnica:** [007-piso-impulso-tiro-itens-02-spec-tech.md](007-piso-impulso-tiro-itens-02-spec-tech.md)
**Última review técnica:** [007-piso-impulso-tiro-itens-03-spec-tech-review-01.md](007-piso-impulso-tiro-itens-03-spec-tech-review-01.md)
**Build inicial:** 2026-09-21

> Documentação **pós-implementação**. Reflete o estado real do código entregue pelo `/code-mod` e atualizado por `/apply-code-review`. Quando o conteúdo aqui diverge da spec técnica, este documento ganha — a spec é planejamento, o asbuild é o que foi feito.

## Arquivos alterados (build inicial)

| Ação | Path | Resumo |
| --- | --- | --- |
| MODIFICADO | `mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/BodiesImpulsePatch.cs` | Ramo de item largado (`ProcessImpulse`): impulso de bala agora usa massa efetiva com teto/piso (`Mathf.Clamp(lootRb.mass, 0.05f, 0.5f)`) e `ForceMode.VelocityChange` em vez de `ForceMode.Impulse` — itens pesados (arma, capacete) passam a reagir de forma perceptível, itens leves (máscara, fone) mantêm o comportamento atual. Ramo de corpo/ragdoll (após o `return`) confirmado byte-a-byte inalterado. |
| MODIFICADO | `mods/VisceralCombat/modded/VisceralCombat/VisceralCombat/VisceralEntry.cs` | Versão `3.11.10` → `3.11.11`. |
| MODIFICADO | `mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.csproj` | `<Version>` `3.11.10` → `3.11.11`. |

## PA-NN-MM resolvidos durante o build

> Pontos da última review técnica que foram **aplicados como parte da implementação** (não como /apply-code-review posterior).

| ID | Categoria · Impacto | Resumo da resolução |
| --- | --- | --- |
| PA-01-01 | C — Erro de Lógica · 🟡 Importante | Já resolvido na spec técnica antes do `/code-mod` (piso `MinMassKg = 0.05f` via `Mathf.Clamp`); o código implementado segue exatamente esse stub. |
| PA-01-02 | A — Gap · 🟢 Menor | Documental — limitação de rotação/torque não corrigida já estava descrita em §1/§7 da spec técnica antes do build; comentário inline replicado no código (`BodiesImpulsePatch.cs`) explicando a limitação aceita. |

## Mudanças posteriores

> Atualizado por `/apply-code-review` a cada rodada. Cada entrada lista os achados aplicados/rejeitados/pulados naquela rodada e os arquivos tocados.

- **2026-09-21 — [06-fix-01](007-piso-impulso-tiro-itens-06-fix-01.md):** `ForceMode.VelocityChange` (não-nativo do PhysX em `AddForceAtPosition`) causava reação fraca/nula por torque imprevisível — trocado por impulso escalado + `ForceMode.Impulse` (nativo). `BodiesImpulsePatch.cs` tocado. Build 3.12.1.

## Histórico

| Data | Evento |
| --- | --- |
| 2026-09-21 | Build concluído via `/code-mod` — 0 erros, mesmos 20 warnings pré-existentes do mod. Instalado automaticamente em `E:/Tarkov Red Line/BepInEx/plugins/VisceralCombat`. |
| 2026-09-21 | [06-fix-01](007-piso-impulso-tiro-itens-06-fix-01.md) aplicado — `ForceMode.Impulse` (nativo) substitui `ForceMode.VelocityChange` (custom, torque imprevisível). Build 3.12.1, 0 erros. |
