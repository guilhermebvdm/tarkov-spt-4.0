using System;
using System.Net;

public class GClass1489 : GClass1488
{
	public GClass1489()
		: base(4)
	{
	}

	public override void ProcessInboundPacket(IPEndPoint endPoint, ref byte[] data, ref int offset, ref int length)
	{
		if (length < 5)
		{
			GClass1475.smethod_5("[NM] DataReceived size: bad!");
			length = 0;
			return;
		}
		int num = length - 4;
		if (GClass1480.Compute(data, offset, num) != BitConverter.ToUInt32(data, num))
		{
			length = 0;
		}
		else
		{
			length -= 4;
		}
	}

	public override void ProcessOutBoundPacket(IPEndPoint endPoint, ref byte[] data, ref int offset, ref int length)
	{
		GClass1481.GetBytes(data, length, GClass1480.Compute(data, offset, length));
		length += 4;
	}
}
