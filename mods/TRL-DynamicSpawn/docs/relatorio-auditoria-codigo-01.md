---
title: "Relatório de Auditoria Técnica de Código — TRL-DynamicSpawn v3.2.9 (Review 01, --perf)"
date: 2026-08-22
status: 🟢 Vivo
authors: Claude (investigação de performance 2026-08-22)
---

# Relatório de Auditoria Técnica de Código — TRL-DynamicSpawn v3.2.9 (Review 01, --perf)

## 1. Resumo Executivo da Auditoria

Auditoria de performance do mod **TRL-DynamicSpawn v3.2.9** (client [mods/TRL-DynamicSpawn/Client/](../Client/), server [mods/TRL-DynamicSpawn/Server/](../Server/)), produzida a partir de **investigação medida em jogo** (2026-08-22, raid Customs hospedada localmente, servidor de produção remoto) cruzada com leitura do fonte e dos assemblies decompilados do EFT 0.16.9. Base de código: commit `f132d3b8` (última versão, main = origin/main).

**Evidência de runtime que ancora este relatório (baseline 2026-08-22):**
- Captura CapFrameX (15:03:41–15:21:39): 100% dos 189 spikes >100 ms são CPU-bound (`CpuActive` ≈ frametime; GPU ~21% de uso).
- **Stutter metronômico de 10,0 s cravados** na fase estável sem bots, em fase constante (+4,2 s) com os bursts de "generate profile with same id" do errors.log.
- **111 requisições `/trldynamicspawn/getConfig`** durante a raid (RequestHandler), ~1 a cada 10,3 s — todas com sucesso (server remoto).
- RAM do processo: 10,8→33,0 GB em ~12 min (~2 GB/min de lixo coletável); colapso 33→0,9 GB às 15:16:06-09 com freeze de 3,3 s dentro; pool de perfis client-side oscilando **337→1155** (`profilesInList`); 414 perfis gerados para 59 bots spawnados; `bot/generate` continuando às 15:21:05-10 com a raid vazia.
- 44 `NullReferenceException` em `EFT.BotSpawner.TrySpawnFreeAndDelay → GClass1890.TrySpawnFreeInner`.
- ~12,7k linhas de log do mod na raid, maioria em `LogWarning` **fora do gate** `Enable Debug Logs`.

### Tabela Resumo de Severidade

| Severidade | Quantidade | Descrição |
|---|:---:|---|
| 🔴 **Crítico** | 0 | — |
| 🟠 **Alto** | 3 | HTTP síncrono periódico na main thread; pool de perfis sem limite (~2 GB/min de churn); cancelamento global de spawn gerando NREs |
| 🟡 **Médio** | 4 | Ciclo de vida global do poller; ausência de backoff em falha; waves com raid vazia; logging Warning sem gate |
| 🔵 **Baixo** | 0 | — |
| 💡 **Otimização** | 0 | — |

**Rodadas (decisão do plano aprovado em 2026-08-22):** rodada 1 = AUD-01-01/02/03 (aprovados); rodada 1.5 pós-validação-V1 = AUD-01-07; rodada 2 (coordenar com Umbigo) = AUD-01-04/05/06 (pendentes de decisão).

---

## 2. Tabela Geral de Achados

