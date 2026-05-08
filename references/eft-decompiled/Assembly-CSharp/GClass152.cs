using System;
using EFT;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.AI;

public class GClass152(BotOwner bot, int priority) : GClass150(bot, priority)
{
	[NonSerialized]
	public const float Float_3 = 25f;

	[NonSerialized]
	public const float Float_4 = 10f;

	[NonSerialized]
	public const float Float_5 = 20f;

	[NonSerialized]
	public const float Float_6 = 8f;

	public const float UNDER_FIRE_DELTA = 30f;

	public const float SDIST_TREE_CLOSE = 900f;

	[NonSerialized]
	public const string String_0 = "R&H_IN";

	[NonSerialized]
	public const string String_1 = "R&H_OUT";

	[NonSerialized]
	public const float Float_7 = 1.5f;

	[NonSerialized]
	public CustomNavigationPoint CustomNavigationPoint_0;

	[NonSerialized]
	public float Float_8;

	[NonSerialized]
	public float Float_9 = 20f;

	[NonSerialized]
	public string String_2 = "R&H_OUT";

	[NonSerialized]
	public bool Bool_4;

	[NonSerialized]
	public float Float_10;

	[NonSerialized]
	public float Float_11;

	public void SetBoss(GClass448 sectantPriest)
	{
	}

	public override bool ShallUseNow()
	{
		return BotOwner_0.Memory.HaveEnemy;
	}

	public override string Name()
	{
		return String_2;
	}

	public override CustomNavigationPoint FindPoint(CoverSearchData data, Func<CoverSearchData, CustomNavigationPoint> p, bool checkCurrent)
	{
		return FindPointForHide(data, CustomNavigationPoint_0);
	}

