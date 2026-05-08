using System;
using Audio.AmbientSubsystem.Data;
using Audio.SpatialSystem;
using Comfort.Common;
using EFT.Weather;
using UnityEngine;
using UnityEngine.Audio;

namespace Audio.AmbientSubsystem;

public class WindAmbientBlender : MonoBehaviour, GInterface101, IDisposable, GInterface98
{
	private const float float_0 = 0.05f;

	private const float float_1 = 0.1f;

	private const float float_2 = 1f;

	private const float float_3 = 0.2f;

	private const float float_4 = 1f;

	[SerializeField]
	private AudioSource _windSource;

	[SerializeField]
	private AudioSource _windMixSource;

	[SerializeField]
	private AudioMixerGroup _outputMixerGroup;

	[SerializeField]
	private float _defaultBlendSpeed = 0.33f;

	[SerializeField]
	private float _crossfadeDuration = 1f;

	private float float_5 = 1f;

	private SeasonAmbientSoundDataSO seasonAmbientSoundDataSO_0;

	private GInterface100 ginterface100_0;

	private EnvironmentType environmentType_0;

	private EnvironmentType environmentType_1;

	private float float_6;

	private float float_7 = -1f;

	private GClass1096.GClass1103 gclass1103_0 = new GClass1096.GClass1103();

	private Action action_0;

	public static Vector2 Vector2_0 => WeatherController.Instance?.WeatherCurve.Wind ?? Vector2.zero;

	public void Init()
	{
		seasonAmbientSoundDataSO_0 = MonoBehaviourSingleton<AmbientAudioSystem>.Instance.AmbientSoundData;
		action_0 = GlobalEventHandlerClass.Instance.SubscribeOnEvent<GClass3573>(method_2);
		_windSource.outputAudioMixerGroup = _outputMixerGroup;
		_windMixSource.outputAudioMixerGroup = _outputMixerGroup;
		ginterface100_0 = new GClass1186(_windSource, _windMixSource);
		method_0(force: true);
	}

	public void ManualUpdate(float dt)
	{
		method_0();
	}

	public void method_0(bool force = false)
	{
		float_6 = method_10();
		if (force || method_1())
		{
			Debug.Log($"[SOUND: start wind coroutine, current magnitude: {float_6}]");
			if (method_3(force))
			{
				float_7 = float_6;
			}
		}
	}

	public bool method_1()
	{
		return Mathf.Abs(float_6 - float_7) > 0.05f;
	}

	public void method_2(GClass3573 roomChangedEvent)
	{
		EAudioRoomTypeMask currentRoomType = roomChangedEvent.CurrentRoomType;
		environmentType_0 = (GClass1118.IsIndoor(currentRoomType) ? EnvironmentType.Indoor : EnvironmentType.Outdoor);
		if (environmentType_0 != environmentType_1)
		{
			environmentType_1 = environmentType_0;
		}
	}

	public bool method_3(bool force = false)
	{
		if (ginterface100_0.IsCrossfadeStarted && !force)
		{
			return false;
		}
		method_5();
		ginterface100_0.Stop();
		method_4();
		ginterface100_0.StartCrossfade(_crossfadeDuration, _defaultBlendSpeed, ginterface100_0.CurrentSource.volume, method_7());
		return true;
	}

	public void method_4()
	{
		AudioSource mixSource = ginterface100_0.MixSource;
		if (method_6(out var clip))
		{
			mixSource.clip = clip;
		}
	}

	public void method_5()
	{
		EWindSpeed windSpeed = method_8();
		float_5 = gclass1103_0.GetWindMult(windSpeed);
		GlobalEventHandlerClass.CreateEvent<GClass3569>().Invoke(windSpeed);
	}

	public bool method_6(out AudioClip clip)
	{
		return seasonAmbientSoundDataSO_0.TryGetWindClip(MonoBehaviourSingleton<AmbientAudioSystem>.Instance.CurrentSeasonStatus, method_8(), EnvironmentType.Outdoor, out clip);
	}

	public float method_7()
	{
		return Mathf.Clamp(method_9(), 0.2f, 1f) * float_5;
	}

	public EWindSpeed method_8()
	{
		EWindSpeed result = EWindSpeed.Light;
		float num = method_9();
		if (num <= 0.2f)
		{
			result = EWindSpeed.Light;
		}
		else if (num <= 0.4f)
		{
			result = EWindSpeed.Moderate;
		}
		else if (num > 0.4f)
		{
			result = EWindSpeed.Strong;
		}
		return result;
	}

	public float method_9()
	{
		return Mathf.Clamp(Vector2_0.magnitude, 0.1f, 1f);
	}

	public float method_10()
	{
		return Vector2_0.sqrMagnitude;
	}

	public void OnDestroy()
	{
		Dispose();
	}

	public void Dispose()
	{
		action_0?.Invoke();
		action_0 = null;
		ginterface100_0?.Stop();
		ginterface100_0 = null;
	}

	public void SetSeasonStatus(ESeasonStatus targetSeasonStatus)
	{
		try
		{
			gclass1103_0 = Singleton<GClass1706>.Instance.AudioSettings.EnvironmentSettings.GetSeasonSettings(targetSeasonStatus);
		}
		catch (Exception arg)
		{
			GClass722.Instance.LogError($"Can't get ClientSettingsConfig SeasonAmbientSettings, using default values: {arg}");
		}
		method_0(force: true);
	}
}
