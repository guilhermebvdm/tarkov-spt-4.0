using System;
using System.Net;
using System.Text;

public class GClass1490 : GClass1488
{
	[NonSerialized]
	public byte[] Byte_0;

	public GClass1490()
		: base(0)
	{
	}

	public GClass1490(byte[] key)
		: this()
	{
		SetKey(key);
	}

	public GClass1490(string key)
		: this()
	{
		SetKey(key);
	}

	public void SetKey(string key)
	{
		Byte_0 = Encoding.UTF8.GetBytes(key);
	}

	public void SetKey(byte[] key)
	{
		if (Byte_0 == null || Byte_0.Length != key.Length)
		{
			Byte_0 = new byte[key.Length];
		}
		Buffer.BlockCopy(key, 0, Byte_0, 0, key.Length);
	}

	public override void ProcessInboundPacket(IPEndPoint endPoint, ref byte[] data, ref int offset, ref int length)
	{
		if (Byte_0 != null)
		{
			int num = offset;
			int num2 = 0;
			while (num2 < length)
			{
				data[num] ^= Byte_0[num2 % Byte_0.Length];
				num2++;
				num++;
			}
		}
	}

	public override void ProcessOutBoundPacket(IPEndPoint endPoint, ref byte[] data, ref int offset, ref int length)
	{
		if (Byte_0 != null)
		{
			int num = offset;
			int num2 = 0;
			while (num2 < length)
			{
				data[num] ^= Byte_0[num2 % Byte_0.Length];
				num2++;
				num++;
			}
		}
	}
}
