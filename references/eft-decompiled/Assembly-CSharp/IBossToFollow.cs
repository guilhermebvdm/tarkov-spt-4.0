using System.Collections.Generic;
using EFT;
using UnityEngine;

public interface IBossToFollow
{
	bool NeedProtection { get; }

	PatrollingData PatrollingData { get; }

	Vector3 PositionOrTargetCover { get; }

	Vector3 PositionIfInCover { get; }

	Vector3 Position { get; }

	float MoveSpeed { get; }

	bool IsAI { get; }

	List<BotOwner> Followers { get; }

	int FollowersTargetCount { get; }

	bool IsAlive { get; }

	void RemoveFollower(BotOwner owner);

	bool IsMe(IPlayer player);

	IPlayer Player();

	EnemyInfo CurEnemy();

	BotOwner GetFirstFollower(bool withGrenade);

	PatrolPoint GetPatrolPosByIndex(int botFollowerIndex);

	ABossLogic GetBossLogic();
}
