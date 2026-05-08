using System;
using EFT;
using UnityEngine;

public class GClass237 : GClass236
{
	public float DangerTime = 1.5f;

	[NonSerialized]
	public GClass178 Gclass178_0;

	[NonSerialized]
	public float Float_14;

	public override float CHANCE_TO_COVER_SPRINT_ON_MIN_DIST_OR_LOWER => 0f;

	public override float CHANCE_TO_COVER_SPRINT_ON_MAX_DIST_OR_HIGHER => 0f;

	public virtual float CHANCE_TO_SPRINT_ON_MIN_DIST_OR_LOWER => 0f;

	public virtual float CHANCE_TO_SPRINT_ON_MAX_DIST_OR_HIGHER => 0f;

	public GClass237(BotOwner bot)
		: base(bot, 80f, 60f)
	{
		Gclass178_0 = GetAiming(bot);
	}

	public override void UpdateNodeByBrain(GClass26 data)
	{
		base.UpdateNodeByBrain(data);
		method_18();
		AimingAndShoot(data);
	}

	public override void LookSimple()
	{
		BotOwner_0.LookData.SetLookPointByHearing();
	}

	public virtual GClass178 GetAiming(BotOwner bot)
	{
		return new GClass183(bot);
	}

	public virtual void AimingAndShoot(GClass26 data)
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy != null)
		{
			if (goalEnemy.CanShoot && (goalEnemy.IsVisible || Time.time - goalEnemy.PersonalLastSeenTime < 5f))
			{
				Gclass178_0.UpdateNodeByBrain(data as GClass27);
			}
			else if (Time.time - goalEnemy.PersonalLastSeenTime < 25f)
			{
				BotOwner_0.Steering.LookToDirection(goalEnemy.Direction);
			}
		}
	}

	public void method_18()
	{
		if (Float_14 < Time.time)
		{
			Float_14 = Time.time + BotOwner_0.Settings.FileSettings.Move.UPDATE_TIME_RECAL_WAY;
		}
	}
}
