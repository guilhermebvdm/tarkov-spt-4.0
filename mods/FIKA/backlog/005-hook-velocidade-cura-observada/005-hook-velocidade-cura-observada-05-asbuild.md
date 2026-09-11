# 005 — Hook genérico de velocidade da animação de cura observada · As-Built

**Mod:** FIKA
**Spec funcional:** [005-hook-velocidade-cura-observada-01-spec.md](005-hook-velocidade-cura-observada-01-spec.md)
**Spec técnica:** [005-hook-velocidade-cura-observada-02-spec-tech.md](005-hook-velocidade-cura-observada-02-spec-tech.md)
**Última review técnica:** [005-hook-velocidade-cura-observada-03-spec-tech-review-01.md](005-hook-velocidade-cura-observada-03-spec-tech-review-01.md)
**Build inicial:** 2026-09-09

> Documentação **pós-implementação**. Reflete o estado real do código entregue pelo `/code-mod`. Quando o conteúdo aqui diverge da spec técnica, este documento ganha.

## Arquivos alterados (build inicial)

| Ação | Path | Resumo |
| --- | --- | --- |
| CRIADO | `mods/FIKA/modded/Fika-Plugin/Fika.Core/Main/ObservedClasses/HandsControllers/ObservedMedsSpeedHook.cs` | Classe pública `ObservedMedsSpeedHook` com o `Func<Player, Item, float>` estático `ExtraSpeedMultiplier` + `ResolveExtra` (null-guard, try/catch, validação finita/positiva). |
| MODIFICADO | `mods/FIKA/modded/Fika-Plugin/Fika.Core/Main/ObservedClasses/HandsControllers/ObservedMedsController.cs` | `ObservedStart`: aplica o extra do hook na 1ª parte do corpo (antes sem multiplicador nenhum). `HealthController_EffectRemovedEvent`: compõe o extra por cima do multiplicador nativo da skill Cirurgia (`(1f + mult) * extra` em vez de `1f + mult`). |
| MODIFICADO | `mods/FIKA/modded/Fika-Plugin/Fika.Core/FikaPlugin.cs` | `FikaVersion` `2.3.15` → `2.3.16`. |

## PA-01-MM resolvidos durante o build

> Já tinham sido aplicados na spec técnica antes deste build (ver Histórico de `02-spec-tech.md`, 2026-09-09) — replicados aqui 1:1 no código.

| ID | Categoria · Impacto | Resumo da resolução |
| --- | --- | --- |
| PA-01-01 | A — Gap · 🟡 | Doc XML de `ObservedMedsSpeedHook` documenta o ciclo de vida (assinar 1x no Awake do consumidor, campo `static` vale a sessão inteira). |
| PA-01-02 | B — Edge Case · 🟡 | `ResolveExtra` valida `player`/`item` não-nulos antes de invocar o delegate do consumidor. |
| PA-01-03 | C — Erro de Lógica · 🟢 | Comentário sobre `FikaLogger` corrigido para "confirmado" (não mais `TODO`). |
| PA-01-04 | B — Edge Case · 🟢 | Edição em `ObservedMedsController.cs` preserva o formato de chaves original — só as linhas novas (`ObservedMedsSpeedHook.ResolveExtra` + a linha do `SetUseTimeMultiplier`) foram tocadas. |

## Mudanças posteriores

### 2026-09-09 — `/apply-code-review` (rodada 01)

| ID | Achado | Ação |
| --- | --- | --- |
| CR-01-01 | `mod.json` desalinhado de `FikaVersion` | ✅ Aplicado — `mods/FIKA/mod.json:11` `2.3.15` → `2.3.16` |
| CR-01-02 | Doc XML sem aviso de main-thread síncrona | ✅ Aplicado — comentário `ref: CR-01-02` em `ObservedMedsSpeedHook.cs` |

Arquivos tocados nesta rodada: `mods/FIKA/mod.json` (modificado), `mods/FIKA/modded/Fika-Plugin/Fika.Core/Main/ObservedClasses/HandsControllers/ObservedMedsSpeedHook.cs` (modificado).

## Histórico

| Data | Evento |
| --- | --- |
| 2026-09-09 | Build concluído via `/code-mod`. |
| 2026-09-09 | Aplicação de 2 achados de code-review 01 via `/apply-code-review` — IDs: CR-01-01, CR-01-02. |
