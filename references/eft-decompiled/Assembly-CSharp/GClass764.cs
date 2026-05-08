public abstract class GClass764
{
	public static int Write(this byte[] buffer, GClass762 playerFrame, int offset = 0)
	{
		int num = offset;
		offset += playerFrame.WriteToBuffer(buffer, offset);
		return offset - num;
	}

	public static GClass762 ReadObservedPlayerFrame(this byte[] buffer, ref int offset)
	{
		GClass762 gClass = new GClass762();
		gClass.ReadFromBuffer(buffer, ref offset);
		return gClass;
	}
}
