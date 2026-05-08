using System;
using EFT;
using UnityEngine;

namespace Audio.AmbientSubsystem;

public class EventSoundBankChanger : MonoBehaviour, GInterface104
{
	[Serializable]
	public class EventSoundBanks : SerializableEnumDictionary<EEventType, SoundBank>
	{
	}

	[SerializeField]
	private EventSoundBanks _eventSoundBanks;

	private GInterface105[] ginterface105_0;

	public void Awake()
	{
		ginterface105_0 = GetComponentsInChildren<GInterface105>();
	}

	public void ChangeSoundContent(EEventType eventType)
	{
		if (!base.enabled)
		{
			Debug.Log(" [AMBIENT SYSTEM] Not working EventSoundBankChanger component " + base.name + " on scene!");
		}
		else if (ginterface105_0 != null && ginterface105_0.Length != 0)
		{
			if (!_eventSoundBanks.TryGetValue(eventType, out var value))
			{
				Debug.Log($"[AMBIENT SYSTEM] Not suit soundbank for event: {eventType}!");
				return;
			}
			GInterface105[] array = ginterface105_0;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].ChangeSoundBank(value);
			}
		}
		else
		{
			Debug.LogError(" [AMBIENT SYSTEM] Not find any changeable components " + base.name + "!");
		}
	}
}
