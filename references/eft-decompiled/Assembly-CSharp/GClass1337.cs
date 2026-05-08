using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

public class GClass1337
{
	[NonSerialized]
	public double Double_0;

	[NonSerialized]
	public double Double_1;

	[NonSerialized]
	public GInterface123 Ginterface123_0;

	[NonSerialized]
	public GInterface125 Ginterface125_0;

	[NonSerialized]
	public List<AnimationClip> List_0;

	[NonSerialized]
	public List<AnimationClipPlayable> List_1;

	[NonSerialized]
	public int Int_0 = 1;

	[NonSerialized]
	public bool Bool_0;

	public GClass1337(GInterface125 layerProcessor, List<AnimationClipPlayable> playables, List<AnimationClip> allClips)
	{
		Ginterface125_0 = layerProcessor;
		List_1 = playables;
		List_0 = allClips;
	}

	public void BlendClips(bool isInTransition, bool isTransitionWasInterrupted, float startTranNormalizedOffset, List<int> nextStateNodeIndexes, List<GStruct141> nodes, float stateSpeed, float nextStateSpeed)
	{
		AnimationMixerPlayable mixer = Ginterface125_0.Mixer;
		double time = mixer.GetTime();
		double previousTime = mixer.GetPreviousTime();
		double num = time - previousTime;
		float num2 = method_1(isInTransition, nextStateNodeIndexes, nodes) / stateSpeed;
		method_0(isInTransition, isTransitionWasInterrupted, startTranNormalizedOffset);
		if (num2 > Mathf.Epsilon)
		{
			Double_0 += num / (double)num2;
		}
		if (isInTransition)
		{
			float num3 = method_1(isInTransition, nextStateNodeIndexes, nodes, calcOnlyCurrentStateWeights: false) / nextStateSpeed;
			if (num3 > Mathf.Epsilon)
			{
				Double_1 += num / (double)num3;
			}
		}
		for (int i = 0; i < nodes.Count; i++)
		{
			int clipIndex = nodes[i].ClipIndex;
			if (isInTransition && nextStateNodeIndexes.IndexOf(clipIndex) != -1)
			{
				List_1[clipIndex].SetTime(Double_1 * (double)List_0[clipIndex].length);
			}
			else
			{
				List_1[clipIndex].SetTime(Double_0 * (double)List_0[clipIndex].length);
			}
		}
	}

	public void method_0(bool isInTransition, bool isTransitionWasInterrupted, float startTranNormalizedOffset)
	{
		if (isTransitionWasInterrupted)
		{
			Double_0 = Double_1;
			Double_1 = startTranNormalizedOffset;
		}
		else if (Bool_0 != isInTransition)
		{
			Int_0++;
			if (isInTransition)
			{
				if (Int_0 % 2 == 1)
				{
					Double_0 = startTranNormalizedOffset;
				}
				else
				{
					Double_1 = startTranNormalizedOffset;
				}
			}
			else
			{
				Double_0 = Double_1;
				Double_1 = startTranNormalizedOffset;
			}
		}
		Bool_0 = isInTransition;
	}

	public float method_1(bool isInTransition, List<int> nextStateNodeIndexes, List<GStruct141> nodes, bool calcOnlyCurrentStateWeights = true)
	{
		float num = 0f;
		int count = nodes.Count;
		for (int i = 0; i < count; i++)
		{
			GStruct141 gStruct = nodes[i];
			bool flag;
			if ((!(flag = isInTransition && nextStateNodeIndexes.IndexOf(gStruct.ClipIndex) != -1) && calcOnlyCurrentStateWeights) || (flag && !calcOnlyCurrentStateWeights))
			{
				num += gStruct.RawWeight;
			}
		}
		float num2 = 1f / num;
		float num3 = 0f;
		for (int j = 0; j < count; j++)
		{
			GStruct141 gStruct2 = nodes[j];
			bool flag2;
			if ((!(flag2 = isInTransition && nextStateNodeIndexes.IndexOf(gStruct2.ClipIndex) != -1) && calcOnlyCurrentStateWeights) || (flag2 && !calcOnlyCurrentStateWeights))
			{
				num3 += List_0[gStruct2.ClipIndex].length * gStruct2.RawWeight * num2 / gStruct2.TimeScale;
			}
		}
		return num3;
	}
}
