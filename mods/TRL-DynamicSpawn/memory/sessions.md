# Memory — TRL-DynamicSpawn

Memória cronológica de sessões de trabalho (timestamps em GMT-3). Cada entrada resume as alterações efetuadas, decisões de arquitetura e estado atual. Atualizada ao fim de cada sessão de trabalho.

> **Por que existe:** o usuário trabalha múltiplos chats/sessões em paralelo. Este arquivo preserva o contexto técnico, decisões de design e roadmap do mod `TRL-DynamicSpawn`, evitando que futuras sessões com assistentes AI reabram discussões do zero.

---

## Estado Atual (Snapshot ao Fim da Sessão — 2026-08-04)

**Mod C# Client (v3.2.5) + C# Server compilados com sucesso (0 erros).**

- **Identity**: `TRL-DynamicSpawn` (Client BepInEx DLL: `TRL-DynamicSpawn.dll`, Server C# DLL: `TRL-DynamicSpawn-Server.dll` com Web UI). Compatível com SPT 4.0.13 e EFT 0.16.9.
- **Sincronização do Raio da Bolha & DynamicMaps Overlay (`TRLMapBubbleOverlay.cs` / `MapNameHelper.cs` / `Index.razor`)**:
  - Slider do painel web (`#mc_despawn_dist`) atualiza em sincronia tanto `spawnBubbleDistance` quanto `despawnDistance`.
  - `MapNameHelper.Normalize("bigmap")` normalizado para `"bigmap"`, garantindo correspondência com a chave de `mapConfigs` do `config.json` e Web UI.
  - `TRLMapBubbleOverlay` e `DynamicSpawnManager` consultam `ServerConfigProvider.Config` continuamente (polling a cada 5s), permitindo ajustes em tempo real durante a partida.
- **Painel Web & Nomenclaturas (Backlog 002 — 🟢 Entregue)**:
  - Abas renomeadas: *"Configuração de mapas"* -> **"ONDAS"** (`WAVES`), *"Configuração de Bosses"* -> **"BOTS"**.
  - Slider de espera inicial da primeira onda (`delayBeforeFirstWave`) ajustado para o limite máximo de **120 segundos** (0 a 120s).
- **Labs Exclusivo para PMCs (Backlog 003 — 🟢 Entregue)**:
  - Quando a partida for no mapa Labs (`laboratory`), `idealScavs` e `scavSlots` no `DynamicSpawnManager` são forçados para `0`.
  - 100% da cota do `MaxBot` em Labs é alocada para PMCs (`sptBear` e `sptUsec`).
  - Raiders nativos do jogo/alarmes permanecem intactos sob gerência da engine Vanilla.
- **Dificuldade de Bots & Integração com SAIN (Backlog 004 — 🟢 Entregue)**:
  - Detecção automática do mod SAIN (`IsSainInstalled`). Se ativo, ignora a sobreposição e repassa `BotDifficulty.normal` para ceder 100% do controle da IA ao SAIN.
  - Amostragem ponderada (`GetRandomDifficulty`) respeitando as porcentagens do painel Web para PMCs, Scavs e Bosses quando o SAIN não está presente.
  - Sincronização prévia com `AddToTargetBackup` e fallback automático transparente para `BotDifficulty.normal` se o SPT retornar `null` ao criar o perfil.
  - Logs de inspeção `[SPY]` e `[SPY-FALLBACK]` no console BepInEx para depuração ativa.
- **Revisão de Bloqueadores de Spawn (Backlog 005 — 🟢 Entregue)**:
  - **Isenção de Bosses Nativos**: Bots com `WildSpawnType` iniciando com `boss` ou `follower` são isentados das restrições de SafeZone e LoS do mod no `TryToSpawnInZoneAndDelayPatch`.
  - **Remoção do Duplo Filtro de LoS**: `IsValidSpawnZone` não testa mais a visibilidade do pivot da `BotZone` inteira.
  - **Inclusão de Colisores de Portas e Objetos**: `Physics.Linecast` expandido com `LayerMaskClass.PlayerStaticCollisionsMask`.
  - **Altura Vertical Ajustada para 4.0m**: Parâmetro `heightLimit` ajustado para 4.0m em mapas verticais multiares.
- **Otimização da Fila de Warmup (Backlog 006 — 🟢 Entregue)**:
  - Implementado o pré-carregamento em lote de perfis (`AddToTargetBackup`) no início de cada `ProcessWave`.
  - Toda a cota da onda (`usecSlots`, `bearSlots`, `normalScavSlots`, `pScavSlots`) é solicitada em uma única requisição HTTP síncrona/lote para o servidor SPT.
  - A Coroutine de injeção suave no Unity consome os perfis direto da RAM com 0ms de latência, mantendo a injeção bot a bot no mapa sem stutters e atingindo o `MaxBot` em 1 a 2 ciclos de Warmup.
  - Mantida a limpeza de requisições antigas (`ClearSptQueue()`) e o controle estrito de bots vivos no mapa (`GetRealAliveBotsCount()`).

---

## Pendências / Próximos Passos Conhecidos (Roadmap)

- ⚪ [004-dificuldade-bots-sain-integration] **Dificuldade de Bots & Compatibilidade SAIN**: Validar aplicação de dificuldade do mod e ignorar alteração caso o SAIN esteja instalado.
- ⚪ [006-otimizacao-fila-spawn-warmup] **Otimização da Fila de Spawn no Warmup**: Investigar atrito no atingimento da cota MaxBot e garantir limpeza completa da fila de criação de bots.
- ⚪ [007-rogues-armas-montadas] **Rogues em Armas Montadas**: Investigar por que os Rogues em Lighthouse vão para metralhadoras/AGS estacionárias mas não as operam.
- 🟡 [P-ROADMAP-01] **Retorno do Viés Direcional (Pós-Debug)**: Retornar a proporção do viés direcional de spawn/teleport para 70% frontal / 30% traseiro pós-testes.
- 🟡 [P-ROADMAP-04] **Standalone Mod — Limpador de Corpos Inteligente (Corpse Cleaner)**: Novo mod separado focado em performance (timer individual por corpo).

---

## Histórico de Sessões

### 2026-08-06 — Restrição Estrita de ISpawnPoints para Sniper Scavs e Melhorias Visuais/I18N na Web UI

- **Restrição Estrita Bilateral de `ISpawnPoint`s para Sniper Scavs**:
  - Criado `SpawnPointHelper.cs` com métodos `IsSniperRole`, `IsSniperZone` e `IsSniperSpawnPoint`.
  - Atualizado `Methods.GetRandomZone(botSpawner, allowSnipeZone)` para evitar o sorteio de `Zone_SniperPeak` em fallbacks de Scavs/PMCs comuns.
  - Atualizado `Patches.cs` (`TryToSpawnInZoneAndDelayPatch`) e `BotDespawnManager.cs` (`GetValidTeleportPoint`) para filtrar bidirecionalmente bots terrestres em pontos de sniper e sniper bots em pontos terrestres.
  - Resolvido o erro `Bot creation failed even after fallback for assault in zone Zone_SniperPeak`.
- **Melhorias Visuais e I18N na Web UI (`Index.razor`)**:
  - Substituído o emoji `ℹ️` pelo ícone circular SVG `ⓘ` (TRL Design System) nas notas de alerta das seções `AI DIFFICULTY`, `EVENTS & INVASIONS` e `ADDITIONAL SPAWNPOINTS`.
  - Corrigidas as strings estáticas em português nos templates de `BOTS/RAIDERS` e `BOTS/ROGUES` em `Index.razor` (`renderActiveBossConfig`), viabilizando a tradução dinâmica ao alternar para `EN-US`.
- **Validação de Build**:
  - `TRL-DynamicSpawn-Client.csproj` e `TRL-DynamicSpawn-Server.csproj` compilados com **0 Erros**. Binários atualizados nos diretórios de plugins/mods do SPT.

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
