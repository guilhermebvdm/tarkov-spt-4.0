# Memory — TRL-DynamicSpawn

Memória cronológica de sessões de trabalho (timestamps em GMT-3). Cada entrada resume as alterações efetuadas, decisões de arquitetura e estado atual. Atualizada ao fim de cada sessão de trabalho.

> **Por que existe:** o usuário trabalha múltiplos chats/sessões em paralelo. Este arquivo preserva o contexto técnico, decisões de design e roadmap do mod `TRL-DynamicSpawn`, evitando que futuras sessões com assistentes AI reabram discussões do zero.

---

## Estado Atual (Snapshot ao Fim da Sessão — 2026-08-25)

**Mod C# Client (v3.4.1) + C# Server (v3.4.1) compilados com sucesso (0 erros).**

- **Identity**: `TRL-DynamicSpawn` (Client BepInEx DLL: `TRL-DynamicSpawn.dll`, Server C# DLL: `TRL-DynamicSpawn-Server.dll` com Web UI). Compatível com SPT 4.0.13 e EFT 0.16.9.
- **Estruturação do Workspace (Dual original/modded)**:
  - `original/`: Backup intacto da versão original canônica.
  - `modded/`: Código-fonte com as refatorações de alta performance e física aplicada.
- **Documentação de Engenharia e Ciclo de Vida**:
  - `docs/ciclo-de-vida-e-arquitetura-bot-spawning.md`: Especificação técnica de ponta a ponta cobrindo as 7 fases de ciclo de vida de bots, topologia FIKA, SAIN e SPT-Waypoints.
  - `docs/relatorio-auditoria-codigo-02.md`: Diagnóstico arquitetural completo de gargalos de CPU/GC e plano de refatoração.
- **Refatorações de Alta Performance Aplicadas (v3.4.1)**:
  - **ZoneCache ([AUD-02-03])**: Eliminação de travamentos de cena de 5ms–15ms por meio do cache estático de `BotZone`.
  - **Geração Atômica de Esquadrões ([AUD-02-01])**: Spawns de grupo criados em 1 única Task (`BotCreationDataClass.Create` com `groupSize`), preservando a coesão no `BotsGroup`.
  - **Erradicação de GC Spikes por LINQ ([AUD-02-04])**: Substituição de `.Where().ToList()` e `.OrderBy()` por loops indexados `for` em passagem única (0 bytes de GC lixo).
  - **Otimização com `sqrMagnitude` ([AUD-02-05])**: Substituição de `Vector3.Distance` por magnitude quadrada, cortando instruções de raiz quadrada (`Mathf.Sqrt`).
  - **Sequência Atômica de Física no Teleporte ([AUD-02-06] & [AUD-02-07])**: Parada de NavMesh e inércia antes do teleporte físico, com reset cirúrgico de combate e suporte defensivo para SAIN (`ClearEnemy`).
  - **Zero Memory Leaks entre Raids ([AUD-02-08])**: Limpeza completa de contêineres estáticos em `RaidLifecycle.OnRaidEnd`.

---

## Pendências / Próximos Passos Conhecidos (Roadmap)

- 🟡 [P-ROADMAP-01] **Testes em Raid / Validação de Frametime**: Validar a estabilidade do frametime e a ausência de stutters em mapas densos (Streets of Tarkov, Lighthouse).
- 🟡 [P-ROADMAP-04] **Standalone Mod — Limpador de Corpos Inteligente (Corpse Cleaner)**: Novo mod separado focado em performance (timer individual por corpo).

---

## Histórico de Sessões

### 2026-08-25 — Auditoria Arquitetural, Refatoração de Alta Performance e Estruturação original/modded (v3.4.1)

- **Documentação do Ciclo de Vida e Auditoria Profunda**:
  - Criado `docs/ciclo-de-vida-e-arquitetura-bot-spawning.md` detalhando os contratos do SPT Server, Assembly EFT, FIKA coop, SAIN e pooling.
  - Elaborado `docs/relatorio-auditoria-codigo-02.md` com o diagnóstico dos 4 eixos de auditoria e snippets C#.
  - Adicionada a **Seção 8 (Catálogo Canônico de BotZones por Mapa)** no documento técnico com todos os nomes oficiais do EFT 0.16.9 / SPT 4.0.
- **Sincronização de Zonas no Web Server (`Index.razor`)**:
  - Atualizado o dicionário `BOT_ZONES_MAP` no Web UI com 100% das zonas oficiais de todos os mapas (incluindo `ZoneScavBase2` em Woods, `ZoneMeteoStation` em Shoreline, `ZoneCarShowroom` em Streets, telhados de Rogues em Lighthouse e zonas de Snipers).
- **Estruturação Dual original/modded**:
  - Reorganizado o repositório em `original/Client`, `original/Server`, `modded/Client`, `modded/Server`.
- **Implementação do ZoneCache & Otimizações de CPU**:
  - Criado `ZoneCache.cs` para acesso em $O(1)$ a zonas de bot.
  - Refatorados `DynamicSpawnManager.cs`, `BotDespawnManager.cs` e `Patches.cs` para eliminar LINQ e usar `sqrMagnitude`.
  - Refatorado `SpawnGroupBotsCoroutine` para geração atômica de esquadrões.
  - Corrigida a ordem de física do teleporte e adicionado reset defensivo para o mod SAIN.
- **Validação de Compilação & SemVer (v3.4.1)**:
  - `TRL-DynamicSpawn-Client.csproj` e `TRL-DynamicSpawn-Server.csproj` compilados com **0 Erros**.

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
