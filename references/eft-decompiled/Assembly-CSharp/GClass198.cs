using System;
using EFT;
using UnityEngine;

public class GClass198 : GClass177<GClass26>
{
	[NonSerialized]
	public GClass178 Gclass178_0;

	[NonSerialized]
	public float Float_0;

	[NonSerialized]
	public float Float_1;

	[NonSerialized]
	public bool Bool_0 = true;

	[NonSerialized]
	public float Float_2;

	public GClass198(BotOwner bot)
		: base(bot)
	{
		Gclass178_0 = new GClass183(bot);
	}

	public override void UpdateNodeByBrain(GClass26 data)
	{
		BotOwner_0.StopMove();
		if (BotOwner_0.GetPlayer.MovementContext.IsInPronePose && BotOwner_0.GetPlayer.PoseLevel <= 0f)
		{
			Bool_0 = true;
		}
		else
		{
			if (Float_0 < Time.time)
			{
				Float_0 = Time.time + 2f;
				BotOwner_0.BotLay.TryLay();
			}
			if (Float_1 < Time.time)
			{
				Bool_0 = !Bool_0;
				Float_1 = Time.time + 6f;
			}
		}
		if (BotOwner_0.WeaponManager.UnderbarrelLauncherController.IsActive)
		{
			if (BotOwner_0.WeaponManager.UnderbarrelLauncherController.NeedToReload())
			{
				BotOwner_0.WeaponManager.UnderbarrelLauncherController.TryReload();
				return;
			}
		}
		else
		{
			if (BotOwner_0.WeaponManager.UnderbarrelLauncherController.CanSwitchInFight(BotOwner_0))
			{
				BotOwner_0.WeaponManager.UnderbarrelLauncherController.TryEnable();
			}
			if (!BotOwner_0.WeaponManager.HaveBullets)
			{
				BotOwner_0.WeaponManager.Reload.TryReload();
				return;
			}
		}
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy == null)
		{
			BotOwner_0.DangerPointsData.CheckDangerPoints(4f);
			method_0();
		}
		else if (goalEnemy.IsVisible)
		{
			if (goalEnemy.CanShoot)
			{
				Gclass178_0.UpdateNodeByBrain(data as GClass27);
			}
			else if (Float_2 < Time.time && Bool_0)
			{
				Float_2 = Time.time + 2f;
				BotOwner_0.Steering.LookToDirection(goalEnemy.Direction);
			}
		}
		else
		{
			method_0();
		}
	}

	public void method_0()
	{
		if (Bool_0)
		{
			BotOwner_0.LookData.SetLookPointByHearing();
		}
	}
}
