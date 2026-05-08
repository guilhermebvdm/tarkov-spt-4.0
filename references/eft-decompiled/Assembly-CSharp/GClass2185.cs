using System;
using System.Linq;
using EFT;
using Newtonsoft.Json;

[GAttribute29]
public class GClass2185
{
	[JsonProperty("QuestId")]
	public MongoID QuestId;

	[JsonProperty("ItemId")]
	public MongoID ItemId;

	[JsonProperty("ZoneId")]
	public string ZoneId;

	public GClass2185()
	{
	}

	public GClass2185(string questId, string itemId, string zoneId)
	{
		QuestId = questId;
		ItemId = itemId;
		ZoneId = zoneId;
	}

	public bool Equals(string[] itemIds, string zoneId)
	{
		if (itemIds.Contains(ItemId.ToString(), StringComparer.InvariantCultureIgnoreCase))
		{
			return ZoneId == zoneId;
		}
		return false;
	}
}
