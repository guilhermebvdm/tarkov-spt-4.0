---
title: "Relatório de Auditoria Técnica de Código — SAIN (Parte 3: Máquina de Decisão, Camadas e Esquadrões)"
date: 2026-08-31
status: 🟢 Vivo
authors: Antigravity
---

# Relatório de Auditoria Técnica de Código — SAIN (Parte 3: Máquina de Decisão, Camadas e Esquadrões)

Auditoria estática e comportamental profunda focada no **Motor de Decisão (`BotDecisionManager`)**, **Camadas e Ações BigBrain (`Layers/`)**, **Coordenação de Esquadrões (`SquadDecisionClass`, `BotSquads`)** e **Comunicação Tática (`GroupTalk`)**.

---

## 1. Resumo Executivo da Auditoria

| Severidade | Quantidade | Descrição |
|---|---|---|
| 🔴 **Crítico** | 1 | Crash imediato (`NullReferenceException`) no ataque melee do Boss Tagilla contra jogadores humanos |
| 🟠 **Alto** | 1 | Risco de NRE em cadeia na tomada de decisão de suporte e supressão de esquadrão (`SquadDecisionClass`) |
| 🟡 **Médio** | 3 | Retenção de referências de esquadrões no teardown (`BotSquads`), mutação/alocação de ordenação in-place em `DogFightDecisionClass` e NRE em `EnemyTalk.Dispose` |
| 🔵 **Baixo** | 2 | Acesso não-defensivo em `GroupTalk` e alocação de reflexão/dicionário repetida em `SAINBotTalkClass` |
| 💡 **Otimização** | 2 | Busca linear de menor distância sem reordenação in-place e cache estático de dicionário de falas |

---

## 2. Tabela de Achados

