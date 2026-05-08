using System;
using System.Runtime.CompilerServices;
using Audio.SpatialSystem.Data;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class GClass1170 : GInterface89, IDisposable
{
	public enum ECalculationStep : byte
	{
		Idle,
		CastsScheduled,
		Complete
	}

	[NonSerialized]
	public const float Float_0 = 0.05f;

	[NonSerialized]
	public const float Float_1 = 1E-06f;

	[NonSerialized]
	public float Float_2;

	[NonSerialized]
	public float Float_3;

	[NonSerialized]
	public QueryParameters QueryParameters_0;

	[NonSerialized]
	public float Float_4;

	[NonSerialized]
	public float Float_5;

	[NonSerialized]
	public int Int_0;

	[NonSerialized]
	public float Float_6;

	[NonSerialized]
	public float Float_7;

	[NonSerialized]
	public float Float_8;

	[NonSerialized]
	public float Float_9;

	[NonSerialized]
	public float Float_10;

	[NonSerialized]
	public int Int_1;

	[NonSerialized]
	public int Int_2;

	[NonSerialized]
	public int Int_3;

	[NonSerialized]
	public float[] Float_11;

	[NonSerialized]
	public float[] Float_12;

	[NonSerialized]
	public JobHandle JobHandle_0;

	[NonSerialized]
	public bool Bool_0;

	[NonSerialized]
	public ECalculationStep EcalculationStep_0;

	[NonSerialized]
	public NativeArray<SpherecastCommand> NativeArray_0;

	[NonSerialized]
	public NativeArray<RaycastHit> NativeArray_1;

	[NonSerialized]
	public NativeArray<SpherecastCommand> NativeArray_2;

	[NonSerialized]
	public NativeArray<RaycastHit> NativeArray_3;

	[NonSerialized]
	public NativeArray<GStruct106> NativeArray_4;

	[NonSerialized]
	public bool Bool_1;

	[NonSerialized]
	public Vector3 Vector3_0;

	[NonSerialized]
	public Vector3 Vector3_1;

	[NonSerialized]
	public NativeArray<float3> NativeArray_5;

	[NonSerialized]
	public bool Bool_2;

	[NonSerialized]
	public NativeArray<RaycastCommand> NativeArray_6;

	[NonSerialized]
	public NativeArray<RaycastCommand> NativeArray_7;

	[NonSerialized]
	public NativeArray<RaycastCommand> NativeArray_8;

	[NonSerialized]
	public NativeArray<SpherecastCommand> NativeArray_9;

	[NonSerialized]
	public NativeArray<RaycastHit> NativeArray_10;

	[NonSerialized]
	public int Int_4;

	[NonSerialized]
	[CompilerGenerated]
	public bool Bool_3 = true;

	public JobHandle LastJobHandle => JobHandle_0;

	public bool CheckHeight
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

	public bool IsCalculationInProgress => Bool_0;

	public GClass1170(TransmissionSettings settings, QueryParameters queryParams, float maxDistance)
	{
		Bool_2 = settings.useRaycast;
		QueryParameters_0 = queryParams;
		Float_5 = maxDistance;
		Float_2 = settings.absorptionPerUnit;
		Float_3 = settings.minEnergyThreshold;
		Int_0 = math.max(1, settings.GetRaysCountByQuality());
		Float_6 = math.max(0.01f, settings.raysWideningRadius);
		Float_7 = math.max(0f, settings.obstacleMinThickness);
		Float_8 = math.max(Float_7, settings.obstacleMaxThickness);
		Float_9 = settings.listenerHeightSamplingOffset;
		Float_10 = settings.sourceHeightSamplingOffset;
		Int_1 = 2;
		Int_2 = 1 + Int_0;
		Int_3 = Int_1 * Int_2;
		Float_11 = new float[Int_2];
		Float_12 = new float[Int_2];
		Float_11[0] = 0f;
		Float_12[0] = 0f;
		for (int i = 1; i < Int_2; i++)
		{
			float x = (float)(i - 1) / (float)Int_0 * MathF.PI * 2f;
			Float_11[i] = math.cos(x);
			Float_12[i] = math.sin(x);
		}
		NativeArray_5 = new NativeArray<float3>(Int_3, Allocator.Persistent);
		NativeArray_0 = new NativeArray<SpherecastCommand>(Int_3, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		NativeArray_1 = new NativeArray<RaycastHit>(Int_3, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		NativeArray_2 = new NativeArray<SpherecastCommand>(Int_3, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		NativeArray_3 = new NativeArray<RaycastHit>(Int_3, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		NativeArray_4 = new NativeArray<GStruct106>(1, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		Bool_1 = true;
		if (Bool_2)
		{
			NativeArray_6 = new NativeArray<RaycastCommand>(Int_3, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
			NativeArray_7 = new NativeArray<RaycastCommand>(Int_3, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
			NativeArray_8 = new NativeArray<RaycastCommand>(Int_3 * 2, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		}
		else
		{
			NativeArray_9 = new NativeArray<SpherecastCommand>(Int_3 * 2, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		}
		NativeArray_10 = new NativeArray<RaycastHit>(Int_3 * 2, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
	}

	public JobHandle StartCalculation(Vector3 listenerPosition, Vector3 sourcePosition, float maxDistance, float qualityFactor = 1f, JobHandle inputDeps = default(JobHandle))
	{
		if (!Bool_1)
		{
			Bool_0 = false;
			EcalculationStep_0 = ECalculationStep.Idle;
			return inputDeps;
		}
		Vector3_0 = listenerPosition;
		Vector3_1 = sourcePosition;
		Float_4 = Mathf.Min(maxDistance, Float_5);
		float3 float5 = listenerPosition;
		float3 float6 = sourcePosition;
		if (Class805.ComputeDistanceSq(float5, float6) > Float_4 * Float_4)
		{
			if (NativeArray_4.IsCreated)
			{
				NativeArray_4[0] = GStruct106.NoPath;
			}
			Bool_0 = false;
			EcalculationStep_0 = ECalculationStep.Idle;
			return inputDeps;
		}
		float3 float7 = Class805.ComputeMainDirection(float5, float6, 1E-06f);
		if (math.length(float7) < 1E-06f)
		{
			if (NativeArray_4.IsCreated)
			{
				NativeArray_4[0] = GStruct106.ClearPath;
			}
			Bool_0 = false;
			EcalculationStep_0 = ECalculationStep.Idle;
			return inputDeps;
		}
		Class805.ComputeHorizontalAxes(float7, 1E-06f, out var forwardXZ, out var rightXZ);
		method_0(float5, float6, forwardXZ, rightXZ);
		int num = Mathf.RoundToInt((float)Int_0 * qualityFactor);
		int num2 = 1 + num;
		int num3 = ((!CheckHeight) ? 1 : Int_1);
		Int_4 = num2 * num3;
		int int_ = Int_4;
		if (Bool_2)
		{
			for (int i = 0; i < int_; i++)
			{
				NativeArray_8[i] = NativeArray_6[i];
				NativeArray_8[i + int_] = NativeArray_7[i];
			}
			JobHandle_0 = RaycastCommand.ScheduleBatch(NativeArray_8, NativeArray_10, int_ * 2, inputDeps);
		}
		else
		{
			for (int j = 0; j < int_; j++)
			{
				NativeArray_9[j] = NativeArray_0[j];
				NativeArray_9[j + int_] = NativeArray_2[j];
			}
			JobHandle_0 = SpherecastCommand.ScheduleBatch(NativeArray_9, NativeArray_10, int_ * 2, inputDeps);
		}
		Bool_0 = true;
		EcalculationStep_0 = ECalculationStep.CastsScheduled;
		return JobHandle_0;
	}

	public void method_0(float3 listenerPosF3, float3 sourcePosF3, float3 forwardXZ, float3 rightXZ)
	{
		int num = ((!CheckHeight) ? 1 : Int_1);
		for (int i = 0; i < num; i++)
		{
			float height = ((i == 0) ? 0f : Float_9);
			float height2 = ((i == 0) ? 0f : Float_10);
			for (int j = 0; j < Int_2; j++)
			{
				int index = i * Int_2 + j;
				float num2 = Float_11[j];
				float num3 = Float_12[j];
				float3 float5 = (rightXZ * num2 + forwardXZ * num3) * Float_6;
				float3 float6 = Class805.ApplyHeightOffset(listenerPosF3, height) + float5;
				float3 float7 = Class805.ApplyHeightOffset(sourcePosF3, height2) + float5;
				NativeArray_5[index] = float6;
				float3 float8 = float7 - float6;
				float distSq = Class805.ComputeDistanceSq(float6, float7);
				if (!Class805.IsValidCastDistance(distSq, 0.05f, Float_4))
				{
					if (Bool_2)
					{
						NativeArray_6[index] = default(RaycastCommand);
						NativeArray_7[index] = default(RaycastCommand);
					}
					else
					{
						NativeArray_0[index] = default(SpherecastCommand);
						NativeArray_2[index] = default(SpherecastCommand);
					}
					continue;
				}
				float num4 = Class805.ComputeCastDistance(distSq);
				float3 float9 = float8 / num4;
				if (Bool_2)
				{
					NativeArray_6[index] = new RaycastCommand(float6, float9, QueryParameters_0, num4);
					NativeArray_7[index] = new RaycastCommand(float7, -float9, QueryParameters_0, num4);
				}
				else
				{
					NativeArray_0[index] = new SpherecastCommand(float6, 0.05f, float9, QueryParameters_0, num4);
					NativeArray_2[index] = new SpherecastCommand(float7, 0.05f, -float9, QueryParameters_0, num4);
				}
			}
		}
	}

	public JobHandle UpdateCalculationStep(JobHandle inputDeps = default(JobHandle))
	{
		if (Bool_0 && Bool_1)
		{
			JobHandle dependsOn = JobHandle.CombineDependencies(inputDeps, JobHandle_0);
			JobHandle jobHandle = default(JobHandle);
			switch (EcalculationStep_0)
			{
			default:
				return default(JobHandle);
			case ECalculationStep.CastsScheduled:
			{
				NativeArray<RaycastHit> subArray = NativeArray_10.GetSubArray(0, Int_4);
				NativeArray<RaycastHit> subArray2 = NativeArray_10.GetSubArray(Int_4, Int_4);
				Struct215 jobData = new Struct215
				{
					listenerToSourceHits = subArray,
					sourceToListenerHits = subArray2,
					rayStartPoints = NativeArray_5,
					ListenerPosition = Vector3_0,
					sourcePosition = Vector3_1,
					absorptionPerUnit = Float_2,
					minEnergyThreshold = Float_3,
					minThicknessThreshold = Float_7,
					maxThicknessThreshold = Float_8,
					maxDistance = Float_4,
					totalRayCount = Int_4,
					aggregatedResultBuffer = NativeArray_4
				};
				JobHandle_0 = jobData.Schedule(dependsOn);
				EcalculationStep_0 = ECalculationStep.Complete;
				return JobHandle_0;
			}
			case ECalculationStep.Complete:
				return JobHandle_0;
			}
		}
		return inputDeps;
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
			EcalculationStep_0 = ECalculationStep.Idle;
			JobHandle_0 = default(JobHandle);
		}
	}

	public GStruct106 GetResult()
	{
		if (!Bool_1)
		{
			return GStruct106.NoPath;
		}
		if (NativeArray_4.IsCreated && NativeArray_4.Length > 0)
		{
			return NativeArray_4[0];
		}
		GClass722.Instance.LogError("TransmissionCalculator result buffer is not valid!");
		return GStruct106.NoPath;
	}

	public float GetNormalizedPathScore()
	{
		if (!Bool_1)
		{
			return 1f;
		}
		return 1f - Mathf.Clamp01(GetResult().RemainingEnergy);
	}

	public void Dispose()
	{
		method_1();
	}

	public void method_1()
	{
		if (!JobHandle_0.Equals(default(JobHandle)))
		{
			JobHandle_0.Complete();
		}
		GClass832.DisposeIfCreated(NativeArray_0);
		GClass832.DisposeIfCreated(NativeArray_1);
		GClass832.DisposeIfCreated(NativeArray_2);
		GClass832.DisposeIfCreated(NativeArray_3);
		if (Bool_2)
		{
			GClass832.DisposeIfCreated(NativeArray_6);
			GClass832.DisposeIfCreated(NativeArray_7);
			GClass832.DisposeIfCreated(NativeArray_8);
		}
		else
		{
			GClass832.DisposeIfCreated(NativeArray_9);
		}
		GClass832.DisposeIfCreated(NativeArray_4);
		GClass832.DisposeIfCreated(NativeArray_5);
		GClass832.DisposeIfCreated(NativeArray_10);
		Bool_1 = false;
	}

	public void CompleteCalculation()
	{
		CompleteCurrentCalculation();
	}
}
