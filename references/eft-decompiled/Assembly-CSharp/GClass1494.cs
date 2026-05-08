using System;
using Audio.SpatialSystem;
using UnityEngine;

public class GClass1494 : GInterface145, IDisposable
{
	[NonSerialized]
	public const int Int_0 = 20;

	[NonSerialized]
	public GameObject GameObject_0;

	[NonSerialized]
	public int Int_1;

	[NonSerialized]
	public Vector3 Vector3_0 = new Vector3(0f, -0.5f, 0f);

	public virtual bool Enabled => true;

	public virtual bool IsPlaying => false;

	public virtual bool IsLoop => false;

	public GClass1494(GameObject lampGo, int rolloff)
	{
		GameObject_0 = lampGo;
		Int_1 = Mathf.Min(20, rolloff);
	}

	public virtual void SetPitch(int seed, float pitch)
	{
	}

	public virtual void Stop()
	{
	}

	public void Play(AudioClip clip, float volume, bool loop = false)
	{
		if (Enabled && !(clip == null))
		{
			Vector3 position = GameObject_0.transform.position + Vector3_0;
			if (!loop)
			{
				method_0(clip, position, volume);
			}
			else
			{
				OnLoopPlay(clip, volume);
			}
		}
	}

	public virtual void ContinuePlay()
	{
	}

	public virtual void Release()
	{
	}

	public virtual void OnLoopPlay(AudioClip clip, float volume)
	{
	}

	public void method_0(AudioClip clip, Vector3 position, float volume)
	{
		if (MonoBehaviourSingleton<BetterAudio>.Instantiated)
		{
			BetterAudio instance = MonoBehaviourSingleton<BetterAudio>.Instance;
			if (instance.TryPlayAtPoint(out var source, position, clip, BetterAudio.AudioSourceGroupType.Impacts, Int_1, volume, EOcclusionTest.None, instance.EnvTechnicalSoundsGroup) && MonoBehaviourSingleton<SpatialAudioSystem>.Instantiated)
			{
				MonoBehaviourSingleton<SpatialAudioSystem>.Instance.ProcessSourceOcclusion(GameObject_0, source, EOcclusionTest.OneShotPropagation, 30000f, Vector3_0, staticPosition: true);
			}
		}
	}

	public virtual void Dispose()
	{
		Stop();
		Release();
	}
}
