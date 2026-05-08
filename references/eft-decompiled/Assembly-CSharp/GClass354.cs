using System;
using Comfort.Common;
using EFT;
using EFT.HealthSystem;
using EFT.Vehicle;
using UnityEngine;

public class GClass354 : BaseBrain
{
	[NonSerialized]
	public const int Int_0 = 1;

	[NonSerialized]
	public const int Int_1 = 2;

	[NonSerialized]
	public bool Bool_0;

	[NonSerialized]
	public bool Bool_1;

	public GClass354(BotOwner owner)
		: base(owner)
	{
		GClass69 layer = new GClass69(owner, 10);
		method_0(1, layer, activeOnStart: true);
		if (Owner.GetPlayer.HealthController is ActiveHealthController activeHealthController)
		{
			activeHealthController.SetDamageCoeff(0f);
		}
		Owner.GetPlayer.Loyalty.MarkAsCanBeFreeKilled();
		if (BTRControllerClass.Instance != null)
		{
			BTRControllerClass.Instance.RegisterBotShooterBTR(Owner);
		}
	}

	public void method_6()
	{
	}

	public void method_7(IPlayer aggressor)
	{
		Owner.BotsGroup.AddEnemy(aggressor, EBotEnemyCause.attackBTR);
		if (string.IsNullOrEmpty(aggressor.GroupId))
		{
			return;
		}
		foreach (Player allAlivePlayers in Singleton<GameWorld>.Instance.AllAlivePlayersList)
		{
			if (allAlivePlayers.GroupId == aggressor.GroupId)
			{
				Owner.BotsGroup.AddEnemy(allAlivePlayers, EBotEnemyCause.attackBTR);
			}
		}
	}

	public override void ManualUpdate()
	{
		if (Bool_0)
		{
			return;
		}
		method_6();
		base.ManualUpdate();
		BTRControllerClass instance = BTRControllerClass.Instance;
		if (instance == null)
		{
			return;
		}
		if (instance.BtrVehicle == null)
		{
			Bool_0 = true;
			return;
		}
		BTRVehicle btrVehicle = instance.BtrVehicle;
		EnemyInfo goalEnemy = Owner.Memory.GoalEnemy;
		if (goalEnemy != null)
		{
			if (goalEnemy.IsVisible)
			{
				btrVehicle.BTRTurret.targetPosition = goalEnemy.Person.PlayerBones.Head.position;
			}
		}
		else
		{
			btrVehicle.BTRTurret.targetPosition = Vector3.zero;
		}
	}

	public override GClass671 EventsPriority()
	{
		return null;
	}

	public override string ShortName()
	{
		return "BTR";
	}
}
