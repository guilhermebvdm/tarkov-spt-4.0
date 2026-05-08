using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

namespace Audio.AudioCulling;

public class SyncLoopSoundPlayer : MonoBehaviour
{
	private const float float_0 = 0.005f;

	[SerializeField]
	private AudioSource[] _sources;

	[SerializeField]
	private AudioClip _clip;

	[SerializeField]
	private AudioMixerGroup _outputMixerGroup;

	[SerializeField]
	private bool _playOnAwake = true;

	[SerializeField]
	private bool _useSoundDelayCalculation;

	private bool bool_0;

	private float float_1;

	private float float_2;

	private Coroutine coroutine_0;

	public void Awake()
	{
		method_0();
		if (_playOnAwake)
		{
			Play();
		}
	}

	public void OnEnable()
	{
		if (!bool_0 && _playOnAwake)
		{
			Play();
		}
	}

	public void method_0()
	{
		float_1 = _clip.length;
		if (_sources.Length == 0)
		{
			method_1();
		}
		AudioSource[] sources = _sources;
		foreach (AudioSource audioSource in sources)
		{
			audioSource.outputAudioMixerGroup = _outputMixerGroup;
			audioSource.playOnAwake = false;
			audioSource.loop = false;
			if (_useSoundDelayCalculation)
			{
				audioSource.clip = _clip;
			}
		}
	}

	public void method_1()
	{
		_sources = GetComponentsInChildren<AudioSource>();
	}

	public void Play()
	{
		if (!bool_0)
		{
			Stop();
			if (base.gameObject.activeInHierarchy)
			{
				bool_0 = true;
				coroutine_0 = StartCoroutine(method_2());
			}
		}
	}

	public IEnumerator method_2()
	{
		while (bool_0)
		{
			if (Time.realtimeSinceStartup > float_2)
			{
				AudioSource[] sources = _sources;
				foreach (AudioSource audioSource in sources)
				{
					if (audioSource.enabled && audioSource.gameObject.activeInHierarchy)
					{
						method_3(audioSource);
					}
				}
				float_2 = Time.realtimeSinceStartup + float_1 + 0.005f;
			}
			yield return null;
		}
	}

	public void method_3(AudioSource source)
	{
		if (_useSoundDelayCalculation)
		{
			float delay = GClass2313.CalculatePhysicSoundDelay(source.transform.position);
			source.PlayDelayed(delay);
		}
		else
		{
			source.PlayOneShot(_clip);
		}
	}

	public void Stop()
	{
		GClass7.TryStopCoroutine(this, ref coroutine_0);
		bool_0 = false;
	}

	public void OnDisable()
	{
		Stop();
	}

	public void OnDestroy()
	{
		Stop();
	}
}
