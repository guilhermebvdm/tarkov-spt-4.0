# 010 — perf-spawn-pipeline-r2

**Mod:** TRL-DynamicSpawn
**Status:** Backlog
**Criado:** 2026-08-22T23:46:00-03:00

> **Perfil desta spec: não-regressão** (item de performance — `/optimize-mod-performance`, rodada 2). O contrato funcional é o **comportamento atual** do spawn; o que muda é quanto trabalho o pipeline de spawn faz por trás. Origem: [relatorio-auditoria-codigo-01.md](../../docs/relatorio-auditoria-codigo-01.md) — achados **AUD-01-04, 05, 06, 07 e 08** (aceitos pelo usuário em 2026-08-22 23:46). Pré-requisito entregue: item [009](../009-perf-config-cache-raid/) (v3.3.0), cuja validação V1 mostrou que o metrônomo de 10 s **persiste** e apontou AUD-01-08 como causa.

## Visão geral

Rodada 2 do client: (1) o spawner contínuo de Scavs do jogo original deixa de **criar perfil e ser barrado** a cada 10 s — passa a ser recusado antes de qualquer trabalho; (2) o estoque de perfis de bot para de crescer sem uso (escolha tolerante à dificuldade + pré-carga menor); (3) a fila de criação do SPT não é mais cancelada a cada segundo durante o aquecimento; (4) ondas e coroutines do mod param no fim da raid e pausam sem jogador humano vivo; (5) todo log de diagnóstico fica atrás do gate `Enable Debug Logs`. 100% client-side.

## Comportamento atual