	public override global::AICoreActionResultStruct<BotLogicDecision, GClass26> GetDecision()
	{
		method_16(null);
		if (BotOwner_0.Memory.IsInCover)
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.holdPosition, "hold");
		}
		return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.runToCover, "hold");
	}

	public void SetInside(bool isInside)
	{
		Bool_4 = isInside;
		Float_9 = (Bool_4 ? 10f : 20f);
		String_2 = (Bool_4 ? "R&H_IN" : "R&H_OUT");
	}

	public CustomNavigationPoint FindPointForHide([CanBeNull] CoverSearchData data, [CanBeNull] CustomNavigationPoint prevPoint)
	{
		float num = Time.time - BotOwner_0.Memory.UnderFireTime;
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (prevPoint == null)
		{
			prevPoint = BotOwner_0.Memory.CurCustomCoverPoint;
		}
		if (num > 30f)
		{
			if (prevPoint != null && method_17(prevPoint))
			{
				return prevPoint;
			}
			float sDist = 0f;
			CustomNavigationPoint customNavigationPoint = null;
			if (!Bool_4)
			{
				if (goalEnemy == null)
				{
					if (BotOwner_0.Settings.FileSettings.Boss.RUN_HIDE_CAN_USE_TREE_COVRES)
					{
						customNavigationPoint = ((data == null) ? BotOwner_0.BotsGroup.CoverPointMaster.ClosestGreenPoint(BotOwner_0.Position, BotOwner_0, null, null, out sDist) : BotOwner_0.BotsGroup.CoverPointMaster.ClosestGreenPoint(data.CenterPos, BotOwner_0, null, null, out sDist));
					}
				}
				else
				{
					customNavigationPoint = BotOwner_0.BotsGroup.CoverPointMaster.ClosestGreenPoint(BotOwner_0.Position, BotOwner_0, 25f, goalEnemy.CurrPosition, null, out sDist);
				}
				if (customNavigationPoint != null && sDist < 900f)
				{
					return customNavigationPoint;
				}
			}
		}
		CustomNavigationPoint customNavigationPoint2 = null;
		if (goalEnemy != null)
		{
			customNavigationPoint2 = BotOwner_0.Covers.FindClosestPoint(BotOwner_0.Position, 25f, goalEnemy.CurrPosition);
		}
		if (customNavigationPoint2 != null && method_17(customNavigationPoint2))
		{
			return customNavigationPoint2;
		}
		CoverSearchData coverSearchData = new CoverSearchData(BotOwner_0.Position, BotOwner_0.CoverSearchInfo, CoverShootType.hide, 25f, 0f, CoverSearchType.distToBot, null, null, null, ECheckSHootHide.shootAndHide, new CoverSearchDefenceDataClass(0f), PointsArrayType.both, useSelfFindPoint: true, null, 15);
		coverSearchData.SearchType = CoverSearchType.distToBot;
		coverSearchData.UseSelfFindPoint = false;
		coverSearchData.ArrayType = PointsArrayType.both;
		coverSearchData.UseLineCastToCover = true;
		coverSearchData.PointToBeClose = null;
		coverSearchData.shootType = CoverShootType.hide;
		coverSearchData.MinSDistToCarePos = 625f;
		CustomNavigationPoint coverPointMain = BotOwner_0.BotsGroup.CoverPointMaster.GetCoverPointMain(coverSearchData, checkCurrent: false);
		BotOwner_0.Memory.BotCurrentCoverInfo.SetCover(coverPointMain);
		return coverPointMain;
	}

	public void SetCorePosition(Vector3 pointForBoss)
	{
	}

	public override AICoreActionEndStruct EndRunToCover()
	{
		if (!BotOwner_0.Memory.IsInCover && (BotOwner_0.Memory.CurCustomCoverPoint == null || !BotOwner_0.Memory.CurCustomCoverPoint.IsSpotted))
		{
			return AICoreActionEndStruct_1;
		}
		BotOwner_0.BotRun.EndMove();
		return new AICoreActionEndStruct("at cover");
	}

	public override AICoreActionEndStruct EndHoldPosition()
	{
		method_16(null);
		if (!BotOwner_0.Memory.IsInCover)
		{
			return new AICoreActionEndStruct("no in cov");
		}
		if (BotOwner_0.Memory.CurCustomCoverPoint == null)
		{
			return new AICoreActionEndStruct("covisnull");
		}
		if (Bool_4)
		{
			if (!method_17(BotOwner_0.Memory.CurCustomCoverPoint))
			{
				return new AICoreActionEndStruct("insidenotgo");
			}
			return AICoreActionEndStruct_1;
		}
		return method_15();
	}

	public float method_13(Vector3 from, Vector3 pos)
	{
		if (Float_10 < Time.time)
		{
			Float_10 = Time.time + 2f;
			NavMeshPath navMeshPath = new NavMeshPath();
			NavMesh.CalculatePath(pos, from, -1, navMeshPath);
			if (navMeshPath.status == NavMeshPathStatus.PathComplete)
			{
				float float_ = GClass371.CalculatePathLength(navMeshPath);
				Float_11 = float_;
			}
			else
			{
				Float_11 = float.MaxValue;
			}
		}
		return Float_11;
	}

	public bool method_14(Vector3 place)
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		Vector3 vector = goalEnemy.CurrPosition - place;
		if (Mathf.Abs(vector.y) > 1.5f)
		{
			return true;
		}
		if (vector.magnitude < 8f)
		{
			return false;
		}
		if (goalEnemy.Person.AIData.PlaceInfo != null && goalEnemy.Person.AIData.PlaceInfo.AreaId == BotOwner_0.AIData.PlaceInfo.AreaId && goalEnemy.Distance < Float_9 && method_13(place, goalEnemy.CurrPosition) < Float_9 * 1.5f)
		{
			return false;
		}
		return true;
	}

	public AICoreActionEndStruct method_15()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if ((Mathf.Abs(goalEnemy.CurrPosition.y - BotOwner_0.Position.y) < 1.5f || goalEnemy.IsVisible) && goalEnemy.Distance < Float_9 && !method_17(BotOwner_0.Memory.CurCustomCoverPoint))
		{
			return new AICoreActionEndStruct("lot params");
		}
		return AICoreActionEndStruct_1;
	}

	public void method_16([CanBeNull] CoverSearchData data)
	{
		if (Float_8 < Time.time)
		{
			Float_8 = Time.time + 1f;
			CustomNavigationPoint_0 = FindPointForHide(data, CustomNavigationPoint_0);
			BotOwner_0.Memory.BotCurrentCoverInfo.SetCover(CustomNavigationPoint_0);
		}
	}

	public bool method_17(CustomNavigationPoint point)
	{
		if (point.IsSpotted)
		{
			return false;
		}
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy == null)
		{
			return true;
		}
		if (Bool_4)
		{
			if (!point.CanIHideFromPos(0f, useRaycast: true, useAng: false, goalEnemy.CurrPosition))
			{
				return false;
			}
			return method_14(point.Position);
		}
		Vector3 vector = point.Position - goalEnemy.CurrPosition;
		float sqrMagnitude = vector.sqrMagnitude;
		float num = Mathf.Abs(vector.y);
		float num2 = Float_9 * Float_9;
		if (sqrMagnitude < num2 && (num < 1.5f || goalEnemy.IsVisible))
		{
			return false;
		}
		return true;
	}
}
