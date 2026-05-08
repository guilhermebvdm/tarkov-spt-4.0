using System;
using UnityEngine;

namespace Audio.AmbientSubsystem.Data;

[Serializable]
public class AudioClipByRainIntensity : SerializableEnumDictionary<RainController.ERainIntensity, AudioClip>
{
}
