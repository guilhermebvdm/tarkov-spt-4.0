using System;
using System.Runtime.CompilerServices;
using Audio.Data;
using Comfort.Common;
using CommonAssets.Scripts.Audio;
using UnityEngine;

public abstract class GClass2313
{
	[Serializable]
	[CompilerGenerated]
	public class Class1531
	{
		public static readonly Class1531 class1531_0 = new Class1531();

		public static Func<float, float> func_0;

		public float method_0(float t)
		{
			return t;
		}
	}

	public const float SOUND_SPEED_MS = 343.2f;

	public const float MIN_DB = -80f;

	public const float MAX_DB = 0f;

	public const float DECIBEL_SCALE_FACTOR = 20f;

	public static float ConvertNormalizedVolumeToDB(float volumeNormalized)
	{
		return Mathf.Log10(Mathf.Clamp(volumeNormalized, 0.0001f, 1f)) * 20f;
	}

	public static float ConvertNormalizedValueToCutoffFrequency(float valueNormalized)
	{
		return valueNormalized * ((float)AudioSettings.outputSampleRate / 2f);
	}

	public static float ConvertPercentageToDB(int percentage, float maxVolumeDb = 0f)
	{
		percentage = Mathf.Clamp(percentage, 0, 100);
		return maxVolumeDb + 20f * Mathf.Log10((float)percentage / 100f);
	}

	public static float CalculateHeightPercentageRelativeToBounds(Vector3 point, in Bounds bounds)
	{
		float y = bounds.size.y;
		return Mathf.Clamp(point.y - bounds.min.y, 0f, y) / y * 100f;
	}

	public static float CalculateSoundDelay(float distance)
	{
		return distance / 343.2f;
	}

	public static bool IsInRange(Vector3 position, float maxDistance)
	{
		return GetSqrDistanceToListener(position) < maxDistance * maxDistance;
	}

	public static bool IsInRange(float sqrDistanceToListener, float maxDistance)
	{
		return sqrDistanceToListener < maxDistance * maxDistance;
	}

	public static float GetSqrDistanceToListener(Vector3 position)
	{
		if (!CameraClass.Exist)
		{
			return float.MaxValue;
		}
		return CameraClass.Instance.SqrDistance(position);
	}

	public static float CalculatePhysicSoundDelay(Vector3 soundPosition)
	{
		return (CameraClass.Exist ? CameraClass.Instance.Distance(soundPosition) : 0f) / 343.2f;
	}

	public static float CalculatePhysicSoundDelay(Vector3 listenerPosition, Vector3 soundPosition)
	{
		return Vector3.Distance(listenerPosition, soundPosition) / 343.2f;
	}

	public static float CalculateVolumeInDB(float baseDb, float multiplier)
	{
		float f = Mathf.Pow(10f, baseDb / 20f) * multiplier;
		return 20f * Mathf.Log10(f);
	}

	public static AudioClip CreateEmptyMonoClip()
	{
		return AudioClip.Create("empty_clip", 4096, 1, AudioSettings.outputSampleRate, stream: false);
	}

	public static void ResetAudioBuffer()
	{
		AudioListener.pause = true;
		AudioConfiguration configuration = AudioSettings.GetConfiguration();
		configuration.dspBufferSize = 0;
		GClass722 instance = GClass722.Instance;
		if (!AudioSettings.Reset(configuration))
		{
			instance?.LogError("Audio buffer not reset");
		}
		if (Singleton<SharedGameSettingsClass>.Instantiated)
		{
			Singleton<SharedGameSettingsClass>.Instance.Sound.Settings.ForceApplyVolumeSettings();
		}
		AudioSettings.GetDSPBufferSize(out var bufferLength, out var numBuffers);
		instance?.LogInfo($"Current DSP buffer length: {bufferLength}, buffers num: {numBuffers}");
		AudioListener.pause = false;
	}

	public static bool CheckClipping(float[] samples)
	{
		bool result = false;
		for (int i = 0; i < samples.Length; i++)
		{
			if (!(Mathf.Abs(samples[i]) <= 1f))
			{
				result = true;
				break;
			}
		}
		return result;
	}

