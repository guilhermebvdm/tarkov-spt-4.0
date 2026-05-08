using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[BurstCompile]
public struct Struct215 : IJob
{
	[ReadOnly]
	public NativeArray<RaycastHit> listenerToSourceHits;

	[ReadOnly]
	public NativeArray<RaycastHit> sourceToListenerHits;

	[ReadOnly]
	public NativeArray<float3> rayStartPoints;

	[ReadOnly]
	public float3 ListenerPosition;

	[ReadOnly]
	public float3 sourcePosition;

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

	[ReadOnly]
	public int totalRayCount;

	[WriteOnly]
	public NativeArray<GStruct106> aggregatedResultBuffer;

	public void Execute()
	{
		float num = 0f;
		float num2 = 0f;
		bool flag = false;
		for (int i = 0; i < totalRayCount; i++)
		{
			RaycastHit raycastHit = listenerToSourceHits[i];
			RaycastHit raycastHit2 = sourceToListenerHits[i];
			GStruct106 gStruct = GStruct106.NoPath;
			bool flag2 = raycastHit.colliderInstanceID != 0 && raycastHit.distance < maxDistance;
			bool flag3 = raycastHit2.colliderInstanceID != 0 && raycastHit2.distance < maxDistance;
			if (!flag2 && !flag3)
			{
				gStruct = GStruct106.ClearPath;
			}
			else if (flag2 != flag3)
			{
				gStruct = GStruct106.NoPath;
			}
			else if (raycastHit.colliderInstanceID != raycastHit2.colliderInstanceID)
			{
				gStruct = GStruct106.NoPath;
			}
			else
			{
				float num3 = math.distance(raycastHit.point, raycastHit2.point);
				if (num3 >= maxThicknessThreshold)
				{
					gStruct = GStruct106.NoPath;
				}
				else
				{
					float remainingEnergy = 1f;
					if (num3 >= minThicknessThreshold)
					{
						remainingEnergy = math.clamp(math.exp((0f - absorptionPerUnit) * num3), 0f, 1f);
					}
					gStruct.RemainingEnergy = remainingEnergy;
					gStruct.TotalThickness = num3;
					gStruct.PathPotentiallyValid = true;
				}
			}
			flag |= gStruct.PathPotentiallyValid;
			float num4 = (gStruct.PathPotentiallyValid ? gStruct.RemainingEnergy : 0f);
			num += num4;
			num2 += num4 * gStruct.TotalThickness;
		}
		float remainingEnergy2 = math.clamp(num / (float)totalRayCount, 0f, 1f);
		float totalThickness = ((num > 0f) ? (num2 / num) : float.MaxValue);
		aggregatedResultBuffer[0] = new GStruct106
		{
			RemainingEnergy = remainingEnergy2,
			TotalThickness = totalThickness,
			PathPotentiallyValid = flag
		};
	}
}
