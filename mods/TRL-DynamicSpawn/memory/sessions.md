# Memory — TRL-DynamicSpawn

Memória cronológica de sessões de trabalho (timestamps em GMT-3). Cada entrada resume as alterações efetuadas, decisões de arquitetura e estado atual. Atualizada ao fim de cada sessão de trabalho.

> **Por que existe:** o usuário trabalha múltiplos chats/sessões em paralelo. Este arquivo preserva o contexto técnico, decisões de design e roadmap do mod `TRL-DynamicSpawn`, evitando que futuras sessões com assistentes AI reabram discussões do zero.

---

## Estado Atual (Snapshot ao Fim da Sessão — 2026-08-16)

**Mod C# Client (v3.2.9) + C# Server (v3.2.9) compilados com sucesso (0 erros).**

- **Identity**: `TRL-DynamicSpawn` (Client BepInEx DLL: `TRL-DynamicSpawn.dll`, Server C# DLL: `TRL-DynamicSpawn-Server.dll` com Web UI). Compatível com SPT 4.0.13 e EFT 0.16.9.
- **Correção de Vazamento de Rogues e Raiders a 0% (`v3.2.9`)**:
  - Adicionadas travas estritas no `DynamicSpawnManager.cs` (`RandomRogueGroup` e `RandomRaiderGroup`).
  - O mod agora valida `GetBossChanceForMap > 0`, `Enable == true` e `!DisableBosses` antes de gerar qualquer grupo aleatório de Rogues (`exUsec`) ou Raiders (`pmcBot`).
  - Se o mapa estiver configurado com 0% para o bot na aba BOTS, nenhum grupo aleatório vazará naquele mapa.
- **Nível de Log de Falha na Criação de Perfil (`DynamicSpawnManager.cs`)**:
  - Rebaixada a mensagem quando o gerador assíncrono do SPT não retorna um perfil no frame de `LogError` para `LogWarning` (`Bot profile creation skipped... Member safely skipped`), indicando com clareza que o integrante foi pulado com segurança sem afetar a raid.
  - Adicionada retentativa suave de `0.1s` antes do fallback de dificuldade, reduzindo a frequência de mensagens no console.
- **Agressividade Total do Zryachiy Não-Nativo (`v3.2.8`)**:
  - Implementado `ZryachiyAggressivenessPatch.cs` (patcheando `ZyriachyBossLogicClass.IsEnemyNow` e `Activate`).

---

## Pendências / Próximos Passos Conhecidos (Roadmap)

- 🟡 [P-ROADMAP-01] **Retorno do Viés Direcional (Pós-Debug)**: Retornar a proporção do viés direcional de spawn/teleport para 70% frontal / 30% traseiro pós-testes.
- 🟡 [P-ROADMAP-04] **Standalone Mod — Limpador de Corpos Inteligente (Corpse Cleaner)**: Novo mod separado focado em performance (timer individual por corpo).

---

## Histórico de Sessões

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
