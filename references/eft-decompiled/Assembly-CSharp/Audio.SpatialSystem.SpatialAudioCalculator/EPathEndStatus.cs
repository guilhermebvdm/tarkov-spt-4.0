namespace Audio.SpatialSystem.SpatialAudioCalculator;

public enum EPathEndStatus : byte
{
	InProgress,
	ReachedListenerClear,
	ReachedListenerBlocked,
	HitObstacle,
	MaxReflectionsOrDistance
}
