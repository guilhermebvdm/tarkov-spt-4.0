using System;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class CoverPointCreatorPreset
{
	[SerializeField]
	public string _name;

	[SerializeField]
	public DirectionCalculationType _directionCalculation;

	[SerializeField]
	public float _angleToRemovePointOnEdge = 6f;

	[SerializeField]
	public float _distanceBetweenCoversOnEdge = 3f;

	[SerializeField]
	public float _smallEdgeSize = 1f;

	[SerializeField]
	public float _clusterLargeDist = 0.8f;

	[SerializeField]
	public float _clusterAmbushDist = 1.8f;

	[SerializeField]
	public float _clusterLargeAngle = 50f;

	[SerializeField]
	public float _clusterNearDist = 0.4f;

	[SerializeField]
	public float _clusterFirePosDist = 0.75f;

	[SerializeField]
	public float _fireStepDist = 0.85f;

	[SerializeField]
	public bool _bothFireposIsBadCover;

	[SerializeField]
	public float _distanceRaycastMultiplier = 1f;

	[SerializeField]
	public float _distanceFireposMultiplier = 1.1f;

	[SerializeField]
	public float _ambushMinSegment = 1.5f;

	[SerializeField]
	public float _legWallCheck = 0.17f;

	[SerializeField]
	public float _legsCheckMinDist = 1.2f;

	[SerializeField]
	public bool _highQualityBorder;

	[FormerlySerializedAs("_checkLineCast")]
	[SerializeField]
	public bool _ignoreRayCast;

	[SerializeField]
	public bool _withCheckPointStay = true;

	[SerializeField]
	public float _checkOnNavMeshDist = 0.04f;

	[SerializeField]
	public float _sphereCastRadius = 0.1f;

	[SerializeField]
	public bool _removeCoversNearDoorAfterCalc = true;

	[SerializeField]
	public float _stepFromEndOfWall = 0.18f;

	public string Name => _name;

	public DirectionCalculationType DirectionCalculation => _directionCalculation;

	public float STEP_FROM_END_OF_WALL => _stepFromEndOfWall;

	public float ANGLE_TO_REMOVE_POINT_ON_EDGE => _angleToRemovePointOnEdge;

	public float DISTANCE_BETWEEN_COVERS_ON_EDGE => _distanceBetweenCoversOnEdge;

	public float SMALL_EDGE_SIZE => _smallEdgeSize;

	public float CLUSTER_LARGE_DIST => _clusterLargeDist;

	public float CLUSTER_AMBUSH_DIST => _clusterAmbushDist;

	public float CLUSTER_LARGE_ANGLE => _clusterLargeAngle;

	public float CLUSTER_NEAR_DIST => _clusterNearDist;

	public float CLUSTER_FIREPOS_DIST => _clusterFirePosDist;

	public float FIRE_STEP_DIST => _fireStepDist;

	public bool BOTH_FIREPOS_IS_BAD_COVER => _bothFireposIsBadCover;

	public float DISTANCE_RAYCAST_MULTIPLIER => _distanceRaycastMultiplier;

	public float DISTANCE_FIREPOS_MULTIPLIER => _distanceFireposMultiplier;

	public float AmbushMinSegment => _ambushMinSegment;

	public float LEG_WALL_CHECK => _legWallCheck;

	public float LegsCheckMinDist => _legsCheckMinDist;

	public bool HighQualityBorder => _highQualityBorder;

	public bool IgnoreLineCast => _ignoreRayCast;

	public bool WithCheckPointStay => _withCheckPointStay;

	public float CheckOnNavMeshDist => _checkOnNavMeshDist;

	public float SphereCastRadius => _sphereCastRadius;

	public bool RemoveCoversNearDoorAfterCalc => _removeCoversNearDoorAfterCalc;

	public static CoverPointCreatorPreset LoadDefault()
	{
		return ((CoverPointCreatorPresetCollection)Resources.Load("CoverPointCreatorPresetCollection")).Presets[0];
	}
}
