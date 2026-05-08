using System;
using System.Collections;
using Audio.AmbientSubsystem.Data;
using Comfort.Common;
using EFT;
using UnityEngine;
using UnityEngine.Audio;

namespace Audio.AmbientSubsystem;

public class DayTimeAmbientBlender : MonoBehaviour, GInterface101, IDisposable, GInterface98, GInterface103
{
	[SerializeField]
	private AudioSource _outdoorAmbientDaySource;

	[SerializeField]
	private AudioSource _outdoorAmbientNightSource;

	[SerializeField]
	private Vector2 _dayTimeRange = new Vector2(5.15f, 21.15f);

	[SerializeField]
	private float _dayToNightBlendDuration = 1.3f;

	[SerializeField]
	private float _nightToDayBlendDuration = 0.45f;

	private SeasonAmbientSoundDataSO seasonAmbientSoundDataSO_0;

	private AudioMixer audioMixer_0;

	private EDayTime edayTime_0;

	private Coroutine coroutine_0;

	private float float_0;

	private Action action_0;

	public float Single_0
	{
		get
		{
			if (EDayTime_0 != EDayTime.Day)
			{
				return _dayTimeRange.y;
			}
			return _dayTimeRange.x;
		}
	}

	public float Single_1 => method_6();

	public EDayTime EDayTime_0
	{
		get
		{
			if (!method_5(Single_1))
			{
				return EDayTime.Night;
			}
			return EDayTime.Day;
		}
	}

	public float Single_2
	{
		get
		{
			if (EDayTime_0 != EDayTime.Day)
			{
				return _dayToNightBlendDuration;
			}
			return _nightToDayBlendDuration;
		}
	}

	public void Init()
	{
		seasonAmbientSoundDataSO_0 = MonoBehaviourSingleton<AmbientAudioSystem>.Instance.AmbientSoundData;
		edayTime_0 = EDayTime_0;
		audioMixer_0 = MonoBehaviourSingleton<BetterAudio>.Instance.Master;
		EDayTimeBlendState state = ((EDayTime_0 == EDayTime.Day) ? EDayTimeBlendState.DayBlendEnd : EDayTimeBlendState.NightBlendEnd);
		method_0();
		GlobalEventHandlerClass.CreateEvent<GClass3571>().Invoke(state);
	}

	public void method_0()
	{
		float blendValue = ((EDayTime_0 == EDayTime.Day) ? 0f : 1f);
		method_4(blendValue);
	}

	public void ManualUpdate(float dt = 0f)
	{
		if (EDayTime_0 != edayTime_0)
		{
			method_1();
		}
	}

	public void method_1()
	{
		Debug.Log($"[SOUND : START DAYTIME BLEND, CURRENT STATE: {EDayTime_0}, CURRENT HOUR: {Single_1}]");
		method_7(ref coroutine_0);
		edayTime_0 = EDayTime_0;
		if (Mathf.Abs(Single_1 - Single_0) > Single_2)
		{
			EDayTimeBlendState state = ((EDayTime_0 == EDayTime.Day) ? EDayTimeBlendState.DayBlendEnd : EDayTimeBlendState.NightBlendEnd);
			method_0();
			GlobalEventHandlerClass.CreateEvent<GClass3571>().Invoke(state);
		}
		else
		{
			coroutine_0 = StartCoroutine(method_2());
		}
	}

	public IEnumerator method_2()
	{
		float_0 = Single_0 + Single_2;
		EDayTimeBlendState state = ((EDayTime_0 == EDayTime.Day) ? EDayTimeBlendState.DayBlendStart : EDayTimeBlendState.NightBlendStart);
		GlobalEventHandlerClass.CreateEvent<GClass3571>().Invoke(state);
		while (float_0 - Single_1 > 0f)
		{
			float num = method_3();
			num = ((EDayTime_0 == EDayTime.Day) ? (1f - num) : num);
			method_4(num);
			yield return null;
		}
		state = ((EDayTime_0 == EDayTime.Day) ? EDayTimeBlendState.DayBlendEnd : EDayTimeBlendState.NightBlendEnd);
		GlobalEventHandlerClass.CreateEvent<GClass3571>().Invoke(state);
		method_0();
		Debug.Log($"[SOUND : STOP DAYTIME BLEND, CURRENT STATE: {EDayTime_0}, CURRENT HOUR: {Single_1}]");
	}

	public float method_3()
	{
		return Mathf.InverseLerp(Single_0, float_0, Single_1);
	}

	public void method_4(float blendValue)
	{
		if (GClass3670.TryGetData<GClass1174>(out var dataContainer))
		{
			float value = GClass2313.ConvertNormalizedVolumeToDB(1f - blendValue);
			float value2 = GClass2313.ConvertNormalizedVolumeToDB(blendValue);
			audioMixer_0.SetFloat(dataContainer.AmbientOutDayEffectsVolume, value);
			audioMixer_0.SetFloat(dataContainer.AmbientOutNightEffectsVolume, value2);
			audioMixer_0.SetFloat(dataContainer.AmbientOutDayMixerVolume, value);
			audioMixer_0.SetFloat(dataContainer.AmbientOutNightMixerVolume, value2);
			audioMixer_0.SetFloat(dataContainer.AmbientInDayMixerVolume, value);
			audioMixer_0.SetFloat(dataContainer.AmbientInNightMixerVolume, value2);
		}
	}

	public bool method_5(float value)
	{
		if (value >= Mathf.Min(_dayTimeRange.x, _dayTimeRange.y))
		{
			return value <= Mathf.Max(_dayTimeRange.x, _dayTimeRange.y);
		}
		return false;
	}

	public float method_6()
	{
		GameWorld instance = Singleton<GameWorld>.Instance;
		if (!(instance == null) && instance.GameDateTime != null)
		{
			return (float)instance.GameDateTime.DateTime_1.TimeOfDay.TotalHours;
		}
		return 12f;
	}

	public void OnDestroy()
	{
		Dispose();
	}

	public void method_7(ref Coroutine coroutine)
	{
		if (coroutine != null)
		{
			StopCoroutine(coroutine);
			coroutine = null;
		}
	}

	public void Dispose()
	{
		method_7(ref coroutine_0);
		action_0?.Invoke();
		action_0 = null;
	}

	public void SetSeasonStatus(ESeasonStatus targetSeasonStatus)
	{
		if (seasonAmbientSoundDataSO_0.TryGetDayTimeSoundContainer(targetSeasonStatus, out var dayTimeAmbientSoundContainer))
		{
			_outdoorAmbientDaySource.clip = dayTimeAmbientSoundContainer.DayAmbientClip;
			_outdoorAmbientNightSource.clip = dayTimeAmbientSoundContainer.NightAmbientClip;
			_outdoorAmbientDaySource.Play();
			_outdoorAmbientNightSource.Play();
		}
		else
		{
			Debug.Log("[SOUND : can't handle ambient sounds on GO: " + base.gameObject.name + "]");
		}
	}

	public void ChangeSoundContainer(DayTimeAmbientSoundContainer soundContainer)
	{
		method_8(_outdoorAmbientDaySource, soundContainer.DayAmbientClip);
		method_8(_outdoorAmbientNightSource, soundContainer.NightAmbientClip);
	}

	public void method_8(AudioSource source, AudioClip clip)
	{
		if (!(clip == null))
		{
			if (source.isPlaying)
			{
				source.Stop();
			}
			source.clip = clip;
		}
	}
}
