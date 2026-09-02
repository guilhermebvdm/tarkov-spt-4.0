---
title: SAIN — Arquitetura de Otimização Multithread e LOD de IA
date: 2026-09-02
status: 🟢 Vivo
authors: [guilhermebvdm, Antigravity]
---

# ⚡ SAIN — Arquitetura de Otimização Multithread e LOD de IA

Este documento especifica a engenharia de alta performance, a redistribuição de processamento multithread e o sistema de LOD de IA adaptativo.

> [!IMPORTANT]
> **Isolamento de Escopo e Versão de Aplicação:**  
> Todos os pontos, estruturas e jobs descritos neste documento serão implementados e validados **exclusivamente nesta versão [`mods/SAIN/modded-multithread/`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/SAIN/modded-multithread/)**.  
> A versão estável de produção ([`mods/SAIN/modded/`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/SAIN/modded/)) permanece intocada como baseline de referência estável.

---

## 🏛️ 1. Princípio da Caixa Preta (Compatibilidade Estrita de API Pública)

O maior requisito de sustentabilidade deste projeto é **NÃO quebrar nenhum mod de terceiro** (*QuestingBots*, *LootingBots*, *FIKA*, mods de UI/HUD, *TRL*, etc.) que dependa ou leia dados do SAIN.

```mermaid
graph LR
    subgraph Ecossistema [Mods Terceiros: QuestingBots, LootingBots, FIKA]
        A["Leitura de bot.Cover.CurrentCoverPoint"]
        B["Leitura de bot.Decision.CurrentSoloDecision"]
        C["Leitura de bot.Enemy.EnemyPlayerData"]
        D["Leitura de bot.Enemy.Path.AllPathNodes"]
    end
    
    subgraph MotorSAIN [SAIN modded-multithread (Motor Interno)]
        E["CoverEvaluationJob (IJobParallelFor em 8-16 núcleos)"]
        F["Adaptive Distance AI LOD (Tick Rate Estrangulado)"]
        G["DirectionDataJob & EnemyPlaceJob Paralelos"]
        H["Persistent Zero-Alloc NativeArray Pools"]
    end
    
    subgraph ContratoPublico [API Pública Intacta (Main Thread)]
        I["CustomCoverPoint (Mesmas variáveis e assinaturas)"]
        J["CurrentSoloDecision (Acessível a qualquer frame)"]
        K["OtherPlayerData.DistanceData (Mesmos getters)"]
        L["BotVisiblePathNode[] (Array populado intacto)"]
    end
    
    A --> I
    B --> J
    C --> K
    D --> L
    
    E -->|Escreve Resultado Vencedor| I
    F -->|Atualiza Estado| J
    G -->|Entrega Vetores Prontos| K
    H -->|Popula Nós de Rota| L
```

### Regras Mandatórias de Preservação:
1. **Zero Renomeação ou Remoção:** Nenhuma classe, interface, enum, propriedade pública, método público ou evento é removido ou alterado.
2. **Encapsulamento de Cálculos:** O trabalho pesado de geometria, produto escalar, amostragem e raycasting roda em segundo plano em estruturas internas (`structs` puras / `NativeArray`), mas entrega o resultado final nas propriedades públicas existentes na Main Thread.
3. **Validação Automatizada por AST:** Toda compilação é previamente auditada pelo script de comparação de AST (`compare_api.js`) garantindo `0` quebras de contrato.

---

## 🚀 2. Catálogo dos 10 Pontos Críticos de Otimização Multithread & Zero-Alloc

Abaixo estão detalhados os **10 subsistemas mapeados** que serão refatorados na versão `modded-multithread`:

### Ponto 1: Avaliação e Pontuação de Coberturas em Paralelo
* **Componentes-Alvo:** [`CoverAnalyzer.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/SAIN/modded-multithread/SAIN/Classes/Coverfinder/CoverAnalyzer.cs), [`CoverFinderComponent.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/SAIN/modded-multithread/SAIN/Components/CoverFinderComponent.cs).
* **Problema na Main Thread:**  
  Quando um bot busca se abrigar, o SAIN testa dezenas de colisores do mapa, calculando alinhamento angular (`CheckCoverDirectionvsTargetDirection`), projeção vetorial e distância para o inimigo de forma síncrona, gerando picos de *frametime*.
* **Solução Multithread (`CoverEvaluationJob`):**  
  - Criação da struct blittable `CoverCandidateData` contendo posições, dimensões e vetores normalizados.
  - Implementação de `IJobParallelFor` distribuído pelos núcleos livres da CPU para calcular a pontuação (*CoverScore*) de todos os pontos de uma só vez.
  - A Main Thread apenas seleciona o melhor candidato e popula `Cover.CurrentCoverPoint`.
* **Impacto:** Eliminação de travamentos durante tiroteios intensos com múltiplos bots buscando abrigo.

---

