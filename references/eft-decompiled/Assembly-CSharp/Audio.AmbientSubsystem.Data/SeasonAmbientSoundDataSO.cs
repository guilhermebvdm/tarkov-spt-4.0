using System;
using EFT.Weather;
using UnityEngine;

namespace Audio.AmbientSubsystem.Data;

[Serializable]
[CreateAssetMenu(menuName = "Scriptable objects/Audio/SeasonAmbientSoundDataSO", fileName = "SeasonAmbientSoundData")]
public class SeasonAmbientSoundDataSO : ScriptableObject
{
	private const ESeasonStatus DEFAULT_SEASON = ESeasonStatus.Summer;

	[SerializeField]
	private WindSoundStorage _windSoundStorage = new WindSoundStorage();

	[SerializeField]
	private DayTimeAmbientSeasonClips _dayTimeAmbientSeasonClips = new DayTimeAmbientSeasonClips();

	[SerializeField]
	private PrecipitationSoundStorageSO _precipitationSoundStorageSo;

	public void OnEnable()
	{
		method_0();
	}

	public void method_0()
	{
		_precipitationSoundStorageSo = ((_precipitationSoundStorageSo == null) ? ScriptableObject.CreateInstance<PrecipitationSoundStorageSO>() : _precipitationSoundStorageSo);
	}

	public bool TryGetDayTimeSoundContainer(ESeasonStatus seasonStatus, out DayTimeAmbientSoundContainer dayTimeAmbientSoundContainer)
	{
		dayTimeAmbientSoundContainer = null;
		if (_dayTimeAmbientSeasonClips.TryGetValue(seasonStatus, out dayTimeAmbientSoundContainer))
		{
			return true;
		}
		if (_dayTimeAmbientSeasonClips.TryGetValue(ESeasonStatus.Summer, out dayTimeAmbientSoundContainer))
		{
			Debug.Log($"Can't find proposed container for: {seasonStatus}, returned default {ESeasonStatus.Summer}");
			return true;
		}
		Debug.Log($"Can't find any proposed container for: {seasonStatus}, returned empty container!");
		dayTimeAmbientSoundContainer = new DayTimeAmbientSoundContainer
		{
			DayAmbientClip = method_1(),
			NightAmbientClip = method_1()
		};
		return false;
	}

	public bool TryGetWindClip(ESeasonStatus seasonStatus, EWindSpeed windSpeed, EnvironmentType environment, out AudioClip clip)
	{
		clip = null;
		if (_windSoundStorage.TryGetClip(seasonStatus, windSpeed, environment, out clip))
		{
			return true;
		}
		if (_windSoundStorage.TryGetClip(ESeasonStatus.Summer, windSpeed, environment, out clip))
		{
			Debug.Log($"Can't find proposed clip for: {seasonStatus} with windSpeed: {windSpeed}, returned default {ESeasonStatus.Summer}");
			return true;
		}
		Debug.Log($"Can't find any proposed clip for: {seasonStatus} with windSpeed: {windSpeed}");
		clip = method_1();
		return false;
	}

	public bool TryGetPrecipitationClip(ESeasonStatus seasonStatus, RainController.ERainIntensity intensity, EnvironmentType environment, out AudioClip clip)
	{
		clip = null;
		if (_precipitationSoundStorageSo.TryGetClip(seasonStatus, intensity, environment, out clip))
		{
			return true;
		}
		if (_precipitationSoundStorageSo.TryGetClip(ESeasonStatus.Summer, intensity, environment, out clip))
		{
			Debug.Log($"Can't find proposed clip for: {seasonStatus} with intensity: {intensity}, returned default {ESeasonStatus.Summer}");
			return true;
		}
		Debug.Log($"Can't find any proposed clip for: {seasonStatus} with intensity: {intensity}");
		clip = method_1();
		return false;
	}

	public AudioClip method_1()
	{
		return AudioClip.Create("empty_clip", 4096, 1, AudioSettings.outputSampleRate, stream: false);
	}
}
