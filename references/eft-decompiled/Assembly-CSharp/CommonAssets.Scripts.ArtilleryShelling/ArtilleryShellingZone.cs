using System;
using System.Collections.Generic;
using UnityEngine;

namespace CommonAssets.Scripts.ArtilleryShelling;

[Serializable]
public struct ArtilleryShellingZone
{
	public string ID;

	public Vector2 PointsInShellings;

	public int ShellingRounds;

	public int ShotCount;

	public Vector2 PauseBetweenRounds;

	public Vector2 PauseBetweenShots;

	public Vector3 Center;

	public Vector2 ExplosionDistanceRange;

	public float Rotate;

	public Vector2 GridStep;

	public Vector2 Points;

	public Vector3[] PointsWorldPositions;

	public float PointRadius;

	public GStruct150[] AlarmStages;

	public int LootTemplateId;

	public bool UsedInPlanedShelling;

	public bool UseInCalledShelling;

	public bool IsActive;

	public List<Vector2> ZoneBounds;
}
