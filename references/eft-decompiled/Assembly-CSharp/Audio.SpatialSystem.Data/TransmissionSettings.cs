using System;
using System.Collections.Generic;
using Audio.Data;
using UnityEngine;

namespace Audio.SpatialSystem.Data;

[Serializable]
public class TransmissionSettings
{
	[Tooltip("Number of additional rays used to check transmission around the center ray")]
	public QualityIntValue[] initialRaysCount = new QualityIntValue[3]
	{
		new QualityIntValue(EAudioQuality.High, 12),
		new QualityIntValue(EAudioQuality.Medium, 10),
		new QualityIntValue(EAudioQuality.Low, 8)
	};

	[Tooltip("Energy loss factor per unit of thickness for transmission")]
	public float absorptionPerUnit = 1.5f;

	[Tooltip("Radius of the circle on which widening transmission rays are cast")]
	public float raysWideningRadius = 0.15f;

	[Tooltip("Vertical offset for second sampling level above listener and source")]
	public float listenerHeightSamplingOffset = 0.2f;

	[Tooltip("Vertical offset for second sampling level above listener and source")]
	public float sourceHeightSamplingOffset = 0.4f;

	[Tooltip("Minimum thickness of an obstacle to start absorbing transmitted sound energy")]
	public float obstacleMinThickness = 0.02f;

	[Tooltip("Maximum thickness of an obstacle before it's considered fully blocking transmission")]
	public float obstacleMaxThickness = 5f;

	[Tooltip("Minimum energy threshold for transmitted sound")]
	public float minEnergyThreshold = 0.1f;

	[Tooltip("Use Raycast instead of Spherecast for transmission")]
	public bool useRaycast;

	public float minClearPathScore = 0.5f;

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
		return InitialRaysCountMap.GetValueOrDefault(GClass2313.GetQualityByLogicCores(), 8);
	}

	public void Apply(TransmissionSettings newSettings)
	{
		absorptionPerUnit = newSettings.absorptionPerUnit;
		initialRaysCount = newSettings.initialRaysCount;
		raysWideningRadius = newSettings.raysWideningRadius;
		listenerHeightSamplingOffset = newSettings.listenerHeightSamplingOffset;
		sourceHeightSamplingOffset = newSettings.sourceHeightSamplingOffset;
		obstacleMinThickness = newSettings.obstacleMinThickness;
		obstacleMaxThickness = newSettings.obstacleMaxThickness;
		minEnergyThreshold = newSettings.minEnergyThreshold;
		useRaycast = newSettings.useRaycast;
		minClearPathScore = newSettings.minClearPathScore;
	}
}
