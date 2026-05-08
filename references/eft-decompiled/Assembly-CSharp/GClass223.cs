using System;
using EFT;
using UnityEngine;

public class GClass223 : GClass222
{
	[NonSerialized]
	public GClass178 Gclass178_0;

	public GClass223(BotOwner bot)
		: base(bot)
	{
		Gclass178_0 = new GClass183(BotOwner_0);
	}

	public override void UpdateNodeByBrain(GClass26 data)
	{
		method_0();
		BotOwner_0.Sprint(val: false);
		NotMovingCheck();
		method_6();
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		bool flag = false;
		if (goalEnemy.IsVisible && goalEnemy.CanShoot)
		{
			flag = true;
			Gclass178_0.UpdateNodeByBrain(data as GClass27);
		}
		else if (!goalEnemy.IsVisible && Time.time - goalEnemy.GroupInfo.EnemyLastSeenTimeSense >= 5f)
		{
			BotOwner_0.LookData.SetLookPointByHearing();
		}
		else
		{
			BotOwner_0.Steering.LookToPoint(goalEnemy.CurrPosition);
		}
		if (BotOwner_0.Mover.HasPathAndNoComplete)
		{
			BotOwner_0.SetTargetMoveSpeed(1f);
			BotOwner_0.SetPose(1f);
			bool flag2 = BotOwner_0.Mover.IsComeTo(BotOwner_0.Settings.FileSettings.Move.REACH_DIST, onCover: false);
			if (BotOwner_0.WeaponManager.UnderbarrelLauncherController.IsActive)
			{
				if (BotOwner_0.WeaponManager.UnderbarrelLauncherController.NeedToReload())
				{
					BotOwner_0.WeaponManager.UnderbarrelLauncherController.TryReload();
				}
			}
			else if (!BotOwner_0.WeaponManager.HaveBullets)
			{
				BotOwner_0.WeaponManager.Reload.TryReload();
			}
			if ((!flag2 || !goalEnemy.IsVisible || !goalEnemy.CanShoot) && flag2)
			{
				method_7();
			}
		}
		else
		{
			if (!flag)
			{
				BotOwner_0.LookData.SetLookPointByHearing();
			}
			BotOwner_0.StopMove();
			BotOwner_0.SetPose(0f);
		}
	}

	public void method_7()
	{
		BotOwner_0.StopMove();
	}
}
