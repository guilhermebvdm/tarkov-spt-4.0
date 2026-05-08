using System;

namespace Audio.AmbientSubsystem;

[Serializable]
public class DayTimeAmbientSeasonClips : SerializableEnumDictionary<ESeasonStatus, DayTimeAmbientSoundContainer>
{
}
