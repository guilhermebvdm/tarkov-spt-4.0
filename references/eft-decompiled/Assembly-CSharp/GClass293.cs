using System;
using EFT;
using UnityEngine;

public class GClass293 : GClass177<GClass26>
{
	[NonSerialized]
	public float Float_0;

	[NonSerialized]
	public Transform Transform_0;

	[NonSerialized]
	public bool Bool_0;

	public GClass293(BotOwner bot)
		: base(bot)
	{
	}

	public override void UpdateNodeByBrain(GClass26 data)
	{
		if (!Bool_0)
		{
			method_0();
		}
		else if (!BotOwner_0.WeaponManager.Grenades.ReadyToThrow && Float_0 < Time.time)
		{
			Float_0 = Time.time + 5f;
			if (Transform_0 != null)
			{
				BotOwner_0.WeaponManager.Grenades.CanThrowGrenade(BotOwner_0.Memory.CurCustomCoverPoint.FirePosition + Vector3.up, Transform_0.position + Vector3.up * 1.4f);
			}
		}
	}

	public void method_0()
	{
		if (GameObject.Find("ShootPractice") != null)
		{
			GameObject gameObject = GameObject.Find("ShootPracticeTarget");
			Transform_0 = gameObject.transform;
		}
		if (Transform_0 == null && BotOwner_0.Memory.GoalEnemy != null)
		{
			Transform_0 = BotOwner_0.Transform.Original;
		}
		if (Transform_0 != null)
		{
			Bool_0 = true;
		}
	}
}
