using System;
using EFT;
using UnityEngine;

public class Class280 : BotRequest
{
	[NonSerialized]
	public Vector3? Nullable_0;

	[NonSerialized]
	public CustomNavigationPoint CustomNavigationPoint_0;

	[NonSerialized]
	public float Float_0;

	[NonSerialized]
	public float Float_1 = -1f;

	public override EBotRequestMode RequestMode => EBotRequestMode.Fight;

	public Class280(Player requester, Vector3? nearestPoint, float period = 0f)
		: base(requester, BotRequestType.getInCover)
	{
		Nullable_0 = nearestPoint;
		if (Nullable_0.HasValue)
		{
			GClass369.DebugDrawArc(requester.Position, Nullable_0.Value, 5f, 5f, Color.yellow);
		}
		if (period > 0f)
		{
			Float_1 = Time.time + period;
		}
	}

	public override bool CanRequest(BotOwner requester)
	{
		if (requester.Memory.GoalEnemy != null && Time.time - requester.Memory.GoalEnemy.PersonalLastSeenTime < 5f)
		{
			return false;
		}
		return true;
	}

	public void method_0()
	{
		if (Float_0 < Time.time)
		{
			Float_0 = 3f + Time.time;
			CustomNavigationPoint_0 = Executor.BotsGroup.CoverPointMaster.GetCoverPointMain(method_1(), checkCurrent: false);
			if (CustomNavigationPoint_0 != null && CustomNavigationPoint_0 != Executor.Memory.CurCustomCoverPoint)
			{
				Executor.Memory.Spotted(byHit: false);
			}
		}
	}

	public override AICoreActionEndStruct EndHoldPosition()
	{
		if (Executor.Memory.IsInCover)
		{
			method_0();
			return ContinueNodeLogic;
		}
		return FinishNodeLogic;
	}

	public override bool CanProceed()
	{
		if (!(Executor != null))
		{
			return false;
		}
		EnemyInfo goalEnemy = Executor.Memory.GoalEnemy;
		if (goalEnemy != null)
		{
			if (goalEnemy.IsVisible)
			{
				return false;
			}
			_ = Time.time - goalEnemy.PersonalLastSeenTime;
			return false;
		}
		if (Float_1 > 0f && Float_1 < Time.time)
		{
			EndIfCantExecute = true;
			return false;
		}
		return true;
	}

	public override bool FindPoint(CoverSearchData data, Func<CoverSearchData, CustomNavigationPoint> func, bool checkCurrent, out CustomNavigationPoint customNavigationPoint)
	{
		if (CustomNavigationPoint_0 != null && (CustomNavigationPoint_0.IsSpotted || !CustomNavigationPoint_0.IsFreeById(Executor.Id)))
		{
			CustomNavigationPoint_0 = null;
		}
		if (CustomNavigationPoint_0 != null)
		{
			customNavigationPoint = CustomNavigationPoint_0;
			return true;
		}
		if (Nullable_0.HasValue)
		{
			data.CenterPos = Nullable_0.Value;
		}
		data.SearchType = CoverSearchType.distToToCenter;
		data.ArrayType = PointsArrayType.both;
		data.CheckShootHide = ECheckSHootHide.hide;
		base.FindPoint(data, func, checkCurrent, out customNavigationPoint);
		if (customNavigationPoint == null)
		{
			CustomNavigationPoint customNavigationPoint_ = data.Bot.Covers.FindClosestPoint(data.CenterPos);
			CustomNavigationPoint_0 = customNavigationPoint_;
			customNavigationPoint = CustomNavigationPoint_0;
		}
		else
		{
			CustomNavigationPoint_0 = customNavigationPoint;
		}
		if (Nullable_0.HasValue && CustomNavigationPoint_0 != null)
		{
			GClass369.DebugDrawArc(data.CenterPos, CustomNavigationPoint_0.Position, 4f, 15f, Color.magenta);
		}
		return true;
	}

	public CoverSearchData method_1()
	{
		CoverSearchData coverSearchData = new CoverSearchData(Executor.Position, Executor.CoverSearchInfo, CoverShootType.shoot, 2500f, 0f, CoverSearchType.distToToCenter, null, null, null, ECheckSHootHide.hide, new CoverSearchDefenceDataClass(0f));
		if (Nullable_0.HasValue)
		{
			coverSearchData.CenterPos = Nullable_0.Value;
		}
		coverSearchData.SearchType = CoverSearchType.distToToCenter;
		return coverSearchData;
	}

	public override void Dispose()
	{
		base.Dispose();
	}
}
