using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[BurstCompile]
public struct GStruct96 : IJobParallelFor
{
	[NonSerialized]
	public const int Int_0 = 2;

	[NonSerialized]
	public const int Int_1 = 1;

	[NonSerialized]
	public const int Int_2 = 0;

	[NonSerialized]
	public const float Float_0 = 0f;

	[ReadOnly]
	public NativeList<float3> candidateEdgePoints;

	[ReadOnly]
	public float3 sourcePos;

	[ReadOnly]
	public float3 listenerPos;

	[ReadOnly]
	public QueryParameters queryParams;

	[ReadOnly]
	public float edgeValidationRayOffset;

	[ReadOnly]
	public NativeArray<int> candidateCount;

	[WriteOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<RaycastCommand> validationRayCommands;

	public void Execute(int index)
	{
		int num = index * 2;
		if (index >= candidateCount[0])
		{
			validationRayCommands[num] = default(RaycastCommand);
			validationRayCommands[num + 1] = default(RaycastCommand);
			return;
		}
		float3 obj = candidateEdgePoints[index];
		float3 float5 = obj - sourcePos;
		float num2 = math.length(float5);
		if (num2 > edgeValidationRayOffset)
		{
			float3 float6 = float5 / num2;
			float3 float7 = sourcePos;
			float num3 = num2 - edgeValidationRayOffset;
			validationRayCommands[num] = ((num3 > 0f) ? new RaycastCommand(float7, float6, queryParams, num3) : default(RaycastCommand));
		}
		else
		{
			validationRayCommands[num] = default(RaycastCommand);
		}
		float3 float8 = obj - listenerPos;
		float num4 = math.length(float8);
		if (num4 > edgeValidationRayOffset)
		{
			float3 float9 = float8 / num4;
			float3 float10 = listenerPos;
			float num5 = num4 - edgeValidationRayOffset;
			validationRayCommands[num + 1] = ((num5 > 0f) ? new RaycastCommand(float10, float9, queryParams, num5) : default(RaycastCommand));
		}
		else
		{
			validationRayCommands[num + 1] = default(RaycastCommand);
		}
	}
}
