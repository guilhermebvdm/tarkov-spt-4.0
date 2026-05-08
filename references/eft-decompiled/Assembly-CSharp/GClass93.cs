using System;
using System.Runtime.CompilerServices;
using EFT;
using UnityEngine;

public class GClass93 : BaseLogicLayerSimpleAbstractClass
{
	[Serializable]
	[CompilerGenerated]
	public class Class217
	{
		public static readonly Class217 class217_0 = new Class217();

		public static Func<GroupPoint, bool> func_0;

		public bool method_0(GroupPoint point)
		{
			return !point.IsSpotted;
		}
	}

	[NonSerialized]
	public CustomNavigationPoint CustomNavigationPoint_0;

	[NonSerialized]
	public AICorePoint AicorePoint_0;

	[NonSerialized]
	public float Float_3;

	public GClass93(BotOwner bot, int priority)
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
		if (CustomNavigationPoint_0 != null && !CustomNavigationPoint_0.IsSpotted)
		{
			return CustomNavigationPoint_0;
		}
		if (AicorePoint_0 == null)
		{
			AICorePointHolder aICorePointsHolder = BotOwner_0.BotsGroup.BotGame.BotsController.CoversData.AICorePointsHolder;
			CustomNavigationPoint closestPoint = BotOwner_0.Covers.GetClosestPoint(BotOwner_0.Position);
			if (closestPoint != null)
			{
				AicorePoint_0 = aICorePointsHolder.GetCorePoint(closestPoint.GroupPoint.CorePointId);
			}
		}
		if (AicorePoint_0 != null)
		{
			if (AicorePoint_0.ConnectionsAtNet.Count > 0)
			{
				AICorePoint aICorePoint = GClass856.RandomElement(AicorePoint_0.ConnectionsAtNet);
				if (aICorePoint != null)
				{
					AicorePoint_0 = aICorePoint;
				}
			}
			float b = 20f;
			float x = GClass856.Random(10f, b) * (float)GClass856.RandomSing();
			float z = GClass856.Random(10f, b) * (float)GClass856.RandomSing();
			Vector3 vector = new Vector3(x, 0f, z);
			CustomNavigationPoint closestPoint2 = BotOwner_0.Covers.GetClosestPoint(AicorePoint_0.Position + vector, (GroupPoint point) => !point.IsSpotted);
			if (closestPoint2 != null)
			{
				CustomNavigationPoint_0 = closestPoint2;
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
		return "Peace Zrch Prtl";
	}

	public override AICoreActionEndStruct EndRunToCover()
	{
		return base.EndRunToCover();
	}

	public override AICoreActionEndStruct EndHoldPosition()
	{
		if (!BotOwner_0.Memory.IsInCover)
		{
			return new AICoreActionEndStruct("IsInCover");
		}
		if (method_13())
		{
			return AICoreActionEndStruct_1;
		}
		BotOwner_0.Memory.Spotted(byHit: false);
		CustomNavigationPoint_0 = null;
		return new AICoreActionEndStruct("EndHol");
	}

	public bool method_13()
	{
		if (!Bool_2)
		{
			return false;
		}
		if (Float_2 > Time.time)
		{
			return true;
		}
		Bool_2 = false;
		return false;
	}
}
