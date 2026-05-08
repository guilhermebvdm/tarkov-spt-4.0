using System;
using Comfort.Common;
using EFT;
using UnityEngine;

public class BotSuppressStationary : GClass429
{
	[NonSerialized]
	public GClass518 NoSupress;

	[NonSerialized]
	public GClass517 MgSuppress;

	[NonSerialized]
	public GClass516 ArtillerySuppress;

	[NonSerialized]
	public EnemyInfo EnemyInfo;

	[field: NonSerialized]
	public Func<Vector3> PointToArtSuppress { get; set; }

	[field: NonSerialized]
	public AbstractSuppressStationary CurUsingLogic { get; set; }

	[field: NonSerialized]
	public float LastHitTargetTime { get; set; }

	public BotSuppressStationary(BotOwner owner)
		: base(owner)
	{
		NoSupress = new GClass518(this, owner);
		MgSuppress = new GClass517(this, owner);
		ArtillerySuppress = new GClass516(this, owner);
		CurUsingLogic = NoSupress;
	}

	public void Activate()
	{
		BotOwner_0.WeaponManager.Stationary.OnLinkedWeaponChange += method_0;
	}

	public void method_0(StationaryWeaponLink obj)
	{
		if (obj == null)
		{
			CurUsingLogic = NoSupress;
		}
		else if (obj.IsGrenade())
		{
			CurUsingLogic = ArtillerySuppress;
		}
		else
		{
			CurUsingLogic = MgSuppress;
		}
	}

	public void SetTargetGetter(Func<Vector3> pointToArtSuppress)
	{
		method_1();
		EnemyInfo = null;
		PointToArtSuppress = pointToArtSuppress;
	}

	public void SetTargetGetter(Func<Vector3> pointToArtSuppress, EnemyInfo enemyInfo)
	{
		method_1();
		EnemyInfo = enemyInfo;
		PointToArtSuppress = pointToArtSuppress;
	}

	public void method_1()
	{
		if (EnemyInfo != null)
		{
			Singleton<GameWorld>.Instance.GetAlivePlayerByProfileID(EnemyInfo.Person.ProfileId).BeingHitAction -= method_2;
		}
	}

	public void Dispose()
	{
		method_1();
	}

	public void method_2(DamageInfoStruct damageInfo, EBodyPart arg2, float arg3)
	{
		if (damageInfo.Player != null && damageInfo.Player.iPlayer.Id == BotOwner_0.GetPlayer.Id)
		{
			LastHitTargetTime = Time.time;
		}
	}

	public bool HaveEnemy()
	{
		return EnemyInfo != null;
	}
}
