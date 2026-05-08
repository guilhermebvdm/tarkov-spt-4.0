using System;
using UnityEngine;

public abstract class GClass988
{
	[NonSerialized]
	public static AudioSource AudioSource_0;

	static GClass988()
	{
		AudioSource_0 = new GameObject("Audio2DPlayer").AddComponent<AudioSource>();
	}

	public static void Play(AudioClip clip)
	{
		AudioSource_0.PlayOneShot(clip);
	}
}
