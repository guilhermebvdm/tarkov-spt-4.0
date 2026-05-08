using System;
using System.Collections.Generic;
using System.Linq;
using Comfort.Common;
using EFT;
using EFT.Interactive;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class AIPlaceInfo : MonoBehaviour, IPhysicsTrigger
{
	public int AreaId;

	public string Id;

	public ThrowGrenadePlace[] GrenadePlaces;

	public BoxCollider Collider;

	public bool IsOnTrigger = true;

	public bool BlockGrenade;

	public bool IsDark;

	public bool IsMute;

	public bool IsInside;

	public bool AtrZone;

	private bool _isInited;

	private readonly HashSet<Player> _playersInside = new HashSet<Player>();

	public AIPlaceInfoLogic InfoLogicAllEnemy;

	public ECoverPointSpecial CoversSpecial;

	private BotsController _botsController;

	public bool UseAsCoverGroupId = true;

	public string Description { get; } = "AIPlaceInfo";

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

	public event Action<Player> OnPlayerEnter;

	public event Action<Player> OnPlayerExit;

	public void Awake()
	{
		base.gameObject.layer = LayerMaskClass.TriggersLayer;
		Collider = GetComponent<BoxCollider>();
		if (Collider == null)
		{
			Debug.LogError("collider not found", base.gameObject);
			IsOnTrigger = false;
		}
		else if (!Collider.enabled)
		{
			IsOnTrigger = false;
		}
	}

	public List<ThrowGrenadePlace> CheckAllStart()
	{
		List<ThrowGrenadePlace> list = new List<ThrowGrenadePlace>();
		for (int i = 0; i < GrenadePlaces.Length; i++)
		{
			ThrowGrenadePlace throwGrenadePlace = GrenadePlaces[i];
			Refresh();
			if (!throwGrenadePlace.IsValid(Id, withSubDoor: true))
			{
				list.Add(throwGrenadePlace);
			}
		}
		return list;
	}

	public int GetCount(Func<Player, bool> isGood)
	{
		return _playersInside.Count(isGood);
	}

	public int GetCount()
	{
		return _playersInside.Count();
	}

	public IEnumerator<int> CheckAllPoints(bool shallRepeat)
	{
		for (int i = 0; i < GrenadePlaces.Length; i++)
		{
			ThrowGrenadePlace throwGrenadePlace = GrenadePlaces[i];
			Refresh();
			EDoorState? eDoorState = null;
			if (throwGrenadePlace.Door != null && throwGrenadePlace.Door.DoorState != EDoorState.Open)
			{
				eDoorState = throwGrenadePlace.Door.DoorState;
				throwGrenadePlace.Door.DoorState = EDoorState.Open;
				throwGrenadePlace.Door.OnValidate();
			}
			yield return 1;
			if (!throwGrenadePlace.IsValid(Id, withSubDoor: true) && !shallRepeat)
			{
				Debug.LogError("can't throw " + throwGrenadePlace.From?.ToString() + "   to:" + throwGrenadePlace.Target?.ToString() + "   by:" + base.gameObject.name);
			}
			yield return 1;
			if (throwGrenadePlace.Door != null && eDoorState.HasValue)
			{
				throwGrenadePlace.Door.DoorState = eDoorState.Value;
				throwGrenadePlace.Door.OnValidate();
			}
		}
		if (shallRepeat)
		{
			StartCoroutine(CheckAllPoints(shallRepeat: false));
		}
	}

	public void OnDrawGizmosSelected()
	{
		if (GrenadePlaces == null)
		{
			return;
		}
		for (int i = 0; i < GrenadePlaces.Length; i++)
		{
			ThrowGrenadePlace throwGrenadePlace = GrenadePlaces[i];
			if (throwGrenadePlace != null)
			{
				throwGrenadePlace.DrawGizmos();
			}
		}
	}

	public void Refresh()
	{
		ThrowGrenadePlace[] componentsInChildren = GetComponentsInChildren<ThrowGrenadePlace>();
		GrenadePlaces = componentsInChildren.ToArray();
	}

	public void Init(IEnumerable<Door> allDoors, BotsController botsController)
	{
		if (!_isInited)
		{
			_isInited = true;
			_botsController = botsController;
			ThrowGrenadePlace[] grenadePlaces = GrenadePlaces;
			for (int i = 0; i < grenadePlaces.Length; i++)
			{
				grenadePlaces[i].Init(allDoors);
			}
			if (InfoLogicAllEnemy != null)
			{
				InfoLogicAllEnemy.Init(this, botsController);
			}
		}
	}

	public virtual void OnEnterPlace(Player player)
	{
		_playersInside.Add(player);
		this.OnPlayerEnter?.Invoke(player);
	}

	public virtual void OnLeavePlace(Player player)
	{
		_playersInside.Remove(player);
		this.OnPlayerExit?.Invoke(player);
	}

	public void OnTriggerExit(Collider col)
	{
		if (IsOnTrigger)
		{
			Player playerByCollider = Singleton<GameWorld>.Instance.GetPlayerByCollider(col);
			if (!(playerByCollider == null) && playerByCollider.AIData != null)
			{
				playerByCollider.AIData.LeavePlace(this);
				OnLeavePlace(playerByCollider);
			}
		}
	}

	void IPhysicsTrigger.OnTriggerExit(Collider col)
	{
		//ILSpy generated this explicit interface implementation from .override directive in OnTriggerExit
		this.OnTriggerExit(col);
	}

	public void OnTriggerEnter(Collider col)
	{
		if (IsOnTrigger)
		{
			Player playerByCollider = Singleton<GameWorld>.Instance.GetPlayerByCollider(col);
			if (!(playerByCollider == null) && playerByCollider.AIData != null)
			{
				playerByCollider.AIData.AddPlaceInfo(this);
				OnEnterPlace(playerByCollider);
			}
		}
	}

	void IPhysicsTrigger.OnTriggerEnter(Collider col)
	{
		//ILSpy generated this explicit interface implementation from .override directive in OnTriggerEnter
		this.OnTriggerEnter(col);
	}

	public void OnDestroy()
	{
		if (InfoLogicAllEnemy != null)
		{
			InfoLogicAllEnemy.Dispose();
		}
		this.OnPlayerEnter = null;
		this.OnPlayerExit = null;
	}

	public virtual void EditorCheck()
	{
		if (InfoLogicAllEnemy != null)
		{
			InfoLogicAllEnemy.EditorCheck();
		}
	}
}
