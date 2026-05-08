using System;
using EFT;
using Newtonsoft.Json;

public class GClass2203
{
	[JsonProperty("fuelCounter")]
	public float FuelCounter;

	[JsonProperty("airFilterCounter")]
	public float AirFilterCounter;

	[JsonProperty("waterFilterCounter")]
	public float WaterFilterCounter;

	[JsonProperty("craftingTimeCounter")]
	public float CraftingTimeCounter;

	public float GetCounter(EAreaType areaType)
	{
		return areaType switch
		{
			EAreaType.AirFilteringUnit => AirFilterCounter, 
			EAreaType.WaterCollector => WaterFilterCounter, 
			EAreaType.Generator => FuelCounter, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}
}
