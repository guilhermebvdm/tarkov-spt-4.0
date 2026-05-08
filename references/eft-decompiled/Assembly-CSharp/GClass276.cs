using System;
using EFT;
using UnityEngine;
using UnityEngine.AI;

public class GClass276 : GClass177<GClass28>
{
	public bool CanLay = true;

	[NonSerialized]
	public GClass178 Gclass178_0;

	[NonSerialized]
	public GClass274 Gclass274_0;

	[NonSerialized]
	public float Float_0;

	[NonSerialized]
	public float Float_1;

	[NonSerialized]
	public float Float_2;

	[NonSerialized]
	public float Float_3;

	[NonSerialized]
	public float Float_4 = 0.7f;

	[NonSerialized]
	public float Float_5 = 2.89f;

	[NonSerialized]
	public int Int_0 = 6;

	public GClass276(BotOwner bot)
		: base(bot)
	{
		Gclass274_0 = new GClass274(bot);
		Gclass178_0 = new GClass183(bot);
	}

	public override void UpdateNodeByBrain(GClass28 data)
	{
		method_1(data);
		method_0();
	}

	public void method_0()
	{
		if (Float_2 > Time.time)
		{
			return;
		}
		Float_2 = Time.time + 2f;
		if (BotOwner_0.BotsGroup.MembersCount <= 1)
		{
			return;
		}
		for (int i = 0; i < Mathf.Min(BotOwner_0.BotsGroup.MembersCount, Int_0); i++)
		{
			BotOwner botOwner = BotOwner_0.BotsGroup.Member(i);
			if (botOwner.Id == BotOwner_0.Id)
			{
				continue;
			}
			Vector3 vector = BotOwner_0.Position - botOwner.Position;
			if (!(vector.sqrMagnitude < Float_5))
			{
				continue;
			}
			Vector3 vector3;
			if (BotOwner_0.Memory.HaveEnemy)
			{
				Vector3 vector2 = GClass855.Rotate90(BotOwner_0.Memory.GoalEnemy.Direction, GClass855.SideTurn.left);
				if (Vector3.Dot(vector2, vector) > 0f)
				{
					vector2 = -vector2;
				}
				vector3 = -GClass855.NormalizeFastSelf(vector2);
			}
			else
			{
				vector3 = GClass855.NormalizeFastSelf(vector);
			}
			Vector3 position = BotOwner_0.Position + vector3 * Float_4;
			BotOwner_0.Sprint(val: false);
			if (BotOwner_0.GoToPoint(position, slowAtTheEnd: false, 0.1f, getUpWithCheck: true, mustHaveWay: false, mustGetUp: true, onlyShortTrie: true) == NavMeshPathStatus.PathComplete)
			{
				Float_3 = Time.time + 1.5f;
			}
		}
	}

	public void method_1(GClass28 data)
	{
		if (BotOwner_0.Memory.GoalEnemy == null)
		{
			return;
		}
		if (BotOwner_0.WeaponManager.IsMelee)
		{
			BotOwner_0.WeaponManager.Selector.ChangeToMain();
		}
		if (BotOwner_0.BotLay.IsLay && Float_0 < Time.time)
		{
			BotOwner_0.BotLay.CheckGetUp();
		}
		if (Float_3 < Time.time)
		{
			BotOwner_0.StopMove();
		}
		Vector3 partToShoot = BotOwner_0.Memory.GoalEnemy.GetPartToShoot();
		BotOwner_0.ShootFromPlace.CheckTarget(partToShoot);
		if (CanLay && BotOwner_0.ShootFromPlace.CanShootLay)
		{
			if (BotOwner_0.BotLay.IsLay)
			{
				method_3(data);
				return;
			}
			if (BotOwner_0.BotsGroup.GroupLaying.CanLay() && BotOwner_0.BotLay.TryLay())
			{
				Float_0 = Time.time + 3f;
				method_3(data);
				return;
			}
		}
		if (BotOwner_0.BotLay.IsLay)
		{
			if (!BotOwner_0.ShootFromPlace.CanShootLay && Float_0 < Time.time)
			{
				BotOwner_0.BotLay.GetUp(withCheck: true);
			}
		}
		else if (BotOwner_0.Settings.FileSettings.Grenade.CAN_THROW_FROM_ANY_PLACE && Gclass274_0.UpdateTryThrow())
		{
			return;
		}
		if (Float_1 < Time.time)
		{
			Float_1 = Time.time + 2f;
			if (BotOwner_0.ShootFromPlace.CanShootSit)
			{
				BotOwner_0.Mover.SetPose(0f);
			}
			else if (method_2())
			{
				BotOwner_0.Mover.SetPose(1f);
			}
		}
		BotUnderbarrelLauncherController underbarrelLauncherController = BotOwner_0.WeaponManager.UnderbarrelLauncherController;
		if (underbarrelLauncherController.IsActive)
		{
			if (underbarrelLauncherController.NeedToReload())
			{
				underbarrelLauncherController.TryReload();
				return;
			}
		}
		else
		{
			if (underbarrelLauncherController.CanSwitchInFight(BotOwner_0))
			{
				underbarrelLauncherController.TryEnable();
			}
			if (!BotOwner_0.WeaponManager.HaveBullets)
			{
				BotOwner_0.WeaponManager.Reload.TryReload();
				return;
			}
		}
		method_3(data);
	}

	public bool method_2()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy == null)
		{
			return true;
		}
		if (goalEnemy.CanShoot && goalEnemy.IsVisible)
		{
			return false;
		}
		return true;
	}

	public void method_3(GClass28 data)
	{
		if (BotOwner_0.WeaponManager.UnderbarrelLauncherController.IsActive)
		{
			Gclass178_0.UpdateNodeByBrain(data);
		}
		else if (BotOwner_0.Memory.GoalEnemy.CanShoot && BotOwner_0.Memory.GoalEnemy.IsVisible)
		{
			Gclass178_0.UpdateNodeByBrain(data);
		}
		else
		{
			BotOwner_0.Steering.LookToPoint(BotOwner_0.Memory.GoalEnemy.CurrPosition + Vector3.up);
		}
	}
}
