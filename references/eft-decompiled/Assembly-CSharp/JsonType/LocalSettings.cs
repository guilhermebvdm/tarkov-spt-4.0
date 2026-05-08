using System;
using Newtonsoft.Json;

namespace JsonType;

[Serializable]
public class LocalSettings
{
	[JsonProperty("serverId")]
	public string serverId;

	[JsonProperty("serverSettings")]
	public GClass1440 settings;

	[JsonProperty("profile")]
	public ProfileInsuranceClass profileInsurance;

	[JsonProperty("locationLoot")]
	public LocationSettingsClass.Location locationLoot;

	[JsonProperty("excludedBosses")]
	public string[] excludedBosses;

	[JsonProperty("transition")]
	public RaidTransitionInfoClass transitionSettings;
}
