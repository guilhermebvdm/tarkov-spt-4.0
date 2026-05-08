using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

[BurstCompile]
public struct Struct219 : IJob
{
	[NonSerialized]
	public const float Float_0 = 0.05f;

	[ReadOnly]
	public NativeArray<GStruct108> calculatedPortalData;

	[WriteOnly]
	public NativeArray<GStruct108> optimalCalculatedPortalData;

	public void Execute()
	{
		float num = float.MaxValue;
		float num2 = float.MaxValue;
		GStruct108 value = GStruct108.Default();
		optimalCalculatedPortalData[0] = GStruct108.Default();
		for (int i = 0; i < calculatedPortalData.Length; i++)
		{
			if (calculatedPortalData[i].portalData.id != 0)
			{
				GStruct108 gStruct = calculatedPortalData[i];
				float score = gStruct.score;
				float cost = gStruct.portalData.cost;
				if (score < num || (smethod_0(score, num) && cost < num2))
				{
					num = score;
					value = gStruct;
					num2 = cost;
				}
			}
		}
		optimalCalculatedPortalData[0] = value;
	}

	public static bool smethod_0(float scoreToCompare, float optimalScore)
	{
		return Mathf.Abs(scoreToCompare - optimalScore) < 0.05f;
	}
}
