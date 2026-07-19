---
name: tarkov-pmc-spawning
description: Como instanciar e spawnar PMCs de forma confiável via injeção direta no SPT 4.0, evitando travas de fila.
---

# Spawn de PMCs e Bots Comuns no SPT 4.0

Este documento atua como memória persistente para futuros agentes de como realizar o spawn de bots (principalmente PMCs) contornando as filas de geração assíncrona do SPT.

## 1. O Problema das Filas do SPT
O SPT utiliza uma fila interna para processar e gerar os perfis de bots no decorrer da raid. Tentar colocar PMCs nesta fila usando os métodos clássicos de waves pode falhar ou demorar, especialmente em raids já iniciadas.

## 2. Solução: Injeção Direta
Para spawnar um bot de forma instantânea e direta em uma zona específica (BotZone), a abordagem correta é:

### Passo A: Criar a Fila de Perfis (BotProfileDataClass)
É fundamental instanciar o `BotProfileDataClass` com as informações de facção e classe adequadas. 

> [!IMPORTANT]
> **Facção (Side)**: Para PMCs, utilize `EPlayerSide.Usec` ou `EPlayerSide.Bear` (ou o `Profile.Side` original do bot). **Nunca** fixe `EPlayerSide.Savage` se a `role` for de PMC, pois isso causará conflito interno no gerador de perfis do SPT e a geração retornará nula.

```csharp
BotSpawnParams spawnParams = new BotSpawnParams();
BotProfileDataClass profileData = new BotProfileDataClass(side, role, difficulty, 0f, spawnParams);
```

### Passo B: Gerar os Dados do Bot (BotCreationDataClass)
Crie os dados de criação de forma assíncrona usando o `BotCreator` (`_botCreator`):
```csharp
var task = BotCreationDataClass.Create(profileData, botCreator, groupSize, botSpawner);
// Em Corrotinas, espere a Task completar:
while (!task.IsCompleted) yield return null;
var creationData = task.Result;
```

### Passo C: Spawner Direto na Zona
Com o `creationData` em mãos, chame o spawner na zona desejada:
```csharp
botSpawner.TryToSpawnInZoneAndDelay(botZone, creationData, false, true, null, false);
```
Isso instanciará o bot fisicamente na raid sem passar pelo loop regular de waves do SPT.
