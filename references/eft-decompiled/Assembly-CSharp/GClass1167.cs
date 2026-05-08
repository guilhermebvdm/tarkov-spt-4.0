using System;
using Audio.SpatialSystem.Data;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class GClass1167 : GInterface89, IDisposable
{
	[NonSerialized]
	public const float Float_0 = 0.05f;

	[NonSerialized]
	public const float Float_1 = 1f;

	[NonSerialized]
	public const int Int_0 = 1;

	[NonSerialized]
	public const float Float_2 = 0.01f;

	[NonSerialized]
	public const float Float_3 = 0.001f;

	[NonSerialized]
	public const float Float_4 = 2f;

	[NonSerialized]
	public const float Float_5 = 0.0001f;

	[NonSerialized]
	public const float Float_6 = 0.01f;

	[NonSerialized]
	public const float Float_7 = 0f;

	[NonSerialized]
	public const float Float_8 = 1f;

	[NonSerialized]
	public const int Int_1 = 2;

	[NonSerialized]
	public const int Int_2 = 2;

	[NonSerialized]
	public const int Int_3 = 4;

	[NonSerialized]
	public const int Int_4 = 8;

	[NonSerialized]
	public float Float_9;

	[NonSerialized]
	public float Float_10;

	[NonSerialized]
	public float Float_11;

	[NonSerialized]
	public int Int_5;

	[NonSerialized]
	public float Float_12;

	[NonSerialized]
	public float Float_13;

	[NonSerialized]
	public JobHandle JobHandle_0;

	[NonSerialized]
	public bool Bool_0;

	[NonSerialized]
	public QueryParameters QueryParameters_0;

	[NonSerialized]
	public float Float_14;

	[NonSerialized]
	public float Float_15;

	[NonSerialized]
	public NativeArray<SpherecastCommand> NativeArray_0;

	[NonSerialized]
	public NativeArray<RaycastHit> NativeArray_1;

	[NonSerialized]
	public NativeArray<RaycastCommand> NativeArray_2;

	[NonSerialized]
	public NativeArray<RaycastHit> NativeArray_3;

	[NonSerialized]
	public NativeList<float3> NativeList_0;

	[NonSerialized]
	public NativeArray<RaycastCommand> NativeArray_4;

	[NonSerialized]
	public NativeArray<RaycastHit> NativeArray_5;

	[NonSerialized]
	public NativeArray<int> NativeArray_6;

	[NonSerialized]
	public NativeArray<GStruct91> NativeArray_7;

	[NonSerialized]
	public NativeArray<GStruct90> NativeArray_8;

	[NonSerialized]
	public bool Bool_1;

	[NonSerialized]
	public int Int_6;

	public bool IsCalculationInProgress => Bool_0;

	public JobHandle LastJobHandle => JobHandle_0;

	public GClass1167(DiffractionSettings settings, QueryParameters queryParameters, float overallMaxDistance)
	{
		Float_15 = overallMaxDistance;
		Float_11 = math.max(1f, settings.maxPathFactor);
		Int_5 = math.max(1, settings.GetRaysCountByQuality());
		Float_12 = math.max(0.01f, settings.edgeSearchRayLength);
		Float_13 = math.max(0.001f, settings.edgeValidationRayOffset);
		QueryParameters_0 = queryParameters;
		NativeArray_0 = new NativeArray<SpherecastCommand>(2, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		NativeArray_1 = new NativeArray<RaycastHit>(2, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		int num = Int_5 * 2;
		NativeArray_2 = new NativeArray<RaycastCommand>(num, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		NativeArray_3 = new NativeArray<RaycastHit>(num, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		NativeList_0 = new NativeList<float3>(num, Allocator.Persistent);
		int length = num * 2;
		NativeArray_4 = new NativeArray<RaycastCommand>(length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		NativeArray_5 = new NativeArray<RaycastHit>(length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		NativeArray_6 = new NativeArray<int>(1, Allocator.Persistent);
		NativeArray_7 = new NativeArray<GStruct91>(1, Allocator.Persistent);
		NativeArray_8 = new NativeArray<GStruct90>(1, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		Bool_1 = true;
	}

	public JobHandle StartCalculation(Vector3 listenerPosition, Vector3 sourcePosition, float maxDistance, QueryParameters queryParameters, JobHandle inputDeps)
	{
		if (!Bool_1)
		{
			Bool_0 = false;
			return inputDeps;
		}
		inputDeps = JobHandle.CombineDependencies(inputDeps, JobHandle_0);
		Float_14 = Mathf.Min(maxDistance, Float_15);
		float3 float5 = sourcePosition;
		float3 float6 = listenerPosition;
		float num = math.distance(float5, float6);
		if (!(num > maxDistance) && num >= 0.1f)
		{
			float3 zero = float3.zero;
			float3 zero2 = float3.zero;
			float3 x = float6 - float5;
			if (math.lengthsq(x) > 0.0001f)
			{
				zero = math.normalize(x);
				zero2 = -zero;
				NativeArray_0[0] = new SpherecastCommand(float5, 0.05f, zero, queryParameters, num - 0.05f);
				NativeArray_0[1] = new SpherecastCommand(float6, 0.05f, zero2, queryParameters, num - 0.05f);
				JobHandle dependsOn = SpherecastCommand.ScheduleBatch(NativeArray_0, NativeArray_1, math.min(NativeArray_0.Length, 4), inputDeps);
				JobHandle dependsOn2 = new GStruct94
				{
					hits = NativeArray_1,
					sourceToListenerDirection = zero,
					listenerToSourceDirection = zero2,
					directDistance = num,
					checkResult = NativeArray_7
				}.Schedule(dependsOn);
				JobHandle dependsOn3 = new GStruct92
				{
					checkResult = NativeArray_7,
					queryParams = queryParameters,
					edgeSearchRayCount = Int_6,
					edgeSearchRayLength = Float_12,
					edgeSearchRayCommands = NativeArray_2
				}.Schedule(innerloopBatchCount: math.min(Int_6, 4), arrayLength: Int_6, dependsOn: dependsOn2);
				int x2 = Int_6 * 2;
				JobHandle dependsOn4 = RaycastCommand.ScheduleBatch(NativeArray_2, NativeArray_3, math.min(x2, NativeArray_3.Length), dependsOn3);
				JobHandle dependsOn5 = new GStruct93
				{
					edgeSearchRayHits = NativeArray_3,
					edgeSearchRayCommands = NativeArray_2,
					checkResult = NativeArray_7,
					edgeSearchRayCount = Int_6,
					candidateEdgePoints = NativeList_0,
					candidateCount = NativeArray_6
				}.Schedule(dependsOn4);
				GStruct96 jobData = new GStruct96
				{
					candidateEdgePoints = NativeList_0,
					sourcePos = float5,
					listenerPos = float6,
					queryParams = queryParameters,
					edgeValidationRayOffset = Float_13,
					candidateCount = NativeArray_6,
					validationRayCommands = NativeArray_4
				};
				int num2 = Int_6 * 2;
				int innerloopBatchCount = math.min(num2, 8);
				JobHandle dependsOn6 = IJobParallelForExtensions.Schedule(jobData, num2, innerloopBatchCount, dependsOn5);
				JobHandle dependsOn7 = RaycastCommand.ScheduleBatch(NativeArray_4, NativeArray_5, math.min(num2, NativeArray_5.Length), dependsOn6);
				GStruct95 jobData2 = new GStruct95
				{
					validationRayHits = NativeArray_5,
					validationRayCommands = NativeArray_4,
					candidateEdgePoints = NativeList_0,
					candidateCount = NativeArray_6,
					sourcePos = float5,
					listenerPos = float6,
					checkResult = NativeArray_7,
					maxDiffractionPathFactor = Float_11,
					result = NativeArray_8
				};
				JobHandle_0 = jobData2.Schedule(dependsOn7);
				Bool_0 = true;
				return JobHandle_0;
			}
			if (NativeArray_8.IsCreated)
			{
				NativeArray_8[0] = GStruct90.NoPath;
			}
			Bool_0 = false;
			return inputDeps;
		}
		if (NativeArray_8.IsCreated)
		{
			NativeArray_8[0] = GStruct90.NoPath;
		}
		Bool_0 = false;
		return inputDeps;
	}

	public GStruct90 GetResult()
	{
		if (!Bool_1)
		{
			return GStruct90.NoPath;
		}
		CompleteCurrentCalculation();
		if (NativeArray_8.IsCreated && NativeArray_8.Length > 0)
		{
			return NativeArray_8[0];
		}
		GClass722.Instance.LogError("EdgeDiffractionCalculator result buffer is not valid!");
		return GStruct90.NoPath;
	}

	public void CompleteCurrentCalculation()
	{
		if (Bool_1)
		{
			if (!JobHandle_0.Equals(default(JobHandle)))
			{
				JobHandle_0.Complete();
			}
			Bool_0 = false;
		}
	}

	public float GetNormalizedPathScore()
	{
		GStruct90 result = GetResult();
		if (result.isDirectPath)
		{
			return 0f;
		}
		if (result.FoundPath && Float_14 > 0.01f)
		{
			return Mathf.Clamp01(result.pathLength / Float_14);
		}
		return 1f;
	}

	public JobHandle UpdateCalculationStep(JobHandle inputDeps = default(JobHandle))
	{
		return JobHandle_0;
	}

	public void CompleteCalculation()
	{
		CompleteCurrentCalculation();
	}

	public JobHandle StartCalculation(Vector3 listenerPosition, Vector3 sourcePosition, float maxDistance, float qualityFactor = 1f, JobHandle inputDeps = default(JobHandle))
	{
		Int_6 = Mathf.Max(1, Mathf.RoundToInt((float)Int_5 * qualityFactor));
		return StartCalculation(listenerPosition, sourcePosition, maxDistance, QueryParameters_0, inputDeps);
	}

	public void Dispose()
	{
		method_0();
	}

	public void method_0()
	{
		if (Bool_0 && !JobHandle_0.Equals(default(JobHandle)))
		{
			JobHandle_0.Complete();
			Bool_0 = false;
		}
		GClass832.DisposeIfCreated(NativeArray_0);
		GClass832.DisposeIfCreated(NativeArray_1);
		GClass832.DisposeIfCreated(NativeArray_7);
		GClass832.DisposeIfCreated(NativeArray_8);
		GClass832.DisposeIfCreated(NativeArray_2);
		GClass832.DisposeIfCreated(NativeArray_3);
		GClass832.DisposeIfCreated(NativeArray_4);
		GClass832.DisposeIfCreated(NativeArray_5);
		GClass832.DisposeIfCreated(NativeArray_6);
		if (NativeList_0.IsCreated)
		{
			NativeList_0.Dispose();
		}
		Bool_1 = false;
	}
}
