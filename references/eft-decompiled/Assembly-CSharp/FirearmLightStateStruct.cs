using BitPacking;

public struct FirearmLightStateStruct
{
	public string Id;

	public bool IsActive;

	public int LightMode;

	public static void Serialize(ISerializer stream, ref FirearmLightStateStruct tacticalComboStatus)
	{
		stream.SerializeLimitedString(ref tacticalComboStatus.Id, ' ', 'z', BitPackingTag.TacticalComboStatusId, 1600u);
		stream.Serialize(ref tacticalComboStatus.IsActive);
		stream.SerializeLimitedInt32(ref tacticalComboStatus.LightMode, 0, 7, BitPackingTag.SelectedMode);
	}

	public override string ToString()
	{
		return $"[ID: {Id} Active: {IsActive} LightMode: {LightMode}]";
	}
}
