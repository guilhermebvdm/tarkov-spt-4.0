# 007 — ACK de Operação de Inventário Bloqueado por Spawn de Bots · Especificação Técnica

**Mod:** FIKA  
**Target / Fork:** `mods/FIKA/modded-V2/`  
**Spec Funcional:** [007-fila-inventario-bloqueada-spawn-bot-01-spec.md](007-fila-inventario-bloqueada-spawn-bot-01-spec.md)  
**Status:** 🟢 Aprovado e Implementado  
**Data:** 2026-09-15  

---

## 1. Diagnóstico do Problema de Concorrência de Canais

### 1.1 O Mecanismo de Fila do LiteNetLib
O LiteNetLib organiza a entrega garantida e ordenada (`DeliveryMethod.ReliableOrdered`) através de canais independentes (`BaseChannel`).
Cada canal possui uma chave sequencial indexada por:
`channelIndex = channelNumber * NetConstants.ChannelTypeCount + (byte)deliveryMethod`

- Tanto o `FikaClient.cs` (`:146`) quanto o `FikaServer.cs` (`:161`) configuram `ChannelsCount = 2`.
- No entanto, todas as chamadas do FIKA utilizam a sobrecarga padrão sem `channelNumber`, o que faz com que **todo o tráfego do jogo seja enviado no Canal 0**:
  - `SendCharacter` (dados pesados de perfil, vestimentas, armas e mods de bots/jogadores no spawn).
  - `InventoryOperation` (requisições de swap, move, split de itens pelo cliente).
  - `OperationCallback` (ACK autoritativo do servidor confirmando ou rejeitando a operação).
  - `UpdateBackendData`, sincronização de lâmpadas, portas, janelas e stashes.

### 1.2 O Fenômeno de *Head-of-Line Blocking* no Spawn de Bots
Quando múltiplos bots entram no mapa simultaneamente (ex: spawn inicial ou ondas de reforço):
1. O servidor enfileira múltiplos pacotes grandes de `SendCharacter` no Canal 0 `ReliableOrdered`.
2. Se um jogador realizar uma recarga de arma com troca de carregador (swap) ou mover um item exatamente nesse momento:
   - A requisição `InventoryOperation` ou a resposta do servidor `OperationCallback` é colocada **no final da mesma fila sequencial do Canal 0**.
3. O protocolo `ReliableOrdered` **bloqueia a entrega de qualquer pacote subsequente** até que todos os fragmentos anteriores de `SendCharacter` tenham sido transmitidos, recebidos e confirmados por ACK na rede.
4. O Watchdog local do jogador (`FikaPlayer.cs:CallbackTimeoutSeconds = 5.0f`, introduzido no item 002) atinge 5 segundos de espera, assume que a rede caiu e força `EOperationStatus.Failed`.
5. Milissegundos depois, a fila do Canal 0 é esvaziada e o `OperationCallback` real do servidor chega como `Succeeded`. Isso causa o "status mismatch" e deixa o item "piscando" na interface até um refresh manual.

---

## 2. Solução Técnica Proposta: Segregação de Canais (QoS)

A solução consiste em ativar o **Canal 1** (`channelNumber = 1`), que já está alocado na inicialização da rede, dedicando-o exclusivamente para operações críticas de inventário que exigem baixa latência e não podem sofrer bloqueio de fila.

### 2.1 Matriz de Segregação de Tráfego

| Canal LiteNetLib | `channelNumber` | Tipos de Pacotes Alocados | Comportamento de Fila |
| :---: | :---: | :--- | :--- |
| **Canal 0 (Geral / Mundo)** | `0` | `SendCharacter` (spawn de bots/jogadores), `UpdateBackendData`, `ClientConnected`/`Disconnected`, stashes, janelas, lâmpadas, quests e metadados. | Fila sequencial padrão. Rajadas de spawn não afetam o inventário. |
| **Canal 1 (Inventário Crítico)** | `1` | `InventoryOperation`, `OperationCallback`, `ItemPacket`, `InventoryOperationResultPacket`. | Fila sequencial isolada. Entrega imediata de ACKs sem *Head-of-Line Blocking*. |

