using Newtonsoft.Json;

public struct GStruct175
{
	[JsonProperty("$oid")]
	public string Id;

	public static implicit operator string(GStruct175 oid)
	{
		return oid.ToString();
	}

	public override string ToString()
	{
		return Id;
	}
}
