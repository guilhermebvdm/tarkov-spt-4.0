using System;

public class GClass1283 : EFTReaderClass, IDisposable
{
	public GClass1283(byte[] bytes)
		: base(bytes)
	{
	}

	public GClass1283(ArraySegment<byte> segment)
		: base(segment)
	{
	}

	public void Dispose()
	{
		PacketToEFTReaderAbstractClass.Return(this);
	}
}
