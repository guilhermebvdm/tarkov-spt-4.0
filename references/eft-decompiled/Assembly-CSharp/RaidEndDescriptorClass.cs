using EFT;
using Newtonsoft.Json;

public class RaidEndDescriptorClass
{
	[JsonProperty("profile")]
	public GClass846 profile;

	[JsonProperty("result")]
	public ExitStatus result;

	[JsonProperty("killerId")]
	public string killerId;

	[JsonProperty("killerAid")]
	public string killerAid;

	[JsonProperty("exitName")]
	public string exitName;

	[JsonProperty("inSession")]
	public bool inSession;

	[JsonProperty("favorite")]
	public bool favorite;

	[JsonProperty("playTime")]
	public int playTime;

	[JsonIgnore]
	public InsuredItemClass[] InsuredItems;

	[JsonIgnore]
	public string ProfileId;
}
