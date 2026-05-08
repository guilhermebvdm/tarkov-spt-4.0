using EFT;
using Newtonsoft.Json;

public class GClass2204
{
	[JsonProperty("active")]
	public bool Active;

	[JsonProperty("type")]
	public int Type;

	[JsonProperty("level")]
	public int Level;

	[JsonProperty("completeTime")]
	public int? CompleteTime;

	[JsonProperty("constructing")]
	public bool Constructing;

	[JsonProperty("passiveBonusesEnabled")]
	public bool PassiveBonusesEnabled;

	[JsonProperty("lastRecipe")]
	public string LastRecipe;

	[JsonProperty("slots")]
	public GClass2426[] Slots;

	[JsonIgnore]
	public EAreaType AreaType => (EAreaType)Type;
}
