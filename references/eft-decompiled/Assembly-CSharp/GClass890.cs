using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class GClass890 : GClass888
{
	[NonSerialized]
	public Queue<GClass893> Queue_0 = new Queue<GClass893>();

	[NonSerialized]
	public double Double_0;

	[NonSerialized]
	public double Double_1;

	[NonSerialized]
	public int Int_0;

	public GClass890()
	{
		AudioSources = new BetterSource[2];
	}

	public void Initialize(BetterSource source1, BetterSource source2)
	{
		AudioSources[0] = source1;
		AudioSources[1] = source2;
	}

	public void SetPriority(int priority)
	{
		BetterSource[] audioSources = AudioSources;
		foreach (BetterSource betterSource in audioSources)
		{
			if (betterSource != null)
			{
				betterSource.SetPriority(priority);
			}
		}
	}

	public void SetMixerGroup(AudioMixerGroup group)
	{
		BetterSource[] audioSources = AudioSources;
		foreach (BetterSource betterSource in audioSources)
		{
			if (betterSource != null)
			{
				betterSource.SetMixerGroup(group);
			}
		}
	}

	public void SetRolloff(float distance)
	{
		BetterSource[] audioSources = AudioSources;
		foreach (BetterSource betterSource in audioSources)
		{
			if (betterSource != null)
			{
				betterSource.SetRolloff(distance);
			}
		}
	}

	public void EnableStereo(bool stereo = false)
	{
		BetterSource[] audioSources = AudioSources;
		for (int i = 0; i < audioSources.Length; i++)
		{
			audioSources[i].EnableStereo(stereo);
		}
	}

	public void Enqueue(AudioClip clip1, AudioClip clip2, float balance, double startTime, float clipTime = 0f, float volume = 1f, float pitch = 1f)
	{
		double num = method_0(startTime);
		double end = method_1(clipTime, num);
		Queue_0.Enqueue(new GClass893(clip1, clip2, balance, num, end)
		{
			Volume = volume,
			Pitch = pitch
		});
		if (Double_0 > num)
		{
			Double_0 = 0.0;
		}
	}

	public void SetPitch(float pitch)
	{
		foreach (GClass893 item in Queue_0)
		{
			item.Pitch = pitch;
		}
		BetterSource[] audioSources = AudioSources;
		foreach (BetterSource betterSource in audioSources)
		{
			if (betterSource != null)
			{
				betterSource.SetPitch(pitch);
			}
		}
	}

	public void SetSpatialBlend(float spatialBlend)
	{
		BetterSource[] audioSources = AudioSources;
		for (int i = 0; i < audioSources.Length; i++)
		{
			audioSources[i].SetSpatialBlend(spatialBlend);
		}
	}

	public override void Update()
	{
		if (Double_0 > AudioSettings.dspTime || Queue_0.Count < 1)
		{
			return;
		}
		GClass893 gClass = Queue_0.Dequeue();
		if (!AudioSources[Int_0].source1.isPlaying)
		{
			Int_0 = ((Int_0 == 0) ? 1 : 0);
		}
		SuperSource obj = (SuperSource)AudioSources[Int_0];
		Int_0 = ((Int_0 == 0) ? 1 : 0);
		SuperSource superSource = (SuperSource)AudioSources[Int_0];
		superSource.Loop = gClass.End == 0.0;
		obj.SetScheduledEndTime(gClass.Start);
		gClass.PlayOn(superSource);
		Double_0 = gClass.Start;
		if (gClass.End == 0.0)
		{
			if (Queue_0.Count >= 1)
			{
				GClass893 gClass2 = Queue_0.Peek();
				if (gClass2 != null)
				{
					superSource.SetScheduledEndTime(gClass2.Start);
				}
			}
		}
		else
		{
			superSource.SetScheduledEndTime(gClass.End);
		}
	}
}
