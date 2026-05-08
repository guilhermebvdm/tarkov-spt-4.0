using System;
using UnityEngine;

public class GClass885
{
	[NonSerialized]
	public const float Float_0 = 5f;

	[NonSerialized]
	public const string String_0 = "Audio/prewarm_sound";

	[NonSerialized]
	public const float Float_1 = 100f;

	[NonSerialized]
	public AudioClip AudioClip_0;

	[NonSerialized]
	public float Float_2;

	[NonSerialized]
	public float Float_3;

	public GClass885()
	{
		method_0();
	}

	public void ProcessPlayPrewarmSound(BetterSource source, float distanceToListener)
	{
		Float_2 += Time.deltaTime;
		if (!(Float_2 < 5f) && Mathf.Abs(distanceToListener - Float_3) >= 1f)
		{
			Float_2 = 0f;
			Float_3 = distanceToListener;
			if (source.gameObject.activeSelf && source.PlayBackState != BetterSource.EPlayBackState.Playing && GClass2313.IsInRange(source.Position, 100f))
			{
				PlayPrewarmSound(source);
			}
		}
	}

	public void PlayPrewarmSound(BetterSource source)
	{
		source.Play(AudioClip_0, null, 1f);
	}

	public void PlayPrewarmSound(AudioSource source)
	{
		source.PlayOneShot(AudioClip_0);
	}

	public void method_0()
	{
		AudioClip_0 = CacheResourcesPopAbstractClass.Pop<AudioClip>("Audio/prewarm_sound");
	}
}
