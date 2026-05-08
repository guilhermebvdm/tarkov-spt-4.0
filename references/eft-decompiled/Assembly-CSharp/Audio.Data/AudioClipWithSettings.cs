using System;
using UnityEngine;

namespace Audio.Data;

[Serializable]
public class AudioClipWithSettings
{
	public AudioClip clip;

	public int maxDistance = 500;

	[Range(0f, 1f)]
	public float volume = 1f;
}
