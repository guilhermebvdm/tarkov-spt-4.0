using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using EFT;
using JetBrains.Annotations;
using UnityEngine;

public class CoverPointMaster
{
	public struct GStruct10
	{
		public Vector3 CoverPos;

		public float MinDistToFriend;
	}

	[CompilerGenerated]
	public class Class110
	{
		public int recursionLevel;

		public Tuple<float, float> method_0()
		{
			return NearCoverGroupsCache.GetSearchRadius(recursionLevel);
		}
	}

	public const float LOWEST_ANG_COVER = 0.1736482f;

	[NonSerialized]
	public static GClass391 NearCoverGroupsCache = new GClass391();

	[NonSerialized]
	public GClass381 FinderByGraph = new GClass381();

	[NonSerialized]
	public bool CanDraw = true;

	[NonSerialized]
	public List<CustomNavigationPoint> ListToDel = new List<CustomNavigationPoint>(130);

	[NonSerialized]
	public HashSet<CustomNavigationPoint> NearGroups = new HashSet<CustomNavigationPoint>();

	[NonSerialized]
	public HashSet<CustomNavigationPoint> NearGroupByPlace = new HashSet<CustomNavigationPoint>();

	[field: NonSerialized]
	public BotZone BotZone { get; }

	public static CoverPointMaster Create(BotZone zone)
	{
		return new CoverPointMaster(zone);
	}

	public static CustomNavigationPoint GetNearPoint(CustomNavigationPoint[] coverPointGroups, Vector3 playerPos, out float sDist)
	{
		CustomNavigationPoint result = null;
		sDist = float.MaxValue;
		foreach (CustomNavigationPoint customNavigationPoint in coverPointGroups)
		{
			float sqrMagnitude = (playerPos - customNavigationPoint.Position).sqrMagnitude;
			if (sqrMagnitude < sDist)
			{
				sDist = sqrMagnitude;
				result = customNavigationPoint;
			}
		}
		return result;
	}

	public static HashSet<CustomNavigationPoint> smethod_0(CustomNavigationPoint[] coverPointGroups, CoverSearchData data, int recursionLevel)
	{
		return NearCoverGroupsCache.GetNearGroups(coverPointGroups, data, recursionLevel);
	}

	public static Func<Tuple<float, float>> smethod_1(int recursionLevel)
	{
		return () => NearCoverGroupsCache.GetSearchRadius(recursionLevel);
	}

	public CoverPointMaster(BotZone zone)
	{
		BotZone = zone;
	}

	public CustomNavigationPoint GetFreeClosePoint(Vector3 pos, BotCoversData bot, float minSDistToEnemy)
	{
		return bot.GetFreeClosePoint(pos, minSDistToEnemy);
	}

	[CanBeNull]
	public CustomNavigationPoint GetCoverPointMain(CoverSearchData data, bool checkCurrent)
	{
		if (data.UseSelfFindPoint)
		{
			return data.Bot.Brain.FindPoint(data, method_0, checkCurrent);
		}
		return method_0(data);
	}

	public bool IsCurrentPointGood(CoverSearchType searchType, CoverSearchData data, out CustomNavigationPoint point)
	{
		point = data.Bot.CurCustomCoverPoint;
		if (point == null)
		{
			return false;
		}
		if (point.CoverType == CoverType.Foliage)
		{
			WildSpawnType type = data.Bot.Type;
			if (type == WildSpawnType.assault || type == WildSpawnType.assaultGroup)
			{
				return false;
			}
		}
		if (point.IsSpotted)
		{
			return false;
		}
		if (!point.IsFreeById(data.Bot.Id))
		{
			return false;
		}
		if (data.PlaceInfo.HasValue && data.PlaceInfo.Value != point.PlaceId)
		{
			return false;
		}
		HashSet<Vector3> carePositions = data.CarePositions;
		if (!point.CanIHide(carePositions, data.Bot.Settings.Cover.MIN_TO_ENEMY_TO_BE_NOT_SAFE_SQRT, data.UseLineCastToCover, data.UseAngCastToCover))
		{
			return false;
		}
		switch (searchType)
		{
		case CoverSearchType.shoot_toCover_toBot_Distances:
			if (GClass369.CanShootToTarget(data.Shoot2Point, point, data.Bot.LookSensorMask))
			{
				return true;
			}
			break;
		case CoverSearchType.distToBot:
			return true;
		case CoverSearchType.distToBotAndToCenter:
			return true;
		case CoverSearchType.distToToCenter:
			if (data.Shoot2Point != null && GClass369.CanShootToTarget(data.Shoot2Point, point, data.Bot.LookSensorMask))
			{
				return true;
			}
			return false;
		case CoverSearchType.closerToSelectedPoint:
		{
			if (data.shootType == CoverShootType.shoot && data.Shoot2Point != null && !GClass369.CanShootToTarget(data.Shoot2Point, point, data.Bot.LookSensorMask))
			{
				return false;
			}
			float gOOD_DIST_TO_POINT_COEF = data.Bot.Settings.Cover.GOOD_DIST_TO_POINT_COEF;
			float num = LocalBotSettingsProviderClass.Core.GOOD_DIST_TO_POINT * gOOD_DIST_TO_POINT_COEF;
			float num2 = num * num;
			if (!data.PointToBeClose.HasValue)
			{
				break;
			}
			Vector3 vector = point.Position - data.PointToBeClose.Value;
			if (Mathf.Abs(vector.y) < LocalBotSettingsProviderClass.Core.MAX_Y_DIFF_TO_PROTECT && vector.sqrMagnitude < num2)
			{
				if (data.shootType == CoverShootType.hide || data.Shoot2Point == null)
				{
					return true;
				}
				if (GClass369.CanShootToTarget(data.Shoot2Point, point, data.Bot.LookSensorMask))
				{
					return true;
				}
			}
			break;
		}
		}
		return false;
	}

