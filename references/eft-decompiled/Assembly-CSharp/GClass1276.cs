using System;
using Dissonance.Integrations.MirrorIgnorance;
using Dissonance.Networking;
using EFT.Network;
using JetBrains.Annotations;

public class GClass1276 : BaseClient<GClass1277, GClass1276, GStruct117>
{
	[NonSerialized]
	public MirrorIgnoranceCommsNetwork MirrorIgnoranceCommsNetwork_0;

	[NonSerialized]
	public EFTWriterClass EFTWriterClass;

	[NonSerialized]
	public byte[] Byte_0 = new byte[1024];

	public GClass1276([NotNull] MirrorIgnoranceCommsNetwork network)
		: base((ICommsNetworkState)network)
	{
		if (network == null)
		{
			throw new ArgumentNullException("network");
		}
		MirrorIgnoranceCommsNetwork_0 = network;
		EFTWriterClass = new EFTWriterClass();
	}

	public override void Connect()
	{
		if (!MirrorIgnoranceCommsNetwork_0.Mode.IsServerEnabled())
		{
			GClass3064.Default.AddMessageListener(MirrorIgnoranceCommsNetwork_0.TypeCode, method_0);
		}
		Connected();
	}

	public override void Disconnect()
	{
		if (!MirrorIgnoranceCommsNetwork_0.Mode.IsServerEnabled())
		{
			GClass3064.Default.AddMessageListener(MirrorIgnoranceCommsNetwork_0.TypeCode, MirrorIgnoranceCommsNetwork.NullMessageReceivedHandler);
		}
		base.Disconnect();
	}

	public override void SendVoiceData(ArraySegment<byte> encodedAudio)
	{
		TalkClass.SetTalkDateTime();
		if (!TalkClass.Blocked)
		{
			base.SendVoiceData(encodedAudio);
		}
	}

	public void method_0([CanBeNull] GStruct416 netMsg)
	{
		NetworkReceivedPacket(MirrorIgnoranceCommsNetwork_0.CopyToArraySegment(new EFTReaderClass(netMsg.buffer), new ArraySegment<byte>(Byte_0)));
	}

	public override void ReadMessages()
	{
	}

	public override void SendReliable(ArraySegment<byte> packet)
	{
		if (!method_1(packet, NetworkChannel.Reliable))
		{
			FatalError("Failed to send reliable packet (unknown Mirror error)");
		}
	}

	public override void SendUnreliable(ArraySegment<byte> packet)
	{
		method_1(packet, NetworkChannel.Unreliable);
	}

	public bool method_1(ArraySegment<byte> packet, NetworkChannel channel)
	{
		if (MirrorIgnoranceCommsNetwork_0.PreprocessPacketToServer(packet))
		{
			return true;
		}
		int bufferCount = MirrorIgnoranceCommsNetwork_0.CopyPacketToNetworkWriter(packet, EFTWriterClass);
		GClass3064.Default.Connection.Send(channel, EFTWriterClass.Byte_0, bufferCount);
		return true;
	}
}
