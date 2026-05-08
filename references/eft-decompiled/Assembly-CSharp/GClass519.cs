using System;
using System.Runtime.CompilerServices;
using EFT;
using JetBrains.Annotations;
using UnityEngine;

public abstract class GClass519 : GClass429
{
	[NonSerialized]
	public bool Bool_0;

	[NonSerialized]
	public CustomNavigationPoint CustomNavigationPoint_0;

	[NonSerialized]
	public EnemyInfo EnemyInfo_0;

	[NonSerialized]
	public float Float_0;

	[NonSerialized]
	public Vector3? Nullable_0;

	[NonSerialized]
	public float Float_1;

	[NonSerialized]
	[CompilerGenerated]
	public bool Bool_1;

	public bool Complete
	{
		[CompilerGenerated]
		get
		{
			return Bool_1;
		}
		[CompilerGenerated]
		set
		{
			Bool_1 = value;
		}
	}

	public CustomNavigationPoint PointToSuppressFrom => CustomNavigationPoint_0;

	public GClass519([NotNull] BotOwner owner)
		: base(owner)
	{
	}

	public float DeltaLastSupperss()
	{
		return Time.time - Float_0;
	}

	public abstract void Activate();

	public bool method_0(EnemyInfo enemyToSupress, CustomNavigationPoint pos = null)
	{
		CustomNavigationPoint_0 = pos;
		Bool_0 = true;
		Complete = false;
		Float_1 = Time.time + 15f;
		EnemyInfo_0 = enemyToSupress;
		Nullable_0 = null;
		return true;
	}

	public bool method_1(Vector3 posToShoot, CustomNavigationPoint pos = null)
	{
		CustomNavigationPoint_0 = pos;
		Bool_0 = true;
		Float_1 = Time.time + 15f;
		Complete = false;
		EnemyInfo_0 = null;
		Nullable_0 = posToShoot;
		return true;
	}

	public virtual Vector3? GetPoint()
	{
		if (Nullable_0.HasValue)
		{
			return Nullable_0.Value;
		}
		BotUnderbarrelLauncherController underbarrelLauncherController = BotOwner_0.WeaponManager.UnderbarrelLauncherController;
		if (EnemyInfo_0 != null)
		{
			return underbarrelLauncherController.IsActive ? EnemyInfo_0.EnemyLastPositionReal : (EnemyInfo_0.EnemyLastPositionReal + BotOwner.STAY_HEIGHT);
		}
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy != null)
		{
			return underbarrelLauncherController.IsActive ? goalEnemy.EnemyLastPositionReal : (goalEnemy.EnemyLastPositionReal + BotOwner.STAY_HEIGHT);
		}
		EnemyInfo lastEnemy = BotOwner_0.Memory.LastEnemy;
		if (lastEnemy != null)
		{
			return underbarrelLauncherController.IsActive ? lastEnemy.EnemyLastPositionReal : (lastEnemy.EnemyLastPositionReal + BotOwner.STAY_HEIGHT);
		}
		return null;
	}

	public void method_2(float delta)
	{
		Complete = true;
		Float_0 = Time.time;
		EnemyInfo_0?.SetSuppressEndTime(Float_0 + delta);
		Bool_0 = false;
	}

	public abstract void Dispose();
}
