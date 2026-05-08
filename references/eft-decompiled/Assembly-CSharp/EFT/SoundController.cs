using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

namespace EFT;

public class SoundController : MonoBehaviour
{
	public AudioMixerGroup OccludedGroup;

	public AudioMixerGroup DirectGroup;

	public AudioMixerGroup ObstructedGroup;

	public Transform AudioListenerTransform;

	private static SoundController soundController_0;

	public static SoundController Instance
	{
		get
		{
			if (soundController_0 == null)
			{
				soundController_0 = GClass870.FindUnityObjectOfType<SoundController>();
			}
			if (soundController_0 == null)
			{
				soundController_0 = new GameObject("SoundController").AddComponent<SoundController>();
				soundController_0.SetAudioListener(GClass870.FindUnityObjectOfType<AudioListener>());
			}
			return soundController_0;
		}
	}

	public void Start()
	{
		StartCoroutine(method_0());
	}

	public IEnumerator method_0()
	{
		while (AudioListenerTransform == null)
		{
			SetAudioListener(GClass870.FindUnityObjectOfType<AudioListener>());
			yield return new WaitForSeconds(1f);
		}
	}

	public float Distance(Vector3 target)
	{
		return Vector3.Distance(AudioListenerTransform.position, target);
	}

	public void SetAudioListener(AudioListener listener)
	{
		if (listener != null)
		{
			AudioListenerTransform = listener.gameObject.transform;
		}
	}
}
