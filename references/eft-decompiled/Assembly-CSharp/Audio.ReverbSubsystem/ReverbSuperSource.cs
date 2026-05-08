using Audio.SpatialSystem;
using EFT;
using UnityEngine;
using UnityEngine.Audio;

namespace Audio.ReverbSubsystem;

public class ReverbSuperSource : SuperSource
{
	[SerializeField]
	private AudioSource _reverbSourceA;

	[SerializeField]
	private AudioSource _reverbSourceB;

	[SerializeField]
	private BaseSpatialAudioSource _spatializerA;

	[SerializeField]
	private BaseSpatialAudioSource _spatializerB;

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
			_reverbSourceA.loop = value;
			_reverbSourceB.loop = value;
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
		AudioSource reverbSourceA = _reverbSourceA;
		_reverbSourceB.enabled = false;
		reverbSourceA.enabled = false;
		_spatializerA.SetActive(active: false);
		_spatializerB.SetActive(active: false);
		AudioSource reverbSourceA2 = _reverbSourceA;
		AudioClip clip = (_reverbSourceB.clip = null);
		reverbSourceA2.clip = clip;
	}

	public override void SetPreset(AudioGroupPreset preset)
	{
		base.SetPreset(preset);
		method_12(_reverbSourceA, _spatializerA, preset);
		method_12(_reverbSourceB, _spatializerB, preset);
		bool_0 = preset.acousticSettings.enabledPrewarm;
	}

	public override void SetMixerGroup(AudioMixerGroup mixerGroup)
	{
		AudioSource reverbSourceA = _reverbSourceA;
		AudioMixerGroup outputAudioMixerGroup = (_reverbSourceB.outputAudioMixerGroup = mixerGroup);
		reverbSourceA.outputAudioMixerGroup = outputAudioMixerGroup;
		base.SetMixerGroup(mixerGroup);
	}

	public override void SetPriority(int priority)
	{
		AudioSource reverbSourceA = _reverbSourceA;
		int priority2 = (_reverbSourceB.priority = priority);
		reverbSourceA.priority = priority2;
		base.SetPriority(priority);
	}

	public void method_12(AudioSource source, BaseSpatialAudioSource spatializer, AudioGroupPreset preset)
	{
		source.rolloffMode = AudioRolloffMode.Custom;
		source.SetCustomCurve(AudioSourceCurveType.CustomRolloff, preset.SoundRolloff);
		spatializer.EnableSpatialization = true;
		spatializer.VolumetricRadius = preset.OcclusionSourceRadius;
		spatializer.ReverbSendDB = preset.acousticSettings.stereo.reverbSendDb;
		spatializer.EarlyReflectionsSendDB = preset.acousticSettings.stereo.earlyReflectionsSendDb;
		spatializer.ReverbReach = preset.acousticSettings.stereo.reverbReach;
		spatializer.DirectSoundEnabled = false;
		spatializer.UpdateParameters();
	}

	public override void PlayScheduled(double time)
	{
		if (bool_0 && EnableSpatialization)
		{
			gclass885_0.PlayPrewarmSound(source1);
			gclass885_0.PlayPrewarmSound(source2);
		}
		base.PlayScheduled(time);
		if (EnableReverb && !EnableSpatialization)
		{
			method_14(_reverbSourceA, _spatializerA);
			method_14(_reverbSourceB, _spatializerB);
			method_13(_reverbSourceA, source1, time);
			method_13(_reverbSourceB, source2, time);
		}
		else
		{
			AudioSource reverbSourceA = _reverbSourceA;
			_reverbSourceB.enabled = false;
			reverbSourceA.enabled = false;
			_spatializerA.SetActive(active: false);
			_spatializerB.SetActive(active: false);
		}
	}

	public void method_13(AudioSource reverbSource, AudioSource donorSource, double time)
	{
		if (!(donorSource.clip == null))
		{
			reverbSource.maxDistance = donorSource.maxDistance;
			reverbSource.volume = donorSource.volume;
			reverbSource.clip = donorSource.clip;
			reverbSource.PlayScheduled(time);
		}
	}

	public override void SetScheduledEndTime(double time)
	{
		_reverbSourceA.SetScheduledEndTime(time);
		_reverbSourceB.SetScheduledEndTime(time);
		base.SetScheduledEndTime(time);
	}

	public override void SetPitch(float p)
	{
		_reverbSourceA.pitch = p;
		_reverbSourceB.pitch = p;
		base.SetPitch(p);
	}

	public override void OnStop()
	{
		_reverbSourceA.Stop();
		_reverbSourceB.Stop();
		AudioSource reverbSourceA = _reverbSourceA;
		_reverbSourceB.enabled = false;
		reverbSourceA.enabled = false;
		_spatializerA.SetActive(active: false);
		_spatializerB.SetActive(active: false);
		AudioSource reverbSourceA2 = _reverbSourceA;
		AudioClip clip = (_reverbSourceB.clip = null);
		reverbSourceA2.clip = clip;
		base.OnStop();
	}

	public override void Balance(float p)
	{
		base.Balance(p);
		_reverbSourceA.volume = PreOcclusionVolume;
		_reverbSourceB.volume = 1f - PreOcclusionVolume;
	}

	public void method_14(AudioSource source, BaseSpatialAudioSource spatializer)
	{
		source.enabled = true;
		spatializer.SetActive(active: true);
		spatializer.EnableReverb = false;
		spatializer.DirectSoundEnabled = true;
		spatializer.UpdateParameters();
		if (bool_0)
		{
			gclass885_0.PlayPrewarmSound(source);
		}
		spatializer.EnableReverb = true;
		spatializer.DirectSoundEnabled = false;
		spatializer.UpdateParameters();
	}
}
