using UnityEngine;

public struct GStruct233
{
	public bool HasData;

	public int ProfileId;

	public float TimeStamp;

	public GStruct231 PrevFrame;

	public GStruct231 CurrentFrame;

	public Vector3 InterpolatedPosition;

	public Vector3 InterpolatedStomachPosition;

	public Vector3 InterpolatedHeadPosition;

	public float Layer0NT;

	public override string ToString()
	{
		return $"HasData: {HasData}, Id: {ProfileId}, TStamp: {TimeStamp}, [{PrevFrame.FrameId}:{CurrentFrame.FrameId}] [{PrevFrame.TimeStamp:F5}:{CurrentFrame.TimeStamp:F5}], ";
	}

	public static void Serialize(GStruct233 info, GInterface131 writer)
	{
		writer.Write(info.HasData);
		if (info.HasData)
		{
			writer.Write(info.ProfileId);
			writer.Write(info.TimeStamp);
			info.PrevFrame.Serialize(writer);
			info.CurrentFrame.Serialize(writer);
			writer.Write(info.InterpolatedPosition);
			writer.Write(info.InterpolatedStomachPosition);
			writer.Write(info.InterpolatedHeadPosition);
			writer.Write(info.Layer0NT);
		}
	}

	public static GStruct233 Deserialize(IDataReader reader)
	{
		if (reader.ReadBool())
		{
			return new GStruct233
			{
				HasData = true,
				ProfileId = reader.ReadInt32(),
				TimeStamp = reader.ReadFloat(),
				PrevFrame = GStruct231.Deserialize(reader),
				CurrentFrame = GStruct231.Deserialize(reader),
				InterpolatedPosition = reader.ReadVector3(),
				InterpolatedStomachPosition = reader.ReadVector3(),
				InterpolatedHeadPosition = reader.ReadVector3(),
				Layer0NT = reader.ReadFloat()
			};
		}
		return default(GStruct233);
	}
}
