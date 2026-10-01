# Documentação Técnica e Auditoria do Mod FIKA — SPT 4.0

Este diretório concentra a documentação de arquitetura, análises técnicas de código, relatórios de auditoria e roteiro de otimizações do **Project FIKA** para Escape From Tarkov / SPT 4.0.

---

## 🎯 Objetivos da Auditoria

A auditoria do mod FIKA visa identificar:
1. **Gargalos de Performance & CPU/GPU:** Otimizar alocação de memória no Unity main loop e threads de rede para reduzir Garbage Collection (GC spikes).
2. **Latência e Confiabilidade de Rede:** Aperfeiçoar o envio/recebimento de pacotes UDP via LiteNetLib, serialização binária e descarte de overhead.
3. **Consistência de Estado (Desync Mitigation):** Analisar a sincronização estrita de inventário, interpolação de posições de jogadores/bots e transição de animações.
4. **Resiliência e Tratamento de Exceções:** Auditar patches Harmony no cliente EFT e rotas no Servidor C# para prevenir quebras de sessão e travamentos em raid.
5. **Preservação Estrita de APIs:** Garantir que nenhuma alteração quebre contratos públicos consumidos por mods externos (ex: *Speak From Tarkov*, *SAIN*, *Dynamic Maps*).

---

## 📚 Documentação Técnica do Código Original

Acesse o índice completo e os artigos temáticos em [`docs/original/`](./original/):

| # | Documento | Escopo |
| :---: | :--- | :--- |
| **01** | [**01. Visão Geral e Arquitetura**](./original/01-visao-geral-e-arquitetura.md) | Topologia dos 3 pilares (Client, Server, Headless), ciclo de vida da raid cooperativa e contratos públicos com mods de terceiros. |
| **02** | [**02. Arquitetura de Rede e Transporte UDP**](./original/02-arquitetura-de-rede-e-transporte-udp.md) | Transporte UDP via LiteNetLib, multiplexação de canais (`DeliveryMethod`), serialização binária, pooling de pacotes e NAT Punching. |
| **03** | [**03. Sincronização de Jogadores e Interpolação**](./original/03-sincronizacao-de-jogadores-e-interpolacao.md) | Entidades observadas (`ObservedPlayer`), buffer de interpolação temporal, predição de física e sincronização de animações/ADS. |
| **04** | [**04. Sincronização de Bots, IA e Spawns**](./original/04-sincronizacao-de-bots-ia-e-spawns.md) | Autoridade de IA no Host, entidade `FikaBot`, otimizações `DynamicAI`, limites de spawn e despawn forçado. |
| **05** | [**05. Inventário Estrito, Balística e Combate**](./original/05-inventario-estrito-balistica-e-combate.md) | Sincronização estrita de inventário (`StrictInventorySync`), transações de itens, pacotes de disparo, balística e registro de dano. |
| **06** | [**06. Ciclo de Vida de Raid e Mundo**](./original/06-ciclo-de-vida-de-raid-e-interatividade-de-mundo.md) | Handshake de ingress, sincronização de portas, lâmpadas, airdrops, BTR, transits entre mapas e reconexão em raid. |
| **07** | [**07. Sistemas Auxiliares: Revival, Pings e HUD**](./original/07-sistemas-auxiliares-revival-pings-e-hud.md) | Sistema de reanimação (Revival/Downed), marcadores táticos 3D (Pings), placas de identificação (NamePlates) e chat in-game. |
| **08** | [**08. Servidor C# e Cliente Headless**](./original/08-servidor-csharp-e-cliente-headless.md) | Endpoints HTTP/WebSocket do `FikaServer`, cliente dedicado `Fika-Headless`, `AssetNuker` e matriz de compatibilidade com terceiros. |

👉 [**Acesse o Índice Central da Documentação Original**](./original/README.md)

---

## 🧭 Roteiro de Engenharia e Auditorias

Para acompanhar as fases de auditoria diagnóstica, relatórios de achados e melhorias planejadas:
👉 [**ROADMAP.md**](./ROADMAP.md)
