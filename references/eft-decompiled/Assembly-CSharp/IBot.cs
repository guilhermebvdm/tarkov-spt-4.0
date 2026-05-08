using System.Collections.Generic;
using EFT;
using UnityEngine;

public interface IBot
{
	WildSpawnType Type { get; }

	Vector3 Position { get; }

	EnemyInfo GoalEnemy { get; }

	BotSettingsComponents Settings { get; }

	IBossToFollow BossToFollow { get; }

	ICoversData Covers { get; }

	int Id { get; }

	bool IsDamaged { get; }

	BotGrenadeController BotGrenadeController { get; }

	LayerMask LookSensorMask { get; }

	CustomNavigationPoint CurCustomCoverPoint { get; }

	StandartBotBrain Brain { get; }

	CustomNavigationPoint LastCover { get; }

	int ConnectionGroupId { get; }

	HashSet<Vector3> CarePositions();

	List<NavGraphVoxelSimple> GetVoxelesInRadius(int step);

	float MaxShootDist();

	NavGraphVoxelSimple GetVoxelSafe(Vector3 centerPos);

	void SetLastSearchDebug(DebugCoverFindGraphSearchData debug);

	bool IsCurrentPointGood(CoverSearchType dataSearchType, CoverSearchData data, out CustomNavigationPoint point);

	List<NavGraphVoxelSimple> GetVoxelesExtended(Vector3 pos, int r, bool onlyWithPoints = false);

	GroupPoint GetClosest(Vector3 pos, int botCGid);

	GroupPoint GetClosestsPointVoxelesExtended(Vector3 pos, int rad, int connectionGroup, bool withFoliage = true);
}
