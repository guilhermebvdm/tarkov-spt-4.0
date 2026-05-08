using System;
using System.Collections.Generic;
using AnimationSystem.RootMotionTable;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

public class GClass1338 : GInterface125
{
	[NonSerialized]
	public int Int_0;

	[NonSerialized]
	public GInterface123 Ginterface123_0;

	[NonSerialized]
	public IAnimator Ianimator_0;

	[NonSerialized]
	public AnimationMixerPlayable AnimationMixerPlayable_0;

	[NonSerialized]
	public LootStateClass LootStateClass;

	[NonSerialized]
	public GClass1337 Gclass1337_0;

	[NonSerialized]
	public List<GStruct141> List_0 = new List<GStruct141>(32);

	[NonSerialized]
	public List<GStruct141> List_1 = new List<GStruct141>(32);

	[NonSerialized]
	public List<AnimationClipPlayable> List_2;

	[NonSerialized]
	public GInterface134 Ginterface134_0;

	[NonSerialized]
	public RootMotionBlendTable RootMotionBlendTable_0;

	[NonSerialized]
	public CharacterClipsKeeper CharacterClipsKeeper_0;

	[NonSerialized]
	public List<int> List_3 = new List<int>(30);

	[NonSerialized]
	public List<int> List_4 = new List<int>(30);

	[NonSerialized]
	public List<float> List_5 = new List<float>(30);

	[NonSerialized]
	public List<int> List_6 = new List<int>(15);

	[NonSerialized]
	public bool Bool_0;

	[NonSerialized]
	public List<GStruct141> List_7;

	[NonSerialized]
	public int Int_1;

	[NonSerialized]
	public int Int_2;

	[NonSerialized]
	public bool Bool_1;

	public int NodesCount => List_2.Count;

	public int LayerIndex => Int_0;

	public AnimationMixerPlayable Mixer => AnimationMixerPlayable_0;

	public GClass1338(int layerIndex, GInterface123 animator, GInterface134 parametersCache, RootMotionBlendTable rootMotionBlendTable, CharacterClipsKeeper clipsKeeper, List<AnimationClip> allClips)
	{
		Int_0 = layerIndex;
		Ginterface123_0 = animator;
		Ianimator_0 = Ginterface123_0.FastAnimator;
		Ginterface134_0 = parametersCache;
		RootMotionBlendTable_0 = rootMotionBlendTable;
		CharacterClipsKeeper_0 = clipsKeeper;
		List_2 = new List<AnimationClipPlayable>(clipsKeeper.Clips[layerIndex].clips.Length);
		LootStateClass = new LootStateClass(animator, this, List_2);
		Gclass1337_0 = new GClass1337(this, List_2, allClips);
		AnimationMixerPlayable_0 = method_3(allClips);
	}

	public void Process(float dt)
	{
		AnimatorStateInfoWrapper currentAnimatorStateInfo = Ianimator_0.GetCurrentAnimatorStateInfo(Int_0);
		RootMotionBlendTable_0._loadedNodes.TryGetValue(currentAnimatorStateInfo.fullPathHash, out var value);
		if (value == null)
		{
			return;
		}
		List_0.Clear();
		List_1.Clear();
		List_6.Clear();
		value.ComputeActiveClips(List_0, Ginterface134_0);
		bool flag = Ianimator_0.IsInTransition(Int_0);
		bool isTransitionWasInterrupted = false;
		float startTranNormalizedOffset = 0f;
		float nextStateSpeed = 1f;
		if (flag)
		{
			float transitionNormalizedTime = Ianimator_0.GetTransitionNormalizedTime(Int_0);
			AnimatorStateInfoWrapper nextAnimatorStateInfo = Ianimator_0.GetNextAnimatorStateInfo(Int_0);
			RootMotionBlendTable_0._loadedNodes.TryGetValue(nextAnimatorStateInfo.fullPathHash, out var value2);
			startTranNormalizedOffset = Ianimator_0.GetCurrentTransitionStartOffset(Int_0);
			nextStateSpeed = nextAnimatorStateInfo.speed;
			if (Int_2 != -1 && (Int_1 != currentAnimatorStateInfo.fullPathHash || Int_2 != nextAnimatorStateInfo.fullPathHash))
			{
				isTransitionWasInterrupted = true;
			}
			if (value2 != null)
			{
				value2.ComputeActiveClips(List_1, Ginterface134_0);
				bool flag2 = false;
				for (int i = 0; i < List_0.Count; i++)
				{
					if (List_0[i].ClipIndex == List_1[0].ClipIndex)
					{
						flag2 = true;
						break;
					}
				}
				if (!flag2)
				{
					int count = List_0.Count;
					List_0.AddRange(List_1);
					for (int j = 0; j < List_0.Count; j++)
					{
						GStruct141 value3 = List_0[j];
						value3.Weight *= ((j < count) ? (1f - transitionNormalizedTime) : transitionNormalizedTime);
						List_0[j] = value3;
						if (j >= count)
						{
							List_6.Add(value3.ClipIndex);
						}
					}
				}
			}
			Int_2 = nextAnimatorStateInfo.fullPathHash;
		}
		else
		{
			Int_2 = -1;
		}
		if (Bool_1)
		{
			isTransitionWasInterrupted = true;
			Bool_1 = false;
		}
		method_0();
		Gclass1337_0.BlendClips(flag, isTransitionWasInterrupted, startTranNormalizedOffset, List_6, List_0, currentAnimatorStateInfo.speed, nextStateSpeed);
		method_1();
		LootStateClass.Process(List_0);
		Int_1 = currentAnimatorStateInfo.fullPathHash;
		if (Bool_0)
		{
			List_7.Clear();
			List_7.AddRange(List_0);
		}
	}