### Ponto 2: Geração e Amostragem Vetorial de Nós de Rota
* **Componentes-Alvo:** [`SAINEnemyPath.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/SAIN/modded-multithread/SAIN/Classes/Bot/EnemyClasses/Path/SAINEnemyPath.cs).
* **Problema na Main Thread:**  
  O método `CalcPathLengthCreateVisionNodes` gera até **1024 nós de visão (`BotVisiblePathNode`)** por inimigo para conferir quinas cegas ao longo do trajeto no NavMesh, consumindo ciclos preciosos da Main Thread em loops de interpolação linear.
* **Solução Multithread (`PathNodeGenerationJob`):**  
  - Como os cantos do caminho (`PathCorners`) são coordenadas `Vector3` estáticas, a matemática de preenchimento dos nós é transferida para um Job nativo (`IJobParallelFor`).
  - O array público `AllPathNodes` e a lista `VisibleNodes` continuam sendo atualizados de forma transparente.
* **Impacto:** Redução de ~1.0ms de processamento por bot em perseguição/reposicionamento.

---

### Ponto 3: Pool Persistente de Memória Nativa (Zero-Alloc em Jobs de Visão)
* **Componentes-Alvo:** [`VisionRaycastJob.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/SAIN/modded-multithread/SAIN/Classes/BotManager/Jobs/VisionRaycastJob.cs), [`DirectionDataJob.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/SAIN/modded-multithread/SAIN/Classes/BotManager/Jobs/DirectionDataJob.cs).
* **Problema na Main Thread:**  
  A cada 33ms (visão) e a cada frame (direção), o código aloca e descarta `NativeArray<RaycastCommand>` e `NativeArray<RaycastHit>` utilizando `Allocator.TempJob`. Essa rotatividade excessiva gera sobrecarga de alocação no motor e fragmentação na CPU.
* **Solução Multithread (Buffers Nativos Reutilizáveis):**  
  - Substituir alocações pontuais por buffers persistentes (`Allocator.Persistent`) redimensionáveis (*Pooled NativeArrays*).
  - Os buffers crescem conforme a quantidade de bots na raid e são liberados apenas no descarte do `BotManagerComponent` no fim da partida.
* **Impacto:** Redução sensível de micro-engasgos (*frametime jitter*) e eliminação de alocações transitórias.

---

### Ponto 4: Matriz Matemática de Ganho de Visão em Lote
* **Componentes-Alvo:** [`EnemyGainSightClass.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/SAIN/modded-multithread/SAIN/Classes/Bot/EnemyClasses/Vision/EnemyGainSightClass.cs).
* **Problema na Main Thread:**  
  625 linhas de cálculos trigonométricos contínuos (visão periférica, lanternas, elevação, postura, tempo desde o último avistamento) executadas para todos os inimigos monitorados.
* **Solução Multithread (`GainSightBatchJob`):**  
  - Consolidação dos coeficientes em structs numéricas desacopladas de classes Unity.
  - Execução dos cálculos de modificador de ganho de visão em segundo plano através do `JobManager`.
  - O método público `GetGainSightModifier(Enemy enemy)` lê o resultado computado sem travar a Main Thread.
* **Impacto:** Alívio imediato no subsistema de percepção sensorial de todos os bots.

---

### Ponto 5: Paralelização da Matriz $N \times N$ de Distâncias e Orientação
* **Componentes-Alvo:** [`OtherPlayersData.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/SAIN/modded-multithread/SAIN/Classes/Player/OtherPlayersData.cs), [`DirectionDataJob.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/SAIN/modded-multithread/SAIN/Classes/BotManager/Jobs/DirectionDataJob.cs).
* **Problema na Main Thread:**  
  A cada ciclo, a orientação relativa, o produto escalar de olhar e a distância euclidiana entre cada bot e todos os outros jogadores são calculados sequencialmente na Main Thread durante a etapa de `Prepare()`.
* **Solução Multithread:**  
  - Estruturação de `PlayerTickJob` como um `IJobParallelFor` completo, cobrindo o cálculo de todos os pares jogador-alvo simultaneamente.
  - Os resultados continuam alimentando `OtherPlayerData.DistanceData` de forma transparente.
* **Impacto:** Ganho exponencial de desempenho em mapas populosos (Customs, Streets) com mais de 20 bots ativos.

---

