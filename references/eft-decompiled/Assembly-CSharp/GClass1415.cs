using EFT;
using Newtonsoft.Json;

public class GClass1415<T> where T : IRaidSettings<T>
{
	[JsonProperty("squad")]
	public GroupPlayerDataClass[] Players;

	[JsonProperty("raidSettings")]
	public T RaidSettings;
}
