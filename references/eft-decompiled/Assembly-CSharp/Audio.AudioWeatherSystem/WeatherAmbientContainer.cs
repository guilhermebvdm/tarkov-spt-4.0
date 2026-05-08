using System;
using Audio.AmbientSubsystem;
using UnityEngine;

namespace Audio.AudioWeatherSystem;

[Serializable]
public class WeatherAmbientContainer
{
	[SerializeField]
	public DayTimeAmbientSeasonClips _soundsByWeather = new DayTimeAmbientSeasonClips();

	public bool TryGetDayTimeSoundContainer(ESeasonStatus status, out DayTimeAmbientSoundContainer soundContainer)
	{
		return _soundsByWeather.TryGetValue(status, out soundContainer);
	}
}
