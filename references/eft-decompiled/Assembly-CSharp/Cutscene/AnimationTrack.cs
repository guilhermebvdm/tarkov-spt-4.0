using System;
using System.Collections.Generic;
using UnityEngine;
using uLipSync;

namespace Cutscene;

[Serializable]
public class AnimationTrack
{
	public BakedData lipSyncBackedData;

	public LipSyncBackedDataRandomVariants lipSyncBackedDataRandomRange;

	[NonSerialized]
	public List<BakedData> RandomLipSyncData;

	[NonSerialized]
	public BakedData SelectedLipSyncData;

	public bool HaveData
	{
		get
		{
			if (!(lipSyncBackedData != null) && !(lipSyncBackedDataRandomRange != null))
			{
				return false;
			}
			return true;
		}
	}

	public float duration
	{
		get
		{
			if (SelectedLipSyncData != null)
			{
				return SelectedLipSyncData.duration;
			}
			if (lipSyncBackedData != null)
			{
				return lipSyncBackedData.duration;
			}
			if (lipSyncBackedDataRandomRange != null)
			{
				return lipSyncBackedDataRandomRange.BakedData[0].duration;
			}
			return 0f;
		}
	}

	public string hint
	{
		get
		{
			if (lipSyncBackedData != null)
			{
				return "lb:" + lipSyncBackedData.name;
			}
			if (lipSyncBackedDataRandomRange != null)
			{
				return "lbr:" + lipSyncBackedDataRandomRange.name;
			}
			return "(null)";
		}
	}

	public BakedData GetBackedDataForPlay()
	{
		if (lipSyncBackedData != null)
		{
			return lipSyncBackedData;
		}
		if (lipSyncBackedDataRandomRange != null)
		{
			if (RandomLipSyncData == null || RandomLipSyncData.Count == 0)
			{
				RandomLipSyncData = new List<BakedData>(lipSyncBackedDataRandomRange.BakedData);
			}
			SelectedLipSyncData = RandomLipSyncData[UnityEngine.Random.Range(0, RandomLipSyncData.Count)];
			if (RandomLipSyncData.Count == 1)
			{
				RandomLipSyncData = new List<BakedData>(lipSyncBackedDataRandomRange.BakedData);
			}
			RandomLipSyncData.Remove(SelectedLipSyncData);
			return SelectedLipSyncData;
		}
		return null;
	}
}
