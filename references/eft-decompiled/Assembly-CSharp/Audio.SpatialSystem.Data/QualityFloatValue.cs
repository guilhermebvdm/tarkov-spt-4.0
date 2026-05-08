using System;
using Audio.Data;

namespace Audio.SpatialSystem.Data;

[Serializable]
public struct QualityFloatValue(EAudioQuality audioQuality, float value)
{
	public EAudioQuality audioQuality = audioQuality;

	public float value = value;
}
