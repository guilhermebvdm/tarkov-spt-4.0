---
title: "ORBIT — Sistema de Objetivos e Metas"
date: 2026-08-22
status: 🟢 Vivo
authors: Antigravity
---

# ORBIT — Sistema de Objetivos e Metas (*Main Objectives*)

O sistema de **Objetivos Principais** (*Main Objectives*) é o coração do ORBIT. Ele garante que cada esquadrão de bots gerado no mapa possua uma lista ordenada de propósitos para cumprir durante a raid, simulando o comportamento de jogadores humanos que entram na partida com metas claras (fazer quests, caçar PvP ou encher a mochila de loot valioso).

---

## 1. Quem Recebe Objetivos?

- **PMCs:** Recebem uma lista de 1 a 5 metas (definidas pelo arquétipo do **SAIN**).
- **PlayerScavs:** Recebem objetivos configuráveis (com forte foco em `LootValue` e sobrevivência).
- **Bots Scavs Regulares / Bosses / Raiders:** Não recebem objetivos principais; permanecem operando pelo sistema de advecção/patrulha local, a menos que tenham opções de roaming ativadas.

---

## 2. Os 3 Tipos de Objetivos

```mermaid
graph LR
    subgraph Tipos_de_Metas [Tipos de Objetivos Principais]
        Q["1. Quest (Missão)<br>Gatilhos reais de quest do EFT"]
        K["2. Kills (PvP/Caça)<br>Hotspots com atração de combate"]
        L["3. LootValue (Valor Financeiro)<br>Células mais ricas do mapa"]
    end
```

### 1. `Quest` (Missões do EFT)
- **Origem:** Extraído dinamicamente dos gatilhos `TriggerWithId` do mapa (pontos onde jogadores precisam pegar itens, plantar marcadores ou investigar áreas).
- **Comportamento:** O esquadrão traça rota até o POI da missão. Ao chegar, o líder e os membros cobrem a área por um período de guarda (*POI Guard Duration*) e podem inspecionar contêineres próximos.

### 2. `Kills` (Caça em Hotspots PvP)
- **Origem:** Âncoras extraídas das zonas de advecção com força positiva (*Positive-Force Zones*), que representam áreas centrais de conflito no mapa (Dormitórios em Customs, Resort em Shoreline, D-2 em Reserve, etc.).
- **Comportamento:** O esquadrão ruma para o hotspot e entra em modo de patrulha agressiva (*Kills Roam*), espalhando-se em um raio de dispersão (`Roam splinter radius`) por um tempo determinado (`KillsRoamDuration`) à procura de alvos.

### 3. `LootValue` (Saque de Alta Densidade)
- **Origem:** O mapa é discretizado em uma grade de células 2D. O sistema calcula a densidade de valor de cada célula com base nos contêineres e itens soltos presentes.
- **Comportamento:** O esquadrão navega até a célula rica escolhida e inicia uma limpeza metódica sala a sala.
- **Timeout de Segurança:** Possui um temporizador de limite (`LootValue timeout`, padrão 300s) para evitar que o esquadrão fique indefinidamente preso em uma mesma sala se o loot acabar ou for inacessível.

---

## 3. Geração e Normalização de Pesos

Ao instanciar o esquadrão, a classe [MainObjectiveBuilder.cs](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/ORBIT/original/Orbit/Sain/MainObjectiveBuilder.cs) executa o seguinte fluxo:

```mermaid
flowchart TD
    Start([Novo Esquadrão Spawna]) --> CheckRole{É PMC ou PlayerScav?}
    CheckRole -- Não --> Skip([Ignora Objetivos Principais])
    CheckRole -- Sim --> ResolveMix[Obtém Pesos de Quest, Kills e LootValue do SAIN]
    ResolveMix --> Normalize[Normaliza Pesos: Total = 100%]
    Normalize --> RollCount[Sorteia Quantidade de Objetivos: N = min..max]
    
    RollCount --> Loop[Para cada slot de objetivo 1..N]
    Loop --> PickType{Sorteio com base no Mix}
    PickType -- Quest --> RollQ[Sorteia POI de Missão do Pool]
    PickType -- Kills --> RollK[Sorteia Hotspot PvP do Mapa]
    PickType -- LootValue --> RollL[Sorteia Célula no Top-N de Riqueza]
    
    RollQ --> DedupCheck{Célula já usada por este Esquadrão?}
    RollK --> DedupCheck
    RollL --> DedupCheck
    
    DedupCheck -- Colisão Interna --> Retry[Re-sorteia até 6 tentativas]
    DedupCheck -- Única --> AddObj[Adiciona à Lista MainObjectives]
    Retry --> AddObj
    
    AddObj --> LoopDone{Todos slots gerados?}
    LoopDone -- Não --> Loop
    LoopDone -- Sim --> Finish([Inicia Execução na Rota])
```

---

## 4. Gestão de Andares e Deslocamento Vertical

Para evitar que os bots fiquem oscilando em escadas ou correndo de cima para baixo sem limpar salas:

- **`SameFloorLootYTolerance` (Padrão 2.5m):** Qualquer item ou contêiner dentro de ±2.5 metros verticais é considerado no "mesmo andar", priorizando a limpeza completa do piso atual.
- **`CrossFloorSplinterChance` (Padrão 10%):** Probabilidade controlada de um membro do esquadrão decidir investigar o andar superior ou inferior enquanto o líder limpa o andar atual, gerando transições orgânicas entre pavimentos.

---

## 5. Máquina de Estados da Estratégia de Objetivos

A execução das metas é coordenada por [GotoObjectiveStrategy.cs](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/ORBIT/original/Orbit/Tasks/Strategies/GotoObjectiveStrategy.cs):

```mermaid
stateDiagram-v2
    [*] --> Navegando_Ao_Objetivo: Define rota via WaypointSystem
    Navegando_Ao_Objetivo --> Sob_Fogo: Inimigo Avistado
    Sob_Fogo --> Navegando_Ao_Objetivo: Fim do Combate (SAIN cede controle)
    
    Navegando_Ao_Objetivo --> No_Objetivo: Esquadrão chega ao raio da meta
    
    state No_Objetivo {
        [*] --> Saqueando_Contêineres: Se LootValue
        [*] --> Patrulhando_PvP: Se Kills
        [*] --> Guardando_Area: Se Quest
    }
    
    No_Objetivo --> Meta_Concluida: Tempo esgotado / Célula limpa / Quest visitada
    Meta_Concluida --> Proxima_Meta: Restam metas na lista
    Proxima_Meta --> Navegando_Ao_Objetivo
    
    Meta_Concluida --> Requisitar_Extracao: Todas as metas foram concluídas!
    Requisitar_Extracao --> [*]
```

Quando a opção `Extract when all mains done` está ativada, a conclusão do último objetivo principal da lista faz com que o esquadrão solicite imediatamente rota de fuga para a extração mais próxima.
