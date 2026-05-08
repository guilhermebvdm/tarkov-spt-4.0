using EFT;
using Newtonsoft.Json;

[GAttribute29]
public class GClass2227
{
	[JsonProperty("id")]
	public MongoID Id;

	[JsonProperty("value")]
	public int Value;

	[JsonProperty("sourceId")]
	public string SourceId;

	[JsonProperty("type")]
	public string Type;

	public GClass2227()
	{
	}

	public GClass2227(TaskConditionCounterClass counter)
	{
		Id = counter.Id;
		Value = counter.Value;
		SourceId = counter.SourceId;
		Type = counter.Type;
	}
}
