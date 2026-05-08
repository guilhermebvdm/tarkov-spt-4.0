public struct GStruct248
{
	public bool RollCylinder;

	public bool RollToZeroCamora;

	public static void Serialize(ISerializer serializer, ref GStruct248 rollCylinderPacket)
	{
		serializer.Serialize(ref rollCylinderPacket.RollCylinder);
		if (rollCylinderPacket.RollCylinder)
		{
			serializer.Serialize(ref rollCylinderPacket.RollToZeroCamora);
		}
	}
}
