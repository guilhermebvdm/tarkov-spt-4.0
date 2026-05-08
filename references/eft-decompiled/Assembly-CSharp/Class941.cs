using System;
using FlyingWormConsole3.LiteNetLib;

public class Class941
{
	[NonSerialized]
	public static int Int_0;

	[NonSerialized]
	public static int[] Int_1;

	public byte[] RawData;

	public int Size;

	public object UserData;

	public Class941 Next;

	public PacketProperty Property
	{
		get
		{
			return (PacketProperty)(RawData[0] & 0x1F);
		}
		set
		{
			RawData[0] = (byte)((uint)(RawData[0] & 0xE0) | (uint)value);
		}
	}

	public byte ConnectionNumber
	{
		get
		{
			return (byte)((RawData[0] & 0x60) >> 5);
		}
		set
		{
			RawData[0] = (byte)((RawData[0] & 0x9F) | (value << 5));
		}
	}

	public ushort Sequence
	{
		get
		{
			return BitConverter.ToUInt16(RawData, 1);
		}
		set
		{
			GClass1481.GetBytes(RawData, 1, value);
		}
	}

	public bool IsFragmented => (RawData[0] & 0x80) != 0;

	public byte ChannelId
	{
		get
		{
			return RawData[3];
		}
		set
		{
			RawData[3] = value;
		}
	}

	public ushort FragmentId
	{
		get
		{
			return BitConverter.ToUInt16(RawData, 4);
		}
		set
		{
			GClass1481.GetBytes(RawData, 4, value);
		}
	}

	public ushort FragmentPart
	{
		get
		{
			return BitConverter.ToUInt16(RawData, 6);
		}
		set
		{
			GClass1481.GetBytes(RawData, 6, value);
		}
	}

	public ushort FragmentsTotal
	{
		get
		{
			return BitConverter.ToUInt16(RawData, 8);
		}
		set
		{
			GClass1481.GetBytes(RawData, 8, value);
		}
	}

	static Class941()
	{
		Int_0 = Enum.GetValues(typeof(PacketProperty)).Length;
		Int_1 = new int[Int_0 + 1];
		for (int i = 0; i < Int_1.Length; i++)
		{
			switch ((PacketProperty)(byte)i)
			{
			default:
				Int_1[i] = 1;
				break;
			case PacketProperty.Channeled:
			case PacketProperty.Ack:
				Int_1[i] = 4;
				break;
			case PacketProperty.Ping:
				Int_1[i] = 3;
				break;
			case PacketProperty.Pong:
				Int_1[i] = 11;
				break;
			case PacketProperty.ConnectRequest:
				Int_1[i] = 14;
				break;
			case PacketProperty.ConnectAccept:
				Int_1[i] = 11;
				break;
			case PacketProperty.Disconnect:
				Int_1[i] = 9;
				break;
			}
		}
	}

	public void MarkFragmented()
	{
		RawData[0] |= 128;
	}

	public Class941(int size)
	{
		RawData = new byte[size];
		Size = size;
	}

	public Class941(PacketProperty property, int size)
	{
		size += GetHeaderSize(property);
		RawData = new byte[size];
		Property = property;
		Size = size;
	}

	public static int GetHeaderSize(PacketProperty property)
	{
		return Int_1[(uint)property];
	}

	public int GetHeaderSize()
	{
		return Int_1[RawData[0] & 0x1F];
	}

	public bool Verify()
	{
		byte b = (byte)(RawData[0] & 0x1F);
		if (b > Int_0)
		{
			return false;
		}
		int num = Int_1[b];
		bool flag = (RawData[0] & 0x80) != 0;
		if (Size >= num)
		{
			if (flag)
			{
				return Size >= num + 6;
			}
			return true;
		}
		return false;
	}
}
