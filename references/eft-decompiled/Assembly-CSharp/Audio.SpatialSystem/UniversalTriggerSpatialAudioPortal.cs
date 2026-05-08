using System;
using System.Collections.Generic;
using EFT.GameTriggers;
using UnityEngine;

namespace Audio.SpatialSystem;

public class UniversalTriggerSpatialAudioPortal : BaseSpatialAudioPortal, GInterface457
{
	[SerializeField]
	private string _openTriggerID;

	[SerializeField]
	private string _closeTriggerID;

	private Action action_0;

	private int int_0;

	private int int_1;

	public IEnumerable<string> InputTriggerIds
	{
		get
		{
			yield return _openTriggerID;
			yield return _closeTriggerID;
		}
	}

	public IEnumerable<string> OutputTriggerIds
	{
		get
		{
			yield break;
		}
	}

	public bool IsClientAuthority => false;

	public GameObject GameObject => base.gameObject;

	public override void OnAwake()
	{
		int_0 = GClass1298.GetStableHashCode(_openTriggerID);
		int_1 = GClass1298.GetStableHashCode(_closeTriggerID);
		base.OnAwake();
	}

	public override void Subscribe()
	{
		action_0 = GlobalEventHandlerClass.Instance.SubscribeOnEvent<TriggerEvent>(method_7);
		base.Subscribe();
	}

	public override void Unsubscribe()
	{
		action_0?.Invoke();
		action_0 = null;
		base.Unsubscribe();
	}

	public void method_7(TriggerEvent triggerEvent)
	{
		if (triggerEvent.TriggerId == int_0)
		{
			SetPortalStatus(PortalState.Open, allowFade: false);
		}
		else if (triggerEvent.TriggerId == int_1)
		{
			SetPortalStatus(PortalState.Closed, allowFade: false);
		}
	}

	public override void SyncState()
	{
		base.State = PortalState.Closed;
		base.SyncState();
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
		if (_openTriggerID.Equals(oldId))
		{
			_openTriggerID = newId;
			return true;
		}
		if (_closeTriggerID.Equals(oldId))
		{
			_closeTriggerID = newId;
			return true;
		}
		return false;
	}
}
