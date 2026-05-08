using System;
using System.Linq;
using System.Runtime.CompilerServices;
using EFT;
using UnityEngine;

public class GClass52 : GClass51
{
	[NonSerialized]
	public float Float_10 = 1f;

	[NonSerialized]
	public float Float_11 = 3f;

	[NonSerialized]
	public float Float_12 = 100f;

	[NonSerialized]
	public float Float_13;

	public GClass52(BotOwner bot, int priority)
		: base(bot, priority, canUseStationary: false)
	{
		method_15(Float_10, Float_11);
	}

	public override bool ShallUseNow()
	{
		return BotOwner_0.BotFollower.HaveBoss;
	}

	public override CustomNavigationPoint FindPoint(CoverSearchData data, Func<CoverSearchData, CustomNavigationPoint> p, bool checkCurrent)
	{
		method_15(Float_10, Float_11);
		if (method_18(out var customNavigationPoint))
		{
			return customNavigationPoint;
		}
		Vector3 vector = Vector3_0;
		if (BotOwner_0.BotFollower.HaveBoss)
		{
			vector = BotOwner_0.BotFollower.BossToFollow.PositionOrTargetCover;
		}
		data.CenterPos = vector + Vector3_1;
		Debug.DrawRay(data.CenterPos, Vector3.up * 15f, BotOwner_0.Boss.IamBoss ? Color.yellow : Color.green, 14f);
		data.SearchType = CoverSearchType.distToToCenter;
		data.ArrayType = PointsArrayType.allWithBush;
		CustomNavigationPoint customNavigationPoint2 = base.FindPoint(data, p, checkCurrent);
		if (customNavigationPoint2 == null)
		{
			customNavigationPoint2 = base.FindPoint(data, p, checkCurrent);
		}
		return customNavigationPoint2;
	}

	public override string Name()
	{
		return "BoarClPatrol";
	}

	public bool method_18(out CustomNavigationPoint customNavigationPoint)
	{
		Vector3 pos = BotOwner_0.BotFollower.BossToFollow.PositionOrTargetCover + Vector3_1;
		customNavigationPoint = BotOwner_0.Covers.GetClosestPoint(pos, delegate(GroupPoint point)
		{
			if (!point.IsFreeById(BotOwner_0.Id))
			{
				return false;
			}
			return (!Bool_4 || point.Special.HasFlag(ECoverPointSpecial.forBoss)) ? true : false;
		}, printErrorLogsIfFail: false, 100);
		if (customNavigationPoint != null)
		{
			if (!Bool_4)
			{
				return true;
			}
			if (customNavigationPoint.Special.HasFlag(ECoverPointSpecial.forBoss))
			{
				return true;
			}
		}
		if (Bool_4)
		{
			GroupPoint groupPoint = GClass856.RandomElement(List_0.Where((GroupPoint x) => x.IsFreeById(BotOwner_0.Id)));
			if (groupPoint != null)
			{
				customNavigationPoint = groupPoint.GetById(BotOwner_0.Id);
				return true;
			}
		}
		customNavigationPoint = BotOwner_0.Covers.GetClosestPoint(pos, (GroupPoint point) => point.IsFreeById(BotOwner_0.Id) ? true : false);
		return true;
	}

	public override AICoreActionEndStruct EndHoldPosition()
	{
		if (Float_13 < Time.time)
		{
			Float_13 = Time.time + 5f;
			Vector3 positionOrTargetCover = BotOwner_0.BotFollower.BossToFollow.PositionOrTargetCover;
			if ((BotOwner_0.Position - positionOrTargetCover).sqrMagnitude > Float_12 && BotOwner_0.Memory.CurCustomCoverPoint != null && method_18(out var customNavigationPoint))
			{
				float sqrMagnitude = (BotOwner_0.Memory.CurCustomCoverPoint.Position - positionOrTargetCover).sqrMagnitude;
				if ((customNavigationPoint.Position - positionOrTargetCover).sqrMagnitude < sqrMagnitude)
				{
					BotOwner_0.Memory.Spotted(byHit: false);
					BotOwner_0.Memory.SetCoverPoints(customNavigationPoint);
				}
			}
		}
		return base.EndHoldPosition();
	}

	[CompilerGenerated]
	public bool method_19(GroupPoint point)
	{
		if (!point.IsFreeById(BotOwner_0.Id))
		{
			return false;
		}
		if (Bool_4 && !point.Special.HasFlag(ECoverPointSpecial.forBoss))
		{
			return false;
		}
		return true;
	}

	[CompilerGenerated]
	public bool method_20(GroupPoint x)
	{
		return x.IsFreeById(BotOwner_0.Id);
	}

	[CompilerGenerated]
	public bool method_21(GroupPoint point)
	{
		if (!point.IsFreeById(BotOwner_0.Id))
		{
			return false;
		}
		return true;
	}
}
