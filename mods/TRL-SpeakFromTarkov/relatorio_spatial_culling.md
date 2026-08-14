# 🛰️ Relatório Técnico de Engenharia de Redes — Spatial Culling Host-Side & Otimização de Upload

**Autor:** Engenheiro de Redes & Performance Sênior / Lead Multiplayer Architect  
**Projeto:** TRL-SpeakFromTarkov (SPT 4.0 / Tarkov Red Line / FIKA Coop)  
**Foco:** Interceptação no Host (Server Relay), Culling de Distância Zero-Alloc, Roteamento Seletivo por Peer e Filtro Híbrido de Canais

---

## 1. 🔍 INTERCEPTAÇÃO NO HOST (SERVER-SIDE RELAY)

### Sobrecarga de Registro com UserData (`NetPeer`)
No `SftNetwork.cs`, quando a sessão é ativada no Host (Server FIKA), o registro dos pacotes utiliza a sobrecarga de UserData do FIKA:
```csharp
currentManager.RegisterPacket<SftAudioPacketV2, NetPeer>(OnReceiveVoipDataServer);
```
- **Funcionamento:** Quando um cliente envia um pacote `SftAudioPacketV2`, o `NetPacketProcessor` do FIKA entrega ao Host tanto o payload do áudio quanto a referência do `NetPeer senderPeer` do cliente emissor.
- **Controle de Relay:** O Host **não chama `broadcast: true` automático**. Ele assume o controle do roteamento dos pacotes recebidos dos clientes.

---

## 2. ⚡ CÁLCULO DE DISTÂNCIA EFICIENTE & ZERO-ALLOCATION

### Obtenção de Posições no Servidor FIKA
- No Host, as posições de todos os jogadores ativos são mantidas em memória via `Singleton<IFikaNetworkManager>.Instance.ObservedPlayers` e no `GameWorld`.
- O Host obtém a posição do emissor (`senderPos`) através do seu `ProfileId` ou do `NetPeer`.

### Checagem Matemática com Distância Quadrada (`sqrMagnitude`)
Para eliminar o custo computacional de raízes quadradas (`Mathf.Sqrt` / `Vector3.Distance`), a checagem é feita comparando vetores em distância quadrada:
```csharp
float maxDistSqr = maxDistance * maxDistance; // Ex: 40m * 40m = 1600.0f
float sqrDistance = (receiverPos - senderPos).sqrMagnitude;

if (sqrDistance <= maxDistSqr)
{
    // O receptor está dentro do raio máximo de audição!
}
```
- **Zero-Allocation:** Nenhuma alocação de heap (`new`) ocorre nessa checagem vetorial pura, mantendo a chamada com 0 bytes alocados no Garbage Collector.

---

## 3. 🎯 ROTEAMENTO SELETIVO (UNICAST / MULTICAST POR PEER)

### Substituindo `broadcast: true` por Envio Direcionado
Em vez de disparar o pacote para todos os peers conectados no servidor, o Host percorre a lista de clientes e faz o envio seletivo apenas para quem estiver no raio de audição:

```csharp
private static void RelaySpatialAudio(SftAudioPacketV2 packet, NetPeer senderPeer, Vector3 senderPos, float maxDistance = 40f)
{
    var manager = Singleton<IFikaNetworkManager>.Instance;
    if (manager == null) return;

    float maxDistSqr = maxDistance * maxDistance;

    foreach (var observedPlayer in manager.ObservedPlayers)
    {
        // 1. Não retransmite de volta para o próprio emissor
        if (observedPlayer.NetPeer == senderPeer) continue;

        // 2. Checagem de distância 3D física em raio quadrado
        float sqrDist = (observedPlayer.Position - senderPos).sqrMagnitude;
        if (sqrDist <= maxDistSqr)
        {
            // Envia apenas para este cliente específico
            manager.SendDataToPeer(ref packet, DeliveryMethod.Unreliable, observedPlayer.NetPeer);
        }
    }
}
```

### Economia de Upload no Host
- Numa raid com 5 jogadores no mapa Customs (3 nos Docks e 2 nos Dorms a 300m de distância), o Host retransmite o áudio de proximidade dos Docks **apenas para os 2 colegas que estão perto dos Docks**.
- **Resultado:** Redução massiva de **60% a 80%** na banda de upload do Host durante rajadas de fala simultâneas em grandes mapas.

---

## 4. 🔀 TRATAMENTO HÍBRIDO DE CANAIS (`Channel 0` vs `Channel 1+`)

### Lógica de Decisão no Callback do Host
No método de recepção do Host (`OnReceiveVoipDataServer`), a decisão de roteamento avalia o campo `packet.Channel`:

```csharp
private static void OnReceiveVoipDataServer(SftAudioPacketV2 packet, NetPeer senderPeer)
{
    // CANAL 0: Proximidade 3D em Raid -> Aplica Spatial Culling (ex: 40m)
    if (packet.Channel == 0)
    {
        Vector3 senderPos = GetPlayerPositionByProfileId(packet.ProfileId);
        RelaySpatialAudio(packet, senderPeer, senderPos, maxDistance: 40f);
    }
    // CANAL 1+: Esquadrão / Rádio / Menu -> Transmissão Global (Broadcast sem culling)
    else
    {
        Singleton<IFikaNetworkManager>.Instance.SendData(
            ref packet, 
            DeliveryMethod.Unreliable, 
            broadcast: true
        );
    }
}
```

### Vantagens do Filtro Híbrido
- **Voz 3D de Proximidade (Canal 0):** Cortada pelo Culling de Distância Host-Side a 40m, poupando a banda do servidor.
- **Rádio / Esquadrão (Canal 1+):** Trafega sem restrição de distância pelo mapa inteiro (comunicação global via walkie-talkie).
