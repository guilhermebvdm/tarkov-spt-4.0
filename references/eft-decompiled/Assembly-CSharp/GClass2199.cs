using EFT;
using Newtonsoft.Json;

[GAttribute29]
public class GClass2199
{
	[JsonProperty("DamageType")]
	public EDamageType DamageType;

	[JsonProperty("Side")]
	public EPlayerSide Side;

	[JsonProperty("Role")]
	public WildSpawnType Role;

	[JsonProperty("WeaponId")]
	public string WeaponId;
}
