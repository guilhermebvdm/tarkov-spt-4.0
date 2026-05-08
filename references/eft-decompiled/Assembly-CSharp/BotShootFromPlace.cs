using System;
using EFT;
using UnityEngine;

public class BotShootFromPlace : GClass429
{
	[NonSerialized]
	public float NextPosibleCheck;

	[field: NonSerialized]
	public bool CanShootSit { get; set; }

	[field: NonSerialized]
	public bool CanShootLay { get; set; }

	public BotShootFromPlace(BotOwner owner)
		: base(owner)
	{
	}

	public void CheckTarget(Vector3 target)
	{
		if (NextPosibleCheck < Time.time)
		{
			NextPosibleCheck = Time.time + 2f;
			Vector3 vector = BotOwner_0.Position + Vector3.up * 0.6f;
			CanShootSit = GClass369.CanShootToTarget(new ShootPointClass(target), vector, BotOwner_0.LookSensor.Mask);
			if (CanShootSit)
			{
				CanShootSit = !BotOwner_0.ShootData.CheckFriendlyFire(vector, target);
			}
			CanShootLay = BotOwner_0.BotLay.CanShootPos(target, withCheckShoot: true, withFriendlyFire: true);
		}
	}
}
