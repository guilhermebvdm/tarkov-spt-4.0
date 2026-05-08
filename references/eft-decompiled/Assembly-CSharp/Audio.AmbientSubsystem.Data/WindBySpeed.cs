using System;
using EFT.Weather;

namespace Audio.AmbientSubsystem.Data;

[Serializable]
public class WindBySpeed : SerializableEnumDictionary<EWindSpeed, EnvironmentSoundContainer>
{
}
