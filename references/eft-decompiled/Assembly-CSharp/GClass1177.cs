using System;
using System.Runtime.CompilerServices;
using Audio.ConfiguredAudioPlayer;
using UnityEngine;
using UnityEngine.Audio;

public class GClass1177 : GInterface90
{
	[CompilerGenerated]
	public class Class809
	{
		public GClass1177 gclass1177_0;

		public Action onStopped;

		public void method_0()
		{
			gclass1177_0.method_1(onStopped);
		}
	}

	[CompilerGenerated]
	public class Class810
	{
		public Action onReleased;

		public void method_0()
		{
			onReleased?.Invoke();
		}
	}

	[NonSerialized]
	public Audio.ConfiguredAudioPlayer.EPlayerState EplayerState_0;

	[NonSerialized]
	public BetterSource BetterSource_0;

	[NonSerialized]
	public AudioClipConfig AudioClipConfig_0;

	[NonSerialized]
	public BetterAudio BetterAudio_0;

	public GClass1177(BetterAudio betterAudio, BetterAudio.AudioSourceGroupType groupType, AudioMixerGroup mixerGroup)
	{
		BetterSource_0 = betterAudio.GetSource(groupType);
		BetterSource_0.SetMixerGroup(mixerGroup);
		BetterAudio_0 = betterAudio;
		EplayerState_0 = Audio.ConfiguredAudioPlayer.EPlayerState.Idle;
	}

	public void method_0(AudioClipConfig config)
	{
		BetterSource_0.SetClip(0, config.clip);
		BetterSource_0.SetBaseVolume(config.volume);
		BetterSource_0.SetPitch(config.pitch);
		BetterSource_0.Loop = config.loop;
		BetterSource_0.Play(config.clip, null, 1f, config.volume, forceStereo: true, !config.loop);
	}

	public void Play(AudioClipConfig config)
	{
		if (EplayerState_0 == Audio.ConfiguredAudioPlayer.EPlayerState.Uninitialized)
		{
			return;
		}
		if (config.clip == null)
		{
			GClass722.Instance.LogError($"Failed play {typeof(GClass1177)}, clip is null");
		}
		else
		{
			if (config.Equals(AudioClipConfig_0))
			{
				return;
			}
			if (EplayerState_0 != Audio.ConfiguredAudioPlayer.EPlayerState.Idle)
			{
				Stop(allowFadeout: true);
			}
			method_0(config);
			AudioClipConfig_0 = config;
			if (config.fadeInTime > 0f)
			{
				EplayerState_0 = Audio.ConfiguredAudioPlayer.EPlayerState.Fading;
				BetterSource_0.VolumeFadeIn(config.fadeInTime, delegate
				{
					EplayerState_0 = Audio.ConfiguredAudioPlayer.EPlayerState.Playing;
				});
			}
			else
			{
				EplayerState_0 = Audio.ConfiguredAudioPlayer.EPlayerState.Playing;
			}
		}
	}

	public void Stop(bool allowFadeout = false, Action onStopped = null)
	{
		if (EplayerState_0 == Audio.ConfiguredAudioPlayer.EPlayerState.Uninitialized)
		{
			return;
		}
		if (allowFadeout && AudioClipConfig_0 != null && AudioClipConfig_0.fadeOutTime > 0f)
		{
			EplayerState_0 = Audio.ConfiguredAudioPlayer.EPlayerState.Fading;
			BetterSource_0?.VolumeFadeOut(AudioClipConfig_0.fadeOutTime, delegate
			{
				method_1(onStopped);
			});
		}
		else
		{
			BetterSource_0?.Stop();
			AudioClipConfig_0 = null;
			EplayerState_0 = Audio.ConfiguredAudioPlayer.EPlayerState.Idle;
			onStopped?.Invoke();
		}
	}

	public void method_1(Action onStopped = null)
	{
		BetterSource_0?.Stop();
		AudioClipConfig_0 = null;
		EplayerState_0 = Audio.ConfiguredAudioPlayer.EPlayerState.Idle;
		onStopped?.Invoke();
	}

	public void Release(bool allowFadeout = false, Action onReleased = null)
	{
		if (EplayerState_0 == Audio.ConfiguredAudioPlayer.EPlayerState.Uninitialized)
		{
			return;
		}
		if (BetterAudio_0 != null && BetterSource_0 != null)
		{
			Stop(allowFadeout, delegate
			{
				onReleased?.Invoke();
			});
			method_2(BetterSource_0, allowFadeout);
		}
		else
		{
			onReleased?.Invoke();
		}
	}

	public void method_2(BetterSource source, bool allowFadeout)
	{
		if (!(source == null))
		{
			if (!allowFadeout)
			{
				source.Release();
				return;
			}
			float num = AudioClipConfig_0?.fadeOutTime ?? 0f;
			BetterAudio_0.AddToAudioSourceQueue(source, AudioSettings.dspTime + (double)num);
		}
	}

	[CompilerGenerated]
	public void method_3()
	{
		EplayerState_0 = Audio.ConfiguredAudioPlayer.EPlayerState.Playing;
	}
}
