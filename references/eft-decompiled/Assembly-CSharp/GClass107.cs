using System;
using System.Runtime.CompilerServices;
using EFT;
using UnityEngine;

public class GClass107 : BaseLogicLayerSimpleAbstractClass
{
	[CompilerGenerated]
	public class Class220
	{
		public GClass107 gclass107_0;

		public Vector3? otherTrg;

		public bool method_0(GroupPoint point)
		{
			if (!point.IsSpotted && point.IsFreeById(gclass107_0.BotOwner_0.Id))
			{
				if (gclass107_0.Gclass453_0.BossLogic.AreaId.HasValue && point.PlaceId != gclass107_0.Gclass453_0.BossLogic.AreaId.Value)
				{
					return false;
				}
				if (otherTrg.HasValue && (point.Position - otherTrg.Value).sqrMagnitude < gclass107_0.Float_5)
				{
					return false;
				}
				return true;
			}
			return false;
		}
	}

	[NonSerialized]
	public bool Bool_4;

	[NonSerialized]
	public Vector3 Vector3_0;

	[NonSerialized]
	public GClass453<GClass441> Gclass453_0;

	[NonSerialized]
	public PatrolPointContainer PatrolPointContainer_0;

	[NonSerialized]
	public CustomNavigationPoint CustomNavigationPoint_0;

	[NonSerialized]
	public float Float_3;

	[NonSerialized]
	public float Float_4;

	[NonSerialized]
	public float Float_5 = 225f;

	public GClass107(BotOwner bot, int priority, bool secutiry)
		: base(bot, priority)
	{
		Bool_4 = secutiry;
		Gclass453_0 = new GClass453<GClass441>(bot);
		Gclass453_0.FindBoss(method_15);
	}

