using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Audio.ConfiguredAudioPlayer;
using UnityEngine;
using UnityEngine.Audio;

public class GClass1176 : GInterface90
{
	[CompilerGenerated]
	public class Class806
	{
		public GClass1176 gclass1176_0;

		public Action onStopped;

		public void method_0()
		{
			gclass1176_0.method_5(onStopped);
		}
	}

	[CompilerGenerated]
	public class Class807
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
	public BetterSource BetterSource_1;

	[NonSerialized]
	public BetterSource BetterSource_2;

	[NonSerialized]
	public BetterSource BetterSource_3;

	[NonSerialized]
	public AudioClipConfig AudioClipConfig_0;

	[NonSerialized]
	public Queue<AudioClipConfig> Queue_0 = new Queue<AudioClipConfig>();

	[NonSerialized]
	public MonoBehaviour MonoBehaviour_0;

	[NonSerialized]
	public BetterAudio BetterAudio_0;

	[NonSerialized]
	public Coroutine Coroutine_0;

	public GClass1176(BetterAudio betterAudio, BetterAudio.AudioSourceGroupType groupType, AudioMixerGroup mixerGroup)
	{
		BetterAudio_0 = betterAudio;
		MonoBehaviour_0 = betterAudio;
		BetterSource_0 = betterAudio.GetSource(groupType);
		BetterSource_1 = betterAudio.GetSource(groupType);
		BetterSource_0.SetMixerGroup(mixerGroup);
		BetterSource_1.SetMixerGroup(mixerGroup);
		BetterSource_2 = BetterSource_0;
		BetterSource_3 = BetterSource_1;
		EplayerState_0 = Audio.ConfiguredAudioPlayer.EPlayerState.Idle;
	}

	public void method_0(BetterSource source, AudioClipConfig config)
	{
		source.SetClip(0, config.clip);
		source.SetBaseVolume(config.volume);
		source.SetPitch(config.pitch);
		source.Loop = config.loop;
		source.Play(config.clip, null, 1f, config.volume, forceStereo: true, !config.loop);
	}

	public void method_1(AudioClipConfig config)
	{
		Queue_0.Clear();
		Queue_0.Enqueue(config);
	}

	public AudioClipConfig method_2()
	{
		if (Queue_0.Count <= 0)
		{
			return null;
		}
		return Queue_0.Dequeue();
	}

	public void Play(AudioClipConfig config)
	{
		if (EplayerState_0 == Audio.ConfiguredAudioPlayer.EPlayerState.Uninitialized)
		{
			return;
		}
		if (config?.clip == null)
		{
			GClass722.Instance.LogError($"Failed play {typeof(GClass1176)}, clip is null");
		}
		else if (!config.Equals(AudioClipConfig_0))
		{
			if (EplayerState_0 == Audio.ConfiguredAudioPlayer.EPlayerState.Transitioning)
			{
				method_1(config);
				return;
			}
			if (MonoBehaviour_0 == null)
			{
				GClass722.Instance.LogError("Coroutine runner not found, cannot start audio transition.");
				return;
			}
			EplayerState_0 = Audio.ConfiguredAudioPlayer.EPlayerState.Transitioning;
			Coroutine_0 = MonoBehaviour_0.StartCoroutine(method_3(config));
		}
	}

	public IEnumerator method_3(AudioClipConfig newConfig)
	{
		AudioClipConfig audioClipConfig_ = AudioClipConfig_0;
		if (audioClipConfig_ != null)
		{
			method_0(BetterSource_3, newConfig);
			BetterSource_3.VolumeFadeIn(newConfig.fadeInTime);
			BetterSource_2.VolumeFadeOut(audioClipConfig_.fadeOutTime);
			float seconds = Mathf.Max(newConfig.fadeInTime, audioClipConfig_.fadeOutTime);
			yield return new WaitForSeconds(seconds);
			BetterSource betterSource_ = BetterSource_3;
			BetterSource betterSource_2 = BetterSource_2;
			BetterSource_2 = betterSource_;
			BetterSource_3 = betterSource_2;
			BetterSource_3.Stop();
		}
		else
		{
			method_0(BetterSource_2, newConfig);
			BetterSource_2.VolumeFadeIn(newConfig.fadeInTime);
			yield return new WaitForSeconds(newConfig.fadeInTime);
		}
		Coroutine_0 = null;
		AudioClipConfig_0 = newConfig;
		method_4();
	}

	public void method_4()
	{
		AudioClipConfig audioClipConfig = method_2();
		if (audioClipConfig != null)
		{
			EplayerState_0 = Audio.ConfiguredAudioPlayer.EPlayerState.Idle;
			Play(audioClipConfig);
		}
		else
		{
			EplayerState_0 = Audio.ConfiguredAudioPlayer.EPlayerState.Idle;
		}
	}

	public void Stop(bool allowFadeout = false, Action onStopped = null)
	{
		if (EplayerState_0 == Audio.ConfiguredAudioPlayer.EPlayerState.Uninitialized)
		{
			return;
		}
		bool flag = EplayerState_0 == Audio.ConfiguredAudioPlayer.EPlayerState.Transitioning;
		Queue_0.Clear();
		if (Coroutine_0 != null)
		{
			MonoBehaviour_0.StopCoroutine(Coroutine_0);
			Coroutine_0 = null;
		}
		if (allowFadeout && AudioClipConfig_0 != null)
		{
			if (flag)
			{
				BetterSource_3.Stop();
			}
			BetterSource_2.VolumeFadeOut(AudioClipConfig_0.fadeOutTime, delegate
			{
				method_5(onStopped);
			});
		}
		else
		{
			method_7();
			AudioClipConfig_0 = null;
			EplayerState_0 = Audio.ConfiguredAudioPlayer.EPlayerState.Idle;
			onStopped?.Invoke();
		}
	}

	public void method_5(Action onStopped = null)
	{
		method_7();
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
		if (BetterAudio_0 != null)
		{
			Stop(allowFadeout, delegate
			{
				onReleased?.Invoke();
			});
			method_6(BetterSource_0, allowFadeout);
			method_6(BetterSource_1, allowFadeout);
		}
		else
		{
			onReleased?.Invoke();
		}
		EplayerState_0 = Audio.ConfiguredAudioPlayer.EPlayerState.Uninitialized;
	}

	public void method_6(BetterSource source, bool allowFadeout)
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

	public void method_7()
	{
		BetterSource_0.Stop();
		BetterSource_1.Stop();
	}
}
