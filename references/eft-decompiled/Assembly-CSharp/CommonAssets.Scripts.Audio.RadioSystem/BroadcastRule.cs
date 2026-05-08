using System;
using UnityEngine;

namespace CommonAssets.Scripts.Audio.RadioSystem;

[Serializable]
[CreateAssetMenu(menuName = "Scriptable objects/Audio/BroadcastRule", fileName = "BroadcastRule")]
public class BroadcastRule : ScriptableObject
{
	[Serializable]
	public class CrossfadeDurationByType : SerializableEnumDictionary<EBroadcastItemType, float>
	{
	}

	public int broadcastItemsCount = 30;

	public Vector2 tracksBeforeJingle = new Vector2(3f, 8f);

	public Vector2 adsAfterTracks = new Vector2(7f, 10f);

	public Vector2 showAfterTracks = new Vector2(12f, 15f);

	public Vector2 showsCount = new Vector2(0f, 2f);

	public bool includeShow = true;

	public bool restartBroadcastAfterFinished = true;

	[SerializeField]
	private CrossfadeDurationByType _crossFadeDurationByType = new CrossfadeDurationByType
	{
		{
			EBroadcastItemType.Music,
			5f
		},
		{
			EBroadcastItemType.Ad,
			1f
		},
		{
			EBroadcastItemType.Jingle,
			1f
		},
		{
			EBroadcastItemType.SFX,
			1f
		},
		{
			EBroadcastItemType.Show,
			3f
		}
	};

	public float GetCrossfadeDuration(EBroadcastItemType type)
	{
		if (!_crossFadeDurationByType.TryGetValue(type, out var value))
		{
			return 1f;
		}
		return value;
	}
}
