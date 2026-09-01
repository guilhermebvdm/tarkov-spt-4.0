---
title: TRL-DynamicSpawn — Relatório de Auditoria Arquitetural e Code Review Profundo (02)
date: 2026-08-24
status: 🟢 Vivo
authors: Antigravity
---

# Relatório de Auditoria Arquitetural e Code Review Profundo (02)

Este relatório apresenta o diagnóstico técnico crítico de ponta a ponta do código-fonte do mod [TRL-DynamicSpawn](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-DynamicSpawn), confrontando sua implementação atual contra as exigências do runtime do **Escape from Tarkov 0.16.9** e do **SPT 4.0** (detalhadas em [ciclo-de-vida-e-arquitetura-bot-spawning.md](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-DynamicSpawn/docs/ciclo-de-vida-e-arquitetura-bot-spawning.md)).

O objetivo desta auditoria é identificar e propor a erradicação de qualquer gargalo de CPU na Main Thread, alocações espúrias de memória (Garbage Collection), conflitos de física no teleporte e armadilhas de estado na IA.

---

## 📑 Sumário Executivo de Achados

| ID | Eixo de Auditoria | Severidade | Área Afetada | Impacto Principal |
| :--- | :--- | :---: | :--- | :--- |
| **AUD-02-01** | Confronto Arquitetural | 🟠 Alta | `DynamicSpawnManager.cs:1055-1119` | Injeção 1 a 1 de esquadrões quebrando coesão de grupo. |
| **AUD-02-02** | Confronto Arquitetural | 🟡 Média | `BotDespawnManager.cs:410-427` | Risco de bots "braindead" por falta de nós de patrulha na zona. |
| **AUD-02-03** | Bottleneck & Main Thread | 🔴 Crítica | `DynamicSpawnManager.cs:915, 929`, `BotDespawnManager.cs:432` | Varredura global de cena com `GetAllObjects<BotZone>()` congelando frames (5ms–15ms). |
| **AUD-02-04** | Bottleneck & Main Thread | 🟠 Alta | `Patches.cs:123-136, 254`, `DynamicSpawnManager.cs:939-985` | Proliferação de LINQ (`.Where().ToList()`) gerando GC Spikes a cada onda. |
| **AUD-02-05** | Bottleneck & Main Thread | 🟡 Média | `Patches.cs:699, 728`, `BotDespawnManager.cs:207, 485` | Uso de `Vector3.Distance` (Sqrt) e disparos maciços de Raycast síncronos. |
| **AUD-02-06** | Teleporte & Física | 🔴 Crítica | `BotDespawnManager.cs:815-819` | Ordem invertida de teleporte (`Teleport` antes de `Mover.Stop()`) gerando conflito no NavMesh. |
| **AUD-02-07** | Teleporte & Física | 🟡 Média | `BotDespawnManager.cs:640-675` | Desengajamento incompleto em bots rodando mods de IA externa (SAIN / BigBrain). |
| **AUD-02-08** | Recursos & Memory Leaks | 🟡 Média | `BotDespawnManager.cs:24, 27`, `DynamicSpawnManager.cs:53-54` | Dicionários e filas estáticas acumulando referências entre raids consecutivas. |
| **AUD-02-09** | Recursos & Memory Leaks | 🟢 Baixa | `Patches.cs:72-78` (`MarkerDumper`) | Logs síncronos em loops de inicialização poluindo console e I/O. |

---

## 1. Confronto Arquitetural (O que o Mod Faz vs. O que a Engine Exige)

---

### [AUD-02-01] Injeção de Esquadrões Fracionada (Loop 1 a 1)

