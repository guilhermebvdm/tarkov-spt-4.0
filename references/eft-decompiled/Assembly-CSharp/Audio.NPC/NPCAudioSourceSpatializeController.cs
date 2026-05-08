using Comfort.Common;
using UnityEngine;

namespace Audio.NPC;

public class NPCAudioSourceSpatializeController : MonoBehaviour
{
	[SerializeField]
	private AudioSource _speechSource;

	[SerializeField]
	private AudioSource _footstepAudioSource;

	public void Awake()
	{
		method_0();
	}

	public void method_0()
	{
		if (!Singleton<SharedGameSettingsClass>.Instantiated)
		{
			Debug.LogWarning("Can't handle Settings Manager");
			return;
		}
		method_1(_speechSource, spatialize: true);
		method_1(_footstepAudioSource, spatialize: true);
	}

	public void method_1(AudioSource source, bool spatialize)
	{
		if (!(source == null))
		{
			if (source.TryGetComponent<GInterface88>(out var component))
			{
				component.EnableSpatialization = spatialize;
			}
			else
			{
				source.spatialize = false;
			}
		}
	}
}
