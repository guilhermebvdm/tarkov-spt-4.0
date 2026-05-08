using System.Net;

public abstract class GClass1488
{
	public readonly int ExtraPacketSizeForLayer;

	public GClass1488(int extraPacketSizeForLayer)
	{
		ExtraPacketSizeForLayer = extraPacketSizeForLayer;
	}

	public abstract void ProcessInboundPacket(IPEndPoint endPoint, ref byte[] data, ref int offset, ref int length);

	public abstract void ProcessOutBoundPacket(IPEndPoint endPoint, ref byte[] data, ref int offset, ref int length);
}
