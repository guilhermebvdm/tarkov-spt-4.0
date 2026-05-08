using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[BurstCompile]
public struct Struct216 : IJobParallelFor
{
	[NonSerialized]
	public const float Float_0 = 0f;

	[NonSerialized]
	public const float Float_1 = 0f;

	[NonSerialized]
	public const float Float_2 = 1f;

	[NonSerialized]
	public const int Int_0 = 0;

	[ReadOnly]
	public NativeArray<RaycastHit> listenerToSourceHits;

	[ReadOnly]
	public NativeArray<RaycastHit> sourceToListenerHits;

	[ReadOnly]
	public float3 ListenerPosition;

	[ReadOnly]
	public float3 sourcePosition;

	[ReadOnly]
	public NativeArray<float3> rayStartPoints;

	[ReadOnly]
	public float absorptionPerUnit;

	[ReadOnly]
	public float minEnergyThreshold;

	[ReadOnly]
	public float minThicknessThreshold;

	[ReadOnly]
	public float maxThicknessThreshold;

	[ReadOnly]
	public float maxDistance;

	[WriteOnly]
	public NativeArray<GStruct106> pathResults;

	public float method_0(float thickness)
	{
		thickness = math.max(0f, thickness);
		return math.clamp(math.exp((0f - absorptionPerUnit) * thickness), 0f, 1f);
	}

	public void Execute(int index)
	{
		RaycastHit raycastHit = listenerToSourceHits[index];
		RaycastHit raycastHit2 = sourceToListenerHits[index];
		_ = rayStartPoints[index];
		GStruct106 value = GStruct106.NoPath;
		bool flag = raycastHit.colliderInstanceID != 0 && raycastHit.distance < maxDistance;
		bool flag2 = raycastHit2.colliderInstanceID != 0 && raycastHit2.distance < maxDistance;
		if (!flag && !flag2)
		{
			value = GStruct106.ClearPath;
		}
		else if (flag != flag2)
		{
			value = GStruct106.NoPath;
		}
		else if (raycastHit.colliderInstanceID != raycastHit2.colliderInstanceID)
		{
			value = GStruct106.NoPath;
		}
		else
		{
			float num = math.distance(raycastHit.point, raycastHit2.point);
			if (num >= maxThicknessThreshold)
			{
				value = GStruct106.NoPath;
			}
			else
			{
				float remainingEnergy = 1f;
				if (num >= minThicknessThreshold)
				{
					remainingEnergy = method_0(num);
				}
				value.RemainingEnergy = remainingEnergy;
				value.TotalThickness = num;
				value.PathPotentiallyValid = true;
			}
		}
		pathResults[index] = value;
	}
}
