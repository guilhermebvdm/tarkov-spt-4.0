using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[BurstCompile]
public struct GStruct95 : IJob
{
	[NonSerialized]
	public const int Int_0 = 2;

	[NonSerialized]
	public const float Float_0 = 0.0001f;

	[NonSerialized]
	public const float Float_1 = 1.5f;

	[NonSerialized]
	public const float Float_2 = 1f;

	[NonSerialized]
	public const float Float_3 = 0.5f;

	[NonSerialized]
	public const int Int_1 = 0;

	[ReadOnly]
	public NativeArray<RaycastHit> validationRayHits;

	[ReadOnly]
	public NativeArray<RaycastCommand> validationRayCommands;

	[ReadOnly]
	public NativeList<float3> candidateEdgePoints;

	[ReadOnly]
	public NativeArray<int> candidateCount;

	[ReadOnly]
	public float3 sourcePos;

	[ReadOnly]
	public float3 listenerPos;

	[ReadOnly]
	public NativeArray<GStruct91> checkResult;

	[ReadOnly]
	public float maxDiffractionPathFactor;

	[WriteOnly]
	public NativeArray<GStruct90> result;

	public float method_0(float3 edgePoint, float3 source, float3 listener)
	{
		if (!(math.distancesq(edgePoint, source) < 0.0001f) && math.distancesq(listener, edgePoint) >= 0.0001f)
		{
			float3 x = math.normalize(edgePoint - source);
			float3 y = math.normalize(listener - edgePoint);
			return math.pow(math.saturate((math.dot(x, y) + 1f) * 0.5f), 1.5f);
		}
		return 0f;
	}

	public void Execute()
	{
		float num = float.MaxValue;
		float3 float5 = float3.zero;
		float energyFactor = 0f;
		bool flag = false;
		if (candidateCount.IsCreated && checkResult.IsCreated && validationRayHits.IsCreated && validationRayCommands.IsCreated && candidateEdgePoints.IsCreated && result.IsCreated)
		{
			GStruct91 gStruct = checkResult[0];
			if (!gStruct.needsEdgeSearch)
			{
				result[0] = GStruct90.DirectPath(gStruct.directDistance, listenerPos);
				return;
			}
			int num2 = candidateCount[0];
			float num3 = gStruct.directDistance * maxDiffractionPathFactor;
			int num4 = num2 * 2;
			if (num4 <= validationRayHits.Length && num4 <= validationRayCommands.Length)
			{
				for (int i = 0; i < num2; i++)
				{
					int num5 = i * 2;
					int index = num5 + 1;
					bool num6 = validationRayCommands[num5].distance > 0f;
					bool flag2 = validationRayCommands[index].distance > 0f;
					if (((num6 && validationRayHits[num5].colliderInstanceID == 0) & (flag2 && validationRayHits[index].colliderInstanceID == 0)) && i < candidateEdgePoints.Length)
					{
						float3 float6 = candidateEdgePoints[i];
						float num7 = math.distance(sourcePos, float6) + math.distance(float6, listenerPos);
						if (num7 <= num3 && num7 < num)
						{
							num = num7;
							float5 = float6;
							energyFactor = method_0(float5, sourcePos, listenerPos);
							flag = true;
						}
					}
				}
				if (flag)
				{
					result[0] = new GStruct90
					{
						pathLength = num,
						diffractionPoint = float5,
						energyFactor = energyFactor
					};
				}
				else
				{
					result[0] = GStruct90.NoPath;
				}
			}
			else
			{
				result[0] = GStruct90.NoPath;
			}
		}
		else if (result.IsCreated)
		{
			result[0] = GStruct90.NoPath;
		}
	}
}
