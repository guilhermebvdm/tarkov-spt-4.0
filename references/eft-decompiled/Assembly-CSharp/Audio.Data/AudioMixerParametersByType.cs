using System;
using Audio.AmbientSubsystem.Data;

namespace Audio.Data;

[Serializable]
public class AudioMixerParametersByType : SerializableEnumDictionary<EMixerParameterType, AudioMixerParamsContainer>
{
}
