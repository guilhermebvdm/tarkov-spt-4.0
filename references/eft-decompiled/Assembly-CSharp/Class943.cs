using System;
using FlyingWormConsole3.LiteNetLib;

public class Class943
{
	public const int Size = 11;

	public readonly long ConnectionId;

	public readonly byte ConnectionNumber;

	public readonly bool IsReusedPeer;

	public Class943(long connectionId, byte connectionNumber, bool isReusedPeer)
	{
		ConnectionId = connectionId;
		ConnectionNumber = connectionNumber;
		IsReusedPeer = isReusedPeer;
	}

	public static Class943 FromData(Class941 packet)
	{
		if (packet.Size > 11)
		{
			return null;
		}
		long connectionId = BitConverter.ToInt64(packet.RawData, 1);
		byte b = packet.RawData[9];
		if (b >= 4)
		{
			return null;
		}
		byte b2 = packet.RawData[10];
		if (b2 > 1)
		{
			return null;
		}
		return new Class943(connectionId, b, b2 == 1);
	}

	public static Class941 Make(long connectId, byte connectNum, bool reusedPeer)
	{
		Class941 @class = new Class941(PacketProperty.ConnectAccept, 0);
		GClass1481.GetBytes(@class.RawData, 1, connectId);
		@class.RawData[9] = connectNum;
		@class.RawData[10] = (byte)(reusedPeer ? 1u : 0u);
		return @class;
	}
}
