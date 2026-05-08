using Newtonsoft.Json;

public class GClass1414
{
	[JsonProperty("players")]
	public GroupPlayerDataClass[] Players;

	[JsonProperty("maxPveCountExceeded")]
	public bool LimitedServersAvailability;
}
