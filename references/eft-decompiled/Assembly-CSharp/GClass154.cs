using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using EFT;
using UnityEngine;

public class GClass154 : GClass150
{
	[NonSerialized]
	public const float Float_3 = 14f;

	[NonSerialized]
	public const float Float_4 = 4f;

	[NonSerialized]
	public float Float_5;

	[NonSerialized]
	public float Float_6;

	[NonSerialized]
	public float Float_7 = 2f;

	[NonSerialized]
	[CompilerGenerated]
	public bool Bool_4;

	public bool HaveCoverToShoot
	{
		[CompilerGenerated]
		get
		{
			return Bool_4;
		}
		[CompilerGenerated]
		set
		{
			Bool_4 = value;
		}
	}

	public GClass154(BotOwner bot, int priority)
		: base(bot, priority)
	{
	}

	public override global::AICoreActionResultStruct<BotLogicDecision, GClass26> GetDecision()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy.CanShoot && goalEnemy.IsVisible && (Float_5 < Time.time || goalEnemy.Distance < 14f))
		{
			Float_5 = Time.time + 4f;
			Float_6 = Time.time;
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.shootFromPlace, "SHOOTPERIOD");
		}
		if (BotOwner_0.Memory.IsInCover)
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.holdPosition, "IsInCover");
		}
		return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.runToCover, "IsInCover");
	}

	public override bool ShallUseNow()
	{
		if (BotOwner_0.Memory.GoalEnemy == null)
		{
			return false;
		}
		return true;
	}

	public override string Name()
	{
		return "Run&Strike";
	}

	public override void OnActivate()
	{
		base.OnActivate();
	}

	public override AICoreActionEndStruct EndRunToCover()
	{
		if (!BotOwner_0.Memory.IsInCover && BotOwner_0.CanSprintPlayer && (BotOwner_0.Memory.CurCustomCoverPoint == null || !BotOwner_0.Memory.CurCustomCoverPoint.IsSpotted))
		{
			return AICoreActionEndStruct_1;
		}
		BotOwner_0.BotRun.EndMove();
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndHoldPosition()
	{
		if (method_14())
		{
			return AICoreActionEndStruct;
		}
		return base.EndHoldPosition();
	}

	public override AICoreActionEndStruct EndShootFromPlace()
	{
		if (Time.time - Float_6 > Float_7)
		{
			return AICoreActionEndStruct;
		}
		return base.EndShootFromPlace();
	}

	public bool method_13(CustomNavigationPoint pointOfSearch)
	{
		if (pointOfSearch == null)
		{
			return false;
		}
		HashSet<Vector3> positionsIMustCare = BotOwner_0.Covers.CarePositions();
		return pointOfSearch.CanIHide(positionsIMustCare, 0f, useRaycast: true);
	}

	public bool method_14()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy.CanShoot && goalEnemy.IsVisible)
		{
			return Float_5 < Time.time;
		}
		return false;
	}

	public override void Dispose()
	{
		base.Dispose();
	}
}
