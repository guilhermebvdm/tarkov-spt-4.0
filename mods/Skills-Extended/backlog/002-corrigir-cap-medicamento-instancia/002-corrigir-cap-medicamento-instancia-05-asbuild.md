# 002 — Corrigir cap de medicamento por instância · As-Built

**Mod:** Skills-Extended
**Spec funcional:** [002-corrigir-cap-medicamento-instancia-01-spec.md](002-corrigir-cap-medicamento-instancia-01-spec.md)
**Spec técnica:** [002-corrigir-cap-medicamento-instancia-02-spec-tech.md](002-corrigir-cap-medicamento-instancia-02-spec-tech.md)
**Última review técnica:** [002-corrigir-cap-medicamento-instancia-03-spec-tech-review-01.md](002-corrigir-cap-medicamento-instancia-03-spec-tech-review-01.md)
**Build inicial:** 2026-09-07

> Documentação pós-implementação. Reflete o estado real do código entregue pelo `/code-mod`.

## Arquivos alterados (build inicial)

| Ação | Path | Resumo |
| --- | --- | --- |
| CRIADO | `mods/Skills-Extended/modded/Plugin/Skills/FieldMedicine/Patches/StimulatorHealthControllerTrackerPatch.cs` | Captura o `HealthController` (dono real do efeito) a partir de `Stimulator.method_6`, resolvido por assinatura (`Class2222` + `bool`), antes de `smethod_0` rodar |
| MODIFICADO | `mods/Skills-Extended/modded/Plugin/Skills/FieldMedicine/Patches/StimulatorApplyBuffPatch.cs` | Resolve o `SkillManager` do dono real do efeito (via `SkillManager_0`, reflection que sobe a hierarquia de classes) em vez de `GameUtils.GetSkillManager()` (sempre MainPlayer); fallback preservado |
| MODIFICADO | `mods/Skills-Extended/modded/Common/SkillsExtendedInfo.cs` | Bump de versão 2.2.3 → 2.2.4 (mesmo bump do item 001) |
| MODIFICADO | `mods/Skills-Extended/modded/Plugin/Plugin.csproj` | Bump de versão 2.2.3 → 2.2.4 |
| MODIFICADO | `mods/Skills-Extended/modded/Server/Server.csproj` | Bump de versão 2.2.1 → 2.2.4 |

## PA-NN-MM resolvidos durante o build

| ID | Categoria · Impacto | Resumo da resolução |
| --- | --- | --- |
| PA-01-01 | C — Erro de Lógica · (era 🟡) | `HealthController` confirmado property pública em `ActiveHealthController.GClass3008:361` — já resolvido na spec técnica antes do build |
| PA-01-02 | A — Gap · (era 🟡) | `SkillManager_0` resolvido via reflection que sobe `BaseType` — já resolvido na spec técnica antes do build |
| PA-01-03 | A — Gap · (era 🟡) | `method_6` resolvido por assinatura (`Class2222` + `bool`) em vez de nome literal — implementado exatamente como mitigado |

## Achado durante o build (não estava em nenhuma review)

Durante a compilação, a suposição da spec técnica de que `newSkillCap` (em `StimulatorApplyBuffPatch`) seria `float?` (motivando um `?? limits.Value.y` no stub) se provou **incorreta** — o compilador rejeitou (`CS0019`) porque `FieldMedicineSkillCap` é do tipo `SkillManager.SkillBuffClass` (classe wrapper com conversão implícita pra `float`, não um `float`/`float?` puro — `Plugin/Skills/Core/SkillManagerExt.cs:29`). Como o membro final da cadeia `?.` é um tipo referência, não um tipo valor, a expressão não vira `Nullable<T>`. Corrigido removendo o `?? limits.Value.y` (o código final é idêntico ao original nesse ponto específico) e documentando a correção na spec técnica §7.

## Mudanças posteriores

### 2026-09-07 — `/apply-code-review` (rodada 01)

| ID | Resultado | Arquivo tocado |
| --- | --- | --- |
| CR-01-01 | ✅ Aplicado | `mods/Skills-Extended/modded/Plugin/Skills/FieldMedicine/Patches/StimulatorHealthControllerTrackerPatch.cs` (guard de null adicionado na property e no método resolvidos por reflection) |

Versão bump: 2.2.4 → 2.2.5 (mesmo bump do item 001).

## Histórico

| Data | Evento |
| --- | --- |
| 2026-09-07 | Build concluído via `/code-mod` — Plugin compilado com 0 erros de código após 2 ajustes descobertos durante a compilação (using EFT ausente; suposição de nullable incorreta) |
| 2026-09-07 | `/apply-code-review` rodada 01 — CR-01-01 aplicado; recompilado com 0 erros |
