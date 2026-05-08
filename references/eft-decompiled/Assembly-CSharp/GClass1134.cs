using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Audio.SpatialSystem;
using Audio.SpatialSystem.Data;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class GClass1134 : GClass1128
{
	public enum ECombinedStep
	{
		Idle,
		DirectTestScheduled,
		TDScheduled,
		RScheduled,
		Complete
	}

	[NonSerialized]
	public float Float_2;

	[NonSerialized]
	public LayerMask LayerMask_0;

	[NonSerialized]
	public float Float_3;

	[NonSerialized]
	public float Float_4;

	[NonSerialized]
	public float Float_5 = 0.5f;

	[NonSerialized]
	public Vector3 Vector3_0;

	[NonSerialized]
	public Vector3 Vector3_1;

	[NonSerialized]
	public float Float_6;

	[NonSerialized]
	public float Float_7;

	[NonSerialized]
	public bool Bool_1;

	[NonSerialized]
	public bool Bool_2 = true;

	[NonSerialized]
	public GInterface89 Ginterface89_0;

	[NonSerialized]
	public GInterface89 Ginterface89_1;

	[NonSerialized]
	public GClass1170 Gclass1170_0;

	[NonSerialized]
	public List<GInterface89> List_0;

	[NonSerialized]
	public GClass1160 Gclass1160_0;

	[NonSerialized]
	public JobHandle JobHandle_0;

	[NonSerialized]
	public JobHandle JobHandle_1;

	[NonSerialized]
	public JobHandle JobHandle_2;

	[NonSerialized]
	public ECombinedStep EcombinedStep_0;

	[NonSerialized]
	public bool Bool_3;

	[NonSerialized]
	public bool Bool_4;

	[NonSerialized]
	public bool Bool_5;

	[NonSerialized]
	public float Float_8;

	[NonSerialized]
	public float Float_9;

	[NonSerialized]
	public float Float_10;

	[NonSerialized]
	public GClass1180 Gclass1180_0;

	[NonSerialized]
	public int Int_0;

	[NonSerialized]
	[CompilerGenerated]
	public GStruct107 Gstruct107_0 = GStruct107.Clear;

	public bool IsCalculating => Bool_1;

	public GStruct107 LastAggregateResult
	{
		[CompilerGenerated]
		get
		{
			return Gstruct107_0;
		}
		[CompilerGenerated]
		set
		{
			Gstruct107_0 = value;
		}
	}

	public GClass1134(AudioOcclusionSettings occlusionSettings, SpatialAudioSystem system)
		: base(system)
	{
		CommonOcclusionSettings commonSettings = occlusionSettings.commonSettings;
		QueryParameters queryParameters = new QueryParameters(commonSettings.occlusionLayerMask);
		Float_2 = commonSettings.maxDistance;
		LayerMask_0 = commonSettings.occlusionLayerMask;
		Float_3 = commonSettings.transmissionThreshold;
		Float_4 = commonSettings.diffractionThreshold;
		Float_5 = occlusionSettings.transmissionSettings.minClearPathScore;
		Ginterface89_0 = new GClass1168(occlusionSettings.reflectionSettings, queryParameters, Float_2);
		Ginterface89_1 = new GClass1167(occlusionSettings.diffractionSettings, queryParameters, Float_2);
		Gclass1170_0 = new GClass1170(occlusionSettings.transmissionSettings, queryParameters, Float_2);
		Gclass1160_0 = new GClass1160();
		Gclass1160_0.SetThreshold(commonSettings.GetPositionThresholdByQuality());
		List_0 = new List<GInterface89> { Ginterface89_0, Ginterface89_1, Gclass1170_0 };
		Gclass1180_0 = new GClass1180();
	}

	public override void OnWarmUp()
	{
		GClass1138 data = GClass3670.GetData<GClass1138>();
		RoomPair value = data.RoomPairsByID.FirstOrDefault().Value;
		if (value != null && data.RoomsByID.TryGetValue(value.FirstRoomID, out var value2) && data.RoomsByID.TryGetValue(value.SecondRoomID, out var value3))
		{
			method_0(Ginterface89_0, value2.Position, value3.Position, 10000f);
			method_0(Gclass1170_0, value2.Position, value3.Position, 10000f);
			method_0(Ginterface89_1, value2.Position, value3.Position, 10000f);
		}
	}

	public void method_0(GInterface89 occlusionCalculator, Vector3 listenerPosition, Vector3 sourcePosition, float maxDistance)
	{
		occlusionCalculator.StartCalculation(listenerPosition, sourcePosition, maxDistance);
		occlusionCalculator.UpdateCalculationStep();
		occlusionCalculator.CompleteCalculation();
	}

	public void StartCalculation(Vector3 listenerPosition, Vector3 sourcePosition, float maxDistance, float sqrMaxDistance)
	{
		EcombinedStep_0 = ECombinedStep.Idle;
		Bool_3 = false;
		Bool_4 = false;
		Bool_5 = false;
		Float_8 = 0f;
		Float_9 = 0f;
		Float_10 = 0f;
		Vector3_0 = listenerPosition;
		Vector3_1 = sourcePosition;
		Float_7 = maxDistance;
		float num = math.distancesq(sourcePosition, listenerPosition);
		Float_6 = sqrMaxDistance;
		if (num > sqrMaxDistance)
		{
			LastAggregateResult = GStruct107.Clear;
			Bool_1 = false;
			Bool_2 = false;
			EcombinedStep_0 = ECombinedStep.Complete;
			return;
		}
		Int_0 = Gclass1180_0.CalculatePriority(num, sqrMaxDistance);
		float qualityFactor = 0.3f;
		Gclass1170_0.CheckHeight = false;
		Bool_3 = true;
		Bool_4 = false;
		Bool_5 = false;
		JobHandle_1 = Gclass1170_0.StartCalculation(listenerPosition, sourcePosition, Float_7, qualityFactor);
		EcombinedStep_0 = ECombinedStep.DirectTestScheduled;
		Bool_1 = true;
		Bool_2 = false;
	}

	public void method_1()
	{
		if (Bool_5)
		{
			JobHandle_0 = Ginterface89_0.UpdateCalculationStep(JobHandle_0);
		}
		if (Bool_4)
		{
			JobHandle_2 = Ginterface89_1.UpdateCalculationStep(JobHandle_2);
		}
		if (Bool_3)
		{
			JobHandle_1 = Gclass1170_0.UpdateCalculationStep(JobHandle_1);
		}
		Bool_1 = (Bool_5 && Ginterface89_0.IsCalculationInProgress) || (Bool_4 && Ginterface89_1.IsCalculationInProgress) || (Bool_3 && Gclass1170_0.IsCalculationInProgress);
	}

	public GStruct107 FinalizeAndGetAggregateResult()
	{
		Ginterface89_0.CompleteCalculation();
		Ginterface89_1.CompleteCalculation();
		Gclass1170_0.CompleteCalculation();
		Bool_1 = false;
		Bool_2 = false;
		float normalizedPathScore = Ginterface89_0.GetNormalizedPathScore();
		float normalizedPathScore2 = Ginterface89_1.GetNormalizedPathScore();
		float normalizedPathScore3 = Gclass1170_0.GetNormalizedPathScore();
		return LastAggregateResult = new GStruct107
		{
			reflectionScore = normalizedPathScore,
			diffractionScore = normalizedPathScore2,
			transmissionScore = normalizedPathScore3
		};
	}

	public override float GetScore()
	{
		GStruct107 lastAggregateResult = LastAggregateResult;
		int num = 0;
		float num2 = 0f;
		if (Bool_3)
		{
			num2 += lastAggregateResult.transmissionScore;
			num++;
		}
		if (Bool_4)
		{
			num2 += lastAggregateResult.diffractionScore;
			num++;
		}
		if (Bool_5)
		{
			num2 += lastAggregateResult.reflectionScore;
			num++;
		}
		if (num <= 0)
		{
			return 0f;
		}
		return math.clamp(num2 / (float)num, 0f, 1f);
	}

	public GStruct107 GetLastResult()
	{
		return LastAggregateResult;
	}

	public override void OnUpdate(SourceContainerClass sourceContainer, Vector3 listenerPosition, bool forceUpdate = false)
	{
		if (Gclass1160_0.IsPositionChanged(sourceContainer.CurrentPosition, listenerPosition))
		{
			Bool_2 = true;
		}
		_ = sourceContainer.CurrentPosition - listenerPosition;
		if (Bool_2 || forceUpdate)
		{
			if (!Bool_1)
			{
				StartCalculation(listenerPosition, sourceContainer.CurrentPosition, sourceContainer.MaxDistance, sourceContainer.SqrMaxDistance);
				return;
			}
			Bool_2 = true;
			method_1();
		}
		else if (Bool_1)
		{
			method_1();
		}
	}

	public override void OnLateUpdate()
	{
		base.OnLateUpdate();
		switch (EcombinedStep_0)
		{
		case ECombinedStep.DirectTestScheduled:
		{
			method_1();
			if (!JobHandle_1.IsCompleted)
			{
				break;
			}
			Gclass1170_0.CompleteCalculation();
			Float_8 = Gclass1170_0.GetNormalizedPathScore();
			if (Float_8 <= Float_5)
			{
				LastAggregateResult = GStruct107.Clear;
				Bool_1 = false;
				EcombinedStep_0 = ECombinedStep.Complete;
				Bool_2 = false;
				break;
			}
			Gclass1170_0.CheckHeight = true;
			bool flag = Int_0 == Gclass1180_0.HighAudioSourcePriority;
			bool flag2 = Int_0 == Gclass1180_0.MedAudioSourcePriority;
			float qualityFactor = Gclass1180_0.CalculateQualityFactorSq(math.distancesq(Vector3_1, Vector3_0), Float_6);
			Bool_3 = true;
			Bool_4 = flag || flag2;
			Bool_5 = false;
			JobHandle_1 = Gclass1170_0.StartCalculation(Vector3_0, Vector3_1, Float_7, qualityFactor);
			if (Bool_4)
			{
				JobHandle_2 = Ginterface89_1.StartCalculation(Vector3_0, Vector3_1, Float_7, qualityFactor);
			}
			else
			{
				JobHandle_2 = default(JobHandle);
			}
			EcombinedStep_0 = ECombinedStep.TDScheduled;
			Bool_1 = true;
			break;
		}
		case ECombinedStep.TDScheduled:
			if (Bool_4)
			{
				if (JobHandle_1.IsCompleted && JobHandle_2.IsCompleted)
				{
					Gclass1170_0.CompleteCalculation();
					Ginterface89_1.CompleteCalculation();
					Float_8 = Gclass1170_0.GetNormalizedPathScore();
					Float_9 = Ginterface89_1.GetNormalizedPathScore();
					if (Int_0 != Gclass1180_0.HighAudioSourcePriority && Int_0 != Gclass1180_0.MedAudioSourcePriority)
					{
						LastAggregateResult = new GStruct107
						{
							reflectionScore = 0f,
							diffractionScore = Float_9,
							transmissionScore = Float_8
						};
						Bool_1 = false;
						Bool_2 = false;
						EcombinedStep_0 = ECombinedStep.Complete;
					}
					else if (Float_8 > Float_3 && Float_9 > Float_4)
					{
						Bool_5 = true;
						float qualityFactor2 = Gclass1180_0.CalculateQualityFactorSq(math.distancesq(Vector3_1, Vector3_0), Float_6);
						JobHandle_0 = Ginterface89_0.StartCalculation(Vector3_0, Vector3_1, Float_7, qualityFactor2);
						EcombinedStep_0 = ECombinedStep.RScheduled;
					}
					else
					{
						LastAggregateResult = new GStruct107
						{
							reflectionScore = 0f,
							diffractionScore = Float_9,
							transmissionScore = Float_8
						};
						Bool_1 = false;
						Bool_2 = false;
						EcombinedStep_0 = ECombinedStep.Complete;
					}
				}
			}
			else if (JobHandle_1.IsCompleted)
			{
				Gclass1170_0.CompleteCalculation();
				Float_8 = Gclass1170_0.GetNormalizedPathScore();
				LastAggregateResult = new GStruct107
				{
					reflectionScore = 0f,
					diffractionScore = 0f,
					transmissionScore = Float_8
				};
				Bool_1 = false;
				Bool_2 = false;
				EcombinedStep_0 = ECombinedStep.Complete;
			}
			break;
		case ECombinedStep.RScheduled:
			if (JobHandle_0.IsCompleted)
			{
				Ginterface89_0.CompleteCalculation();
				Float_10 = Ginterface89_0.GetNormalizedPathScore();
				LastAggregateResult = new GStruct107
				{
					reflectionScore = Float_10,
					diffractionScore = Float_9,
					transmissionScore = Float_8
				};
				Bool_1 = false;
				Bool_2 = false;
				EcombinedStep_0 = ECombinedStep.Complete;
			}
			break;
		case ECombinedStep.Idle:
		case ECombinedStep.Complete:
			break;
		}
	}

	public override void OnDispose()
	{
		if (IsCalculating)
		{
			FinalizeAndGetAggregateResult();
		}
		Ginterface89_0.Dispose();
		Ginterface89_1.Dispose();
		Gclass1170_0.Dispose();
		GC.SuppressFinalize(this);
		base.OnDispose();
	}

	public override void OnReset()
	{
		LastAggregateResult = GStruct107.Clear;
		base.OnReset();
	}

	~GClass1134()
	{
		Dispose();
	}
}
