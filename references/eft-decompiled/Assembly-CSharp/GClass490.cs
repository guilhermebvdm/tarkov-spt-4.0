using System;
using EFT;
using EFT.InventoryLogic;

public class GClass490 : GClass489
{
	[NonSerialized]
	public const string String_0 = "5e99735686f7744bfc4af32c";

	public GClass490(BotOwner owner, Action<bool> callback)
		: base(owner, callback)
	{
	}

	public override void GetDamage()
	{
		base.GetDamage();
		if (base.Using)
		{
			CancelCurrent();
		}
	}

	public override void RefreshMeds()
	{
		Player getPlayer = BotOwner_0.GetPlayer;
		EquipmentSlot[] secureSlots = BotMedecine.secureSlots;
		List_0.Clear();
		getPlayer.InventoryController.GetAcceptableItemsNonAlloc(secureSlots, List_0);
		foreach (MedsItemClass item in List_0)
		{
			if (item.TemplateId == (MongoID)"5e99735686f7744bfc4af32c")
			{
				base.CurUsingMeds = item;
				return;
			}
		}
		base.CurUsingMeds = null;
	}
}
