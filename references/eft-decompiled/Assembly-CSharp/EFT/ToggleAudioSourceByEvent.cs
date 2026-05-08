using EFT.GlobalEvents;
using UnityEngine;

namespace EFT;

public class ToggleAudioSourceByEvent : MonoBehaviour
{
	[SerializeField]
	private AudioSource _audioSource;

	[SerializeField]
	private BaseEventFilter _enableAudioByFilter;

	[SerializeField]
	private BaseEventFilter _disableAudioByFilter;

	public void Awake()
	{
		_enableAudioByFilter.OnFilterPassed += method_0;
		_disableAudioByFilter.OnFilterPassed += method_1;
	}

	public void OnDestroy()
	{
		_enableAudioByFilter.OnFilterPassed -= method_0;
		_disableAudioByFilter.OnFilterPassed -= method_1;
	}

	public void method_0(BaseEventFilter eventFilter, GClass3542 invokedEvent)
	{
		_audioSource.enabled = true;
	}

	public void method_1(BaseEventFilter eventFilter, GClass3542 invokedEvent)
	{
		_audioSource.enabled = false;
	}
}
