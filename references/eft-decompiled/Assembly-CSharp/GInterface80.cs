using CommonAssets.Scripts.Audio.RadioSystem;
using UnityEngine;

public interface GInterface80
{
	ERadioStation RadioStation { get; }

	void Initialize();

	void ChangeStation(ERadioStation station);

	void SyncBroadcast(AudioClip clip, double time = 0.0, float crossfadeDuration = 1f);

	void Stop();

	void Mute(bool value);
}