	public void EnableProfiling(bool val)
	{
		Bool_0 = val;
		if (Bool_0)
		{
			List_7 = new List<GStruct141>();
		}
	}

	public List<GStruct141> GetLastProcessedActiveNodes()
	{
		return List_7;
	}

	public bool IsNextStateClip(int clipIndex)
	{
		return List_6.IndexOf(clipIndex) != -1;
	}

	public float GetNodeTime(int clipIndex)
	{
		return (float)List_2[clipIndex].GetTime();
	}

	public float GetNodePreviousTime(int clipIndex)
	{
		return (float)List_2[clipIndex].GetPreviousTime();
	}

	public void RaiseImmediateTransitionHappened()
	{
		Bool_1 = true;
	}

	public void method_0()
	{
		for (int i = 0; i < List_0.Count; i++)
		{
			GStruct141 value = List_0[i];
			int clipIndex = value.ClipIndex;
			float weight = value.Weight;
			for (int num = List_0.Count - 1; num >= 0; num--)
			{
				if (num != i && List_0[num].ClipIndex == clipIndex)
				{
					value.Weight = weight + List_0[num].Weight;
					List_0[i] = value;
					List_0.RemoveAt(num);
				}
			}
		}
	}

	public void method_1()
	{
		List_3.Clear();
		List_5.Clear();
		CharacterClipsKeeper.LayerData layerData = CharacterClipsKeeper_0.Clips[Int_0];
		for (int i = 0; i < List_0.Count; i++)
		{
			GStruct141 gStruct = List_0[i];
			int clipIndex = gStruct.ClipIndex;
			float weight = gStruct.Weight;
			RootMotionBlendTable.ParameterRelatedCurve[] parameterRelatedCurves = layerData.clips[clipIndex].parameterRelatedCurves;
			float time = (float)List_2[clipIndex].GetTime();
			for (int j = 0; j < parameterRelatedCurves.Length; j++)
			{
				RootMotionBlendTable.ParameterRelatedCurve parameterRelatedCurve = parameterRelatedCurves[j];
				method_2(parameterRelatedCurve.parameterHash, parameterRelatedCurve.curve.Evaluate(time) * weight);
			}
		}
		for (int k = 0; k < List_4.Count; k++)
		{
			int num = List_4[k];
			if (List_3.IndexOf(num) == -1)
			{
				Ianimator_0.SetFloat(num, 0f);
			}
		}
		List_4.Clear();
		for (int l = 0; l < List_3.Count; l++)
		{
			int num2 = List_3[l];
			Ianimator_0.SetFloat(num2, List_5[l]);
			List_4.Add(num2);
		}
	}

	public void method_2(int parameterHash, float val)
	{
		int num = 0;
		while (true)
		{
			if (num < List_3.Count)
			{
				if (List_3[num] == parameterHash)
				{
					break;
				}
				num++;
				continue;
			}
			List_3.Add(parameterHash);
			List_5.Add(val);
			return;
		}
		List_5[num] += val;
	}

	public AnimationMixerPlayable method_3(List<AnimationClip> clips)
	{
		AnimationMixerPlayable result = AnimationMixerPlayable.Create(Ginterface123_0.Graph, clips.Count);
		for (int i = 0; i < clips.Count; i++)
		{
			AnimationClipPlayable animationClipPlayable = AnimationClipPlayable.Create(Ginterface123_0.Graph, clips[i]);
			List_2.Add(animationClipPlayable);
			animationClipPlayable.SetSpeed(1.0);
		}
		return result;
	}
}
