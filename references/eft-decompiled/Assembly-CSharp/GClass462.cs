using System.Collections.Generic;
using EFT;
using EFT.InventoryLogic;

public class GClass462 : BotReload
{
	public GClass462(BotOwner owner, Weapon weapon)
		: base(owner, weapon)
	{
	}

	public override bool FightShallReload()
	{
		return false;
	}

	public override void Reload()
	{
	}

	public override bool CanReload(bool withCheckByPeriod, out MagazineItemClass foundMag, out List<AmmoItemClass> ammoForInternalReload)
	{
		foundMag = null;
		ammoForInternalReload = null;
		return false;
	}
}
