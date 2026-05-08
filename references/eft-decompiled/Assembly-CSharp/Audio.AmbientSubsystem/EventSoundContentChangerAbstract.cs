using EFT;
using UnityEngine;

namespace Audio.AmbientSubsystem;

public abstract class EventSoundContentChangerAbstract : MonoBehaviour, GInterface104
{
	public abstract void ChangeSoundContent(EEventType eventType);

	public EventSoundContentChangerAbstract()
	{
	}
}
