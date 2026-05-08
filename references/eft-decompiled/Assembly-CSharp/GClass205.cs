using System;
using System.Runtime.CompilerServices;
using EFT;
using UnityEngine;

public class GClass205 : GClass200<GClass26>
{
	[Serializable]
	[CompilerGenerated]
	public class Class115
	{
		public static readonly Class115 class115_0 = new Class115();

		public static Action action_0;

		public void method_0()
		{
		}
	}

	[CompilerGenerated]
	public class Class116
	{
		public GClass205 gclass205_0;

		public float recalcTime;

		public bool withShoot;

		public void method_0(CustomNavigationPoint navigationPoint)
		{
			gclass205_0.BotOwner_0.BotTalk.TrySay(EPhraseTrigger.OnFight, withGroupDelay: true);
			recalcTime = Time.time;
			if (recalcTime - gclass205_0.Float_0 < 2f && gclass205_0.BotOwner_0.Memory.CurCustomCoverPoint != null)
			{
				if (!gclass205_0.BotOwner_0.Memory.CurCustomCoverPoint.CanIShootToEnemy && withShoot)
				{
					gclass205_0.Int_0++;
					if (gclass205_0.Int_0 > gclass205_0.BotOwner_0.Settings.FileSettings.Shoot.CAN_SHOOTS_TIME_TO_AMBUSH)
					{
						gclass205_0.BotOwner_0.Tactic.SetTactic(BotsGroup.BotCurrentTactic.Ambush, shallAutoReturnToAttack: true, 2f);
					}
				}
			}
			else
			{
				gclass205_0.Int_0 = 0;
			}
			gclass205_0.Float_0 = recalcTime;
			gclass205_0.BotOwner_0.Memory.SetCoverPoints(navigationPoint);
			gclass205_0.BotOwner_0.GoToPoint(navigationPoint);
		}
	}

	public float DangerTime = 1.5f;

	[NonSerialized]
	public GClass178 Gclass178_0;

	[NonSerialized]
	public BifacialTransform BifacialTransform_0;

	[NonSerialized]
	public BifacialTransform BifacialTransform_1;

	[NonSerialized]
	public float Float_0;

	[NonSerialized]
	public int Int_0;

	[NonSerialized]
	public Vector3 Vector3_0;

	[NonSerialized]
	public float Float_1;

	[NonSerialized]
	public float Float_2;

	public GClass205(BotOwner bot)
		: base(bot)
	{
		BifacialTransform_0 = BotOwner_0.Transform;
		BifacialTransform_1 = BotOwner_0.GetPlayer.PlayerBones.WeaponRoot;
		Gclass178_0 = GetAiming(bot);
	}

	public override void UpdateNodeByBrain(GClass26 data)
	{
		method_0();
		BotOwner_0.SetTargetMoveSpeed(1f);
		BotOwner_0.Sprint(val: false);
		BotOwner_0.SetPose(1f);
		float recalcTime = 0f;
		method_5();
		bool canRunNoAmmo;
		Vector3 centerPos = ((!BotOwner_0.Memory.IsInCover || BotOwner_0.LookSensor.EnoughDistToShoot(out canRunNoAmmo)) ? BotOwner_0.Transform.position : ((BifacialTransform_0.position + BotOwner_0.Memory.GoalEnemy.EnemyLastPosition) / 2f));
		bool withShoot = BotOwner_0.Tactic.IsCurTactic(BotsGroup.BotCurrentTactic.Attack) || BotOwner_0.Tactic.IsCurTactic(BotsGroup.BotCurrentTactic.Protect);
		CoverShootType coverShootType = ((!withShoot) ? CoverShootType.hide : CoverShootType.shoot);
		CoverSearchType searchType = BotOwner_0.Tactic.SubTactic.SearchTypeAttackMoving(coverShootType);
		if (Float_2 < Time.time)
		{
			Float_2 = Time.time + 2f;
			BotOwner_0.BotAttackManager.TryPointGetting(centerPos, coverShootType, LocalBotSettingsProviderClass.Core.START_DIST_TO_COV, searchType, BotOwner_0.CurrentEnemyTargetPosition(sensPosition: true), delegate(CustomNavigationPoint navigationPoint)
			{
				BotOwner_0.BotTalk.TrySay(EPhraseTrigger.OnFight, withGroupDelay: true);
				recalcTime = Time.time;
				if (recalcTime - Float_0 < 2f && BotOwner_0.Memory.CurCustomCoverPoint != null)
				{
					if (!BotOwner_0.Memory.CurCustomCoverPoint.CanIShootToEnemy && withShoot)
					{
						Int_0++;
						if (Int_0 > BotOwner_0.Settings.FileSettings.Shoot.CAN_SHOOTS_TIME_TO_AMBUSH)
						{
							BotOwner_0.Tactic.SetTactic(BotsGroup.BotCurrentTactic.Ambush, shallAutoReturnToAttack: true, 2f);
						}
					}
				}
				else
				{
					Int_0 = 0;
				}
				Float_0 = recalcTime;
				BotOwner_0.Memory.SetCoverPoints(navigationPoint);
				BotOwner_0.GoToPoint(navigationPoint);
			}, delegate
			{
			});
		}
		if (BotOwner_0.Mover.IsComeTo(BotOwner_0.Settings.FileSettings.Move.REACH_DIST, onCover: true) && !BotOwner_0.Memory.ComeToPoint())
		{
			BotOwner_0.BotAttackManager.UpdateNextTick();
		}
		BotOwner_0.Mover.SetPose(1f);
		BotOwner_0.WeaponManager.TryReloadWeaponOrUnderbarrelLauncher();
		if (!BotOwner_0.HasPathAndNotComplete && BotOwner_0.Memory.CurCustomCoverPoint != null && BotOwner_0.Memory.CurCustomCoverPoint.Owner == BotOwner_0.Id)
		{
			if ((BotOwner_0.Position - BotOwner_0.Memory.CurCustomCoverPoint.Position).sqrMagnitude > 1f)
			{
				BotOwner_0.GoToPoint(BotOwner_0.Memory.CurCustomCoverPoint);
			}
			else if (!BotOwner_0.Memory.ComeToPoint())
			{
				BotOwner_0.BotAttackManager.UpdateNextTick();
			}
		}
		AimingAndShoot(data);
	}

	public virtual GClass178 GetAiming(BotOwner bot)
	{
		return new GClass183(bot);
	}

	public virtual void AimingAndShoot(GClass26 data)
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy != null && goalEnemy.CanShoot && goalEnemy.IsVisible)
		{
			if (BotOwner_0.WeaponManager.UnderbarrelLauncherController.CanSwitchInFight(BotOwner_0))
			{
				BotOwner_0.WeaponManager.UnderbarrelLauncherController.TryEnable();
			}
			Gclass178_0.UpdateNodeByBrain(data as GClass27);
		}
		else
		{
			BotOwner_0.LookData.SetLookPointByHearing();
		}
	}

	public void method_5()
	{
		if (Float_1 < Time.time)
		{
			Float_1 = Time.time + BotOwner_0.Settings.FileSettings.Move.UPDATE_TIME_RECAL_WAY;
		}
	}
}
