using System;

namespace Audio.SpatialSystem.Utils;

[Serializable]
public class SpatialAudioPoolsConfig
{
	public int parallelPropagationCalculatorsPreCacheCount = 30;

	public int combinedCalculatorsPreCacheCount = 30;

	public int transmissionCalculatorsPoolCount = 30;

	public int pendingCompleteCalculatorsCount = 5;
}
