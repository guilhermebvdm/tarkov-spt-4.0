using UnityEngine;

public struct GStruct16
{
	public Vector3 RaycastTargetPosition;

	public bool Visible;

	public float VisibilityLevel;

	public float VisibilityDuration;

	public EEnemyPartVisibleType VisibleType;

	public bool HasLineOfSight;

	public ELookObstacleType BotToTargetObstacleType;

	public Vector3 BotToTargetHitPosition;

	public float ObstaclePenetrationDepth;
}
