using System;
using EFT;
using UnityEngine;

public class DangerDataClass
{
	public int _curShootTime;

	public PlaceForCheck _place;

	public int shootTime;

	[NonSerialized]
	public float Float_0;

	public bool HaveCloseDanger
	{
		get
		{
			if (_place != null && Float_0 > Time.time)
			{
				TargetNull();
				return false;
			}
			return _place != null;
		}
	}

	public void SetTarget(PlaceForCheck place, BotOwner owner)
	{
		_place = place;
		Float_0 = Time.time + 1f;
		_curShootTime = 0;
		shootTime = UnityEngine.Random.Range(owner.Settings.FileSettings.Mind.MIN_SHOOTS_TIME, owner.Settings.FileSettings.Mind.MAX_SHOOTS_TIME + 1);
	}

	public void ShootDone()
	{
		if (_place != null)
		{
			_curShootTime++;
			if (_curShootTime >= shootTime)
			{
				_place = null;
			}
		}
	}

	public void TargetNull()
	{
		_place = null;
	}
}
