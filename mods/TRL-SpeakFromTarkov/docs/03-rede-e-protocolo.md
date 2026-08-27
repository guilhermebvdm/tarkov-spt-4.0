---
title: "TRL-SpeakFromTarkov — Camada de Rede e Protocolo de Pacotes"
date: 2026-08-27
status: 🟢 Vivo
authors: Antigravity
---

# TRL-SpeakFromTarkov — Camada de Rede e Protocolo de Pacotes

Documenta o protocolo de transporte de áudio sobre LiteNetLib/FIKA: estrutura dos pacotes, versionamento, fila de envio thread-safe, registro dinâmico no NetPacketProcessor e roteamento de pacotes recebidos para os RemoteSpeakers corretos.

---

## Visão Geral da Camada de Rede

```mermaid
flowchart TD
    subgraph CaptureThread["Thread de Captura (background)"]
        VP["VoipProcessor\nOnOpusDataEncoded"]
    end
    subgraph MainThread["Main Thread (Update)"]
        SN_BC["SftNetwork.Broadcast()\nenfileira PendingAudio"]
        SN_DQ["SftNetwork.DrainSendQueue()\ntransmite via IFikaNetworkManager"]
        REG["EnsurePacketsRegistered()\nverifica referência do manager"]
    end
    subgraph FikaLayer["FIKA / LiteNetLib"]
        NM["IFikaNetworkManager\n.SendData() / .RegisterPacket()"]
        UDP["UDP Datagram\n(Unreliable para voz\nReliableOrdered para canais)"]
    end
    subgraph Receive["Recepção (callback estático)"]
        CB_V2["OnReceiveVoipDataV2()"]
        CB_V1["OnReceiveVoipDataLegacy()"]
        CB_CH["OnReceiveChannelAnnouncement()"]
        DISP["DispatchVoipPacket()\nguards: mod ativo, raid/menu, headless, self-echo"]
        HP["HandleVoipPacket()\npayload → RemoteSpeaker correto"]
    end

    VP --> SN_BC
    SN_BC --> SN_DQ
    SN_DQ --> REG
    REG --> NM
    NM --> UDP
    UDP --> CB_V2
    UDP --> CB_V1
    UDP --> CB_CH
    CB_V2 --> DISP
    CB_V1 --> DISP
    DISP --> HP
    CB_CH --> MenuVoipHUD
```

---

## Estrutura dos Pacotes

### SftAudioPacketV2 (formato atual, ≥ 1.4.0)

