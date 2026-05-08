using Audio.SpatialSystem;
using UnityEngine;
using UnityEngine.Audio;

public class SuperSource : BetterSource
{
	public AudioSource source2;

	private BaseSpatialAudioSource baseSpatialAudioSource_0;

	private float float_0 = 1f;

	public override bool Loop
	{
		get
		{
			return source1.loop;
		}
		set
		{
			AudioSource audioSource = source1;
			bool loop = (source2.loop = value);
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

	public override AudioClip GetClip(int id)
	{
		if (id != 0)
		{
			return source2.clip;
		}
		return source1.clip;
	}

	public override void SetClip(int id, AudioClip clip)
	{
		AudioSource audioSource = ((id == 0) ? source1 : source2);
		if (!audioSource.isPlaying)
		{
			audioSource.clip = clip;
		}
	}

	public override void SetMixerGroup(AudioMixerGroup mixerGroup)
	{
		AudioSource audioSource = source1;
		AudioMixerGroup outputAudioMixerGroup = (source2.outputAudioMixerGroup = mixerGroup);
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
		source2.volume = Mathf.Clamp01(Mathf.Lerp(source2.volume, b, speed));
	}

	public override void Init()
	{
		source1 = GetComponent<AudioSource>();
		source2 = method_11();
		source2.spatialBlend = source1.spatialBlend;
		source2.SetCustomCurve(AudioSourceCurveType.Spread, source1.GetCustomCurve(AudioSourceCurveType.Spread));
		source2.rolloffMode = source1.rolloffMode;
		source2.SetCustomCurve(AudioSourceCurveType.CustomRolloff, source1.GetCustomCurve(AudioSourceCurveType.CustomRolloff));
		source2.maxDistance = source1.maxDistance;
		source2.minDistance = source1.minDistance;
		source2.outputAudioMixerGroup = source1.outputAudioMixerGroup;
		baseSpatialAudioSource_0 = method_10();
		base.Init();
	}

	public override void SetActive(bool active)
	{
		source2.enabled = active;
		base.SetActive(active);
	}

	public override void SetSpatialBlend(float spatialBlend = 1f)
	{
		AudioSource audioSource = source1;
		float spatialBlend2 = (source2.spatialBlend = spatialBlend);
		audioSource.spatialBlend = spatialBlend2;
	}

	public override void Mute(bool muted)
	{
		AudioSource audioSource = source1;
		bool mute = (source2.mute = muted);
		audioSource.mute = mute;
	}

	public override void PlayScheduled(double time)
	{
		base.PlayScheduled(time);
		EndPlaybackTime = CalculateEndPlaybackTime(source1.clip, source2.clip, time - AudioSettings.dspTime);
		PreOcclusionVolume = source1.volume;
		float_0 = source2.volume;
		UpdateSourceVolume();
		if (source1.clip != null)
		{
			source1.PlayScheduled(time);
		}
		if (source2.clip != null)
		{
			source2.PlayScheduled(time);
		}
	}

	public override void SetScheduledEndTime(double time)
	{
		source1.SetScheduledEndTime(time);
		source2.SetScheduledEndTime(time);
	}

	public override void SetPitch(float p)
	{
		AudioSource audioSource = source1;
		float pitch = (source2.pitch = p);
		audioSource.pitch = pitch;
	}

	public override float GetPitch(int clipIndex)
	{
		if (clipIndex != 0)
		{
			return source2.pitch;
		}
		return source1.pitch;
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
			source1.clip = clip1;
			source1.Play();
			if (clip2 != null)
			{
				source2.clip = clip2;
				source2.Play();
			}
		}
	}

	public override void OnStop()
	{
		EndPlaybackTime = AudioSettings.dspTime;
		if (source2 != null)
		{
			source2.Stop();
			source2.clip = null;
		}
		base.OnStop();
	}

	public override void Clear(float spatial = 1f, float pitch = 1f)
	{
		Loop = false;
		AudioSource audioSource = source1;
		AudioClip clip = (source2.clip = null);
		audioSource.clip = clip;
		AudioSource audioSource2 = source1;
		float spatialBlend = (source2.spatialBlend = spatial);
		audioSource2.spatialBlend = spatialBlend;
		SetPitch(pitch);
	}

	public override void SetPriority(int priority)
	{
		base.SetPriority(priority);
		source2.priority = priority;
	}

	public override void Balance(float p)
	{
		PreOcclusionVolume = p;
		float_0 = 1f - p;
	}

	public override void Enable(bool p0)
	{
		AudioSource audioSource = source1;
		bool flag = (source2.enabled = p0);
		audioSource.enabled = flag;
	}

	public override void SetRolloff(float distance)
	{
		base.MaxDistance = distance;
		AudioSource audioSource = source1;
		float maxDistance = (source2.maxDistance = base.MaxDistance * base.OcclusionRolloffScale);
		audioSource.maxDistance = maxDistance;
	}

	public BaseSpatialAudioSource method_10()
	{
		return base.transform.GetChild(0).GetComponent<BaseSpatialAudioSource>();
	}

	public AudioSource method_11()
	{
		return base.transform.GetChild(0).GetComponent<AudioSource>();
	}
}
