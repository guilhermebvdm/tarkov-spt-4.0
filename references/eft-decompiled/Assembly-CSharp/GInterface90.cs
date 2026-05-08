using System;
using Audio.ConfiguredAudioPlayer;

public interface GInterface90
{
	void Play(AudioClipConfig config);

	void Stop(bool allowFadeout = false, Action onStopped = null);

	void Release(bool allowFadeout = false, Action onReleased = null);
}
