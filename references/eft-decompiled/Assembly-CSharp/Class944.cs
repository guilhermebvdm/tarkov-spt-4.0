using System;
using System.Threading;
using FlyingWormConsole3.LiteNetLib;

public class Class944
{
	[NonSerialized]
	public Class941 Class941_0;

	[NonSerialized]
	public int Int_0;

	[NonSerialized]
	public object Object_0 = new object();

	public Class941 GetWithData(PacketProperty property, byte[] data, int start, int length)
	{
		int headerSize = Class941.GetHeaderSize(property);
		Class941 packet = GetPacket(length + headerSize);
		packet.Property = property;
		Buffer.BlockCopy(data, start, packet.RawData, headerSize, length);
		return packet;
	}

	public Class941 GetWithProperty(PacketProperty property, int size)
	{
		Class941 packet = GetPacket(size + Class941.GetHeaderSize(property));
		packet.Property = property;
		return packet;
	}

	public Class941 GetWithProperty(PacketProperty property)
	{
		Class941 packet = GetPacket(Class941.GetHeaderSize(property));
		packet.Property = property;
		return packet;
	}

	public Class941 GetPacket(int size)
	{
		if (size > GClass1474.Int_3)
		{
			return new Class941(size);
		}
		Class941 class941_;
		lock (Object_0)
		{
			class941_ = Class941_0;
			if (class941_ == null)
			{
				return new Class941(size);
			}
			Class941_0 = Class941_0.Next;
		}
		Interlocked.Decrement(ref Int_0);
		class941_.Size = size;
		if (class941_.RawData.Length < size)
		{
			class941_.RawData = new byte[size];
		}
		return class941_;
	}

	public void Recycle(Class941 packet)
	{
		if (packet.RawData.Length <= GClass1474.Int_3 && Int_0 < 1000)
		{
			Interlocked.Increment(ref Int_0);
			packet.RawData[0] = 0;
			lock (Object_0)
			{
				packet.Next = Class941_0;
				Class941_0 = packet;
			}
		}
	}
}
