using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using EFT;
using JetBrains.Annotations;
using UnityEngine;

public class GClass404 : IBot, ICoversData
{
	public DebugCoverSystemTester Tester;

	public CustomNavigationPoint[] _coverPoints;

	public CustomNavigationPoint[] _ambushPoints;

	public CustomNavigationPoint[] _bushPoints;

	public CustomNavigationPoint[] _allPoints;

	public CustomNavigationPoint[] _allPointsWithBush;

	[NonSerialized]
	public NavGraphContainer NavGraphContainer_0;

	[NonSerialized]
	public DebugCoverFindGraphSearchData DebugCoverFindGraphSearchData_0;

	[NonSerialized]
	[CompilerGenerated]
	public WildSpawnType WildSpawnType_0;

	[NonSerialized]
	[CompilerGenerated]
	public BotSettingsComponents BotSettingsComponents_0;

	[NonSerialized]
	[CompilerGenerated]
	public LayerMask LayerMask_0;

	[NonSerialized]
	[CompilerGenerated]
	public CustomNavigationPoint CustomNavigationPoint_0;

	[NonSerialized]
	[CompilerGenerated]
	public int Int_0;

	[NonSerialized]
	[CompilerGenerated]
	public CustomNavigationPoint CustomNavigationPoint_1;

	public WildSpawnType Type
	{
		[CompilerGenerated]
		get
		{
			return WildSpawnType_0;
		}
		[CompilerGenerated]
		set
		{
			WildSpawnType_0 = value;
		}
	}

	public Vector3 Position => Tester.transform.position;

	public bool GetTrulyDist => false;

	public bool Protecting => false;

	public EnemyInfo GoalEnemy => Tester.GetGoalEnemy();

	public BotSettingsComponents Settings
	{
		[CompilerGenerated]
		get
		{
			return BotSettingsComponents_0;
		}
		[CompilerGenerated]
		set
		{
			BotSettingsComponents_0 = value;
		}
	}

	public IBossToFollow BossToFollow => null;

	public ICoversData Covers => this;

	public int Id => 1;

	public bool IsDamaged => false;

	public BotGrenadeController BotGrenadeController => null;

	public LayerMask LookSensorMask
	{
		[CompilerGenerated]
		get
		{
			return LayerMask_0;
		}
		[CompilerGenerated]
		set
		{
			LayerMask_0 = value;
		}
	}

	public CustomNavigationPoint CurCustomCoverPoint => null;

	public StandartBotBrain Brain => null;

	public CustomNavigationPoint LastCover
	{
		[CompilerGenerated]
		get
		{
			return CustomNavigationPoint_0;
		}
	}

	public bool Boolean_0 => false;

	public CustomNavigationPoint[] CoverPoints => _coverPoints;

	public CustomNavigationPoint[] AmbushPoints => _ambushPoints;

	public CustomNavigationPoint[] BushPoints => _bushPoints;

	public CustomNavigationPoint[] AllPoints => _allPoints;

	public CustomNavigationPoint[] AllPointsWithBush => _allPointsWithBush;

	public int ConnectionGroupId
	{
		[CompilerGenerated]
		get
		{
			return Int_0;
		}
	}

	public CustomNavigationPoint LastCustomNavigationPoint
	{
		[CompilerGenerated]
		get
		{
			return CustomNavigationPoint_1;
		}
	}

	public void Init(DebugCoverSystemTester tester)
	{
		Type = tester.WildSpawnType;
		Tester = tester;
		Settings = LocalBotSettingsProviderClass.GetSettings(BotDifficulty.normal, Type, Boolean_0);
		LookSensorMask = LayerMaskClass.HighPolyWithTerrainMask;
	}

	public HashSet<Vector3> CarePositions()
	{
		return Tester.CarePositions();
	}

	public List<NavGraphVoxelSimple> GetVoxelesInRadius(int step)
	{
		Debug.LogError("not implemented todo");
		return new List<NavGraphVoxelSimple>();
	}

	public float MaxShootDist()
	{
		return Tester.MaxShootDist;
	}

	public NavGraphVoxelSimple GetVoxelSafe(Vector3 centerPos)
	{
		return null;
	}

	public void SetPathToCover(CustomNavigationPoint resultPoint)
	{
		Debug.Log("SetPathToCover ok");
	}

	public NavGraphEditorPoint NextPointToGo()
	{
		return null;
	}

	public NavGraphContainer GetNavGraphContainer()
	{
		return NavGraphContainer_0;
	}

	public void SetLastSearchDebug(DebugCoverFindGraphSearchData debug)
	{
		Debug.Log("SetLastSearchDebug");
		DebugCoverFindGraphSearchData_0 = debug;
	}

	public bool IsCurrentPointGood(CoverSearchType dataSearchType, CoverSearchData data, out CustomNavigationPoint point)
	{
		point = null;
		return false;
	}

	public List<NavGraphVoxelSimple> GetVoxelesExtended(Vector3 pos, int r, bool onlyWithPoints = false)
	{
		throw new NotImplementedException();
	}

	public List<NavGraphVoxelSimple> GetVoxelesExtended(Vector3 pos, int r)
	{
		throw new NotImplementedException();
	}

	public CustomNavigationPoint FindClosestPoint(Vector3 dataCenterPos, bool noRestrictions = false, [CanBeNull] Func<GroupPoint, bool> goodFunc = null)
	{
		return null;
	}

	public CustomNavigationPoint GetFreeClosePoint(Vector3 pos, float minSDistToEnemy)
	{
		return null;
	}

	public GroupPoint GetClosest(Vector3 center, int connectionGroupId)
	{
		throw new NotImplementedException();
	}

	public GroupPoint GetClosestsPointVoxelesExtended(Vector3 pos, int rad, int connectionGroup, bool withFoliage)
	{
		throw new NotImplementedException();
	}

	public GroupPoint GetClosestByClosestsVoxeles(Vector3 center)
	{
		throw new NotImplementedException();
	}
}
