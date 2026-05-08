using System.Collections.Generic;
using Newtonsoft.Json;

public class BackendConfigClass
{
	[JsonProperty("config")]
	public BackendConfigSettingsClass Config;

	public Dictionary<string, GClass1405> ItemPresets;

	public Dictionary<string, int> LocationInfection;

	[JsonProperty("bot_presets")]
	public BotPresetClass[] BotPresets;

	[JsonProperty("BotWeaponScatterings")]
	public GClass612[] BotWeaponScatterings;
}
