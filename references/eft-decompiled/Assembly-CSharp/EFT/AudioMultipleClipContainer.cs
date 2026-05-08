using System;
using UnityEngine;

namespace EFT;

[Serializable]
public class AudioMultipleClipContainer : IAudioClipContainer
{
	[SerializeField]
	public AudioClip[] _clips;

	[SerializeField]
	[Range(0f, 1f)]
	public float _volume = 1f;

	[SerializeField]
	public int _maxDistance = 100;

	public AudioClip GetClip()
	{
		int num = UnityEngine.Random.Range(0, _clips.Length);
		return _clips[num];
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
