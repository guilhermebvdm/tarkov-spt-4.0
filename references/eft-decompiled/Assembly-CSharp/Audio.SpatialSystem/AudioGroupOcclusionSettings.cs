using System;
using Audio.Effects;
using UnityEngine;

namespace Audio.SpatialSystem;

[Serializable]
public class AudioGroupOcclusionSettings : ISettingsFromJson
{
	public bool occlusionEnabled = true;

	[Range(0f, 1f)]
	public float occlusionIntensity = 1f;

	[Range(0f, 1f)]
	public float maxQualityFactor = 1f;

	public bool useQualityCompression;

	[Range(0.1f, 2f)]
	public float indoorToOutdoorFactor = 1.5f;

	[Range(0.1f, 2f)]
	public float outdoorToIndoorFactor = 1.5f;

	[Range(0.1f, 1f)]
	public float rolloffScale = 1f;

	public AudioOcclusionEQPreset obstructionEQPreset = new AudioOcclusionEQPreset();

	public AudioOcclusionEQPreset propagationEQPreset = new AudioOcclusionEQPreset();

	public AnimationCurve stairsHeightCurve = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 1f), new Keyframe(2f, 0.75f), new Keyframe(3f, 0.5f), new Keyframe(4f, 0.25f), new Keyframe(5f, 0f));

	public void ApplyFromJson(string json)
	{
		JsonUtility.FromJsonOverwrite(json, this);
	}
}
