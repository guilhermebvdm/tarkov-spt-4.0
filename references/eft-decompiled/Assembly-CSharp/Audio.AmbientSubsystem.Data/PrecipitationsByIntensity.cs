using System;

namespace Audio.AmbientSubsystem.Data;

[Serializable]
public class PrecipitationsByIntensity : SerializableEnumDictionary<RainController.ERainIntensity, EnvironmentSoundContainer>
{
}
