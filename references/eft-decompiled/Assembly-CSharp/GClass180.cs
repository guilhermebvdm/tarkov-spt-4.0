using System;
using EFT;
using UnityEngine;

public class GClass180 : GClass178
{
	[NonSerialized]
	public AnimationCurve AnimationCurve_0;

	public GClass180(BotOwner bot)
		: base(bot)
	{
		AnimationCurve_0 = BotCurvSettings.Instance.AgsAimUp;
	}

	public override Vector3? GetTarget()
	{
		Vector3? result = null;
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy == null)
		{
			return null;
		}
		if (goalEnemy.CanShoot && goalEnemy.IsVisible)
		{
			return (!(goalEnemy.Distance < BotOwner_0.Settings.FileSettings.Aiming.DIST_TO_SHOOT_TO_CENTER)) ? goalEnemy.GetPartToShoot() : goalEnemy.GetBodyPartPosition();
		}
		if (BotOwner_0.Memory.LastEnemy != null)
		{
			result = BotOwner_0.Memory.LastEnemy.CurrPosition + Vector3.up * BotOwner_0.Settings.FileSettings.Aiming.DANGER_UP_POINT;
		}
		return result;
	}
}
