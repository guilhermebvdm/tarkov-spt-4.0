using System;
using System.Runtime.CompilerServices;
using Comfort.Common;
using EFT;
using UnityEngine;

public class GClass129 : BaseLogicLayerSimpleAbstractClass
{
	[NonSerialized]
	public Action Action_2;

	[NonSerialized]
	public bool Bool_4;

	[NonSerialized]
	public int Int_1;

	[NonSerialized]
	public float Float_3;

	[NonSerialized]
	public CustomNavigationPoint CustomNavigationPoint_0;

	public GClass129(BotOwner bot, int priority)
		: base(bot, priority)
	{
		BotOwner_0.Memory.OnGoalEnemyChanged += method_13;
	}

	public override void Dispose()
	{
		BotOwner_0.Memory.OnGoalEnemyChanged -= method_13;
		method_17();
		base.Dispose();
	}

	public void method_13(BotOwner obj)
	{
		method_17();
		if (BotOwner_0.Memory.GoalEnemy != null)
		{
			Player alivePlayerByProfileID = Singleton<GameWorld>.Instance.GetAlivePlayerByProfileID(BotOwner_0.Memory.GoalEnemy.Person.ProfileId);
			if (alivePlayerByProfileID != null)
			{
				Action_2 = alivePlayerByProfileID.OnExitTriggerVisited.Subscribe(method_14);
			}
		}
	}

	public override AICoreActionEndStruct EndRunToCoverZigZag()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy.CanShoot && goalEnemy.IsVisible)
		{
			return new AICoreActionEndStruct("sav4");
		}
		return base.EndRunToCoverZigZag();
	}

	public override AICoreActionEndStruct EndRunToCover()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy.CanShoot && goalEnemy.IsVisible)
		{
			return new AICoreActionEndStruct("sav5");
		}
		return base.EndRunToCover();
	}

	public void method_14()
	{
		if (BotOwner_0.Memory.GoalEnemy != null)
		{
			Int_1 = BotOwner_0.Memory.GoalEnemy.Person.Id;
			Bool_4 = true;
		}
	}

	public override global::AICoreActionResultStruct<BotLogicDecision, GClass26> GetDecision()
	{
		global::AICoreActionResultStruct<BotLogicDecision, GClass26>? aICoreActionResultStruct = InFightLogic();
		if (aICoreActionResultStruct.HasValue)
		{
			return aICoreActionResultStruct.Value;
		}
		if (method_3())
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.dogFight, "BotLogicDec");
		}
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy.CanShoot && goalEnemy.IsVisible)
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.shootFromPlace, "t11c");
		}
		method_16(out var _);
		if (!(goalEnemy.Distance > 35f))
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.runToCover, "gh9v1");
		}
		return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(GClass856.IsTrue100(50f) ? BotLogicDecision.runToCover : BotLogicDecision.runToCoverZigZag, "rtc4");
	}

	public override CustomNavigationPoint FindPoint(CoverSearchData data, Func<CoverSearchData, CustomNavigationPoint> p, bool checkCurrent)
	{
		if (CustomNavigationPoint_0 != null && BotOwner_0.Memory.GoalEnemy.Distance > 30f)
		{
			return CustomNavigationPoint_0;
		}
		return base.FindPoint(data, p, checkCurrent);
	}

	public override AICoreActionEndStruct EndGoToPoint()
	{
		return AICoreActionEndStruct;
	}

	public void method_15()
	{
		if (!(Time.time - Float_3 < 6f))
		{
			Vector3 pos;
			if (BotOwner_0.Memory.HaveEnemy)
			{
				GClass855.NormalizeFastSelf(BotOwner_0.Memory.GoalEnemy.CurrPosition - BotOwner_0.Position);
				pos = BotOwner_0.Memory.GoalEnemy.CurrPosition;
			}
			else
			{
				pos = BotOwner_0.Position;
			}
			CustomNavigationPoint customNavigationPoint = null;
			customNavigationPoint = BotOwner_0.Covers.GetClosestPoint(pos, (GroupPoint x) => !x.IsSpotted && x.IsFreeById(BotOwner_0.Id));
			CustomNavigationPoint_0 = customNavigationPoint;
			Float_3 = Time.time;
		}
	}

	public override bool ShallUseNow()
	{
		if (BotOwner_0.Memory.HaveEnemy && Bool_4)
		{
			return Int_1 == BotOwner_0.Memory.GoalEnemy.Person.Id;
		}
		return false;
	}

	public override AICoreActionEndStruct EndHoldPosition()
	{
		if (method_16(out var aiCoreActionEnd))
		{
			return aiCoreActionEnd;
		}
		if (method_7())
		{
			return new AICoreActionEndStruct("EndHol");
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
				return new AICoreActionEndStruct("CLOSEANDVIS");
			}
		}
		return AICoreActionEndStruct_1;
	}

	public bool method_16(out AICoreActionEndStruct aiCoreActionEnd)
	{
		method_15();
		if (BotOwner_0.Memory.CurCustomCoverPoint == null)
		{
			aiCoreActionEnd = new AICoreActionEndStruct("noCv4");
			return true;
		}
		if (CustomNavigationPoint_0 == null)
		{
			aiCoreActionEnd = AICoreActionEndStruct_1;
			return true;
		}
		if (BotOwner_0.Memory.GoalEnemy == null)
		{
			aiCoreActionEnd = default(AICoreActionEndStruct);
			return false;
		}
		Vector3 currPosition = BotOwner_0.Memory.GoalEnemy.CurrPosition;
		float magnitude = (BotOwner_0.Memory.CurCustomCoverPoint.Position - currPosition).magnitude;
		float magnitude2 = (CustomNavigationPoint_0.Position - currPosition).magnitude;
		float num = Mathf.Abs(magnitude);
		float num2 = Mathf.Abs(magnitude2);
		if (num2 < num)
		{
			BotOwner_0.Memory.Spotted(byHit: false);
			BotOwner_0.Memory.SetCoverPoints(CustomNavigationPoint_0);
			aiCoreActionEnd = new AICoreActionEndStruct($"PrNx:{num2}<{num}");
			return true;
		}
		aiCoreActionEnd = default(AICoreActionEndStruct);
		return false;
	}

	public void method_17()
	{
		if (Action_2 != null)
		{
			Action_2();
		}
		Bool_4 = false;
	}

	public override string Name()
	{
		return "PrtPst";
	}

	[CompilerGenerated]
	public bool method_18(GroupPoint x)
	{
		if (!x.IsSpotted)
		{
			return x.IsFreeById(BotOwner_0.Id);
		}
		return false;
	}
}
