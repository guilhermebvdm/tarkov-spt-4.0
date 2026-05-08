using Newtonsoft.Json;

public class GClass1413
{
	[JsonProperty("_id")]
	public string Id;

	[JsonProperty("requestId")]
	public string RequestId;

	[JsonProperty("from")]
	public string From;

	[JsonProperty("to")]
	public string To;

	[JsonProperty("profile")]
	public GroupPlayerDataClass FromProfile;

	[JsonProperty("members")]
	public GroupPlayerDataClass[] Members;
}
