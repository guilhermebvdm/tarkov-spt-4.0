using System;
using UnityEngine;

namespace Audio.AmbientSubsystem;

[Serializable]
public class RoomAmbientSoundPlayer : ISoundPlayer
{
	[NonSerialized]
	public RoomAmbientData AmbientData;

	[NonSerialized]
	public BetterSource Source;

	public Vector3 Position
	{
		get
		{
			return Source.Position;
		}
		set
		{
			Source.Position = value;
		}
	}

	[field: NonSerialized]
	public bool IsPlaying { get; set; }

	public event Action Stopped;

	public event Action Played;

	public event Action StartPlay;

	public RoomAmbientSoundPlayer(RoomAmbientData ambientData)
	{
		AmbientData = ambientData;
	}

	public void Play()
	{
		if (MonoBehaviourSingleton<BetterAudio>.Instantiated)
		{
			method_0();
			IsPlaying = true;
			this.Played?.Invoke();
			this.StartPlay?.Invoke();
			Source = MonoBehaviourSingleton<BetterAudio>.Instance.GetSource(BetterAudio.AudioSourceGroupType.Nonspatial);
			Source.EnabledEQ(targetState: false);
			Source.EnabledUpdate(targetState: false);
			Source.SetMixerGroup(MonoBehaviourSingleton<BetterAudio>.Instance.AmbientInMixer);
			Source.IncludeInOcclusionProcess(included: false);
			Source.SetSpatialBlend(0f);
			Source.Loop = true;
			Source.Play(AmbientData.RoomTone, null, 1f, AmbientData.RoomToneVolume, forceStereo: true, oneShot: false);
			FadeIn(AmbientData.FadeInSeconds);
		}
	}

	public void Stop(bool force = false)
	{
		if (!(Source == null))
		{
			FadeOut(force ? 0f : AmbientData.FadeOutSeconds);
		}
	}

	public void FadeIn(float fadeInSec)
	{
		if (!(Source == null))
		{
			Source.VolumeFadeIn(fadeInSec);
		}
	}

	public void FadeOut(float fadeOutSec)
	{
		if (!(Source == null))
		{
			Source.VolumeFadeOut(fadeOutSec, method_0);
		}
	}

	public void method_0()
	{
		if (Source != null)
		{
			Source.source1.Stop();
			Source.Release();
			Source = null;
		}
		IsPlaying = false;
		this.Stopped?.Invoke();
	}
}
