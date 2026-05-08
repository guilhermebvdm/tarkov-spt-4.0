using System;
using Newtonsoft.Json;

[Serializable]
public class SessionModeResponse
{
	[JsonProperty("gameMode")]
	public ESessionMode SessionMode;

	[JsonProperty("backendUrl")]
	public string BackendUrl;
}
