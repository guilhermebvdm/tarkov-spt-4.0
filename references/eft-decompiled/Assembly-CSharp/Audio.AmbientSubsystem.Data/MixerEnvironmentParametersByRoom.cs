using System;
using Audio.SpatialSystem;

namespace Audio.AmbientSubsystem.Data;

[Serializable]
public class MixerEnvironmentParametersByRoom : SerializableEnumDictionary<EAudioRoomTypeMask, float>
{
}
