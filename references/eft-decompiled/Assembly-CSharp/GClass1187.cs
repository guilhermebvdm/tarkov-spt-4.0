using System;
using Audio.SpatialSystem.Data;
using Unity.Collections;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using UnityEngine;

public class GClass1187 : IDisposable
{
	[NonSerialized]
	public JobHandle JobHandle_0;

	[NonSerialized]
	public NativeArray<PortalData> NativeArray_0;

	[NonSerialized]
	public NativeArray<float> NativeArray_1;

	[NonSerialized]
	public NativeArray<GStruct108> NativeArray_2;

	[NonSerialized]
	public NativeArray<GStruct108> NativeArray_3;

	[NonSerialized]
	public NativeArray<short> NativeArray_4;

	[NonSerialized]
	public bool Bool_0 = true;

	public bool IsCompleted => JobHandle_0.IsCompleted;

	public GStruct108 OptimalCalculatedData
	{
		get
		{
			if (!NativeArray_3.IsCreated)
			{
				return GStruct108.Default();
			}
			return NativeArray_3[0];
		}
	}

	public void Complete()
	{
		JobHandle_0.Complete();
	}

	public void PrepareData(PortalData[] currentOutPortalsData, short[] currentIndoorRoomsIDs)
	{
		NativeArray_0 = new NativeArray<PortalData>(currentOutPortalsData, Allocator.Persistent);
		NativeArray_1 = new NativeArray<float>(currentOutPortalsData.Length, Allocator.Persistent);
		NativeArray_2 = new NativeArray<GStruct108>(currentOutPortalsData.Length, Allocator.Persistent);
		NativeArray_3 = new NativeArray<GStruct108>(1, Allocator.Persistent);
		NativeArray_4 = new NativeArray<short>(currentIndoorRoomsIDs, Allocator.Persistent);
		Bool_0 = false;
	}

	public void Schedule(Vector3 listenerPosition, int currentInRoomID, float listenerHeightWeight, JobHandle jobHandle = default(JobHandle))
	{
		Struct217 jobData = new Struct217
		{
			PortalsData = NativeArray_0,
			DistancesToListener = NativeArray_1,
			ListenerPosition = listenerPosition
		};
		Struct218 jobData2 = new Struct218
		{
			currentIndoorRoomID = currentInRoomID,
			indoorRoomsIDs = NativeArray_4,
			listenerHeightWeight = listenerHeightWeight,
			listenerPosition = listenerPosition,
			portalsData = NativeArray_0,
			distancesToListener = NativeArray_1,
			calculatedPortalData = NativeArray_2
		};
		Struct219 jobData3 = new Struct219
		{
			calculatedPortalData = NativeArray_2,
			optimalCalculatedPortalData = NativeArray_3
		};
		int length = NativeArray_0.Length;
		int num = Mathf.Max(1, JobsUtility.JobWorkerCount);
		int innerloopBatchCount = Mathf.CeilToInt((float)length / (float)num);
		JobHandle_0 = IJobParallelForExtensions.Schedule(jobData, length, innerloopBatchCount, jobHandle);
		JobHandle_0 = IJobParallelForExtensions.Schedule(jobData2, length, innerloopBatchCount, JobHandle_0);
		JobHandle_0 = jobData3.Schedule(JobHandle_0);
	}

	public void Dispose()
	{
		if (!Bool_0)
		{
			JobHandle_0.Complete();
			NativeArray_0.Dispose();
			NativeArray_1.Dispose();
			NativeArray_3.Dispose();
			NativeArray_2.Dispose();
			NativeArray_4.Dispose();
			Bool_0 = true;
		}
	}
}
