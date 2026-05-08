using EFT;
using Newtonsoft.Json;

[GAttribute29]
public class GClass2217
{
	[JsonProperty("Amount")]
	public float Amount;

	[JsonProperty("Type")]
	public EDamageType Type;

	[JsonProperty("SourceId")]
	public string SourceId;

	[JsonProperty("OverDamageFrom")]
	public EBodyPart? OverDamageFrom;

	[JsonProperty("Blunt")]
	public bool Blunt;

	[JsonProperty("ImpactsCount")]
	public float ImpactsCount;

	public GClass2217()
	{
	}

	public GClass2217(GClass2200 damageStats)
	{
		Amount = damageStats.Amount;
		Type = damageStats.Type;
		SourceId = damageStats.SourceId;
		OverDamageFrom = damageStats.OverDamageFrom;
		Blunt = damageStats.Blunt;
		ImpactsCount = damageStats.ImpactsCount;
	}
}
