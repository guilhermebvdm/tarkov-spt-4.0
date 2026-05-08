using System;
using EFT;
using UnityEngine;

namespace Audio.AmbientSubsystem;

public class EventAudioClipChanger : MonoBehaviour, GInterface104
{
	[Serializable]
	public class EventAudioClips : SerializableEnumDictionary<EEventType, AudioClip>
	{
	}

	[SerializeField]
	private EventAudioClips _eventAudioClips;

	private GInterface102[] ginterface102_0;

	public void Awake()
	{
		ginterface102_0 = GetComponentsInChildren<GInterface102>();
	}

	public void ChangeSoundContent(EEventType eventType)
	{
		if (ginterface102_0 != null && ginterface102_0.Length != 0)
		{
			if (!_eventAudioClips.TryGetValue(eventType, out var value))
			{
				Debug.Log($"Not suitable clip for event: {eventType}!");
				return;
			}
			GInterface102[] array = ginterface102_0;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].ChangeAudioClip(value);
			}
		}
		else
		{
			Debug.LogError("Not find any changeable components!");
		}
	}
}
