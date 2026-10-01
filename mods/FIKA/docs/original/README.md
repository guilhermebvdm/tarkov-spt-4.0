# Índice da Documentação Técnica do Código Original — Project FIKA

Este índice reúne todos os artigos temáticos da especificação e arquitetura técnica do código original do **Project FIKA** (Client `v2.3.9`, Server `v2.3.5`, Headless `v1.4.15` e Wiki).

---

## 📚 Sumário dos Documentos de Arquitetura

| # | Documento | Tema Principal | Status |
| :---: | :--- | :--- | :---: |
| **01** | [**01. Visão Geral e Arquitetura**](./01-visao-geral-e-arquitetura.md) | Topologia dos 3 pilares (Client, Server, Headless), ciclo de vida da raid cooperativa e contratos públicos com mods de terceiros. | 🟢 Vivo |
| **02** | [**02. Arquitetura de Rede e Transporte UDP**](./02-arquitetura-de-rede-e-transporte-udp.md) | Transporte UDP via LiteNetLib, multiplexação de canais (`DeliveryMethod`), serialização binária, pooling de pacotes e NAT Punching. | 🟢 Vivo |
| **03** | [**03. Sincronização de Jogadores e Interpolação**](./03-sincronizacao-de-jogadores-e-interpolacao.md) | Entidades observadas (`ObservedPlayer`), buffer de interpolação temporal, predição de física e sincronização de animações/ADS. | 🟢 Vivo |
| **04** | [**04. Sincronização de Bots, IA e Spawns**](./04-sincronizacao-de-bots-ia-e-spawns.md) | Autoridade de IA no Host, entidade `FikaBot`, otimizações `DynamicAI`, limites de spawn e despawn forçado. | 🟢 Vivo |
| **05** | [**05. Inventário Estrito, Balística e Combate**](./05-inventario-estrito-balistica-e-combate.md) | Sincronização estrita de inventário (`StrictInventorySync`), transações de itens, pacotes de disparo, balística e registro de dano. | 🟢 Vivo |
| **06** | [**06. Ciclo de Vida de Raid e Mundo**](./06-ciclo-de-vida-de-raid-e-interatividade-de-mundo.md) | Handshake de ingress, sincronização de portas, lâmpadas, airdrops, BTR, transits entre mapas e reconexão em raid. | 🟢 Vivo |
| **07** | [**07. Sistemas Auxiliares: Revival, Pings e HUD**](./07-sistemas-auxiliares-revival-pings-e-hud.md) | Sistema de reanimação (Revival/Downed), marcadores táticos 3D (Pings), placas de identificação (NamePlates) e chat in-game. | 🟢 Vivo |
| **08** | [**08. Servidor C# e Cliente Headless**](./08-servidor-csharp-e-cliente-headless.md) | Endpoints HTTP/WebSocket do `FikaServer`, cliente dedicado `Fika-Headless`, `AssetNuker` e matriz de compatibilidade com terceiros. | 🟢 Vivo |

---

## 🗂️ Mapeamento dos Arquivos de Código-Fonte

### 1. Client BepInEx Plugin (`Fika-Plugin / Fika.Core`)
- [`FikaPlugin.cs`](../../original/Fika-Plugin/Fika.Core/FikaPlugin.cs) — Ponto de entrada do plugin BepInEx.
- [`FikaConfig.cs`](../../original/Fika-Plugin/Fika.Core/FikaConfig.cs) — Gerenciador de configurações BepInEx F12.
- [`IFikaNetworkManager.cs`](../../original/Fika-Plugin/Fika.Core/Networking/IFikaNetworkManager.cs) — Interface central de transporte de rede.
- [`FikaServer.cs`](../../original/Fika-Plugin/Fika.Core/Networking/FikaServer.cs) — Gerenciador de rede do Host.
- [`FikaClient.cs`](../../original/Fika-Plugin/Fika.Core/Networking/FikaClient.cs) — Gerenciador de rede do Cliente.
- [`FikaPlayer.cs`](../../original/Fika-Plugin/Fika.Core/Main/Players/FikaPlayer.cs) — Base de replicação de jogadores.
- [`ObservedPlayer.cs`](../../original/Fika-Plugin/Fika.Core/Main/Players/ObservedPlayer.cs) — Clone observado de jogadores remotos e bots.
- [`FikaBot.cs`](../../original/Fika-Plugin/Fika.Core/Main/Players/FikaBot.cs) — Entidade autoritativa de simulação de IA no Host.
- [`FikaEventDispatcher.cs`](../../original/Fika-Plugin/Fika.Core/Modding/FikaEventDispatcher.cs) — Dispatcher global de eventos para interoperabilidade de mods.

### 2. Mod de Servidor C# (`Fika-Server-CSharp / FikaServer`)
- [`Plugin.cs`](../../original/Fika-Server-CSharp/FikaServer/Plugin.cs) — Ponto de entrada do mod de servidor SPT.
- [`FikaHttpRouter.cs`](../../original/Fika-Server-CSharp/FikaServer/Routers/FikaHttpRouter.cs) — Roteamento das chamadas REST `/fika/...`.
- [`FikaWebSocketRouter.cs`](../../original/Fika-Server-CSharp/FikaServer/Routers/FikaWebSocketRouter.cs) — Roteamento de eventos WebSocket.
- [`RaidController.cs`](../../original/Fika-Server-CSharp/FikaServer/Controllers/RaidController.cs) — Gerenciamento de lobbies e sessões cooperativas.

### 3. Cliente Dedicado Headless (`Fika-Headless`)
- [`FikaHeadlessPlugin.cs`](../../original/Fika-Headless/Fika.Headless/FikaHeadlessPlugin.cs) — Plugin de desativação de renderização e automação de hosting.
- [`Program.cs`](../../original/Fika-Headless/Fika.Headless.AssetNuker/Program.cs) — Utilitário de compactação e redução de assets para instâncias headless.
