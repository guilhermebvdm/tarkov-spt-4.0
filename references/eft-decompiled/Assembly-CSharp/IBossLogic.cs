using System;
using System.Collections.Generic;
using EFT;
using UnityEngine;

public interface IBossLogic
{
	Vector3? PointForBoss { get; }

	event Action OnPositionsRecalculated;

	Dictionary<BotOwner, Vector3> FollowersPositions();

	void SetFollowersPositions();

	Vector3 GetTargetToLook();
}
