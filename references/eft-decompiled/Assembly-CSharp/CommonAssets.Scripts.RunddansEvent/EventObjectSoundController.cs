using System;
using System.Runtime.CompilerServices;
using Audio.SpatialSystem;
using EFT;
using UnityEngine;

namespace CommonAssets.Scripts.RunddansEvent;

public class EventObjectSoundController : MonoBehaviour
{
	private const float float_0 = 0.1f;

	[SerializeField]
	private EventObject _eventObject;

	[SerializeField]
	private AudioSingleClipContainer _readySound;

	[SerializeField]
	private AudioSingleClipContainer _runningStartSound;

	[SerializeField]
	private AudioSingleClipContainer _runningLoopSound;

	[SerializeField]
	private AudioSingleClipContainer _stopSound;

	[SerializeField]
	private bool _spatialize;

	[SerializeField]
	private float _loopFadeTime = 0.5f;

	private BetterSource betterSource_0;

	private BetterSource betterSource_1;

	private EventObject.EState estate_0;

	private Action action_0;

	public void Awake()
	{
		_eventObject.OnStateChanged += method_3;
		action_0 = GlobalEventHandlerClass.Instance.SubscribeOnEvent<GClass3576>(method_2);
		betterSource_0 = method_0();
		betterSource_1 = method_0();
	}

	public void Start()
	{
		method_3(_eventObject.State);
	}

	public BetterSource method_0()
	{
		BetterSource source = MonoBehaviourSingleton<BetterAudio>.Instance.GetSource(BetterAudio.AudioSourceGroupType.InteractiveObjects);
		source.SetParent(base.transform, worldPositionStay: false);
		return source;
	}

	public void method_1(BetterSource source)
	{
		MonoBehaviourSingleton<SpatialAudioSystem>.Instance.ProcessSourceOcclusion(base.gameObject, source, EOcclusionTest.ContinuousPropagated, 30000f, default(Vector3), staticPosition: true);
	}

	public void method_2(GClass3576 initializedEvent)
	{
		method_1(betterSource_0);
		method_1(betterSource_1);
	}

	public void method_3(EventObject.EState state)
	{
		if (estate_0 == state)
		{
			return;
		}
		switch (state)
		{
		case EventObject.EState.Ready:
			method_4();
			break;
		case EventObject.EState.Running:
			method_5();
			break;
		default:
			if (estate_0 == EventObject.EState.Running)
			{
				method_6();
			}
			break;
		}
		estate_0 = state;
	}

	public void method_4()
	{
		if (_readySound != null)
		{
			method_7(betterSource_0, _readySound, oneShot: true);
		}
	}

	public void method_5()
	{
		if (_runningStartSound != null)
		{
			method_7(betterSource_0, _runningStartSound, oneShot: true);
		}
		if (_runningLoopSound != null)
		{
			GClass855.WaitSeconds(betterSource_1, method_8(_runningStartSound), delegate
			{
				method_7(betterSource_1, _runningLoopSound, oneShot: false, loop: true);
			});
		}
	}

	public void method_6()
	{
		if (_stopSound == null)
		{
			betterSource_1.VolumeFadeOut(_loopFadeTime, delegate
			{
				betterSource_1.source1.Stop();
			});
			return;
		}
		if (betterSource_1.PlayBackState == BetterSource.EPlayBackState.Playing && betterSource_1.Loop)
		{
			betterSource_1.VolumeFadeOut(_loopFadeTime, betterSource_1.source1.Stop);
		}
		method_7(betterSource_0, _stopSound, oneShot: true);
	}

	public void method_7(BetterSource source, IAudioClipContainer container, bool oneShot = false, bool loop = false)
	{
		float volume = container.GetVolume();
		source.Loop = loop;
		source.SetRolloff(container.GetMaxDistance());
		source.SetBaseVolume(volume);
		source.Play(container.GetClip(), null, 1f, volume, !_spatialize, oneShot);
	}

	public float method_8(IAudioClipContainer container)
	{
		if (container == null)
		{
			return 0f;
		}
		return container.GetClip().length - 0.1f;
	}

	public void OnDestroy()
	{
		method_9(ref betterSource_0);
		method_9(ref betterSource_1);
		if (_eventObject != null)
		{
			_eventObject.OnStateChanged -= method_3;
		}
		action_0?.Invoke();
		action_0 = null;
	}

	public void method_9(ref BetterSource source)
	{
		if (!(source == null))
		{
			source.Stop();
			source.StopAllCoroutines();
			source.SetParent(null);
			source.Release();
			source = null;
		}
	}

	[CompilerGenerated]
	public void method_10()
	{
		method_7(betterSource_1, _runningLoopSound, oneShot: false, loop: true);
	}

	[CompilerGenerated]
	public void method_11()
	{
		betterSource_1.source1.Stop();
	}
}
