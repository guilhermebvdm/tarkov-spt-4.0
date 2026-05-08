using EFT;
using UnityEngine;

public class GClass610 : GClass609
{
	public override Vector3 ShootStartPos => BotOwner_0.BotBtrData.TurretMachineGunShootPoint;

	public override void UpdateShootPosition(Vector3 weaponRootPoint)
	{
	}

	public GClass610(BotOwner owner)
		: base(owner)
	{
	}
}