| ID | Severidade | Arquivo / Linha | Categoria | Descrição Resumida |
|---|---|---|---|---|
| `AUD-01-01` | 🟠 Alto | `Client/Helpers/ServerConfigProvider.cs:14-36` | HTTP síncrono em main thread | Cache com TTL de 5 s refaz `RequestHandler.GetJson` bloqueante + re-desserialização completa por tick |
| `AUD-01-02` | 🟡 Médio | `Client/Components/BotDespawnManager.cs:39-52` + `Client/Plugin.cs:45` | Lifecycle | Poller `DontDestroyOnLoad` com `while(true)` — polling continua no menu/hideout/entre raids |
| `AUD-01-03` | 🟡 Médio | `Client/Helpers/ServerConfigProvider.cs:26` | Falha sem backoff | `_lastFetchTime` só avança no sucesso — com rota falhando, toda leitura vira HTTP (cenário headless) |
| `AUD-01-04` | 🟠 Alto | `Client/Patches/Patches.cs:820-852` (+ vanilla `BotProfileDataClass.cs:87`) | Crescimento sem limite | Pool `List_0` cresce 337→1155: patch só resolve PMC; Scav cai no match estrito de dificuldade e acumula perfis órfãos |
| `AUD-01-05` | 🟠 Alto | `Client/Components/DynamicSpawnManager.cs:168-175, :386` | Cancelamento global / NRE | `ClearSptQueue()` → `StopBotSpawn()` a cada 1 s no warmup cancela `BotCreationDataClass` em voo → NRE vanilla |
| `AUD-01-06` | 🟡 Médio | `Client/Components/DynamicSpawnManager.cs:339, 367-451` | Trabalho com raid vazia | `SpawnHordeLoop` continua gerando waves/`bot/generate` com 0 bots vivos; `Replace Despawned Bots` realimenta |
| `AUD-01-07` | 🟡 Médio | `Client/Patches/Patches.cs:805-814` + `Client/Patches/BotSpawnLoggerPatch.cs:26` | Logging sem gate | Maiores emissores (6 linhas Warning por `ChooseProfile`) escapam do gate `Enable Debug Logs` |
| `AUD-01-08` | 🟡 Médio | `Client/Patches/Patches.cs:374-378` (+ vanilla `LocalGame.cs:139-143, :187-194, :359-361`) | Trabalho barrado por tentativa | Ondas vanilla são interceptadas a **cada tentativa** (prefix em `ActivateBotsByWave`) em vez de desligadas **uma vez** na fonte (cenários de onda do `LocalGame`) — *registrado em 2026-08-22 23:16, pós-rodada 1* |

---

## 3. Detalhamento dos Achados

