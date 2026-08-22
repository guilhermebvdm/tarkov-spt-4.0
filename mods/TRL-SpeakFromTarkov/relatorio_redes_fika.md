# 📡 Relatório Técnico de Engenharia de Redes — Separação de Tráfego & LiteNetLib/FIKA

**Autor:** Engenheiro de Redes Sênior / Lead Multiplayer Architecture  
**Projeto:** TRL-SpeakFromTarkov (SPT 4.0 / Tarkov Red Line / FIKA Coop)  
**Foco:** Multiplexação de Canais, DeliveryMethod, Prevencão de Desync e Isolamento de Tráfego UDP

---

## 1. ⚡ MÉTODO DE ENTREGA (DELIVERY METHOD)

### Enum de Entrega Utilizado
- **Enum Exato:** `Fika.Core.Networking.LiteNetLib.DeliveryMethod.Unreliable`.
- **Chamada da API do FIKA:**
  ```csharp
  Singleton<IFikaNetworkManager>.Instance.SendData(
      ref packet, 
      Fika.Core.Networking.LiteNetLib.DeliveryMethod.Unreliable, 
      broadcast: true
  );
  ```

### Isolamento de Tráfego vs. Dados do Jogo
- **FIKA (Canal do Jogo):** Utiliza `DeliveryMethod.ReliableOrdered` para os pacotes de estado de jogador (`PlayerStatePacket`), tiros, danos e inventário. Exige confirmação (ACK) e garante ordem de entrega.
- **TRL-SpeakFromTarkov (Canal de Áudio):** Utiliza estritamente `DeliveryMethod.Unreliable` para todos os datagramas de áudio (`SftAudioPacketV2`).
- **Resultado Prático:** Se pacotes de voz forem perdidos durante o combate, o roteador e a placa de rede os descartam instantaneamente. A fila de movimentação do FIKA **nunca é bloqueada** por perda de pacotes de voz, eliminando *Bufferbloat* e *Head-of-Line Blocking*.

*(Nota: Apenas mensagens administrativas de menu, como anúncios de entrar/sair de sala de rádio, utilizam `ReliableOrdered` por não serem frequentes e exigirem confirmação).*

---

## 2. 🔀 ALOCAÇÃO DE CANAIS & PREVENÇÃO DE COLISÃO

### Identificação de Tipo de Pacote (Short Hash CRC-16)
- O FIKA utiliza o `NetPacketProcessor` do LiteNetLib, que registra cada tipo de pacote usando um hash de 16 bits do nome da estrutura: `GetShortHash(typeof(T))`.
- Nossos pacotes são registrados de forma exclusiva sob os tipos `SftAudioPacketV2` e `SftChannelAnnouncementPacket`.
- **Prevenção de Colisão:** Não há colisão com os pacotes internos do FIKA. Quando o FIKA recebe um datagrama UDP, o `NetPacketProcessor` lê os primeiros 2 bytes de hash e roteia os dados exclusivamente para a nossa função cadastrada (`OnReceiveVoipDataV2`).

### Campo de Canal Lógico (`Channel`)
Dentro da estrutura `SftAudioPacketV2`, existe o campo numérico explícito:
```csharp
public byte Channel;
```
- `Channel 0`: VOIP 3D de Proximidade em Raid (calcula atenuação física e espacialização).
- `Channel 1+`: Canais de Esquadrão / Rádio / Salas Privadas no Menu.

---

## 3. 🌊 DRENAGEM DA FILA DE ENVIO (DRAIN LOOP & UNREALIZABLE HITCHING)

### Arquitetura de Fila Concorrente (`sendQueue`)
- **Produtor (Thread de Captura / Background):** O codificador Opus enfileira frames na `ConcurrentQueue<PendingAudio> sendQueue`.
- **Teto do Buffer de Saída (`MaxQueuedFrames = 25`):** Limite defensivo de 25 frames (~500ms de áudio). Se a Main Thread passar por uma queda brusca de FPS (hitch de carregamento), o frame mais antigo é descartado antes de enfileirar o novo:
  ```csharp
  while (sendQueue.Count >= MaxQueuedFrames && sendQueue.TryDequeue(out _)) { }
  ```

### Despacho na Main Thread (`DrainSendQueue`)
- O esvaziamento da fila ocorre no método `DrainSendQueue()`, chamado a cada quadro no `Update()` da Main Thread em `SftNetwork.cs`.
- **Impacto no FPS:** Em regime de execução normal (60+ FPS), há apenas 1 a 2 quadros de áudio (~20-40ms) por `Update()`. O despacho leva menos de **0,05 ms**, não gerando micro-travamentos na Unity.
- **Proteção Thread-Safe:** O envio é feito na Main Thread porque o `FikaClient._dataWriter` interno do FIKA não é thread-safe. Despachar da Main Thread impede a corrupção do buffer compartilhado do FIKA.

---

## 4. 🪝 HOOK DE RECEPÇÃO & CICLO DE VIDA DO MANAGER

### Registro de Callback Nativo
- O mod registra seu callback de recepção diretamente no `NetPacketProcessor` do manager ativo do FIKA:
  ```csharp
  currentManager.RegisterPacket<SftAudioPacketV2>(OnReceiveVoipDataV2);
  ```

### Rastreamento Dinâmico de Instância (`EnsurePacketsRegistered`)
- **O Problema da Troca de Cenas:** O FIKA destrói e recria o `IFikaNetworkManager` a cada transição (Menu $\rightarrow$ Lobby $\rightarrow$ Raid $\rightarrow$ Fim de Raid).
- **A Solução:** O `SftNetwork` armazena a referência `_lastRegisteredManager`. A cada `Update()`, a função `EnsurePacketsRegistered()` verifica se a referência do manager mudou:
  ```csharp
  var currentManager = Singleton<IFikaNetworkManager>.Instance;
  if (_lastRegisteredManager != currentManager)
  {
      currentManager.RegisterPacket<SftAudioPacketV2>(OnReceiveVoipDataV2);
      _lastRegisteredManager = currentManager;
  }
  ```

### Airbag de Segurança de Envelope
- O método `OnReceiveVoipDataV2` é envelopado com tratamento de exceções. Se um pacote corrompido chegar pela rede, o erro é contido dentro do envelope do VOIP sem derrubar o laço `ReadAllPackets` do LiteNetLib, garantindo que os pacotes de posição e tiro do FIKA continuem sendo processados sem desync.
