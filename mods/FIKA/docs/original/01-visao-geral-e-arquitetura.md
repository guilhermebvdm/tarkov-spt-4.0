---
title: "FIKA — Visão Geral e Arquitetura do Ecossistema"
date: 2026-09-02
status: 🟢 Vivo
authors: Antigravity
---

# FIKA — Visão Geral e Arquitetura do Ecossistema

O **Project FIKA** é a infraestrutura definitiva de conectividade cooperativa multiplayer para o Escape From Tarkov no ecossistema SPT 4.0. Ele transforma a experiência single-player em um ambiente multijogador em tempo real, integrando transporte de dados de baixa latência em UDP, autoridade compartilhada de simulação de mundo, sincronização de física/balística e clientes dedicados headless.

---

## 1. Topologia e Três Fundações do FIKA

O ecossistema FIKA opera sobre três pilares arquiteturais interdependentes:

```mermaid
graph TB
    subgraph SPT_Server_Space [1. Servidor SPT 4.0 / C#]
        FikaServer["Fika-Server-CSharp (FikaServer)"]
        HttpRoutes["Rotas HTTP (/fika/...)"]
        WsServer["WebSocket Server (Eventos & Lobby)"]
        ProfileService["Sincronização de Perfis & Quests"]
        NatPunchBackend["NAT Punching Signal Server"]
        
        FikaServer --> HttpRoutes
        FikaServer --> WsServer
        FikaServer --> ProfileService
        FikaServer --> NatPunchBackend
    end

    subgraph Client_Process [2. Cliente EFT / BepInEx (Host ou Peer)]
        FikaPlugin["FikaPlugin (BaseUnityPlugin)"]
        FikaNetMgr["IFikaNetworkManager (LiteNetLib UDP)"]
        FikaCoopHandler["CoopHandler (Simulação de Raid)"]
        HarmonyPatches["Harmony Patches (EFT Ingress & Hooks)"]
        FikaUI["Matchmaker & In-Game UI"]
        
        FikaPlugin --> FikaNetMgr
        FikaPlugin --> FikaCoopHandler
        FikaPlugin --> HarmonyPatches
        FikaPlugin --> FikaUI
    end

    subgraph Headless_Host [3. Cliente Headless Dedicado (Opcional)]
        FikaHeadless["Fika-Headless (Node.js / TS)"]
        DediState["Simulação de Estado sem Renderer Unity"]
        DediNet["Gerenciador de Sockets UDP Headless"]
        
        FikaHeadless --> DediState
        FikaHeadless --> DediNet
    end

    Client_Process <-->|HTTP / WebSockets| SPT_Server_Space
    Headless_Host <-->|HTTP / WebSockets| SPT_Server_Space
    Client_Process <===>|UDP P2P / LiteNetLib (Gameplay & Física)| Client_Process
    Client_Process <===>|UDP Client-Server| Headless_Host
```

### Papel de Cada Fundação:

1. [**Fika-Plugin (`Fika.Core.dll`)**](../../original/Fika-Plugin/Fika.Core/FikaPlugin.cs):
   - Injetado no processo do cliente Unity via BepInEx.
   - Responsável pelo loop de rede UDP in-game ([`IFikaNetworkManager`](../../original/Fika-Plugin/Fika.Core/Networking/IFikaNetworkManager.cs)), patches no ciclo de vida de raid da BSG, sincronização de animações, física, tiro e interfaces do usuário.
2. [**Fika-Server-CSharp (`FikaServer`)**](../../original/Fika-Server-CSharp/FikaServer/Plugin.cs):
   - Módulo de servidor C# integrado nativamente ao SPT Server 4.0.
   - Fornece endpoints REST HTTP para listagem de raids cooperativas ativas, negociação de IPs/portas, sincronização de progresso compartilhado de missões e persistência de perfis pós-raid.
3. [**Fika-Headless**](../../original/Fika-Headless/src/index.ts):
   - Aplicação leve em TypeScript/Node.js capaz de simular um host dedicado sem instanciar a engine gráfica do Unity.
   - Assume a autoridade de spawn e sincronização da partida, desonerando a CPU dos jogadores clientes.

---

## 2. Ciclo de Vida da Raid Cooperativa

O fluxo de uma partida cooperativa percorre etapas de matchmaking, negociação de transporte UDP, inicialização sincronizada de assets e encerramento seguro de perfil:

