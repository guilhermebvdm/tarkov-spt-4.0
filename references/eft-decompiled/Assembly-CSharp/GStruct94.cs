using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[BurstCompile]
public struct GStruct94 : IJob
{
	[NonSerialized]
	public const int Int_0 = 0;

	[NonSerialized]
	public const int Int_1 = 1;

	[NonSerialized]
	public const int Int_2 = 0;

	[ReadOnly]
	public NativeArray<RaycastHit> hits;

	[ReadOnly]
	public float3 sourceToListenerDirection;

	[ReadOnly]
	public float3 listenerToSourceDirection;

	[ReadOnly]
	public float directDistance;

	[WriteOnly]
	public NativeArray<GStruct91> checkResult;

	public void Execute()
	{
		RaycastHit raycastHit = hits[0];
		RaycastHit raycastHit2 = hits[1];
		if ((raycastHit.colliderInstanceID == 0) | (raycastHit2.colliderInstanceID == 0))
		{
			checkResult[0] = new GStruct91
			{
				needsEdgeSearch = false
			};
			return;
		}
		float3 sourceHitPoint = raycastHit.point;
		float3 listenerHitPoint = raycastHit2.point;
		float3 sourceHitNormal = raycastHit.normal;
		float3 listenerHitNormal = raycastHit2.normal;
		int colliderInstanceID = raycastHit.colliderInstanceID;
		int colliderInstanceID2 = raycastHit2.colliderInstanceID;
		checkResult[0] = new GStruct91
		{
			needsEdgeSearch = true,
			sourceHitPoint = sourceHitPoint,
			listenerHitPoint = listenerHitPoint,
			sourceHitNormal = sourceHitNormal,
			listenerHitNormal = listenerHitNormal,
			sourceHitColliderId = colliderInstanceID,
			listenerHitColliderId = colliderInstanceID2,
			sourceToHitDist = raycastHit.distance,
			listenerToHitDist = raycastHit2.distance,
			dirSl = sourceToListenerDirection,
			dirLs = listenerToSourceDirection,
			directDistance = directDistance
		};
	}
}
