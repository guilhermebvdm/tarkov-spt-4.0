using System;
using EFT.UI;
using UnityEngine;

public class ItemSounds : ScriptableObject
{
	[Serializable]
	public class ClipsByEInventorySoundType
	{
		public AudioClip[] Clips;
	}

	public ClipsByEInventorySoundType[] Clips;

	public AudioClip[] LootingClips;

	public AudioClip GetClip(string partName, EInventorySoundType soundType)
	{
		AudioClip[] clips = Clips[(int)soundType].Clips;
		string text = partName + "_" + soundType;
		int num = 0;
		while (true)
		{
			if (num < clips.Length)
			{
				if (clips[num].name == text)
				{
					break;
				}
				num++;
				continue;
			}
			return null;
		}
		return clips[num];
	}
}
