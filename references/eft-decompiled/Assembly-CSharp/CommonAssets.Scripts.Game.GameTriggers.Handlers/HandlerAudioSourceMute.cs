using System;
using System.Runtime.CompilerServices;
using System.Threading;
using EFT.GameTriggers;
using UnityEngine;

namespace CommonAssets.Scripts.Game.GameTriggers.Handlers;

public class HandlerAudioSourceMute : BaseTriggerHandler
{
	[CompilerGenerated]
	private Action<bool> action_0;

	[SerializeField]
	private bool _targetMuteState;

	[SerializeField]
	private AudioSource _audioSource;

	public event Action<bool> OnMuteStateChanged
	{
		[CompilerGenerated]
		add
		{
			Action<bool> action = action_0;
			Action<bool> action2;
			do
			{
				action2 = action;
				Action<bool> value2 = (Action<bool>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<bool> action = action_0;
			Action<bool> action2;
			do
			{
				action2 = action;
				Action<bool> value2 = (Action<bool>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public void Awake()
	{
		if (!(_audioSource == null))
		{
			_audioSource.mute = !_targetMuteState;
		}
	}

	public void ApplyState(bool state)
	{
		if (!(_audioSource == null))
		{
			_audioSource.mute = _targetMuteState;
		}
	}

	public void Start()
	{
		GClass3592.Instance.Subscribe(_triggerId, method_0);
	}

	public void method_0(TriggerEvent triggerEvent)
	{
		ApplyState(_targetMuteState);
		action_0?.Invoke(_targetMuteState);
	}
}
