using System;
using System.Collections.Generic;
using EFT;
using UnityEngine;

public interface GInterface6
{
	bool EnoughtHaveGoodCovers { get; }

	Vector3 LootPosition { get; }

	bool ShallAttack { get; }

	event Action OnThrowRequest;

	void SupressRequestTaken();

	bool CanStartSuppress();

	Dictionary<BotOwner, Vector3> FollowersPositions();

	bool ISeeEnemy();

	void SetHaveGoodCover(BotOwner owner, bool haveGoodCover);

	bool TooManyEnemies();

	Vector3 GetTargetToLook();

	bool IsRunning();

	void SetWannaFight();

	bool IsNearLoot(EnemyInfo enemy);
}
