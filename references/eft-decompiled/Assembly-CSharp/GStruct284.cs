using Newtonsoft.Json;

public readonly struct GStruct284(string text, double startTime, double endTime)
{
	[JsonProperty("t")]
	public readonly string Text = text;

	[JsonProperty("s")]
	public readonly double StartTime = startTime;

	[JsonProperty("e")]
	public readonly double EndTime = endTime;
}
