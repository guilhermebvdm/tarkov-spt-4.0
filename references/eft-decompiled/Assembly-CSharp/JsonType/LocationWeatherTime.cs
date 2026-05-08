using Newtonsoft.Json;

namespace JsonType;

public class LocationWeatherTime
{
	[JsonProperty("season")]
	public ESeason Season;

	public SeasonsSettingsClass SeasonsSettings;

	[JsonProperty("weather")]
	public WeatherClass Weather;

	[JsonProperty("acceleration")]
	public float Acceleration;

	[JsonProperty("date")]
	public string Date;

	[JsonProperty("time")]
	public string Time;

	public static LocationWeatherTime CreateDefault()
	{
		return new LocationWeatherTime(WeatherClass.CreateDefault(), 0f, "", "");
	}

	public LocationWeatherTime(WeatherClass node, float acceleration, string date, string time)
	{
		Weather = node;
		Acceleration = acceleration;
		Date = date;
		Time = time;
	}
}
