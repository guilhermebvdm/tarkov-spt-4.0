using System;
using System.Collections.Generic;
using EFT;
using EFT.EnvironmentEffect;
using EFT.Interactive;
using EFT.InventoryLogic;
using UnityEngine;

public interface IAIData
{
	IAIDataRoomLogic roomLogicLogic { get; }

	HashSet<AIPlaceInfo> CurrentZones { get; }

	bool IsAI { get; }

	bool UseZombieSimpleAnimator { get; }

	BotOwner BotOwner { get; set; }

	AIDataRequestController AskRequests { get; }

	int EnvironmentId { get; }

	AIPlaceInfo PlaceInfo { get; }

	bool HaveHelmet { get; }

	bool IsAxe { get; }

	bool ShallPursuit { get; }

	bool IsInTree { get; }

	float FoliageIntersectionPercent { get; }

	Transform CurrentFoliageRoot { get; }

	float PowerOfEquipment { get; }

	float LastShotTime { get; }

	bool IsScavAttacker { get; }

	bool IAmBoss { get; }

	bool IsNoOffsetShooting { get; }

	bool PlayerLoyaltyIsAggressor { get; }

	bool PlayerLoyaltyBossNoAttack { get; }

	bool PlayerLoyaltyCanBeFreeKilled { get; }

	string PlayerProfileNickname { get; }

	Player Player { get; }

	float FlarePower { get; }

	bool IsInside { get; }

	bool UsingLight { get; }

	bool GetFlare { get; }

	float PoseVisibilityCoef { get; }

	AIBossPlayer AIBossPlayer { get; }

	float LastResetPlaceInfo { get; }

	bool IsDrunk { get; }

	DateTime DrinkTimestamp { get; }

	bool IsMute { get; }

	event Action<PlayerAIDataClass> OnBecomeScavAttacker;

	event Action<PlayerAIDataClass> OnBecomeDrunk;

	event Action<AIPlaceInfo> OnComeToPlace;

	event Action<AIPlaceInfo> OnLeavePlace;

	void Activate();

	void KillEnemy(Player player);

	void TryPlayShootSound(Player getPlayer, Vector3 position, AISoundType spredPower);

	void SetStationaryWeapon(Weapon stationaryWeaponItem);

	void TacticalModeChange(bool p0);

	void SetEnvironment(IndoorTrigger trigger);

	bool IsBossOrFollowerRequireRevenge();

	void LeavePlace(AIPlaceInfo aiPlaceInfo);

	void AddPlaceInfo(AIPlaceInfo aiPlaceInfo);

	void Dispose();

	void BecomeDrunk();

	void SetAttackByLoyalityScav();

	void DrawGizmosSelectedVoxel();

	void CalcPower();

	void EnterTree(TreeInteractive treeInteractive);

	void StayInTree(TreeInteractive treeInteractive, Collider enteredCollider, Collider triggerCollider);

	void ExitTree(TreeInteractive treeInteractive);

	void SetPosToVoxel(Vector3 ownerPosition);

	void LateUpdate();

	void DrawGizmosFoliageIntersection();
}
