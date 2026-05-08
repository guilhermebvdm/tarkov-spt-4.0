using System;
using Audio.Data;

public interface GInterface81 : IDisposable
{
	bool IsPlaying { get; }

	void Play(SoundBankWithSettings bank, bool stereo = false);

	void ManualUpdate(float dt = 0f);

	void Stop();
}