	public static float GetOutputVolume(float[] samples)
	{
		if (samples.Length == 0)
		{
			return 0f;
		}
		float num = 0f;
		foreach (float num2 in samples)
		{
			num += num2 * num2;
		}
		float num3 = Mathf.Sqrt(num / (float)samples.Length);
		if (num3 <= 0f)
		{
			return -80f;
		}
		float value = ConvertRmsToDecibels(num3, -80f, 1f);
		value = Mathf.Clamp(value, -80f, 0f);
		if (float.IsNaN(value))
		{
			return 0f;
		}
		return (float)Math.Round(value, 2);
	}

	public static float ConvertRmsToDecibels(float rms, float minDb, float refValue)
	{
		float num = rms / refValue;
		if (num <= 0f)
		{
			return 0f;
		}
		return Mathf.Max(minDb, 20f * Mathf.Log10(num));
	}

	public static Func<float, float> GetEasingFunction(EAudioFadeType fadeType)
	{
		return fadeType switch
		{
			EAudioFadeType.Linear => (float t) => t, 
			EAudioFadeType.EaseIn => EaseInInterpolation, 
			EAudioFadeType.EaseOut => EaseOutInterpolation, 
			EAudioFadeType.EaseInOut => EaseInOutInterpolation, 
			EAudioFadeType.Exponential => ExponentialInterpolation, 
			EAudioFadeType.InverseExponential => InverseExponentialInterpolation, 
			EAudioFadeType.Logarithmic => LogarithmicInterpolation, 
			EAudioFadeType.SmoothStep => SmoothStepInterpolation, 
			EAudioFadeType.Logistic => LogisticInterpolation, 
			_ => throw new ArgumentOutOfRangeException("fadeType", fadeType, null), 
		};
	}

	public static float EaseInInterpolation(float t)
	{
		return t * t;
	}

	public static float EaseOutInterpolation(float t)
	{
		return 1f - (1f - t) * (1f - t);
	}

	public static float EaseInOutInterpolation(float t)
	{
		if (!(t < 0.5f))
		{
			return 1f - Mathf.Pow(-2f * t + 2f, 2f) / 2f;
		}
		return 2f * t * t;
	}

	public static float ExponentialInterpolation(float t)
	{
		return Mathf.Pow(2f, 10f * (t - 1f));
	}

	public static float LogarithmicInterpolation(float t)
	{
		return Mathf.Log10(t * 9f + 1f);
	}

	public static float InverseExponentialInterpolation(float t)
	{
		return 1f - Mathf.Pow(2f, -10f * t);
	}

	public static float SmoothStepInterpolation(float t)
	{
		return t * t * t * (t * (t * 6f - 15f) + 10f);
	}

	public static float LogisticInterpolation(float t)
	{
		return 1f / (1f + Mathf.Exp(-10f * (t - 0.5f)));
	}

	public static float EvaluateFade(EAudioFadeType fadeType, float t)
	{
		return fadeType switch
		{
			EAudioFadeType.Linear => t, 
			EAudioFadeType.EaseIn => EaseInInterpolation(t), 
			EAudioFadeType.EaseOut => EaseOutInterpolation(t), 
			EAudioFadeType.EaseInOut => EaseInOutInterpolation(t), 
			EAudioFadeType.Exponential => ExponentialInterpolation(t), 
			EAudioFadeType.InverseExponential => InverseExponentialInterpolation(t), 
			EAudioFadeType.Logarithmic => LogarithmicInterpolation(t), 
			EAudioFadeType.SmoothStep => SmoothStepInterpolation(t), 
			EAudioFadeType.Logistic => LogisticInterpolation(t), 
			_ => t, 
		};
	}

	public static EAudioQuality GetQualityByLogicCores()
	{
		int processorCount = SystemInfo.processorCount;
		if (processorCount < 12)
		{
			if (processorCount < 8)
			{
				return EAudioQuality.Low;
			}
			return EAudioQuality.Medium;
		}
		return EAudioQuality.High;
	}
}
