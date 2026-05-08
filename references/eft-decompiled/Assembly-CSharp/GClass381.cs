using System;
using System.Collections.Generic;
using EFT;
using JetBrains.Annotations;
using UnityEngine;

public class GClass381 : GClass379
{
	[NonSerialized]
	public HashSet<int> HashSet_0 = new HashSet<int>();

	[NonSerialized]
	public HashSet<int> HashSet_1 = new HashSet<int>();

	[NonSerialized]
	public GClass368<GroupPointSearchData> Gclass368_0 = new GClass368<GroupPointSearchData>();

	[CanBeNull]
	public CustomNavigationPoint GetCover(CoverSearchData data)
	{
		HashSet_0.Clear();
		HashSet_1.Clear();
		Gclass368_0.Clear();
		Vector3 centerPos = data.CenterPos;
		NavGraphVoxelSimple voxelSafe = data.Bot.GetVoxelSafe(centerPos);
		string replaceInfo;
		GroupPoint startPoint = GClass380.GetStartPoint(data.Bot, centerPos, out replaceInfo);
		GroupPointSearchData pointToCheck = method_5(startPoint, data);
		DebugCoverFindGraphSearchData debugCoverFindGraphSearchData = new DebugCoverFindGraphSearchData(data, voxelSafe);
		if (method_6(data, pointToCheck, debugCoverFindGraphSearchData, out var customNavigationPoint))
		{
			data.Bot.SetLastSearchDebug(debugCoverFindGraphSearchData);
			debugCoverFindGraphSearchData.TryPrintLogs(customNavigationPoint);
			return customNavigationPoint;
		}
		data.Bot.SetLastSearchDebug(debugCoverFindGraphSearchData);
		debugCoverFindGraphSearchData.TryPrintLogs(null);
		return null;
	}

	public GroupPointSearchData method_5(GroupPoint e, CoverSearchData data)
	{
		if (!HashSet_1.Contains(e.Id) && !HashSet_0.Contains(e.Id))
		{
			float power = data.GetPower(e);
			GroupPointSearchData groupPointSearchData = new GroupPointSearchData(e, power);
			if (data.shootType != CoverShootType.hide && data.Bot.GoalEnemy != null && (data.Bot.GoalEnemy.CurrPosition - data.Bot.Position).magnitude > data.Bot.MaxShootDist())
			{
				groupPointSearchData.CanIShootToEnemy = false;
			}
			HashSet_0.Add(groupPointSearchData.Point.Id);
			Gclass368_0.Add(groupPointSearchData);
			return groupPointSearchData;
		}
		return null;
	}

	public bool method_6(CoverSearchData data, GroupPointSearchData pointToCheck, DebugCoverFindGraphSearchData debug, out CustomNavigationPoint customNavigationPoint)
	{
		data.IncreasePointCheckCounter();
		if (data.PointsCheckedTotal <= LocalBotSettingsProviderClass.Core.MAX_POINS_CHECK_TOTAL && data.PointsCheckedRay <= LocalBotSettingsProviderClass.Core.MAX_POINS_CHECK_RAY)
		{
			if (pointToCheck == null)
			{
				customNavigationPoint = null;
				return false;
			}
			bool num = method_7(data, pointToCheck);
			debug.AddStep(pointToCheck, Gclass368_0);
			data.TryDropExtraPlacements(Gclass368_0.Count);
			if (num)
			{
				customNavigationPoint = pointToCheck.Point.GetById(data.Bot.Id);
				customNavigationPoint.CanIShootToEnemy = pointToCheck.CanIShootToEnemy;
				return true;
			}
			foreach (GroupPointWay neighbourhoodsWay in pointToCheck.Point.NeighbourhoodsWays)
			{
				method_5(neighbourhoodsWay.Target, data);
			}
			if (Gclass368_0.Count == 0)
			{
				customNavigationPoint = null;
				return false;
			}
			GroupPointSearchData groupPointSearchData = Gclass368_0.TakeFirstAndRemove();
			HashSet_1.Add(groupPointSearchData.Point.Id);
			return method_6(data, groupPointSearchData, debug, out customNavigationPoint);
		}
		data.DebugCoverPointsDataCollector.LeaveRecursive("MAX_POINS_CHECK_TOTAL || MAX_POINS_CHECK_RAY");
		customNavigationPoint = null;
		return false;
	}

	public bool method_7(CoverSearchData data, GroupPointSearchData check)
	{
		WildSpawnType type = data.Bot.Type;
		if ((type == WildSpawnType.assault || type == WildSpawnType.assaultGroup) && check.Point.CoverType == CoverType.Foliage)
		{
			return false;
		}
		if (data.Special > (ECoverPointSpecial)0 && !check.Point.Special.HasFlag(data.Special))
		{
			return false;
		}
		if (data.EnvironmentId.HasValue)
		{
			if (check.Point.IdEnvironment != data.EnvironmentId)
			{
				return false;
			}
		}
		else if (data.PlaceInfo.HasValue && check.Point.PlaceId != data.PlaceInfo)
		{
			return false;
		}
		if (data.Bot.Settings.Cover.DELETE_POINTS_BEHIND_ENEMIES && !method_2(data.Bot.Position, data.CarePositions, check.Point, data.EditorsCoverPointDebugData, data))
		{
			check.Cause = ECoverCheckCause.Behind;
			return false;
		}
		if (check.Point.IsSpotted)
		{
			check.Cause = ECoverCheckCause.IsSpotted;
			return false;
		}
		if (!check.Point.IsFreeById(data.Bot.Id))
		{
			check.Cause = ECoverCheckCause.NotFree;
			return false;
		}
		return data.CheckShootHide switch
		{
			ECheckSHootHide.shootAndHide => method_1(data.Bot, data.shootType, check, data.Shoot2Point, data.CarePositions, data.EditorsCoverPointDebugData, data), 
			ECheckSHootHide.shoot => method_0(data.Bot, data.shootType, check, data.Shoot2Point, data.CarePositions, data.EditorsCoverPointDebugData, data), 
			ECheckSHootHide.hide => method_4(check, data.CarePositions, data.EditorsCoverPointDebugData, data), 
			_ => true, 
		};
	}
}
