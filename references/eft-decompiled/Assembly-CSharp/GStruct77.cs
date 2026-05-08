using System;
using Unity.Burst;
using Unity.Collections;
using UnityEngine.Jobs;

[BurstCompile]
public struct GStruct77(NativeArray<GStruct76> lights) : IJobParallelForTransform
{
	[NonSerialized]
	public NativeArray<GStruct76> NativeArray_0 = lights;

	public void Execute(int index, TransformAccess transform)
	{
		GStruct76 value = NativeArray_0[index];
		value.WorldPos = transform.position;
		NativeArray_0[index] = value;
	}
}
