# 009 — perf-config-cache-raid

**Mod:** TRL-DynamicSpawn
**Status:** Backlog
**Criado:** 2026-08-22T22:26:51-03:00

> **Perfil desta spec: não-regressão** (item de performance — `/optimize-mod-performance`, rodada 1). O contrato funcional é o **comportamento atual**: o que o jogador vê/sente não muda. O que muda é **quantas vezes, por quanto tempo e em que contexto** o mod faz trabalho. Origem: [relatorio-auditoria-codigo-01.md](../../docs/relatorio-auditoria-codigo-01.md) — achados **AUD-01-01**, **AUD-01-02** e **AUD-01-03** (aceitos em 2026-08-22). AUD-01-04/05/06 (rodada 2) e AUD-01-07 (rodada 1.5) **não** entram aqui.

## Visão geral

Rodada 1 de performance do client: a configuração do servidor (`/trldynamicspawn/getConfig`) passa a ser buscada **uma vez por raid** em vez de a cada ~5–10 s na main thread; o poller de despawn/teleporte só roda **durante a raid**; uma falha de fetch passa a respeitar um **intervalo mínimo de nova tentativa** (hoje cada leitura vira um novo HTTP bloqueante). Mudança 100% client-side — nenhuma subida de server.

## Comportamento atual

