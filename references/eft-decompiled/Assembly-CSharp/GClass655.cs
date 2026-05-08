using Newtonsoft.Json;

public class GClass655
{
	[JsonProperty("method")]
	public string Method;

	[JsonProperty("params")]
	public object @params;

	[JsonProperty("id")]
	public string Id;

	public override string ToString()
	{
		return $"WSRequest:\n   Method: {Method},\n   Params: {@params},\n   Id: {Id}";
	}
}
