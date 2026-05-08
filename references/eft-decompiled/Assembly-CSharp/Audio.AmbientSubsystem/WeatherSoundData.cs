using System;
using EFT.Weather;
using UnityEngine;

namespace Audio.AmbientSubsystem;

[Serializable]
public class WeatherSoundData<T> : IEquatable<WeatherSoundData<T>> where T : UnityEngine.Object
{
	public bool includeWindSpeed;

	public EWindSpeed windSpeed;

	public bool includeRainIntensity = true;

	public RainController.ERainIntensity rainIntensity;

	public Vector2 randomDelayRange;

	public T soundContent;

	public bool Equals(WeatherSoundData<T> other)
	{
		if (other == null)
		{
			return false;
		}
		if (this == other)
		{
			return true;
		}
		if (windSpeed == other.windSpeed && includeWindSpeed == other.includeWindSpeed && rainIntensity == other.rainIntensity)
		{
			return includeRainIntensity == other.includeRainIntensity;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (this == obj)
		{
			return true;
		}
		if (obj.GetType() != GetType())
		{
			return false;
		}
		return Equals((WeatherSoundData<T>)obj);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine((int)windSpeed, (int)rainIntensity);
	}
}
