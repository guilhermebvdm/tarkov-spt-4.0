---
title: "Relatório de Auditoria Técnica de Código — SAIN (Parte 1: Ciclo de Vida, Memória e Hooks Globais)"
date: 2026-08-31
status: 🟢 Vivo
authors: Antigravity
---

# Relatório de Auditoria Técnica de Código — SAIN (Parte 1: Ciclo de Vida, Memória e Hooks Globais)

Auditoria estática e comportamental profunda focada no **Ciclo de Vida de Raid**, **Gestão de Memória / Leaks**, **Patches de Inicialização**, **Componentes Globais** e **Interoperabilidade Client-Server** do mod SAIN.

---

## 1. Resumo Executivo da Auditoria

| Severidade | Quantidade | Descrição |
|---|---|---|
| 🔴 **Crítico** | 0 | Falhas catastróficas imediatas ou corrupção de dados |
| 🟠 **Alto** | 2 | Leaks de memória acumulativa entre mortes de bots (`AlivePlayerArray`) e alocação de coroutines/LINQ por disparo |
| 🟡 **Médio** | 4 | Reflection contínuo em `WorldTickPatch`, bug de distância em `RaycastJob`, busca de cena em `findSpawnPointMarkers` e risco de NRE em `PlayerComponent.Dispose` |
| 🔵 **Baixo** | 1 | Acesso não-defensivo a singleton em `AddBotComponentPatch` |
| 💡 **Otimização** | 2 | Proposta de pooling unificado de projéteis e cache de delegado para o Fika Co-op |

---

## 2. Tabela de Achados