### AUD-01-01 · Poll HTTP síncrono de config na main thread (TTL 5 s)
- **Severidade:** 🟠 Alto
- **Evidência:** Forte (111 requisições medidas + metrônomo de 10,0 s na captura + código confirmado)
- **Execução:** propriedade `Config` tocada por: `BotDespawnManager.DespawnLoop` (`:54`, a cada 20-60 s), prefixes de spawn (`Patches.cs:436/564/640`, por tentativa de spawn), overlay do mapa em `LateUpdate` (`TRLMapBubbleOverlay.cs:209`, por frame com mapa aberto) — cadência agregada de miss ≈ 10 s, raid inteira.
- **Localização no Mod:** [ServerConfigProvider.cs:14-36](../Client/Helpers/ServerConfigProvider.cs#L14-L36)
- **Causa Raiz:** `if (_cachedConfig == null || Time.realtimeSinceStartup - _lastFetchTime > 5f)` (`:18`) → `RequestHandler.GetJson("/trldynamicspawn/getConfig")` (`:22`, **síncrono/bloqueante**, do SPT.Common) → `JsonConvert.DeserializeObject<TRLConfig>` (`:25`) realoca o TRLConfig inteiro (MapConfigs + EliteConfig + dicionários) a cada tick, para consumidores que só leem meia dúzia de escalares. O único motivo do re-fetch é o painel web aplicar edição ao vivo; o servidor (`Server/Routers/TRLRouters.cs:35-41`) serializa sempre o mesmo objeto em memória (`CurrentConfig`, só muda em `/saveConfig`).
- **Impacto Técnico Real:** cada miss = round-trip HTTP inteiro parado na main thread (servidor remoto → latência de rede vira frametime) + churn de GC. Componente request-phase do metrônomo de 10 s do baseline.
- **Proposta de Correção:**
  - *Atual:* TTL de 5 s na propriedade, fetch em qualquer call site.
  - *Otimizada:* cache com **escopo de raid** — popular 1× no início (o one-shot `FetchServerConfigAndStart` em [DynamicSpawnManager.cs:56-166](../Client/Components/DynamicSpawnManager.cs#L56) **já existe e é o modelo desejado**; o provider deve servir desse cache), invalidar no início/fim de raid. **Atenção: hook de fim de raid não existe no mod — criar (ex.: patch em `GameWorld.Dispose`).** `ForceRefresh()` (`:38-42`, hoje sem chamador) vira o caminho manual de atualização (hotkey F12 ou rota push — decidir na spec).
- **Como validar:** RequestHandler no log da raid de validação: `getConfig` = 1 (baseline: 111); metrônomo de 10 s ausente na captura CapFrameX.
- **Decisão:**
  - `[x]` Aceitar sugestão *(rodada 1 — aprovado via plano 2026-08-22)*

### AUD-01-02 · Poller com ciclo de vida global (roda fora de raid)
- **Severidade:** 🟡 Médio
- **Evidência:** Forte (código; `DontDestroyOnLoad` + `while(true)` sem `yield break`)
- **Execução:** contínua desde o boot do jogo, inclusive menu/hideout/entre raids; todos os early-exits do loop são `continue` que voltam ao topo — e o topo é o fetch (`:54`).
- **Localização no Mod:** [BotDespawnManager.cs:39-52](../Client/Components/BotDespawnManager.cs#L39-L52), [Plugin.cs:45](../Client/Plugin.cs#L45)
- **Causa Raiz:** `Enable()` roda em `Plugin.Start()` (não no ciclo de raid); GameObject `DontDestroyOnLoad`; `DespawnLoop` é `while(true)`.
- **Impacto Técnico Real:** polling HTTP e trabalho de scan fora de raid, para sempre.
- **Proposta de Correção:** loop só ativo em raid (`yield break` fora + re-arm no início de raid, ou gate por `GameWorld` válido antes de qualquer fetch).
- **Como validar:** RequestHandler silencioso no menu/hideout após a raid.
- **Decisão:**
  - `[x]` Aceitar sugestão *(rodada 1 — aprovado via plano 2026-08-22)*

### AUD-01-03 · Falha de fetch sem backoff (martelo HTTP no headless)
- **Severidade:** 🟡 Médio
- **Evidência:** Forte (código) + correlação com memória do projeto (headless OOM: config nunca chega lá)
- **Execução:** com a rota falhando, **cada** leitura da propriedade vira um HTTP síncrono (sem TTL), multiplicado pelos call sites por-frame/por-spawn.
- **Localização no Mod:** [ServerConfigProvider.cs:26](../Client/Helpers/ServerConfigProvider.cs#L26) (e `BotDespawnManager.cs:57-61`, loop de 5 s com `_serverConfig == null`)
- **Causa Raiz:** `_lastFetchTime` só é atualizado no ramo de sucesso.
- **Impacto Técnico Real:** no host local é latente; no **headless** (onde o server mod não responde) é martelo HTTP contínuo — provável contribuinte do OOM/perf do headless.
- **Proposta de Correção:** atualizar `_lastFetchTime` também em falha (backoff exponencial ou fixo ≥30 s) + fallback documentado para defaults.
- **Como validar:** simular rota ausente (server local sem o mod) e contar requisições/min no log (critério: ≤2/min).
- **Decisão:**
  - `[x]` Aceitar sugestão *(rodada 1 — aprovado via plano 2026-08-22)*

### AUD-01-04 · Pool de perfis cresce sem limite (337→1155; ~2 GB/min de churn)
- **Severidade:** 🟠 Alto
- **Evidência:** Forte (pool medido em jogo + mecânica confirmada no código vanilla e do mod)
- **Execução:** por wave × raid inteira; acúmulo permanente (nunca há `Clear()`; remoção só via `ChooseProfile(withDelete:true)`).
- **Localização no Mod:** [Patches.cs:820-852](../Client/Patches/Patches.cs#L820-L852) (`ChooseProfilePatch` só resolve PMC; `:852` retorna `true` → vanilla p/ Scav), [DynamicSpawnManager.cs:281-300](../Client/Components/DynamicSpawnManager.cs#L281-L300) (`GetRandomDifficulty` por wave), `:141-153/:490-492/:740-743` (`AddToTargetBackup`)
- **Referência Cruzada:** `references/eft-decompiled/Assembly-CSharp/BotProfileDataClass.cs:87` (match estrito Side+Role+Difficulty), `BotsPresets.cs:178-187` (miss → `LoadBots` lote de 3 → `List_0.AddRange`)
- **Causa Raiz:** dificuldade sorteada por wave gera combinações que o consumo estrito do vanilla nunca drena; cada miss pede +3 perfis na mesma dificuldade órfã.
- **Impacto Técnico Real:** 414 perfis gerados p/ 59 spawns; candidato principal aos ~2 GB/min de crescimento de RAM (10,8→33 GB) e à pressão de GC (stutters de 100-600 ms).
- **Proposta de Correção:** estender o `ChooseProfilePatch` para resolver também Scav/marksman ignorando dificuldade (como já faz p/ PMC), OU normalizar a dificuldade pedida ao `AddToTargetBackup` para a mesma usada no consumo; adicionar limpeza do pool no fim de raid.
- **Como validar:** logar `profilesInList` no início/fim (instrumentação abaixo); critério: pool estável (±50) durante a raid e curva de RAM sem crescimento monotônico.
- **Decisão:**
  - `[x]` Aceitar sugestão *(rodada 2 — aprovado pelo usuário em 2026-08-22 23:46; inclui revisar o tamanho da pré-carga `AddToTargetBackup` como meta medível)*

### AUD-01-05 · `ClearSptQueue()` cancela spawns em voo a cada 1 s (44 NREs)
- **Severidade:** 🟠 Alto
- **Evidência:** Forte para a cadeia da NRE (stack do errors.log + código); Suspeita para o leak associado (evento não desassinado)
- **Execução:** 1×/s durante todo o warmup (ESTÁGIO A do `SpawnHordeLoop`).
- **Localização no Mod:** [DynamicSpawnManager.cs:168-175](../Client/Components/DynamicSpawnManager.cs#L168-L175) e `:386`
- **Referência Cruzada:** `BotCreationDataClass.cs:102-105` (`SpawnStopped` → `Create` retorna null), `BotSpawner.cs:388-398` (repassa null sem checar), `GClass1890.cs:15` (`data.SpawnStopped` → NRE)
- **Causa Raiz:** `StopBotSpawn()` é global — cancela também os `BotCreationDataClass` do motor vanilla cujo `await` está pendente.
- **Impacto Técnico Real:** 44 NREs/raid (exceção + stack trace na main thread), spawns perdidos, retries e churn extra de `bot/generate`. Suspeita: instâncias canceladas ficam presas no delegate `OnStopBotSpawn` (subscribe no ctor `:116`, unsubscribe só em `StopSpawn` `:145`) — contribuinte de retenção.
- **Proposta de Correção:** remover a chamada periódica (ou torná-la one-shot no início do warmup); se o objetivo é limpar fila travada, filtrar apenas os pedidos do próprio mod.
- **Como validar:** errors.log da raid de validação: 0 NREs em `TrySpawnFreeInner` (baseline: 44).
- **Decisão:**
  - `[x]` Aceitar sugestão *(rodada 2 — aprovado pelo usuário em 2026-08-22 23:46)*

### AUD-01-06 · Waves continuam com a raid vazia; replace realimenta o ciclo
- **Severidade:** 🟡 Médio
- **Evidência:** Forte (`bot/generate` às 15:21:05-10 com todos os bots mortos + código)
- **Execução:** por ciclo de wave, raid inteira, mesmo sem jogadores-alvo de spawn.
- **Localização no Mod:** [DynamicSpawnManager.cs:339](../Client/Components/DynamicSpawnManager.cs#L339) (único `yield break` é `NoBots`), `:367-451` (`SpawnHordeLoop`), `:1195-1244` (`RequestReplacementBot`), `Settings.cs:28` (`Replace Despawned Bots`, `true` no deploy)
- **Causa Raiz:** `GetRealAliveBotsCount()==0 < cap` dispara nova wave sempre; cada despawn dispara um replacement (`bot/generate` extra).
- **Impacto Técnico Real:** geração/materialização contínua de perfis (os spikes grandes do baseline coincidem com esses lotes).
- **Proposta de Correção:** condicionar novas waves a estado de raid ativo + orçamento de spawn; rever se replace deve valer para despawn por distância.
- **Como validar:** RequestHandler: nenhuma `bot/generate` após o último bot morto (cenário "limpar o mapa e esperar 2 min").
- **Decisão:**
  - `[x]` Aceitar sugestão *(rodada 2 — aprovado pelo usuário em 2026-08-22 23:46)*

### AUD-01-07 · Logging Warning fora do gate de debug
- **Severidade:** 🟡 Médio
- **Evidência:** Forte (12,7k linhas na raid com gate `Enable Debug Logs` conferido no código)
- **Execução:** 6 linhas Warning por chamada de `ChooseProfile` (~2,1k chamadas/raid) + 1 por bot spawnado + blocos por wave.
- **Localização no Mod:** [Patches.cs:805-814](../Client/Patches/Patches.cs#L805-L814) (ChooseProfile + 5 "Available profile"), `:836/:848` (fallback/chosen), [BotSpawnLoggerPatch.cs:26](../Client/Patches/BotSpawnLoggerPatch.cs#L26) (postfix de `BotOwner.Create`), `DynamicSpawnManager.cs:731-736` e vários `[SPY]`
- **Causa Raiz:** emissores adicionados fora do `if (Settings.enableDebugLogs.Value)`; nível Warning passa qualquer filtro de console.
- **Impacto Técnico Real:** com console BepInEx ligado, cada bloco é escrita síncrona na main thread durante o pipeline de spawn — amplifica exatamente os frames que já são caros.
- **Proposta de Correção:** aplicar o gate existente a todos os emissores e rebaixar para `LogDebug`/`LogInfo`.
- **Como validar:** raid com `Enable Debug Logs = false` → 0 linhas `ChooseProfile`/`Available profile`/`SPAWN ->` no log.
- **Decisão:**
  - `[x]` Aceitar com modificação: **executar somente na rodada 1.5, após a validação V1** (os logs são a observabilidade do teste da rodada 1 — decisão do usuário no plano) — *2026-08-22 23:46: V1 parcial concluída (getConfig = 1); usuário aprovou entrar na rodada 2 junto com 04/05/06/08. Os emissores ficam **gated** por `Enable Debug Logs` (não removidos) para continuarem servindo à V2.*

### AUD-01-08 · Ondas vanilla barradas por tentativa em vez de desligadas na fonte
- **Severidade:** 🟡 Médio
- **Evidência:** Suspeita (mecanismo confirmado no código do mod e do vanilla; **frequência das tentativas e efeito colateral de parar os cenários cedo ainda não medidos**) — *achado registrado após a rodada 1 (2026-08-22 23:16), a partir da pergunta do usuário sobre os nomes de bots no console fora da hora da onda*
- **Execução:** por tentativa de onda vanilla, raid inteira. O jogo mantém seus próprios "roteiros" de spawn — `WavesSpawnScenario` (ondas cronometradas), `NonWavesSpawnScenario` (spawner contínuo) e `BossSpawnScenario` — criados em `LocalGame.cs:139-143` e postos a rodar em `:187-194`; cada onda disparada chama `BotsController.ActivateBotsByWave`, onde o prefix do mod a rejeita.
- **Localização no Mod:** [Patches.cs:374-378](../Client/Patches/Patches.cs#L374-L378) (`DisableVanillaWavesPatch` → `BotsController.ActivateBotsByWave(BotWaveDataClass)`), `:407-409` (`DisableVanillaBossWavesPatch`), logs `:431/:444/:513` ("Blocked Vanilla Horde Wave" / Rogue / Raider)
- **Referência Cruzada:** `references/eft-decompiled/Assembly-CSharp/EFT/LocalGame.cs:141` (`WavesSpawnScenario.smethod_0(..., wave => botsController_0.ActivateBotsByWave(wave), ...)`), `:139` (`NonWavesSpawnScenario`), `:359-361` (o próprio `LocalGame.Stop` para os três cenários com `.Stop()` — a API de desligar existe e é a canônica)
- **Causa Raiz:** o mod escolheu **interceptar o efeito** (cada ativação de onda) em vez de **parar a causa** (os cenários). O vanilla continua agendando, acordando e tentando; o mod paga o prefix + log a cada tentativa e o jogo paga o agendamento. Parte dos "nomes de bots fora da hora da onda" no console vem daqui (a outra parte é AUD-01-05/06/07).
- **Impacto Técnico Real:** trabalho zumbi do vanilla a raid inteira + linhas de log por tentativa; não é o maior ofensor do baseline (CPU por tentativa é baixo), mas é custo por construção que a rodada 2 pode zerar de graça.
- **Proposta de Correção:** no start hook do mod (`RaidLifecycle.OnRaidStart`, já existe desde o item 009), chamar **uma vez** `wavesSpawnScenario_0.Stop()` / `nonWavesSpawnScenario_0.Stop()` do `LocalGame` (campos privados — resolver por reflection cacheada ou pela API pública se houver), mantendo `bossSpawnScenario_0` conforme a configuração de chefes nativos (o mod **quer** os bosses nativos — ver `AdjustVanillaBossWaves`). O prefix atual vira rede de segurança (continua, mas sem tráfego). **Antes de decidir:** (1) medir quantas vezes/raid o prefix dispara hoje (contador `// PERF-INSTR AUD-01-08`); (2) confirmar no dump que `Stop()` cedo não derruba o `BotSpawner`/`BotsController` de que o mod depende (`Patches.cs:378` usa o mesmo `BotsController`); (3) conferir o equivalente no `CoopGame` do Fika (`CoopGame.cs`) — o headless roda o mod.
- **Como validar:** log da raid sem nenhuma linha `Blocked Vanilla Horde Wave` (baseline: por tentativa); spawns do mod inalterados; bosses nativos continuam nascendo conforme config.
- **Decisão:**
  - `[x]` Aceitar sugestão (revisada na atualização abaixo: prefix em `NonWavesSpawnScenario.Run`) *(rodada 2 — aprovado pelo usuário em 2026-08-22 23:46)*

> **Atualização 2026-08-22 23:35 — evidência promovida para Forte (medida na raid de validação V1, client v3.3.0).** O metrônomo de 10 s **persiste com `getConfig` = 1** — logo AUD-01-01 era contribuinte, não a causa. A causa é este achado, e é o próprio jogo: `NonWavesSpawnScenario.Update()` (`references/eft-decompiled/Assembly-CSharp/EFT/NonWavesSpawnScenario.cs:115-159`) roda a cada `float_2` segundos — `location.BotSpawnPeriodCheck` com **piso de 10 s** (`:32-34`, `:146-148`) — calcula `BotMax − bots vivos` (com a raid vazia = o cap inteiro, que o `SetMaxBotCountPatch` ainda eleva) e, para cada vaga que `TrySpawn` libera, chama `ActivateBotsWithoutWave` (`:153-158`) → `BotCreationDataClass.Create` → `ChooseProfile` sobre o pool (**473 perfis** nesta raid, 6 linhas `Warning` por escolha — AUD-01-07) → `BotSpawner.TryToSpawnInZoneAndDelay`, onde o prefix do mod (`Patches.cs:546-554`) barra. **Na raid medida:** 163 bloqueios de `assault` em rajadas de 3–8 a cada ~10 s + 228 `ChooseProfile` + ~1.600 linhas de perfil no console, **com 0 bots vivos e fora da onda do mod**; FPS 90→60 a cada rajada. O `LocalGame.Stop` desliga esse cenário com `nonWavesSpawnScenario_0.Stop()` (`LocalGame.cs:360`) — a API de desligar é a canônica.
> **Correção proposta (revisada):** em vez de reflection nos campos privados do `LocalGame`, **prefix em `NonWavesSpawnScenario.Run()`** (`:98`, público) retornando `false` quando o mod governa os spawns (host/solo, `!FikaHelper.IsClient()`): `bool_1` nunca arma, `Update()` sai na primeira linha (`:117`) — custo zero por frame, sem reflection, vale para `LocalGame` e `CoopGame`. Os bosses nativos (`BossSpawnScenario`) **não** são tocados; as ondas cronometradas (`WavesSpawnScenario`, 17 bloqueios/raid) ficam para a mesma rodada. Validação: zero `Blocked Vanilla Assault Scav Spawn` no log; metrônomo de 10 s ausente; spawns do mod inalterados.

---

## Panorama de execução

| Superfície | Frequência | Entidades | Gate | Quem para / quando |
|---|---|---|---|---|
| `ServerConfigProvider.Config` (HTTP) | miss a cada ≥5 s, agregada ~10 s | 1 | TTL 5 s | nunca (AUD-01-01/02) |
| `BotDespawnManager.DespawnLoop` | 20-60 s (config) | ×bots vivos | `EnableDespawn` por mapa | nunca — `DontDestroyOnLoad` |
| `TRLMapBubbleOverlay.UpdateOverlay` | por frame | 1 | mapa aberto | fechar o mapa |
| `SpawnHordeLoop` | ~1 s (warmup) / por wave | ×waves | `NoBots` | fim de raid não para (AUD-01-06) |
| `ChooseProfilePatch` (prefix) | por bot escolhido (~2,1k/raid) | ×perfis no pool | — | — |
| `BotSpawnLoggerPatch` (postfix `BotOwner.Create`) | por bot spawnado | ×bots | — (AUD-01-07) | — |

## Configuração

| Chave | Default atual | Proposto | Onde entra |
|---|---|---|---|
| `Enable Debug Logs` (BepInEx F12) | `false` (deploy está `true` — **manter `true` até a V1**) | `false` pós-1.5 | `Settings.cs:25` |
| `Replace Despawned Bots` | `true` | reavaliar na rodada 2 | `Settings.cs:28` |
| TTL do provider (hardcoded 5 s) | — | cache por raid + `ForceRefresh()` | `ServerConfigProvider.cs:18` |

## Instrumentação proposta

- **AUD-01-05 (Suspeita de retenção por evento):** contador temporário `// PERF-INSTR AUD-01-05` logando, a cada 60 s, o nº de inscritos em `BotEventHandler.OnStopBotSpawn` (reflection no delegate) e instâncias vivas de `BotCreationDataClass` — fecha o eixo "leak por evento" antes de decidir a rodada 2.

## Plano de validação (V1 — pós-rodada 1)

Protocolo: raid Customs, mesma rota do baseline 2026-08-22, CapFrameX + curva de RAM + logs. Rodada 1 é 100% client-side (nenhuma subida de server necessária).

- [ ] `getConfig` no RequestHandler durante a raid = 1 (baseline: 111)
- [ ] Metrônomo de 10 s ausente na captura (baseline: stutters >50 ms a cada 10,0 s cravados)
- [ ] RequestHandler silencioso no menu/hideout pós-raid
- [ ] Não-regressão: spawns/despawns/teleports funcionando como antes; painel web aplica config via `ForceRefresh` (caminho manual — toggle F12 `Server Config → Reload Server Config`)
- [ ] Log do mod mostra, 1× por raid: `Server config fetched (raid-scoped cache)` e `Raid end hook fired (BaseLocalGame.Stop)` — se a fonte logada for `GameWorld.OnDestroy`, o patch de `Stop` não dispara (PA-01-05: remover e anotar na spec)

> **Resultado parcial V1 (raid de 2026-08-22 ~23:20, client 3.3.0, log `LogOutput.log:279056-285190`):** ✅ `getConfig` = **1** (baseline 111) · ✅ `Server config fetched (raid-scoped cache)` 1× · ✅ `Raid end hook fired` 1× — **fonte `GameWorld.OnDestroy`**, ou seja, o patch em `BaseLocalGame<>.Stop` **não disparou** (PA-01-05: remover na próxima build e anotar na spec) · ❌ **metrônomo de 10 s persiste** (FPS 90→60 em rajadas com 0 bots) — causa identificada e medida em **AUD-01-08** (spawner contínuo do vanilla), não em AUD-01-01. `bot/generate` = 0 nesta raid (pool já com 473 perfis).
>
> **Status da rodada 1 (2026-08-22):** implementada no item [009-perf-config-cache-raid](../backlog/009-perf-config-cache-raid/) — client **v3.3.0** compilada e instalada em `BepInEx/plugins/TRL-DynamicSpawn.dll` (rollback: `TRL-DynamicSpawn.dll.bak-3.2.9`). AUD-01-01/02/03: **aplicados, aguardando validação V1** (medição in-game pendente — não fecham sem números).

## Plano de validação (V2 — pós-rodada 2, client v3.4.0)

Protocolo: mesma raid Customs do baseline/V1, CapFrameX + curva de RAM + `LogOutput.log` + `errors.log`. **Rodar com `Enable Debug Logs = true`** (os contadores abaixo dependem das linhas gated) e uma segunda raid curta com `false` para o AC-M5.

- [ ] **AUD-01-08:** `Blocked Vanilla Assault Scav Spawn` = 0 (V1: 163); `Refused vanilla continuous spawn` aparece no lugar (1 linha por vaga, só com debug); metrônomo de 10 s **ausente** com 0 bots vivos (V1: FPS 90→60)
- [ ] **AUD-01-04:** `profilesInList` no fim − no início ≤ 50 (baseline 337→1155; V1: 473 estável sem `bot/generate`); `bot/generate` (incl. `byBackup`) ≤ 2 × bots spawnados; curva de RAM sem crescimento monotônico
- [ ] **AUD-01-05:** `NullReferenceException` em `TrySpawnFreeInner` ≤ 1 (baseline 44); `Clearing pending/stuck` = 1 por raid; nenhum `Member safely skipped` por cancelamento
- [ ] **AUD-01-06:** após `Raid end hook fired` nenhuma linha de onda/`SQUAD`/`bot/generate`; morrer com bots por nascer → onda interrompida em ≤ 1 s (`SQUAD MEMBER SPAWNED` para)
- [ ] **AUD-01-07:** raid com `Enable Debug Logs = false` → 0 linhas `Logger`/`SPY`/`SPAWN ->`/`Available profile`/`Horde Breakdown` (V1: ~1.900); só as operacionais por onda (≤ 8/onda)
- [ ] **Hooks (PA-02-03):** fonte do `Raid end hook fired` = `CoopGame.Stop` (Fika) — `GameWorld.OnDestroy` como fonte significa que o hook cedo não disparou
- [ ] **Não-regressão:** NR-1..NR-8 da [01-spec do 010](../backlog/010-perf-spawn-pipeline-r2/010-perf-spawn-pipeline-r2-01-spec.md) (snipers vanilla seguem nascendo, bosses nativos, composição das ondas, F12 `Initial Profile Preload` visível em Avançado)

> **Status da rodada 2 (2026-08-23 00:41):** implementada no item [010-perf-spawn-pipeline-r2](../backlog/010-perf-spawn-pipeline-r2/) — client **v3.4.0** compilada e instalada (rollback: `TRL-DynamicSpawn.dll.bak-3.3.0`). AUD-01-04/05/06/07/08: **aplicados, aguardando validação V2**. Achado novo da code review (CR-01-01 do 010): `AddToTargetBackup` é **nível permanente** de cache reposto pelo SPT (`GClass684.cs:258-263`), não "pedir N perfis" — a pré-carga de Scav sempre foi no-op; semântica corrigida no código e na doc.

## 4. Plano de Ação

1. **Rodada 1 (aprovada):** AUD-01-01 + 02 + 03 via `/optimize-mod-performance TRL-DynamicSpawn --fase 2 --escopo Client`.
2. **Validação V1** (checklist acima) → **rodada 1.5:** AUD-01-07.
3. **Rodada 2 (pendente, com Umbigo):** decidir AUD-01-04/05/06 — os dois primeiros são os candidatos diretos ao crescimento de RAM (~2 GB/min) e às NREs do baseline — **+ AUD-01-08** (desligar os cenários de onda vanilla na fonte; Suspeita → medir o contador antes de decidir).
