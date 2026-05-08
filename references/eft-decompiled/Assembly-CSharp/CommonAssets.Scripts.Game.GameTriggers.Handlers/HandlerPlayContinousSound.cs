using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Audio.SpatialSystem;
using Comfort.Common;
using EFT;
using EFT.GameTriggers;
using UnityEngine;
using UnityEngine.Audio;

namespace CommonAssets.Scripts.Game.GameTriggers.Handlers;

public class HandlerPlayContinousSound : MonoBehaviour, GInterface457
{
	[Serializable]
	public struct PlaySoundBankConfig
	{
		public SoundBank SoundBank;

		[Range(0f, 1f)]
		public float Volume;

		public BetterAudio.AudioSourceGroupType GroupType;

		public int RollOff;

		public float Pitch;

		public bool IsForceStereo;

		public EOcclusionTest OcclusionTest;

		public AudioMixerGroup MixerGroup;
	}

	[SerializeField]
	private string _playTriggerId;

	[SerializeField]
	private string _stopTriggerId;

	[SerializeField]
	private PlaySoundBankConfig _soundsConfig;

	[SerializeField]
	private Transform _overrideSoundPosition;

	[SerializeField]
	private float _playingCooldown = 1f;

	[SerializeField]
	private bool _trackListenerPosChange = true;

	private BetterSource betterSource_0;

	private float float_0;

	private Coroutine coroutine_0;

	private bool bool_0;

	private Transform transform_0;

	private Vector3 vector3_0;

	public IEnumerable<string> InputTriggerIds
	{
		get
		{
			yield return _playTriggerId;
			yield return _stopTriggerId;
		}
	}

	public IEnumerable<string> OutputTriggerIds
	{
		get
		{
			yield break;
		}
	}

	public GameObject GameObject => base.gameObject;

	public bool IsClientAuthority => false;

	public void Start()
	{
		GClass3592.Instance.Subscribe(_playTriggerId, (Action<TriggerEvent>)delegate
		{
			method_1();
		});
		GClass3592.Instance.Subscribe(_stopTriggerId, (Action<TriggerEvent>)delegate
		{
			method_3();
		});
		transform_0 = Singleton<AudioListenerConsistencyManager>.Instance.transform;
		vector3_0 = transform_0.position;
	}

	public IEnumerator method_0()
	{
		while (bool_0)
		{
			if (Time.realtimeSinceStartup >= float_0)
			{
				if (_trackListenerPosChange)
				{
					Vector3 position = transform_0.position;
					if (!GClass940.IsPositionIdentical(vector3_0, transform_0.position, 0.1f))
					{
						method_2(_soundsConfig);
						vector3_0 = position;
					}
				}
				else
				{
					method_2(_soundsConfig);
				}
				float_0 = Time.realtimeSinceStartup + _playingCooldown;
			}
			yield return null;
		}
	}

	public void method_1()
	{
		method_3();
		bool_0 = true;
		coroutine_0 = StartCoroutine(method_0());
	}

	public void method_2(PlaySoundBankConfig soundBankConfig)
	{
		AudioClip audioClip = soundBankConfig.SoundBank.PickSingleClip(0);
		if (audioClip == null)
		{
			return;
		}
		Vector3 position = base.transform.position;
		if (_overrideSoundPosition != null)
		{
			position = _overrideSoundPosition.position;
		}
		if (MonoBehaviourSingleton<BetterAudio>.Instance.TryPlayAtPoint(out var source, position, audioClip, soundBankConfig.GroupType, soundBankConfig.RollOff, soundBankConfig.Volume, EOcclusionTest.None, soundBankConfig.MixerGroup, !soundBankConfig.IsForceStereo))
		{
			betterSource_0 = source;
			betterSource_0.SetPitch(soundBankConfig.Pitch);
			betterSource_0.SetBaseVolume(soundBankConfig.Volume);
			if (soundBankConfig.OcclusionTest != EOcclusionTest.None && MonoBehaviourSingleton<SpatialAudioSystem>.Instantiated)
			{
				MonoBehaviourSingleton<SpatialAudioSystem>.Instance.ProcessSourceOcclusion(base.gameObject, source, soundBankConfig.OcclusionTest, 30000f, Vector3.up, staticPosition: true);
			}
		}
	}

	public void method_3()
	{
		bool_0 = false;
		GClass7.TryStopCoroutine(this, ref coroutine_0);
		if (!(betterSource_0 == null))
		{
			betterSource_0.Stop();
			betterSource_0.Loop = false;
			betterSource_0.Release();
		}
	}

	public void OnDisable()
	{
		method_3();
	}

	public void OnDestroy()
	{
		method_3();
	}

	public bool TryRenameId(string oldId, string newId)
	{
		if (string.IsNullOrEmpty(oldId))
		{
			return false;
		}
		if (string.IsNullOrEmpty(newId))
		{
			return false;
		}
		if (_playTriggerId.Equals(oldId))
		{
			_playTriggerId = newId;
			return true;
		}
		if (_stopTriggerId.Equals(oldId))
		{
			_stopTriggerId = newId;
			return true;
		}
		return false;
	}

	[CompilerGenerated]
	public void method_4(TriggerEvent _)
	{
		method_1();
	}

	[CompilerGenerated]
	public void method_5(TriggerEvent _)
	{
		method_3();
	}
}
