using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using EFT;
using JetBrains.Annotations;
using UnityEngine;

public class BotCoversData : GClass429, ICoversData
{
	[CompilerGenerated]
	public class Class178
	{
		public BotCoversData botCoversData_0;

		public float minSDistToEnemy;

		public bool method_0(GroupPoint p)
		{
			if (p.IsSpotted)
			{
				return false;
			}
			EnemyInfo goalEnemy = botCoversData_0.BotOwner_0.Memory.GoalEnemy;
			if (goalEnemy != null && (goalEnemy.CurrPosition - p.Position).sqrMagnitude < minSDistToEnemy)
			{
				return false;
			}
			return true;
		}
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	public struct Struct9
	{
		public Vector3 pos;

		public float sDist;

		public List<CustomNavigationPoint> list;

		public BotCoversData botCoversData_0;
	}

	[CompilerGenerated]
	public class Class179
	{
		public BotCoversData botCoversData_0;

		public CoverLevel coverLvl;

		public bool checkPlaceID;

		public int? placeId;

		public CoverPointMaster.GStruct10? friendCover;

		public bool checksDistMinToTarget;

		public Vector3? trg;

		public float sDistMinToTarget;

		public bool method_0(GroupPoint p)
		{
			if (p.CoverType != CoverType.Foliage)
			{
				return false;
			}
			if (!botCoversData_0.method_0(p, coverLvl))
			{
				return false;
			}
			if (checkPlaceID && p.PlaceId != placeId)
			{
				return false;
			}
			if (friendCover.HasValue && (friendCover.Value.CoverPos - p.Position).sqrMagnitude < friendCover.Value.MinDistToFriend)
			{
				return false;
			}
			if (checksDistMinToTarget && (trg.Value - p.Position).sqrMagnitude < sDistMinToTarget)
			{
				return false;
			}
			return true;
		}
	}

	[CompilerGenerated]
	public class Class180
	{
		public int? placeId;

		public bool onlyWithInsideCover;

		public float minLvl;

		public CoverPointMaster.GStruct10? friednData;

		public BotCoversData botCoversData_0;

		public bool method_0(GroupPoint point)
		{
			if (placeId.HasValue)
			{
				if (point.PlaceId <= 0)
				{
					return false;
				}
				if (point.PlaceId != placeId.Value)
				{
					return false;
				}
			}
			if (onlyWithInsideCover && !point.IsGoodInsideBuilding)
			{
				return false;
			}
			if ((float)point.HideLevel > minLvl)
			{
				if (friednData.HasValue && (friednData.Value.CoverPos - point.Position).sqrMagnitude < friednData.Value.MinDistToFriend)
				{
					return false;
				}
				if (!point.IsFreeById(botCoversData_0.BotOwner_0.Id) || point.IsSpotted)
				{
					return false;
				}
			}
			return true;
		}
	}

	[CompilerGenerated]
	public class Class181
	{
		public BotCoversData botCoversData_0;

		public Vector3 trg;

		public float sMinDist;

		public Func<GroupPoint, bool> isGood;

		public bool method_0(GroupPoint point)
		{
			if (!point.IsFreeById(botCoversData_0.BotOwner_0.Id))
			{
				return false;
			}
			if (point.IsSpotted)
			{
				return false;
			}
			if ((trg - point.Position).sqrMagnitude < sMinDist)
			{
				return false;
			}
			if (isGood != null && !isGood(point))
			{
				return false;
			}
			return true;
		}
	}

	[CompilerGenerated]
	public class Class182
	{
		public Vector3 botPos;

		public bool method_0(Vector3 position)
		{
			if (GClass856.SqrDistance(position, botPos) > LocalBotSettingsProviderClass.Core.MAX_DANGER_CARE_DIST_SQRT)
			{
				return true;
			}
			return false;
		}
	}

	public BotOwner LastClosestFriend;

	public float LastClosestFriendSDist;

	public GClass531 CoverFinderAnalyzer;

	[NonSerialized]
	public GClass380 Gclass380_0 = new GClass380();

	[field: NonSerialized]
	public CoverSearchData LastSearchData { get; set; }

	public int ConnectionGroupId => BotOwner_0.StartCorePoint.ConnectionGroupId;

	public CustomNavigationPoint LastCustomNavigationPoint => BotOwner_0.Memory.BotCurrentCoverInfo.LastCover;

	public BotCoversData([NotNull] BotOwner owner)
		: base(owner)
	{
		CoverFinderAnalyzer = new GClass531(owner);
	}

