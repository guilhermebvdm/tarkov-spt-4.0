using System;
using System.Runtime.CompilerServices;
using EFT;
using UnityEngine;

public class GClass130 : BaseLogicLayerSimpleAbstractClass
{
	[NonSerialized]
	public bool Bool_4;

	[NonSerialized]
	public CustomNavigationPoint CustomNavigationPoint_0;

	[NonSerialized]
	public float Float_3;

	[NonSerialized]
	public float Float_4;

	[NonSerialized]
	public float Float_5;

	[NonSerialized]
	public float Float_6 = 20f;

	[NonSerialized]
	public const float Float_7 = 2500f;

	[NonSerialized]
	public const float Float_8 = 60f;

	[NonSerialized]
	public const float Float_9 = 50f;

	[NonSerialized]
	public const float Float_10 = 70f;

	[NonSerialized]
	public const float Float_11 = 90f;

	public GClass130(BotOwner bot, int priority)
		: base(bot, priority)
	{
		BotOwner_0.MinesData.OnMinesStartCache += method_19;
		BotOwner_0.MinesData.OnMinesCacheCompleted += method_18;
	}

	public override global::AICoreActionResultStruct<BotLogicDecision, GClass26> GetDecision()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (BotOwner_0.Medecine.FirstAid.Have2Do && BotOwner_0.Memory.IsInCover)
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.heal, "heal");
		}
		if (!BotOwner_0.Memory.IsInCover)
		{
			method_14();
			if (CustomNavigationPoint_0 != null)
			{
				BotOwner_0.Memory.Spotted(byHit: false);
				BotOwner_0.Memory.SetCoverPoints(CustomNavigationPoint_0);
				if (method_20())
				{
					return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.holdPosition, "jy6");
				}
				if (goalEnemy.Distance > 70f)
				{
					return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.runToCover, "nt3");
				}
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.moveStealthy, "moveTl");
			}
		}
		else
		{
			if (method_13())
			{
				Float_6 = GClass856.Random(15f, 35f);
				Float_4 = Time.time;
				BotOwner_0.BotTalk.Say(EPhraseTrigger.Provocation, sayImmediately: true);
				BotOwner_0.SuppressShoot.InitToPoint(goalEnemy.CurrPosition + new Vector3(0f, GClass856.Random(0f, 5f), 0f));
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.suppressFire, "prov");
			}
			if (goalEnemy.Distance > 70f || !method_15(BotOwner_0.Position))
			{
				method_14();
				if (CustomNavigationPoint_0 != null)
				{
					BotOwner_0.Memory.Spotted(byHit: false);
					BotOwner_0.Memory.SetCoverPoints(CustomNavigationPoint_0);
					return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.moveStealthy, "moveT2");
				}
				if (method_20())
				{
					return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.holdPosition, "jh8");
				}
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.runToCover, "moveT2");
			}
		}
		return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.holdPosition, "st4");
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
		return AICoreActionEndStruct_1;
	}

	public bool method_13()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy == null)
		{
			return false;
		}
		if (goalEnemy.Distance > 60f)
		{
			return false;
		}
		if ((BotOwner_0.MinesData.LastCacheCenter - goalEnemy.CurrPosition).sqrMagnitude > 2500f)
		{
			return false;
		}
		float num = Time.time - BotOwner_0.Memory.ComeToCoverTime;
		float num2 = Time.time - Float_4;
		if (num > 20f && num2 > Float_6)
		{
			return true;
		}
		return false;
	}

	public override AICoreActionEndStruct EndSuppressFire()
	{
		if (Time.time - Float_4 > 5f)
		{
			Float_4 = Time.time;
			return AICoreActionEndStruct;
		}
		return base.EndSuppressFire();
	}

	public void method_14()
	{
		if (!(Time.time - Float_5 < 6f))
		{
			Vector3 pos;
			if (BotOwner_0.Memory.HaveEnemy)
			{
				Vector3 vector = GClass855.NormalizeFastSelf(BotOwner_0.Memory.GoalEnemy.CurrPosition - BotOwner_0.Position);
				pos = BotOwner_0.Memory.GoalEnemy.CurrPosition - vector * 50f;
			}
			else
			{
				pos = BotOwner_0.Position;
			}
			CustomNavigationPoint customNavigationPoint = null;
			customNavigationPoint = BotOwner_0.Covers.GetClosestPoint(pos, (GroupPoint x) => !x.IsSpotted && x.IsFreeById(BotOwner_0.Id) && method_15(x.Position));
			CustomNavigationPoint_0 = customNavigationPoint;
			Float_5 = Time.time;
		}
	}

	public bool method_15(Vector3 groupPoint)
	{
		if (BotOwner_0.MinesData.LastPlanted == null)
		{
			return true;
		}
		if ((BotOwner_0.MinesData.LastPlanted.Position - groupPoint).sqrMagnitude > 100f)
		{
			return true;
		}
		return false;
	}

	public override CustomNavigationPoint FindPoint(CoverSearchData data, Func<CoverSearchData, CustomNavigationPoint> p, bool checkCurrent)
	{
		if (CustomNavigationPoint_0 != null)
		{
			return CustomNavigationPoint_0;
		}
		return base.FindPoint(data, p, checkCurrent);
	}

	public void method_16()
	{
		if (Float_3 < Time.time)
		{
			Float_3 = Time.time + GClass856.Random(15f, 60f);
			BotOwner_0.BotTalk.Say(EPhraseTrigger.Provocation, sayImmediately: true);
		}
	}

	public override AICoreActionEndStruct EndHoldPosition()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy.Distance < 50f)
		{
			method_16();
			if (Time.time - BotOwner_0.Memory.ComeToCoverTime > 90f)
			{
				BotOwner_0.MinesData.CacheAllInRadius(goalEnemy.CurrPosition, 50f);
				return AICoreActionEndStruct_1;
			}
			if (Time.time - BotOwner_0.MinesData.LastPlantTime < 5f && method_17(out var aiCoreActionEnd))
			{
				return aiCoreActionEnd;
			}
		}
		if (goalEnemy.Distance > 70f && method_17(out var aiCoreActionEnd2))
		{
			return aiCoreActionEnd2;
		}
		if (BotOwner_0.Medecine.FirstAid.Have2Do && goalEnemy.Distance > 20f && !goalEnemy.IsVisible && Time.time - goalEnemy.PersonalLastSeenTime > 15f && BotOwner_0.Memory.IsInCover)
		{
			return new AICoreActionEndStruct("heal");
		}
		if (method_13())
		{
			return new AICoreActionEndStruct("prEnd");
		}
		return AICoreActionEndStruct_1;
	}

	public bool method_17(out AICoreActionEndStruct aiCoreActionEnd)
	{
		method_14();
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
		float num = Mathf.Abs(50f - magnitude);
		float num2 = Mathf.Abs(50f - magnitude2);
		if (num2 < num)
		{
			BotOwner_0.Memory.Spotted(byHit: false);
			BotOwner_0.Memory.SetCoverPoints(CustomNavigationPoint_0);
			aiCoreActionEnd = new AICoreActionEndStruct($"goNext:{num2}<{num}");
			return true;
		}
		aiCoreActionEnd = default(AICoreActionEndStruct);
		return false;
	}

	public override bool ShallUseNow()
	{
		if (BotOwner_0.Memory.HaveEnemy && Bool_4)
		{
			return BotOwner_0.Memory.GoalEnemy.Person.BtrState != EPlayerBtrState.Inside;
		}
		return false;
	}

	public void method_18()
	{
		Bool_4 = true;
	}

	public void method_19()
	{
		Bool_4 = false;
	}

	public bool method_20()
	{
		if (CustomNavigationPoint_0 != null && (CustomNavigationPoint_0.Position - BotOwner_0.Position).sqrMagnitude < 1f)
		{
			return true;
		}
		return false;
	}

	public override AICoreActionEndStruct EndMoveStealthy()
	{
		if (method_20())
		{
			return new AICoreActionEndStruct("cclt");
		}
		if (BotOwner_0.Memory.IsInCover)
		{
			return new AICoreActionEndStruct("inCover");
		}
		if (BotOwner_0.Memory.GoalEnemy.Distance > 70f)
		{
			return new AICoreActionEndStruct("wantSpr");
		}
		return AICoreActionEndStruct_1;
	}

	public override AICoreActionEndStruct EndHeal()
	{
		return base.EndHeal();
	}

	public override string Name()
	{
		return "PrtStalk";
	}

	public override void Dispose()
	{
		BotOwner_0.MinesData.OnMinesStartCache -= method_19;
		BotOwner_0.MinesData.OnMinesCacheCompleted -= method_18;
		base.Dispose();
	}

	[CompilerGenerated]
	public bool method_21(GroupPoint x)
	{
		if (!x.IsSpotted && x.IsFreeById(BotOwner_0.Id))
		{
			return method_15(x.Position);
		}
		return false;
	}
}
