using System;
using EFT;
using UnityEngine;

public class GClass41 : GClass38
{
	[NonSerialized]
	public float Float_4;

	[NonSerialized]
	public bool Bool_5;

	[NonSerialized]
	public float Float_5 = 20f;

	public float Single_1 => BotOwner_0.Settings.FileSettings.Mind.PUSH_AND_SUPPRESS_HIDE;

	public float Single_2 => BotOwner_0.Settings.FileSettings.Mind.PUSH_AND_SUPPRESS_PUSH;

	public GClass41(BotOwner bot, int priority, float attackDist = 20f)
		: base(bot, priority)
	{
		Float_5 = attackDist;
	}

	public override global::AICoreActionResultStruct<BotLogicDecision, GClass26> GetDecision()
	{
		Gclass25_0.Update();
		if (Bool_4)
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BaseLogicLayerAbstractClass.HoldOrCoverRun(BotOwner_0), "toofar");
		}
		global::AICoreActionResultStruct<BotLogicDecision, GClass26>? aICoreActionResultStruct = InFightLogic();
		if (aICoreActionResultStruct.HasValue)
		{
			return aICoreActionResultStruct.Value;
		}
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy.IsVisible && goalEnemy.CanShoot)
		{
			if (BotOwner_0.WeaponManager.HaveBullets)
			{
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.shootFromPlace, "nds");
			}
			if (!BotOwner_0.Memory.IsInCover)
			{
				BotOwner_0.WeaponManager.Reload.Reload();
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.runToCover, "t2relo");
			}
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.shootFromPlace, "n6l2");
		}
		float num = Time.time - Float_4;
		float num2 = Time.time - goalEnemy.PersonalLastSeenTime;
		if (Bool_5 && num > 10f && num2 < 15f)
		{
			Float_4 = Time.time;
			Vector3 enemyLastPositionReal = goalEnemy.EnemyLastPositionReal;
			if (!BotOwner_0.WeaponManager.UnderbarrelLauncherController.IsActive)
			{
				enemyLastPositionReal += new Vector3(0f, 1.5f, 0f);
			}
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.suppressFire, "supFire", new GClass27(enemyLastPositionReal));
		}
		if (goalEnemy.Distance > Single_1)
		{
			CustomNavigationPoint customNavigationPoint = BotOwner_0.Covers.FindHidePoint(BotOwner_0.Position, 0f);
			if (customNavigationPoint != null)
			{
				if (!BotOwner_0.Memory.IsInCover)
				{
					return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.runToCover, "hd68", new GClass31(customNavigationPoint));
				}
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.holdPosition, "hj7");
			}
		}
		if (goalEnemy.Distance < Single_2)
		{
			if (GClass855.IsOnNavMesh(goalEnemy.CurrPosition, 3f))
			{
				if (BotOwner_0.WeaponManager.HaveBullets && !BotOwner_0.WeaponManager.Reload.Reloading)
				{
					return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.runToEnemy, "tcls");
				}
				BotOwner_0.WeaponManager.Reload.TryReload();
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.holdPosition, "w2nec", new GClass28(2f, null));
			}
			if (BotOwner_0.Memory.IsInCover)
			{
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.holdPosition, "w2nec", new GClass28(2f, null));
			}
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.attackMovingFlank, "findAnt");
		}
		if (!BotOwner_0.Memory.IsInCover)
		{
			if (goalEnemy.Distance < 50f)
			{
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.attackMovingFlank, "gh65");
			}
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.runToCover, "rtc75");
		}
		return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.holdPosition, "w2sec", new GClass28(1f, null));
	}

	public override AICoreActionEndStruct EndHoldPosition()
	{
		if (!BotOwner_0.Memory.IsInCover)
		{
			return new AICoreActionEndStruct("IsInCover");
		}
		Gclass25_0.Update();
		if (Bool_4)
		{
			return AICoreActionEndStruct_1;
		}
		if (method_7())
		{
			return new AICoreActionEndStruct("EndHol");
		}
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy.IsVisible && goalEnemy.CanShoot)
		{
			return new AICoreActionEndStruct("CanShoot");
		}
		if (goalEnemy.IsVisible && goalEnemy.Distance < BotOwner_0.Settings.FileSettings.Cover.END_HOLD_IF_ENEMY_CLOSE_AND_VISIBLE)
		{
			return new AICoreActionEndStruct("CLOSEANDVIS");
		}
		if (goalEnemy.Distance < Float_5)
		{
			BotOwner_0.BotsGroup.ReportAboutEnemy(goalEnemy.Person, EEnemyPartVisibleType.Visible, BotOwner_0);
			BotOwner_0.Memory.Spotted(byHit: false);
		}
		return AICoreActionEndStruct_1;
	}

	public override AICoreActionEndStruct EndShootFromPlace()
	{
		return base.EndShootFromPlace();
	}

	public override AICoreActionEndStruct EndRunToCover()
	{
		if (BotOwner_0.Memory.IsInCover)
		{
			return new AICoreActionEndStruct("InCover");
		}
		if (!BotOwner_0.CanSprintPlayer)
		{
			return new AICoreActionEndStruct("CanSprintPl");
		}
		if (method_3())
		{
			return new AICoreActionEndStruct("StartD");
		}
		if (BotOwner_0.Memory.CurCustomCoverPoint != null && BotOwner_0.Memory.CurCustomCoverPoint.IsSpotted)
		{
			return new AICoreActionEndStruct("IsSpotted");
		}
		return AICoreActionEndStruct_1;
	}

	public override AICoreActionEndStruct EndSuppressFire()
	{
		if (Time.time - Float_4 > 6f)
		{
			return new AICoreActionEndStruct("more6");
		}
		return base.EndSuppressFire();
	}

	public override AICoreActionEndStruct EndAttackMovingFlank()
	{
		if (method_3())
		{
			return new AICoreActionEndStruct("lj32");
		}
		if (BotOwner_0.Memory.IsInCover)
		{
			return new AICoreActionEndStruct("h32");
		}
		return AICoreActionEndStruct_1;
	}

	public override AICoreActionEndStruct EndRunToEnemy()
	{
		return base.EndRunToEnemy();
	}

	public override bool ShallUseNow()
	{
		return BotOwner_0.Memory.HaveEnemy;
	}

	public override string Name()
	{
		return "PushAndSup";
	}
}