	public virtual HashSet<Vector3> CarePositions()
	{
		HashSet<Vector3> hashSet = new HashSet<Vector3>();
		foreach (BotSettingsClass value in BotOwner_0.BotsGroup.Enemies.Values)
		{
			if (value.IsHaveSeen && Time.time - value.EnemyLastSeenTimeReal < LocalBotSettingsProviderClass.Core.CARE_ENEMY_ONLY_TIME)
			{
				hashSet.Add(value.EnemyLastPosition);
			}
		}
		if (BotOwner_0.Memory.GoalTarget.HavePlaceTarget() && BotOwner_0.Memory.GoalTarget.IsDanger)
		{
			hashSet.Add(BotOwner_0.Memory.GoalTarget.GoalTarget.BasePoint);
		}
		if (BotOwner_0.BewareGrenade.GrenadeDangerPoint?.Grenade != null)
		{
			hashSet.Add(BotOwner_0.BewareGrenade.GrenadeDangerPoint.DangerPoint);
		}
		if (BotOwner_0.ArtilleryDangerPlace.IsActive)
		{
			foreach (KeyValuePair<string, List<CustomNavigationPoint>> activeZonesSpottedCover in BotOwner_0.ArtilleryDangerPlace.ActiveZonesSpottedCovers)
			{
				if (BotOwner_0.BotsController.ArtilleryZonesController.TryGetZoneById(activeZonesSpottedCover.Key, out var zoneData))
				{
					hashSet.Add(zoneData.Position);
				}
			}
		}
		Vector3 botPos = BotOwner_0.Transform.position;
		hashSet.RemoveWhere((Vector3 position) => (GClass856.SqrDistance(position, botPos) > LocalBotSettingsProviderClass.Core.MAX_DANGER_CARE_DIST_SQRT) ? true : false);
		return hashSet;
	}

	public CustomNavigationPoint GetFreeClosePoint(Vector3 pos, float minSDistToEnemy, bool printErrorLogsIfFail = false)
	{
		Func<GroupPoint, bool> goodFunc = delegate(GroupPoint p)
		{
			if (p.IsSpotted)
			{
				return false;
			}
			EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
			return (goalEnemy == null || !((goalEnemy.CurrPosition - p.Position).sqrMagnitude < minSDistToEnemy)) ? true : false;
		};
		return Gclass380_0.GetClosestPoint(BotOwner_0, pos, noRestrictions: false, goodFunc, printErrorLogsIfFail);
	}

	public List<CustomNavigationPoint> GetClosePoints(Vector3 pos, float dist)
	{
		Struct9 struct9_ = default(Struct9);
		struct9_.pos = pos;
		struct9_.botCoversData_0 = this;
		struct9_.sDist = dist * dist;
		struct9_.list = new List<CustomNavigationPoint>();
		try
		{
			NavGraphVoxelSimple voxelSafe = BotOwner_0.VoxelesPersonalData.GetVoxelSafe(struct9_.pos);
			float num = 10f;
			int num2 = (int)(dist / num) + 1;
			method_1(voxelSafe, ref struct9_);
			for (int i = 1; i <= num2; i++)
			{
				foreach (NavGraphVoxelSimple item in GetVoxelesInRadius(i))
				{
					method_1(item, ref struct9_);
				}
			}
		}
		catch (Exception)
		{
			Debug.LogError($"GetClosePoints error:{BotOwner_0?.Id}");
		}
		return struct9_.list;
	}

	public CustomNavigationPoint ClosestGreenPoint(Vector3 pos, out float sDist, float minDistToTarget = 0f, Vector3? trg = null, int? placeId = null, CoverPointMaster.GStruct10? friendCover = null, CoverLevel coverLvl = CoverLevel.Stay, bool printErrorLogsIfFail = false)
	{
		float sDistMinToTarget = minDistToTarget * minDistToTarget;
		bool checkPlaceID = placeId.HasValue && placeId > 0;
		bool checksDistMinToTarget = sDistMinToTarget > 0f && trg.HasValue;
		Func<GroupPoint, bool> goodFunc = delegate(GroupPoint p)
		{
			if (p.CoverType != CoverType.Foliage)
			{
				return false;
			}
			if (!method_0(p, coverLvl))
			{
				return false;
			}
			if (checkPlaceID && p.PlaceId != placeId)
			{
				return false;
			}
			if (friendCover.HasValue && (friendCover.Value.CoverPos - p.Position).sqrMagnitude < friendCover.Value.MinDistToFriend)
			{
				return false;
			}
			return (!checksDistMinToTarget || !((trg.Value - p.Position).sqrMagnitude < sDistMinToTarget)) ? true : false;
		};
		CustomNavigationPoint closestPoint = Gclass380_0.GetClosestPoint(BotOwner_0, pos, noRestrictions: false, goodFunc, printErrorLogsIfFail);
		if (closestPoint != null)
		{
			sDist = (closestPoint.Position - pos).sqrMagnitude;
		}
		else
		{
			sDist = 0f;
		}
		return closestPoint;
	}

