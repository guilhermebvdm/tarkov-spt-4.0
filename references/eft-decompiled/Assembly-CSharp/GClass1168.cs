using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Audio.SpatialSystem.Data;
using Audio.SpatialSystem.SpatialAudioCalculator;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class GClass1168 : GInterface89, IDisposable
{
	[BurstCompile]
	public struct Struct213 : IJob
	{
		public NativeList<GStruct101> pathPointInfos;

		public void Execute()
		{
			pathPointInfos.Clear();
		}
	}

	[NonSerialized]
	public const float Float_0 = 0.001f;

	[NonSerialized]
	public const float Float_1 = 0.03f;

	[NonSerialized]
	public const float Float_2 = 0.05f;

	[NonSerialized]
	public const float Float_3 = 0.05f;

	[NonSerialized]
	public const int Int_0 = 2048;

	[NonSerialized]
	public int Int_1;

	[NonSerialized]
	public int Int_2;

	[NonSerialized]
	public QueryParameters QueryParameters_0;

	[NonSerialized]
	public float Float_4;

	[NonSerialized]
	public float Float_5;

	[NonSerialized]
	public int Int_3;

	[NonSerialized]
	public List<Vector3> List_0;

	[NonSerialized]
	public float Float_6;

	[NonSerialized]
	public float Float_7;

	[NonSerialized]
	public int Int_4;

	[NonSerialized]
	public bool Bool_0;

	[NonSerialized]
	public int Int_5 = -1;

	[NonSerialized]
	public Vector3 Vector3_0;

	[NonSerialized]
	public Vector3 Vector3_1;

	[NonSerialized]
	public bool Bool_1;

	[NonSerialized]
	public NativeArray<Vector3> NativeArray_0;

	[NonSerialized]
	public NativeArray<GStruct105> NativeArray_1;

	[NonSerialized]
	public NativeArray<SpherecastCommand> NativeArray_2;

	[NonSerialized]
	public NativeArray<RaycastHit> NativeArray_3;

	[NonSerialized]
	public NativeArray<int2> NativeArray_4;

	[NonSerialized]
	public NativeArray<float> NativeArray_5;

	[NonSerialized]
	public NativeArray<float> NativeArray_6;

	[NonSerialized]
	public NativeArray<byte> NativeArray_7;

	[NonSerialized]
	public NativeList<GStruct101> NativeList_0;

	[NonSerialized]
	public NativeArray<GStruct104> NativeArray_8;

	[NonSerialized]
	public NativeArray<GStruct103> NativeArray_9;

	[NonSerialized]
	public NativeArray<SpherecastCommand> NativeArray_10;

	[NonSerialized]
	public bool Bool_2;

	[NonSerialized]
	public JobHandle JobHandle_0;

	[NonSerialized]
	public GStruct102 Gstruct102_0;

	[NonSerialized]
	public GStruct102 Gstruct102_1;

	[NonSerialized]
	public GStruct104 Gstruct104_0;

	[NonSerialized]
	public GStruct104 Gstruct104_1;

	[NonSerialized]
	public int Int_6;

	[NonSerialized]
	public float Float_8;

	[NonSerialized]
	[CompilerGenerated]
	public bool Bool_3;

	public JobHandle LastJobHandle => JobHandle_0;

	public bool IsCalculationInProgress => Bool_0;

	public bool IsDirectPathClear
	{
		[CompilerGenerated]
		get
		{
			return Bool_3;
		}
		[CompilerGenerated]
		set
		{
			Bool_3 = value;
		}
	}

	public GClass1168(ReflectionSettings settings, QueryParameters queryParameters, float sourceMaxDistance)
	{
		Int_1 = Mathf.Max(1, settings.GetRaysCountByQuality());
		Int_2 = Mathf.Max(1, settings.maxReflections);
		Float_4 = settings.energyLossFactorPerReflection;
		Float_5 = settings.minEnergyAtMaxDistance;
		QueryParameters_0 = queryParameters;
		Float_7 = sourceMaxDistance;
		NativeArray_0 = GClass1169.GenerateFibonacciSphereDirections(Int_1, Allocator.Persistent);
		List_0 = new List<Vector3>();
		Int_3 = 1 + Int_1 * 2;
		NativeArray_1 = new NativeArray<GStruct105>(Int_1, Allocator.Persistent);
		NativeArray_2 = new NativeArray<SpherecastCommand>(Int_3, Allocator.Persistent);
		NativeArray_3 = new NativeArray<RaycastHit>(Int_3, Allocator.Persistent);
	}

	public void method_0()
	{
		if (!Bool_2)
		{
			NativeArray_4 = new NativeArray<int2>(Int_1, Allocator.Persistent);
			NativeArray_5 = new NativeArray<float>(Int_1, Allocator.Persistent);
			NativeArray_6 = new NativeArray<float>(Int_1, Allocator.Persistent);
			NativeArray_7 = new NativeArray<byte>(Int_1, Allocator.Persistent);
			int initialCapacity = Math.Max(2048, Int_1 * Int_2);
			NativeList_0 = new NativeList<GStruct101>(initialCapacity, Allocator.Persistent);
			NativeArray_8 = new NativeArray<GStruct104>(1, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
			NativeArray_9 = new NativeArray<GStruct103>(1, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
			NativeArray_10 = new NativeArray<SpherecastCommand>(Int_3, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
			Bool_2 = true;
		}
	}

	public void method_1()
	{
		if (Bool_2)
		{
			GClass832.DisposeIfCreated(NativeArray_4);
			GClass832.DisposeIfCreated(NativeArray_5);
			GClass832.DisposeIfCreated(NativeArray_6);
			GClass832.DisposeIfCreated(NativeArray_7);
			GClass832.DisposeIfCreated(NativeArray_8);
			GClass832.DisposeIfCreated(NativeArray_9);
			GClass832.DisposeIfCreated(NativeArray_10);
			if (NativeList_0.IsCreated)
			{
				NativeList_0.Dispose();
			}
			Bool_2 = false;
		}
	}

	public bool method_2(Vector3 listenerPosition, Vector3 sourcePosition, float maxDistance)
	{
		if (Bool_1)
		{
			return false;
		}
		Vector3_0 = listenerPosition;
		Vector3_1 = sourcePosition;
		float sqrMagnitude = (sourcePosition - listenerPosition).sqrMagnitude;
		float num = maxDistance * maxDistance;
		if (sqrMagnitude > num)
		{
			Gstruct102_0 = GStruct102.NoPath;
			Gstruct102_1 = GStruct102.NoPath;
			return false;
		}
		method_0();
		Gstruct104_0 = new GStruct104
		{
			found = false,
			pathLength = float.MaxValue
		};
		Gstruct104_1 = new GStruct104
		{
			found = false,
			pathLength = float.MaxValue
		};
		return true;
	}

	public int method_3(Vector3 sourcePosition, out int commandCount)
	{
		NativeArray<Vector3> nativeArray_ = NativeArray_0;
		commandCount = 0;
		int result = -1;
		float3 x = (float3)Vector3_0 - (float3)sourcePosition;
		float num = math.sqrt(math.lengthsq(x));
		if (num > 0.001f && num <= Float_6)
		{
			if (commandCount < Int_3)
			{
				result = commandCount;
				if (math.lengthsq(x) > 0.0001f)
				{
					float3 float5 = math.normalize(x);
					NativeArray_2[commandCount++] = new SpherecastCommand(sourcePosition, 0.03f, float5, QueryParameters_0, num - 0.001f);
				}
				else
				{
					result = -1;
				}
			}
			else
			{
				GClass722.Instance.LogError("[ReflectionCalculator] Max commands exceeded (direct path).");
			}
		}
		for (int i = 0; i < Int_6; i++)
		{
			Vector3 vector = ((i < nativeArray_.Length) ? nativeArray_[i] : Vector3.forward);
			int raySegmentCommandIndex = -1;
			if (commandCount < Int_3)
			{
				raySegmentCommandIndex = commandCount;
				NativeArray_2[commandCount++] = new SpherecastCommand(sourcePosition, 0.03f, vector, QueryParameters_0, Float_6);
			}
			else
			{
				GClass722.Instance.LogError($"[ReflectionCalculator] Max commands exceeded (ray {i}).");
			}
			NativeArray_1[i] = new GStruct105
			{
				isActive = true,
				currentOrigin = sourcePosition,
				currentDirection = vector,
				distanceTraveled = 0f,
				remainingDistance = Float_6,
				reflectionCount = 0,
				currentEnergy = 1f,
				raySegmentCommandIndex = raySegmentCommandIndex,
				listenerCheckSphereCommandIndex = -1
			};
		}
		for (int j = Int_6; j < Int_1; j++)
		{
			NativeArray_1[j] = new GStruct105
			{
				isActive = false,
				raySegmentCommandIndex = -1,
				listenerCheckSphereCommandIndex = -1,
				currentOrigin = default(Vector3),
				currentDirection = default(Vector3),
				distanceTraveled = 0f,
				remainingDistance = 0f,
				reflectionCount = 0,
				currentEnergy = 0f
			};
		}
		return result;
	}

	public JobHandle method_4(JobHandle spherecastBatchHandle)
	{
		JobHandle dependsOn = new Struct213
		{
			pathPointInfos = NativeList_0
		}.Schedule(spherecastBatchHandle);
		GStruct100 jobData = new GStruct100
		{
			minEnergyAtMaxDistance = Float_5,
			minValidReflectionEnergy = 0.05f,
			commandIndices = NativeArray_4,
			previousStepSphereHits = NativeArray_3,
			rayStates = NativeArray_1,
			listenerPosition = Vector3_0,
			maxReflections = Int_2,
			rayOffset = 0.001f,
			queryParams = QueryParameters_0,
			maxCommandsInBuffer = Int_3,
			reflectionEnergyLossFactor = Float_4,
			spherecastRadius = 0.03f,
			listenerCheckSpherecastRadius = 0.05f,
			nextSpherecastCommands = NativeArray_2,
			minPathLengths = NativeArray_5,
			pathEnergies = NativeArray_6,
			pathEndStatuses = NativeArray_7,
			pathPointInfosWriter = NativeList_0.AsParallelWriter()
		};
		jobData.nextSpherecastCommands = NativeArray_2;
		int innerloopBatchCount = Mathf.Max(1, Mathf.CeilToInt((float)Int_6 / 128f));
		return IJobParallelForExtensions.Schedule(jobData, Int_6, innerloopBatchCount, dependsOn);
	}

	public JobHandle method_5(int level, JobHandle deps)
	{
		return new GStruct98
		{
			rayMinPathLengths = NativeArray_5,
			rayEnergies = NativeArray_6,
			rayPathPointInfos = NativeList_0,
			rayEndStatuses = NativeArray_7,
			initialBestOverallResult = Gstruct104_0,
			initialBestReflectedResult = Gstruct104_1,
			previousStepSphereHits = NativeArray_3,
			directSpherecastCommandIndex = Int_5,
			sourcePosition = Vector3_1,
			listenerPosition = Vector3_0,
			minReflectionEnergyThreshold = 0.05f,
			scheduledSpherecastCommands = NativeArray_2,
			rayStates = NativeArray_1,
			maxCommandBufferSize = Int_3,
			outputSpherecastCommands = NativeArray_10,
			outputCommandIndicesMap = NativeArray_4,
			outputBatchProcessing = NativeArray_9
		}.Schedule(deps);
	}

	public JobHandle StartCalculation(Vector3 listenerPosition, Vector3 sourcePosition, float maxDistance, float qualityFactor = 1f, JobHandle inputDeps = default(JobHandle))
	{
		Int_6 = Mathf.Max(1, Mathf.CeilToInt((float)Int_1 * qualityFactor));
		Bool_0 = false;
		Float_6 = Mathf.Min(maxDistance, Float_7);
		if (!method_2(listenerPosition, sourcePosition, maxDistance))
		{
			return default(JobHandle);
		}
		int commandCount;
		int int_ = method_3(sourcePosition, out commandCount);
		Int_4 = 0;
		Gstruct102_0 = GStruct102.NoPath;
		Gstruct102_1 = GStruct102.NoPath;
		IsDirectPathClear = false;
		Int_5 = int_;
		if (Bool_2)
		{
			for (int i = 0; i < Int_1; i++)
			{
				NativeArray_4[i] = new int2((i < Int_6) ? NativeArray_1[i].raySegmentCommandIndex : (-1), -1);
			}
			if (commandCount == 0)
			{
				Bool_0 = false;
				Debug.LogWarning("[Reflection Start] No commands, skipping.");
				return default(JobHandle);
			}
			Bool_0 = true;
			JobHandle dependsOn = JobHandle.CombineDependencies(inputDeps, JobHandle_0);
			int minCommandsPerJob = Mathf.Max(1, Mathf.CeilToInt((float)commandCount / 128f));
			JobHandle spherecastBatchHandle = SpherecastCommand.ScheduleBatch(NativeArray_2.GetSubArray(0, commandCount), NativeArray_3.GetSubArray(0, commandCount), minCommandsPerJob, dependsOn);
			JobHandle deps = method_4(spherecastBatchHandle);
			Int_4 = 1;
			return JobHandle_0 = method_5(Int_4, deps);
		}
		Bool_0 = false;
		JobHandle_0 = default(JobHandle);
		Debug.LogError("[ReflectionCalculator.StartCalculation] Failed to allocate buffers, cannot proceed.");
		return default(JobHandle);
	}

	public void method_6(GStruct104 reflectedData)
	{
		if (List_0 == null || !reflectedData.found || reflectedData.pathPoints.Length == 0)
		{
			return;
		}
		foreach (GStruct101 pathPoint in reflectedData.pathPoints)
		{
			List_0.Add(pathPoint.point);
		}
	}

	public float GetNormalizedPathScore()
	{
		if (Bool_1)
		{
			return 1f;
		}
		if (Bool_0)
		{
			return Float_8;
		}
		float num;
		if (IsDirectPathClear)
		{
			num = 0f;
		}
		else
		{
			GStruct102 gstruct102_ = Gstruct102_0;
			num = ((gstruct102_.pathType == EPathType.Obstructed || Float_6 <= 0f) ? 1f : Mathf.Clamp01(gstruct102_.pathLength / Float_6));
		}
		Float_8 = num;
		return num;
	}

	public void CompleteCalculationAndWait()
	{
		if (!Bool_1 && (Bool_0 || !JobHandle_0.Equals(default(JobHandle))))
		{
			if (!JobHandle_0.Equals(default(JobHandle)))
			{
				JobHandle_0.Complete();
			}
			method_7();
			JobHandle_0 = default(JobHandle);
		}
	}

	public void method_7()
	{
		if (Bool_1 || !Bool_2)
		{
			return;
		}
		if (NativeArray_9.IsCreated && NativeArray_9.Length != 0)
		{
			GStruct103 gStruct = NativeArray_9[0];
			Gstruct104_0 = gStruct.bestOverallDataSoFar;
			Gstruct104_1 = gStruct.bestReflectedDataSoFar;
			if (gStruct.foundDirectPathClearThisBatch)
			{
				IsDirectPathClear = true;
			}
			if (Gstruct104_0.found)
			{
				Gstruct102_0 = new GStruct102
				{
					pathLength = Gstruct104_0.pathLength,
					pathType = EPathType.Reflection
				};
				float num = math.distance(Vector3_1, Vector3_0);
				if (math.abs(Gstruct104_0.pathLength - num) < 0.01f)
				{
					Gstruct102_0.pathType = EPathType.Direct;
				}
			}
			else
			{
				Gstruct102_0 = GStruct102.NoPath;
			}
			if (Gstruct104_1.found)
			{
				float num2 = math.distance(Vector3_1, Vector3_0);
				if (math.abs(Gstruct104_1.pathLength - num2) >= 0.01f)
				{
					Gstruct102_1 = new GStruct102
					{
						pathLength = Gstruct104_1.pathLength,
						pathType = EPathType.Reflection
					};
				}
				else
				{
					Gstruct102_1 = GStruct102.NoPath;
				}
			}
			else
			{
				Gstruct102_1 = GStruct102.NoPath;
			}
			Bool_0 = false;
		}
		else
		{
			GClass722.Instance.LogError("[Reflection Final Processing] _batchProcessingResultBuffer not valid! Cannot finalize.");
			Bool_0 = false;
		}
	}

	public void Dispose()
	{
		method_8();
	}

	public void method_8()
	{
		if (!Bool_1)
		{
			CompleteCalculationAndWait();
			GClass832.DisposeIfCreated(NativeArray_1);
			GClass832.DisposeIfCreated(NativeArray_2);
			GClass832.DisposeIfCreated(NativeArray_3);
			method_1();
			GClass832.DisposeIfCreated(NativeArray_0);
			Bool_1 = true;
		}
	}

	public JobHandle UpdateCalculationStep(JobHandle inputDeps = default(JobHandle))
	{
		if (!Bool_0)
		{
			return default(JobHandle);
		}
		if (!JobHandle_0.IsCompleted)
		{
			return JobHandle_0;
		}
		if (Int_4 < Int_2)
		{
			int int_ = Int_4 + 1;
			int minCommandsPerJob = Mathf.Max(1, Mathf.CeilToInt((float)Int_3 / 128f));
			JobHandle spherecastBatchHandle = SpherecastCommand.ScheduleBatch(NativeArray_10.GetSubArray(0, Int_3), NativeArray_3.GetSubArray(0, Int_3), minCommandsPerJob, JobHandle_0);
			JobHandle deps = method_4(spherecastBatchHandle);
			Int_4 = int_;
			return JobHandle_0 = method_5(Int_4, deps);
		}
		return JobHandle_0;
	}

	public void CompleteCalculation()
	{
		CompleteCalculationAndWait();
	}
}
