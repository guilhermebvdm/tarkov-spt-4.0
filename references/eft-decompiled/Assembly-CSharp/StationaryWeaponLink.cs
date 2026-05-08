using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using EFT;
using EFT.Interactive;
using EFT.InventoryLogic;
using UnityEngine;

public class StationaryWeaponLink : MonoBehaviour, IAICorePointLink
{
	[CompilerGenerated]
	public class Class159
	{
		public Vector3 pos;

		public StationaryWeapon method_0(StationaryWeapon x1, StationaryWeapon x2)
		{
			if ((x1.OperatorPosition - pos).magnitude > (x2.OperatorPosition - pos).magnitude)
			{
				return x2;
			}
			return x1;
		}
	}

	public StationaryWeapon Weapon;

	public Vector3 InitialDir;

	public float CosAngleBase;

	private Weapon _weaponItem;

	private BotOwner _curOwner;

	private int _curOwnerId = -1;

	private float _lastDeathAtStationary = -99999f;

	[SerializeField]
	private int _corePointId;

	[SerializeField]
	private AICorePoint _corePoint;

	public int CurOwnerId => _curOwnerId;

	public bool BadLoad { get; set; }

	public AICorePoint CorePointInGame => _corePoint;

	public Vector3 Position => base.transform.position;

	public bool Init()
	{
		SetOwner(null);
		List<StationaryWeapon> list = LocationScene.GetAllObjects<StationaryWeapon>().ToList();
		if (list.Count == 0)
		{
			BadLoad = true;
			return false;
		}
		Vector3 pos = base.transform.position;
		StationaryWeapon stationaryWeapon = list.Aggregate((StationaryWeapon x1, StationaryWeapon x2) => ((x1.OperatorPosition - pos).magnitude > (x2.OperatorPosition - pos).magnitude) ? x2 : x1);
		if ((pos - stationaryWeapon.OperatorPosition).sqrMagnitude > 1f)
		{
			BadLoad = true;
			return false;
		}
		Weapon = stationaryWeapon;
		InitialDir = -GClass855.NormalizeFastSelf(Weapon.WeaponTransform.rotation * Vector3.up);
		float num = Mathf.Max(Weapon.YawLimit.x, Weapon.YawLimit.y);
		float num2 = Mathf.Min(Weapon.YawLimit.x, Weapon.YawLimit.y);
		float num3 = (num - num2) / 2f * 1.15f;
		CosAngleBase = Mathf.Cos(num3 * (MathF.PI / 180f));
		return true;
	}

	public bool IsGrenade()
	{
		return Weapon.Animation == StationaryWeapon.EStationaryAnimationType.AGS_17;
	}

	public bool IsFree(int id)
	{
		if (CurOwnerId == id)
		{
			return true;
		}
		if (Weapon.Locked)
		{
			return false;
		}
		if (CurOwnerId < 0)
		{
			return true;
		}
		return false;
	}

	public void ClearOwner()
	{
		SetOwner(null);
	}

	public void SetOwner(BotOwner bot)
	{
		_curOwner = bot;
		if (_curOwner != null)
		{
			_curOwnerId = _curOwner.Id;
		}
		else
		{
			_curOwnerId = -1;
		}
	}

	public void Dispose()
	{
		if (_curOwner != null)
		{
			_curOwnerId = -1;
			_curOwner = null;
		}
	}

	public bool HaveAmmo()
	{
		_weaponItem = Weapon.Item as Weapon;
		if (_weaponItem == null)
		{
			return false;
		}
		return _weaponItem.GetCurrentMagazineCount() > 0;
	}

	public string AmmoInfo()
	{
		_weaponItem = Weapon.Item as Weapon;
		if (_weaponItem == null)
		{
			return "_weaponItem == null";
		}
		return $"IdWeapon:{_weaponItem.Id} count:{_weaponItem.GetCurrentMagazineCount()}";
	}

	public void SetCorePoint(AICorePoint core)
	{
		_corePoint = core;
		_corePointId = _corePoint.Id;
	}

	public void Restore(AICorePointHolder holder)
	{
		if (_corePoint == null)
		{
			_corePoint = holder.GetCorePoint(_corePointId);
		}
	}

	public bool CanTakeByPrevDeath(float needPeriod)
	{
		if (_lastDeathAtStationary < 0f)
		{
			return true;
		}
		return Time.time - _lastDeathAtStationary > needPeriod;
	}

	public void DeathAtStationary()
	{
		_lastDeathAtStationary = Time.time;
	}
}