	public List<CustomNavigationPoint> GetClosePoints(Vector3 pos, BotOwner bot, float dist)
	{
		return bot.Covers.GetClosePoints(pos, dist);
	}

	[CanBeNull]
	public CustomNavigationPoint ClosestGreenPoint(Vector3 point, BotOwner bot, GStruct10? friendCover, int? placeId, out float sDist)
	{
		return ClosestGreenPoint(point, bot, friendCover, placeId, CoverLevel.Lay, out sDist);
	}

	[CanBeNull]
	public CustomNavigationPoint ClosestGreenPoint(Vector3 point, BotOwner bot, GStruct10? friendCover, int? placeId, CoverLevel minCoverLevel, out float sDist)
	{
		return bot.Covers.ClosestGreenPoint(point, out sDist, 0f, null, placeId, friendCover, minCoverLevel);
	}

	[CanBeNull]
	public CustomNavigationPoint ClosestGreenPoint(Vector3 point, BotOwner bot, float minDistToTarget, Vector3 trg, GStruct10? friendCover, out float sDist)
	{
		return bot.Covers.ClosestGreenPoint(point, out sDist, minDistToTarget, trg, null, friendCover);
	}

	[CanBeNull]
	public CustomNavigationPoint method_0(CoverSearchData data)
	{
		if (data.shootType != CoverShootType.hide && data.Shoot2Point == null)
		{
			data.shootType = CoverShootType.hide;
		}
		Stopwatch stopwatch = new Stopwatch();
		stopwatch.Start();
		CustomNavigationPoint customNavigationPoint = FinderByGraph.GetCover(data);
		if (customNavigationPoint == null)
		{
			customNavigationPoint = data.Bot.Covers.GetFreeClosePoint(data.CenterPos, 1f);
			if (customNavigationPoint != null)
			{
				customNavigationPoint.CanIShootToEnemy = false;
			}
		}
		if (customNavigationPoint == null)
		{
			customNavigationPoint = data.Bot.Covers.FindClosestPoint(data.CenterPos);
			if (customNavigationPoint != null)
			{
				customNavigationPoint.CanIShootToEnemy = false;
			}
		}
		if (customNavigationPoint == null)
		{
			NavGraphVoxelSimple voxelSafe = data.Bot.GetVoxelSafe(data.CenterPos);
			if (voxelSafe != null)
			{
				customNavigationPoint = voxelSafe.PointStartSearch.GetById(data.Bot.Id);
				customNavigationPoint.CanIShootToEnemy = false;
			}
		}
		stopwatch.Stop();
		if (data.PointToBeClose.HasValue)
		{
			float sqrMagnitude = (customNavigationPoint.Position - data.PointToBeClose.Value).sqrMagnitude;
			float num = LocalBotSettingsProviderClass.Core.GOOD_DIST_TO_POINT * data.Bot.Settings.Cover.GOOD_DIST_TO_POINT_COEF;
			if (sqrMagnitude > num * num)
			{
				CustomNavigationPoint freeClosePoint = data.Bot.Covers.GetFreeClosePoint(data.PointToBeClose.Value, data.MinSDistToCarePos);
				GClass398.Instance.IsTraceEnable();
				return freeClosePoint;
			}
		}
		return customNavigationPoint;
	}

	public void Dispose()
	{
		ListToDel.Clear();
		NearGroups.Clear();
		NearGroupByPlace.Clear();
		NearCoverGroupsCache.Dispose();
		NearGroups.Clear();
		NearGroupByPlace.Clear();
	}
}
