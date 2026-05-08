using System;
using UnityEngine;

namespace EFT;

[Serializable]
public class AudioSingleClipContainer : IAudioClipContainer
{
	[SerializeField]
	public AudioClip _clip;

	[SerializeField]
	[Range(0f, 1f)]
	public float _volume = 1f;

	[SerializeField]
	public int _maxDistance = 100;

	public AudioClip GetClip()
	{
		return _clip;
	}

	public float GetVolume()
	{
		return _volume;
	}

	public int GetMaxDistance()
	{
		return _maxDistance;
	}
}
