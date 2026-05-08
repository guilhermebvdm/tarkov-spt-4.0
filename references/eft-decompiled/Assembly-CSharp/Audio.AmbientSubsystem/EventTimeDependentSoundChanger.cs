using EFT;
using UnityEngine;

namespace Audio.AmbientSubsystem;

public class EventTimeDependentSoundChanger : MonoBehaviour, GInterface104
{
	[SerializeField]
	private DayTimeAmbientSoundContainer _soundContainer;

	private GInterface103[] ginterface103_0;

	public void Awake()
	{
		ginterface103_0 = GetComponentsInChildren<GInterface103>();
	}

	public void ChangeSoundContent(EEventType eventType)
	{
		if (ginterface103_0 != null && ginterface103_0.Length != 0)
		{
			GInterface103[] array = ginterface103_0;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].ChangeSoundContainer(_soundContainer);
			}
		}
		else
		{
			Debug.LogError("Not find any changeable components!");
		}
	}
}
