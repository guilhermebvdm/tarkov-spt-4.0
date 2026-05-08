using System;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class Class822
{
	[NonSerialized]
	public NativeArray<Vector3> NativeArray_0;

	[NonSerialized]
	public NativeArray<Vector3> NativeArray_1;

	[NonSerialized]
	public NativeArray<Vector3> NativeArray_2;

	[NonSerialized]
	public NativeArray<Vector3> NativeArray_3;

	[NonSerialized]
	public NativeArray<double> NativeArray_4;

	[NonSerialized]
	public NativeArray<Vector3> NativeArray_5;

	[NonSerialized]
	public NativeArray<double> NativeArray_6;

	[NonSerialized]
	public NativeArray<bool> NativeArray_7;

	[NonSerialized]
	public NativeArray<Struct225> NativeArray_8;

	[NonSerialized]
	public NativeArray<int> NativeArray_9;

	[NonSerialized]
	public int Int_0;

	[NonSerialized]
	public float Float_0;

	[NonSerialized]
	public int Int_1;

	[NonSerialized]
	[CompilerGenerated]
	public bool Bool_0;

	public bool Disposed
	{
		[CompilerGenerated]
		get
		{
			return Bool_0;
		}
		[CompilerGenerated]
		set
		{
			Bool_0 = value;
		}
	}

	public Class822(Vector3[] points, Vector3[] lineStarts, Vector3[] lineEnds, NativeArray<Struct225> result, int resolution, float calculationRadius)
	{
		NativeArray_0 = new NativeArray<Vector3>(points, Allocator.Persistent);
		NativeArray_1 = new NativeArray<Vector3>(lineStarts, Allocator.Persistent);
		NativeArray_2 = new NativeArray<Vector3>(lineEnds, Allocator.Persistent);
		NativeArray_8 = result;
		Int_0 = resolution;
		Float_0 = calculationRadius;
		NativeArray_3 = new NativeArray<Vector3>(Int_0, Allocator.Persistent);
		NativeArray_4 = new NativeArray<double>(Int_0, Allocator.Persistent);
		NativeArray_5 = new NativeArray<Vector3>(1, Allocator.Persistent);
		NativeArray_6 = new NativeArray<double>(1, Allocator.Persistent);
		NativeArray_7 = new NativeArray<bool>(1, Allocator.Persistent);
		NativeArray_9 = new NativeArray<int>(1, Allocator.Persistent);
		Int_1 = Mathf.Min(resolution, 128);
		Disposed = false;
	}

	public JobHandle Schedule(JobHandle dependency, Vector3 listenerPos)
	{
		dependency = IJobParallelForExtensions.Schedule(new Struct227
		{
			listenerPos = listenerPos,
			lineStarts = NativeArray_1,
			lineEnds = NativeArray_2,
			calculatedClosestPoints = NativeArray_3,
			calculatedSqrDistances = NativeArray_4
		}, Int_0, Int_1, dependency);
		dependency = IJobParallelForExtensions.Schedule(new Struct228
		{
			lineStarts = NativeArray_1,
			lineEnds = NativeArray_2,
			listenerPos = listenerPos,
			calculationRadius = Float_0,
			calculatedSqrDistances = NativeArray_4,
			totalDirection = NativeArray_5,
			totalWeight = NativeArray_6,
			withinRange = NativeArray_7
		}, Int_0, Int_1, dependency);
		dependency = IJobParallelForExtensions.Schedule(new Struct226
		{
			targetPoint = listenerPos,
			points = NativeArray_0,
			crossingNumber = NativeArray_9
		}, Int_0, Int_1, dependency);
		dependency = new Struct229
		{
			calculatedClosestPoints = NativeArray_3,
			calculatedSqrDistances = NativeArray_4,
			totalDirection = NativeArray_5,
			totalWeight = NativeArray_6,
			calculationRadius = Float_0,
			result = NativeArray_8,
			withinRange = NativeArray_7,
			crossingNumber = NativeArray_9
		}.Schedule(dependency);
		dependency = IJobParallelForExtensions.Schedule(new Struct230
		{
			calculatedClosestPoints = NativeArray_3,
			calculatedSqrDistances = NativeArray_4,
			totalDirection = NativeArray_5,
			totalWeight = NativeArray_6,
			withinRange = NativeArray_7,
			crossingNumber = NativeArray_9
		}, Int_0, Int_1, dependency);
		return dependency;
	}

	public void Dispose()
	{
		if (!Disposed && NativeArray_0.IsCreated)
		{
			NativeArray_0.Dispose();
			NativeArray_1.Dispose();
			NativeArray_2.Dispose();
			NativeArray_3.Dispose();
			NativeArray_4.Dispose();
			NativeArray_5.Dispose();
			NativeArray_6.Dispose();
			NativeArray_7.Dispose();
			NativeArray_9.Dispose();
			Disposed = true;
		}
	}
}
