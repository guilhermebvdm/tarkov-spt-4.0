using System;
using EFT;
using EFT.InventoryLogic;
using UnityEngine;

public class BotMedecine : GClass429
{
	public static readonly EquipmentSlot[] secureSlots = new EquipmentSlot[1] { EquipmentSlot.SecuredContainer };

	public static readonly EquipmentSlot[] anySlots = new EquipmentSlot[7]
	{
		EquipmentSlot.Pockets,
		EquipmentSlot.TacticalVest,
		EquipmentSlot.Backpack,
		EquipmentSlot.SecuredContainer,
		EquipmentSlot.FirstPrimaryWeapon,
		EquipmentSlot.SecondPrimaryWeapon,
		EquipmentSlot.Holster
	};

	public static readonly EquipmentSlot[] pockets = new EquipmentSlot[1] { EquipmentSlot.Pockets };

	public static readonly EquipmentSlot[] allVisible = new EquipmentSlot[3]
	{
		EquipmentSlot.Pockets,
		EquipmentSlot.TacticalVest,
		EquipmentSlot.Backpack
	};

	public BotFirstAidClass FirstAid;

	public GClass491 Stimulators;

	public GClass489 SurgicalKit;

	[NonSerialized]
	public float RecheckPeriod;

	[field: NonSerialized]
	public bool Using { get; set; }

	public BotMedecine(BotOwner owner)
		: base(owner)
	{
		Stimulators = new GClass491(owner, method_0);
		switch (owner.Profile.Info.Settings.Role)
		{
		case WildSpawnType.bossZryachiy:
		case WildSpawnType.followerZryachiy:
			FirstAid = new GClass488(owner, method_0);
			SurgicalKit = new GClass489(owner, method_0);
			break;
		default:
			FirstAid = new BotFirstAidClass(owner, method_0);
			SurgicalKit = new GClass489(owner, method_0);
			break;
		case WildSpawnType.followerSanitar:
		case WildSpawnType.bossSanitar:
			FirstAid = new GClass487(owner, method_0);
			SurgicalKit = new GClass490(owner, method_0);
			break;
		}
	}

	public void Activate()
	{
		FirstAid.Activate();
		Stimulators.Activate();
		SurgicalKit.Activate();
	}

	public void ManualUpdate()
	{
		if (!(RecheckPeriod > Time.time))
		{
			RecheckPeriod = Time.time + 20f;
			FirstAid.Refresh();
			FirstAid.CheckParts();
			SurgicalKit.Refresh();
			SurgicalKit.FindDamagedPart();
		}
	}

	public void RefreshCurMeds()
	{
		FirstAid.Refresh();
		Stimulators.Refresh();
		SurgicalKit.Refresh();
	}

	public void GetDamaged()
	{
		FirstAid.GetDamaged();
		SurgicalKit.GetDamage();
	}

	public void method_0(bool obj)
	{
		Using = obj;
	}

	public void Dispose()
	{
		FirstAid.Dispose();
		Stimulators.Dispose();
		SurgicalKit.Dispose();
	}
}
