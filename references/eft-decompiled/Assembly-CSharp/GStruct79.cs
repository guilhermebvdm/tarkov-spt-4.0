using System;
using MultiFlare;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

[BurstCompile]
public struct GStruct79(GClass1030 lightStorage, NativeList<GStruct74> flareCollection, BatchSettings settings, NativeList<GStruct75> output) : IJobParallelFor
{
	[NonSerialized]
	public float Float_0 = settings.SizeMultiplier;

	[NonSerialized]
	public float Float_1 = settings.AlphaMultiplier;

	[NonSerialized]
	[ReadOnly]
	public NativeArray<GStruct76>.ReadOnly ReadOnly_0 = lightStorage.GetJobData();

	[NonSerialized]
	[ReadOnly]
	public NativeArray<GStruct74> NativeArray_0 = flareCollection.AsArray();

	[NonSerialized]
	[WriteOnly]
	public NativeList<GStruct75>.ParallelWriter ParallelWriter_0 = output.AsParallelWriter();

	public void Execute(int index)
	{
		GStruct74 gStruct = NativeArray_0[index];
		GStruct76 gStruct2 = ReadOnly_0[gStruct.LightIndex];
		if (gStruct2.Enabled)
		{
			GStruct75 value = default(GStruct75);
			float num = gStruct2.Alpha * gStruct.MinAlpha * gStruct.Alpha * Float_1;
			value.UvRect = new float4(gStruct.Uv.x, gStruct.Uv.y, gStruct.Uv.width, gStruct.Uv.height);
			value.Scale = gStruct.Scale * (gStruct.MinScale * gStruct2.Scale) * Float_0;
			value.Color.x = math.saturate(gStruct.Color.r * num);
			value.Color.y = math.saturate(gStruct.Color.g * num);
			value.Color.z = math.saturate(gStruct.Color.b * num);
			value.Color.w = gStruct2.Alpha;
			value.MinDist = gStruct.MinDist;
			value.DistInverse = 1f / (gStruct.MaxDist - gStruct.MinDist);
			value.ScaleRange = (gStruct.MaxScale - gStruct.MinScale) / gStruct.MinScale;
			value.AlphaRange = (gStruct.MaxAlpha - gStruct.MinAlpha) / gStruct.MinAlpha;
			value.WorldPos = gStruct2.WorldPos;
			ParallelWriter_0.AddNoResize(value);
		}
	}
}
