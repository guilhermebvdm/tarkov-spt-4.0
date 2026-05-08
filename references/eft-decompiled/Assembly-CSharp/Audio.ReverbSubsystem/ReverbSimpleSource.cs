using Audio.SpatialSystem;
using EFT;
using UnityEngine;
using UnityEngine.Audio;

namespace Audio.ReverbSubsystem;

public class ReverbSimpleSource : SimpleSource
{
	[SerializeField]
	private AudioSource _reverbSource;

	[SerializeField]
	private BaseSpatialAudioSource _spatializer;

	private GClass885 gclass885_0;

	private bool bool_0;

	public override bool Loop
	{
		get
		{
			return base.Loop;
		}
		set
		{
			_reverbSource.loop = value;
			base.Loop = value;
		}
	}

	public override void Init()
	{
		gclass885_0 = new GClass885();
		base.Init();
	}

	public override void SetActive(bool active)
	{
		base.SetActive(active);
		_reverbSource.enabled = false;
		_reverbSource.clip = null;
		_spatializer.SetActive(active: false);
	}

	public override void SetPreset(AudioGroupPreset preset)
	{
		base.SetPreset(preset);
		_reverbSource.rolloffMode = AudioRolloffMode.Custom;
		_reverbSource.SetCustomCurve(AudioSourceCurveType.CustomRolloff, preset.SoundRolloff);
		_spatializer.EnableSpatialization = true;
		_spatializer.ReverbReach = preset.acousticSettings.stereo.reverbReach;
		_spatializer.VolumetricRadius = preset.OcclusionSourceRadius;
		_spatializer.ReverbSendDB = preset.acousticSettings.stereo.reverbSendDb;
		_spatializer.EarlyReflectionsSendDB = preset.acousticSettings.stereo.earlyReflectionsSendDb;
		_spatializer.DirectSoundEnabled = false;
		bool_0 = preset.acousticSettings.enabledPrewarm;
		UpdateParameters();
	}

	public override void SetPriority(int priority)
	{
		_reverbSource.priority = priority;
		base.SetPriority(priority);
	}

	public override void SetMixerGroup(AudioMixerGroup mixerGroup)
	{
		_reverbSource.outputAudioMixerGroup = mixerGroup;
		base.SetMixerGroup(mixerGroup);
	}

	public override void Play(AudioClip clip1, AudioClip clip2, float balance, float volume = 1f, bool forceStereo = false, bool oneShot = true)
	{
		if (!base.CanPlay)
		{
			return;
		}
		if (EnableSpatialization && bool_0)
		{
			gclass885_0.PlayPrewarmSound(source1);
		}
		base.Play(clip1, clip2, balance, volume, forceStereo, oneShot);
		if (EnableReverb && (forceStereo || !EnableSpatialization))
		{
			method_10();
			_reverbSource.maxDistance = source1.maxDistance;
			_reverbSource.volume = source1.volume;
			if (!oneShot && clip1 != null)
			{
				_reverbSource.clip = clip1;
				_reverbSource.Play();
			}
			else if (clip2 != null && clip1 != null)
			{
				_reverbSource.PlayOneShot(clip1, volume * balance);
				_reverbSource.PlayOneShot(clip2, volume * (1f - balance));
			}
			else if (clip1 != null)
			{
				_reverbSource.PlayOneShot(clip1, volume);
			}
			else if (clip2 != null)
			{
				_reverbSource.PlayOneShot(clip2, volume);
			}
		}
		else
		{
			_reverbSource.enabled = false;
			_spatializer.SetActive(active: false);
		}
	}

	public override void OnStop()
	{
		_reverbSource.Stop();
		_reverbSource.enabled = false;
		_reverbSource.clip = null;
		_spatializer.SetActive(active: false);
		base.OnStop();
	}

	public override void UpdateParameters()
	{
		_spatializer.UpdateParameters();
		base.UpdateParameters();
	}

	public void method_10()
	{
		_reverbSource.enabled = true;
		_spatializer.SetActive(active: true);
		_spatializer.EnableReverb = false;
		_spatializer.DirectSoundEnabled = true;
		_spatializer.UpdateParameters();
		if (bool_0)
		{
			gclass885_0.PlayPrewarmSound(_reverbSource);
		}
		_spatializer.EnableReverb = true;
		_spatializer.DirectSoundEnabled = false;
		_spatializer.UpdateParameters();
	}

	public override void UpdateSourceVolume(float speed = 1f)
	{
		base.UpdateSourceVolume(speed);
		_reverbSource.volume = source1.volume;
	}
}
