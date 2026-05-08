using System;
using Audio.SpatialSystem;
using Comfort.Common;
using EFT;
using UnityEngine;

public class GClass1495 : GClass1494
{
	[NonSerialized]
	public AudioSource AudioSource_0;

	[NonSerialized]
	public BetterSource BetterSource_0;

	[NonSerialized]
	public Action Action_0;

	public override bool Enabled
	{
		get
		{
			if (AudioSource_0.gameObject.activeInHierarchy)
			{
				return AudioSource_0.enabled;
			}
			return false;
		}
	}

	public override bool IsLoop => AudioSource_0.loop;

	public override bool IsPlaying
	{
		get
		{
			if (AudioSource_0.isPlaying)
			{
				return Enabled;
			}
			return false;
		}
	}

	public GClass1495(AudioSource source, GameObject lampGo, int rolloff)
		: base(lampGo, rolloff)
	{
		AudioSource_0 = source;
		if (!MonoBehaviourSingleton<BetterAudio>.Instantiated)
		{
			Debug.LogError("Lamp Sound Controller not properly initialized, because BetterAudio not exist!");
			return;
		}
		BetterAudio instance = MonoBehaviourSingleton<BetterAudio>.Instance;
		BetterSource_0 = instance.CreateBetterSourceWithParentSettings<SimpleSource>(source, BetterAudio.AudioSourceGroupType.Lamp);
		BetterSource_0.SetRolloff(Int_1);
		BetterSource_0.SetMixerGroup(instance.EnvTechnicalSoundsGroup);
		Action_0 = GlobalEventHandlerClass.Instance.SubscribeOnEvent<GClass3576>(method_2);
	}

	public override void SetPitch(int seed, float pitch)
	{
		float pitch2 = 1f + ((float)seed / 1000f - 0.5f) * pitch;
		AudioSource_0.pitch = pitch2;
		base.SetPitch(seed, pitch);
	}

	public override void Stop()
	{
		if (!(AudioSource_0 == null))
		{
			method_1();
			AudioSource_0.Stop();
			base.Stop();
		}
	}

	public override void ContinuePlay()
	{
		if (Enabled)
		{
			AudioSource_0.Play();
			base.ContinuePlay();
		}
	}

	public override void Release()
	{
		if (BetterSource_0 != null && MonoBehaviourSingleton<BetterAudio>.Instantiated)
		{
			BetterSource_0.Release();
		}
		base.Release();
	}

	public override void OnLoopPlay(AudioClip clip, float volume)
	{
		if (!(AudioSource_0.clip == clip) || !IsPlaying)
		{
			AudioSource_0.loop = true;
			AudioSource_0.maxDistance = Int_1;
			AudioSource_0.Play();
		}
	}

	public void method_1()
	{
		AudioSourceCulling audioSourceCulling = Singleton<GameWorld>.Instance?.AudioSourceCulling;
		if (audioSourceCulling != null && AudioSource_0 != null)
		{
			audioSourceCulling.EnableRestart(AudioSource_0.GetInstanceID(), restartEnabled: false);
		}
	}

	public void method_2(GClass3576 initializedEvent)
	{
		method_3();
		method_4();
	}

	public void method_3()
	{
		if (MonoBehaviourSingleton<SpatialAudioSystem>.Instantiated && SpatialAudioSystem.Initialized)
		{
			MonoBehaviourSingleton<SpatialAudioSystem>.Instance.ProcessSourceOcclusion(GameObject_0, BetterSource_0, EOcclusionTest.ContinuousPropagated, 30000f, Vector3_0, staticPosition: true);
		}
	}

	public void method_4()
	{
		Action_0?.Invoke();
		Action_0 = null;
	}

	public override void Dispose()
	{
		method_4();
		base.Dispose();
	}

	~GClass1495()
	{
		Release();
		method_4();
	}
}
