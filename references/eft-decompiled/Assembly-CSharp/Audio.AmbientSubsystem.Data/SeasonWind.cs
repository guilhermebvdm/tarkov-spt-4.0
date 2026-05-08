using System;

namespace Audio.AmbientSubsystem.Data;

[Serializable]
public class SeasonWind : SerializableEnumDictionary<ESeasonStatus, WindBySpeed>
{
}
