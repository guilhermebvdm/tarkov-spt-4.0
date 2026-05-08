using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace EFT;

[Serializable]
public class SupplyData : IBasePriceSource
{
	[JsonProperty("supplyNextTime")]
	public int SupplyNextTime;

	[JsonProperty("prices")]
	public Dictionary<string, double> MarketPrices;

	[JsonProperty("currencyCourses")]
	public Dictionary<string, double> CurrencyCourses;

	public double GetBasePrice(MongoID itemId)
	{
		if (!MarketPrices.TryGetValue(itemId, out var value))
		{
			Debug.Log($"Not find item for id {itemId} name: {GClass2348.LocalizedName(itemId)} with price: {value}");
		}
		return value;
	}
}
