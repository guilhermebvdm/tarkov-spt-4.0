using System;
using System.Runtime.CompilerServices;
using EFT;
using UnityEngine;

public class GClass97 : GClass95
{
	[Serializable]
	[CompilerGenerated]
	public class Class219
	{
		public static readonly Class219 class219_0 = new Class219();

		public static Func<GroupPoint, bool> func_0;

		public bool method_0(GroupPoint point)
		{
			return !point.IsSpotted;
		}
	}

	[NonSerialized]
	public CustomNavigationPoint CustomNavigationPoint_0;

	[NonSerialized]
	public Vector3? Nullable_1;

	public GClass97(BotOwner bot, int priority)
		: base(bot, priority)
	{
	}

	public override global::AICoreActionResultStruct<BotLogicDecision, GClass26> GetDecision()
	{
		if (BotOwner_0.Memory.IsInCover && !BotOwner_0.Memory.CurCustomCoverPoint.IsSpotted)
		{
			HoldFor(GClass856.Random(15f, 30f));
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.holdPosition, "hld");
		}
		BotOwner_0.Memory.BotCurrentCoverInfo.SetCover(CustomNavigationPoint_0);
		if (GClass856.IsTrue100(50f))
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.runToCover, "rld");
		}
		return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.goToCoverPoint, "gld");
	}

	public override CustomNavigationPoint FindPoint(CoverSearchData data, Func<CoverSearchData, CustomNavigationPoint> p, bool checkCurrent)
	{
		if (base.GClass444_0 != null && !Nullable_1.HasValue)
		{
			float b = 20f;
			float x = GClass856.Random(10f, b) * (float)GClass856.RandomSing();
			float z = GClass856.Random(10f, b) * (float)GClass856.RandomSing();
			Vector3 vector = new Vector3(x, 0f, z);
			Nullable_1 = base.GClass444_0.GetSummonCenter() + vector;
		}
		if (Nullable_1.HasValue)
		{
			CustomNavigationPoint closestPoint = BotOwner_0.Covers.GetClosestPoint(Nullable_1.Value, (GroupPoint point) => !point.IsSpotted);
			if (closestPoint != null)
			{
				CustomNavigationPoint_0 = closestPoint;
				return CustomNavigationPoint_0;
			}
		}
		return base.FindPoint(data, p, checkCurrent);
	}

	public override bool ShallUseNow()
	{
		return true;
	}

	public override string Name()
	{
		return "PriestPatrol";
	}

	public override AICoreActionEndStruct EndRunToCover()
	{
		return base.EndRunToCover();
	}

	public override AICoreActionEndStruct EndHoldPosition()
	{
		if (method_7())
		{
			BotOwner_0.Memory.Spotted(byHit: false);
			return new AICoreActionEndStruct("EndHol");
		}
		if (!BotOwner_0.Memory.IsInCover)
		{
			return new AICoreActionEndStruct("IsInCover");
		}
		return AICoreActionEndStruct_1;
	}
}
