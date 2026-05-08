using System;
using System.Collections.Generic;
using Dissonance.Integrations.MirrorIgnorance;
using Dissonance.Networking;
using Dissonance.Networking.Server;
using EFT.Network;
using JetBrains.Annotations;

public class GClass1277 : BaseServer<GClass1277, GClass1276, GStruct117>
{
	[NonSerialized]
	[NotNull]
	public MirrorIgnoranceCommsNetwork MirrorIgnoranceCommsNetwork_0;

	[NonSerialized]
	public EFTWriterClass EFTWriterClass = new EFTWriterClass();

	[NonSerialized]
	public byte[] Byte_0 = new byte[1024];

	[NonSerialized]
	public List<GClass3071> List_0 = new List<GClass3071>();

	public GClass1277([NotNull] MirrorIgnoranceCommsNetwork network)
	{
		if (network == null)
		{
			throw new ArgumentNullException("network");
		}
		MirrorIgnoranceCommsNetwork_0 = network;
	}

	public override void Connect()
	{
		GClass3065.Default.AddMessageListener(MirrorIgnoranceCommsNetwork_0.TypeCode, method_0);
		base.Connect();
	}

	public void method_0([NotNull] GStruct416 netmsg)
	{
		NetworkReceivedPacket(new GStruct117(netmsg.Connection), MirrorIgnoranceCommsNetwork_0.CopyToArraySegment(new EFTReaderClass(netmsg.buffer), new ArraySegment<byte>(Byte_0)));
	}

	public override void AddClient([NotNull] ClientInfo<GStruct117> client)
	{
		base.AddClient(client);
		if (client.PlayerName != MirrorIgnoranceCommsNetwork_0.PlayerName)
		{
			List_0.Add(client.Connection.Connection);
		}
	}

	public override void Disconnect()
	{
		base.Disconnect();
		GClass3065.Default.AddMessageListener(MirrorIgnoranceCommsNetwork_0.TypeCode, MirrorIgnoranceCommsNetwork.NullMessageReceivedHandler);
	}

	public override void ReadMessages()
	{
	}

	public override ServerState Update()
	{
		for (int num = List_0.Count - 1; num >= 0; num--)
		{
			if (!List_0[num].IsConnected)
			{
				ClientDisconnected(new GStruct117(List_0[num]));
				List_0.RemoveAt(num);
			}
		}
		return base.Update();
	}

	public override void SendReliable(GStruct117 connection, ArraySegment<byte> packet)
	{
		if (!method_1(packet, connection, NetworkChannel.Reliable))
		{
			FatalError("Failed to send reliable packet (unknown Mirror error)");
		}
	}

	public override void SendUnreliable(GStruct117 connection, ArraySegment<byte> packet)
	{
		method_1(packet, connection, NetworkChannel.Unreliable);
	}

	public bool method_1(ArraySegment<byte> packet, GStruct117 connection, NetworkChannel channel)
	{
		if (MirrorIgnoranceCommsNetwork_0.PreprocessPacketToClient(packet, connection))
		{
			return true;
		}
		if (!connection.Connection.IsConnected)
		{
			return true;
		}
		int bufferCount = MirrorIgnoranceCommsNetwork_0.CopyPacketToNetworkWriter(packet, EFTWriterClass);
		if (connection.Connection == null)
		{
			Log.Error("Cannot send to a null destination");
			return false;
		}
		connection.Connection.Send(channel, EFTWriterClass.Byte_0, bufferCount);
		return true;
	}
}
