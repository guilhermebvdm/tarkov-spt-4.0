# 011 — Otimização de Rede do Host (Área de Interesse AoI Culling & Multithreading de Sockets)

**Mod:** FIKA  
**Target / Fork:** `mods/FIKA/modded-V2/`  
**Status:** Em Validação  
**Criado:** 2026-09-15  

---

## 1. Visão Geral

Quando um jogador hospeda uma partida cooperativa no FIKA (ou executa um servidor Headless dedicado), o processo da Unity atua simultaneamente como cliente de renderização visual e servidor autoritativo de rede:
- A cada tick de física e rede (20 a 30 Hz), o componente `BotStateManager` itera sobre todos os bots ativos no mapa (tipicamente entre 20 e 35 bots).
- No código original do FIKA, o método `SendBatchStates()` serializa todos os bots indiscriminadamente em um buffer compartilhado e executa um broadcast cego via `_server.BatchSendStates(_writer)` chamando `_netServer.SendToAll(...)`.
- **Problema:** O Host consome CPU preciosa serializando passos, mira e rotações de cabeça de bots localizados a mais de 1.000m de distância dos seus amigos e despachando pacotes UDP na placa de rede **dentro da Main Thread gráfica da Unity**. Isso gera perda direta de 3 a 6 ms por quadro (redução de 20 a 40 FPS no host) e introduz atraso perceptível de resposta nos controles (*input lag*).

---

## 2. Comportamento Atual (Legado)

1. **Broadcast Cego e Ineficiente:**
   - Todos os clientes conectados recebem pacotes de estado de todos os bots vivos na taxa máxima configurada (`SendRate`, 20/30 Hz), independentemente de estarem a 10 metros ou a 1.500 metros de distância.
   - Não existe distinção entre um bot travado em combate direto contra um amigo e um bot ocioso patrulhando o outro extremo do mapa.
2. **Sobrecarga da Main Thread:**
   - A coleta de estados dos bots, a validação de parâmetros do animator, o empacotamento em `NetDataWriter`, o controle de estouro de MTU e a chamada ao socket UDP da biblioteca LiteNetLib são executados dentro de `MonoBehaviour.Update()`.
   - A CPU do host para a renderização gráfica para fazer trabalho puramente matemático de rede e I/O de sockets.
3. **Ausência de Controle Dinâmico:**
   - Não há opções no menu F12 para desativar ou calibrar o raio de replicação de entidades ou descarregar a rede para threads secundárias.

---

## 3. Comportamento Desejado

1. **Área de Interesse (Area of Interest - AoI Culling):**
   - A transmissão dos estados dos bots passa a ser individualizada por cliente conectado (`NetPeer`), avaliando a distância euclidiana entre o bot e o jogador humano.
   - **Zona Tática (`< 250m` ou Bot em Combate):** Transmissão com taxa cheia (20/30 Hz) para garantir máxima suavidade visual e precisão balística.
   - **Prioridade Absoluta por Combate:** Se o bot tiver um alvo ativo (`GoalEnemy != null`), ele é promovido automaticamente à Zona Tática, independentemente da distância euclidiana (assegurando snipers precisos à longa distância).
   - **Zona Periférica (`250m a 500m`):** Transmissão intercalada a cada 4 ticks (~5 Hz) com escalonamento de carga homogêneo (`(tick + botIndex) % 4 == 0`).
   - **Zona Morta (`> 500m`):** Heartbeat lento a cada 20 ticks (~1 Hz) para manter coordenadas gerais e bússola/mapa sem sobrecarregar a banda.
2. **Threading de Rede (Background Worker - Snapshot Separation):**
   - **Main Thread (< 0.15 ms):** Apenas lê as posições e flags dos bots vivos e posições dos clientes remotos para um buffer pré-alocado de structs blittable puras `PlayerStateData` (39 bytes).
   - **Background Worker Thread:** Acorda via `AutoResetEvent`, consome o snapshot via Double Buffering zero-alloc, aplica os cálculos de AoI, monta o `NetDataWriter` e despacha para os sockets UDP via `NetPeer.Send()`.
3. **Menu F12 (Configurações em Tempo Real):**
   - Chaves BepInEx no menu F12 para ligar/desligar AoI Culling e Network Threading, além de permitir calibração das distâncias near/mid.
4. **Compatibilidade dos Clientes:**
   - Preservação integral do formato de pacote `EPacketType.PlayerState`. O cliente EFT/FIKA interpola os pacotes recebidos sem qualquer quebra de protocolo ou necessidade de atualização binária no cliente.

---

## 4. Critérios de Aceite

- [x] O Host não serializa nem despacha pacotes de bots na Main Thread quando o threading estiver habilitado.
- [x] Bots a menos de 250m de um cliente recebem atualizações em todo tick de rede.
- [x] Bots em combate ativo contra qualquer jogador humano são promovidos à taxa máxima, mesmo se estiverem a > 250m.
- [x] Bots entre 250m e 500m são transmitidos a ~5 Hz (1 a cada 4 ticks), sem picos simultâneos de rede.
- [x] Bots a mais de 500m são transmitidos a ~1 Hz (1 a cada 20 ticks).
- [x] Configurações no menu F12 permitem ativar/desativar AoI e Threading em tempo real.
- [x] Nenhuma DLL é copiada para fora do diretório do mod durante a compilação.
- [x] 0 Erros de compilação no `dotnet build -c Release`.
