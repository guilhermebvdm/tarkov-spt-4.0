using System;
using System.Runtime.CompilerServices;
using EFT;
using UnityEngine;

public class GClass179 : GClass178
{
	[CompilerGenerated]
	public class Class112
	{
		public DebugShootPractice obj;

		public Vector3 method_0()
		{
			return obj.ShootPracticeTarget.position;
		}
	}

	[CompilerGenerated]
	public class Class113
	{
		public GameObject tr;

		public Vector3 method_0()
		{
			return tr.transform.position;
		}
	}

	[NonSerialized]
	public Func<Vector3> Func_0;

	public GClass179(BotOwner bot)
		: base(bot)
	{
	}

	public override Vector3? GetTarget()
	{
		method_2();
		BotOwner_0.AimingManager.CurrentAiming.DebugDraw();
		if (Func_0 != null)
		{
			return Func_0();
		}
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy != null && goalEnemy.CanShoot)
		{
			return goalEnemy.GetPartToShoot();
		}
		return BotOwner_0.SuppressShoot.GetPoint();
	}

	public override void ReadyToShoot()
	{
		_ = (BotOwner_0.Position - Func_0()).magnitude;
	}

	public void method_2()
	{
		if (Func_0 != null)
		{
			return;
		}
		DebugShootPractice obj = UnityEngine.Object.FindObjectOfType<DebugShootPractice>();
		if (obj != null)
		{
			Func_0 = () => obj.ShootPracticeTarget.position;
		}
		else if (GameObject.Find("ShootPractice") != null)
		{
			GameObject tr = GameObject.Find("ShootPracticeTarget");
			Func_0 = () => tr.transform.position;
		}
	}
}
