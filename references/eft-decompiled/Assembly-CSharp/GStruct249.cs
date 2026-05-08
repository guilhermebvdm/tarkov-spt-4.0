using System;
using BitPacking;

public struct GStruct249
{
	[NonSerialized]
	public const int Int_0 = 6;

	public bool StatusChanged;

	public int CamoraIndex;

	public bool HammerClosed;

	public static void Serialize(ISerializer serializer, ref GStruct249 magStatus)
	{
		serializer.Serialize(ref magStatus.StatusChanged);
		if (magStatus.StatusChanged)
		{
			serializer.SerializeLimitedInt32(ref magStatus.CamoraIndex, 0, 6, BitPackingTag.CamoraIndex);
			serializer.Serialize(ref magStatus.HammerClosed);
		}
	}
}
