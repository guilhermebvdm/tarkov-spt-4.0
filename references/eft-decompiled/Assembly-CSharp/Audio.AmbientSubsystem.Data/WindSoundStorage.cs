using System;
using EFT.Weather;
using UnityEngine;

namespace Audio.AmbientSubsystem.Data;

[Serializable]
public class WindSoundStorage : ISeasonSoundStorage<EWindSpeed>
{
	[SerializeField]
	public SeasonWind _seasonPrecipitations = new SeasonWind();

	public bool TryGetClip(ESeasonStatus seasonStatus, EWindSpeed windSpeed, EnvironmentType environment, out AudioClip clip)
	{
		if (_seasonPrecipitations.TryGetValue(seasonStatus, out var value) && value.TryGetValue(windSpeed, out var value2))
		{
			clip = value2.GetClip(environment);
			return true;
		}
		clip = null;
		return false;
	}
}
