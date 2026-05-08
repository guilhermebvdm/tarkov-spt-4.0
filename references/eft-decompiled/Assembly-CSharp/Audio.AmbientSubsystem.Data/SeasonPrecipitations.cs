using System;

namespace Audio.AmbientSubsystem.Data;

[Serializable]
public class SeasonPrecipitations : SerializableEnumDictionary<ESeasonStatus, PrecipitationsByIntensity>
{
}
