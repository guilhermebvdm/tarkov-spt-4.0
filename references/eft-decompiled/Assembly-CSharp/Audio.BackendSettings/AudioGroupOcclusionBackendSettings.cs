using System;
using Newtonsoft.Json.Linq;

namespace Audio.BackendSettings;

[Serializable]
public class AudioGroupOcclusionBackendSettings
{
	public BetterAudio.AudioSourceGroupType groupType = BetterAudio.AudioSourceGroupType.Environment;

	public JToken occlusionSettings;
}
