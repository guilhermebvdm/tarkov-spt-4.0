using Newtonsoft.Json;

public class WeatherRequestClass
{
	[JsonProperty("season")]
	public ESeason Season;

	public SeasonsSettingsClass SeasonsSettings;

	[JsonProperty("weather")]
	public WeatherClass[] Weathers;
}
