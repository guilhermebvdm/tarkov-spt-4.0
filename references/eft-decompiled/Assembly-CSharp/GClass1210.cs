public class GClass1210 : GClass1209<ushort>
{
	public GClass1210(int cap)
		: base(cap)
	{
	}

	public GClass1210(ushort[] nonReusableBuffer)
		: base(0)
	{
		buffer = nonReusableBuffer;
		numIndices = nonReusableBuffer.Length;
	}
}
