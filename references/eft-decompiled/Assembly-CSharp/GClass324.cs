using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using UnityEngine;

public class GClass324 : BaseBrain
{
	[CompilerGenerated]
	public class Class232
	{
		public GClass324 gclass324_0;

		public IEnumerable<Player> allPlayers;

		public float sDistTooClose;

		public Vector3 point;

		public bool method_0(Player x)
		{
			if (x.IsAI)
			{
				return gclass324_0.Owner.EnemiesController.IsEnemy(x);
			}
			return true;
		}

		public bool method_1(GroupPoint pointToCheck)
		{
			foreach (Player item in allPlayers)
			{
				if (!((item.Position - pointToCheck.Position).sqrMagnitude >= sDistTooClose))
				{
					return false;
				}
			}
			return GClass856.SqrDistance(pointToCheck.Position, point) > sDistTooClose;
		}
	}

	[NonSerialized]
	public const int Int_0 = 1;

	[NonSerialized]
	public const int Int_1 = 2;

	[NonSerialized]
	public const int Int_2 = 3;

	[NonSerialized]
	public const int Int_3 = 5;

	[NonSerialized]
	public const int Int_4 = 6;

	[NonSerialized]
	public const int Int_5 = 7;

	[NonSerialized]
	public GClass674 Gclass674_0;

	[NonSerialized]
	public GClass98 Gclass98_0;

	[NonSerialized]
	public GClass98.EZombieMode EzombieMode_0;

	[NonSerialized]
	public Class101 Class101_0;

	public GClass98.EZombieMode CurrentZombieMode => Gclass98_0.CurrentZombieMode;

	public GClass324(BotOwner owner)
		: base(owner)
	{
		Class102 layer = new Class102(owner, 90);
		method_0(5, layer, activeOnStart: false);
		GClass85 layer2 = new GClass85(owner, 70);
		GClass140 layer3 = new GClass140(owner, 65);
		method_0(7, layer3, activeOnStart: true);
		method_0(6, layer2, activeOnStart: true);
		Gclass98_0 = new GClass98(owner, 61);
		method_0(1, Gclass98_0, activeOnStart: true);
		EzombieMode_0 = GetZombieModeByDifficulty(Owner.Profile.Info.Settings.BotDifficulty);
		Gclass98_0.SetZombieMode(EzombieMode_0);
		Class101_0 = new Class101(owner, 30);
		method_0(2, Class101_0, activeOnStart: true);
		GClass99 layer4 = new GClass99(owner, 11);
		method_0(3, layer4, activeOnStart: true);
		Owner.GameEventsData.HalloweenGameEvent.OnCallAction += method_7;
		Vector3 nearestPursuitCenter = Owner.BotsController.EventsController.BotHalloweenWithZombies.GetNearestPursuitCenter(Owner);
		method_6(nearestPursuitCenter);
		Owner.GetPlayer.MovementContext.SetCharacterMovementSpeed((EzombieMode_0 == GClass98.EZombieMode.Fast) ? 1 : 0, force: true);
		Owner.Loyalty.MarkAsCanBeFreeKilled();
	}

	public override void ManualUpdate()
	{
		base.ManualUpdate();
		if (EzombieMode_0 != GClass98.EZombieMode.Shooting)
		{
			Item item = Owner.GetPlayer.HandsController.Item;
			Item containedItem = Owner.GetPlayer.InventoryController.Inventory.Equipment.GetSlot(EquipmentSlot.Scabbard).ContainedItem;
			if (item != containedItem)
			{
				Owner.WeaponManager.Selector.ChangeToMelee();
			}
		}
	}

	public void method_6(Vector3 point)
	{
		Vector3 normalized = (Owner.Position - point).normalized;
		CustomNavigationPoint customNavigationPoint = null;
		float minSpawnDistToPlayer = Owner.BotsController.EventsController.BotHalloweenWithZombies.MinSpawnDistToPlayer;
		float sDistTooClose = minSpawnDistToPlayer * minSpawnDistToPlayer;
		IEnumerable<Player> allPlayers = Singleton<GameWorld>.Instance.AllAlivePlayersList.Where((Player x) => !x.IsAI || Owner.EnemiesController.IsEnemy(x));
		for (float num = minSpawnDistToPlayer; num <= minSpawnDistToPlayer * 3f; num += 20f)
		{
			Vector3 pos = point + normalized * num;
			customNavigationPoint = Owner.Covers.GetClosestPoint(pos, delegate(GroupPoint pointToCheck)
			{
				foreach (Player item in allPlayers)
				{
					if (!((item.Position - pointToCheck.Position).sqrMagnitude >= sDistTooClose))
					{
						return false;
					}
				}
				return GClass856.SqrDistance(pointToCheck.Position, point) > sDistTooClose;
			});
			if (customNavigationPoint != null && GClass856.SqrDistance(customNavigationPoint.Position, point) > sDistTooClose)
			{
				break;
			}
		}
		UnityEngine.Debug.DrawRay(point, Vector3.up * 88f, Color.green, 15f);
		if (customNavigationPoint != null)
		{
			UnityEngine.Debug.DrawRay(customNavigationPoint.Position, Vector3.up * 66f, Color.yellow, 15f);
			Owner.Mover.Teleport(customNavigationPoint.Position);
		}
	}

	public static GClass98.EZombieMode GetZombieModeByDifficulty(BotDifficulty difficulty)
	{
		return difficulty switch
		{
			BotDifficulty.easy => GClass98.EZombieMode.Slow, 
			BotDifficulty.normal => GClass98.EZombieMode.Fast, 
			BotDifficulty.hard => GClass98.EZombieMode.Shooting, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	public void method_7(Vector3 positionToCheck, float callRadiusSqr, GClass674 activePursuit)
	{
		if (!(GClass856.SqrDistance(Owner.GetPlayer.Position, positionToCheck) > callRadiusSqr))
		{
			Gclass674_0 = activePursuit;
			Class101_0.SetActivePursuit(Gclass674_0);
			Owner.BotsGroup.AddPointToSearch(positionToCheck, 1000f, Owner);
		}
	}

	public override string ShortName()
	{
		return Gclass98_0.CurrentZombieMode switch
		{
			GClass98.EZombieMode.Slow => "InfectedSlow", 
			GClass98.EZombieMode.Fast => "InfectedFast", 
			GClass98.EZombieMode.Shooting => "InfectedShooting", 
			_ => "Zombie[?]", 
		};
	}

	public override GClass671 EventsPriority()
	{
		return new GClass671(-1, -1, -1, -1);
	}

	public void SetCanAttack(bool canAttack)
	{
		if (canAttack)
		{
			method_3(5);
		}
		else
		{
			method_1(5);
		}
	}

	public override void Dispose()
	{
		Owner.GameEventsData.HalloweenGameEvent.OnCallAction -= method_7;
		base.Dispose();
	}
}
