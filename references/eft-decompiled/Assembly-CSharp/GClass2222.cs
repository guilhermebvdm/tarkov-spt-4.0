using EFT;
using Newtonsoft.Json;

[GAttribute29]
public class GClass2222
{
	[JsonProperty("banType")]
	public EBanType Type;

	[JsonProperty("dateTime")]
	public long ExpirationTime;

	public GClass2222()
	{
	}

	public GClass2222(VOIPBanDataClass ban)
	{
		Type = ban.Type;
		ExpirationTime = ban.ExpirationTime;
	}
}