---

## 3. Implementação Proposta

### 3.1 Seletor Determinístico de Canal
Em `Fika.Core.Networking.NetworkUtils` (ou classe compartilhada de rede):

```csharp
public static class NetworkChannels
{
    public const byte ChannelGeneral = 0;
    public const byte ChannelInventory = 1;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte GetChannelForSubPacket(EGenericSubPacketType type)
    {
        return type switch
        {
            EGenericSubPacketType.InventoryOperation => ChannelInventory,
            EGenericSubPacketType.OperationCallback => ChannelInventory,
            EGenericSubPacketType.ItemPacket => ChannelInventory,
            _ => ChannelGeneral
        };
    }
}
```

### 3.2 Atualização em `FikaClient.cs`
Estender `SendGenericPacket` e `SendNetReusable` para aceitar `byte channelNumber`:
```csharp
public void SendGenericPacket(EGenericSubPacketType type, IPoolSubPacket subpacket, bool broadcast = false, NetPeer peerToIgnore = null)
{
    var packet = _genericPacket;
    packet.Type = type;
    packet.SubPacket = subpacket;
    var channel = NetworkChannels.GetChannelForSubPacket(type);
    SendNetReusable(ref packet, DeliveryMethod.ReliableOrdered, channel, broadcast);
}

public void SendNetReusable<T>(ref T packet, DeliveryMethod deliveryMethod, byte channelNumber = 0, bool broadcast = false) where T : INetReusable
{
    _dataWriter.Reset();
    _dataWriter.Put(broadcast);
    _dataWriter.PutEnum(EPacketType.Serializable);

    _packetProcessor.WriteNetReusable(_dataWriter, ref packet);
    _netClient.SendToAll(_dataWriter, channelNumber, deliveryMethod);

    packet.Clear();
}
```

### 3.3 Atualização em `FikaServer.cs`
Aplicar a mesma segregação em `SendGenericPacket` e `SendGenericPacketToPeer`:
```csharp
public void SendGenericPacket(EGenericSubPacketType type, IPoolSubPacket subpacket, bool broadcast = false, NetPeer peerToIgnore = null)
{
    var packet = _genericPacket;
    packet.Type = type;
    packet.SubPacket = subpacket;
    var channel = NetworkChannels.GetChannelForSubPacket(type);
    SendNetReusable(ref packet, DeliveryMethod.ReliableOrdered, channel, broadcast, peerToIgnore);
}

public void SendGenericPacketToPeer(EGenericSubPacketType type, IPoolSubPacket subpacket, NetPeer peer)
{
    var packet = _genericPacket;
    packet.Type = type;
    packet.SubPacket = subpacket;
    var channel = NetworkChannels.GetChannelForSubPacket(type);
    SendNetReusableToPeer(ref packet, DeliveryMethod.ReliableOrdered, channel, peer);
}
```

---

## 4. Análise de Compatibilidade e Riscos

1. **Formato de Pacote:**
   - O payload dos pacotes (`GenericPacket`) não é alterado em nenhum byte. O `channelNumber` é transportado no header interno do frame do LiteNetLib.
   - 0 colisões de hash e 0 alterações em esquemas de serialização.
2. **Compatibilidade com Versões Anteriores:**
   - Como o `ChannelsCount = 2` sempre esteve presente nas versões oficiais do FIKA, clientes antigos que se conectarem a servidores novos ou vice-versa já possuem o Canal 1 alocado na inicialização da pilha do LiteNetLib, processando o pacote normalmente no evento `OnNetworkReceive`.
3. **Ordenação Garantida:**
   - A ordenação estrita entre operações consecutivas de inventário do mesmo jogador é 100% mantida, pois todas continuam dentro da mesma fila ordenada do Canal 1.