	public CustomNavigationPoint GetClosestPoint(Vector3 pos, Func<GroupPoint, bool> goodFunc = null, bool printErrorLogsIfFail = false, int maxIterations = 1000)
	{
		bool noRestrictions = goodFunc == null;
		return Gclass380_0.GetClosestPoint(BotOwner_0, pos, noRestrictions, goodFunc, printErrorLogsIfFail, maxIterations);
	}

	[CanBeNull]
	public CustomNavigationPoint FindHidePoint(Vector3 pos, float minLvl, CoverPointMaster.GStruct10? friednData = null, bool onlyWithInsideCover = false, int? placeId = null, bool printErrorLogsIfFail = false)
	{
		Func<GroupPoint, bool> goodFunc = delegate(GroupPoint point)
		{
			if (placeId.HasValue)
			{
				if (point.PlaceId <= 0)
				{
					return false;
				}
				if (point.PlaceId != placeId.Value)
				{
					return false;
				}
			}
			if (onlyWithInsideCover && !point.IsGoodInsideBuilding)
			{
				return false;
			}
			if ((float)point.HideLevel > minLvl)
			{
				if (friednData.HasValue && (friednData.Value.CoverPos - point.Position).sqrMagnitude < friednData.Value.MinDistToFriend)
				{
					return false;
				}
				if (!point.IsFreeById(BotOwner_0.Id) || point.IsSpotted)
				{
					return false;
				}
			}
			return true;
		};
		return Gclass380_0.GetClosestPoint(BotOwner_0, pos, noRestrictions: false, goodFunc, printErrorLogsIfFail);
	}

	[CanBeNull]
	public CustomNavigationPoint FindClosestPoint(Vector3 pos, float minDistToTarget, Vector3 trg, bool noRestrictions = false, Func<GroupPoint, bool> isGood = null, bool printErrorLogsIfFail = false)
	{
		float sMinDist = minDistToTarget * minDistToTarget;
		_ = BotOwner_0.VoxelesPersonalData.CurVoxel;
		Func<GroupPoint, bool> goodFunc = delegate(GroupPoint point)
		{
			if (!point.IsFreeById(BotOwner_0.Id))
			{
				return false;
			}
			if (point.IsSpotted)
			{
				return false;
			}
			if ((trg - point.Position).sqrMagnitude < sMinDist)
			{
				return false;
			}
			return (isGood == null || isGood(point)) ? true : false;
		};
		return Gclass380_0.GetClosestPoint(BotOwner_0, pos, noRestrictions, goodFunc, printErrorLogsIfFail);
	}

	[CanBeNull]
	public List<NavGraphVoxelSimple> GetVoxelesInRadius(int step)
	{
		List<NavGraphVoxelSimple> list = new List<NavGraphVoxelSimple>();
		NavGraphVoxelSimple curVoxel = BotOwner_0.VoxelesPersonalData.CurVoxel;
		if (curVoxel == null)
		{
			BotOwner_0.AIData.SetPosToVoxel(BotOwner_0.Position);
			curVoxel = BotOwner_0.VoxelesPersonalData.CurVoxel;
		}
		int maxX = BotOwner_0.VoxelesPersonalData.MaxX;
		_ = BotOwner_0.VoxelesPersonalData.MaxY;
		int maxZ = BotOwner_0.VoxelesPersonalData.MaxZ;
		int num = Mathf.Clamp(curVoxel.IndexX + step, 0, maxX);
		ushort indexY = curVoxel.IndexY;
		int num2 = Mathf.Clamp(curVoxel.IndexZ + step, 0, maxZ);
		int num3 = Mathf.Clamp(curVoxel.IndexX - step, 0, maxX);
		int num4 = Mathf.Clamp(curVoxel.IndexZ - step, 0, maxZ);
		for (int i = num3; i < num; i++)
		{
			NavGraphVoxelSimple voxelSafeByIndex = BotOwner_0.VoxelesPersonalData.GetVoxelSafeByIndex(i, indexY, num2);
			list.Add(voxelSafeByIndex);
		}
		for (int num5 = num2 - 1; num5 >= num4; num5--)
		{
			NavGraphVoxelSimple voxelSafeByIndex2 = BotOwner_0.VoxelesPersonalData.GetVoxelSafeByIndex(num, indexY, num5);
			list.Add(voxelSafeByIndex2);
		}
		for (int num6 = num - 1; num6 >= num3; num6--)
		{
			NavGraphVoxelSimple voxelSafeByIndex3 = BotOwner_0.VoxelesPersonalData.GetVoxelSafeByIndex(num6, indexY, num4);
			list.Add(voxelSafeByIndex3);
		}
		for (int j = num4; j < num2; j++)
		{
			NavGraphVoxelSimple voxelSafeByIndex4 = BotOwner_0.VoxelesPersonalData.GetVoxelSafeByIndex(num3, indexY, j);
			list.Add(voxelSafeByIndex4);
		}
		return list;
	}

