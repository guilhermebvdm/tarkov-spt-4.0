using System;
using System.Collections.Generic;
using UnityEngine;

public class EquipmentClass(BetterSource[] audioSources) : GClass888(audioSources)
{
	[NonSerialized]
	public Queue<GClass892> Queue_0 = new Queue<GClass892>();

	[NonSerialized]
	public double Double_0;

	[NonSerialized]
	public double Double_1;

	[NonSerialized]
	public int Int_0;

	public void Enqueue(AudioClip clip1, float startTime, float clipTime = 0f, float volume = 1f, float pitch = 1f)
	{
		double num = method_0(startTime);
		double end = method_1(clipTime, num);
		Queue_0.Enqueue(new GClass892(clip1, num, end)
		{
			Volume = volume,
			Pitch = pitch
		});
		if (Double_0 > num)
		{
			Double_0 = 0.0;
		}
	}

	public override void Update()
	{
		if (Double_0 > AudioSettings.dspTime || Queue_0.Count < 1)
		{
			return;
		}
		GClass892 gClass = Queue_0.Dequeue();
		if (!AudioSources[Int_0].source1.isPlaying)
		{
			Int_0 = ((Int_0 == 0) ? 1 : 0);
		}
		SimpleSource obj = (SimpleSource)AudioSources[Int_0];
		Int_0 = ((Int_0 == 0) ? 1 : 0);
		SimpleSource simpleSource = (SimpleSource)AudioSources[Int_0];
		simpleSource.Loop = gClass.End == 0.0;
		obj.SetScheduledEndTime(gClass.Start);
		gClass.PlayOn(simpleSource);
		Double_0 = gClass.Start;
		if (gClass.End == 0.0)
		{
			if (Queue_0.Count >= 1)
			{
				GClass892 gClass2 = Queue_0.Peek();
				if (gClass2 != null)
				{
					simpleSource.SetScheduledEndTime(gClass2.Start);
				}
			}
		}
		else
		{
			simpleSource.SetScheduledEndTime(gClass.End);
		}
	}
}
