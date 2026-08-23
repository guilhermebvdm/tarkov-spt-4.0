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
  - `[ ]` Pendente *(rodada 2 — coordenar com Umbigo)*

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
  - `[ ]` Pendente *(rodada 2 — coordenar com Umbigo)*

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
  - `[ ]` Pendente *(rodada 2 — coordenar com Umbigo)*

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
  - `[x]` Aceitar com modificação: **executar somente na rodada 1.5, após a validação V1** (os logs são a observabilidade do teste da rodada 1 — decisão do usuário no plano)

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
- [ ] Não-regressão: spawns/despawns/teleports funcionando como antes; painel web aplica config via `ForceRefresh` (caminho manual)

## 4. Plano de Ação

1. **Rodada 1 (aprovada):** AUD-01-01 + 02 + 03 via `/optimize-mod-performance TRL-DynamicSpawn --fase 2 --escopo Client`.
2. **Validação V1** (checklist acima) → **rodada 1.5:** AUD-01-07.
3. **Rodada 2 (pendente, com Umbigo):** decidir AUD-01-04/05/06 — os dois primeiros são os candidatos diretos ao crescimento de RAM (~2 GB/min) e às NREs do baseline.
