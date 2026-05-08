using Newtonsoft.Json;

public class GClass1384
{
	[JsonProperty("summoned")]
	public GClass1382[] Summoned;

	[JsonProperty("leaved")]
	public string[] Leaved;

	[JsonProperty("gameDate")]
	public string Date;

	[JsonProperty("gameTime")]
	public string Time;

	[JsonProperty("gameTimeAcceleration")]
	public float Acceleration;

	[JsonProperty("halloweenEvent")]
	public GClass1387 HalloweenEvent;

	[JsonProperty("season")]
	public ESeason Season;

	[JsonProperty("event")]
	public GClass1389 AirdropEvent;

	[JsonProperty("excludedBosses")]
	public string[] excludedBosses;
}
