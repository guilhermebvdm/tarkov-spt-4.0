using System;
using Audio.SpatialSystem;
using UnityEngine;
using UnityEngine.Audio;

public class SuperSourceDistant : BetterSource
{
	public float Delay;

	public float SpatialBlend = 1f;

	private AudioSource audioSource_0;

	private BaseSpatialAudioSource baseSpatialAudioSource_0;

	private double double_0;

	private float float_0;

	public override bool Loop
	{
		get
		{
			return source1.loop;
		}
		set
		{
			AudioSource audioSource = source1;
			bool loop = (audioSource_0.loop = value);
			audioSource.loop = loop;
		}
	}

	public override bool EnableSpatialization
	{
		get
		{
			return base.EnableSpatialization;
		}
		set
		{
			if (baseSpatialAudioSource_0 != null)
			{
				baseSpatialAudioSource_0.EnableSpatialization = value;
			}
			base.EnableSpatialization = value;
		}
	}

	public override bool EnableReverb
	{
		get
		{
			return Spatializer.EnableReverb;
		}
		set
		{
			if (!(Spatializer == null))
			{
				base.EnableReverb = value;
				baseSpatialAudioSource_0.EnableReverb = value;
			}
		}
	}

	public override float ReverbSendDB
	{
		get
		{
			return base.ReverbSendDB;
		}
		set
		{
			if (!(Spatializer == null))
			{
				base.ReverbSendDB = value;
				baseSpatialAudioSource_0.ReverbSendDB = value;
			}
		}
	}

	public override float EarlyReflectionsSendDB
	{
		get
		{
			return Spatializer.EarlyReflectionsSendDB;
		}
		set
		{
			if (!(Spatializer == null))
			{
				base.EarlyReflectionsSendDB = value;
				baseSpatialAudioSource_0.EarlyReflectionsSendDB = value;
			}
		}
	}

	public override float ReverbReach
	{
		get
		{
			return Spatializer.ReverbReach;
		}
		set
		{
			if (!(Spatializer == null))
			{
				base.ReverbReach = value;
				baseSpatialAudioSource_0.ReverbReach = value;
			}
		}
	}

	public override void PlayScheduled(double time)
	{
		throw new NotImplementedException();
	}

	public override void SetScheduledEndTime(double time)
	{
		throw new NotImplementedException();
	}

	public override AudioClip GetClip(int id)
	{
		if (id != 0)
		{
			return audioSource_0.clip;
		}
		return source1.clip;
	}

	public override void SetClip(int id, AudioClip clip)
	{
		AudioSource audioSource = ((id == 0) ? source1 : audioSource_0);
		if (!audioSource.isPlaying)
		{
			audioSource.clip = clip;
		}
	}

	public void Start()
	{
		Init();
	}

	public override void Init()
	{
		source1 = GetComponent<AudioSource>();
		audioSource_0 = method_12();
		audioSource_0.spatialBlend = source1.spatialBlend;
		audioSource_0.spread = source1.spread;
		audioSource_0.rolloffMode = source1.rolloffMode;
		audioSource_0.maxDistance = source1.maxDistance;
		audioSource_0.minDistance = source1.minDistance;
		audioSource_0.outputAudioMixerGroup = source1.outputAudioMixerGroup;
		baseSpatialAudioSource_0 = method_11();
		base.Init();
	}

	public override void SetActive(bool active)
	{
		audioSource_0.enabled = active;
		base.SetActive(active);
	}

	public override void SetPitch(float p)
	{
		AudioSource audioSource = source1;
		float pitch = (audioSource_0.pitch = p);
		audioSource.pitch = pitch;
	}

	public override void SetSpatialBlend(float spatialBlend = 1f)
	{
		AudioSource audioSource = source1;
		float spatialBlend2 = (audioSource_0.spatialBlend = spatialBlend);
		audioSource.spatialBlend = spatialBlend2;
	}

	public override void Mute(bool muted)
	{
		AudioSource audioSource = source1;
		bool mute = (audioSource_0.mute = muted);
		audioSource.mute = mute;
	}

	public override void Play(AudioClip clip1, AudioClip clip2, float balance, float volume = 1f, bool forceStereo = false, bool oneShot = true)
	{
		if (base.CanPlay)
		{
			base.Play(clip1, clip2, balance, volume, forceStereo, oneShot);
			float num = base.BaseVolume * volume;
			PreOcclusionVolume = num * balance;
			float_0 = num * (1f - balance);
			UpdateSourceVolume();
			method_10(source1, clip1);
			method_10(audioSource_0, clip2);
			EndPlaybackTime = CalculateEndPlaybackTime(clip1, clip2, Delay);
		}
	}

	public void method_10(AudioSource source, AudioClip clip)
	{
		if ((object)clip != null)
		{
			source.enabled = true;
			source.clip = clip;
			source.spatialBlend = SpatialBlend;
			source.PlayDelayed(Delay);
		}
	}

	public override void OnStop()
	{
		EndPlaybackTime = AudioSettings.dspTime;
		if (audioSource_0 != null)
		{
			audioSource_0.Stop();
			audioSource_0.clip = null;
		}
		base.OnStop();
	}

	public override void Clear(float spatial = 1f, float pitch = 1f)
	{
		Loop = false;
		AudioSource audioSource = source1;
		AudioClip clip = (audioSource_0.clip = null);
		audioSource.clip = clip;
		SetPitch(pitch);
		AudioSource audioSource2 = source1;
		float spatialBlend = (audioSource_0.spatialBlend = spatial);
		audioSource2.spatialBlend = spatialBlend;
	}

	public override void Balance(float p)
	{
		PreOcclusionVolume = p;
		float_0 = 1f - p;
	}

	public override void Enable(bool p0)
	{
		AudioSource audioSource = source1;
		bool flag = (audioSource_0.enabled = p0);
		audioSource.enabled = flag;
	}

	public override void SetRolloff(float distance)
	{
		base.MaxDistance = distance;
		AudioSource audioSource = source1;
		float maxDistance = (audioSource_0.maxDistance = base.MaxDistance * base.OcclusionRolloffScale);
		audioSource.maxDistance = maxDistance;
	}

	public override void SetMixerGroup(AudioMixerGroup mixerGroup)
	{
		AudioSource audioSource = source1;
		AudioMixerGroup outputAudioMixerGroup = (audioSource_0.outputAudioMixerGroup = mixerGroup);
		audioSource.outputAudioMixerGroup = outputAudioMixerGroup;
	}

	public override void SetBaseVolume(float volume)
	{
		base.SetBaseVolume(volume);
		Balance(base.BaseVolume);
		UpdateSourceVolume();
	}

	public override void UpdateSourceVolume(float speed = 1f)
	{
		base.UpdateSourceVolume(speed);
		float b = float_0 * base.OcclusionVolumeFactor * base.FadeFactor;
		audioSource_0.volume = Mathf.Clamp01(Mathf.Lerp(audioSource_0.volume, b, speed));
	}

	public override float GetPitch(int clipIndex)
	{
		if (clipIndex != 0)
		{
			return audioSource_0.pitch;
		}
		return source1.pitch;
	}

	public BaseSpatialAudioSource method_11()
	{
		return base.transform.GetChild(0).GetComponent<BaseSpatialAudioSource>();
	}

	public AudioSource method_12()
	{
		return base.transform.GetChild(0).GetComponent<AudioSource>();
	}
}
