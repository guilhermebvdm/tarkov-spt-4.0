# 016 — elite-nao-nativo-clona-boss · As-Built

**Mod:** TRL-DynamicSpawn
**Spec funcional:** [016-elite-nao-nativo-clona-boss-01-spec.md](016-elite-nao-nativo-clona-boss-01-spec.md)
**Spec técnica:** [016-elite-nao-nativo-clona-boss-02-spec-tech.md](016-elite-nao-nativo-clona-boss-02-spec-tech.md)
**Última review técnica:** [016-elite-nao-nativo-clona-boss-03-spec-tech-review-01.md](016-elite-nao-nativo-clona-boss-03-spec-tech-review-01.md)
**Build inicial:** 2026-09-23

> Documentação **pós-implementação**. Reflete o estado real do código entregue pelo `/code-mod` e atualizado por `/apply-code-review`. Quando o conteúdo aqui diverge da spec técnica, este documento ganha — a spec é planejamento, o asbuild é o que foi feito.

## Arquivos alterados (build inicial)

| Ação | Path | Resumo |
| --- | --- | --- |
| CRIADO | `mods/TRL-DynamicSpawn/modded/Client/Helpers/EliteFollowerMap.cs` | Tabela estática `WildSpawnType → WildSpawnType[]` com os guardas dedicados de 10 chefes únicos (inclui `sectantPriest→sectantWarrior`) e `GetFollowers(WildSpawnType)` |
| MODIFICADO | `mods/TRL-DynamicSpawn/modded/Client/Components/DynamicSpawnManager.cs` | Bloco `foreach (var entry in eliteEntries)` (antigo :690-774) reescrito: grunt-squad (Rogues/Raiders/Bloodhounds, clones idênticos, mecanismo inalterado) separado de boss+guardas (todo chefe único nasce 1x + N guardas do `EliteFollowerMap`, nunca clone); caso especial hardcoded do Knight removido (absorvido pela tabela genérica); `GroupChance` corrigida pra decidir só SE o grupo de guardas forma (não o tamanho) |
| MODIFICADO | `mods/TRL-DynamicSpawn/modded/Client/Models/TRLConfig.cs` | Initializer de `Bloodhounds` (linha 156) ganhou `MaxGroupSize = 4` e `MaxGroupSizeByMap` com todas as entradas em `4` (fallback em código caso `config.json` não exista) |
| MODIFICADO | `mods/TRL-DynamicSpawn/modded/Server/config/config.json` | Bloco `arenaFighterEvent`: `maxGroupSize` e todas as entradas de `maxGroupSizeByMap` de `3` para `4` |
| MODIFICADO | `mods/TRL-DynamicSpawn/modded/Server/config/config.default.json` | Mesmo ajuste do arquivo acima, no template do botão "PADRÃO" / `/resetConfig` |
| MODIFICADO | `mods/TRL-DynamicSpawn/modded/Server/Models/TRLConfig.cs` | **Fora do escopo original da spec técnica** — descoberto durante o build: existe uma cópia server-side (`System.Text.Json`) do mesmo `TRLConfig`/`EliteLocationInfo` do Client (`Newtonsoft`), mantida em sincronia manualmente, sem fonte compartilhada. `TRLRouters.CurrentConfig = new TRLConfig()` (linha 16) é o default em memória antes de qualquer `config.json` carregar. Aplicado o mesmo ajuste de `Bloodhounds` (`MaxGroupSize`/`MaxGroupSizeByMap` = 4) pra não deixar o default do server dessincronizado do Client/`config.json` |

## PA-NN-MM resolvidos durante o build

> Pontos da última review técnica que foram **aplicados como parte da implementação** (não como /apply-code-review posterior).

| ID | Categoria · Impacto | Resumo da resolução |
| --- | --- | --- |
| PA-01-01 | C — Erro de Lógica · 🟡 | Citações de linha corrigidas no comentário do `EliteFollowerMap.cs` (`bossKnight` → `:31`; `sectantWarrior`/`sectantPriest` → `:25,26`) — já aplicado na spec técnica antes do build, código nasceu correto |
| PA-01-02 | A — Gap · 🟡 | `GroupChance` implementada como "decide se o grupo forma" (não o tamanho): `0` nunca forma, `>=100` sempre forma no teto do mapa (determinístico), 1-99 sorteia se forma e, se sim, sorteia o tamanho |
| PA-01-03 | C — Erro de Lógica · 🟡 | Spec funcional atualizada antes do build para incluir Bloodhounds/Cultistas — sem impacto no código, só na documentação de intenção |
| PA-01-04 | C — Erro de Lógica · 🟢 | Diagrama da spec técnica corrigido antes do build — sem impacto no código |
| PA-01-05 | B — Edge Case · 🟢 | Não implementado (decisão: registrar como pendência de memória pra limpeza futura do painel Web, fora do escopo deste fix) |

## Mudanças posteriores

> Atualizado por `/apply-code-review` a cada rodada. Cada entrada lista os achados aplicados/rejeitados/pulados naquela rodada e os arquivos tocados.

(vazio inicialmente — preenchido por `/apply-code-review`)

## Compilação

`/compile-mod TRL-DynamicSpawn` (2026-09-23) — versão `3.7.24 → 3.7.25` (patch, bug fix) em Client e Server, sincronizadas em `Plugin.cs` (BepInPlugin), ambos `.csproj` e `ModMetadata.cs`. Build híbrido (2 projetos):

| Projeto | Resultado | Avisos |
| --- | --- | --- |
| `TRL-DynamicSpawn-Server.csproj` | ✓ 0 erros | 5 (todos pré-existentes — nullability em `ModMetadata.cs`/`TRLRouters.cs`, não relacionados a este fix) |
| `TRL-DynamicSpawn-Client.csproj` | ✓ 0 erros | 0 |

Instalado em `E:/Tarkov Red Line` (via `.spt-path`): Client → `BepInEx/plugins/TRL-DynamicSpawn/TRL-DynamicSpawn.dll`; Server → `SPT/user/mods/TRL-DynamicSpawn/` (DLL + `config/` + `wwwroot/`). DLLs anteriores arquivados automaticamente pelo script antes da sobrescrita (`TRL-DynamicSpawn-Server-260923-0228.dll`, `TRL-DynamicSpawn-260923-0228.dll`).

## Pendente de validação manual (fora do escopo do `/code-mod`)

Os itens abaixo exigem teste em raid e ficam como próximo passo do usuário:
- Sanitar em Laboratory com `groupChance=100`: confirmar 1x `bossSanitar` + sempre 2x `followerSanitar`, nunca 2x `bossSanitar`.
- Mesmo cenário com `groupChance=0`: confirmar Sanitar sempre sozinho.
- Mesmo cenário com `disableFollowers=true`: confirmar Sanitar sempre sozinho.
- Rogues/Raiders: confirmar squad de clones idêntico ao comportamento pré-fix (sem regressão).
- Bloodhounds em Customs/Woods: confirmar esquadrão sempre de 4 `arenaFighterEvent`, com 1 líder automático.
- Cultistas em raid noturna: confirmar 1x `sectantPriest` + guardas `sectantWarrior`, nunca 2 padres.
- Knight em mapa não-nativo: confirmar trio Knight/BigPipe/BirdEye igual a antes.

## Histórico

| Data | Evento |
| --- | --- |
| 2026-09-23 | Build concluído via `/code-mod` |
| 2026-09-23 | Compilado e instalado via `/compile-mod` (v3.7.25) — 0 erros em Client e Server |
