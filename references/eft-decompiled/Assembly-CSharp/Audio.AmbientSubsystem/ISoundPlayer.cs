using System;
using UnityEngine;

namespace Audio.AmbientSubsystem;

public interface ISoundPlayer
{
	Vector3 Position { get; set; }

	bool IsPlaying { get; }

	event Action Stopped;

	event Action Played;

	event Action StartPlay;

	void Play();

	void Stop(bool force = false);

	void FadeIn(float fadeInSec);

	void FadeOut(float fadeOutSec);
}
