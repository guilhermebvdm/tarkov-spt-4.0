using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[BurstCompile]
public struct GStruct92 : IJobParallelFor
{
	[NonSerialized]
	public const int Int_0 = 2;

	[NonSerialized]
	public const float Float_0 = 0.99f;

	[NonSerialized]
	public const float Float_1 = 0.001f;

	[NonSerialized]
	public const float Float_2 = MathF.PI * 2f;

	[NonSerialized]
	public const int Int_1 = 0;

	[ReadOnly]
	public NativeArray<GStruct91> checkResult;

	[ReadOnly]
	public QueryParameters queryParams;

	[ReadOnly]
	public int edgeSearchRayCount;

	[ReadOnly]
	public float edgeSearchRayLength;

	[WriteOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<RaycastCommand> edgeSearchRayCommands;

	public void method_0(float3 forward, out float3 right, out float3 up)
	{
		forward = math.normalizesafe(forward);
		float3 x = ((math.abs(forward.y) < 0.99f) ? new float3(0f, 1f, 0f) : new float3(1f, 0f, 0f));
		right = math.normalizesafe(math.cross(x, forward));
		if (math.lengthsq(right) < 0.001f)
		{
			right = math.normalizesafe(math.cross(new float3(0f, 0f, 1f), forward));
		}
		if (math.lengthsq(right) < 0.001f)
		{
			right = new float3(1f, 0f, 0f);
		}
		up = math.normalizesafe(math.cross(forward, right));
		if (math.lengthsq(up) < 0.001f)
		{
			up = math.normalizesafe(math.cross(right, forward));
		}
	}

	public void Execute(int index)
	{
		GStruct91 gStruct = checkResult[0];
		float x = (float)index / (float)edgeSearchRayCount * (MathF.PI * 2f);
		for (int i = 0; i < 2; i++)
		{
			float3 float5 = ((i == 0) ? gStruct.sourceHitPoint : gStruct.listenerHitPoint);
			float3 forward = math.normalizesafe((i == 0) ? gStruct.sourceHitNormal : gStruct.listenerHitNormal);
			method_0(forward, out var right, out var up);
			float3 float6 = math.normalize(right * math.cos(x) + up * math.sin(x));
			int index2 = i * edgeSearchRayCount + index;
			edgeSearchRayCommands[index2] = new RaycastCommand(float5, float6, queryParams, edgeSearchRayLength);
		}
	}
}
