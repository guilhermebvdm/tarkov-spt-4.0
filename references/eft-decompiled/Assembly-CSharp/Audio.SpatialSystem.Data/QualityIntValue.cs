using System;
using Audio.Data;

namespace Audio.SpatialSystem.Data;

[Serializable]
public struct QualityIntValue(EAudioQuality audioQuality, int value)
{
	public EAudioQuality audioQuality = audioQuality;

	public int value = value;
}
