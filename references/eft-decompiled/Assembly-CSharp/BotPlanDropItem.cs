using System;
using System.Linq;
using System.Runtime.CompilerServices;
using EFT;
using JetBrains.Annotations;
using UnityEngine;

public class BotPlanDropItem : GClass429
{
	[Serializable]
	[CompilerGenerated]
	public class Class266
	{
		public static readonly Class266 class266_0 = new Class266();

		public static Func<AIPlaceInfo, bool> func_0;

		public bool method_0(AIPlaceInfo x)
		{
			return x is AIPlaceWithPoint;
		}
	}

	[NonSerialized]
	public const float DROP_ITEM_PERIOD = 30f;

	[NonSerialized]
	public const float FIND_ITEM_PERIOD = 30f;

	[NonSerialized]
	public float Period;

	[NonSerialized]
	public float PeriodGo;

	[NonSerialized]
	public Vector3 PointToDrop;

	[NonSerialized]
	[CanBeNull]
	public string ItemToDrop;

	[NonSerialized]
	public float NextPossibleDropTime;

	[NonSerialized]
	public bool HavePlace;

	[NonSerialized]
	public float ComeTime;

	public BotPlanDropItem(BotOwner owner)
		: base(owner)
	{
	}

	public void Activate()
	{
		if (BotOwner_0.BotsGroup.BotGame.BotsController.CoversData.AIPlaceInfoHolder.Places == null)
		{
			HavePlace = false;
			return;
		}
		AIPlaceInfo aIPlaceInfo = BotOwner_0.BotsGroup.BotGame.BotsController.CoversData.AIPlaceInfoHolder.Places.FirstOrDefault((AIPlaceInfo x) => x is AIPlaceWithPoint);
		if (aIPlaceInfo != null)
		{
			AIPlaceWithPoint aIPlaceWithPoint = aIPlaceInfo as AIPlaceWithPoint;
			PointToDrop = aIPlaceWithPoint.Point.transform.position;
			HavePlace = true;
		}
	}

	public bool WantDropItem()
	{
		if (!HavePlace)
		{
			return false;
		}
		if (ItemToDrop != null)
		{
			return true;
		}
		if (NextPossibleDropTime < Time.time)
		{
			method_0();
		}
		return false;
	}

	public void ManualUpdate()
	{
		if (!HavePlace)
		{
			return;
		}
		if (Time.time - ComeTime < 3f)
		{
			BotOwner_0.StopMove();
		}
		else
		{
			if (ItemToDrop == null)
			{
				return;
			}
			Vector3 vector = PointToDrop - BotOwner_0.Position;
			if (vector.y < 0.5f && vector.sqrMagnitude < 1f)
			{
				ComeTime = Time.time;
				BotOwner_0.StopMove();
				method_1();
				return;
			}
			BotOwner_0.Steering.LookToMovingDirection();
			if (PeriodGo < Time.time)
			{
				PeriodGo = Time.time + 5f;
				BotOwner_0.GoToPoint(PointToDrop);
			}
		}
	}

	public void method_0()
	{
		if (BotOwner_0.ExternalItemsController.HaveItemsToDrop() && Period < Time.time)
		{
			Period = Time.time + 30f;
			ItemToDrop = BotOwner_0.ExternalItemsController.GetRandomItemToDrop();
		}
	}

	public void method_1()
	{
		BotOwner_0.ItemDropper.SetItemToDropNext(ItemToDrop);
		ItemToDrop = null;
		NextPossibleDropTime = Time.time + 30f;
		BotOwner_0.ItemDropper.TryDoDrop();
	}
}
