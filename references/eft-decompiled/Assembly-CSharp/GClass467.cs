using System;
using Comfort.Common;
using EFT;
using EFT.InventoryLogic;

public class GClass467 : BotWeaponSelector
{
	public GClass467(BotOwner owner)
		: base(owner)
	{
	}

	public override void FindItemsInSlots(bool anyway)
	{
		base.FindItemsInSlots(anyway);
		if (BotSettingsRepoClass.IsInfected(BotOwner_0.Profile.Info.Settings.Role))
		{
			if (method_1())
			{
				base.CanChangeToMeleeWeapons = true;
				CanChangeToSupportWeapons = false;
				CanChangeToSecondWeapons_1 = false;
			}
			else
			{
				base.CanChangeToMeleeWeapons = false;
				CanChangeToSupportWeapons = true;
				CanChangeToSecondWeapons_1 = true;
			}
		}
	}

	public bool method_1()
	{
		GClass98.EZombieMode zombieModeByDifficulty = GClass324.GetZombieModeByDifficulty(BotOwner_0.Profile.Info.Settings.BotDifficulty);
		if ((uint)(zombieModeByDifficulty - 1) <= 1u)
		{
			return true;
		}
		return false;
	}

	public override bool TryChangeToSlot(EquipmentSlot slot, bool changeToMain, Action<Result<IHandsController>> callback = null)
	{
		if (method_1())
		{
			return base.TryChangeToSlot(EquipmentSlot.Scabbard, changeToMain: true, callback);
		}
		return base.TryChangeToSlot(EquipmentSlot.Holster, changeToMain: true, callback);
	}

	public override void OnWeaponTaken(Result<IHandsController> x)
	{
		base.IsChanging = false;
		bool allFine = false;
		if (x.Succeed)
		{
			BotOwner_0.WeaponManager.UpdateHandsController(x.Value, out allFine);
		}
		if (BotOwner_0.BotState != EBotState.Active && BotOwner_0.BotState != EBotState.PreActive)
		{
			BotOwner_0.AITaskManager.RegisterDelayedTask(BotOwner_0, 0.5f, base.TakeMainWeapon);
		}
	}
}
