using CommonAssets.Scripts.Audio.RadioSystem;

public interface GInterface148
{
	ERadioStation Station { get; }

	BroadcastItemData CurrentBroadcastItem { get; }

	float CurrentBroadcastTime { get; }

	void StartBroadcast();

	void Pause();

	void StopBroadcast();
}
