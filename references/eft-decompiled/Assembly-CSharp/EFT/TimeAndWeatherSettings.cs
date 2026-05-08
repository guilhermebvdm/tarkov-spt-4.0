using System;
using EFT.Weather;
using Newtonsoft.Json;

namespace EFT;

[Serializable]
public struct TimeAndWeatherSettings(bool randomTime, bool randomWeather, int cloudinessType, int rainType, int windSpeed, int fogType, int timeFlowType, int hourOfDay = -1)
{
	[JsonProperty("isRandomTime")]
	public bool IsRandomTime = randomTime;

	[JsonProperty("isRandomWeather")]
	public bool IsRandomWeather = randomWeather;

	[JsonProperty("cloudinessType")]
	public ECloudinessType CloudinessType = (ECloudinessType)cloudinessType;

	[JsonProperty("rainType")]
	public ERainType RainType = (ERainType)rainType;

	[JsonProperty("windType")]
	public EWindSpeed WindType = (EWindSpeed)windSpeed;

	[JsonProperty("fogType")]
	public EFogType FogType = (EFogType)fogType;

	[JsonProperty("timeFlowType")]
	public ETimeFlowType TimeFlowType = (ETimeFlowType)timeFlowType;

	[JsonProperty("hourOfDay")]
	public int HourOfDay = hourOfDay;
}
