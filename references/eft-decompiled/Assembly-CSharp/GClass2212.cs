using System.Collections.Generic;
using EFT;
using EFT.InventoryLogic;

public class GClass2212
{
	public readonly Dictionary<EAreaType, CompoundItem> AreasStashes;

	public readonly CompoundItem CustomizationStash;

	public readonly HideoutProfileDescriptorClass Info;

	public readonly bool IsGuest;

	public readonly string OwnerName;

	public readonly string OwnerAccountId;

	public GClass2212(HideoutProfileDescriptorClass info, Dictionary<EAreaType, CompoundItem> areasStashes, CompoundItem customizationStash, bool isGuest, string ownerAccountId, string ownerName = "")
	{
		Info = info;
		AreasStashes = areasStashes;
		CustomizationStash = customizationStash;
		IsGuest = isGuest;
		OwnerName = ownerName;
		OwnerAccountId = ownerAccountId;
	}
}
