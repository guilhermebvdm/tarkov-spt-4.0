using System;
using System.Net;
using FlyingWormConsole3.LiteNetLib;

public class Class942
{
	public const int HeaderSize = 14;

	public readonly long ConnectionTime;

	public readonly byte ConnectionNumber;

	public readonly byte[] TargetAddress;

	public readonly GClass1482 Data;

	public Class942(long connectionTime, byte connectionNumber, byte[] targetAddress, GClass1482 data)
	{
		ConnectionTime = connectionTime;
		ConnectionNumber = connectionNumber;
		TargetAddress = targetAddress;
		Data = data;
	}

	public static int GetProtocolId(Class941 packet)
	{
		return BitConverter.ToInt32(packet.RawData, 1);
	}

	public static Class942 FromData(Class941 packet)
	{
		if (packet.ConnectionNumber >= 4)
		{
			return null;
		}
		long connectionTime = BitConverter.ToInt64(packet.RawData, 5);
		int num = packet.RawData[13];
		if (num != 16 && num != 28)
		{
			return null;
		}
		byte[] array = new byte[num];
		Buffer.BlockCopy(packet.RawData, 14, array, 0, num);
		GClass1482 gClass = new GClass1482(null, 0, 0);
		if (packet.Size > 14 + num)
		{
			gClass.SetSource(packet.RawData, 14 + num, packet.Size);
		}
		return new Class942(connectionTime, packet.ConnectionNumber, array, gClass);
	}

	public static Class941 Make(GClass1484 connectData, SocketAddress addressBytes, long connectId)
	{
		Class941 @class = new Class941(PacketProperty.ConnectRequest, connectData.Length + addressBytes.Size);
		GClass1481.GetBytes(@class.RawData, 1, 11);
		GClass1481.GetBytes(@class.RawData, 5, connectId);
		@class.RawData[13] = (byte)addressBytes.Size;
		for (int i = 0; i < addressBytes.Size; i++)
		{
			@class.RawData[14 + i] = addressBytes[i];
		}
		Buffer.BlockCopy(connectData.Data, 0, @class.RawData, 14 + addressBytes.Size, connectData.Length);
		return @class;
	}
}
