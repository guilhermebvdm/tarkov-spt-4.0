using System;
using System.Collections.Generic;
using Audio.AmbientSubsystem;
using Audio.SpatialSystem;
using UnityEngine;
using UnityEngine.Audio;

public class SourceOccluder : MonoBehaviour
{
	[SerializeField]
	private AudioMixerGroup _customOutputGroup;

	[SerializeField]
	private AudioSource[] _sources;

	[SerializeField]
	private bool _includeInAudioCulling = true;

	[SerializeField]
	private BetterAudio.AudioSourceGroupType _groupType = BetterAudio.AudioSourceGroupType.Environment;

	[SerializeField]
	private EOcclusionTest _occlusionTest = EOcclusionTest.ContinuousPropagated;

	[SerializeField]
	private bool _enabledLowPassFilter = true;

	[SerializeField]
	private bool _enabledHighPassFilter = true;

	[SerializeField]
	private bool _allowSourceVolumeUpdate = true;

	[SerializeField]
	private bool _enabledReverb;

	public int UpdateEveryFrames = 20;

	private int int_0;

	private readonly Dictionary<int, int> dictionary_0 = new Dictionary<int, int>();

	private BetterSource[] betterSource_0 = Array.Empty<BetterSource>();

	private SpatialAudioSystem spatialAudioSystem_0;

	private bool bool_0;

	private Action action_0;

	public void Awake()
	{
		if (_sources.Length == 0)
		{
			CollectSources();
		}
		if (SpatialAudioSystem.Initialized)
		{
			method_0();
		}
		else
		{
			action_0 = GlobalEventHandlerClass.Instance.SubscribeOnEvent<GClass3576>(method_1);
		}
	}

	public void method_0()
	{
		spatialAudioSystem_0 = MonoBehaviourSingleton<SpatialAudioSystem>.Instance;
		method_2();
		method_3();
		bool_0 = true;
	}

	public void method_1(GClass3576 initEvent)
	{
		method_0();
	}

	public void CollectSources()
	{
		_sources = GetComponentsInChildren<AudioSource>();
	}

	public void method_2()
	{
		bool flag = _customOutputGroup != null;
		bool flag2 = _includeInAudioCulling && MonoBehaviourSingleton<AmbientAudioSystem>.Instantiated;
		AudioSource[] sources = _sources;
		foreach (AudioSource audioSource in sources)
		{
			if (flag)
			{
				audioSource.outputAudioMixerGroup = _customOutputGroup;
			}
			if (flag2)
			{
				MonoBehaviourSingleton<AmbientAudioSystem>.Instance.RegisterInAudioCulling(audioSource, audioSource.loop);
			}
		}
	}

	public void method_3()
	{
		betterSource_0 = new BetterSource[_sources.Length];
		for (int i = 0; i < _sources.Length; i++)
		{
			AudioSource audioSource = _sources[i];
			SimpleSource simpleSource = MonoBehaviourSingleton<BetterAudio>.Instance.CreateBetterSourceWithParentSettings<SimpleSource>(audioSource, _groupType, _enabledLowPassFilter, _enabledHighPassFilter);
			simpleSource.UpdateVolume = _allowSourceVolumeUpdate;
			if (simpleSource.EnableSpatialization)
			{
				simpleSource.EnableReverb = _enabledReverb;
			}
			betterSource_0[i] = simpleSource;
			if (_customOutputGroup != null)
			{
				simpleSource.SetMixerGroup(_customOutputGroup);
			}
			int num = spatialAudioSystem_0.ProcessSourceOcclusion(simpleSource, _occlusionTest);
			spatialAudioSystem_0.PauseContainerUpdate(num);
			dictionary_0[audioSource.GetInstanceID()] = num;
		}
	}

	public void LateUpdate()
	{
		if (!bool_0)
		{
			return;
		}
		int_0 = (int_0 + 1) % UpdateEveryFrames;
		for (int i = int_0; i < _sources.Length; i += UpdateEveryFrames)
		{
			AudioSource audioSource = _sources[i];
			if (dictionary_0.TryGetValue(audioSource.GetInstanceID(), out var value))
			{
				if (!audioSource.isActiveAndEnabled)
				{
					spatialAudioSystem_0.PauseContainerUpdate(value);
				}
				else
				{
					spatialAudioSystem_0.ResumeContainerUpdate(value);
				}
			}
		}
	}

	public void OnDestroy()
	{
		dictionary_0.Clear();
		action_0?.Invoke();
		if (_includeInAudioCulling && MonoBehaviourSingleton<AmbientAudioSystem>.Instantiated)
		{
			AudioSource[] sources = _sources;
			foreach (AudioSource source in sources)
			{
				MonoBehaviourSingleton<AmbientAudioSystem>.Instance.RemoveFromAudioCulling(source);
			}
		}
	}
}
