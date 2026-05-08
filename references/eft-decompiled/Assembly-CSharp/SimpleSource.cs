using UnityEngine;
using UnityEngine.Audio;

public class SimpleSource : BetterSource
{
	public override bool Loop
	{
		get
		{
			return source1.loop;
		}
		set
		{
			source1.loop = value;
		}
	}

	public override void Init()
	{
		source1 = GetComponent<AudioSource>();
		base.Init();
	}

	public override AudioClip GetClip(int id)
	{
		return source1.clip;
	}

	public override void SetClip(int id, AudioClip clip)
	{
		if (!source1.isPlaying)
		{
			source1.clip = clip;
		}
	}

	public override void SetSpatialBlend(float spatialBlend = 1f)
	{
		source1.spatialBlend = spatialBlend;
	}

	public override void Mute(bool muted)
	{
		source1.mute = muted;
	}

	public override void SetMixerGroup(AudioMixerGroup mixerGroup)
	{
		source1.outputAudioMixerGroup = mixerGroup;
	}

	public override void PlayScheduled(double time)
	{
		base.PlayScheduled(time);
		PreOcclusionVolume = source1.volume;
		EndPlaybackTime = CalculateEndPlaybackTime(source1.clip, null, time - AudioSettings.dspTime);
		float volume = PreOcclusionVolume * base.OcclusionVolumeFactor;
		source1.volume = volume;
		source1.PlayScheduled(time);
	}

	public override void SetScheduledEndTime(double time)
	{
		source1.SetScheduledEndTime(time);
	}

	public override void SetPitch(float p)
	{
		source1.pitch = p;
	}

	public override void Play(AudioClip clip1, AudioClip clip2, float balance, float volume = 1f, bool forceStereo = false, bool oneShot = true)
	{
		if (!base.CanPlay)
		{
			return;
		}
		base.Play(clip1, clip2, balance, volume, forceStereo, oneShot);
		if (!oneShot && clip1 != null)
		{
			PreOcclusionVolume = base.BaseVolume * volume;
			UpdateSourceVolume();
			source1.clip = clip1;
			source1.Play();
			return;
		}
		PreOcclusionVolume = base.BaseVolume;
		UpdateSourceVolume();
		if (clip2 != null && clip1 != null)
		{
			source1.PlayOneShot(clip1, volume * balance);
			source1.PlayOneShot(clip2, volume * (1f - balance));
		}
		else if (clip1 != null)
		{
			source1.PlayOneShot(clip1, volume);
		}
		else if (clip2 != null)
		{
			source1.PlayOneShot(clip2, volume);
		}
	}

	public override void Clear(float spatial = 1f, float pitch = 1f)
	{
		Loop = false;
		source1.clip = null;
		source1.spatialBlend = spatial;
		source1.pitch = pitch;
	}

	public override void Balance(float p)
	{
		Debug.LogWarning($"You can't balance SingleSource, cause it's single");
	}

	public override void Enable(bool p0)
	{
		source1.enabled = p0;
	}

	public override void SetRolloff(float distance)
	{
		base.MaxDistance = distance;
		source1.maxDistance = base.MaxDistance * base.OcclusionRolloffScale;
	}

	public override void SetBaseVolume(float volume)
	{
		base.SetBaseVolume(volume);
		PreOcclusionVolume = base.BaseVolume;
		UpdateSourceVolume();
	}
}
