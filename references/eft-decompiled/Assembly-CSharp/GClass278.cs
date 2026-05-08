using System;
using EFT;
using UnityEngine;

public class GClass278 : GClass277
{
	[NonSerialized]
	public float Float_10;

	[NonSerialized]
	public float Float_11;

	public GClass278(BotOwner bot)
		: base(bot)
	{
	}

	public override void UpdateNodeByBrain(GClass28 data)
	{
		BotOwner_0.Sprint(val: false);
		BotOwner_0.StopMove();
		if (data != null && data.FinishTime < Time.time)
		{
			BotOwner_0.Memory.Spotted(byHit: false);
			return;
		}
		method_13();
		method_12();
		if (BotOwner_0.Memory.GoalEnemy != null && BotOwner_0.Memory.GoalEnemy.IsVisible)
		{
			if (BotOwner_0.Settings.FileSettings.Cover.CAN_LAY_TO_COVER && BotOwner_0.BotLay.IsLay)
			{
				if (BotOwner_0.Memory.GoalEnemy.Distance > BotOwner_0.Settings.FileSettings.Cover.CAN_LAY_TO_COVER_DIST_LOOK_TO_ENEMY)
				{
					BotOwner_0.Steering.LookToPoint(BotOwner_0.Memory.GoalEnemy.CurrPosition);
				}
			}
			else
			{
				BotOwner_0.Steering.LookToPoint(BotOwner_0.Memory.GoalEnemy.CurrPosition);
			}
		}
		else if ((BotOwner_0.Memory.GoalEnemy == null || BotOwner_0.Memory.GoalEnemy.IsVisible || !BotOwner_0.Settings.FileSettings.Cover.CAN_LAY_TO_COVER || !BotOwner_0.BotLay.IsLay || !(BotOwner_0.Memory.GoalEnemy.Distance < BotOwner_0.Settings.FileSettings.Cover.CAN_LAY_TO_COVER_DIST_LOOK_TO_ENEMY)) && BotOwner_0.Settings.FileSettings.Cover.CAN_LAY_TO_COVER_DIST_LOOK_TO_ENEMY < 0f)
		{
			Look();
		}
		if (BotOwner_0.Tactic.IsCurTactic(BotsGroup.BotCurrentTactic.Ambush) && base.CustomNavigationPoint_0 != null)
		{
			BotOwner_0.BotLight.TurnOff();
		}
		if (BotOwner_0.Memory.IsInCover)
		{
			if (BotOwner_0.Settings.FileSettings.Cover.CAN_LAY_TO_COVER)
			{
				if (!BotOwner_0.BotLay.TryLay())
				{
					BotOwner_0.SetPose(0.1f);
				}
			}
			else if (BotOwner_0.Settings.FileSettings.Cover.SIT_DOWN_WHEN_HOLDING)
			{
				BotOwner_0.SetPose(0.1f);
			}
		}
		else if (BotOwner_0.Settings.FileSettings.Cover.SIT_DOWN_WHEN_HOLDING)
		{
			BotOwner_0.SetPose(0.1f);
		}
	}

	public void method_12()
	{
		if (!(Float_11 > Time.time))
		{
			Float_11 = Time.time + 2f;
			if (BotOwner_0.WeaponManager.IsMelee)
			{
				BotOwner_0.WeaponManager.Selector.ChangeToMain();
			}
		}
	}

	public virtual void Look()
	{
		if (BotOwner_0.NeutralsCheskData.ClosestsVisible != null && BotOwner_0.NeutralsCheskData.IsInPeriod())
		{
			BotOwner_0.Steering.LookToPoint(BotOwner_0.NeutralsCheskData.ClosestsVisible.Position + Vector3.up);
		}
		else
		{
			BotOwner_0.LookData.SetLookPointByHearing(BotOwner_0.Memory.CurCustomCoverPoint);
		}
	}

	public void method_13()
	{
		if (Float_10 > Time.time || BotOwner_0.WeaponManager.Reload.Reloading)
		{
			return;
		}
		Float_10 = Time.time + 1f;
		float num = (float)BotOwner_0.WeaponManager.Reload.BulletCount / (float)BotOwner_0.WeaponManager.Reload.MaxBulletCount;
		if (num < 0.1f)
		{
			BotOwner_0.WeaponManager.Reload.TryReload();
			return;
		}
		if (num < 0.5f)
		{
			if (BotOwner_0.Memory.HaveEnemy)
			{
				if (Time.time - BotOwner_0.Memory.GoalEnemy.GroupOwner.EnemyLastSeenTimeReal > 5f)
				{
					BotOwner_0.WeaponManager.Reload.TryReload();
				}
			}
			else
			{
				BotOwner_0.WeaponManager.Reload.TryReload();
			}
		}
		if (!BotOwner_0.Memory.HaveEnemy && BotOwner_0.WeaponManager.UnderbarrelLauncherController.NeedToReload())
		{
			BotOwner_0.WeaponManager.UnderbarrelLauncherController.TryReload();
		}
	}
}
