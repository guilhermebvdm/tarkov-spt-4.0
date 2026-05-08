using System;
using System.Collections;
using System.Collections.Generic;
using EFT.Weather;
using UnityEngine;

namespace Audio.AmbientSubsystem;

[Serializable]
public class WeatherSoundDataContainer<T> : IEnumerable where T : UnityEngine.Object
{
	[SerializeField]
	[HideInInspector]
	public List<WeatherSoundData<T>> _commonWeatherSoundData = new List<WeatherSoundData<T>>();

	public int Count => _commonWeatherSoundData.Count;

	public WeatherSoundData<T> this[int i] => _commonWeatherSoundData[i];

	public bool TryGetCurrentWeatherData(EWindSpeed windSpeed, RainController.ERainIntensity rainIntensity, out WeatherSoundData<T> weatherData)
	{
		weatherData = null;
		if (_commonWeatherSoundData == null)
		{
			return false;
		}
		foreach (WeatherSoundData<T> commonWeatherSoundDatum in _commonWeatherSoundData)
		{
			if ((commonWeatherSoundDatum.includeWindSpeed || commonWeatherSoundDatum.includeRainIntensity) && ((!commonWeatherSoundDatum.includeWindSpeed || commonWeatherSoundDatum.windSpeed == windSpeed) & (!commonWeatherSoundDatum.includeRainIntensity || commonWeatherSoundDatum.rainIntensity == rainIntensity)))
			{
				weatherData = commonWeatherSoundDatum;
				return true;
			}
		}
		return false;
	}

	public void Add(WeatherSoundData<T> weatherData)
	{
		_commonWeatherSoundData.Add(weatherData);
	}

	public void Remove(WeatherSoundData<T> weatherData)
	{
		_commonWeatherSoundData.Remove(weatherData);
	}

	public void RemoveAt(int index)
	{
		if (Count > index && index >= 0)
		{
			_commonWeatherSoundData.RemoveAt(index);
		}
	}

	public bool CheckDuplicates()
	{
		for (int num = _commonWeatherSoundData.Count - 1; num >= 0; num--)
		{
			WeatherSoundData<T> weatherSoundData = _commonWeatherSoundData[num];
			for (int num2 = _commonWeatherSoundData.Count - 1; num2 >= 0; num2--)
			{
				if (num != num2)
				{
					WeatherSoundData<T> weatherSoundData2 = _commonWeatherSoundData[num2];
					if (weatherSoundData.Equals(weatherSoundData2))
					{
						Debug.LogError($"Duplicate weather sound data: {weatherSoundData2.windSpeed}, {weatherSoundData2.rainIntensity}");
						return true;
					}
				}
			}
		}
		return false;
	}

	public IEnumerator GetEnumerator()
	{
		return _commonWeatherSoundData.GetEnumerator();
	}

	public IEnumerator GetEnumerator_1()
	{
		return GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		//ILSpy generated this explicit interface implementation from .override directive in GetEnumerator_1
		return this.GetEnumerator_1();
	}
}
