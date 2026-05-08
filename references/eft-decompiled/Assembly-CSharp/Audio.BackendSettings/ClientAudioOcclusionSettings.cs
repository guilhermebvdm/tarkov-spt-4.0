using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Audio.BackendSettings;

[Serializable]
public class ClientAudioOcclusionSettings
{
	public LocationOcclusionBackendSettings locationOcclusionSettings = new LocationOcclusionBackendSettings();

	public AudioGroupOcclusionBackendSettings[] audioGroupOcclusionSettings = Array.Empty<AudioGroupOcclusionBackendSettings>();

	[NonSerialized]
	public Dictionary<BetterAudio.AudioSourceGroupType, AudioGroupOcclusionBackendSettings> AudioGroupOcclusionSettingsMap = new Dictionary<BetterAudio.AudioSourceGroupType, AudioGroupOcclusionBackendSettings>();

	public bool TryGetSettingsForGroup(BetterAudio.AudioSourceGroupType groupType, out string jsonSettings)
	{
		jsonSettings = string.Empty;
		if (AudioGroupOcclusionSettingsMap.Count == 0 || AudioGroupOcclusionSettingsMap.Count != audioGroupOcclusionSettings.Length)
		{
			AudioGroupOcclusionSettingsMap.Clear();
			AudioGroupOcclusionBackendSettings[] array = audioGroupOcclusionSettings;
			foreach (AudioGroupOcclusionBackendSettings audioGroupOcclusionBackendSettings in array)
			{
				AudioGroupOcclusionSettingsMap[audioGroupOcclusionBackendSettings.groupType] = audioGroupOcclusionBackendSettings;
			}
		}
		if (!AudioGroupOcclusionSettingsMap.TryGetValue(groupType, out var value))
		{
			return false;
		}
		jsonSettings = value.occlusionSettings?.ToString(Formatting.None);
		if (string.IsNullOrEmpty(jsonSettings))
		{
			return false;
		}
		return true;
	}
}
