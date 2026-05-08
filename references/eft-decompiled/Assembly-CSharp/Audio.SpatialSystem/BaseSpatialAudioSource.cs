using System.Runtime.CompilerServices;
using EFT;
using UnityEngine;

namespace Audio.SpatialSystem;

[RequireComponent(typeof(AudioSource))]
public abstract class BaseSpatialAudioSource : MonoBehaviour, GInterface88, GInterface78, GInterface69
{
	[CompilerGenerated]
	private AudioSource audioSource_0;

	public AudioSource ParentSource
	{
		[CompilerGenerated]
		get
		{
			return audioSource_0;
		}
		[CompilerGenerated]
		set
		{
			audioSource_0 = value;
		}
	}

	public abstract bool EnableSpatialization { get; set; }

	public abstract float HrtfIntensity { get; set; }

	public abstract float DirectivityIntensity { get; set; }

	public abstract bool EnableReverb { get; set; }

	public abstract bool EnableDirectSound { get; set; }

	public abstract float ReverbSendDB { get; set; }

	public abstract float EarlyReflectionsSendDB { get; set; }

	public abstract float ReverbReach { get; set; }

	public abstract float VolumetricRadius { get; set; }

	public abstract bool DirectSoundEnabled { get; set; }

	public abstract void SetParameters(AudioGroupPreset preset);

	public abstract void UpdateParameters();

	public virtual void Awake()
	{
		ParentSource = GetComponent<AudioSource>();
	}

	public abstract void ManualUpdate();

	public abstract void SetActive(bool active);

	public BaseSpatialAudioSource()
	{
	}
}
