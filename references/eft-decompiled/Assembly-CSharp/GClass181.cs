using System;
using EFT;
using UnityEngine;

public class GClass181 : GClass178
{
	[NonSerialized]
	public const float Float_1 = 4f;

	public GClass181(BotOwner bot)
		: base(bot)
	{
	}

	public override Vector3? GetTarget()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy != null && Time.time - goalEnemy.TimeLastSeen < 4f)
		{
			if (goalEnemy.CanShoot && goalEnemy.IsVisible)
			{
				if (goalEnemy.Distance < BotOwner_0.Settings.FileSettings.Aiming.DIST_TO_SHOOT_TO_CENTER)
				{
					return goalEnemy.GetBodyPartPosition();
				}
				return goalEnemy.GetPartToShoot();
			}
			return BotOwner_0.WeaponManager.UnderbarrelLauncherController.IsActive ? goalEnemy.EnemyLastPosition : (goalEnemy.EnemyLastPosition + Vector3.up * BotOwner_0.Settings.FileSettings.Aiming.DANGER_UP_POINT);
		}
		Vector3? result = null;
		if (BotOwner_0.Memory.LastEnemy != null && Time.time - BotOwner_0.Memory.LastEnemy.TimeLastSeen < 4f)
		{
			result = BotOwner_0.Memory.LastEnemy.CurrPosition + Vector3.up * BotOwner_0.Settings.FileSettings.Aiming.DANGER_UP_POINT;
		}
		return result;
	}
}
