using System;
using BitPacking;

public struct GStruct247
{
	[NonSerialized]
	public const int Int_0 = 63;

	public bool Reload;

	public string[] AmmoIds;

	public byte[] LocationDescription;

	public static void Serialize(ISerializer stream, ref GStruct247 packet)
	{
		stream.Serialize(ref packet.Reload);
		if (packet.Reload)
		{
			int value = ((packet.AmmoIds != null) ? packet.AmmoIds.Length : 0);
			stream.SerializeLimitedInt32(ref value, 0, 63, BitPackingTag.AmmoIdsLength);
			if (packet.AmmoIds == null && value > 0)
			{
				packet.AmmoIds = new string[value];
			}
			for (int i = 0; i < value; i++)
			{
				stream.SerializeLimitedString(ref packet.AmmoIds[i], ' ', 'z', BitPackingTag.ReloadWithAmmoAmmoIds, 1600u);
			}
			stream.SerializeBytesAndSize(ref packet.LocationDescription, 1600u);
		}
	}
}
