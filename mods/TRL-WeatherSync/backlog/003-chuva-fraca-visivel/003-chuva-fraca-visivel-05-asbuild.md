# 003 — Chuva Fraca Mais Visível (Tamanho Mínimo de Gota) · As-Built

**Mod:** TRL-WeatherSync  
**Spec funcional:** [003-chuva-fraca-visivel-01-spec.md](003-chuva-fraca-visivel-01-spec.md)  
**Spec técnica:** [003-chuva-fraca-visivel-02-spec-tech.md](003-chuva-fraca-visivel-02-spec-tech.md)  
**Última review técnica:** [003-chuva-fraca-visivel-03-spec-tech-review-01.md](003-chuva-fraca-visivel-03-spec-tech-review-01.md)  
**Build inicial:** 2026-09-12  

> Documentação **pós-implementação**. Reflete o estado real do código entregue e atualizado por code-review. Quando o conteúdo aqui diverge da spec técnica, este documento ganha — a spec é planejamento, o asbuild é o que foi feito.

## Arquivos alterados (build inicial)

| Ação | Path | Resumo |
| --- | --- | --- |
| CRIADO | `mods/TRL-WeatherSync/modded/Client/Patches/RainDropVisibilityPatch.cs` | Patch `ModulePatch` com `[PatchPostfix]` em `RainFallDrops.method_2` aplicando piso mínimo de tamanho no shader `_Size`. |
| MODIFICADO | `mods/TRL-WeatherSync/modded/Client/Plugin.cs` | Bump de versão para `1.2.0`, adição de `ConfigEntry<float> MinRainDropSize` e habilitação de `RainDropVisibilityPatch`. |
| MODIFICADO | `mods/TRL-WeatherSync/modded/Client/TRL-WeatherSync.csproj` | Sincronização de `<Version>` para `1.2.0`. |
| MODIFICADO | `mods/TRL-WeatherSync/PROPRIEDADES.md` | Documentação da seção `Rain` e da nova propriedade `Min Rain Drop Size` (versão atualizada para `v1.2.0`). |

## PA-NN-MM resolvidos durante o build

> Pontos da última review técnica que foram aplicados diretamente como parte da implementação:

| ID | Categoria · Impacto | Resumo da resolução |
| --- | --- | --- |
| PA-01-01 | C — Erro de Lógica · 🟡 | Flag estática `_hasLoggedError` implementada para garantir que qualquer erro em `RainFallDrops.method_2` (executado por frame) registre apenas uma vez no console. |
| PA-01-02 | B — Edge Case · 🟡 | Proporção de aspecto da tela preservada multiplicando o piso do componente X por `(float)Screen.height / (float)Screen.width`, alinhado com o shader nativo. |
| PA-01-03 | A — Gap · 🟢 | Guarda nula antecipada `if (minSize <= 0f || ___material_0 == null) return;` no topo do Postfix. |

## Mudanças posteriores

(Nenhuma divergência identificada — implementação bate 100% com o stub e design revisado da spec técnica).

## Histórico

| Data | Evento |
| --- | --- |
| 2026-09-12 | Build inicial concluído e validado com sucesso. |
