using System;
using System.Runtime.CompilerServices;
using Audio.SpatialSystem;
using EFT;
using EFT.Weather;
using UnityEngine;

namespace Audio.AmbientSubsystem;

public class WeatherRandomAmbientSoundPlayer : BaseRandomAmbientSoundPlayer, GInterface98, GInterface99, GInterface97
{
	[Serializable]
	public class SeasonWeatherSoundData : SerializableEnumDictionary<ESeasonStatus, WeatherSoundDataContainer<SoundBank>>
	{
	}

	[HideInInspector]
	[Space]
	public bool enabledSeasons;

	[SerializeField]
	private Vector2 _fadeOutSecRangeOnPrecipitationStop = new Vector2(1f, 5f);

	[SerializeField]
	[HideInInspector]
	private SeasonWeatherSoundData _seasonWeatherSoundData = new SeasonWeatherSoundData();

	[SerializeField]
	[HideInInspector]
	private WeatherSoundDataContainer<SoundBank> _commonWeatherSoundData = new WeatherSoundDataContainer<SoundBank>();

	private RainController.ERainIntensity erainIntensity_0;

	private EWindSpeed ewindSpeed_0 = EWindSpeed.Hurricane;

	private EAudioRoomTypeMask eaudioRoomTypeMask_0 = EAudioRoomTypeMask.Outdoor;

	private ESeasonStatus eseasonStatus_0;

	private Action action_3;

	[CompilerGenerated]
	private SoundBank soundBank_0;

	public override SoundBank CurrentBank
	{
		[CompilerGenerated]
		get
		{
			return soundBank_0;
		}
		[CompilerGenerated]
		set
		{
			soundBank_0 = value;
		}
	}

	public EnvironmentType EnvironmentType_0
	{
		get
		{
			if (!GClass1118.IsIndoor(eaudioRoomTypeMask_0))
			{
				return EnvironmentType.Outdoor;
			}
			return EnvironmentType.Indoor;
		}
	}

	public override void Awake()
	{
		base.Awake();
		GlobalEventHandlerClass instance = GlobalEventHandlerClass.Instance;
		action_3 = instance.SubscribeOnEvent<GClass3573>(method_7);
	}

	public void ApplyPrecipitationsIntensity(RainController.ERainIntensity targetIntensity)
	{
		if (erainIntensity_0 != targetIntensity)
		{
			erainIntensity_0 = targetIntensity;
			bool flag = method_5();
			if (erainIntensity_0 == RainController.ERainIntensity.None)
			{
				UpdateFadeOutTime(TransformHelperClass.Random(_fadeOutSecRangeOnPrecipitationStop));
				Stop();
			}
			else if (!base.IsPlaying && flag)
			{
				Play();
			}
		}
	}

	public void ApplyWindSpeed(EWindSpeed targetWindSpeed)
	{
		if (ewindSpeed_0 != targetWindSpeed)
		{
			ewindSpeed_0 = targetWindSpeed;
			method_5();
		}
	}

	public void SetSeasonStatus(ESeasonStatus targetSeasonStatus)
	{
		if (eseasonStatus_0 != targetSeasonStatus)
		{
			eseasonStatus_0 = targetSeasonStatus;
			method_5();
		}
	}

	public bool method_5()
	{
		if (method_6(out var weatherData))
		{
			ChangeSoundBank(weatherData.soundContent);
			return true;
		}
		return false;
	}

	public override void ChangeSoundBank(SoundBank newSoundBank)
	{
		Stop(force: true);
		CurrentBank = newSoundBank;
		Play();
	}

	public override AudioClip GetClip()
	{
		if (CurrentBank == null)
		{
			return base.EmptyClip;
		}
		return CurrentBank.PickSingleClip((int)((CurrentBank.Environments.Length > 1) ? EnvironmentType_0 : EnvironmentType.Outdoor));
	}

	public override float GetRandomDelay()
	{
		if (method_6(out var weatherData))
		{
			return TransformHelperClass.Random(weatherData.randomDelayRange);
		}
		return base.GetRandomDelay();
	}

	public bool method_6(out WeatherSoundData<SoundBank> weatherData)
	{
		weatherData = null;
		WeatherSoundDataContainer<SoundBank> value = null;
		if (enabledSeasons)
		{
			_seasonWeatherSoundData.TryGetValue(eseasonStatus_0, out value);
		}
		else
		{
			value = _commonWeatherSoundData;
		}
		return value?.TryGetCurrentWeatherData(ewindSpeed_0, erainIntensity_0, out weatherData) ?? false;
	}

	public void method_7(GClass3573 roomChangedEvent)
	{
		if (!GClass867.Is(eaudioRoomTypeMask_0, roomChangedEvent.CurrentRoomType))
		{
			eaudioRoomTypeMask_0 = roomChangedEvent.CurrentRoomType;
		}
	}

	public override void OnDestroy()
	{
		action_3?.Invoke();
		action_3 = null;
		base.OnDestroy();
	}
}
