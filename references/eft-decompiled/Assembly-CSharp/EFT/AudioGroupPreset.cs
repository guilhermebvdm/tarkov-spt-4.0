using System;
using Audio;
using Audio.SpatialSystem;
using UnityEngine;

namespace EFT;

[Serializable]
[CreateAssetMenu(menuName = "Scriptable objects/AudioGroupPreset", fileName = "AudioGroupPreset")]
public class AudioGroupPreset : ScriptableObject
{
	[Serializable]
	public class AcousticSettings
	{
		public bool enabledReverb = true;

		[Range(-60f, 20f)]
		public float reverbSendDb = -3f;

		[Range(-60f, 20f)]
		public float earlyReflectionsSendDb = -3f;

		[Range(0f, 1f)]
		public float reverbReach = 0.5f;

		public void Apply(AcousticSettings settings)
		{
			enabledReverb = settings.enabledReverb;
			reverbSendDb = settings.reverbSendDb;
			earlyReflectionsSendDb = settings.earlyReflectionsSendDb;
			reverbReach = settings.reverbReach;
		}
	}

	[Serializable]
	public class AudioGroupAcousticSettings : ISettingsFromJson
	{
		public AcousticSettings mono = new AcousticSettings();

		public AcousticSettings stereo = new AcousticSettings();

		public bool enabledPrewarm;

		public void ApplyFromJson(string json)
		{
			JsonUtility.FromJsonOverwrite(json, this);
		}
	}

	public BetterAudio.AudioSourceGroupType Type;

	[Range(0f, 1f)]
	public float OverallVolume = 1f;

	[Range(0f, 1f)]
	public float SpatialBlend = 1f;

	public bool SteamSpatialize = true;

	public bool DirectBinaural = true;

	public bool DisableBinauralByDist;

	public float EnableBinauralDist;

	public float HeightDiffToAllowBinaural;

	public float AngleToAllowBinaural;

	[Range(0f, 1f)]
	public float HrtfIntensity = 1f;

	public MetaXRAudioSourceExperimentalFeatures.DirectivityPatternType DirectivityPatternType;

	[Range(0f, 1f)]
	public float DirectivityIntensity = 0.5f;

	[Range(0.1f, 10f)]
	public float OcclusionSourceRadius = 1f;

	public AnimationCurve SoundRolloff;

	public AnimationCurve SpreadCurve;

	public int AudioSourcePriority = 128;

	public int PreCachedSourcesCount = 2;

	public int SourcesCountLimit = 100;

	public float DefaultMaxDistance = 500f;

	public AudioGroupAcousticSettings acousticSettings = new AudioGroupAcousticSettings();

	public AudioGroupOcclusionSettings occlusionSettings = new AudioGroupOcclusionSettings();
}
