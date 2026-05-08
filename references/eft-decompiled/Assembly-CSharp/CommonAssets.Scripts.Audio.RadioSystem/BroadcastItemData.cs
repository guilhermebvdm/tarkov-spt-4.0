using System;
using UnityEngine;

namespace CommonAssets.Scripts.Audio.RadioSystem;

[Serializable]
public class BroadcastItemData
{
	public AudioClip clip;

	[HideInInspector]
	public int clipIndex;

	[HideInInspector]
	public float clipLength;

	[HideInInspector]
	public EBroadcastItemType itemType;
}
