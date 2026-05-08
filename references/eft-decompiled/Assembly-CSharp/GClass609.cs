using System;
using EFT;
using UnityEngine;

public class GClass609
{
	[NonSerialized]
	public BotOwner BotOwner_0;

	[NonSerialized]
	public Vector3 Vector3_0;

	public virtual Vector3 ShootStartPos => Vector3_0;

	public GClass609(BotOwner owner)
	{
		BotOwner_0 = owner;
	}

	public virtual void UpdateShootPosition(Vector3 weaponRootPoint)
	{
		Vector3 vector = GClass855.NormalizeFast(BotOwner_0.LookDirection);
		Vector3_0 = weaponRootPoint - vector * BotOwner_0.Settings.FileSettings.Aiming.WEAPON_ROOT_OFFSET;
	}
}
