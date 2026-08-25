# Memory — TRL-DynamicSpawn

Memória cronológica de sessões de trabalho (timestamps em GMT-3). Cada entrada resume as alterações efetuadas, decisões de arquitetura e estado atual. Atualizada ao fim de cada sessão de trabalho.

> **Por que existe:** o usuário trabalha múltiplos chats/sessões em paralelo. Este arquivo preserva o contexto técnico, decisões de design e roadmap do mod `TRL-DynamicSpawn`, evitando que futuras sessões com assistentes AI reabram discussões do zero.

---

## Estado Atual (Snapshot ao Fim da Sessão — 2026-08-24)

**Client em v3.4.0 (deployada em `D:\SPT\BepInEx\plugins\TRL-DynamicSpawn.dll`, rollbacks `.bak-3.3.0`/`.bak-3.2.9`); Server segue v3.2.9 (nada de server nas rodadas 1–2). Tudo mergeado na `main` (PR #7, merge `e83d05e1`).**

- **Identity**: `TRL-DynamicSpawn` (Client BepInEx `TRL-DynamicSpawn.dll`, Server C# `TRL-DynamicSpawn-Server.dll` com Web UI). SPT 4.0.13 / EFT 0.16.9 / Fika.
- **Rodadas 1–2 de performance entregues e validadas (V2 parcial)** — processo `/optimize-mod-performance`, itens de backlog [009](../backlog/009-perf-config-cache-raid/) e [010](../backlog/010-perf-spawn-pipeline-r2/), relatório canônico [relatorio-auditoria-codigo-01.md](../docs/relatorio-auditoria-codigo-01.md):
  - Config do painel buscada **1×/raid** (era 111 HTTP bloqueantes; toggle F12 `Reload Server Config` para aplicar ao vivo).
  - Spawner contínuo vanilla de Scavs **recusado antes de criar perfil** (era a causa real do stutter de 10 s; V2: 0 barradas caras, FPS estável confirmado pelo usuário); snipers vanilla preservados.
  - `ChooseProfile` tolerante à dificuldade (fim dos perfis órfãos +3/miss); `ClearSptQueue` **1×/raid** (0 NREs `TrySpawnFreeInner`, eram 44); coroutines param no fim da raid e sem humano vivo; logs de diagnóstico sob gate `Enable Debug Logs`.
  - Hooks de raid: start `GameWorld.OnGameStarted`; fim `GameWorld.OnDestroy` (primário) + `LocalGame.Stop`/`CoopGame.Stop` soft (com Fika, `LocalGame` nunca existe e `CoopGame.Stop` **não chama a base** — por isso o hook genérico era inerte).
  - F12 novos: `Reload Server Config` (Server Config) e `Initial Profile Preload` (Profile Pool (Advanced), nível **permanente** de cache PMC, 5–30, default 15) — ver `PROPRIEDADES.md`.
- **Fato-chave do estoque de perfis:** `AddToTargetBackup` NÃO "pede N perfis" — registra um **nível permanente por (papel, dificuldade)** que o SPT repõe a cada ~30 s a raid inteira (`GClass684.cs:258-263`; vanilla já registra assault/marksman 8/dificuldade). Pool medido na V2: 452→504 (estável; os ~452 iniciais são pré-geração vanilla das waves do mapa — fora do mod).
- **Fato-chave do SAIN:** o SAIN **respeita** a etiqueta de dificuldade (seções easy/normal/hard/impossible por bot; 48 parâmetros diferem no BEAR) — a premissa do item 004 ("SAIN ignora dificuldade → pedir tudo normal") é **falsa**; hoje os pesos do painel não valem com SAIN.
- **Equipamento × dificuldade:** ProgressiveBotSystem 2.2.1 (vendorizado em `mods/progressivebotsystem-csharp/`, grafo gerado) seleciona equipamento por **Tier (nível do jogador) + papel**; dificuldade só aparece em logging → reescrever a etiqueta na escolha é seguro neste setup.
- **Item [011-perf-estoque-dificuldade](../backlog/011-perf-estoque-dificuldade/)** especificado (⚪): teto de estoque por mapa + etiqueta reescrita na escolha + painel valendo com SAIN (reverte 004). Aguarda decisão/execução — **handoff para o Umbigo**.

---

## Pendências / Próximos Passos Conhecidos (Roadmap)

> **Handoff (2026-08-24): os próximos itens são do Umbigo.** Porta de entrada: [relatorio-auditoria-codigo-01.md](../docs/relatorio-auditoria-codigo-01.md) (Decisões por achado + planos V1/V2 com números) e o fluxo `/optimize-mod-performance` no WORKFLOW.md.

- 🟡 [P-6.1] **Resíduo da validação V2** (aberta 2026-08-24): medir curva de RAM numa raid longa; 1 raid host com `Enable Debug Logs = false` (AC-M5); conferir a fonte do hook de fim numa raid encerrada por **extração normal** (nas raids V2 o jogo foi fechado no alt+F4 → só `GameWorld.OnDestroy` rodou; esperado com Fika: `CoopGame.Stop`).
- 🟡 [P-6.2] **Rodada 3 (decisões de design — Umbigo)** (aberta 2026-08-24): [AUD-01-09](../docs/relatorio-auditoria-codigo-01.md) warmup que não converge (ondas de 1–3 vagas a cada 30 s para sempre; propor tolerância de teto); pool inicial vanilla ~452 perfis pré-gerados para waves que o mod bloqueia; item 011 (estoque/dificuldade/SAIN).
- 🟢 [P-6.3] **Log flood de terceiros observado na V2** (aberta 2026-08-24): ORBIT `value-skip` 1 linha/frame (1.134 em ~3k linhas) com bot preso em cadáver; NREs `EFT.Player.get_PointOfView` ao espectar no Fika. Nenhum é deste mod — reportar aos donos/rodada de perf deles.
- 🟡 [P-ROADMAP-01] **Retorno do Viés Direcional (Pós-Debug)**: Retornar a proporção do viés direcional de spawn/teleport para 70% frontal / 30% traseiro pós-testes.
- 🟡 [P-ROADMAP-04] **Standalone Mod — Limpador de Corpos Inteligente (Corpse Cleaner)**: Novo mod separado focado em performance (timer individual por corpo).

---

## Histórico de Sessões

## 2026-08-24 22:53 (GMT-3) — Sessão 6: Rodadas 1–2 de performance (v3.3.0/v3.4.0), validação V2 e handoff da rodada 3

> Sessão contínua de 22→24/08 (worktree `perf-dynspawn-config-cache`, mergeado via PR #7). Registrada como uma entrada única no fechamento.

**Tema central:** eliminar o custo de fundo do mod (stutter de 10 s, churn de perfis, 44 NREs) via processo `/optimize-mod-performance` com specs de não-regressão e reviews independentes; fechar com medição in-game.

**Decisões-chave:**
- **Rodada 1 (item 009, v3.3.0):** config do painel com cache por raid + backoff 30 s + poller de despawn só em raid — porque 111 HTTP síncronos/raid paravam a main thread. Ref: `Client/Helpers/ServerConfigProvider.cs`, `RaidLifecycle.cs`, relatório AUD-01-01/02/03.
- **Rodada 2 (item 010, v3.4.0):** recusar o spawner contínuo vanilla em `BotsController.ActivateBotsWithoutWave` (antes de criar perfil), `ChooseProfile` tolerante para todos os papéis, `ClearSptQueue` 1×/raid, `StopSpawnLoops` nos hooks de fim, logs sob gate — porque a V1 provou que o metrônomo de 10 s era o `NonWavesSpawnScenario` (163 tentativas barradas tarde), não o getConfig. Ref: `Client/Patches/SpawnGatePatches.cs`, AUD-01-04..08.
- **Stop hook concreto:** patch em `LocalGame.Stop` + `CoopGame.Stop` (soft por nome) no lugar do genérico `BaseLocalGame<>` — porque com Fika o `LocalGame` nunca é instanciado e `CoopGame.Stop` não chama a base. Ref: PA-01-03/PA-02-03 do 010.
- **Semântica do estoque corrigida (CR-01-01 do 010):** `AddToTargetBackup` = nível permanente reposto pelo SPT; F12 `Initial Profile Preload` documentado com a semântica real; pré-carga de Scav removida (era no-op — vanilla registra 8/dificuldade).
- **Decisão do usuário:** logs de debug ficaram ligados até a V1; mudança de edição ao vivo do painel (5 s → por raid + toggle F12) aceita como trade-off declarado (AC-X1..X6 nas specs).

**Lições / hipóteses descartadas:**
- **"O metrônomo de 10 s é o poll de getConfig" — parcialmente falsa:** V1 mediu getConfig 111→1 e o stutter persistiu; a causa era o `NonWavesSpawnScenario.Update` (piso de 10 s, `NonWavesSpawnScenario.cs:32-34`) criando perfis que o mod barrava tarde. Lição: correlação de cadência não é causa — medir por mecanismo.
- **"AddToTargetBackup pede N perfis" — falsa:** registra nível permanente, chave só se ausente (`GClass684.cs:258-263`); a "pré-carga" de Scav (20) nunca teve efeito. Descoberto por code review independente contra o dump.
- **"SAIN ignora a dificuldade" (premissa do item 004) — falsa:** SAIN aplica seções easy/normal/hard/impossible por bot (48 parâmetros diferem no BEAR, conferido em `D:\SPT\BepInEx\plugins\SAIN\Default Bot Config Values\`). Hoje o mod anula os pesos do painel com SAIN ativo — corrigir no item 011.
- **"Patch em `BaseLocalGame<EftGamePlayerOwner>.Stop` cobre o Fika" — falsa:** inerte na V1; `CoopGame.Stop` não chama a base (`CoopGame.cs:811-818`) e `LocalGame` nem existe com Fika (`TarkovApplication_LocalGameCreator_Patch.cs:192`).
- **"Dificuldade influencia equipamento" — falsa neste setup:** ProgressiveBotSystem 2.2.1 usa Tier (nível do jogador) + papel; "difficulty" só em logging (3 ocorrências).
- **"Onda 2 infinita" (relato do usuário na V2) — não é regressão:** é o ESTÁGIO A pré-existente que só entra no cooldown longo com o mapa 100% cheio; com mortes contínuas fica em ondas de 1–3 vagas a cada 30 s (Attempt 43 medido) → AUD-01-09, rodada 3.

**Atividade cronológica (resumo):**
1. Fase 2–4 do item 009: spec não-regressão + review (5 PA) + código + review de código (4 CR) + build 3.3.0 + deploy — V1: getConfig 111→1 ✓, stutter persistiu ✗ → AUD-01-08.
2. Decisão do usuário: atacar todos os pendentes → item 010 (AUD-01-04..08 + PA-01-05), reviews independentes por agente (12 PA em 2 rodadas + 5 CR), build 3.4.0 + deploy.
3. Push + PR #7 + merge na `main` (`e83d05e1`); worktree mantido até fechar a V2.
4. V2 (2 raids do usuário, 24/08): raid A host (0 barradas caras, 1 getConfig, 1 limpeza de fila, 0 NRE TrySpawnFreeInner, pool 452→504, 0 linhas pós-fim, FPS estável ✓); raid B guest (mod inerte ✓). Achados novos: AUD-01-09 (warmup), pool inicial vanilla ~452, floods de terceiros (ORBIT/Fika).
5. ProgressiveBotSystem 2.2.1 vendorizado + auditoria equipamento×dificuldade + grafo; item 011 especificado.

**Pendências abertas nesta sessão:** [P-6.1] resíduo V2 🟡 · [P-6.2] rodada 3 (AUD-01-09 + pool vanilla + 011, com Umbigo) 🟡 · [P-6.3] floods de terceiros 🟢 — detalhes no topo.

**Cross-refs:**
- Artefatos canônicos: [relatorio-auditoria-codigo-01.md](../docs/relatorio-auditoria-codigo-01.md) (achados, decisões, V1/V2 com números) · backlog [009](../backlog/009-perf-config-cache-raid/), [010](../backlog/010-perf-spawn-pipeline-r2/), [011](../backlog/011-perf-estoque-dificuldade/) · `PROPRIEDADES.md` (F12 novos).
- Item 004 (SAIN): premissa refutada nesta sessão — a reversão está desenhada no 011, não aplicada.
- Build sem `/compile-mod`: o script exige `modded/`; este mod compila com `dotnet build` direto (refs temporárias fora do repo — ver asbuilds dos itens 009/010). Dívida de harness.

### 2026-08-16 — Correção de Vazamento de Rogues/Raiders (v3.2.9)

- **Correção de Vazamento de Rogues e Raiders a 0% (`v3.2.9`)**:
  - Trava estrita adicionada em `DynamicSpawnManager.cs` exigindo `GetBossChanceForMap > 0`, `Enable == true` e `!DisableBosses` para grupos aleatórios.
- **Validação de Build**:
  - `TRL-DynamicSpawn-Client.csproj` e `TRL-DynamicSpawn-Server.csproj` compilados com **0 Erros**.





### 2026-08-06 — Invasão Dinâmica de Elites/Rogues (v3.2.3), MaxBot Dinâmico (v3.2.0) e Code Review 008

- **Correção da Causa Raiz do Spawning de Rogues/Elites Não-Nativos (`DynamicSpawnManager.cs`)**:
  - `SpawnHordeLoop` invocava `ProcessWave(false)` de forma hardcoded. Ajustado para `ProcessWave(warmupAttempt == 1)`, ativando `isFirstWave = true` no 1º ciclo de Warmup da raid.
- **Integridade e Spawn Conjunto de Esquadrões de Rogues**:
  - Eliminado o fracionamento de grupos em instâncias de 1 bot. O grupo é enfileirado como uma única unidade (`GroupSize = MaxGroupSize`).
  - Removido `exUsec` da sub-lista de PMCs comuns no algoritmo de interleaving, alocando Rogues no topo da lista (`elites`) para nascerem juntos no segundo 0 da onda na mesma zona.
- **Validações e QA (Code Review 008)**:
  - Confirmado que a bolha de distância (`enableSpawnBubble`) **não afeta Rogues/Elites** (já isentos em `IsValidSpawnZone`).
  - Aplicada comparação insensível a caixa (`StringComparison.OrdinalIgnoreCase`) em `GetZoneFromConfig`.
  - Adicionado pré-carregamento síncrono de Rogues (`exUsec`) no `AddToTargetBackup` do SPT.
- **Validação de Build & SemVer (`v3.2.3`)**:
  - BepInPlugin e Server csproj atualizados para `3.2.3`. Compilados com **0 Erros**.

### 2026-08-05 — Suporte a Copiar Mapa, Referência Imutável config.default.json, Modal do Default e Remoção do BotMountPatch

- **Remoção do `BotMountWeaponFixPatch` de TRL-DynamicSpawn**:
  - Migrado e centralizado no mod `TRL-Fixes` (`BotMountWeaponFixPatch.cs`). Removidas as referências em `TRL-DynamicSpawn/Client/Patches/Patches.cs`.
- **Referência Canônica para Restauração de Padrões (`config.default.json`)**:
  - Criado o arquivo `Server/config/config.default.json` com as 894 linhas completas das configurações originais do autor.
  - Atualizado o método `TRLConfigManager.ResetConfig()` para ler de `config.default.json` ao processar a rota `/trldynamicspawn/resetConfig`.
- **Melhorias e UX do Painel Web (`Index.razor`)**:
  - **Copiar Configuração de Mapa**: Adicionado dropdown `-- Copiar de outro mapa --` e botão `[📋 Copiar]` no cabeçalho de cada mapa. Clona profundamente as configurações do mapa selecionado para o mapa ativo e limpa a seleção.
  - **Modal de Confirmação no Botão PADRÃO**: Adicionado modal responsivo com `z-index: 99999` para evitar cliques acidentais ao restaurar o padrão.
  - **Remoção de Botões Obsoletos**: Removidos os botões `DESFAZER` (`Undo`) e `RECARREGAR` (`Reload`) da barra superior, mantendo salvamento automático em 1s.
  - **Temporizador da Primeiras Onda (`delayBeforeFirstWave`)**: Vinculado o slider de espera inicial da primeira onda para ler e gravar dinamicamente `delayBeforeFirstWave` por mapa.
- **Validação de Build**:
  - `TRL-DynamicSpawn-Client.csproj` e `TRL-DynamicSpawn-Server.csproj` compilados com **0 Erros**.

### 2026-08-04 — Fix de Sincronização do Raio no DynamicMaps, Backlogs 002, 003 e 005

- **Fix do Raio da Bolha & Normalização de Nomes de Mapa**:
  - Corrigida a divergência no JavaScript do Web UI (`Index.razor`) que salvava apenas `despawnDistance` e ignorava `spawnBubbleDistance`.
  - Corrigida a normalização de `MapNameHelper.Normalize("bigmap")` para retornar `"bigmap"`, alinhado com a chave do `config.json`.
  - Atualizada a consulta de `TRLMapBubbleOverlay` para `ServerConfigProvider.Config` (polling 5s).
- **Execução do Backlog 002 (Web UI)**:
  - Renomeadas as abas no Web UI e dicionários I18N para "ONDAS" e "BOTS".
  - Reduzido o limite máximo do slider `delayBeforeFirstWave` de 1200s para 120s.
- **Execução do Backlog 003 (Labs Exclusivo PMC)**:
  - Implementada trava no `DynamicSpawnManager` para zerar vagas de Scavs em Labs (`laboratory`) e alocar 100% da cota `playerCap` para PMCs (`sptBear`/`sptUsec`).
- **Execução do Backlog 005 (Revisão de Bloqueadores de Spawn)**:
  - Native Bosses e Followers isentados da filtragem do mod no `TryToSpawnInZoneAndDelayPatch`.
  - `IsValidSpawnZone` simplificado para não rejeitar `BotZone`s inteiras por LoS.
  - Linecast atualizado com `LayerMaskClass.PlayerStaticCollisionsMask` para reconhecer portas e objetos.
  - `heightLimit` ajustado para 4.0m em mapas com múltiplos andares.
- **Compilação e Versionamento**:
  - Incrementado BepInPlugin para `3.2.3` em `Plugin.cs`.
  - Cliente e Servidor compilados com sucesso (0 erros).

### 2026-08-02 — Fix da Injeção Prematura do DynamicSpawnManager no GameWorld.OnGameStarted

- **Diagnóstico do Log `LogOutput.log`**:
  - Identificado o erro `Cannot inject DynamicSpawnManager: IBotGame is not instantiated yet` no evento `OnGameStarted`.
- **Implementação do Aguardo Assíncrono (`DynamicSpawnManagerPatch.cs`)**:
  - Adicionado helper `TryInjectImmediate(GameWorld)` e Coroutine `WaitForBotGameAndInjectCoroutine`.
- **Validação de Build**:
  - Compilado `TRL-DynamicSpawn-Client.csproj` com 0 Erros (`TRL-DynamicSpawn.dll`).

### 2026-07-28 — Stuttering Fix, Sincronização de Timers, Regras de Warmup, Histórico de Teleporte & Master Fallback

- Eliminado Stuttering de 2s em `TRLMapBubbleOverlay.cs`.
- Sincronização do Timer de Teleporte em `BotDespawnManager.cs`.
- Refinamento do `SpawnHordeLoop` e paridade do Teleporte com Master Fallback.
