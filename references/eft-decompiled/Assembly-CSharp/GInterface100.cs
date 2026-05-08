using System;
using UnityEngine;

public interface GInterface100
{
	bool IsCrossfadeStarted { get; }

	AudioSource CurrentSource { get; }

	AudioSource MixSource { get; }

	void StartCrossfade(float duration = 1f, float speed = 1f, float startVolume = 1f, float endVolume = 0f, bool startFromRndTime = true, Action callback = null);

	void Stop();
}
