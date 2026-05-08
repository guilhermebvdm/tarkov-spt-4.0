using System;

public struct GStruct251
{
	public enum ETag
	{
		Command = 1,
		Status
	}

	public ETag Tag;

	public GStruct252 CommandPacket;

	public GStruct378 StatusPacket;

	public void Serialize(GInterface131 writer)
	{
		writer.Write((byte)Tag);
		switch (Tag)
		{
		default:
			throw new ArgumentOutOfRangeException();
		case ETag.Status:
			GClass3006.Serialize(writer, ref StatusPacket);
			break;
		case ETag.Command:
			CommandPacket.Serialize(writer);
			break;
		}
	}

	public void Deserialize(IDataReader reader)
	{
		Tag = (ETag)reader.ReadByte();
		switch (Tag)
		{
		default:
			throw new ArgumentOutOfRangeException();
		case ETag.Status:
			GClass3006.Serialize(reader, ref StatusPacket);
			break;
		case ETag.Command:
			CommandPacket.Deserialize(reader);
			break;
		}
	}
}
