using System;
using System.Collections;
using System.Runtime.CompilerServices;
using EFT;
using UnityEngine;

public class GClass1186 : GInterface100
{
	[NonSerialized]
	public AudioSource AudioSource_0;

	[NonSerialized]
	public AudioSource AudioSource_1;

	[NonSerialized]
	public Coroutine Coroutine_0;

	[NonSerialized]
	[CompilerGenerated]
	public bool Bool_0;

	[NonSerialized]
	[CompilerGenerated]
	public AudioSource AudioSource_2;

	public bool IsCrossfadeStarted
	{
		[CompilerGenerated]
		get
		{
			return Bool_0;
		}
		[CompilerGenerated]
		set
		{
			Bool_0 = value;
		}
	}

	public AudioSource CurrentSource
	{
		[CompilerGenerated]
		get
		{
			return AudioSource_2;
		}
		[CompilerGenerated]
		set
		{
			AudioSource_2 = value;
		}
	}

	public AudioSource MixSource
	{
		get
		{
			if (CurrentSource.GetHashCode() != AudioSource_0.GetHashCode())
			{
				return AudioSource_0;
			}
			return AudioSource_1;
		}
	}

	public GClass1186(AudioSource sourceA, AudioSource sourceB)
	{
		AudioSource_0 = sourceA;
		AudioSource_1 = sourceB;
		CurrentSource = sourceA;
	}

	public void StartCrossfade(float duration = 1f, float speed = 1f, float startVolume = 1f, float endVolume = 0f, bool startFromRndTime = true, Action callback = null)
	{
		StaticManager instance = StaticManager.Instance;
		if (!IsCrossfadeStarted && !(instance == null))
		{
			IsCrossfadeStarted = true;
			method_1();
			Coroutine_0 = instance.StartCoroutine(method_0(duration, speed, startVolume, endVolume, startFromRndTime, callback));
		}
	}

	public IEnumerator method_0(float duration, float speed, float startVolume, float endVolume, bool startFromRndTime = true, Action callback = null)
	{
		if (MixSource.clip != null)
		{
			if (startFromRndTime)
			{
				float time = UnityEngine.Random.Range(0f, MixSource.clip.length);
				MixSource.time = time;
			}
			MixSource.Play();
		}
		float num = 0f;
		while (num < duration)
		{
			float t = num / duration;
			CurrentSource.volume = Mathf.Lerp(startVolume, 0f, t);
			MixSource.volume = Mathf.Lerp(0f, endVolume, t);
			num += Time.deltaTime * speed;
			yield return null;
		}
		CurrentSource.volume = 0f;
		MixSource.volume = endVolume;
		CurrentSource.Stop();
		CurrentSource = MixSource;
		IsCrossfadeStarted = false;
		callback?.Invoke();
	}

	public void Stop()
	{
		method_1();
		IsCrossfadeStarted = false;
	}

	public void method_1()
	{
		StaticManager instance = StaticManager.Instance;
		if (Coroutine_0 != null && instance != null)
		{
			instance.StopCoroutine(Coroutine_0);
			Coroutine_0 = null;
		}
	}
}
