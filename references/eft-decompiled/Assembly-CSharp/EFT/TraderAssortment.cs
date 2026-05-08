using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace EFT;

[Serializable]
public class TraderAssortment : ExchangeRateDTO
{
	[JsonProperty("items")]
	public FlatItemsDataClass[] Items;

	[JsonProperty("barter_scheme")]
	public Dictionary<string, BarterScheme> BarterScheme;

	[JsonProperty("loyal_level_items")]
	public Dictionary<string, int> LoyaltyLevelItems;

	[JsonProperty("nextResupply")]
	public int NextResupply;
}
