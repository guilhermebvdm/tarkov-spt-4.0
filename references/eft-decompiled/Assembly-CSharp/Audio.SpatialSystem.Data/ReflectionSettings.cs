using System;
using System.Collections.Generic;
using Audio.Data;
using UnityEngine;

namespace Audio.SpatialSystem.Data;

[Serializable]
public class ReflectionSettings
{
	[Tooltip("Number of initial rays cast for reflection sampling")]
	public QualityIntValue[] initialRaysCount = new QualityIntValue[3]
	{
		new QualityIntValue(EAudioQuality.High, 24),
		new QualityIntValue(EAudioQuality.Medium, 20),
		new QualityIntValue(EAudioQuality.Low, 16)
	};

	[Tooltip("Maximum number of reflections per ray")]
	public int maxReflections = 3;

	[Tooltip("Energy loss factor applied per reflection")]
	public float energyLossFactorPerReflection = 0.7f;

	[Tooltip("Minimum energy at maximum distance for reflections")]
	public float minEnergyAtMaxDistance = 0.1f;

	[NonSerialized]
	public Dictionary<EAudioQuality, int> InitialRaysCountMap = new Dictionary<EAudioQuality, int>();

	public int GetRaysCountByQuality()
	{
		if (InitialRaysCountMap.Count == 0 || InitialRaysCountMap.Count != initialRaysCount.Length)
		{
			InitialRaysCountMap.Clear();
			QualityIntValue[] array = initialRaysCount;
			for (int i = 0; i < array.Length; i++)
			{
				QualityIntValue qualityIntValue = array[i];
				InitialRaysCountMap[qualityIntValue.audioQuality] = qualityIntValue.value;
			}
		}
		return InitialRaysCountMap.GetValueOrDefault(GClass2313.GetQualityByLogicCores(), 16);
	}

	public void Apply(ReflectionSettings newSettings)
	{
		initialRaysCount = newSettings.initialRaysCount;
		maxReflections = newSettings.maxReflections;
		energyLossFactorPerReflection = newSettings.energyLossFactorPerReflection;
		minEnergyAtMaxDistance = newSettings.minEnergyAtMaxDistance;
	}
}
