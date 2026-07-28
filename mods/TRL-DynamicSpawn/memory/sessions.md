# Memory — TRL-DynamicSpawn

Memória cronológica de sessões de trabalho (timestamps em GMT-3). Cada entrada resume as alterações efetuadas, decisões de arquitetura e estado atual. Atualizada ao fim de cada sessão de trabalho.

> **Por que existe:** o usuário trabalha múltiplos chats/sessões em paralelo. Este arquivo preserva o contexto técnico, decisões de design e roadmap do mod `TRL-DynamicSpawn`, evitando que futuras sessões com assistentes AI reabram discussões do zero.

---

## Estado Atual (Snapshot ao Fim da Sessão — 2026-07-28)

**Mod C# Client + TypeScript Server completo e compilado (0 erros).**

- **Identity**: `TRL-DynamicSpawn` (Client BepInEx DLL: `TRL-DynamicSpawn.dll`, Server: SPT Server Mod em TypeScript com Web UI). Compatible com SPT 4.0.13 e EFT 0.16.9.
- **Sistema de Bolha de Spawn (`SpawnBubbleDistance`) & Zona Segura (`SafeZoneDistance`)**:
  - Bolha de Spawn (padrão 300m): bots dentro da bolha de 300m ao redor de qualquer jogador vivo são mantidos. Bots fora da bolha ($\ge 300m$) e fora de vista (LoS) entram na fila de teletransporte para a frente da progressão do jogador.
  - Zona Segura (padrão 100m / Factory 15m): bots nunca nascem ou são teleportados a menos de 100m do jogador.
  - Teletransporte: bots dentro da bolha de 300m (ou na zona segura de 100m) **NUNCA são teleportados**. Apens bots fora da bolha ($\ge 300m$) e sem visão direta são movidos.
- **Overlay no Mapa (`TRLMapBubbleOverlay.cs`)**:
  - Desenha no minimapa/mapa do SPT-DynamicMaps os anéis da Zona Segura (amarelo/laranja, 100m), Bolha de Spawn (azul, 300m) e o cone de visão/LoS (amarelo).
  - Otimizado com **0 alocações de GC**: usa apenas `GameObject.Find("MapView")`. Se o mapa estiver fechado (`activeInHierarchy == false`), aborta na primeira linha sem executar `FindObjectsOfType`.
  - Configuração no BepInEx F12 na aba `Map Overlay (SPT-DynamicMaps)` (tecla de atalho para toggle individual das camadas).
- **Fluxo de Ondas, Warmup e Timers (`SpawnHordeLoop` em `DynamicSpawnManager.cs`)**:
  - **Janela Vanilla Inicial (0s a 60s)**: `IsWarmupActive = true` nos primeiros 60s (`DelayBeforeFirstWave`), liberando os spawns nativos do jogo para preencher o mapa na inicialização.
  - **Trava do Vanilla aos 60s**: Ao completar 60s, `IsWarmupActive = false`. O patch de módulo (`DisableVanillaWavesPatch`) trava todas as ondas comuns do vanilla e o `DynamicSpawnManager` assume o controle 100%.
  - **Warmup (Loop de 30 em 30s)**: Timer responsável pelos spawns de fato. Roda a cada 30s preenchendo o mapa suavemente até atingir `aliveBots >= maxCap`.
  - **Cooldown de Ondas (`SecondsBetweenWaves`, ex: 600s)**: Quando o mapa atinge 100% da capacidade (`aliveBots >= maxCap`), o Warmup para e inicia a contagem regressiva do `SecondsBetweenWaves`. Ao expirar o cooldown, o Warmup (loop de 30s) é reativado.
  - **Limpeza de Fila Pré-Contagem**: No início de cada pulso do Warmup, o mod executa `ClearSptQueue()` + 1.0s de pausa para garantir que requisições presas na fila do SPT não distorçam a contagem real de bots vivos.
  - **Balanceamento Dinâmico de Facções**: `ProcessWave` conta `aliveBears`, `aliveUsecs` e `aliveScavs` em tempo real e calcula as vagas distribuindo prioritariamente para a facção com maior defasagem.
  - **Exclusividade de Bots Comuns**: As ondas dinâmicas gerenciam **apenas** `pmcUSEC`, `pmcBEAR` e `assault` (Scavs). Bosses, cultistas e elites não são gerenciados pelo loop comum.
