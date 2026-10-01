# 014 — Invasor de raid não vira inimigo dos bots (corrida com registro de jogador vivo) · As-Built

**Mod:** FIKA
**Spec funcional:** [014-invasor-nao-vira-inimigo-bots-01-spec.md](014-invasor-nao-vira-inimigo-bots-01-spec.md)
**Spec técnica:** [014-invasor-nao-vira-inimigo-bots-02-spec-tech.md](014-invasor-nao-vira-inimigo-bots-02-spec-tech.md)
**Última review técnica:** [014-invasor-nao-vira-inimigo-bots-03-spec-tech-review-01.md](014-invasor-nao-vira-inimigo-bots-03-spec-tech-review-01.md)
**Build inicial:** 2026-09-24

> Documentação **pós-implementação**. Reflete o estado real do código entregue pelo `/code-mod` e atualizado por `/apply-code-review`. Quando o conteúdo aqui diverge da spec técnica, este documento ganha — a spec é planejamento, o asbuild é o que foi feito.

## Arquivos alterados (build inicial)

| Ação | Path | Resumo |
| --- | --- | --- |
| MODIFICADO | `mods/FIKA/modded-V2/Fika-Plugin/Fika.Core/Main/Components/CoopHandler.cs` | `AddClientToBotEnemies` ganha uma espera (teto de 5s reais, `Time.time`) checando `GameWorld.GetAlivePlayerByProfileID` antes de chamar `AddActivePLayer` — fecha a corrida com `BotsGroup.AddEnemy` (vanilla) que falhava silenciosamente. Loga erro visível se o teto esgotar. |
| MODIFICADO | `mods/FIKA/modded-V2/Fika-Plugin/Fika.Core/FikaPlugin.cs` | Bump de versão `FikaVersion` 2.4.5 → 2.4.6 (patch, fix de bug). |
| MODIFICADO | `mods/FIKA/modded-V2/Fika-Plugin/Fika.Core/Fika.Core.csproj` | `<Version>` acompanha o bump acima (2.4.6). |

## PA-NN-MM resolvidos durante o build

> Pontos da última review técnica que foram **aplicados como parte da implementação** (não como /apply-code-review posterior).

| ID | Categoria · Impacto | Resumo da resolução |
| --- | --- | --- |
| PA-01-01 | A — Gap · 🟡 | Teto da nova espera trocado de contagem de frames pra segundos reais (`Time.time`) — um host headless não tem framerate previsível, contar frames não dava um teto de tempo real confiável. |

## Mudanças posteriores

> Atualizado por `/apply-code-review` a cada rodada. Cada entrada lista os achados aplicados/rejeitados/pulados naquela rodada e os arquivos tocados.

### Rodada `/code-review` 01 (2026-09-24)

- **CR-01-01** (✅ Aplicado) — `CoopHandler.cs`: log `#if DEBUG` adicionado logo após o laço de espera de `AddClientToBotEnemies`, disparando só quando a espera precisou de fato iterar (a corrida do backlog 014 aconteceu e foi resolvida pelo retry) — diferencia esse caso do caminho "já estava vivo, 0 iterações" nos logs de diagnóstico.

## Histórico

| Data | Evento |
| --- | --- |
| 2026-09-24 | Build concluído via `/code-mod` — `Fika.Core` v2.4.6 (fork `modded-V2`) |
| 2026-09-24 | Aplicação de 1 achado de code-review 01 via `/apply-code-review` — ID: CR-01-01 |