| ID | Severidade | Arquivo / Linha | Categoria | Descrição Resumida |
|---|---|---|---|---|
| `AUD-01-01` | 🟠 Alto | [`PlayerSpawnTracker.cs:L185`](../modded/SAIN/Classes/PlayerManager/Players/PlayerSpawnTracker.cs#L185) | Memory Leak / CPU Churn | `AlivePlayerArray` (`HashSet`) não remove bots mortos e não é limpo no teardown, mantendo bots mortos no loop de update a raid inteira. |
| `AUD-01-02` | 🟠 Alto | [`GameWorldComponent.cs:L61-L74`](../modded/SAIN/Components/GameWorldComponent.cs#L61-L74) | GC Pressure / Coroutine Churn | Cada tiro inicia uma coroutine `TrackBullet` com alocações LINQ no Heap, gerando picos massivos de GC em tiroteios intensos. |
| `AUD-01-03` | 🟡 Médio | [`WorldTickPatch.cs:L21`](../modded/SAIN/Patches/GameWorld/WorldTickPatch.cs#L21) | Hot Path Reflection | `FikaInterop.IsClient()` executa `PropertyInfo.GetValue(null)` a cada tick de mundo (60–144 FPS) na main thread. |
| `AUD-01-04` | 🟡 Médio | [`RaycastJob.cs:L132-L140`](../modded/SAIN/Types/Jobs/RaycastJob.cs#L132-L140) | Lógica / Física Unity | Sobrecarga de `RaycastJob` baseada em `List<Vector3>` usa direção sem normalização e fixa alcance incorreto em 1 metro (`1f`). |
| `AUD-01-05` | 🟡 Médio | [`GameWorldComponent.cs:L221-L228`](../modded/SAIN/Components/GameWorldComponent.cs#L221-L228) | Polling Pesado em Cena | `findSpawnPointMarkers` executa `FindObjectsOfType<SpawnPointMarker>()` frame-a-frame até obter a câmera ativa. |
| `AUD-01-06` | 🔵 Baixo | [`AddBotComponentPatch.cs:L25`](../modded/SAIN/Patches/GameWorld/AddBotComponentPatch.cs#L25) | AP-02 Singleton Inseguro | Chamada direta `BotSpawnController.Instance.AddBot` sem checagem de nulo, gerando exceções no log em spawns antecipados. |
| `AUD-01-07` | 🟡 Médio | [`PlayerComponent.cs:L372`](../modded/SAIN/Components/PlayerComponent.cs#L372) | NRE em Teardown | `Player.MovementContext.OnStateChanged -= ...` é invocado sem operador nulo-seguro durante `Dispose()`. |

---

## 3. Detalhamento dos Achados

### AUD-01-01 · Retenção e Atualização de Bots Mortos em `AlivePlayerArray`
- **Severidade:** 🟠 Alto
- **Localização no Mod:** [`PlayerSpawnTracker.cs:L185`](../modded/SAIN/Classes/PlayerManager/Players/PlayerSpawnTracker.cs#L185), [`PlayerSpawnTracker.cs:L199-L219`](../modded/SAIN/Classes/PlayerManager/Players/PlayerSpawnTracker.cs#L199-L219)
- **Referência Cruzada:** [`GameWorldComponent.cs:L134-L147`](../modded/SAIN/Components/GameWorldComponent.cs#L134-L147)
- **Causa Raiz:** Em `TryAddPlayerComponent`, o componente é registrado tanto em `AlivePlayersDictionary` quanto em `AlivePlayerArray` (`HashSet<PlayerComponent>`). Porém, o método `TryRemove(string profileId, ...)` remove apenas do dicionário (`AlivePlayersDictionary.Remove`), esquecendo de chamar `AlivePlayerArray.Remove(playerComponent)`. Além disso, no `Dispose()`, o conjunto `AlivePlayerArray` não é limpo com `.Clear()`.
- **Impacto Técnico Real:** Como `GameWorldComponent.ManualUpdate` itera sobre `AlivePlayerArray` a cada frame para chamar `Player.ManualUpdate` e `TickSoundCaches`, bots mortos ou despawnados continuam sendo iterados pelo resto da partida inteira, desperdiçando ciclos de CPU com checagem de sons e mantendo referências vivas na memória RAM.
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  - Atualizar `TryRemove` e `Dispose` para remover e limpar explicitamente o `AlivePlayerArray`.
  - *Código Refatorado:*

```csharp
private bool TryRemove(string profileId, out bool destroyedComponent)
{
    destroyedComponent = false;
    if (profileId.IsNullOrEmpty())
    {
        ClearNullPlayers();
        return false;
    }
    if (AlivePlayersDictionary.TryGetValue(profileId, out PlayerComponent playerComponent))
    {
        OnPlayerRemoved?.Invoke(profileId, playerComponent);
        if (playerComponent != null)
        {
            destroyedComponent = true;
            AlivePlayerArray.Remove(playerComponent); // CORREÇÃO: Remove do HashSet
            playerComponent.Dispose();
        }
        AlivePlayersDictionary.Remove(profileId);
        return true;
    }
    return false;
}

public void Dispose()
{
    if (_sainGameWorld == null) return;
    var gameWorld = _sainGameWorld.GameWorld;
    if (gameWorld != null)
    {
        gameWorld.OnPersonAdd -= AddPlayer;
    }
    foreach (var (_, player) in AlivePlayersDictionary)
    {
        player?.Dispose();
    }
    AlivePlayersDictionary.Clear();
    AlivePlayerArray.Clear(); // CORREÇÃO: Limpa o HashSet no encerramento de raid
}
```

- **Decisão:**
  - `[x] Aceitar sugestão (Aplicado no commit Onda 1)`

---

### AUD-01-02 · Pressão de GC por Coroutines e LINQ em `TrackBullet`
- **Severidade:** 🟠 Alto
- **Localização no Mod:** [`GameWorldComponent.cs:L61-L107`](../modded/SAIN/Components/GameWorldComponent.cs#L61-L107)
- **Causa Raiz:** O método `RegisterShot` inicia uma coroutine individual (`StartCoroutine(TrackBullet)`) para cada bala disparada no mapa. Dentro da coroutine, é alocada uma nova lista `List<OtherPlayerData>` via LINQ com `from Data in ... where ... select Data.Value`. Em um combate com armas automáticas (600–900 RPM) e vários bots disparando, dezenas de coroutines e iteradores são instanciados por segundo no Heap.
- **Impacto Técnico Real:** Picos intermitentes de coleta de lixo (GC spikes), resultando em micro-congelamentos (*stutters*) perceptíveis pelo jogador no exato momento de trocas de tiro intensas.
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  - Reutilizar um buffer/lista estática reciclada ou manter uma lista centralizada de projéteis ativos atualizada no `GameWorldComponent.ManualUpdate` sem disparar uma coroutine por bala, substituindo o LINQ por um loop `for` indexado sem alocação.
- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar: _________________

---

### AUD-01-03 · Invocação de Reflection no `WorldTickPatch` (60–144 Hz)
- **Severidade:** 🟡 Médio
- **Localização no Mod:** [`WorldTickPatch.cs:L21`](../modded/SAIN/Patches/GameWorld/WorldTickPatch.cs#L21), [`ModDetection.cs:L71-L80`](../modded/SAIN/Plugin/ModDetection.cs#L71-L80)
- **Referência Cruzada:** [`Fika.Core`](../../../references/fika-plugin/Fika.Core/Main/Utils/FikaBackendUtils.cs)
- **Causa Raiz:** A cada execução do tick de mundo do Tarkov (`WorldTickPatch.Patch`), é feita a chamada `ModDetection.FikaInterop.IsClient()`, que por sua vez executa `IsClientProperty.GetValue(null)` via Reflection clássico.
- **Impacto Técnico Real:** Execução de centenas de chamadas de Reflection por segundo na main thread com boxing contínuo de retorno (`bool` como `object`), violando o antipadrão AP-04.
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  - Compilar um `Func<bool>` na inicialização através de `Delegate.CreateDelegate` ou armazenar a flag em cache no carregamento da partida (`AddGameWorldPatch`).
  - *Código Refatorado:*

```csharp
// ModDetection.cs - FikaInterop
private static Func<bool> _isClientDelegate;

public static void InitializeInterop()
{
    // ...
    if (IsClientProperty != null)
    {
        var getMethod = IsClientProperty.GetGetMethod();
        if (getMethod != null)
        {
            _isClientDelegate = (Func<bool>)Delegate.CreateDelegate(typeof(Func<bool>), getMethod);
        }
    }
}

public static bool IsClient()
{
    return _isClientDelegate != null && _isClientDelegate();
}
```

- **Decisão:**
  - `[x] Aceitar sugestão (Aplicado no commit Onda 2)`

---

### AUD-01-04 · Inconsistência de Distância e Vetor em Sobrecarga de `RaycastJob`
- **Severidade:** 🟡 Médio
- **Localização no Mod:** [`RaycastJob.cs:L132-L140`](../modded/SAIN/Types/Jobs/RaycastJob.cs#L132-L140)
- **Causa Raiz:** Na sobrecarga `CreateCommands(int Count, List<Vector3> Points, Vector3 ViewPosition, LayerMask Mask)`, o comando do Unity é construído como `new RaycastCommand(ViewPosition, Direction, new QueryParameters { layerMask = Mask }, 1f)`. A direção não é normalizada (`Direction` em vez de `Direction.normalized`) e o parâmetro de distância é passado como `1f` constante (1 metro), em vez de `Direction.magnitude`.
- **Impacto Técnico Real:** Qualquer job que utilize listas em vez de arrays falha em detectar visibilidade a distâncias superiores a 1 metro ou produz resultados errôneos no raycast multithread.
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  - Harmonizar a implementação com o padrão da sobrecarga de array (linhas 116–130).
  - *Código Refatorado:*

```csharp
private static NativeArray<RaycastCommand> CreateCommands(int Count, List<Vector3> Points, Vector3 ViewPosition, LayerMask Mask)
{
    var Result = new NativeArray<RaycastCommand>(Count, Allocator.TempJob);
    for (int i = 0; i < Count; i++)
    {
        Vector3 Direction = Points[i] - ViewPosition;
        Result[i] = new RaycastCommand(
            ViewPosition,
            Direction.normalized,
            new QueryParameters { layerMask = Mask },
            Direction.magnitude
        );
    }
    return Result;
}
```

- **Decisão:**
  - `[x] Aceitar sugestão (Aplicado no commit Onda 2)`

---

### AUD-01-05 · Varredura Repetitiva de Hierarquia em `findSpawnPointMarkers`
- **Severidade:** 🟡 Médio
- **Localização no Mod:** [`GameWorldComponent.cs:L221-L228`](../modded/SAIN/Components/GameWorldComponent.cs#L221-L228)
- **Causa Raiz:** O método `findSpawnPointMarkers()` é chamado em todo tick de `ManualUpdate` enquanto a câmera principal for nula ou o array não estiver inicializado. `FindObjectsOfType<SpawnPointMarker>()` varre todos os GameObjects da cena ativa.
- **Impacto Técnico Real:** Degradação temporária de performance durante os primeiros segundos de carregamento/início de raid.
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  - Utilizar uma coroutine com throttling (ex.: checar a cada 1.0s) ou disparar a busca apenas no evento `OnGameStarted` / ativação do `BotsController`.
- **Decisão:**
  - `[x] Aceitar sugestão (Aplicado no commit Onda 3)`

---

### AUD-01-06 · Falta de Validação de Nulo em `BotSpawnController.Instance`
- **Severidade:** 🔵 Baixo
- **Localização no Mod:** [`AddBotComponentPatch.cs:L25`](../modded/SAIN/Patches/GameWorld/AddBotComponentPatch.cs#L25)
- **Causa Raiz:** O patch executa `BotSpawnController.Instance.AddBot(__instance)` diretamente. Caso o bot seja pré-ativado antes da instanciação do `GameWorldComponent` (ex.: em instâncias de transição ou Hideout), uma `NullReferenceException` é disparada e capturada pelo bloco `catch`.
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  - Adicionar guarda defensiva `if (BotSpawnController.Instance != null)`.
- **Decisão:**
  - `[x] Aceitar sugestão (Aplicado no commit Onda 3)`

---

### AUD-01-07 · Risco de NullReferenceException em `PlayerComponent.Dispose`
- **Severidade:** 🟡 Médio
- **Localização no Mod:** [`PlayerComponent.cs:L372`](../modded/SAIN/Components/PlayerComponent.cs#L372)
- **Causa Raiz:** Durante o encerramento do componente de jogador (`Dispose`), a linha executa `Player.MovementContext.OnStateChanged -= SoundController.HandleMovementState;` sem operador nulo-seguro no `Player` ou `MovementContext`.
- **Impacto Técnico Real:** Caso o jogador ou seu `MovementContext` já tenha sido destruído pelo motor do jogo durante a transição de mapa ou desconexão abrupta, a chamada dispara uma exceção não tratada, abortando o restante do método `Dispose()` e deixando `Equipment` e `OtherPlayersData` presos na memória.
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  - Proteger a desinscrição com validação defensiva:
  ```csharp
  if (Player?.MovementContext != null && SoundController != null)
  {
      Player.MovementContext.OnStateChanged -= SoundController.HandleMovementState;
  }
  ```
- **Decisão:**
  - `[x] Aceitar sugestão (Aplicado no commit Onda 3)`

---

## 4. Plano de Ação e Recomendações

1. **Correção Imediata de Leak (AUD-01-01):** Aplicar o ajuste em `PlayerSpawnTracker.cs` para remover bots de `AlivePlayerArray` no momento da morte e limpar a lista no teardown.
2. **Otimização de Hot Path (AUD-01-03):** Substituir a chamada via Reflection por delegado direto em `ModDetection.FikaInterop`.
3. **Correção de Física (AUD-01-04):** Corrigir a distância e normalização em `RaycastJob.cs`.
4. **Refatoração de Projéteis (AUD-01-02):** Planejar um item de backlog para eliminar coroutines individuais em tiros e centralizar o processamento de balas no `GameWorldComponent`.
