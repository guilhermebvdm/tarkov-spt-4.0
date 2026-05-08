using EFT;
using Newtonsoft.Json;

[GAttribute29]
public class GClass2186
{
	[JsonProperty("QuestId")]
	public MongoID QuestId;

	[JsonProperty("ItemId")]
	public MongoID ItemId;

	public GClass2186()
	{
	}

	public GClass2186(string questId, string itemId)
	{
		QuestId = questId;
		ItemId = itemId;
	}
}
