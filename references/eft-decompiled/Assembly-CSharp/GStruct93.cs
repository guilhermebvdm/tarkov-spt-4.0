using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[BurstCompile]
public struct GStruct93 : IJob
{
	[NonSerialized]
	public const int Int_0 = 2;

	[NonSerialized]
	public const float Float_0 = 0.001f;

	[NonSerialized]
	public const int Int_1 = 0;

	[ReadOnly]
	public NativeArray<RaycastHit> edgeSearchRayHits;

	[ReadOnly]
	public NativeArray<RaycastCommand> edgeSearchRayCommands;

	[ReadOnly]
	public NativeArray<GStruct91> checkResult;

	[ReadOnly]
	public int edgeSearchRayCount;

	public NativeList<float3> candidateEdgePoints;

	public NativeArray<int> candidateCount;

	public void Execute()
	{
		candidateEdgePoints.Clear();
		GStruct91 gStruct = checkResult[0];
		if (!gStruct.needsEdgeSearch)
		{
			candidateCount[0] = 0;
			return;
		}
		for (int i = 0; i < 2; i++)
		{
			int num = i * edgeSearchRayCount;
			int num2 = ((i == 0) ? gStruct.sourceHitColliderId : gStruct.listenerHitColliderId);
			for (int j = 0; j < edgeSearchRayCount; j++)
			{
				int index = num + j;
				RaycastHit raycastHit = edgeSearchRayHits[index];
				RaycastCommand raycastCommand = edgeSearchRayCommands[index];
				if (raycastHit.colliderInstanceID != 0 && raycastHit.colliderInstanceID == num2)
				{
					continue;
				}
				float3 value = ((raycastHit.colliderInstanceID == 0) ? (raycastCommand.from + raycastCommand.direction * raycastCommand.distance) : raycastHit.point);
				bool flag = false;
				for (int k = 0; k < candidateEdgePoints.Length; k++)
				{
					if (!(math.distancesq(value, candidateEdgePoints[k]) >= 0.001f))
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					candidateEdgePoints.Add(in value);
				}
			}
		}
		candidateCount[0] = candidateEdgePoints.Length;
	}
}
