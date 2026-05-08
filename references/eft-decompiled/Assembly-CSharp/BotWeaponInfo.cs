using System;
using EFT;
using EFT.InventoryLogic;

public class BotWeaponInfo
{
	public Weapon weapon;

	public BotReload Reload;

	[NonSerialized]
	public Func<Weapon.EFireMode, bool> ChangeAction;

	public bool IsNowAutomatic
	{
		get
		{
			Weapon obj = weapon;
			if (obj == null)
			{
				return false;
			}
			return obj.SelectedFireMode == Weapon.EFireMode.fullauto;
		}
	}

	public int BulletCount => Reload.BulletCount;

	public BotWeaponInfo(BotOwner owner, Weapon weapon, EquipmentSlot slot, Func<Weapon.EFireMode, bool> changeAction)
	{
		this.weapon = weapon;
		ChangeAction = changeAction;
		Reload = BotReload.Create(owner, weapon, slot);
	}

	public void ChangeFireMode(Weapon.EFireMode mode)
	{
		ChangeAction(mode);
	}

	public bool CheckHaveAmmoForReload()
	{
		Reload.CanReload(withCheckByPeriod: false, out var foundMag, out var ammoForInternalReload);
		if (foundMag == null)
		{
			if (ammoForInternalReload != null)
			{
				return ammoForInternalReload.Count > 0;
			}
			return false;
		}
		return true;
	}
}