**Arquivo:** [`SftAudioPacket.cs`](../modded-V3-audit/SftAudioPacket.cs#L73)

```
[Datagrama LiteNetLib]
  [ushort hash CRC-16 do tipo = hash("SftAudioPacketV2")]
  [ushort body_length]         ← prefixo PutBytesWithLength (ENVELOPE)
  [body]
    [string ProfileId]         ← ID do jogador remetente
    [byte   Channel]           ← 0=raid 3D, 1=menu/lobby, 2=spectator
    [ushort audio_length]
    [byte[] AudioData]         ← payload Opus (max 1275 bytes + cabeçalho)
    [float  VoiceLevel]        ← RMS de DisplayLevel do VoipProcessor
```

**Motivo do envelope (`PutBytesWithLength`):**
O `NetPacketProcessor` do FIKA processa múltiplos pacotes no mesmo datagrama (`while (reader.AvailableBytes > 0)`). Sem o envelope de comprimento, um Deserialize que consumir bytes a mais/menos desalinha o `NetDataReader` e faz o próximo hash ser lido como lixo → `ParseException` → toda a fila de rede do frame é descartada (inclusive posição/movimento FIKA).

**`[ThreadStatic] _innerWriter`:** buffer interno por thread para serialização sem GC extra, mesmo se futuramente serializado de outra thread.

---

### SftAudioPacket (formato legado V1, ≤ 1.3.0)

**Arquivo:** [`SftAudioPacket.cs`](../modded-V3-audit/SftAudioPacket.cs#L27)

```
[Datagrama LiteNetLib]
  [ushort hash CRC-16 do tipo = hash("SftAudioPacket")]
  [string ProfileId]
  [byte   Channel]
  [ushort audio_length]
  [byte[] AudioData]
  [float  VoiceLevel]
```

**Compatibilidade:**
- V1.4.0 OUVE V1.3.0 (registra handler para V1)
- V1.3.0 NÃO ouve V1.4.0 (hash diferente → `ParseException` no peer antigo)
- Política: **LOCKSTEP** — todos os peers + headless devem atualizar juntos

---

### SftChannelAnnouncementPacket

**Arquivo:** [`Network/SftChannelAnnouncementPacket.cs`](../modded-V3-audit/Network/SftChannelAnnouncementPacket.cs)

```
[Datagrama LiteNetLib / DeliveryMethod.ReliableOrdered]
  [envelope PutBytesWithLength]
    [byte   ChannelId]
    [string ChannelName]
    [string HostProfileId]
    [string HostNickname]
    [string TargetProfileId]    ← string.Empty para broadcast
    [byte   Action]             ← enum abaixo
```

| Action | Valor | Semântica |
|---|---|---|
| Announce | 0 | Canal criado / atualizado |
| Close | 1 | Canal encerrado pelo host |
| Join | 2 | Jogador entrou no canal |
| Leave | 3 | Jogador saiu do canal |
| Kick | 4 | Jogador expulso |
| Ban | 5 | Jogador banido do canal |

Além do LiteNetLib, o mesmo anúncio é enviado via **HTTP POST** para `sft/channels/announce` no servidor SPT (visível para jogadores ainda no menu antes da sessão FIKA existir).

---

## SftNetwork — Gestão de Estado e Thread-Safety

**Arquivo:** [`Network/SftNetwork.cs`](../modded-V3-audit/Network/SftNetwork.cs)

### Fila de Envio Thread-Safe

```mermaid
sequenceDiagram
    participant CT as CaptureThread
    participant Q as ConcurrentQueue(PendingAudio)
    participant MT as Main Thread (Update)
    participant FIKA as IFikaNetworkManager

    CT->>Q: sendQueue.Enqueue(opusData, channel, voiceLevel)
    Note over CT,Q: Se Count >= 25 frames, descarta o mais antigo primeiro
    MT->>Q: DrainSendQueue() — TryDequeue em loop
    MT->>FIKA: SendData(SftAudioPacketV2, Unreliable, broadcast=true)
```

**Por que a thread de captura não transmite diretamente:**
`FikaClient._dataWriter` é um `NetDataWriter` de instância compartilhado **sem lock**. Chamar `SendData` fora da main thread corromperia esse buffer e produziria datagramas malformados → `ParseException: Undefined packet in NetDataReader` nos peers.

**Teto da fila:** `MaxQueuedFrames = 25` (~500ms de áudio @ 20ms/frame). Protege contra hitches típicos do Tarkov (entrada de jogador, carregamento de loot).

---

### Registro Dinâmico de Pacotes (EnsurePacketsRegistered)

```mermaid
flowchart TD
    CHECK{"IFikaNetworkManager\nInstantiado?"}
    SAME{"currentManager ==\n_lastRegisteredManager?"}
    REG["RegisterPacket V2, V1 e Channel\n→ _lastRegisteredManager = currentManager"]
    NOP["Retorna (já registrado)"]

    CHECK -->|Não| ResetLast["_lastRegisteredManager = null"]
    CHECK -->|Sim| SAME
    SAME -->|Sim| NOP
    SAME -->|Não| REG
```

**Motivo da comparação por referência:**
O FIKA destrói e recria o `IFikaNetworkManager` a cada transição menu→lobby→raid. A nova instância tem o `NetPacketProcessor` vazio. Comparar a referência detecta essa troca e re-registra os handlers.

Chamado em:
1. `SftNetwork.Update()` — todo frame
2. `VoIPPlugin.Update()` — redundância defensiva (caso o `SftNetwork` seja destruído)
3. `DrainSendQueue()` — antes de cada lote de envio
4. `InitFikaSession()` — ao entrar em raid

---

### Guards de Recepção (DispatchVoipPacket + HandleVoipPacket)

```mermaid
flowchart TD
    START["Pacote recebido"]
    G1{"EnableMod == true?"}
    G2{"inRaid OU inMenuChannel?"}
    G3{"IsHeadless?"}
    G4{"self == null?"}
    G5{"profileId == próprio?"}
    G6["Canal 0: filtros\nvivos/mortos + culling"]
    PLAY["HandleVoipPacket\n→ RemoteSpeaker.EnqueuePacket()"]

    START --> G1
    G1 -->|Não| DROP["Descarta"]
    G1 -->|Sim| G2
    G2 -->|Não| DROP
    G2 -->|Sim| G3
    G3 -->|Sim headless| DROP
    G3 -->|Não| G4
    G4 -->|Null| DROP
    G4 -->|OK| G5
    G5 -->|Próprio perfil| DROP
    G5 -->|Outro player| G6
    G6 --> PLAY
```

**Filtros específicos do Canal 0 (raid 3D):**

| Filtro | Condição de descarte |
|---|---|
| Vivos/mortos | `isLocalAlive && !isSenderAlive` → descarta (jogadores vivos não ouvem fantasmas) |
| Culling espacial (sqrMagnitude) | distância > `MaxHearingDistance × 1.10f` → descarta antes de alocar RemoteSpeaker |

---

### Gerenciamento de RemoteSpeakers

Cada `profileId` único recebe um `RemoteSpeaker` próprio:

```
HandleVoipPacket()
  ├── remoteSpeakers.TryGetValue(profileId, out speaker)
  │   └── não encontrado: CreateRemoteSpeaker(profileId)
  ├── inRaid: ancora speaker ao PlayerBones.Head (3D)
  └── menu: SetEmergency2DMode(true) (2D estéreo plano)
```

**`StopSession()`:** Nunca chama `UnregisterPacket<>()`. Remover handlers do NetPacketProcessor compartilhado do FIKA causa `ParseException` em pacotes ainda em voo. O gate de "fora de raid" é feito por guard clause nos callbacks estáticos.

---

### Throttle de Log de Erros

Para evitar que falhas sistemáticas de rede gerem dezenas de stack traces por segundo (causando hitching):

- Primeira ocorrência de cada tipo de exceção: loga stack trace completo.
- Ocorrências subsequentes: suprimidas por 5 segundos, depois loga `(+N falhas suprimidas)`.
- Thread-safe via `Environment.TickCount` (não usa `Time.time`, indisponível fora da main thread).

---

## Resumo de Fluxo Completo

```mermaid
sequenceDiagram
    participant Mic as Microfone
    participant MC as MicrophoneCapturer
    participant VP as VoipProcessor
    participant SN as SftNetwork
    participant FIKA as FIKA LiteNetLib
    participant RS as RemoteSpeaker(peer)

    Mic->>MC: PCM raw (polling 48kHz)
    MC->>MC: Ring buffer + CaptureThread
    MC->>VP: ProcessAudio(pcmSamples)
    VP->>VP: AudioFilter.Apply() (RNNoise/HPF)
    VP->>VP: UpdateTransmittingState (VAD/PTT/Open)
    VP->>SN: OnOpusDataEncoded(opusBytes, voiceLevel)
    SN->>SN: sendQueue.Enqueue(PendingAudio)
    Note over SN: Update() Main Thread
    SN->>FIKA: SendData(SftAudioPacketV2, Unreliable)
    FIKA->>RS: OnReceiveVoipDataV2 callback
    RS->>RS: Decode Opus → streamBuffer
    RS->>RS: OnAudioFilterRead: jitter, pan, oclusão, atenuação
```
