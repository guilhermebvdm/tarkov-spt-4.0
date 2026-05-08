using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using Audio.AudioCulling;
using CommonAssets.Scripts.Game.GameTriggers.Handlers;
using EFT.GameTriggers;
using UnityEngine;

namespace CommonAssets.Scripts.Game;

public class SyncableLoopSoundPlayer : HandlerStateBase, GInterface457
{
	[SerializeField]
	private string _playTriggerId;

	[SerializeField]
	private string _stopTriggerId;

	[SerializeField]
	private SyncLoopSoundPlayer _loopSoundPlayer;

	[CompilerGenerated]
	private Action<bool> action_0;

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

	public override event Action<bool> StateChanged
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

	public void Start()
	{
		GClass3592.Instance.Subscribe(_playTriggerId, (Action<TriggerEvent>)delegate
		{
			method_0();
		});
		GClass3592.Instance.Subscribe(_stopTriggerId, (Action<TriggerEvent>)delegate
		{
			method_1();
		});
	}

	public void method_0()
	{
		_loopSoundPlayer.Play();
		action_0?.Invoke(obj: true);
	}

	public void method_1()
	{
		_loopSoundPlayer.Stop();
		action_0?.Invoke(obj: false);
	}

	public override void ApplyState(bool state)
	{
		if (state)
		{
			method_0();
		}
		else
		{
			method_1();
		}
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
	public void method_2(TriggerEvent _)
	{
		method_0();
	}

	[CompilerGenerated]
	public void method_3(TriggerEvent _)
	{
		method_1();
	}
}
