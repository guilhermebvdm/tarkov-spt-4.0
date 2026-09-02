---
title: "FIKA — Servidor C# (FikaServer) e Cliente Headless"
date: 2026-09-02
status: 🟢 Vivo
authors: Antigravity
---

# FIKA — Servidor C# (FikaServer) e Cliente Headless

A sustentação das sessões cooperativas no SPT 4.0 apoia-se em dois módulos de retaguarda: o **Fika-Server-CSharp**, responsável pelo backend HTTP/WebSocket e gerenciamento de sessões no servidor SPT, e o **Fika-Headless**, que permite executar o cliente Escape From Tarkov em modo servidor dedicado com baixo consumo de memória.

---

## 1. Módulo de Servidor C# (`Fika-Server-CSharp`)

O servidor C# do FIKA é compilado como extensão do servidor SPT 4.0, integrando-se via injeção de dependência e roteamento dinâmico:

```mermaid
graph TD
    subgraph SPT_Server_Engine [Servidor SPT 4.0 Core]
        HttpListener["SPT HTTP Dynamic Router"]
        WsEngine["SPT WebSocket Handler"]
        DatabaseServer["SPT Database Server (Locales, Items, Quests)"]
    end

    subgraph FikaServer_Architecture [FikaServer (C# Module)]
        FikaHttpRouter["FikaHttpRouter (/fika/...)"]
        FikaWsRouter["FikaWebSocketRouter (/fika/ws)"]
        
        RaidController["RaidController (Criação, Join, Listagem de Raids)"]
        ProfileController["ProfileController (Sincronização & Persistência)"]
        NATController["NATController (Sinalização STUN / Hole Punch)"]
        QuestSharingService["QuestSharingService (Progresso Compartilhado)"]
        
        FikaHttpRouter --> RaidController
        FikaHttpRouter --> ProfileController
        FikaHttpRouter --> NATController
        FikaWsRouter --> RaidController
        RaidController --> QuestSharingService
    end

    HttpListener --> FikaHttpRouter
    WsEngine --> FikaWsRouter
    DatabaseServer <--> FikaServer_Architecture
```

### Principais Endpoints e Serviços:
- **Rotas de Matchmaking (`/fika/raid/...`):**
  - `POST /fika/raid/create`: Registra uma nova sala com configurações de mapa, clima, hora e opções de host.
  - `GET /fika/raid/list`: Retorna todas as incursões ativas disponíveis para ingresso.
  - `POST /fika/raid/join`: Reserva vaga e obtém as credenciais UDP (IP/porta) do host.
- **Sincronização de Missões e Perfis (`QuestSharingService`):**
  - Gerencia o compartilhamento de objetivos completados em grupo, distribuindo experiência e desbloqueios sem corromper os arquivos JSON de perfil do SPT.
- **Servidor de Perfuração NAT Integrado (`NATServer`):**
  - Fornece sinalização STUN local para viabilizar conexões P2P diretas entre os clientes.

---

## 2. Cliente Headless Dedicado (`Fika-Headless`)

O **Fika-Headless** é composto por dois subprojetos projetados para executar o Tarkov como servidor dedicado:

```mermaid
graph LR
    subgraph AssetNuker [1. Fika.Headless.AssetNuker]
        CLI["AssetNuker CLI (.NET)"]
        CleanBundles["Substitui Texturas / Sons / Meshes por Placeholders"]
        ReducedFootprint["Redução do Peso do Jogo de ~40GB para Mínimo"]
        CLI --> CleanBundles --> ReducedFootprint
    end

    subgraph HeadlessPlugin [2. Fika.Headless (BepInEx Client Plugin)]
        Plugin["FikaHeadlessPlugin.cs"]
        DisableGpu["Desativa Câmeras, Shaders & Renderização Unity"]
        AutoHostLoop["Loop Automático de Matchmaking e Hospedagem"]
        WsClient["Conexão WebSocket com FikaServer"]
        
        Plugin --> DisableGpu
        Plugin --> AutoHostLoop
        Plugin --> WsClient
    end
```

### Funcionalidades do Headless:
1. [**`Fika.Headless.AssetNuker`**](../../original/Fika-Headless/Fika.Headless.AssetNuker/Program.cs):
   - Ferramenta de linha de comando que substitui assets visuais pesados por arquivos vazios, reduzindo a pegada de RAM e tempo de carregamento da instância dedicada.
2. [**`FikaHeadlessPlugin`**](../../original/Fika-Headless/Fika.Headless/FikaHeadlessPlugin.cs):
   - Inicializa o Tarkov com a flag `IsHeadless = true`.
   - Desliga pipelines de iluminação, câmeras virtuais e áudio local.
   - Comunica-se via WebSocket com o `FikaServer`, aguardando comandos para instanciar raids automaticamente sempre que um grupo de jogadores requisitar um servidor dedicado.

---

## 3. Matriz de Compatibilidade e Preservação de APIs

Para garantir que mods de terceiros (como *Speak From Tarkov*, *SAIN*, *Dynamic Maps*, *Questing Bots*, *TRL-PvpMode*) continuem funcionando sem falhas:

| Componente | Regra de Compatibilidade Estrita |
| :--- | :--- |
| **`IFikaNetworkManager`** | Manter propriedades `IsServer`, `IsClient`, `ConnectedPeers` e método `SendData` inalterados. |
| **`FikaEventDispatcher`** | Preservar todos os tipos de eventos (`FikaEvent`, `PeerConnectedEvent`, `FikaRaidStartedEvent`). |
| **`FikaPlayer` / `FikaBot`** | Manter públicas as propriedades de identificação (`IsAI`, `IsObservedAI`, `NetId`, `ProfileId`). |
| **`FikaBackendUtils`** | Manter estático o acesso ao `Profile`, `PMCName` e flags de estado de sessão. |
| **Rotas do `FikaServer`** | Preservar as estruturas JSON de requisição/resposta nos endpoints `/fika/...`. |
