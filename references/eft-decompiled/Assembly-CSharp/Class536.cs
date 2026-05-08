using System;
using System.Collections.Generic;
using UnityEngine;

public class Class536
{
	public class Class537
	{
		public AudioClip Clip;

		public double Start;

		public double End;

		public Class537(AudioClip clip, double start, double end)
		{
			Clip = clip;
			Start = start;
			End = end;
		}
	}

	[NonSerialized]
	public AudioSource[] AudioSource_0;

	[NonSerialized]
	public int Int_0;

	[NonSerialized]
	public LinkedList<Class537> LinkedList_0 = new LinkedList<Class537>();

	[NonSerialized]
	public double Double_0;

	public int Count => LinkedList_0.Count;

	public Class536(AudioSource source0, AudioSource source1)
	{
		AudioSource_0 = new AudioSource[2] { source0, source1 };
	}

	public void AddClipFTime(AudioClip clip, float start, float end = 0f)
	{
		float time = Time.time;
		double dspTime = AudioSettings.dspTime;
		if (end == 0f)
		{
			AddClip(clip, (double)(start - time) + dspTime);
		}
		else
		{
			AddClip(clip, (double)(start - time) + dspTime, (double)(end - time) + dspTime);
		}
	}

	public void AddClip(AudioClip clip, double start, double end = 0.0)
	{
		if (end != 0.0 && end <= start)
		{
			Debug.LogError("AudioQueue: clip end time is to small, clip wasn't added to queue =(\nstart time:" + start + "  end time:" + end);
			return;
		}
		LinkedList_0.AddLast(new Class537(clip, start, end));
		if (Double_0 > start)
		{
			Double_0 = 0.0;
		}
	}

	public void Update()
	{
		double dspTime = AudioSettings.dspTime;
		if (!(Double_0 > dspTime) && LinkedList_0.First != null)
		{
			Class537 value = LinkedList_0.First.Value;
			LinkedList_0.RemoveFirst();
			if (!AudioSource_0[Int_0].isPlaying)
			{
				Int_0 = ((Int_0 == 0) ? 1 : 0);
			}
			AudioSource audioSource = AudioSource_0[Int_0];
			Int_0 = ((Int_0 == 0) ? 1 : 0);
			AudioSource audioSource2 = AudioSource_0[Int_0];
			audioSource2.loop = false;
			audioSource.SetScheduledEndTime(value.Start);
			audioSource2.clip = value.Clip;
			audioSource2.PlayScheduled(value.Start);
			if (value.End != 0.0)
			{
				audioSource2.SetScheduledEndTime(value.End);
			}
			else if (LinkedList_0.First != null)
			{
				audioSource2.SetScheduledEndTime(LinkedList_0.First.Value.Start);
			}
			Double_0 = value.Start;
		}
	}
}
