using Newtonsoft.Json;

public class GClass2187
{
	[JsonProperty("maxPveCountExceeded")]
	public bool limitedServersAvailability;

	[JsonProperty("profiles")]
	public ProfileStatusClass[] profiles;
}
