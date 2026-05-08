using System.Collections.Generic;
using AnimationSystem.RootMotionTable;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace FastAnimatorSystem;

public class PlayableAnimator : MonoBehaviour, GInterface123
{
	public Animator outputAnimator;

	public InitialLayerInfo[] initialLayerInfo;

	private bool bool_0;

	private PlayableGraph playableGraph_0;

	private List<GClass1338> list_0;

	private List<AnimationClip>[] list_1;

	private AnimationPlayableOutput animationPlayableOutput_0;

	private AnimationLayerMixerPlayable animationLayerMixerPlayable_0;

	private IAnimator ianimator_0;

	private GInterface124 ginterface124_0;

	private bool bool_1;

	public bool Initialized => bool_0;

	public bool ManualUpdateMode => bool_1;

	public IAnimator FastAnimator => ianimator_0;

	public PlayableGraph Graph => playableGraph_0;

	public int LayersCount => ianimator_0.layerCount;

	public void Init(IAnimator animator, GInterface134 parametersCache, RootMotionBlendTable rootMotionBlendTable, CharacterClipsKeeper clipsKeeper, bool manualUpdate)
	{
		bool_0 = true;
		ianimator_0 = animator;
		playableGraph_0 = PlayableGraph.Create();
		animationPlayableOutput_0 = AnimationPlayableOutput.Create(playableGraph_0, "Animation", GetComponent<Animator>());
		list_1 = new List<AnimationClip>[LayersCount];
		list_0 = new List<GClass1338>();
		bool_1 = manualUpdate;
		playableGraph_0.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
		for (int i = 0; i < ianimator_0.layerCount; i++)
		{
			list_1[i] = new List<AnimationClip>(clipsKeeper.Clips[i].clips.Length);
			for (int j = 0; j < clipsKeeper.Clips[i].clips.Length; j++)
			{
				list_1[i].Add(clipsKeeper.Clips[i].clips[j].clip);
			}
		}
		for (int k = 0; k < ianimator_0.layerCount; k++)
		{
			list_0.Add(new GClass1338(k, this, parametersCache, rootMotionBlendTable, clipsKeeper, list_1[k]));
		}
		method_0();
	}

	public void Play()
	{
		if (!bool_1)
		{
			playableGraph_0.Play();
		}
	}

	public void Stop()
	{
		if (!bool_1)
		{
			playableGraph_0.Stop();
		}
	}

	public void Process(bool isVisible, float dt)
	{
		if (!ginterface124_0.ProcessAnimator(isVisible))
		{
			return;
		}
		for (int i = 0; i < list_0.Count; i++)
		{
			if (animationLayerMixerPlayable_0.GetInputWeight(i) > Mathf.Epsilon)
			{
				list_0[i].Process(dt);
			}
		}
		if (bool_1)
		{
			playableGraph_0.Evaluate(dt);
		}
		if (ginterface124_0.IsImportantParameterChanged && !isVisible)
		{
			if (!bool_1)
			{
				playableGraph_0.Evaluate(dt);
			}
			ginterface124_0.IsImportantParameterChanged = false;
		}
	}

	public void SetLayerWeight(int layerIndex, float weight)
	{
		animationLayerMixerPlayable_0.SetInputWeight(layerIndex, weight);
	}

	public float GetLayerWeight(int layerIndex)
	{
		if (layerIndex >= 0 && layerIndex < list_0.Count)
		{
			return animationLayerMixerPlayable_0.GetInputWeight(layerIndex);
		}
		return -1f;
	}

	public void RaiseImmediateTransitionHappened(int layerIndex)
	{
		list_0[layerIndex].RaiseImmediateTransitionHappened();
	}

	public void SetCuller(GInterface124 culler)
	{
		ginterface124_0 = culler;
	}

	public GInterface125 GetLayerProcessor(int layerIndex)
	{
		if (layerIndex >= 0 && layerIndex < list_0.Count)
		{
			return list_0[layerIndex];
		}
		return null;
	}

	public AnimationClip GetClipByIndex(int layerIndex, int clipIndex)
	{
		if (layerIndex >= 0 && layerIndex < list_1.Length && clipIndex >= 0 && clipIndex < list_1[layerIndex].Count)
		{
			return list_1[layerIndex][clipIndex];
		}
		return null;
	}

	public void method_0()
	{
		animationLayerMixerPlayable_0 = AnimationLayerMixerPlayable.Create(playableGraph_0, list_0.Count);
		for (uint num = 0u; num < initialLayerInfo.Length; num++)
		{
			if (initialLayerInfo[num].avatarMask != null)
			{
				animationLayerMixerPlayable_0.SetLayerMaskFromAvatarMask(num, initialLayerInfo[num].avatarMask);
			}
			animationLayerMixerPlayable_0.SetLayerAdditive(num, initialLayerInfo[num].isAdditive);
		}
		for (int i = 0; i < list_0.Count; i++)
		{
			playableGraph_0.Connect(list_0[i].Mixer, 0, animationLayerMixerPlayable_0, list_0[i].LayerIndex);
			float weight = ((i < initialLayerInfo.Length) ? initialLayerInfo[i].weight : 0f);
			animationLayerMixerPlayable_0.SetInputWeight(i, weight);
		}
		animationPlayableOutput_0.SetSourcePlayable(animationLayerMixerPlayable_0);
	}

	public void OnAnimatorMove()
	{
	}

	public void OnDestroy()
	{
		ginterface124_0 = null;
		if (bool_0)
		{
			playableGraph_0.Destroy();
		}
	}
}
