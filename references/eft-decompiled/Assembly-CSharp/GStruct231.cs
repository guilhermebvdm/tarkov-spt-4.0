using UnityEngine;

public struct GStruct231
{
	public int FrameId;

	public float TimeStamp;

	public Vector3 Position;

	public void Serialize(GInterface131 writer)
	{
		writer.Write(FrameId);
		writer.Write(TimeStamp);
		writer.Write(Position);
	}

	public static GStruct231 Deserialize(IDataReader reader)
	{
		return new GStruct231
		{
			FrameId = reader.ReadInt32(),
			TimeStamp = reader.ReadFloat(),
			Position = reader.ReadVector3()
		};
	}
}
