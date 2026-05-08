using System;
using EFT;
using EFT.InventoryLogic;

public class GClass527 : BotRandomPlanItemDropper
{
	[NonSerialized]
	public const string String_0 = "590c678286f77426c9660122";

	public GClass527(BotOwner owner)
		: base(owner)
	{
	}

	public override Item FindItemToDropToBot()
	{
		return method_1();
	}

	public Item method_1()
	{
		Player getPlayer = BotOwner_0.GetPlayer;
		EquipmentSlot[] secureSlots = BotMedecine.secureSlots;
		CacheItemsList.Clear();
		getPlayer.InventoryController.GetAcceptableItemsNonAlloc(secureSlots, CacheItemsList);
		foreach (Item cacheItems in CacheItemsList)
		{
			if (cacheItems is MedKitItemClass && cacheItems.TemplateId == (MongoID)"590c678286f77426c9660122")
			{
				return cacheItems;
			}
		}
		return null;
	}
}
