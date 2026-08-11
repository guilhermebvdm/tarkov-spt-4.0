using Fika.Core.Networking.LiteNetLib.Utils;

namespace VisceralCombat.Ragdolls.Classes.Packets;

/// <summary>
/// Bidirectional handshake packet to verify that all human players in a FIKA raid
/// have VisceralCombat installed.
///
/// Flow:
///   1. Host broadcasts this packet with IsRequest=true immediately after raid start.
///   2. Each client with VisceralCombat loaded responds with IsRequest=false (ACK).
///   3. Host collects ACKs. If count == expected human count → AllPlayersHaveVisceralCombat = true.
/// </summary>
public struct VisceralHandshakePacket : INetSerializable
{
	/// <summary>
	/// True  = host → clients (broadcast ping).
	/// False = client → host  (ACK/pong response).
	/// </summary>
	public bool IsRequest { get; set; }

	/// <summary>NetId of the responding client (filled in by client, ignored in requests).</summary>
	public int ResponderNetId { get; set; }

	public void Serialize(NetDataWriter writer)
	{
		writer.Put(IsRequest);
		writer.Put(ResponderNetId);
	}

	public void Deserialize(NetDataReader reader)
	{
		IsRequest   = reader.GetBool();
		ResponderNetId = reader.GetInt();
	}
}
