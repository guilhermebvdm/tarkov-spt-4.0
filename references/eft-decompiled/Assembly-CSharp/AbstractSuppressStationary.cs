using System;
using EFT;
using UnityEngine;

public abstract class AbstractSuppressStationary : GClass429
{
	[NonSerialized]
	public BotSuppressStationary SuppressStationary;

	[field: NonSerialized]
	public float EndSuppressTime { get; set; }

	[field: NonSerialized]
	public float StartArtSupress { get; set; }

	public abstract bool Usable { get; }

	public bool IsInProcess => EndSuppressTime > Time.time;

	public AbstractSuppressStationary(BotSuppressStationary suppressStationary, BotOwner bot)
		: base(bot)
	{
		SuppressStationary = suppressStationary;
	}

	public abstract bool CanStartSuppressAt(Vector3 enemyPos);

	public abstract bool CanStartSupressEnemy(EnemyInfo memoryGoalEnemy);

	public virtual bool IsReady()
	{
		if (SuppressStationary.PointToArtSuppress != null && EndSuppressTime > Time.time)
		{
			return true;
		}
		return false;
	}

	public abstract void StopExternal(Vector3 badPos);

	public void SetPeriod(float sec)
	{
		EndSuppressTime = Time.time + sec;
		StartArtSupress = Time.time;
	}

	public bool IsAtSector(Vector3 point)
	{
		if (BotOwner_0.WeaponManager.Stationary.CurLink == null)
		{
			return false;
		}
		return BotOwner_0.WeaponManager.Stationary.IsEnemyAtSector(BotOwner_0.WeaponManager.Stationary.CurLink, point);
	}

	public void Stop()
	{
		EndSuppressTime = Time.time;
	}
}
