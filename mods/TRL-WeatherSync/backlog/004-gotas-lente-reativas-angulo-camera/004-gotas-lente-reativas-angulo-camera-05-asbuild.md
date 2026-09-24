# 004 — Gotas na Lente Reativas ao Ângulo da Câmera · As-Built

**Mod:** TRL-WeatherSync  
**Spec funcional:** [004-gotas-lente-reativas-angulo-camera-01-spec.md](004-gotas-lente-reativas-angulo-camera-01-spec.md)  
**Spec técnica:** [004-gotas-lente-reativas-angulo-camera-02-spec-tech.md](004-gotas-lente-reativas-angulo-camera-02-spec-tech.md)  
**Última review técnica:** [004-gotas-lente-reativas-angulo-camera-03-spec-tech-review-01.md](004-gotas-lente-reativas-angulo-camera-03-spec-tech-review-01.md)  
**Build inicial:** 2026-09-12  

> Documentação **pós-implementação**. Reflete o estado real do código entregue e atualizado por code-review. Quando o conteúdo aqui diverge da spec técnica, este documento ganha — a spec é planejamento, o asbuild é o que foi feito.

## Arquivos alterados (build inicial)

| Ação | Path | Resumo |
| --- | --- | --- |
| CRIADO | `mods/TRL-WeatherSync/modded/Client/Patches/CameraLensRainDropsPatch.cs` | Patch `ModulePatch` com `[PatchPrefix]` em `GClass986.Update(float dt)` modulando a taxa de chuva na câmera conforme o ângulo vertical. |
| MODIFICADO | `mods/TRL-WeatherSync/modded/Client/Plugin.cs` | Bump de versão para `1.3.0`, registro das 5 `ConfigEntry` da seção `Lens Drops` e ativação de `CameraLensRainDropsPatch`. |
| MODIFICADO | `mods/TRL-WeatherSync/modded/Client/TRL-WeatherSync.csproj` | Sincronização de `<Version>` para `1.3.0`. |
| MODIFICADO | `mods/TRL-WeatherSync/PROPRIEDADES.md` | Documentação da seção `Lens Drops` com as novas propriedades e versão `v1.3.0`. |

## PA-NN-MM resolvidos durante o build

> Pontos da última review técnica aplicados como parte da implementação:

| ID | Categoria · Impacto | Resumo da resolução |
| --- | --- | --- |
| PA-01-01 | C — Erro de Lógica · 🟡 | Limite de spawn por rajada limitado entre 1 e 12 gotas com verificação `__instance.Queue_0.Count > 0`, prevenindo esgotamento do pool. |
| PA-01-02 | B — Edge Case · 🟡 | Validação defensiva de nulidade para `__instance.Transform_0` no início do Prefix. |
| PA-01-03 | A — Gap · 🟢 | Fallback imediato para o código nativo (retorno `true`) caso `EnableLensDropsTuning` esteja desmarcado ou ocorra exceção capturada no bloco `catch`. |

## Mudanças posteriores

(Nenhuma divergência identificada — implementação bate 100% com a spec técnica aprovada).

## Histórico

| Data | Evento |
| --- | --- |
| 2026-09-12 | Build inicial concluído e validado localmente. |
