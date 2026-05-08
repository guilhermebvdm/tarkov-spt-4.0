using System;
using UnityEngine;

namespace AnimationSystem.RootMotionTable;

public class CharacterClipsKeeper : ScriptableObject
{
	[Serializable]
	public struct LayerData
	{
		public AnimationClipData[] clips;
	}

	[Serializable]
	public struct AnimationClipData
	{
		public AnimationClip clip;

		public RootMotionBlendTable.ParameterRelatedCurve[] parameterRelatedCurves;
	}

	public LayerData[] Clips;

	public int GetClipIndex(int layerIndex, AnimationClip clip)
	{
		AnimationClipData[] clips = Clips[layerIndex].clips;
		int num = 0;
		while (true)
		{
			if (num < clips.Length)
			{
				if (clips[num].clip.Equals(clip))
				{
					break;
				}
				num++;
				continue;
			}
			return -1;
		}
		return num;
	}
}
