using System;
using EFT;

namespace Audio.Data;

[Serializable]
public class SurfaceSoundContainers : SerializableEnumDictionary<BaseBallistic.ESurfaceSound, AudioMultipleClipContainer>
{
}