	[CanBeNull]
	public BotOwner GetClosestFriend(out float sqrDist)
	{
		float num = float.MaxValue;
		BotOwner botOwner = null;
		sqrDist = 0f;
		for (int i = 0; i < BotOwner_0.BotsGroup.MembersCount; i++)
		{
			BotOwner botOwner2 = BotOwner_0.BotsGroup.Member(i);
			if (!(botOwner2 == BotOwner_0))
			{
				float sqrMagnitude = (BotOwner_0.Position - botOwner2.Transform.position).sqrMagnitude;
				if (sqrMagnitude < num)
				{
					botOwner = botOwner2;
					num = (sqrDist = sqrMagnitude);
				}
			}
		}
		LastClosestFriend = botOwner;
		LastClosestFriendSDist = sqrDist;
		return botOwner;
	}

	public float DistToClosestFriend()
	{
		GetClosestFriend(out var sqrDist);
		return Mathf.Sqrt(sqrDist);
	}

	public Vector3? ClosestFriendCoverPoint()
	{
		return ClosestFriendCoverPoint(BotOwner_0.Position);
	}

	public Vector3? ClosestFriendCoverPoint(Vector3 closeToPoint)
	{
		float num = float.MaxValue;
		Vector3? result = null;
		for (int i = 0; i < BotOwner_0.BotsGroup.MembersCount; i++)
		{
			BotOwner botOwner = BotOwner_0.BotsGroup.Member(i);
			if (!(botOwner == BotOwner_0))
			{
				Vector3 vector = ((botOwner.Memory.CurCustomCoverPoint == null) ? botOwner.Position : botOwner.Memory.CurCustomCoverPoint.Position);
				float sqrMagnitude = (closeToPoint - vector).sqrMagnitude;
				if (sqrMagnitude < num)
				{
					result = vector;
					num = sqrMagnitude;
				}
			}
		}
		return result;
	}

	public void SetClose()
	{
	}

	public void SetLong()
	{
	}

	public void SetLastCoverSearchInfo(CoverSearchData data)
	{
		LastSearchData = data;
	}

	public bool method_0(GroupPoint point, CoverLevel minCoverLevel)
	{
		switch (minCoverLevel)
		{
		default:
			return false;
		case CoverLevel.Stay:
			return true;
		case CoverLevel.Sit:
			if (point.CoverLevel != CoverLevel.Lay)
			{
				return point.CoverLevel == CoverLevel.Sit;
			}
			return true;
		case CoverLevel.Lay:
			return point.CoverLevel == CoverLevel.Lay;
		}
	}

	public CustomNavigationPoint FindClosestPoint_1(Vector3 pos, bool noRestrictions, Func<GroupPoint, bool> goodFunc)
	{
		return Gclass380_0.GetClosestPoint(BotOwner_0, pos, noRestrictions, goodFunc, printErrorLogsIfFail: false);
	}

	CustomNavigationPoint ICoversData.FindClosestPoint(Vector3 pos, bool noRestrictions, Func<GroupPoint, bool> goodFunc)
	{
		//ILSpy generated this explicit interface implementation from .override directive in FindClosestPoint_1
		return this.FindClosestPoint_1(pos, noRestrictions, goodFunc);
	}

	public CustomNavigationPoint GetFreeClosePoint_1(Vector3 pos, float minSDistToEnemy)
	{
		return Gclass380_0.GetClosestPoint(BotOwner_0, pos, noRestrictions: true, null, printErrorLogsIfFail: false);
	}

	CustomNavigationPoint ICoversData.GetFreeClosePoint(Vector3 pos, float minSDistToEnemy)
	{
		//ILSpy generated this explicit interface implementation from .override directive in GetFreeClosePoint_1
		return this.GetFreeClosePoint_1(pos, minSDistToEnemy);
	}

	public GroupPoint GetClosest(Vector3 center, int connectionGroupId)
	{
		return BotOwner_0.BotsController.CoversData.GetClosest(center, connectionGroupId);
	}

	public GroupPoint GetClosestByClosestsVoxeles(Vector3 center)
	{
		return GClass380.FoundStartPointExtended(BotOwner_0.CoverSearchInfo, center);
	}

	public void Dispose()
	{
	}

	public void Init()
	{
	}

	[CompilerGenerated]
	public void method_1(NavGraphVoxelSimple voxel, ref Struct9 struct9_0)
	{
		if (voxel == null || voxel.Points == null)
		{
			return;
		}
		foreach (GroupPoint point in voxel.Points)
		{
			if ((point.Position - struct9_0.pos).sqrMagnitude < struct9_0.sDist)
			{
				struct9_0.list.Add(point.GetById(BotOwner_0.Id));
			}
		}
	}
}
