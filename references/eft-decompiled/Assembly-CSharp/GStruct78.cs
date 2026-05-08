using System;
using MultiFlare;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

[BurstCompile]
public struct GStruct78(NativeArray<GStruct75> data, FlareOverlapSettings overlapSettings) : IJob
{
	[NonSerialized]
	public int Int_0 = overlapSettings.MaxNeighborCount;

	[NonSerialized]
	public float Float_0 = overlapSettings.SearchRange;

	[NonSerialized]
	public float Float_1 = overlapSettings.MaxScaleMultiplier;

	[NonSerialized]
	public NativeArray<GStruct75> NativeArray_0 = data;

	public void Execute()
	{
		int length = NativeArray_0.Length;
		for (int i = 0; i < length; i++)
		{
			GStruct75 value = NativeArray_0[i];
			float num = method_0(i);
			float num2 = math.lerp(1f, Float_1, num / (float)Int_0);
			value.Scale *= num2;
			NativeArray_0[i] = value;
		}
	}

	public float method_0(int index)
	{
		float num = 0f;
		float3 worldPos = NativeArray_0[index].WorldPos;
		for (int i = 0; i < NativeArray_0.Length; i++)
		{
			if (index != i)
			{
				float num2 = math.distance(NativeArray_0[i].WorldPos, worldPos);
				if (!(num2 > Float_0))
				{
					num += 1f - num2 / Float_0;
				}
			}
		}
		return math.min(num, Int_0);
	}
}
