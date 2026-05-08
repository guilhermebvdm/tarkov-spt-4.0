using System;
using System.Collections.Generic;
using EFT;
using EFT.InventoryLogic;

public class AimingManager : GClass429
{
	[NonSerialized]
	public EAimingType Current;

	[NonSerialized]
	public Dictionary<EAimingType, IBotAiming> AimingTypes = new Dictionary<EAimingType, IBotAiming>();

	public IBotAiming CurrentAiming => AimingTypes[Current];

	public AimingManager(BotOwner owner)
		: base(owner)
	{
		AimingTypes.Add(EAimingType.CommonAiming, new BotAimingClass(owner));
		AimingTypes.Add(EAimingType.UnderbarrelLauncher, new GClass605(owner));
		SetAimingType(EAimingType.CommonAiming);
	}

	public void SetAimingType(EAimingType newAimingType)
	{
		Current = newAimingType;
		CurrentAiming.Activate();
	}

	public void SetWeapon(Weapon weapon)
	{
		CurrentAiming.SetWeapon(weapon);
	}

	public void ManualUpdate()
	{
		CurrentAiming.ManualUpdate();
	}

	public void NodeUpdate()
	{
		CurrentAiming.NodeUpdate();
	}
}
