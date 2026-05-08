using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using EFT;
using UnityEngine;

public class BotGrenadeToPortal(BotOwner owner) : GClass429(owner)
{
	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	public struct Struct10
	{
		public float d;
	}

	[NonSerialized]
	public float NextCheck = Time.time;

	[NonSerialized]
	public int IndexAng;

	[NonSerialized]
	public const float MaxThrowSDist = 2500f;

	[NonSerialized]
	public const float MaxEnemyToPortalSDist = 100f;

	[NonSerialized]
	public const float PERIOD_GRENADE_TO_PORTALS = 25f;

	[NonSerialized]
	public static List<List<AIGreandeAng>> ListOfPossibleAng = new List<List<AIGreandeAng>>
	{
		new List<AIGreandeAng>
		{
			AIGreandeAng.ang5,
			AIGreandeAng.ang25
		},
		new List<AIGreandeAng>
		{
			AIGreandeAng.ang15,
			AIGreandeAng.ang35
		},
		new List<AIGreandeAng>
		{
			AIGreandeAng.ang45,
			AIGreandeAng.ang55
		}
	};

	public void ManualUpdate()
	{
		if (!(NextCheck > Time.time))
		{
			NextCheck = Time.time + 3f;
			method_0();
		}
	}

	public void method_0()
	{
		EnemyInfo enemyInfo = BotOwner_0.Memory.GoalEnemy;
		if (enemyInfo == null && BotOwner_0.Memory.LastEnemy != null)
		{
			enemyInfo = BotOwner_0.Memory.LastEnemy;
			if (Time.time - enemyInfo.GroupInfo.EnemyLastSeenTimeSense > 120f)
			{
				return;
			}
		}
		if (enemyInfo == null || !enemyInfo.Person.AIData.roomLogicLogic.HaveRoom() || !BotOwner_0.WeaponManager.Grenades.HaveGrenade || (BotOwner_0.HasPathAndNotComplete && BotOwner_0.Mover.Sprinting))
		{
			return;
		}
		List<ISpatialPortal> portals = enemyInfo.Person.AIData.roomLogicLogic.GetPortals();
		if (portals.Count <= 0)
		{
			return;
		}
		float num = float.MaxValue;
		ISpatialPortal spatialPortal = null;
		foreach (ISpatialPortal item in portals)
		{
			if (item.IsOpen())
			{
				float sqrMagnitude = (item.Center() - BotOwner_0.Position).sqrMagnitude;
				if (sqrMagnitude < num)
				{
					num = sqrMagnitude;
					spatialPortal = item;
				}
			}
		}
		if (spatialPortal != null && !(num > 2500f) && !((spatialPortal.Center() - enemyInfo.CurrPosition).sqrMagnitude > 100f) && method_1(spatialPortal, out var aiGreanageThrowData) && BotOwner_0.BotsGroup.RequestsController.TryActivateThrowGrenadeRequest(BotOwner_0, aiGreanageThrowData, out var _))
		{
			NextCheck = Time.time + 25f;
		}
	}

	public bool method_1(ISpatialPortal portal, out AIGreanageThrowData aiGreanageThrowData)
	{
		if (!portal.IsOpen())
		{
			aiGreanageThrowData = null;
			return false;
		}
		Vector3 placeToThrow = portal.Center();
		IndexAng++;
		if (IndexAng >= ListOfPossibleAng.Count)
		{
			IndexAng = 0;
		}
		AIGreanageThrowData aIGreanageThrowData = GClass601.GetThrowTraectory(angsToTest: ListOfPossibleAng[IndexAng], placeToThrow: placeToThrow, executor: BotOwner_0, throwType: null, fromThrowPlace: BotOwner_0.Position);
		if (aIGreanageThrowData != null)
		{
			aiGreanageThrowData = aIGreanageThrowData;
			if (aIGreanageThrowData.CanThrow)
			{
				method_2(aIGreanageThrowData.Direction);
			}
			return aIGreanageThrowData.CanThrow;
		}
		aiGreanageThrowData = null;
		return false;
	}

	public void method_2(Vector3 dir)
	{
		Struct10 struct10_ = default(Struct10);
		struct10_.d = BotOwner_0.Settings.FileSettings.Grenade.GRENADE_PRECISION_PORTALS;
		Vector3 nextOffset = new Vector3(smethod_0(ref struct10_), smethod_0(ref struct10_), smethod_0(ref struct10_));
		BotOwner_0.WeaponManager.Grenades.NextThrowOffset(nextOffset);
	}

	[CompilerGenerated]
	public static float smethod_0(ref Struct10 struct10_0)
	{
		return GClass856.Random(0f - struct10_0.d, struct10_0.d);
	}
}
