using System;
using System.Collections.Generic;
using EFT;
using UnityEngine;

public class BotCoverSearchInfo : GClass429, IBot
{
	[NonSerialized]
	public DebugCoverFindGraphSearchData LastDebug;

	[NonSerialized]
	public DebugCoverFindGraphClosests LastDebugClosests;

	public int DebugLastCoverDrawSteps;

	public int DebugLastCoverDrawClosests;

	public StandartBotBrain Brain => BotOwner_0.Brain;

	public CustomNavigationPoint LastCover => BotOwner_0.Memory.BotCurrentCoverInfo.LastCover;

	public int ConnectionGroupId => BotOwner_0.StartCorePoint.ConnectionGroupId;

	public WildSpawnType Type => BotOwner_0.Profile.Info.Settings.Role;

	public Vector3 Position => BotOwner_0.Position;

	public DebugCoverFindGraphSearchData LastDebugFunc => LastDebug;

	public DebugCoverFindGraphClosests LastDebugClosest => LastDebugClosests;

	public IBossToFollow BossToFollow => BotOwner_0.BotFollower.BossToFollow;

	public ICoversData Covers => BotOwner_0.Covers;

	public int Id => BotOwner_0.Id;

	public bool IsDamaged => BotOwner_0.Memory.IsDamaged;

	public BotGrenadeController BotGrenadeController => BotOwner_0.WeaponManager.Grenades;

	public LayerMask LookSensorMask => BotOwner_0.LookSensor.Mask;

	public CustomNavigationPoint CurCustomCoverPoint => BotOwner_0.Memory.CurCustomCoverPoint;

	public BotSettingsComponents Settings => BotOwner_0.Settings.FileSettings;

	public EnemyInfo GoalEnemy => BotOwner_0.Memory.GoalEnemy;

	public BotCoverSearchInfo(BotOwner owner)
		: base(owner)
	{
	}

	public HashSet<Vector3> CarePositions()
	{
		return BotOwner_0.CarePositions();
	}

	public List<NavGraphVoxelSimple> GetVoxelesInRadius(int step)
	{
		return BotOwner_0.Covers.GetVoxelesInRadius(step);
	}

	public float MaxShootDist()
	{
		return BotOwner_0.LookSensor.MaxShootDist;
	}

	public NavGraphVoxelSimple GetVoxelSafe(Vector3 centerPos)
	{
		return BotOwner_0.VoxelesPersonalData.GetVoxelSafe(centerPos);
	}

	public void SetLastSearchDebug(DebugCoverFindGraphSearchData debug)
	{
		LastDebug = debug;
	}

	public bool IsCurrentPointGood(CoverSearchType dataSearchType, CoverSearchData data, out CustomNavigationPoint point)
	{
		return BotOwner_0.BotsGroup.CoverPointMaster.IsCurrentPointGood(dataSearchType, data, out point);
	}

	public List<NavGraphVoxelSimple> GetVoxelesExtended(Vector3 pos, int r, bool onlyWithPoints = false)
	{
		return BotOwner_0.VoxelesPersonalData.GetVoxelesExtended(pos, r, onlyWithPoints);
	}

	public GroupPoint GetClosestsPointVoxelesExtended(Vector3 pos, int r, int connectionGroup, bool withFoliage)
	{
		return BotOwner_0.VoxelesPersonalData.GetClosestsPointVoxelesExtended(pos, r, connectionGroup, withFoliage);
	}

	public GroupPoint GetClosest(Vector3 pos, int botCGid)
	{
		return BotOwner_0.BotsController.CoversData.GetClosest(pos, botCGid);
	}

	public void TryDrawGizmosLastSearchDebugByFull()
	{
		if (DebugLastCoverDrawSteps > 0)
		{
			LastDebug?.GizmosDrawBySteps(DebugLastCoverDrawSteps);
		}
	}

	public void TryDrawGizmosLastSearchDebugClosest()
	{
		if (DebugLastCoverDrawClosests > 0)
		{
			LastDebugClosests?.GizmosDrawBySteps(DebugLastCoverDrawClosests);
		}
	}

	public void SetLastSearchClosestsDebug(GClass368<GroupPointSearchData> list, Vector3 startPos, CustomNavigationPoint taken)
	{
		if (LastDebugClosests == null)
		{
			LastDebugClosests = new DebugCoverFindGraphClosests();
		}
		LastDebugClosests.UpdateList(list, startPos, taken);
	}
}
