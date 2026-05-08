using System;
using UnityEngine;

namespace Audio.Effects;

[Serializable]
public class AudioOcclusionEQPreset
{
	[Serializable]
	public class EnvironmentAudioThresholds
	{
		public float indoorIsolated = 0.015f;

		public float diffRoomsTypeIsolated = 0.015f;

		public float diffEnvironmentIsolated;

		public float indoorToOutdoor = 0.05f;

		public float outdoorToIndoor = 0.05f;

		public float baseValue = 0.1f;
	}

	[Serializable]
	public class PositionEQThresholds
	{
		[Range(20f, 20000f)]
		public float levelFreq = 20f;

		[Range(20f, 20000f)]
		public float aboveFreq = 20f;

		[Range(20f, 20000f)]
		public float belowFreq = 20f;

		[Range(20f, 20000f)]
		public float behindFreq = 20f;
	}

	[Serializable]
	public class AudioFilterSettings
	{
		public AnimationCurve frequencyCurve = AnimationCurve.Linear(0f, 80f, 1f, 200f);

		public AnimationCurve resonanceCurve = new AnimationCurve();

		public AnimationCurve distanceCurve = new AnimationCurve();

		public AnimationCurve heightCurve = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 1.5f), new Keyframe(2f, 2f), new Keyframe(3f, 2.5f));

		public PositionEQThresholds positionEqThresholds = new PositionEQThresholds();

		public EnvironmentAudioThresholds environmentEqThresholds = new EnvironmentAudioThresholds();
	}

	public AnimationCurve volumeCurve = new AnimationCurve();

	public EnvironmentAudioThresholds environmentVolumeThresholds = new EnvironmentAudioThresholds();

	public AnimationCurve heightVolumeCurve = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 1f), new Keyframe(2f, 0.75f), new Keyframe(3f, 0.5f), new Keyframe(4f, 0.25f), new Keyframe(5f, 0f));

	public AudioFilterSettings hpfSettings = new AudioFilterSettings();

	public AudioFilterSettings lpfSettings = new AudioFilterSettings();

	[Range(0f, 0.5f)]
	public float distanceCoefficient = 0.5f;

	[Range(0f, 0.2f)]
	public float rotationCoefficient = 0.2f;
}
