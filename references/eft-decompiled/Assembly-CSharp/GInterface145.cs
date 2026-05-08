using System;
using JetBrains.Annotations;
using UnityEngine;

public interface GInterface145 : IDisposable
{
	bool Enabled { get; }

	bool IsPlaying { get; }

	bool IsLoop { get; }

	void SetPitch(int seed, float pitch);

	void Stop();

	void Play([CanBeNull] AudioClip clip, float volume, bool loop = false);

	void ContinuePlay();

	void Release();
}
