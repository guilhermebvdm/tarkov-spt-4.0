namespace Audio.ConfiguredAudioPlayer;

public enum EPlayerState : byte
{
	Uninitialized,
	Idle,
	Playing,
	Fading,
	Transitioning
}
