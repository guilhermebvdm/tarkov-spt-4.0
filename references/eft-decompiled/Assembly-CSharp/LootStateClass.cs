using System;
using System.Collections.Generic;
using UnityEngine.Animations;
using UnityEngine.Playables;

public class LootStateClass
{
	[NonSerialized]
	public GInterface123 Ginterface123_0;

	[NonSerialized]
	public GInterface125 Ginterface125_0;

	[NonSerialized]
	public List<AnimationClipPlayable> List_0;

	[NonSerialized]
	public List<int> List_1 = new List<int>(64);

	public LootStateClass(GInterface123 animator, GInterface125 layerProcessor, List<AnimationClipPlayable> playables)
	{
		Ginterface123_0 = animator;
		Ginterface125_0 = layerProcessor;
		List_0 = playables;
	}

	public void Process(List<GStruct141> activeNodes)
	{
		if (activeNodes.Count == List_1.Count)
		{
			bool flag = true;
			for (int i = 0; i < activeNodes.Count; i++)
			{
				if (activeNodes[i].ClipIndex != List_1[i])
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				for (int j = 0; j < activeNodes.Count; j++)
				{
					Ginterface125_0.Mixer.SetInputWeight(j, activeNodes[j].Weight);
				}
				return;
			}
		}
		for (int k = 0; k < List_1.Count; k++)
		{
			Ginterface123_0.Graph.Disconnect(Ginterface125_0.Mixer, k);
		}
		List_1.Clear();
		for (int l = 0; l < activeNodes.Count; l++)
		{
			Ginterface123_0.Graph.Connect(List_0[activeNodes[l].ClipIndex], 0, Ginterface125_0.Mixer, l);
			Ginterface125_0.Mixer.SetInputWeight(l, activeNodes[l].Weight);
			List_1.Add(activeNodes[l].ClipIndex);
		}
	}
}
