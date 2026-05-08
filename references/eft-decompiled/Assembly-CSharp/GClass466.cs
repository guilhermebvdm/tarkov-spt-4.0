using System;
using EFT;
using EFT.InventoryLogic;
using UnityEngine;

public class GClass466 : BotWeaponSelector
{
	[NonSerialized]
	public const float Float_0 = 130f;

	[NonSerialized]
	public const float Float_1 = 27f;

	[NonSerialized]
	public bool Bool_0;

	[NonSerialized]
	public bool Bool_1;

	[NonSerialized]
	public float Float_2;

	public bool IsGrenadeLauncher => base.LastEquipmentSlot == EquipmentSlot.SecondPrimaryWeapon;

	public GClass466(BotOwner owner)
		: base(owner)
	{
	}

	public override void Activate()
	{
		Bool_0 = method_1(base.SecondPrimaryWeaponItem);
		Bool_1 = method_1(base.FirstPrimaryWeaponItem);
		if (!Bool_1 && !Bool_0)
		{
			CanChangeToSecondWeapons_1 = false;
		}
		else if (Bool_0)
		{
			TryChangeWeapon();
		}
	}

	public override void ManualUpdate()
	{
		if (!base.CanChangeToSecondWeapons || Float_2 > Time.time)
		{
			return;
		}
		float num = 1f;
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy == null)
		{
			return;
		}
		bool flag = goalEnemy.Distance > 130f || goalEnemy.Distance < 27f;
		if (Bool_1)
		{
			flag = !flag;
		}
		if (flag)
		{
			if (base.LastEquipmentSlot == EquipmentSlot.SecondPrimaryWeapon)
			{
				if (BotOwner_0.WeaponManager.MainWeaponInfo.BulletCount == 0 && !BotOwner_0.WeaponManager.MainWeaponInfo.CheckHaveAmmoForReload())
				{
					num = 999f;
				}
				else if (TryChangeToMain())
				{
					num = 15f;
				}
			}
		}
		else if (base.LastEquipmentSlot == EquipmentSlot.FirstPrimaryWeapon && TryChangeWeapon())
		{
			num = 15f;
		}
		if (method_2() && base.LastEquipmentSlot == EquipmentSlot.SecondPrimaryWeapon && TryChangeToMain())
		{
			num = 15f;
		}
		Float_2 = Time.time + num;
	}

	public bool method_1(Item secondPrimaryWeapon)
	{
		if (!(secondPrimaryWeapon is Weapon weapon))
		{
			return false;
		}
		return weapon.IsGrenadeLauncher;
	}

	public bool method_2()
	{
		return false;
	}
}
