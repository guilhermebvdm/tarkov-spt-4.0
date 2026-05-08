using System;
using System.Collections.Generic;
using Comfort.Common;
using EFT;
using UnityEngine;

[Serializable]
public class ServerRoomColliderArea : MonoBehaviour, IPhysicsTriggerWithStay, IPhysicsTrigger
{
	[SerializeField]
	private BoxCollider areaCollider;

	[SerializeField]
	private bool _validatePlayerCenterInside = true;

	private Dictionary<int, int> _playersTriggerCounter = new Dictionary<int, int>();

	private Dictionary<Collider, IPlayer> _playersColliders = new Dictionary<Collider, IPlayer>();

	public BoxCollider AreaCollider => areaCollider;

	public virtual string Description { get; } = "ServerRoomColliderArea";

	bool IPhysicsTrigger.enabled
	{
		get
		{
			return base.enabled;
		}
		set
		{
			base.enabled = value;
		}
	}

	public event EventHandler<GEventArgs0> OnTriggerArea;

	public void Awake()
	{
		base.gameObject.layer = LayerMaskClass.TriggersLayer;
		if ((object)areaCollider == null)
		{
			areaCollider = base.gameObject.GetComponent<BoxCollider>();
		}
	}

	public void OnTriggerEnter(Collider coll)
	{
		IPlayer player = Singleton<GameWorld>.Instance.GetAlivePlayerBridgeByCollider(coll)?.iPlayer;
		if (player != null)
		{
			_playersColliders[coll] = player;
		}
	}

	public void OnTriggerStay(Collider coll, Collider triggerCollider)
	{
		if (!_playersColliders.TryGetValue(coll, out var value))
		{
			return;
		}
		Vector3 point = value.PlayerColliderPointOnCenterAxis(0.5f);
		if (_playersTriggerCounter.TryGetValue(value.Id, out var value2))
		{
			if (value2 <= 0 && (!_validatePlayerCenterInside || GClass369.PointInOABB(point, areaCollider)) && value2 == 0)
			{
				method_0(value);
				_playersTriggerCounter[value.Id]++;
			}
		}
		else if (!_validatePlayerCenterInside || GClass369.PointInOABB(point, areaCollider))
		{
			_playersTriggerCounter.Add(value.Id, 1);
			method_0(value);
		}
	}

	void IPhysicsTriggerWithStay.OnTriggerStay(Collider coll, Collider triggerCollider)
	{
		//ILSpy generated this explicit interface implementation from .override directive in OnTriggerStay
		this.OnTriggerStay(coll, triggerCollider);
	}

	public void OnTriggerExit(Collider coll)
	{
		IPlayer player = Singleton<GameWorld>.Instance.GetAlivePlayerBridgeByCollider(coll)?.iPlayer;
		if (player == null)
		{
			return;
		}
		int id = player.Id;
		if (_playersTriggerCounter.TryGetValue(id, out var value))
		{
			value--;
			_playersTriggerCounter[id] = value;
			if (value == 0)
			{
				method_1(player);
				_playersTriggerCounter.Remove(id);
				_playersColliders.Remove(coll);
			}
		}
	}

	public void method_0(IPlayer player)
	{
		GEventArgs0 e = new GEventArgs0
		{
			TriggerEventType = GEventArgs0.ETriggerEventType.TriggerEnter,
			Player = player
		};
		this.OnTriggerArea?.Invoke(this, e);
	}

	public void method_1(IPlayer player)
	{
		GEventArgs0 e = new GEventArgs0
		{
			TriggerEventType = GEventArgs0.ETriggerEventType.TriggerExit,
			Player = player
		};
		this.OnTriggerArea?.Invoke(this, e);
	}

	public BoxCollider GetCollider()
	{
		if ((object)areaCollider == null)
		{
			areaCollider = base.gameObject.GetComponent<BoxCollider>();
			return areaCollider;
		}
		return areaCollider;
	}
}