### Ponto 6: Sistema de LOD de IA Adaptativo com Despertar Instantâneo
* **Componentes-Alvo:** [`BotComponent.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/SAIN/modded-multithread/SAIN/Components/BotComponent.cs).
* **Problema na Main Thread:**  
  Bots distantes a mais de 200m ou 300m executam árvores de decisão e lógica comportamental complexa 60 vezes por segundo, competindo por CPU com os bots que estão trocando tiros diretamente com o jogador.
* **Solução de LOD Adaptativo:**  
  - **Faixa 0 (Combate Ativo ou < 50m do Jogador):** Tick rate pleno a cada frame (60+ Hz).
  - **Faixa 1 (Média Distância 50m–150m fora de combate):** Tick rate intercalado (~20–30 Hz).
  - **Faixa 2 (Longa Distância > 150m fora de combate):** Tick rate reduzido (~5–10 Hz).
  - **Gatilho de Despertar Imediato (*Instant Wake-up*):** Qualquer evento crítico (dano sofrido, tiro ouvido nas proximidades ou linha de visão confirmada) promove o bot instantaneamente para a Faixa 0 no mesmo frame.
* **Impacto:** Redução de até 40% da carga total de CPU gerada pela IA global do mapa.

---

### Ponto 7: Zero-Alloc e Throttling em `EnemyPlaceRaycastJob`
* **Componentes-Alvo:** [`EnemyPlaceRaycastJob.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/SAIN/modded-multithread/SAIN/Classes/BotManager/Jobs/EnemyPlaceRaycastJob.cs).
* **Problema na Main Thread:**  
  A cada frame, o loop de rastreamento de posições de inimigos aloca **7 NativeArrays temporários** (`PlacePositions`, `BotPositions`, `EnemyPositions`, `PlaceDistancesToBot`, `PlaceDistancesToEnemy`, `_commands`, `_hits`) com `Allocator.TempJob`.
* **Solução Multithread / Zero-Alloc:**  
  - Substituir as 7 alocações por frame por buffers persistentes reutilizáveis.
  - Implementar throttling: checar distâncias de lugares ouvidos/vistos a cada 100ms para bots fora de combate.
* **Impacto:** Elimina centenas de alocações/descartes nativos por segundo.

---

### Ponto 8: Cache e Throttling de Rota em `EnemyPathVisibilityRaycastJob`
* **Componentes-Alvo:** [`EnemyPathVisibilityRaycastJob.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/SAIN/modded-multithread/SAIN/Classes/BotManager/Jobs/EnemyPathVisibilityRaycastJob.cs).
* **Problema na Main Thread:**  
  `CalcEnemyPaths` recalcula caminhos no NavMesh repetidamente para todos os bots do mapa a cada 50ms, mesmo que o inimigo esteja parado na mesma posição.
* **Solução:**  
  - Adicionar threshold de movimento (`(enemyPos - lastCalcPos).sqrMagnitude > 2.25f` / 1.5m) antes de invocar novo cálculo de trajeto.
  - Reutilizar os buffers de raycast do `PathVisionJob`.
* **Impacto:** Redução drástica de cálculos desnecessários no motor do NavMesh.

---

### Ponto 9: Dispersão de Áudio com Triagem Vetorial em `HearingDispersionClass`
* **Componentes-Alvo:** [`HearingDispersionClass.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/SAIN/modded-multithread/SAIN/Classes/Bot/Sense/Hearing/HearingDispersionClass.cs).
* **Problema na Main Thread:**  
  Loops de até 10–20 tentativas de `NavMesh.SamplePosition` na Main Thread toda vez que um som precisa ser dispersado ao redor do jogador ou bot.
* **Solução:**  
  - Triagem de limites espaciais previamente calculada em vetor e amostragem em lote.
* **Impacto:** Elimina pequenos engasgos durante tiroteios com grande volume de disparos simultâneos.

---

### Ponto 10: Otimização de Feixes em `FlashlightRaycastJob`
* **Componentes-Alvo:** [`FlashlightRaycastJob.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/SAIN/modded-multithread/SAIN/Classes/BotManager/Jobs/FlashlightRaycastJob.cs), [`RaycastJob.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/SAIN/modded-multithread/SAIN/Types/Jobs/RaycastJob.cs).
* **Problema na Main Thread:**  
  Criação contínua de novas instâncias de `RaycastJob` alocando `NativeArray<RaycastHit>` a cada ciclo do feixe de lanterna/laser.
* **Solução:**  
  - Integrar os raios de lanternas ao buffer persistente global de raycasts do `JobManager`.
* **Impacto:** Zero sobrecarga de memória nativa em raids noturnas ou ambientes escuros com múltiplos bots utilizando lanternas táticas.

---

## 📊 3. Tabela Comparativa de Arquitetura

| Dimensão | Versão Baseline (`modded`) | Versão Otimizada (`modded-multithread`) |
|---|:---:|:---:|
| **Uso de Núcleos de CPU** | Concentrado na Main Thread (1–2 núcleos) | Distribuído em paralelo (4 a 16 núcleos via Unity Jobs) |
| **Avaliação de Coberturas** | Síncrona na Main Thread (loop de colisores) | Paralela com `CoverEvaluationJob` |
| **Alocação de Buffers Nativo** | Alocação/descarte frequente (`Allocator.TempJob`) | Buffers persistentes reutilizáveis (Zero-Alloc) |
| **Tick Rate de Bots Distantes** | Fixo em 60+ Hz para todo o mapa | Dinâmico e adaptativo por distância (LOD) |
| **Checagem de Lugares de Inimigos** | 7 NativeArrays alocados por frame | Buffers persistentes com checagem por evento/LOD |
| **Compatibilidade com Outros Mods** | 100% compatível | **100% compatível (Princípio da Caixa Preta)** |
