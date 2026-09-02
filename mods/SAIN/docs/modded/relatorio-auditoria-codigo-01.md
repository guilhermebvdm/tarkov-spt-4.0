---
title: "SAIN — Relatório de Auditoria Técnica de Código (Parte 1)"
date: 2026-09-01
status: 🟢 Vivo
authors: Antigravity
---

# SAIN — Relatório de Auditoria Técnica de Código (Parte 1)
**Domínio:** Ciclo de Vida de Raid, Gestão de Memória / Leaks, Patches Globais e Interoperabilidade Client-Server  
**Escopo Auditado:** `mods/SAIN/modded/SAIN/` (`SAINPlugin.cs`, `Components/`, `Patches/GameWorld/`, `Plugin/ModDetection.cs`, `Classes/PlayerManager/Players/PlayerSpawnTracker.cs`, `Classes/BotManager/Jobs/`)  
**Versão Alvo:** SPT 4.0.13 / Escape From Tarkov 0.16.9 / SAIN v4.5.0  

---

## 1. Sumário Executivo

Esta auditoria representa a **segunda rodada de verificação profunda** sobre a base de código do SAIN v4.5.0, focando no ciclo de vida de raid, controle de memória, supressão de coletas de lixo (*GC pressure*) e integridade de multithreading.

