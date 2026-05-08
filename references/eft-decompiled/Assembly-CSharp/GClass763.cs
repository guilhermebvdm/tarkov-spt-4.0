public abstract class GClass763
{
	public static int Write(this byte[] buffer, GClass761 playerFrame, int offset = 0)
	{
		int num = offset;
		offset += playerFrame.WriteToBuffer(buffer, offset);
		return offset - num;
	}

	public static void ReadPlayerFrame(this GClass761 playerFrame, byte[] buffer, ref int offset)
	{
		playerFrame.ReadFromBuffer(buffer, ref offset);
	}

	public static GClass761 ReadPlayerFrame(this byte[] buffer, ref int offset)
	{
		GClass761 gClass = new GClass761();
		gClass.ReadFromBuffer(buffer, ref offset);
		return gClass;
	}
}