- **Espaçamento, Histórico e Cascata com Master Fallback (`Patches.cs` e `BotDespawnManager.cs`)**:
  - **Alternância de `BotZone`**: Spawns e Teleportes consecutivos priorizam enviar grupos para `BotZone`s diferentes (`_lastSelectedZone` nos Spawns e `_lastTeleportZone` nos Teleportes).
  - **Histórico de `ISpawnPoint`**: Filas de histórico de 6 posições (`_lastSpawnPositions` e `_lastTeleportPositions`). Pontos a menos de 50m de um spawn/teleporte recente são categorizados como `tooCloseToRecent` e movidos para fallback.
  - **Cascata Estrita de Decisão**:
    1. 🥇 `strictPoints`: Dentro da Bolha ($\le 300m$) + Fora da Zona Segura ($\ge 100m$) + Sem LoS + Ponto Fresco ($>50m$).
    2. 🥈 `fallbackStrictPoints`: Dentro da Bolha ($\le 300m$) + Fora da Zona Segura ($\ge 100m$) + Sem LoS + Histórico Reutilizado ($<50m$).
    3. 🥉 Pontos secundários da bolha com LoS relaxado se necessário.
    4. 🚨 **Master Fallback (ÚLTIMA INSTÂNCIA)**: Acionado **apenas se todas as opções dentro da bolha retornarem 0 pontos**. Libera pontos fora da bolha no mapa, **mantendo 100% INVIOLÁVEIS a Zona Segura (100m) e a ausência de Line of Sight (!LoS)**.

---

## Pendências / Próximos Passos Conhecidos (Roadmap)

- 🟡 [P-ROADMAP-01] **Retorno do Viés Direcional (Pós-Debug)**: Retornar a proporção do viés direcional de spawn/teleport de 100/0% frontal (usado temporariamente no modo de testes) para **70% frontal / 30% traseiro** assim que os testes em jogo forem concluídos pelo usuário.
- 🟡 [P-ROADMAP-02] **Validação Mínima no Server Web UI**: Adicionar regra de validação mínima de 60s para o campo `Delay Before First Wave` no formulário Web UI do mod no Servidor.
- 🟡 [P-ROADMAP-03] **Refatoração de Perfis PMC**: Refatorar a criação do perfil PMC (`BotProfileDataClass`) para honrar a dificuldade recebida no Server do Mod, bypassando a limitação do `BotsPresets` original que aborta a geração caso o SPT altere a dificuldade nos bastidores.
- 🟡 [P-ROADMAP-04] **Standalone Mod — Limpador de Corpos Inteligente (Corpse Cleaner)**: Novo mod separado focado em performance. Timer individual instanciado por corpo ao invés de um wipe global, para distribuição no Forge SPT.
- 🟡 [P-ROADMAP-05] **Teleporte Baseado em Inércia de Rota**: Capturar o rastro (breadcrumb) do player no último minuto. Se a distância líquida for curta (looteando/defendendo), manter spawn aleatório (360º). Se for longa (travessia), aplicar viés direcional de spawn de 70% na frente da rota projetada e 30% nas costas.

---

## Histórico de Sessões

### 2026-07-28 — Stuttering Fix, Sincronização de Timers, Regras de Warmup, Histórico de Teleporte & Master Fallback

- **Eliminação do Stuttering de 2s (`TRLMapBubbleOverlay.cs`)**:
  - Removido o fallback `FindObjectsOfType<MonoBehaviour>()` que varria a cena a cada 2.0s enquanto o mapa estava fechado.
  - Implementada verificação leve via `GameObject.Find("MapView")` e trava instantânea `activeInHierarchy` com 0 alocações de memória GC.
- **Sincronização do Timer de Teleporte (`BotDespawnManager.cs`)**:
  - Leitura imediata de `_currentLocation` e `DespawnInterval` do servidor no início de cada pulso do `DespawnLoop`.
  - Fixado o raio da bolha em `SpawnBubbleDistance` (300m), garantindo que bots a menos de 300m nunca sejam teleportados.
- **Refinamento do `SpawnHordeLoop` (7 Regras de Spawns Dinâmicos)**:
  - Garantiu a janela inicial de 60s para o spawn vanilla agir.
  - Trava do spawn vanilla aos 60s (`IsWarmupActive = false`).
  - Warmup de 30 em 30s como único responsável por disparar spawns de preenchimento.
  - `SecondsBetweenWaves` ativado como cooldown longo somente quando o mapa atinge 100% da capacidade (`aliveBots >= maxCap`).
  - Limpeza de fila do SPT (`ClearSptQueue()`) executada **antes** da contagem de bots vivos e balanceamento de facções.
- **Paridade do Teleporte com o Spawn & Master Fallback (`Patches.cs` e `BotDespawnManager.cs`)**:
  - Adicionada alternância de `BotZone` (`_lastTeleportZone`) e histórico de `ISpawnPoint` (`_lastTeleportPositions` queue 6) no teleporte.
  - Implementada a cascata estrita de seleção de pontos com **Master Fallback em última instância** se a bolha não possuir pontos viáveis.
  - **Trava Inviolável**: Mesmo no Master Fallback, o bot **NUNCA** pode spawnar/teleportar dentro da Zona Segura de 100m ou visível na tela (LoS) do jogador.
- **Compilação**: Projeto `TRL-DynamicSpawn-Client.csproj` compilado com 0 erros e 0 warnings impeditivos (`TRL-DynamicSpawn.dll`).
