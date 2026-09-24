---
title: FIKA — Otimização de Rede do Host (Área de Interesse & Multithreading)
date: 2026-09-15
status: 🟢 Vivo
authors: [guilhermebvdm, Antigravity]
---

# 🚀 FIKA — Otimização de Rede do Host (Área de Interesse & Multithreading)

Este documento estabelece a especificação técnica e a arquitetura de engenharia para resolver o gargalo de desempenho sofrido pelo jogador que **hospeda partidas cooperativas no FIKA** (`mods/FIKA/modded-V2`).

---

## 🛑 1. Diagnóstico do Problema no Host

Quando um jogador hospeda uma partida no FIKA, seu jogo deixa de ser apenas um cliente e se torna simultaneamente o **Servidor Dedicado e o Cliente Gráfico** no mesmo processo da Unity:

```
[ ORÇAMENTO DE TEMPO DA MAIN THREAD (Limite de 16.6 ms para 60 FPS) ]
Solo:  [ Render ~5ms ][ IA ~4ms ][ Física ~1ms ] = ~10ms (100 FPS)
Coop:  [ Render ~5ms ][ IA Multiplicada ~8ms ][ Rede FIKA ~5ms ][ Sync Jogadores ~4ms ] = ~22ms (45 FPS)
```

### Causas Raiz Mapeadas no Código:
1. **Broadcast Global Cego de Bots ([BotStateManager.cs:81-122](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/FIKA/modded-V2/Fika-Plugin/Fika.Core/Main/Components/BotStateManager.cs#L81-L122)):**
   * O método `SendBatchStates()` empacota todos os 20–30 bots vivos do mapa em um buffer único e dispara `_server.BatchSendStates(_writer)`, que chama `_netServer.SendToAll(...)`.
   * **Problema:** Um amigo a 1.200m de distância recebe a rotação de cabeça e os passos de um Scav a 30 Hz desnecessariamente.
2. **Serialização e Envio Presos na Main Thread da Unity ([FikaServer.cs:546-570](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/FIKA/modded-V2/Fika-Plugin/Fika.Core/Networking/FikaServer.cs#L546-L570)):**
   * Toda a iteração de arrays de bots, empacotamento em `NetDataWriter`, controle de MTU e despacho nos sockets UDP roda dentro de `MonoBehaviour.Update()`.
   * **Problema:** A thread principal é obrigada a parar a renderização gráfica para fazer trabalho de I/O de rede.
3. **Multiplicação Exponencial de Raycasts de Bots ([LookSensor.cs:347-357](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/references/eft-decompiled/Assembly-CSharp/LookSensor.cs#L347-L357)):**
   * Cada bot vivo no mapa testa linha de visão contra todos os jogadores humanos da partida a cada frame.

---

## 🎯 2. Pilar A: Área de Interesse (Area of Interest - AoI Culling)

Em vez de transmitir todos os bots para todos os jogadores, a replicação passa a ser filtrada dinamicamente pela distância euclidiana entre o bot e cada cliente.

### Arquitetura de Filtragem por Faixas de Distância:

| Faixa | Distância | Taxa de Envio | Comportamento |
| :--- | :---: | :---: | :--- |
| **Zona Tática** | `< 250 metros` ou em combate | **Taxa Plena (20/30 Hz)** | Movimentação, mira e rotação com máxima precisão e suavidade. |
| **Zona Periférica** | `250m a 500 metros` | **Intercalada (~5 Hz)** | Envio a cada 4 ou 5 ticks. O cliente interpola sem saltos visuais. |
| **Zona Morta** | `> 500 metros` | **Heartbeat (~1 Hz)** | Apenas confirmação de existência e coordenadas gerais para mapa/radar. |

### Estruturas de Dados Existentes a Utilizar:
* **Posição do Convidado:** Acessível via `CoopHandler.HumanPlayers[i].Position` ou `ObservedPlayer.Position` em [CoopHandler.cs:50](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/FIKA/modded-V2/Fika-Plugin/Fika.Core/Main/Components/CoopHandler.cs#L50).
* **Posição do Bot:** Acessível via `PlayerStateData.Position` em [PlayerStateData.cs:47](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/FIKA/modded-V2/Fika-Plugin/Fika.Core/Networking/Packets/Player/PlayerStateData.cs#L47) (struct blittable de 39 bytes).
* **Impacto Estimado:** Redução de **50% a 70%** no volume de dados trafegados e no processamento de rede do Host.

---

## ⚡ 3. Pilar B: Threading de Rede (Background Worker)

Desacoplar a coleta de dados (que exige a Unity) da serialização e envio (que não exige a Unity) utilizando o **Snapshot Separation Pattern**:

```mermaid
graph TD
    subgraph Unity_MainThread [Main Thread da Unity (Tempo < 0.2 ms)]
        A["BotStateManager.Update()"] --> B["Coleta de Posições e Estados da Unity"]
        B --> C["Grava em Array de Structs Puras: PlayerStateData[]"]
        C --> D["Despacha Snapshot para Canal Thread-Safe"]
    end

    subgraph Background_Worker [Background Worker Thread (Zero Custo de FPS)]
        D --> E["Consome o Snapshot do Tick"]
        E --> F["Calcula AoI Culling por Convidado"]
        F --> G["Escreve nos Buffers do NetDataWriter"]
        G --> H["Despacha Pacotes UDP via NetPeer.Send()"]
    end
```

### Regras Mandatórias de Implementação:
1. **Zero-Alloc em Background:** Utilizar arrays pré-alocados de `PlayerStateData` (`NativeArray` ou pools estáticos) para não estressar o Garbage Collector.
2. **Não Invocar Métodos Unity na Thread Secundária:** Nenhuma propriedade de `Transform`, `GameObject` ou `Component` pode ser tocada na thread de fundo. Todo dado necessário deve vir pré-extraído na struct do snapshot.
3. **Impacto Estimado:** Devolve de **3 a 6 milissegundos por quadro** diretamente para a thread gráfica do Host, eliminando o *input lag* nos controles.

---

## 🧠 4. Pilar C: Sinergia com o Sensory Culling do SAIN Multithread

Para fechar o triângulo de alta performance no coop, o Host deve utilizar a versão [mods/SAIN/modded-multithread](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/SAIN/modded-multithread/):
* **LOD Adaptativo de IA ([BotComponent.cs:189-318](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/SAIN/modded-multithread/SAIN/Components/BotComponent.cs#L189-L318)):** Reduz a frequência do cérebro de bots distantes para 8 Hz (`LOD_FAR_INTERVAL = 0.12f`) e paraleliza raycasts de visão via `GainSightBatchJob`.
* **Resultado Combinado:** 
  - O **SAIN Multithread** impede que bots distantes consumam a CPU do Host calculando raycasts de visão contra os amigos.
  - O **FIKA AoI + Background Worker** impede que esses bots consumam a CPU do Host gerando pacotes de rede indevidos.

---

## 📂 5. Arquivos-Alvo em `mods/FIKA/modded-V2/`

1. **`Fika-Plugin/Fika.Core/Main/Components/BotStateManager.cs`**:
   - Implementação da triagem de Área de Interesse (AoI).
   - Separação do loop de `SendBatchStates()` para disparar via worker task.
2. **`Fika-Plugin/Fika.Core/Networking/FikaServer.cs`**:
   - Criação de método `SendStatesToPeer(NetPeer peer, NetDataWriter writer)` para substituir o `SendToAll` global nos estados de bots.
3. **`Fika-Plugin/Fika.Core/FikaConfig.cs`**:
   - Criação de chaves de configuração F12:
     - `EnableAoICulling` (`bool`, padrão `true`).
     - `AoINearDistance` (`float`, padrão `250.0m`).
     - `AoIMidDistance` (`float`, padrão `500.0m`).
     - `EnableNetworkThreading` (`bool`, padrão `true`).
