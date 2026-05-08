using System;
using System.Collections.Generic;
using Audio.Data;
using UnityEngine;

namespace Audio.SpatialSystem.Data;

[Serializable]
public class DiffractionSettings
{
	[Tooltip("Number of rays used to search for an edge candidate from each side (source/listener hit point)")]
	public QualityIntValue[] edgeSearchRayCount = new QualityIntValue[3]
	{
		new QualityIntValue(EAudioQuality.High, 16),
		new QualityIntValue(EAudioQuality.Medium, 12),
		new QualityIntValue(EAudioQuality.Low, 8)
	};

	[Tooltip("Maximum distance from edge to consider for diffraction")]
	public float maxEdgeDist = 0.15f;

	[Tooltip("Maximum allowed path length multiplier for diffracted rays")]
	public float maxPathFactor = 3f;

	[Tooltip("Length of the rays used to search for edge candidates")]
	public float edgeSearchRayLength = 0.5f;

	[Tooltip("Offset from the candidate edge point when checking visibility from source/listener")]
	public float edgeValidationRayOffset = 0.01f;

	[NonSerialized]
	public Dictionary<EAudioQuality, int> EdgeSearchRayCount = new Dictionary<EAudioQuality, int>();

	public int GetRaysCountByQuality()
	{
		if (EdgeSearchRayCount.Count == 0 || EdgeSearchRayCount.Count != edgeSearchRayCount.Length)
		{
			EdgeSearchRayCount.Clear();
			QualityIntValue[] array = edgeSearchRayCount;
			for (int i = 0; i < array.Length; i++)
			{
				QualityIntValue qualityIntValue = array[i];
				EdgeSearchRayCount[qualityIntValue.audioQuality] = qualityIntValue.value;
			}
		}
		return EdgeSearchRayCount.GetValueOrDefault(GClass2313.GetQualityByLogicCores(), 8);
	}

	public void Apply(DiffractionSettings newSettings)
	{
		maxEdgeDist = newSettings.maxEdgeDist;
		maxPathFactor = newSettings.maxPathFactor;
		edgeSearchRayCount = newSettings.edgeSearchRayCount;
		edgeSearchRayLength = newSettings.edgeSearchRayLength;
		edgeValidationRayOffset = newSettings.edgeValidationRayOffset;
	}
}