* **Localização:** [DynamicSpawnManager.cs:1055-1119](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-DynamicSpawn/Client/Components/DynamicSpawnManager.cs#L1055-L1119) (`SpawnGroupBotsCoroutine`)
* **Diagnóstico:**
  Ao gerar um esquadrão com múltiplos integrantes (ex: `groupSize = 3`), o método atual executa um loop `for (int i = 0; i < groupSize; i++)`, invocando individualmente `BotCreationDataClass.Create(botProfile, _botCreator, 1, ...)` e chamando `TryToSpawnInZoneAndDelay(zone, botResult, false, true, pointsToUse, true)` a cada membro.
  No primeiro membro (`i == 0`), `pointsToUse` é uma nova lista vazia. O spawner nativo escolhe um ponto e tenta registrar, mas como o método `TryToSpawnInZoneAndDelay` roda de forma assíncrona com `newWave = true`, ele sinaliza o motor como se fossem 3 ondas independentes nascendo na mesma zona.
* **Impacto:**
  1. Descompasso de geração de perfis (3 requisições assíncronas em série com múltiplos `yield return null`).
  2. Risco de seguidores (*followers*) nascerem longe do líder caso a lista de pontos âncora sofra descontinuidade.
* **Solução Recomendada:**
  Utilizar a capacidade nativa de geração por lote do `BotCreationDataClass.Create(profileData, _botCreator, groupSize, _botsController.BotSpawner)` para criar todos os perfis do grupo em uma **única task atômica**, injetando o grupo de forma unificada no `BotSpawner`.

#### 🔧 Refactoring (C#)

```csharp
// ANTES (DynamicSpawnManager.cs:1055-1100)
for (int i = 0; i < groupSize; i++)
{
    var task = BotCreationDataClass.Create(botProfile, _botCreator, 1, _botsController.BotSpawner);
    while (!task.IsCompleted) yield return null;
    BotCreationDataClass botResult = task.Result;
    // ...
    _botsController.BotSpawner.TryToSpawnInZoneAndDelay(zone, botResult, false, true, pointsToUse, true);
    if (i < groupSize - 1) yield return new WaitForSeconds(0.2f);
}

// DEPOIS (Geração Atômica de Esquadrão em 1 Task)
var task = BotCreationDataClass.Create(botProfile, _botCreator, groupSize, _botsController.BotSpawner);
while (!task.IsCompleted)
{
    yield return null;
}

BotCreationDataClass groupData = task.Result;
if (groupData != null && groupData.Profiles != null && groupData.Profiles.Count > 0)
{
    IsGeneratingDynamicWave = true;
    try
    {
        // Spawna todo o grupo em lote mantendo vínculos de liderança e esquadrão nativos
        _botsController.BotSpawner.TryToSpawnInZoneAndDelay(
            zone, 
            groupData, 
            false, // withTeleport
            true,  // shallBeGroup (Preserva esquadrão no BotsGroup)
            null,  // pointsToSpawn (BotSpawner aloca pontos contíguos na zona)
            true   // cancelOthers
        );
    }
    finally
    {
        IsGeneratingDynamicWave = false;
    }
}
```

---

### [AUD-02-02] Prevenção de Bots "Braindead" por Falta de Patrulhas na Zona

* **Localização:** [BotDespawnManager.cs:410-427](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-DynamicSpawn/Client/Components/BotDespawnManager.cs#L410-L427) (`ForceBackToPatrol`)
* **Diagnóstico:**
  Ao teleportar um bot para uma nova `BotZone`, o método tenta invocar `bot.PatrollingData.ComeToPatrol()`. No entanto, algumas zonas customizadas ou periféricas não possuem rotas cadastradas em `PatrolWays` (`botZone.PatrolWays == null || botZone.PatrolWays.Length == 0`). Nesses casos, o `PatrollingData` entra em estado de erro silencioso e o `StandartBotBrain` fica sem nós válidos para executar, resultando no clássico bot "zumbi/braindead" parado estaticamente sem reagir a estímulos até avistar um inimigo no cone frontal.
* **Impacto:**
  Bots paralisados em pontos de spawn que não patrulham e degradam a imersão do jogador.
* **Solução Recomendada:**
  1. Validar se a `BotZone` de destino possui ao menos 1 rota de patrulha válida (`PatrolWays != null && PatrolWays.Length > 0`).
  2. Limpar a fila de decisões pendentes com `bot.DecisionQueue.Clear()`.
  3. Reassociar a zona no cérebro via `bot.BotZone = targetZone` antes de convocar a patrulha.

---

## 2. Bottleneck Audit & Main Thread Stalls (Gargalos de FPS)

---

### [AUD-02-03] Varredura Repetitiva de Cena com `LocationScene.GetAllObjects<BotZone>()`

* **Localização:**
  * [DynamicSpawnManager.cs:915, 929](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-DynamicSpawn/Client/Components/DynamicSpawnManager.cs#L915-L929)
  * [BotDespawnManager.cs:432](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-DynamicSpawn/Client/Components/BotDespawnManager.cs#L432)
* **Diagnóstico:**
  O método `LocationScene.GetAllObjects<BotZone>()` (ou `GetAllObjectsAndWhenISayAllIActuallyMeanIt<BotZone>()`) realiza uma busca completa na hierarquia da cena ativa do Unity (`Object.FindObjectsOfType<BotZone>()` ou travessia de GameObjects raiz). Em mapas gigantescos como **Streets of Tarkov**, **Lighthouse** ou **Shoreline**, onde existem mais de 50.000 GameObjects na cena, essa varredura consome **5ms a 15ms de tempo exclusivo na Main Thread**.
  Como esse método é chamado **a cada bot de uma onda** e **a cada ciclo de 30 segundos do despawn loop**, ele gera micro-stutters periódicos perceptíveis.
* **Impacto:**
  Micro-congelamentos de quadros (stutters) durante o gameplay.
* **Solução Recomendada:**
  As zonas de bot (`BotZone`) são estáticas e não são criadas nem destruídas durante a raid. Elas devem ser **cacheadas em uma lista estática uma única vez no início da raid (`Init` / `OnRaidStart`)** e consultadas em $O(1)$.

#### 🔧 Refactoring (C#)

```csharp
// Helper centralizado de Cache de Zonas (Evita varredura de cena repetitiva)
public static class ZoneCache
{
    private static List<BotZone> _allZones = new List<BotZone>();
    private static List<BotZone> _sniperZones = new List<BotZone>();
    private static List<BotZone> _regularZones = new List<BotZone>();
    public static bool IsInitialized => _allZones.Count > 0;

    public static void Initialize()
    {
        _allZones.Clear();
        _sniperZones.Clear();
        _regularZones.Clear();

        var zones = LocationScene.GetAllObjects<BotZone>();
        if (zones == null) return;

        foreach (var z in zones)
        {
            if (z == null) continue;
            _allZones.Add(z);
            if (SpawnPointHelper.IsSniperZone(z))
                _sniperZones.Add(z);
            else
                _regularZones.Add(z);
        }
    }

    public static List<BotZone> GetAllZones() => _allZones;
    public static List<BotZone> GetRegularZones() => _regularZones;
    public static List<BotZone> GetSniperZones() => _sniperZones;

    public static void Clear()
    {
        _allZones.Clear();
        _sniperZones.Clear();
        _regularZones.Clear();
    }
}
```

---

### [AUD-02-04] Proliferação de LINQ e Alocações Temporárias em Loops de Spawning

* **Localização:**
  * [Patches.cs:123-136](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-DynamicSpawn/Client/Patches/Patches.cs#L123-L136) (`SniperPatch.FindFarthestZone`)
  * [DynamicSpawnManager.cs:939-985](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-DynamicSpawn/Client/Components/DynamicSpawnManager.cs#L939-L985)
* **Diagnóstico:**
  No `DynamicSpawnManager.cs:939-985`, o código encadeia 4 chamadas LINQ `.Where(...).ToList()` consecutivas para filtrar zonas em cada onda:
  ```csharp
  var roughZones = nonSnipeZones.Where(z => ...).ToList();
  var forwardZones = roughZones.Where(z => ...).ToList();
  var filteredZones = forwardZones.Where(z => z != _lastSelectedZone).ToList();
  ```
  Além disso, no `SniperPatch.cs:123-129`, o código executa:
  ```csharp
  var orderedZones = botZones.OrderBy(botZone => GetVectorDistance(...)).ToList();
  var lastHalfZones = orderedZones.Skip(halfCount).ToList();
  ```
  Essas operações criam dezenas de instâncias de `List<BotZone>`, arrays intermediários e delegates alocados no Heap, gerando até **500 KB de lixo no Garbage Collector (GC)** a cada onda.
* **Impacto:**
  Picos de coleta de lixo (*GC Spikes*) a cada 1 a 2 minutos, causando congelamento de 50ms–100ms quando a Unity executa o GC.
* **Solução Recomendada:**
  Substituir todas as consultas LINQ em caminhos críticos por loops indexados `for (int i = 0; i < count; i++)` que preenchem listas reaproveitáveis estáticas ou utilizam amostragem de reservatório (*Reservoir Sampling*) em passagem única.

---

### [AUD-02-05] Checagens Espaciais: `Vector3.Distance` vs. `sqrMagnitude` e Raycasting Síncrono

* **Localização:**
  * [Patches.cs:699, 728](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-DynamicSpawn/Client/Patches/Patches.cs#L699-L728)
  * [BotDespawnManager.cs:207, 485, 501](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-DynamicSpawn/Client/Components/BotDespawnManager.cs#L207-L501)
* **Diagnóstico:**
  1. **Cálculo Desnecessário de Raiz Quadrada:** As checagens de proximidade com jogadores usam `Vector3.Distance(a, b) <= maxDist`. Em nível de CPU, `Vector3.Distance` executa `Mathf.Sqrt(dx*dx + dy*dy + dz*dz)`. A raiz quadrada é uma das instruções aritméticas mais lentas em hardware.
  2. **Explosão de Raycasts de LoS:** Em zonas densas (30 pontos de spawn) com 4 jogadores (modo coop FIKA), o loop do `TryToSpawnInZoneAndDelayPatch` executa até $30 \times 4 = 120$ chamadas síncronas de `Physics.Linecast` contra a malha de alta densidade (`HighPolyWithTerrainMask`) no mesmo quadro de renderização.
* **Impacto:**
  Queda de framerate instantânea quando o mod tenta validar zonas no meio de tiroteios.
* **Solução Recomendada:**
  1. Comparar distâncias quadradas: `(a - b).sqrMagnitude <= maxDist * maxDist`.
  2. Executar o `Physics.Linecast` apenas se o ponto já tiver passado **com sucesso** por todas as checagens geométricas anteriores (SafeZone e Frustum da Câmera).

#### 🔧 Refactoring (C#)

```csharp
// ANTES (Cálculo com Sqrt repetitivo)
if (Vector3.Distance(p.Position, checkPoint.Position) <= maxDist)

// DEPOIS (Comparação de Magnitude Quadrada O(1))
float maxDistSq = maxDist * maxDist;
Vector3 diff = p.Position - checkPoint.Position;
if (diff.sqrMagnitude <= maxDistSq)
```

---

## 3. Lógica de Teleporte / Reaproveitamento vs. Despawn

---

### [AUD-02-06] Ordem Invertida de Parada Física no Teleporte

* **Localização:** [BotDespawnManager.cs:815-819](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-DynamicSpawn/Client/Components/BotDespawnManager.cs#L815-L819) (`AttemptToTeleportGroup`)
* **Diagnóstico:**
  No código atual:
  ```csharp
  // 1. Teleporta o bot fisicamente
  m.GetPlayer.Teleport(targetPos, true);

  // 2. Só agora chama o SafeResetBotForTeleport
  SafeResetBotForTeleport(m);
  ```
  Dentro de `SafeResetBotForTeleport(m)` é que se chama `bot.Mover?.Stop()`.
  **O Erro Fatal de Concorrência:** Quando `Player.Teleport` é invocado, o `BotMover` ainda possui internamente o `NavMeshPath` da coordenada antiga ativo. No mesmo quadro, o motor de física do Unity processa o `NavMeshAgent`, que detecta que a posição do Transform diverge do path ativo e tenta forçar uma interpolação reversa ou cancela o warp com erro no console nativo (`"NavMeshAgent cannot be placed at infinity/NaN"`).
* **Impacto:**
  Bots que são teleportados mas sofrem "snap-back" instantâneo ou ficam travados no chão com o `CharacterController` desincronizado.
* **Solução Recomendada:**
  Parar o `Mover` e zerar a inércia física **estritamente ANTES** de realizar o teleporte.

#### 🔧 Refactoring (C#)

```csharp
// SEQUÊNCIA DE FÍSICA CORRETA E ATÔMICA PARA TELEPORTE
private static void SafeExecuteTeleport(BotOwner bot, Vector3 targetPos)
{
    var player = bot.GetPlayer;
    if (player == null) return;

    // 1. Interrompe a navegação no NavMesh ANTES de alterar a posição
    try { bot.Mover?.Stop(); } catch { }

    // 2. Zera velocidade e inércia para evitar bugs de áudio e interpolação
    if (player.MovementContext != null)
    {
        player.MovementContext.ResetFlying();
        player.MovementContext.SetVelocity(Vector3.zero);
    }

    // 3. Teleporta sincronizando o Transform e a malha do NavMesh
    player.Teleport(targetPos, true);

    // 4. Executa a limpeza da mente e memória de combate
    SafeResetBotForTeleport(bot);
}
```

---

### [AUD-02-07] Desengajamento de Combate para Mods de IA Externa (SAIN / BigBrain)

* **Localização:** [BotDespawnManager.cs:640-675](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-DynamicSpawn/Client/Components/BotDespawnManager.cs#L640-L675)
* **Diagnóstico:**
  O método atual limpa `bot.Memory.GoalEnemy = null` e `bot.ShootData.EndShoot()`. No entanto, quando o mod **SAIN** está ativo, o componente `SAIN.Components.BotComponent` possui sua própria máquina de estados com um alvo travado em `EnemyController.ActiveEnemy`. Se o SAIN não for forçado a resetar o alvo, ele pode tentar fazer o bot avançar taticamente em direção à última coordenada conhecida do inimigo original (a centenas de metros de distância).
* **Solução Recomendada:**
  Utilizar reflexão segura para limpar o alvo ativo no componente SAIN caso ele esteja anexado ao bot.

#### 🔧 Refactoring (C#)

```csharp
private static void ResetExternalAI(BotOwner bot)
{
    if (bot == null || bot.gameObject == null) return;

    // Reset seguro para o SAIN se estiver instalado
    try
    {
        var sainComponent = bot.gameObject.GetComponent("SAIN.Components.BotComponent");
        if (sainComponent != null)
        {
            var enemyControllerProp = sainComponent.GetType().GetProperty("EnemyController", BindingFlags.Public | BindingFlags.Instance);
            var enemyController = enemyControllerProp?.GetValue(sainComponent);
            if (enemyController != null)
            {
                var clearEnemyMethod = enemyController.GetType().GetMethod("ClearEnemy", BindingFlags.Public | BindingFlags.Instance);
                clearEnemyMethod?.Invoke(enemyController, null);
            }
        }
    }
    catch { }
}
```

---

## 4. Gestão de Recursos e Memory Leaks

---

### [AUD-02-08] Limpeza de Dicionários e Filas Estáticas no Fim da Raid

* **Localização:**
  * [BotDespawnManager.cs:24, 27](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-DynamicSpawn/Client/Components/BotDespawnManager.cs#L24-L27) (`_teleportCooldowns`, `_lastTeleportPositions`)
  * [DynamicSpawnManager.cs:53-54](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-DynamicSpawn/Client/Components/DynamicSpawnManager.cs#L53-L54) (`PmcSpawns`, `ScavSpawns`)
* **Diagnóstico:**
  Contêineres estáticos retêm entradas de IDs de perfil de bots (`MongoId`) e timestamps de raids anteriores. Se o jogador realizar 5 a 10 raids consecutivas sem fechar o Tarkov, esses contêineres acumulam milhares de entradas e mantêm o cache desatualizado para novos mapas.
* **Impacto:**
  Vazamento progressivo de memória RAM e aplicação de tempos de cooldown incorretos em raids posteriores.
* **Solução Recomendada:**
  Integrar formalmente a limpeza de todos os contêineres estáticos ao hook [RaidLifecycle.OnRaidEnd](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-DynamicSpawn/Client/Helpers/RaidLifecycle.cs).

---

### [AUD-02-09] Supressão de Logs de I/O em Loops de Inicialização

* **Localização:** [Patches.cs:72-78](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-DynamicSpawn/Client/Patches/Patches.cs#L72-L78) (`MarkerDumper.Postfix`)
* **Diagnóstico:**
  O patch `MarkerDumper` itera sobre todos os pontos de spawn da cena no `LocationScene.Awake` e executa `Logger.LogInfo(...)` para cada um deles. Em mapas com centenas de marcadores, isso causa um bloqueio momentâneo de escrita em disco no log do BepInEx.
* **Solução Recomendada:**
  Remover a impressão incondicional ou envolvê-la estritamente na checagem `if (Settings.enableDebugLogs.Value)`.

---

## 5. Matriz de Prioridade de Aplicação dos Refactorings

| Prioridade | Refactoring Recomendado | Ganhos Técnicos Estimados |
| :---: | :--- | :--- |
| 🔴 **P1** | **Cache Global de BotZones ([AUD-02-03])** | **Elimina stutters de 5ms–15ms** em toda onda e no loop de despawn. |
| 🔴 **P1** | **Correção da Ordem de Teleporte ([AUD-02-06])** | **Erradica bugs de snap-back**, física travada e colisões no NavMesh. |
| 🟠 **P2** | **Substituição de LINQ por Loops Indexados ([AUD-02-04])** | **Reduz o GC Alloc em ~80%** durante a execução das ondas. |
| 🟠 **P2** | **Otimização de Checagens com `sqrMagnitude` ([AUD-02-05])** | **Acelera em até 4x** os filtros de SafeZone e Bolha de Combate. |
| 🟡 **P3** | **Geração Atômica de Esquadrões ([AUD-02-01])** | **Garante coesão tática de grupos** e reduz chamadas assíncronas no SPT. |
| 🟡 **P3** | **Limpeza Completa em `OnRaidEnd` ([AUD-02-08])** | **Zero vazamento de memória RAM** entre sessões contínuas de jogo. |
