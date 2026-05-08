using UnityEngine;

public struct SmokeGrenadeDataPacketStruct
{
	public Vector3 Position;

	public string Id;

	public string Template;

	public int Time;

	public Quaternion Orientation;

	public short PlatformId;

	public static SmokeGrenadeDataPacketStruct Read(EFTReaderClass reader)
	{
		return new SmokeGrenadeDataPacketStruct
		{
			Id = GClass1285.ReadString(reader),
			Position = GClass1285.ReadVector3(reader),
			Template = GClass1285.ReadString(reader),
			Time = GClass1285.ReadInt(reader),
			Orientation = GClass1285.ReadQuaternion(reader),
			PlatformId = GClass1285.ReadShort(reader)
		};
	}

	public void Write(EFTWriterClass writer)
	{
		GClass1290.WriteString(writer, Id);
		GClass1290.WriteVector3(writer, Position);
		GClass1290.WriteString(writer, Template);
		GClass1290.WriteInt(writer, Time);
		GClass1290.WriteQuaternion(writer, Orientation);
		GClass1290.WriteShort(writer, PlatformId);
	}
}
