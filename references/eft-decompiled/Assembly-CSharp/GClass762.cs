using Comfort.Common;

public class GClass762 : GClass761
{
	public GStruct38[] AnimatorParams;

	public int StateHash;

	public override int WriteToBuffer(byte[] buffer, int offset = 0)
	{
		int num = offset;
		offset += base.WriteToBuffer(buffer, offset);
		offset += buffer.Write(AnimatorParams.Length, offset);
		for (int i = 0; i < AnimatorParams.Length; i++)
		{
			offset += AnimatorParams[i].WriteToBuffer(buffer, offset);
		}
		offset += buffer.Write(StateHash, offset);
		return offset - num;
	}

	public override void ReadFromBuffer(byte[] buffer, ref int offset)
	{
		base.ReadFromBuffer(buffer, ref offset);
		int num = buffer.ReadInt32(ref offset);
		AnimatorParams = new GStruct38[num];
		for (int i = 0; i < num; i++)
		{
			AnimatorParams[i].ReadFromBuffer(buffer, ref offset);
		}
		StateHash = buffer.ReadInt32(ref offset);
	}
}
