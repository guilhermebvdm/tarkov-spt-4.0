using Comfort.Common;

public abstract class GClass767
{
	public static int Write(this byte[] buffer, GClass766 info, int offset = 0)
	{
		int num = offset;
		offset = ((info != null) ? (offset + buffer.Write((int)info.StateName, offset)) : (offset + buffer.Write(-1, offset)));
		return offset - num;
	}

	public static GClass766 ReadPlayerStateInfo(this byte[] buffer, ref int offset)
	{
		int num = buffer.ReadInt32(ref offset);
		if (num == -1)
		{
			return null;
		}
		return new GClass766
		{
			StateName = (EPlayerState)num
		};
	}
}
