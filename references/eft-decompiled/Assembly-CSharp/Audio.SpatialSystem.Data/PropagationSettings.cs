using System;
using System.Collections.Generic;
using Audio.Data;
using UnityEngine;

namespace Audio.SpatialSystem.Data;

[Serializable]
public class PropagationSettings
{
	[Tooltip("percentage total routes count by quality")]
	public QualityFloatValue[] routesCompressionFactorByQuality = new QualityFloatValue[3]
	{
		new QualityFloatValue(EAudioQuality.High, 1f),
		new QualityFloatValue(EAudioQuality.Medium, 0.75f),
		new QualityFloatValue(EAudioQuality.Low, 0.65f)
	};

	[Tooltip("Weight of distance factor in propagation-based occlusion calculation")]
	public float distanceWeight = 0.7f;

	[Tooltip("Exponent applied to diffraction factor in propagation calculation")]
	public float diffractionExponent = 0.5f;

	[Tooltip("Exponent applied to height influence (normalized)")]
	public float heightExponent = 1f;

	[Tooltip("Weight of ascending segment height cost")]
	public float segmentHeightWeightUp;

	[Tooltip("Weight of descending segment height cost")]
	public float segmentHeightWeightDown;

	[Tooltip("Min cost percentage when source or listener close to portal")]
	public float minPortalCostPercent = 0.1f;

	[Tooltip("Number of portal-to-portal relaxation iterations")]
	[Obsolete]
	public int relaxationIterations = 3;

	[Tooltip("Weight of absolute vertical displacement in propagation calculation")]
	public float absoluteHeightWeight;

	[Tooltip("Typical room height (in meters) for normalization of vertical displacement")]
	public float typicalRoomHeight = 2.7f;

	[Tooltip("Max path segment length (in meters) for normalization of magnitude scaling")]
	public float maxSegmentLength = 10f;

	[NonSerialized]
	public Dictionary<EAudioQuality, float> RoutesCompressionFactorByQualityMap = new Dictionary<EAudioQuality, float>();

	public float GetCompressionFactorByQuality()
	{
		if (RoutesCompressionFactorByQualityMap.Count == 0 || RoutesCompressionFactorByQualityMap.Count != routesCompressionFactorByQuality.Length)
		{
			RoutesCompressionFactorByQualityMap.Clear();
			QualityFloatValue[] array = routesCompressionFactorByQuality;
			for (int i = 0; i < array.Length; i++)
			{
				QualityFloatValue qualityFloatValue = array[i];
				RoutesCompressionFactorByQualityMap[qualityFloatValue.audioQuality] = qualityFloatValue.value;
			}
		}
		return RoutesCompressionFactorByQualityMap.GetValueOrDefault(GClass2313.GetQualityByLogicCores(), 0.65f);
	}

	public void Apply(PropagationSettings newSettings)
	{
		routesCompressionFactorByQuality = newSettings.routesCompressionFactorByQuality;
		distanceWeight = newSettings.distanceWeight;
		diffractionExponent = newSettings.diffractionExponent;
		heightExponent = newSettings.heightExponent;
		segmentHeightWeightUp = newSettings.segmentHeightWeightUp;
		segmentHeightWeightDown = newSettings.segmentHeightWeightDown;
		minPortalCostPercent = newSettings.minPortalCostPercent;
		relaxationIterations = newSettings.relaxationIterations;
		absoluteHeightWeight = newSettings.absoluteHeightWeight;
		typicalRoomHeight = newSettings.typicalRoomHeight;
		maxSegmentLength = newSettings.maxSegmentLength;
	}
}