| Dimensão Técnica | Avaliação | Diagnóstico Sintético |
|---|:---:|---|
| **1. Validação em references/** | 🟢 Excelente | Compatibilidade total com EFT 0.16.9, SPT 4.0 e FIKA coop sem dependência de reflection em hot paths. |
| **2. Update() vs Lógica Otimizada** | 🟢 Conforme | Throttling ativo em spawn markers e atualizações de cache com frequência desacoplada. |
| **3. Leaks de Memória & GC** | 🟡 Atenção | Mutação com `.Sort()` e alocações de lambda em `PlayerSpawnTracker.FindClosestHumanPlayer` e LINQ no rastreio de projéteis. |
| **4. Funções Órfãs & Código Morto** | 🟢 Limpo | Métodos e patches inativos removidos na rodada anterior. |
| **5. Antipadrões SPT (AP-01..09)** | 🟢 Conforme | Singletons seguros com desinscrições no `GameWorld.OnDispose`. |
| **6. Threading & Unity Jobs** | 🟢 Excelente | Jobs paralelos de lanterna e caminhos NavMesh sincronizados corretamente com a main thread. |

---

## 2. Panorama das 6 Dimensões de Auditoria

```mermaid
graph LR
    subgraph Dimensoes_Auditadas [Auditoria de Código - Parte 1]
        D1["1. Assinaturas EFT & FIKA: 100% OK"]
        D2["2. Loops e Frequência: 100% OK"]
        D3["3. GC Pressure & List Mutations: 2 Apontamentos"]
        D4["4. Código Morto: 100% OK"]
        D5["5. Antipadrões SPT: 100% OK"]
        D6["6. Unity Jobs & Multithreading: 1 Apontamento Menor"]
    end
```

---

## 3. Achados Técnicos Detalhados

### AUD-01-01 · Mutação de Coleção e Alocação de Lambda em `FindClosestHumanPlayer`
- **Severidade:** 🟡 Médio
- **Localização no Mod:** [`PlayerSpawnTracker.cs:L60-L77`](../modded/SAIN/Classes/PlayerManager/Players/PlayerSpawnTracker.cs#L60-L77)
- **Causa Raiz:** O método `FindClosestHumanPlayer(out float distance, PlayerComponent quierrier, out Player player)` executa uma ordenação in-place na lista compartilhada `quierrier.OtherPlayersData.DataList` utilizando um delegado anônimo (`otherPlayers.Sort((x, y) => x.DistanceData.Distance.CompareTo(y.DistanceData.Distance))`).
- **Impacto Concreto:**
  1. A cada pulso do `SAINAILimit.CheckAILimit()` de cada bot ativo, a lista de jogadores é reordenada com complexidade $O(N \log N)$ e aloca um closure no Heap.
  2. A ordenação muta a lista interna de `OtherPlayersData`, podendo interferir em outros iteradores concorrentes.
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  - Substituir a ordenação por uma única varredura linear $O(N)$ com acumulador de menor distância, sem alocações e sem mutar a lista:

```csharp
public PlayerComponent FindClosestHumanPlayer(out float distance, PlayerComponent quierrier, out Player player)
{
    List<OtherPlayerData> otherPlayers = quierrier.OtherPlayersData.DataList;
    PlayerComponent closestHuman = null;
    float minDistance = float.MaxValue;
    player = null;

    for (int i = 0; i < otherPlayers.Count; i++)
    {
        OtherPlayerData otherPlayer = otherPlayers[i];
        if (otherPlayer != null && !otherPlayer.OtherPlayerComponent.IsAI)
        {
            float dist = otherPlayer.DistanceData.Distance;
            if (dist < minDistance)
            {
                minDistance = dist;
                closestHuman = otherPlayer.OtherPlayerComponent;
                player = otherPlayer.OtherPlayerComponent.Player;
            }
        }
    }

    distance = minDistance;
    return closestHuman;
}
```

- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar: _________________

---

### AUD-01-02 · Alocação de LINQ em Corrotina de Rastreio de Projéteis (`TrackBullet`)
- **Severidade:** 🟡 Médio
- **Localização no Mod:** [`GameWorldComponent.cs:L61-L75`](../modded/SAIN/Components/GameWorldComponent.cs#L61-L75)
- **Causa Raiz:** A corrotina `TrackBullet` inicializa `PlayersToCheck` através de uma consulta LINQ com cláusulas `from ... let ... where ... select` a cada disparo registrado no mundo.
- **Impacto Concreto:** Em tiroteios com armas automáticas (20 a 50 tiros/segundo), dezenas de iteradores e closures LINQ são instanciados no Heap, gerando picos periódicos de coleta de lixo.
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  - Substituir o LINQ por um laço `foreach` direto preenchendo a lista:

```csharp
private IEnumerator TrackBullet(PlayerComponent Player, EftBulletClass Bullet)
{
    var OtherPlayerData = Player.OtherPlayersData.DataDictionary;
    Vector3 PlayerLookDir = Player.LookDirection;

    List<OtherPlayerData> PlayersToCheck = new List<OtherPlayerData>(OtherPlayerData.Count);
    foreach (var kvp in OtherPlayerData)
    {
        var data = kvp.Value;
        var otherComp = data?.OtherPlayerComponent;
        if (otherComp?.IsAI == true && otherComp.IsActive)
        {
            if (Vector3.Dot(data.DistanceData.DirectionNormal, PlayerLookDir) > 0.75f)
            {
                PlayersToCheck.Add(data);
            }
        }
    }
    // ... restante da corrotina inalterada
```

- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar: _________________

---

### AUD-01-03 · Alocação de Delegado em Ordenação de Eventos Sonoros (`AddCachedAISoundEvent`)
- **Severidade:** 🔵 Baixo
- **Localização no Mod:** [`PlayerComponent.cs:L203-L226`](../modded/SAIN/Components/PlayerComponent.cs#L203-L226)
- **Causa Raiz:** Quando `AISoundCachedEvents` atinge `MaxCachedSounds` (4 elementos), o método insere o novo evento e executa `AISoundCachedEvents.Sort((a, b) => b.BaseRangeWithVolume.CompareTo(a.BaseRangeWithVolume))` com remoção do último índice.
- **Impacto Concreto:** O delegate `(a, b) => ...` é alocado no Heap a cada som registrado por jogador/bot sob capacidade máxima de cache.
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  - Localizar o índice do elemento com menor `BaseRangeWithVolume` em um laço simples de 4 iterações e substituí-lo diretamente se o novo som for mais alto, sem usar `.Sort()`:

```csharp
int minIndex = 0;
float minRange = AISoundCachedEvents[0].BaseRangeWithVolume;
for (int i = 1; i < Count; i++)
{
    if (AISoundCachedEvents[i].BaseRangeWithVolume < minRange)
    {
        minRange = AISoundCachedEvents[i].BaseRangeWithVolume;
        minIndex = i;
    }
}
if (BaseRange > minRange)
{
    AISoundCachedEvents[minIndex] = new(InSoundType, InPosition, this, InRange, InVolume, SoundSpeed, Phrase, TagStatus);
}
```

- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar: _________________

---

### AUD-01-04 · Esvaziamento Explícito de Lista no `JobManager.Dispose()`
- **Severidade:** 🔵 Baixo
- **Localização no Mod:** [`JobManager.cs:L33-L39`](../modded/SAIN/Classes/BotManager/Jobs/JobManager.cs#L33-L39)
- **Causa Raiz:** O método `Dispose()` invoca `job?.Stop();` em cada item, mas não limpa a lista `Jobs.Clear()`.
- **Impacto Concreto:** Risco de retenção de referências aos objetos de job se o `JobManager` for mantido em instâncias persistentes.
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  - Adicionar `Jobs.Clear();` no final de `Dispose()`.

- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar: _________________

---

## 4. Plano de Ação e Recomendações

1. **Otimização de Varredura de Proximidade (AUD-01-01):** Migrar `FindClosestHumanPlayer` para busca linear $O(N)$ sem mutar a lista `DataList`.
2. **Eliminação de LINQ em Disparos (AUD-01-02):** Trocar a query LINQ em `TrackBullet` por preenchimento iterativo com capacidade pré-dimensionada.
3. **Cache de Áudio Zero-Alloc (AUD-01-03):** Substituir ordenação de 4 itens por substituição direta do menor valor.
4. **Limpeza de Jobs (AUD-01-04):** Adicionar `Jobs.Clear()` no `JobManager.Dispose()`.
