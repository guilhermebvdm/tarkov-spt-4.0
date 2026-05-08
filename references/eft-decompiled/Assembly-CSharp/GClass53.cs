using System;
using System.Linq;
using System.Runtime.CompilerServices;
using EFT;
using UnityEngine;

public class GClass53 : GClass51
{
	public GClass53(BotOwner bot, int priority)
		: base(bot, priority, canUseStationary: true)
	{
		method_15(Float_6, Float_7);
	}

	public override bool ShallUseNow()
	{
		return true;
	}

	public override CustomNavigationPoint FindPoint(CoverSearchData data, Func<CoverSearchData, CustomNavigationPoint> p, bool checkCurrent)
	{
		method_15(Float_6, Float_7);
		if (method_18(out var customNavigationPoint))
		{
			return customNavigationPoint;
		}
		Vector3 vector = Vector3_0;
		if (BotOwner_0.BotFollower.HaveBoss)
		{
			Vector3 positionOrTargetCover = BotOwner_0.BotFollower.BossToFollow.PositionOrTargetCover;
			if ((Vector3_0 - positionOrTargetCover).magnitude < 30f)
			{
				vector = positionOrTargetCover;
			}
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
		return "BoarPatrol";
	}

	public bool method_18(out CustomNavigationPoint customNavigationPoint)
	{
		Vector3 pos = Vector3_1 + Vector3_0;
		if (BotOwner_0.Boss.IamBoss)
		{
			customNavigationPoint = BotOwner_0.Covers.GetClosestPoint(pos, delegate(GroupPoint point)
			{
				if (!point.IsFreeById(BotOwner_0.Id))
				{
					return false;
				}
				return (!Bool_4 || point.Special.HasFlag(ECoverPointSpecial.forBoss)) ? true : false;
			}, printErrorLogsIfFail: false, 300);
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
		}
		customNavigationPoint = BotOwner_0.Covers.GetClosestPoint(pos, delegate(GroupPoint point)
		{
			if (!point.IsFreeById(BotOwner_0.Id))
			{
				return false;
			}
			return (!Bool_4 || point.Special.HasFlag(ECoverPointSpecial.forFollowers)) ? true : false;
		}, printErrorLogsIfFail: false, 300);
		if (customNavigationPoint != null)
		{
			if (!Bool_4)
			{
				return true;
			}
			if (customNavigationPoint.Special.HasFlag(ECoverPointSpecial.forFollowers))
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
	public bool method_20(GroupPoint point)
	{
		if (!point.IsFreeById(BotOwner_0.Id))
		{
			return false;
		}
		if (Bool_4 && !point.Special.HasFlag(ECoverPointSpecial.forFollowers))
		{
			return false;
		}
		return true;
	}

	[CompilerGenerated]
	public bool method_21(GroupPoint x)
	{
		return x.IsFreeById(BotOwner_0.Id);
	}

	[CompilerGenerated]
	public bool method_22(GroupPoint point)
	{
		if (!point.IsFreeById(BotOwner_0.Id))
		{
			return false;
		}
		return true;
	}
}
