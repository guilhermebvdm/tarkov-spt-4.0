using BitPacking;

public struct GStruct239
{
	public string Id;

	public bool IsActive;

	public int SelectedMode;

	public int ScopeCalibrationIndex;

	public static void Serialize(ISerializer stream, ref GStruct239 tacticalComboStatus)
	{
		stream.SerializeLimitedString(ref tacticalComboStatus.Id, ' ', 'z', BitPackingTag.TacticalComboStatusId, 1600u);
		stream.Serialize(ref tacticalComboStatus.IsActive);
		stream.SerializeLimitedInt32(ref tacticalComboStatus.SelectedMode, 0, 7, BitPackingTag.SelectedMode);
		stream.SerializeLimitedInt32(ref tacticalComboStatus.ScopeCalibrationIndex, 0, 20, BitPackingTag.ScopeCalibrationIndex);
	}

	public override string ToString()
	{
		return $"Id: {Id}, IsActive: {IsActive}, SelectedMode: {SelectedMode}, ScopeCalibrationIndex: {ScopeCalibrationIndex}";
	}
}