| ID | Severidade | Arquivo / Linha | Categoria | Descrição Resumida |
|---|---|---|---|---|
| `AUD-03-01` | 🔴 Crítico | [`BotDecisionManager.cs:L84`](../modded/SAIN/Classes/Bot/Decision/BotDecisionManager.cs#L84), [`L89`](../modded/SAIN/Classes/Bot/Decision/BotDecisionManager.cs#L89), [`L94`](../modded/SAIN/Classes/Bot/Decision/BotDecisionManager.cs#L94) | Crash / NRE | `enemy.BotOwner.WeaponManager.Melee` lança `NullReferenceException` quando Tagilla persegue o player humano (`enemy.BotOwner == null`). |
| `AUD-03-02` | 🟠 Alto | [`SquadDecisionClass.cs:L83`](../modded/SAIN/Classes/Bot/Decision/SquadDecisionClass.cs#L83) | NRE Defensivo | `member.GoalEnemy.EnemyPlayer` assume que `GoalEnemy` é não-nulo para todos os membros que possuem inimigo ativo. |
| `AUD-03-03` | 🟡 Médio | [`BotSquads.cs:L10-L42`](../modded/SAIN/Classes/BotManager/BotSquads.cs#L10-L42) | Memory Leak / Teardown | `BotSquads` não limpa `Squads` nem `SquadArray` no `Dispose()`, retendo dados de esquadrão após o fim da raid. |
| `AUD-03-04` | 🟡 Médio | [`DogFightDecisionClass.cs:L86`](../modded/SAIN/Classes/Bot/Decision/DogFightDecisionClass.cs#L86) | Efeito Colateral / GC | `KnownEnemies.Sort(...)` altera a ordem da lista compartilhada de inimigos conhecidos e aloca delegates no Heap a cada 0.5s. |
| `AUD-03-05` | 🔵 Baixo | [`GroupTalk.cs:L233`](../modded/SAIN/Classes/Bot/Talk/GroupTalk.cs#L233) | NRE Defensivo | Invocação direta `Player.HandsController.ShowGesture` sem operador nulo-seguro durante transições de mãos. |
| `AUD-03-06` | 🟡 Médio | [`EnemyTalkClass.cs:L101`](../modded/SAIN/Classes/Bot/Talk/EnemyTalkClass.cs#L101) | AP-02 Singleton Inseguro | `BotManagerComponent.Instance.BotHearing.PlayerTalk -= ...` sem proteção contra `Instance` nula no teardown de cena. |
| `AUD-03-07` | 🔵 Baixo | [`SAINBotTalkClass.cs:L27`](../modded/SAIN/Classes/Bot/Talk/SAINBotTalkClass.cs#L27), [`L451`](../modded/SAIN/Classes/Bot/Talk/SAINBotTalkClass.cs#L451) | Reflection / Heap Churn | Construtor do componente de fala reexecuta `Enum.GetValues(typeof(EPhraseTrigger))` e instancia 60 entradas por bot em vez de usar dicionário estático compartilhado. |

---

## 3. Detalhamento dos Achados

### AUD-03-01 · Crash por `NullReferenceException` no Ataque de Martelo do Tagilla
- **Severidade:** 🔴 Crítico
- **Localização no Mod:** [`BotDecisionManager.cs:L84`](../modded/SAIN/Classes/Bot/Decision/BotDecisionManager.cs#L84), [`BotDecisionManager.cs:L89`](../modded/SAIN/Classes/Bot/Decision/BotDecisionManager.cs#L89), [`BotDecisionManager.cs:L94`](../modded/SAIN/Classes/Bot/Decision/BotDecisionManager.cs#L94)
- **Causa Raiz:** No método `shallTagillaHammerAttack(Enemy enemy)`, o código tenta gerenciar a flag de ataque melee do Tagilla chamando:
  `enemy.BotOwner.WeaponManager.Melee.ShallEndRun = false;`
  No entanto, `enemy` representa o alvo de Tagilla. Se o alvo for o jogador humano (`Player`), `enemy.BotOwner` é **sempre nulo**, pois jogadores humanos não são controlados por `BotOwner`.
- **Impacto Técnico Real:** No momento exato em que Tagilla decide investir contra o jogador humano para golpear com o martelo, uma `NullReferenceException` fatal é disparada, interrompendo o ciclo de decisão da IA. A intenção original era modificar o gerenciador de armas do próprio Tagilla (`Bot.BotOwner.WeaponManager.Melee.ShallEndRun`).
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  - Corrigir a referência para o `Bot.BotOwner` do próprio bot (Tagilla).
  - *Código Refatorado:*

```csharp
private bool shallTagillaHammerAttack(Enemy enemy)
{
    if (enemy == null) return false;
    bool alreadyAttacking = CurrentCombatDecision == ECombatDecision.MeleeAttack;
    ETagStatus status = Bot.Memory.Health.HealthStatus;

    if (!alreadyAttacking)
    {
        if (CurrentSelfDecision != ESelfActionType.None) return false;
        if (status != ETagStatus.Healthy && status != ETagStatus.Injured) return false;
        if (enemy.Path.PathToEnemyStatus != UnityEngine.AI.NavMeshPathStatus.PathComplete) return false;

        if (enemy.RealDistance < 35 && enemy.Path.PathLength < 30 && enemy.Status.VulnerableAction != EEnemyAction.None)
        {
            Bot.BotOwner.WeaponManager.Melee.ShallEndRun = false; // CORREÇÃO: Bot do Tagilla, não o Enemy
            return true;
        }
        if (enemy.RealDistance < 20 && enemy.Path.PathLength < 15)
        {
            Bot.BotOwner.WeaponManager.Melee.ShallEndRun = false; // CORREÇÃO: Bot do Tagilla
            return true;
        }
        return false;
    }
    if (Bot.BotOwner.WeaponManager.Melee.ShallEndRun) // CORREÇÃO: Bot do Tagilla
    {
        return false;
    }
    if (status != ETagStatus.Dying && enemy.RealDistance < 40 && enemy.Path.PathLength < 35)
    {
        return true;
    }
    return false;
}
```

- **Decisão:**
  - `[x] Aceitar sugestão (Aplicado no commit Onda 1)`

---

### AUD-03-02 · Risco de NRE em Tomada de Decisão de Suporte de Esquadrão
- **Severidade:** 🟠 Alto
- **Localização no Mod:** [`SquadDecisionClass.cs:L81-L85`](../modded/SAIN/Classes/Bot/Decision/SquadDecisionClass.cs#L81-L85)
- **Causa Raiz:** Em `EnemyDecision`, ao verificar se aliados de esquadrão compartilham o mesmo alvo para supressão/ajuda:
  `if (myEnemy != null && member.HasEnemy)`
  `if (myEnemy.EnemyPlayer == member.GoalEnemy.EnemyPlayer)`
  A propriedade `member.HasEnemy` verifica se existe qualquer inimigo rastreado (`Bot.Enemy != null || Bot.GoalEnemy != null`), mas `member.GoalEnemy` pode ser temporariamente nulo antes da seleção de alvo do frame.
- **Impacto Técnico Real:** Disparo de `NullReferenceException` durante o loop de decisão de esquadrão quando um bot aliado ainda está elegendo seu `GoalEnemy`.
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  - Utilizar verificação segura com elvis operator (`member.GoalEnemy?.EnemyPlayer`).
  - *Código Refatorado:*

```csharp
if (myEnemy != null && member.HasEnemy)
{
    var memberGoalEnemy = member.GoalEnemy;
    if (memberGoalEnemy != null && myEnemy.EnemyPlayer == memberGoalEnemy.EnemyPlayer)
    {
        if (shallSuppressEnemy(member))
        {
            Decision = ESquadDecision.Suppress;
            return true;
        }
        if (shallHelp(member))
        {
            Decision = ESquadDecision.Help;
            return true;
        }
    }
}
```

- **Decisão:**
  - `[x] Aceitar sugestão (Aplicado no commit Onda 3)`

---

### AUD-03-03 · Falta de Limpeza de Esquadrões no Teardown (`BotSquads`)
- **Severidade:** 🟡 Médio
- **Localização no Mod:** [`BotSquads.cs:L10-L42`](../modded/SAIN/Classes/BotManager/BotSquads.cs#L10-L42)
- **Causa Raiz:** `BotSquads` mantém instâncias ativas de esquadrão em `Squads` e `SquadArray`, mas não sobrescreve o método `Dispose()` para desregistrar eventos (`OnSquadEmpty`) e esvaziar os containers.
- **Impacto Técnico Real:** Manutenção residual de ponteiros para objetos `BotComponent` e dados de raid anterior.
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  - Implementar `Dispose()` em `BotSquads`.
  - *Código Refatorado:*

```csharp
public override void Dispose()
{
    foreach (var squad in SquadArray)
    {
        if (squad != null)
        {
            squad.OnSquadEmpty -= RemoveSquad;
            squad.Dispose();
        }
    }
    Squads.Clear();
    SquadArray.Clear();
    _squadsToRemove.Clear();
    base.Dispose();
}
```

- **Decisão:**
  - `[x] Aceitar sugestão (Aplicado no commit Onda 3)`

---

### AUD-03-04 · Mutação In-Place de Coleção e Alocação em `DogFightDecisionClass`
- **Severidade:** 🟡 Médio
- **Localização no Mod:** [`DogFightDecisionClass.cs:L86`](../modded/SAIN/Classes/Bot/Decision/DogFightDecisionClass.cs#L86)
- **Causa Raiz:** `KnownEnemies.Sort((x, y) => x.Path.PathLength.CompareTo(y.Path.PathLength))` reordena diretamente a lista viva do `EnemyController` e aloca um delegate/closure a cada 0.5 segundos por bot em combate próximo.
- **Impacto Técnico Real:** Efeito colateral indesejado em outros sistemas que dependem da ordem cronológica ou de prioridade de `KnownEnemies`, além de alocações recorrentes no Heap.
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  - Realizar uma varredura linear para encontrar o inimigo com menor `PathLength` sem alterar a coleção.
  - *Código Refatorado:*

```csharp
if (_changeDFTargetTime < Time.time)
{
    _changeDFTargetTime = Time.time + 0.5f;
    Enemy closestEnemy = null;
    float shortestPath = float.MaxValue;

    for (int i = 0; i < KnownEnemies.Count; i++)
    {
        Enemy enemy = KnownEnemies[i];
        if (enemy != null && ShallDogfightEnemy(enemy))
        {
            float len = enemy.Path.PathLength;
            if (len < shortestPath)
            {
                shortestPath = len;
                closestEnemy = enemy;
            }
        }
    }

    if (closestEnemy != null)
    {
        _lastDogFightTarget = closestEnemy;
        result = closestEnemy;
        return true;
    }
}
```

- **Decisão:**
  - `[x] Aceitar sugestão (Aplicado no commit Onda 2)`

---

### AUD-03-05 · Invocação Insegura de Gesto de Mão em `GroupTalk`
- **Severidade:** 🔵 Baixo
- **Localização no Mod:** [`GroupTalk.cs:L233`](../modded/SAIN/Classes/Bot/Talk/GroupTalk.cs#L233)
- **Causa Raiz:** `Player.HandsController.ShowGesture(gesture);` sem checagem defensiva de nulo.
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  - Substituir por `Player.HandsController?.ShowGesture(gesture);`.
- **Decisão:**
  - `[x] Aceitar sugestão (Aplicado no commit Onda 3)`

---

### AUD-03-06 · Risco de NullReferenceException em `EnemyTalk.Dispose`
- **Severidade:** 🟡 Médio
- **Localização no Mod:** [`EnemyTalkClass.cs:L30`](../modded/SAIN/Classes/Bot/Talk/EnemyTalkClass.cs#L30), [`EnemyTalkClass.cs:L101`](../modded/SAIN/Classes/Bot/Talk/EnemyTalkClass.cs#L101)
- **Causa Raiz:** `BotManagerComponent.Instance.BotHearing.PlayerTalk -= playerTalked;` acessa o singleton `BotManagerComponent.Instance` diretamente no `Dispose()`. Se o `BotManagerComponent` for destruído primeiro durante o encerramento da partida ou descarga de cena, `Instance` torna-se `null`, disparando uma exceção não tratada que aborta o restante da rotina de limpeza.
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  - Adicionar proteção com operador nulo-seguro:
  ```csharp
  if (BotManagerComponent.Instance?.BotHearing != null)
  {
      BotManagerComponent.Instance.BotHearing.PlayerTalk -= playerTalked;
  }
  ```
- **Decisão:**
  - `[x] Aceitar sugestão (Aplicado no commit Onda 3)`

---

### AUD-03-07 · Instanciação Redundante de Dicionário de Frases por Bot via Reflection
- **Severidade:** 🔵 Baixo
- **Localização no Mod:** [`SAINBotTalkClass.cs:L27`](../modded/SAIN/Classes/Bot/Talk/SAINBotTalkClass.cs#L27), [`SAINBotTalkClass.cs:L451-L455`](../modded/SAIN/Classes/Bot/Talk/SAINBotTalkClass.cs#L451-L455)
- **Causa Raiz:** Cada nova instância de `SAINBotTalkClass` aloca seu próprio dicionário `_phraseDictionary` e executa `System.Enum.GetValues(typeof(EPhraseTrigger))` para registrar 60 entradas de `PhraseInfo`. Como as prioridades e delays padrão são estáticos e universais, executar essa reflexão e alocação no construtor de cada bot gera sobrecarga desnecessária na inicialização de spawns.
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  - Compartilhar o mapa padrão em um `static readonly Dictionary<EPhraseTrigger, PhraseInfo>` estático, instanciado uma única vez na inicialização do plugin.
- **Decisão:**
  - `[x] Aceitar sugestão (Aplicado no commit Onda 2)`

---

## 4. Plano de Ação e Recomendações

1. **Correção Imediata do Tagilla (AUD-03-01):** Ajustar `shallTagillaHammerAttack` para referenciar o `BotOwner` do próprio bot, prevenindo crash imediato no combate corpo a corpo contra o jogador.
2. **Guarda Defensiva de Esquadrão (AUD-03-02):** Proteger o acesso a `member.GoalEnemy.EnemyPlayer`.
3. **Limpeza de Recursos (AUD-03-03):** Adicionar rotina de `Dispose()` em `BotSquads`.
4. **Otimização de Algoritmo de Dogfight (AUD-03-04):** Substituir o `.Sort()` por varredura linear simples.
