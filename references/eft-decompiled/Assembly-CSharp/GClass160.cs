using System;
using System.Runtime.CompilerServices;
using EFT;
using UnityEngine;

public class GClass160(BotOwner bot, int priority) : GClass157(bot, priority)
{
	[NonSerialized]
	public const float Float_5 = 10000f;

	[NonSerialized]
	public const float Float_6 = 4f;

	[NonSerialized]
	public float Float_7;

	[NonSerialized]
	public bool Bool_5;

	[NonSerialized]
	public float Float_8;

	[NonSerialized]
	public float Float_9 = float.MaxValue;

	public override string Name()
	{
		return "TagillaAmbush";
	}

	public override bool ShallUseNow()
	{
		if (BotOwner_0.BotFollower.HaveBoss)
		{
			return false;
		}
		if (!method_15())
		{
			method_14();
			return false;
		}
		if (BotOwner_0.Memory.HaveEnemy)
		{
			Float_9 = Time.time + 4f;
		}
		bool flag;
		if (!(flag = BotOwner_0.Memory.GoalTarget.HaveMainTarget() && !BotOwner_0.Memory.HaveEnemy && Time.time < Float_9) && Float_7 + 4f < Time.time)
		{
			Bool_5 = false;
			foreach (BotOwner botOwner in BotOwner_0.BotsController.Bots.BotOwners)
			{
				if (botOwner != BotOwner_0 && botOwner.Memory.HaveEnemy && !(botOwner.SDistTo(BotOwner_0.Position) <= 10000f))
				{
					Bool_5 = true;
					BotOwner_0.EnemiesController.SetSameEnemy(botOwner.Memory.GoalEnemy);
					break;
				}
			}
			Float_7 = Time.time;
		}
		if (!flag)
		{
			return Bool_5;
		}
		return true;
	}

	public void method_14()
	{
		if (Float_8 < Time.time)
		{
			Float_8 = Time.time + 3f;
			BotOwner_0.BotAttackManager.TryPointGetting(CoverShootType.hide, CoverSearchType.distToBot, null, 0f, delegate(CustomNavigationPoint navigationPoint)
			{
				BotOwner_0.Ambush.SetCoverAtMiddle(navigationPoint);
			});
		}
	}

	public bool method_15()
	{
		return BotOwner_0.Ambush.HaveCover();
	}

	public override global::AICoreActionResultStruct<BotLogicDecision, GClass26> GetDecision()
	{
		if (BotOwner_0.Memory.GoalEnemy == null)
		{
			BotOwner_0.CallForHelp.FreeSavages(null);
		}
		BotOwner_0.LookData.SetLookPointByHearing();
		if (BotOwner_0.Medecine.FirstAid.Have2Do)
		{
			if (BotOwner_0.Memory.IsInCover)
			{
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.heal, "HealInCover");
			}
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.goToCoverPoint, "GoCoverForH");
		}
		if (BotOwner_0.Memory.GoalEnemy == null)
		{
			BotOwner_0.Memory.GoalEnemy = BotOwner_0.EnemyChooser.FindDangerEnemy();
		}
		if (BotOwner_0.DecisionQueue.Count > 0)
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotOwner_0.DecisionQueue.Peek().Decision, "DecisionQue");
		}
		if (BotOwner_0.SmokeGrenade.ShallShoot())
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.shootToSmoke, "SmokeS");
		}
		if (!BotOwner_0.Memory.AttackImmediately && BotOwner_0.Memory.LastEnemy != null && Time.time - BotOwner_0.Memory.LastEnemy.TimeLastSeen < BotOwner_0.Settings.FileSettings.Cover.TIME_TO_MOVE_TO_COVER)
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BaseLogicLayerAbstractClass.HoldOrCover(BotOwner_0), "HoldOrCover");
		}
		if (BotOwner_0.Memory.IsInCover)
		{
			return method_8();
		}
		return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.runToCover, "RunToCover");
	}

	public override CustomNavigationPoint FindPoint(CoverSearchData data, Func<CoverSearchData, CustomNavigationPoint> p, bool checkCurrent)
	{
		if (BotOwner_0.WeaponManager.Reload.Reloading)
		{
			data = new CoverSearchData(data.CenterPos, data.Bot, CoverShootType.hide, data.MaxDistSqr, 0f, CoverSearchType.distToBot, data.Shoot2Point, null, null, data.CheckShootHide, new CoverSearchDefenceDataClass(BotOwner_0.Settings.FileSettings.Cover.MIN_DEFENCE_LEVEL));
			return base.FindPoint(data, p, checkCurrent);
		}
		if (BotOwner_0.Ambush.TryGetAmbushPoint(out var ambushPoint))
		{
			return ambushPoint;
		}
		data = BotOwner_0.Ambush.GetCoverSearchData();
		CustomNavigationPoint customNavigationPoint = base.FindPoint(data, p, checkCurrent);
		BotOwner_0.Ambush.SetCoverAtMiddle(customNavigationPoint);
		return customNavigationPoint;
	}

	public override AICoreActionEndStruct EndRunToCover()
	{
		if (Gclass459_0.WantMeleeAssault())
		{
			return new AICoreActionEndStruct("WantMeleeAs");
		}
		if (BotOwner_0.Memory.IsInCover)
		{
			Nullable_0 = method_8().Action;
			return new AICoreActionEndStruct("IsInCover");
		}
		return base.EndRunToCover();
	}

	public override AICoreActionEndStruct EndHoldPosition()
	{
		if (method_7())
		{
			return new AICoreActionEndStruct("EndHoldTime");
		}
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (!BotOwner_0.Memory.IsInCover)
		{
			return new AICoreActionEndStruct("IsInCover");
		}
		if (goalEnemy != null)
		{
			if (goalEnemy.IsVisible && goalEnemy.CanShoot)
			{
				return new AICoreActionEndStruct("CanShoot");
			}
			if (goalEnemy.IsVisible && goalEnemy.Distance < BotOwner_0.Settings.FileSettings.Cover.END_HOLD_IF_ENEMY_CLOSE_AND_VISIBLE)
			{
				return new AICoreActionEndStruct("ENEMYCLOSE");
			}
		}
		return AICoreActionEndStruct_1;
	}

	[CompilerGenerated]
	public void method_16(CustomNavigationPoint navigationPoint)
	{
		BotOwner_0.Ambush.SetCoverAtMiddle(navigationPoint);
	}
}
