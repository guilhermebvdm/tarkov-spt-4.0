using Newtonsoft.Json;

public class GClass1400 : WeatherRequestClass
{
	[JsonProperty("location")]
	public LocationSettingsClass.Location Location;
}