	public override global::AICoreActionResultStruct<BotLogicDecision, GClass26> GetDecision()
	{
		if (!Bool_4)
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BaseLogicLayerAbstractClass.HoldOrCoverRun(BotOwner_0), "rn");
		}
		return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BaseLogicLayerAbstractClass.HoldOrCover(BotOwner_0), "go");
	}

	public override CustomNavigationPoint FindPoint(CoverSearchData data, Func<CoverSearchData, CustomNavigationPoint> p, bool checkCurrent)
	{
		if (CustomNavigationPoint_0 != null && (!CustomNavigationPoint_0.IsFreeById(BotOwner_0.Id) || CustomNavigationPoint_0.IsSpotted))
		{
			CustomNavigationPoint_0 = null;
		}
		if (CustomNavigationPoint_0 != null)
		{
			return CustomNavigationPoint_0;
		}
		Vector3 vector = GClass856.GreateRandom(Float_4) * Vector3_0;
		data.CenterPos = method_17() + vector;
		CustomNavigationPoint freeClosePoint = BotOwner_0.BotsGroup.CoverPointMaster.GetFreeClosePoint(data.CenterPos, BotOwner_0.Covers, 1f);
		if (freeClosePoint != null)
		{
			return freeClosePoint;
		}
		return base.FindPoint(data, p, checkCurrent);
	}

	public override bool ShallUseNow()
	{
		return BotOwner_0.BotFollower.HaveBoss;
	}

	public override string Name()
	{
		return "ksPartol";
	}

	public override AICoreActionEndStruct EndHoldPosition()
	{
		if (!BotOwner_0.Memory.IsInCover)
		{
			return new AICoreActionEndStruct("!InCover");
		}
		if (!BotOwner_0.Memory.CurCustomCoverPoint.IsFreeById(BotOwner_0.Id))
		{
			return new AICoreActionEndStruct("notFree");
		}
		return AICoreActionEndStruct_1;
	}

	public void method_13(PatrolPointContainer obj)
	{
		if (Float_3 < Time.time)
		{
			Float_3 = Time.time + 60f;
			Vector3_0 = GClass855.Rotate90(Vector3_0, GClass856.IsTrue100(50f) ? GClass855.SideTurn.right : GClass855.SideTurn.left);
		}
		if (!BotOwner_0.BotFollower.HaveBoss)
		{
			return;
		}
		CustomNavigationPoint_0 = null;
		CoverSearchData coverSearchData = method_14(obj);
		Float_3 = 3f + Time.time;
		Vector3 vector = GClass856.GreateRandom(Float_4) * Vector3_0;
		BotOwner botOwner = null;
		for (int i = 0; i < BotOwner_0.BotsGroup.MembersCount; i++)
		{
			BotOwner botOwner2 = BotOwner_0.BotsGroup.Member(i);
			if (botOwner2.Id != BotOwner_0.Id && botOwner2.Profile.Info.Settings.Role == WildSpawnType.followerKolontaySecurity)
			{
				botOwner = botOwner2;
				break;
			}
		}
		Vector3? otherTrg = null;
		if (botOwner != null)
		{
			otherTrg = (botOwner.Mover.HasPathAndNoComplete ? botOwner.Mover.TargetPoint : new Vector3?(BotOwner_0.Position));
		}
		CustomNavigationPoint_0 = BotOwner_0.Covers.GetClosestPoint(coverSearchData.CenterPos + vector, delegate(GroupPoint point)
		{
			if (!point.IsSpotted && point.IsFreeById(BotOwner_0.Id))
			{
				if (Gclass453_0.BossLogic.AreaId.HasValue && point.PlaceId != Gclass453_0.BossLogic.AreaId.Value)
				{
					return false;
				}
				if (otherTrg.HasValue && (point.Position - otherTrg.Value).sqrMagnitude < Float_5)
				{
					return false;
				}
				return true;
			}
			return false;
		});
		if (CustomNavigationPoint_0 != null)
		{
			BotOwner_0.Memory.Spotted(byHit: false);
			BotOwner_0.Memory.BotCurrentCoverInfo.SetCover(CustomNavigationPoint_0);
		}
	}

	public CoverSearchData method_14(PatrolPointContainer obj)
	{
		int num = 75;
		Vector3 vector = GClass856.GreateRandom(Float_4) * Vector3_0;
		Vector3 centerPos = (obj?.Position ?? method_17()) + vector;
		int num2 = num * num;
		return new CoverSearchData(centerPos, BotOwner_0.CoverSearchInfo, CoverShootType.hide, num2, 0f, CoverSearchType.distToToCenter, null, null, null, ECheckSHootHide.hide, new CoverSearchDefenceDataClass(-1f))
		{
			PlaceInfo = Gclass453_0.BossLogic.AreaId
		};
	}

	public void method_15()
	{
		Gclass453_0.BossLogic.OnPatrolChanged += method_16;
		Float_4 = (Bool_4 ? 6f : 30f);
		Vector3_0 = Gclass453_0.BossLogic.GetOffset();
		method_13(null);
		BotOwner_0.AITaskManager.RegisterDelayedTask(BotOwner_0, 3f, delegate
		{
			method_13(Gclass453_0.BossLogic.LastPatrol);
		});
	}

	public void method_16(PatrolPointContainer obj)
	{
		PatrolPointContainer_0 = obj;
		method_13(obj);
	}

	public Vector3 method_17()
	{
		if (BotOwner_0.BotFollower.HaveBoss)
		{
			if (Gclass453_0.BossLogic.LastPatrol != null)
			{
				return Gclass453_0.BossLogic.LastPatrol.Position;
			}
			return BotOwner_0.BotFollower.BossToFollow.PositionOrTargetCover;
		}
		return BotOwner_0.Position;
	}

	public override void Dispose()
	{
		if (Gclass453_0 != null && Gclass453_0.BossLogic != null)
		{
			Gclass453_0.BossLogic.OnPatrolChanged -= method_16;
		}
		base.Dispose();
	}

	[CompilerGenerated]
	public void method_18()
	{
		method_13(Gclass453_0.BossLogic.LastPatrol);
	}
}