```mermaid
sequenceDiagram
    autonumber
    actor Host as Jogador Host / Headless
    actor Client as Jogador Peer (Cliente)
    participant Server as Fika-Server (SPT)
    participant Nat as NAT Puncher / UPnP

    Note over Host,Server: Fase 1: Matchmaking & Abertura de Sala
    Host->>Server: POST /fika/raid/create (Mapa, Hora, Clima, Configs)
    Host->>Nat: Inicia escuta UDP (Porta 25565 / UPnP / STUN)
    Server-->>Host: Sessão criada com SessionID

    Note over Client,Server: Fase 2: Ingress & Conexão de Rede
    Client->>Server: GET /fika/raid/list
    Client->>Server: POST /fika/raid/join (SessionID)
    Server-->>Client: IP, Porta UDP e Metadados do Host
    Client->>Host: Handshake UDP LiteNetLib (ConnectRequest)
    Host-->>Client: ConnectionAccepted (Atribuição de PeerId)

    Note over Host,Client: Fase 3: Carregamento Sincronizado do Mundo
    Host->>Host: Instancia GameWorld & Gera Spawns de Loot/Bots
    Host->>Client: Envia pacotes de Loot, Clima, Portas e Lâmpadas
    Client->>Client: Carrega bundles e aplica estado inicial do Host
    Client->>Host: Notifica "ReadyToSpawn"
    Host->>Client: Dispara início sincronizado da partida

    Note over Host,Client: Fase 4: Loop de Gameplay & Física em Tempo Real
    loop A cada tick de rede (10Hz a 30Hz)
        Host<->Client: Pacotes UDP (Movimento, Balística, Inventário, Pings)
    end

    Note over Host,Server: Fase 5: Exfiltração & Persistência
    Client->>Host: Notifica Extração / Morte
    Host->>Server: Salva estado final do perfil e progresso de missões
    Client->>Server: Download do Perfil Atualizado pós-raid
```

---

## 3. Contratos Públicos e Interoperabilidade com Mods de Terceiros

O FIKA serve como fundação essencial para uma vasta gama de outros mods (tanto do workspace quanto da comunidade global de SPT). Qualquer alteração de assinatura ou comportamento em suas classes públicas pode quebrar mods dependentes.

### Tabela de Interfaces Críticas de Terceiros:

| Componente Público do FIKA | Mod Consumidor | Padrão de Uso / Integração | Risco de Quebra |
| :--- | :--- | :--- | :---: |
| [`Singleton<IFikaNetworkManager>.Instance`](../../original/Fika-Plugin/Fika.Core/Networking/IFikaNetworkManager.cs) | *Speak From Tarkov*, *Dynamic Maps* | Obtenção do estado de rede ativo, `IsServer`, `IsClient` e `CoopHandler`. | 🔴 Crítico |
| [`FikaEventDispatcher`](../../original/Fika-Plugin/Fika.Core/Modding/FikaEventDispatcher.cs) | Mods de Eventos / HUD | Inscrição em `OnFikaEvent`, `FikaRaidStartedEvent`, `PeerConnectedEvent`. | 🔴 Crítico |
| [`FikaPlayer`](../../original/Fika-Plugin/Fika.Core/Main/Players/FikaPlayer.cs) | *SAIN*, *Speak From Tarkov* | Checagem de flags `fp.IsAI`, `fp.IsObservedAI` e identificadores de esquadrão. | 🔴 Crítico |
| [`FikaBackendUtils.PMCName`](../../original/Fika-Plugin/Fika.Core/Main/Utils/FikaBackendUtils.cs) | *Speak From Tarkov*, *TRL-PvpMode* | Obtenção do apelido do operador e perfil HTTP ativo no backend. | 🟠 Alto |
| [`MainMenuUIScript.Instance`](../../original/Fika-Plugin/Fika.Core/UI/Custom/MainMenuUIScript.cs) | Mods de Interface e Menu | Ancoragem de painéis customizados no menu principal cooperativo. | 🟠 Alto |
| [`FikaConfig`](../../original/Fika-Plugin/Fika.Core/FikaConfig.cs) | *SAIN*, *Questing Bots* | Leitura de flags como `SharedQuestProgression`, `FriendlyFire`, `DynamicAI`. | 🟠 Alto |

> [!CAUTION]
> **Regra Estrita de Preservação:** Durante todo o processo de auditoria e refatoração, **nenhuma assinatura pública, namespace, propriedade ou evento de `Fika.Core`, `FikaServer` ou `FikaEventDispatcher` deve ser renomeada, removida ou ter sua visibilidade alterada**.