- **`ServerConfigProvider.Config`** ([ServerConfigProvider.cs:14-36](../../Client/Helpers/ServerConfigProvider.cs#L14-L36)): propriedade estática com cache de **5 s**. Expirado o TTL, qualquer leitura faz `RequestHandler.GetJson` **síncrono** (a main thread para esperando a resposta HTTP) e re-desserializa o `TRLConfig` inteiro. Consumidores: `BotDespawnManager.DespawnLoop` (`:54`, a cada 20–60 s), prefixes de spawn (`Patches.cs:436/564/640`, por tentativa de spawn), overlay do mapa em `LateUpdate` (`TRLMapBubbleOverlay.cs:209`, por frame com o mapa aberto) e `DynamicSpawnManager.ServerConfig` (`:33`). Medido: **111 requisições por raid**, em fase com o stutter metronômico de 10 s do baseline.
- **`ForceRefresh()`** (`:38-42`) existe e **não tem chamador**.
- **`BotDespawnManager`** ([BotDespawnManager.cs:36-52](../../Client/Components/BotDespawnManager.cs#L36-L52)): GameObject `DontDestroyOnLoad` criado em `Plugin.Start()`; `DespawnLoop` é `while (true)` cujo **primeiro passo é ler `Config`** — o polling HTTP continua no menu, hideout e entre raids, para sempre. Os early-exits são `continue` (voltam ao topo = ao fetch).
- **Falha de fetch** (`ServerConfigProvider.cs:26`): `_lastFetchTime` só avança no sucesso. Com a rota indisponível, **cada leitura** da propriedade dispara um novo HTTP — multiplicado pelos call sites por-frame/por-spawn. No Fika headless (onde o server mod pode não responder), é um martelo HTTP contínuo.
- **O único motivo do re-fetch periódico** é o painel web aplicar edições ao vivo. O servidor ([TRLRouters.cs:35-41](../../Server/Routers/TRLRouters.cs#L35-L41)) serializa sempre o mesmo objeto em memória (`CurrentConfig`), que só muda em `/saveConfig`.

## Comportamento desejado

- Durante uma raid, a config do servidor é obtida **uma vez** (na primeira leitura) e reutilizada até o fim da raid. Cada raid nova busca de novo (edições feitas no painel web entre raids continuam valendo na raid seguinte).
- Edição do painel web **durante** a raid deixa de ser aplicada automaticamente; passa a existir um **caminho manual** de atualização (ver AC-M1, exceção declarada).
- O poller de despawn/teleporte não faz trabalho nenhum (nem HTTP) fora de raid; dentro da raid se comporta exatamente como hoje.
- Falha de fetch: próxima tentativa só depois de um intervalo mínimo (≥30 s); enquanto não há config, os consumidores seguem no mesmo caminho de `null` que já tratam hoje.

## Critérios de aceite

### Não-regressão (comportamento que deve permanecer idêntico)

- [ ] **NR-1 (AUD-01-01):** spawns dinâmicos, bloqueio de waves vanilla, filtros de Rogue em Lighthouse, bolha/safe zone e LoS culling decidem com **os mesmos valores de config** que hoje — lidos da mesma rota `getConfig`, sem alteração de schema (`TRLConfig.cs` intocado).
- [ ] **NR-2 (AUD-01-01):** overlay do mapa (`TRLMapBubbleOverlay`) desenha os mesmos círculos (safe zone / bolha / cone LoS) com os mesmos raios.
- [ ] **NR-3 (AUD-01-02):** despawn por distância, teleporte de grupo, cooldown de 30 s por bot, alternância de BotZone e `Replace Despawned Bots` funcionam em raid como antes (mesmo intervalo `DespawnInterval` por mapa, mesmo mínimo de 5 s, mesmo gate `Enable Despawn System` / `Enable Spawn Bubble` / `EnableDespawn` por mapa).
- [ ] **NR-4 (AUD-01-02):** `_teleportCooldowns` continua limpo no início de cada raid e ao trocar de mapa (hoje: `Start()` uma vez por processo + troca de `_currentLocation`). Nova raid nunca herda cooldown da anterior.
- [ ] **NR-5 (AUD-01-03):** com config indisponível, o mod degrada como hoje — `Config == null` → consumidores usam seus defaults/`return true`/pulam a lógica, sem exceção nova e sem mudar a decisão de spawn.
- [ ] **NR-6:** preset ativo (`Balanced`/`Random`/…) e modificadores de preset aplicados em `FetchServerConfigAndStart` seguem valendo na raid (a cópia `_serverConfig` do `DynamicSpawnManager` não é tocada por este item).
- [ ] **Fika/multiplayer:** host/solo — comportamento idêntico ao atual; guest — continua sem `DynamicSpawnManager` (já é assim) e o poller de despawn continua inerte (`IsHostOrSolo()` falso) — só que agora sem HTTP de fundo. Headless: o mod roda nele; a mudança só **reduz** requisições (AC-M3).
- [ ] **Estado entre raids:** raid1 → exit → raid2 (e alt-F4/morte/MIA): raid2 busca config nova (não herda cache), poller reinicia limpo, nenhuma coroutine duplicada (um `DespawnLoop` por raid, nunca dois).

### Metas medíveis (do Plano de validação V1 do relatório)

- [ ] **AC-M1 (AUD-01-01):** `getConfig` no RequestHandler durante uma raid Customs completa = **1** (baseline 2026-08-22: 111). Toleráveis: +1 por uso manual de refresh e +1 se `FetchServerConfigAndStart` mantiver o fetch próprio (ver spec técnica — alvo final ≤ 2).
- [ ] **AC-M2 (AUD-01-01):** metrônomo de stutter de **10,0 s** ausente na captura CapFrameX (mesmo mapa/rota do baseline).
- [ ] **AC-M3 (AUD-01-02):** RequestHandler **silencioso** no menu/hideout antes e depois da raid (zero `getConfig` fora de raid; baseline: ~1 a cada 10 s para sempre).
- [ ] **AC-M4 (AUD-01-03):** com a rota indisponível (server sem o mod, ou host desligado), requisições `getConfig` ≤ **2/min** (baseline: uma por leitura da propriedade, até centenas/min com o mapa aberto).

### Exceção declarada (mudança perceptível, com trade-off)

- [ ] **AC-X1 — Edição ao vivo do painel web.** Hoje, mudar um valor no painel web é aplicado em ≤5 s dentro da raid. Depois: aplicado **na próxima raid**, ou **sob demanda** via o novo caminho manual (`ForceRefresh()` exposto no F12 do BepInEx — ver spec técnica §3). Trade-off aceito: o custo dessa conveniência era 111 HTTP bloqueantes por raid para um valor que raramente muda durante a raid. O caminho manual tem de estar documentado em `PROPRIEDADES.md` e no README do painel.

## Corner cases

- [ ] **Primeira leitura antes de `OnGameStarted`:** patches de wave vanilla (`Patches.cs:436`) podem ler `Config` durante o setup do `BotsController`, antes do hook de início de raid. O cache tem de ser **fetch-on-miss** (busca na primeira leitura, sem depender do hook de início) — o hook só invalida.
- [ ] **Fim de raid por caminhos diferentes** (extract / morte / MIA / volta ao menu): dois hooks de fim (`GameWorld.OnDestroy` **e** `BaseLocalGame<EftGamePlayerOwner>.Stop`), ambos idempotentes — qualquer um que dispare invalida o cache e para o poller; o segundo é no-op. Alt-F4/crash encerram o **processo** (estado estático some com ele) — não há cache a invalidar.
- [ ] **Refresh manual com a rota falhando:** `ForceRefresh()` zera o cache; a próxima leitura tenta 1× e, falhando, entra no backoff normal (não vira martelo).
- [ ] **Hideout:** `Singleton<GameWorld>.Instantiated` é verdadeiro no hideout (`HideoutPlayer`). O poller deve tratar hideout como "fora de raid" (hoje ele roda lá — com `botsController == null` cai num `continue`, mas **depois** de ter feito o fetch).
- [ ] **Troca de mapa na mesma sessão** (raid1 Customs → raid2 Woods): `_currentLocation` muda → `_teleportCooldowns.Clear()` como hoje; config de raid2 reflete o mapa novo (cache invalidado entre as raids).
- [ ] **Dois `Enable()`/dois starts do poller** (patch disparando 2× — Fika re-entra em `OnGameStarted` em reconexão): start idempotente — se já há coroutine viva para esta raid, não cria outra.
- [ ] **`Enable Despawn System = false` em raid:** loop continua dormindo em 5 s como hoje, sem HTTP (o gate de config deixa de ser o primeiro passo).

## Fora de escopo

- [ ] AUD-01-04 (pool de perfis), AUD-01-05 (`ClearSptQueue`/NREs), AUD-01-06 (waves com raid vazia) — rodada 2, coordenar com Umbigo.
- [ ] AUD-01-07 (logging fora do gate) — rodada 1.5, **depois** da validação V1. **Os logs de debug ficam exatamente como estão** nesta rodada (são a observabilidade da V1).
- [ ] Tornar o fetch assíncrono (`GetJsonAsync`/coroutine): reduzir a **frequência** a 1×/raid elimina o problema; trocar o mecanismo de fetch mexe em todos os consumidores síncronos sem ganho proporcional.
- [ ] Qualquer mudança em `Server/` (rota push, websocket, etc.).
- [ ] Mexer em `FetchServerConfigAndStart` além de, opcionalmente, fazê-lo servir do mesmo cache (decisão na spec técnica).

## Referências

- [docs/relatorio-auditoria-codigo-01.md](../../docs/relatorio-auditoria-codigo-01.md) — AUD-01-01/02/03, Panorama de execução, Plano de validação V1
- Plano aprovado 2026-08-22 (`~/.claude/plans/vamos-la-precisamos-criar-hidden-lampson.md`, §2.1 e §FASE 3 V1)
- Skill `spt-performance-analysis` §4 (ciclo de vida de execução) e §7 (validação medida)

## Histórico

| Data | Evento |
|---|---|
| 2026-08-22 | Item criado via `/optimize-mod-performance --fase 2` (perfil não-regressão; agrupa AUD-01-01/02/03) |
