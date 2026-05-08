using BitPacking;

public struct GStruct242
{
	public string Id;

	public int SelectedMode;

	public int ScopeIndexInsideSight;

	public int ScopeCalibrationIndex;

	public static void Serialize(ISerializer stream, ref GStruct242 tacticalComboStatus)
	{
		stream.SerializeLimitedString(ref tacticalComboStatus.Id, ' ', 'z', BitPackingTag.SightModeStatusId, 1600u);
		stream.SerializeLimitedInt32(ref tacticalComboStatus.SelectedMode, 0, 7, BitPackingTag.SelectedMode0);
		stream.SerializeLimitedInt32(ref tacticalComboStatus.ScopeIndexInsideSight, 0, 7, BitPackingTag.ScopeIndexInsideSight0);
		stream.SerializeLimitedInt32(ref tacticalComboStatus.ScopeCalibrationIndex, 0, 20, BitPackingTag.ScopeCalibrationIndex0);
	}
}