- **AUD-01-08 — spawner vanilla barrado tarde.** `NonWavesSpawnScenario.Update()` (jogo) roda a cada ≥10 s, calcula `BotMax − vivos` e, para cada vaga, chama `BotsController.ActivateBotsWithoutWave` → `BotSpawner.ActivateBotsWithoutWave` → `BotCreationDataClass.Create` → `ChooseProfile` (varre o pool; o mod imprime 6 linhas) → `BotSpawner.TryToSpawnInZoneAndDelay`, onde o prefix do mod ([Patches.cs:546-554](../../Client/Patches/Patches.cs#L546-L554)) recusa `assault`/`cursedAssault`. Medido na V1: **163 recusas em rajadas de 3–8 a cada ~10 s com 0 bots vivos**, 228 `ChooseProfile`, FPS 90→60 por rajada. Snipers vanilla (`marksman`) passam por esse mesmo caminho e **são aceitos**.
- **AUD-01-04 — pool de perfis cresce sem uso.** `BotProfileDataClass.ChooseProfile` vanilla exige **Side + Role + Dificuldade exatos** (`BotProfileDataClass.cs:87`); o mod sorteia a dificuldade **por onda** ([DynamicSpawnManager.cs:732-733](../../Client/Components/DynamicSpawnManager.cs#L732-L733)) e o `ChooseProfilePatch` só relaxa o match para PMC ([Patches.cs:820-852](../../Client/Patches/Patches.cs#L820-L852)). Scav/marksman sem match exato → `BotsPresets.CreateProfile` fabrica **+3** perfis daquela combinação (`BotsPresets.cs:178-187`) que podem nunca ser consumidos. Pré-carga fixa: 30 USEC + 30 BEAR + 20 assault (+10 Rogue, +15 Goons) no início ([:147-158](../../Client/Components/DynamicSpawnManager.cs#L147-L158)) **e** +10/+10/+10 `normal` em **toda** onda ([:496-498](../../Client/Components/DynamicSpawnManager.cs#L496-L498)), além da pré-busca por vagas com a dificuldade da onda ([:746-749](../../Client/Components/DynamicSpawnManager.cs#L746-L749)). Baseline: 337→1155 perfis; V1: 473.
- **AUD-01-05 — cancelamento global a cada 1 s.** Durante o aquecimento, `ClearSptQueue()` → `BotEventHandler.StopBotSpawn()` ([:392](../../Client/Components/DynamicSpawnManager.cs#L392), dentro do `while`) cancela **todo** `BotCreationDataClass` em voo — do jogo **e do próprio mod** (`SpawnGroupBotsCoroutine` → "Member safely skipped"). `Create` devolve `null` → vanilla `TrySpawnFreeAndDelay(null)` → 44 NREs/raid. Nasceu no item 006 para destravar uma "fila presa".
- **AUD-01-06 — trabalho com a raid acabando/vazia.** `SpawnHordeLoop`/`ProcessWave`/`SpawnGroupBotsCoroutine` só morrem com o `GameWorld` (o componente vive nele); entre `Stop` e `OnDestroy` continuam. Não há checagem de jogador humano vivo. A parte "reposição realimenta o ciclo" do relatório é **código morto**: `AttemptToDespawnBotCoroutine` (único chamador de `RequestReplacementBot`) nunca é invocado ([BotDespawnManager.cs:327](../../Client/Components/BotDespawnManager.cs#L327)); `Despawned bot` = 0 na V1.
- **AUD-01-07 — logs fora do gate.** `ChooseProfile CALLED` + 5 `Available profile` + `CHOSEN`/`WARNING` ([Patches.cs:805-848](../../Client/Patches/Patches.cs#L805-L848)), `SPAWN ->` ([BotSpawnLoggerPatch.cs:26](../../Client/Patches/BotSpawnLoggerPatch.cs#L26)), `Horde Breakdown` (4 linhas Warning/onda, [:737-742](../../Client/Components/DynamicSpawnManager.cs#L737-L742)) e os `[SPY]` por membro de esquadrão ([:970/:1015](../../Client/Components/DynamicSpawnManager.cs#L970)) — todos em `LogWarning`/`LogInfo` **sem** `if (Settings.enableDebugLogs.Value)`; a string é formatada mesmo com o gate desligado. V1: ~1.900 linhas Warning/raid.
- **PA-01-05 (item 009):** o patch em `BaseLocalGame<>.Stop` **não disparou** na V1 (fonte logada: `GameWorld.OnDestroy`) — hook inerte.

## Comportamento desejado

- Nenhuma tentativa do spawner contínuo vanilla para `assault`/`cursedAssault` chega a criar perfil: é recusada no primeiro passo (`ActivateBotsWithoutWave`). Snipers vanilla (`marksman`) continuam passando como hoje.
- Pedido de perfil com dificuldade sem match no pool usa um perfil do **mesmo lado e papel** em outra dificuldade, em vez de fabricar 3 novos; pré-carga inicial configurável (default menor) e sem a pré-carga fixa por onda.
- A fila do SPT é limpa **uma vez** no início do aquecimento, não a cada segundo.
- No fim da raid (qualquer hook de fim) as coroutines de spawn do mod param; sem jogador humano vivo, a onda não é calculada.
- Com `Enable Debug Logs = false`, o mod não imprime nenhuma linha de diagnóstico por perfil/bot/onda; com `true`, imprime as mesmas de hoje (nível Info).

## Critérios de aceite

### Não-regressão (comportamento que deve permanecer idêntico)

- [ ] **NR-1 (AUD-01-08):** quantidade, composição (PMC/Scav/pScav), zonas, bolha/safe zone/LoS e cadência das ondas **do mod** não mudam — o mod já era o único spawner efetivo de `assault` (o vanilla era recusado em 100% das tentativas).
- [ ] **NR-2 (AUD-01-08):** snipers vanilla (`marksman`) continuam nascendo pelo spawner contínuo do jogo como hoje; a regra de sniper do mod (`SniperChance` por mapa, 1ª onda) inalterada.
- [ ] **NR-3 (AUD-01-08):** bosses nativos, guardas, Rogues/Raiders, cultistas e Zryachiy — nada muda (outro sistema: `BossSpawnScenario`, não tocado).
- [ ] **NR-4 (AUD-01-04):** quando o pool **tem** perfil na dificuldade pedida, a escolha é exatamente a de hoje (match exato primeiro); a tolerância PMC do patch (qualquer perfil USEC/BEAR por lado **ou** papel) permanece — exceto o caso declarado em AC-X5.
- [ ] **NR-5 (AUD-01-05):** aquecimento continua atingindo o cap com o mesmo intervalo (`DelayBeforeFirstWave`) e as mesmas tentativas; a limpeza única no início do aquecimento preserva a intenção do item 006.
- [ ] **NR-6 (AUD-01-06):** com ao menos um humano vivo (host, ou guest no Fika), ondas/cooldown/reposição por cap comportam-se como hoje.
- [ ] **NR-7 (AUD-01-07):** com `Enable Debug Logs = true`, todas as mensagens de diagnóstico de hoje continuam disponíveis (mesmo conteúdo; nível Info em vez de Warning). Avisos **reais** (`MASTER FALLBACK`, `FAILED`, erros) continuam sem gate.
- [ ] **NR-8:** difficulty com SAIN ativo continua `normal` (inalterado).
- [ ] **Fika/multiplayer:** host/solo idênticos ao atual; guest continua sem nenhum dos mecanismos (todos os pontos novos fazem `return` cedo em `FikaHelper.IsClient()`); headless comporta-se como host.
- [ ] **Estado entre raids:** raid1 → exit → raid2 (e morte/MIA): nenhuma coroutine de spawn sobrevive ao fim da raid; `IsGeneratingDynamicWave`/`IsWarmupActive` (estáticos) resetados no fim; o pool de perfis é por raid (objeto do `BotsController`) e não é tocado.

### Metas medíveis (V2 — mesma raid Customs do baseline/V1, CapFrameX + log)

- [ ] **AC-M1 (AUD-01-08):** `Blocked Vanilla Assault Scav Spawn` = **0** (V1: 163) e `ChooseProfile` para `assault` fora das ondas do mod = 0.
- [ ] **AC-M2 (AUD-01-08):** metrônomo de 10 s **ausente** com 0 bots vivos (V1: FPS 90→60 a cada ~10 s).
- [ ] **AC-M3 (AUD-01-05):** `NullReferenceException` em `TrySpawnFreeInner` ≤ **1** por raid (baseline 44; a limpeza única ainda pode cancelar um `marksman` vanilla em voo — PA-01-07) e nenhum `Member safely skipped` causado por cancelamento.
- [ ] **AC-M4 (AUD-01-04):** `profilesInList` no fim da raid − no início ≤ **50** (baseline 337→1155); `bot/generate` ≤ 2 × bots spawnados.
- [ ] **AC-M5 (AUD-01-07):** com `Enable Debug Logs = false`: 0 linhas `Logger`/`SPY`/`SPAWN ->`/`Available profile`/`Horde Breakdown` (V1: ~1.900).
- [ ] **AC-M6 (AUD-01-06):** após o hook de fim de raid, nenhuma linha de onda/`bot/generate` do mod no log.

### Exceções declaradas (mudança perceptível, com trade-off)

- [ ] **AC-X1 — dificuldade tolerante (AUD-01-04).** Se o pool não tem perfil na dificuldade pedida, o bot nasce com **outra dificuldade** do mesmo lado/papel (hoje: o jogo fabrica 3 novos na dificuldade certa, com custo de HTTP + memória). Só acontece no **miss**; a pré-busca por onda continua pedindo a dificuldade certa, então o caso é raro. Com SAIN ativo é irrelevante (SAIN governa a dificuldade).
- [ ] **AC-X2 — pré-carga inicial menor (AUD-01-04).** 30/30/20 → **15/15/15** via propriedade F12 `Initial Profile Preload` (Avançado, 0–30). Trade-off: primeira onda em mapa com cap alto pode esperar um `bot/generate` a mais. O valor certo é calibrado pela medição `profilesInList` da V2.
- [ ] **AC-X3 — onda pausa sem humano vivo (AUD-01-06).** Com todos os humanos mortos (solo: você), o mod interrompe a onda em andamento (o grupo que já estava nascendo termina — limite do Unity ao parar coroutines aninhadas) e deixa de calcular ondas até o fim da raid. Trade-off: nenhum — a raid está acabando.
- [ ] **AC-X4 — hook `BaseLocalGame.Stop` substituído (PA-01-05 do 009 / PA-01-03).** Comprovadamente inerte na V1; entra no lugar o override concreto `LocalGame.Stop` (+ `CoopGame.Stop` quando o Fika está presente), que fecha a janela entre o fim lógico da raid e a destruição do mundo. Sem efeito perceptível; a V2 confere a fonte no log.
- [ ] **AC-X5 — vaga PMC sem perfil USEC/BEAR no pool (AUD-01-04 / PA-01-04).** Hoje o patch pega **qualquer** perfil, até um Scav, e o faz nascer como "PMC". Depois: devolve ao jogo, que fabrica 3 perfis PMC corretos. Trade-off: um `bot/generate` a mais nesse caso raro, em troca de nunca mais nascer Scav disfarçado de PMC.
- [ ] **AC-X6 — `ChooseProfile` com Halloween (AUD-01-08 / PA-01-02).** O evento sazonal `BotHalloweenEvent` chama o spawner direto e contorna a recusa antecipada; cai no backstop atual (recusado depois de criar perfil). AC-M1 tolera essas ocorrências quando o evento está ativo.

## Corner cases

- [ ] **Onda do mod em andamento quando o spawner vanilla tenta:** o prefix de `ActivateBotsWithoutWave` respeita `IsGeneratingDynamicWave` — mas o mod **não** usa `ActivateBotsWithoutWave` (usa `Create` + `TryToSpawnInZoneAndDelay` direto), então o prefix pode recusar `assault` vanilla **sempre** no host; o check da flag fica como defesa.
- [ ] **Pool sem nenhum perfil do lado/papel:** o patch devolve ao vanilla (`return true`) → `null` → `LoadBots(3)` como hoje (único caso em que fabricar é necessário).
- [ ] **`Initial Profile Preload = 0`:** nenhuma pré-carga inicial; a primeira onda depende da pré-busca por vagas (`:746-749`). Permitido, documentado.
- [ ] **Limpeza única da fila cancela uma criação vanilla em voo (boss tardio):** hoje acontece 1×/s; passa a acontecer 1× por aquecimento, após `DelayBeforeFirstWave` (bosses nativos já nasceram). Se ainda gerar NRE na V2, a próxima decisão é remover a limpeza de vez.
- [ ] **Host Fika morto, guests vivos:** humanos vivos > 0 → ondas continuam (NR-6). Headless: o "player" headless é ignorado na contagem (`IsHeadlessPlayer`), conta só humanos reais.
- [ ] **Fim de raid durante `SpawnGroupBotsCoroutine`:** `StopAllCoroutines` no hook de fim; `IsGeneratingDynamicWave` forçado para `false` (o `finally` da coroutine não roda quando ela é parada).
- [ ] **`Enable Debug Logs` ligado no meio da raid:** gate lido a cada emissão → passa a logar imediatamente (como hoje para os emissores já gated).

## Fora de escopo

- [ ] `WavesSpawnScenario` (ondas cronometradas vanilla, 17 recusas/raid na V1) — continua sendo barrado no prefix de `ActivateBotsByWave`; desligar na fonte é `async` awaitado pelo `LocalGame` (risco) — registrar como dívida.
- [ ] Reescrever `AttemptToDespawnBotCoroutine`/reposição (código morto) — decisão de produto do Umbigo; apenas documentado.
- [ ] Leak vanilla de `OnStopBotSpawn` (`BotCreationDataClass` nunca desassina se não cancelado) — fora do mod.
- [ ] `OnGameStartPatches.cs` morto (CR-01-04 do item 009) — removido aqui **apenas** se não custar review extra; caso contrário, rodada 3.

## Referências

- [docs/relatorio-auditoria-codigo-01.md](../../docs/relatorio-auditoria-codigo-01.md) — AUD-01-04/05/06/07/08 + resultado parcial V1
- [009-perf-config-cache-raid](../009-perf-config-cache-raid/) — `RaidLifecycle` (hooks de raid reaproveitados aqui)
- [006-otimizacao-fila-spawn-warmup](../006-otimizacao-fila-spawn-warmup/) — origem do `ClearSptQueue`

## Histórico

| Data | Evento |
|---|---|
| 2026-08-22 | Item criado via `/optimize-mod-performance --fase 2` (perfil não-regressão; agrupa AUD-01-04/05/06/07/08 + PA-01-05 do 009) |
| 2026-08-23 | Review técnica 01: NR-4 ajustado, AC-M3 com tolerância ≤1, AC-X3 interrompe onda em voo, AC-X4 reescrito (hook concreto), AC-X5 e AC-X6 novos |
