using System;
using BitPacking;

public struct GStruct250
{
	public enum EReloadWithAmmoStatus
	{
		None,
		StartReload,
		EndReload,
		AbortReload
	}

	[NonSerialized]
	public const int Int_0 = 63;

	[NonSerialized]
	public const int Int_1 = 3;

	public bool Reload;

	public bool FastReload;

	public EReloadWithAmmoStatus ReloadWithAmmoStatus;

	public int AmmoLoadedToMag;

	public string[] AmmoIds;

	public int[] ShellsIds;

	public static void Serialize(ISerializer serializer, ref GStruct250 reloadWithAmmoPacket)
	{
		serializer.Serialize(ref reloadWithAmmoPacket.Reload);
		if (!reloadWithAmmoPacket.Reload)
		{
			return;
		}
		serializer.Serialize(ref reloadWithAmmoPacket.FastReload);
		serializer.Serialize(ref reloadWithAmmoPacket.ReloadWithAmmoStatus);
		if (reloadWithAmmoPacket.ReloadWithAmmoStatus == EReloadWithAmmoStatus.StartReload || reloadWithAmmoPacket.ReloadWithAmmoStatus == EReloadWithAmmoStatus.EndReload)
		{
			int value = ((reloadWithAmmoPacket.AmmoIds != null) ? reloadWithAmmoPacket.AmmoIds.Length : 0);
			serializer.SerializeLimitedInt32(ref value, 0, 63, BitPackingTag.AmmoIdsLength);
			if (reloadWithAmmoPacket.AmmoIds == null && value > 0)
			{
				reloadWithAmmoPacket.AmmoIds = new string[value];
			}
			for (int i = 0; i < value; i++)
			{
				serializer.SerializeLimitedString(ref reloadWithAmmoPacket.AmmoIds[i], ' ', 'z', BitPackingTag.ReloadWithAmmoAmmoIds, 1600u);
			}
		}
		if (reloadWithAmmoPacket.ReloadWithAmmoStatus == EReloadWithAmmoStatus.EndReload)
		{
			serializer.SerializeLimitedInt32(ref reloadWithAmmoPacket.AmmoLoadedToMag, 0, 63, BitPackingTag.AmmoLoadedToMag);
		}
	}

	public override string ToString()
	{
		return string.Format("Reload: {0}, ReloadWithAmmoStatus: {1}, AmmoLoadedToMag: {2}, AmmoIds: {3}", Reload, ReloadWithAmmoStatus, AmmoLoadedToMag, (AmmoIds == null) ? "null" : string.Join(", ", AmmoIds));
	}
}
