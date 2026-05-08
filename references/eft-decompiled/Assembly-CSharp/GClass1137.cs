using System;
using System.Runtime.CompilerServices;
using Audio.SpatialSystem;
using Audio.SpatialSystem.Data;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class GClass1137 : IDisposable
{
	[NonSerialized]
	public const byte Byte_0 = 2;

	[NonSerialized]
	public const short Short_0 = 350;

	[NonSerialized]
	public PropagationSettings PropagationSettings_0;

	[NonSerialized]
	public NativeArray<GStruct88> NativeArray_0;

	[NonSerialized]
	public NativeArray<AudioRouteData> NativeArray_1;

	[NonSerialized]
	public NativeArray<int> NativeArray_2;

	[NonSerialized]
	public NativeList<RoomPair.AudioPortalData> NativeList_0;

	[NonSerialized]
	public NativeArray<float> NativeArray_3;

	[NonSerialized]
	public NativeArray<float> NativeArray_4;

	[NonSerialized]
	public NativeArray<float> NativeArray_5;

	[NonSerialized]
	public NativeArray<float> NativeArray_6;

	[NonSerialized]
	public float Float_0;

	[NonSerialized]
	[CompilerGenerated]
	public JobHandle JobHandle_0;

	public float PropagationScore => Mathf.Clamp01(Float_0);

	public bool DataDisposed => !NativeArray_4.IsCreated;

	public JobHandle LastHandle
	{
		[CompilerGenerated]
		get
		{
			return JobHandle_0;
		}
		[CompilerGenerated]
		set
		{
			JobHandle_0 = value;
		}
	}

	public GClass1137(SpatialAudioLocationInfo locationInfo, PropagationSettings settings)
	{
		int maxRoutesCount = (int)locationInfo.maxRoutesCount;
		int length = (int)locationInfo.maxRoutePortalsCount + 2 * maxRoutesCount;
		PropagationSettings_0 = settings;
		NativeArray_0 = new NativeArray<GStruct88>(length, Allocator.Persistent);
		NativeArray_3 = new NativeArray<float>(maxRoutesCount, Allocator.Persistent);
		NativeArray_5 = new NativeArray<float>(maxRoutesCount, Allocator.Persistent);
		NativeArray_4 = new NativeArray<float>(1, Allocator.Persistent);
		NativeArray_4[0] = float.MaxValue;
		NativeArray_6 = new NativeArray<float>(1, Allocator.Persistent);
		NativeArray_6[0] = float.MaxValue;
	}

	public void PrepareData(RoomPair roomPair, bool isGlobalDataNeedUpdate)
	{
		if (!GClass3670.TryGetData<GClass1138>(out var dataContainer))
		{
			return;
		}
		if (isGlobalDataNeedUpdate)
		{
			if (!dataContainer.TryGetRoutesData(roomPair.ID, out var data) || !dataContainer.TryGetPortalIndicesData(roomPair.ID, out var data2))
			{
				return;
			}
			NativeArray_1 = data;
			NativeArray_2 = data2;
		}
		NativeList_0 = dataContainer.StaticPortalDataList;
	}

	public JobHandle Schedule(in Vector3 sourcePosition, in Vector3 listenerPosition, bool isReversed, float qualityFactor = 1f)
	{
		int length = NativeArray_1.Length;
		int length2 = NativeArray_2.Length + 2 * length;
		NativeArray<float> subArray = NativeArray_3.GetSubArray(0, length);
		NativeArray<float> subArray2 = NativeArray_5.GetSubArray(0, length);
		NativeArray<GStruct88> subArray3 = NativeArray_0.GetSubArray(0, length2);
		JobHandle dependsOn = new Struct206
		{
			LowerCosts = subArray,
			LowerDistances = subArray2,
			BestCost = NativeArray_4,
			BestDistance = NativeArray_6
		}.Schedule(LastHandle);
		Struct204 jobData = new Struct204
		{
			minPortalCostPercent = PropagationSettings_0.minPortalCostPercent,
			diffractionExponent = PropagationSettings_0.diffractionExponent,
			distanceWeight = PropagationSettings_0.distanceWeight,
			heightExponent = PropagationSettings_0.heightExponent,
			segmentHeightWeightUp = PropagationSettings_0.segmentHeightWeightUp,
			segmentHeightWeightDown = PropagationSettings_0.segmentHeightWeightDown,
			absoluteHeightWeight = PropagationSettings_0.absoluteHeightWeight,
			typicalRoomHeight = PropagationSettings_0.typicalRoomHeight,
			maxSegmentLength = PropagationSettings_0.maxSegmentLength,
			RoutesData = NativeArray_1,
			RoutePortalIndices = NativeArray_2,
			StaticPortalData = NativeList_0,
			LowestCosts = subArray,
			LowestDistances = subArray2,
			SourcePosition = (isReversed ? listenerPosition : sourcePosition),
			ListenerPosition = (isReversed ? sourcePosition : listenerPosition),
			RouteNodes = subArray3
		};
		Struct205 jobData2 = new Struct205
		{
			lowerCosts = subArray,
			lowerDistances = subArray2,
			bestCost = NativeArray_4,
			bestDistance = NativeArray_6,
			lowerCostsLength = length
		};
		int length3 = NativeArray_1.Length;
		int num = Mathf.Max(Mathf.Min(350, length3), Mathf.CeilToInt((float)length3 * qualityFactor));
		int innerloopBatchCount = Mathf.Max(1, num / 128);
		JobHandle dependsOn2 = IJobParallelForExtensions.Schedule(jobData, num, innerloopBatchCount, dependsOn);
		LastHandle = jobData2.Schedule(dependsOn2);
		return LastHandle;
	}

	public void CompleteAndDisposeData()
	{
		Complete();
		DisposeData();
	}

	public void Complete()
	{
		LastHandle.Complete();
		if (NativeArray_4.IsCreated)
		{
			Float_0 = NativeArray_4[0];
		}
	}

	public void DisposeData()
	{
		if (NativeArray_0.IsCreated)
		{
			NativeArray_0.Dispose();
		}
		if (NativeArray_3.IsCreated)
		{
			NativeArray_3.Dispose();
		}
		if (NativeArray_5.IsCreated)
		{
			NativeArray_5.Dispose();
		}
		if (NativeArray_4.IsCreated)
		{
			NativeArray_4.Dispose();
		}
		if (NativeArray_6.IsCreated)
		{
			NativeArray_6.Dispose();
		}
	}

	public void Dispose()
	{
		DisposeData();
	}
}
