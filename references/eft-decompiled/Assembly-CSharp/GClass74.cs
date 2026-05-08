using System;
using System.Runtime.CompilerServices;
using EFT;
using UnityEngine;

public class GClass74 : BaseLogicLayerSimpleAbstractClass
{
	[Serializable]
	[CompilerGenerated]
	public class Class214
	{
		public static readonly Class214 class214_0 = new Class214();

		public static Func<GroupPoint, bool> func_0;

		public bool method_0(GroupPoint point)
		{
			return !point.IsSpotted;
		}
	}

	[NonSerialized]
	public bool Bool_4 = true;

	[NonSerialized]
	public CustomNavigationPoint CustomNavigationPoint_0;

	[NonSerialized]
	public CustomNavigationPoint CustomNavigationPoint_1;

	[NonSerialized]
	public AICorePoint AicorePoint_0;

	public GClass74(BotOwner bot, int priority)
		: base(bot, priority)
	{
		BotOwner_0.Memory.OnInCoverChange += method_13;
	}

	public void method_13(bool obj, CustomNavigationPoint arg2)
	{
		if (obj)
		{
			Bool_4 = true;
		}
	}

	public override global::AICoreActionResultStruct<BotLogicDecision, GClass26> GetDecision()
	{
		if (BotOwner_0.Memory.IsInCover)
		{
			HoldFor(GClass856.Random(10f, 15f));
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.holdPosition, "t1hg");
		}
		if (!BotOwner_0.CanSprintPlayer)
		{
			if (GClass856.IsTrue100(15f))
			{
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.goToCoverPoint, "rtc3");
			}
			HoldFor(GClass856.Random(10f, 15f));
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.holdPosition, "t2h");
		}
		return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(GClass856.IsTrue100(50f) ? BotLogicDecision.runToCover : BotLogicDecision.runToCoverZigZag, "rtc4");
	}

	public override AICoreActionEndStruct EndHoldPosition()
	{
		if (Time.time - BotOwner_0.Memory.BotCurrentCoverInfo.ComeToCoverTime > 15f)
		{
			BotOwner_0.Memory.Spotted(byHit: false);
			return new AICoreActionEndStruct("endByT2");
		}
		if (method_7())
		{
			return new AICoreActionEndStruct("EndHol");
		}
		if (!BotOwner_0.Memory.IsInCover)
		{
			return new AICoreActionEndStruct("IsInCover");
		}
		return AICoreActionEndStruct_1;
	}

	public override CustomNavigationPoint FindPoint(CoverSearchData data, Func<CoverSearchData, CustomNavigationPoint> p, bool checkCurrent)
	{
		if (CustomNavigationPoint_1 != null && !CustomNavigationPoint_1.IsSpotted)
		{
			return CustomNavigationPoint_1;
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
				CustomNavigationPoint_1 = closestPoint2;
				return CustomNavigationPoint_1;
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
		return "Obd Ptrl";
	}
}
