using System;
using UnityEngine;

namespace Audio.AmbientSubsystem.Data;

[Serializable]
public class PrecipitationSoundStorage : ISeasonSoundStorage<RainController.ERainIntensity>
{
	[SerializeField]
	public SeasonPrecipitations _seasonPrecipitations = new SeasonPrecipitations();

	public bool TryGetClip(ESeasonStatus seasonStatus, RainController.ERainIntensity intensity, EnvironmentType environment, out AudioClip clip)
	{
		if (_seasonPrecipitations.TryGetValue(seasonStatus, out var value) && value.TryGetValue(intensity, out var value2))
		{
			clip = value2.GetClip(environment);
			return true;
		}
		clip = null;
		return false;
	}
}
