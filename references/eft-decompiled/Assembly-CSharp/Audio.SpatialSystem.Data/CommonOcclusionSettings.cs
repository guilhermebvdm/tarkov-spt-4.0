using System;
using System.Collections.Generic;
using Audio.Data;
using UnityEngine;

namespace Audio.SpatialSystem.Data;

[Serializable]
public class CommonOcclusionSettings
{
	[Tooltip("Layer mask used for occlusion raycasts")]
	public LayerMask occlusionLayerMask = 1;

	[Tooltip("How much position change triggers a recalculation start")]
	public QualityFloatValue[] positionChangeThreshold = new QualityFloatValue[3]
	{
		new QualityFloatValue(EAudioQuality.High, 0.25f),
		new QualityFloatValue(EAudioQuality.Medium, 0.35f),
		new QualityFloatValue(EAudioQuality.Low, 0.4f)
	};

	[Tooltip("How fast the effect value adapts to changes. Higher value = faster.")]
	public float smoothingFactor = 20f;

	[Tooltip("Minimum change in occlusion effect to trigger per-source recalculation")]
	public float effectChangeThreshold = 0.05f;

	[Tooltip("Maximum distance at which occlusion is calculated")]
	public float maxDistance = 250f;

	public float floorHeight = 2.7f;

	public float playerObstructionYOffset = 0.5f;

	[Tooltip("Threshold above which transmission score triggers reflection stage (0 to 1)")]
	[Range(0f, 1f)]
	public float transmissionThreshold = 0.55f;

	[Tooltip("Threshold above which diffraction score triggers reflection stage (0 to 1)")]
	[Range(0f, 1f)]
	public float diffractionThreshold = 0.5f;

	[NonSerialized]
	public Dictionary<EAudioQuality, float> PositionChangeThresholdMap = new Dictionary<EAudioQuality, float>();

	public float GetPositionThresholdByQuality()
	{
		if (PositionChangeThresholdMap.Count == 0 || PositionChangeThresholdMap.Count != positionChangeThreshold.Length)
		{
			PositionChangeThresholdMap.Clear();
			QualityFloatValue[] array = positionChangeThreshold;
			for (int i = 0; i < array.Length; i++)
			{
				QualityFloatValue qualityFloatValue = array[i];
				PositionChangeThresholdMap[qualityFloatValue.audioQuality] = qualityFloatValue.value;
			}
		}
		return PositionChangeThresholdMap.GetValueOrDefault(GClass2313.GetQualityByLogicCores(), 0.4f);
	}

	public void Apply(CommonOcclusionSettings newSettings)
	{
		positionChangeThreshold = newSettings.positionChangeThreshold;
		smoothingFactor = newSettings.smoothingFactor;
		effectChangeThreshold = newSettings.effectChangeThreshold;
		maxDistance = newSettings.maxDistance;
		floorHeight = newSettings.floorHeight;
		playerObstructionYOffset = newSettings.playerObstructionYOffset;
		transmissionThreshold = newSettings.transmissionThreshold;
		diffractionThreshold = newSettings.diffractionThreshold;
	}
}
