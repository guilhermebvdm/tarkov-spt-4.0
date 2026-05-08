using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using UnityEngine;

public class BotEatDrinkData : GClass429
{
	[CompilerGenerated]
	public class Class260
	{
		public Action callback;

		public BotEatDrinkData botEatDrinkData_0;

		public Callback<IOnHandsUseCallback> callback_0;

		public void method_0(Result<GInterface203> hands)
		{
			if (hands.Succeed)
			{
				hands.Value.SetOnUsedCallback(delegate
				{
					callback?.Invoke();
					botEatDrinkData_0.BotOwner_0.WeaponManager.Selector.TakePrevWeapon();
					botEatDrinkData_0.method_1();
					botEatDrinkData_0.Using = false;
					botEatDrinkData_0.method_0();
					botEatDrinkData_0.ShallStart = false;
				});
			}
		}

		public void method_1(Result<IOnHandsUseCallback> applyResult)
		{
			callback?.Invoke();
			botEatDrinkData_0.BotOwner_0.WeaponManager.Selector.TakePrevWeapon();
			botEatDrinkData_0.method_1();
			botEatDrinkData_0.Using = false;
			botEatDrinkData_0.method_0();
			botEatDrinkData_0.ShallStart = false;
		}
	}

	[NonSerialized]
	public const float START_TIME_OFFSET = 90f;

	[NonSerialized]
	public Action<bool> Callback;

	[NonSerialized]
	public List<FoodDrinkItemClass> ItemsToUse = new List<FoodDrinkItemClass>();

	[NonSerialized]
	public float NextPossibleUseTime;

	[NonSerialized]
	public bool Using_1;

	[NonSerialized]
	public bool HaveItems;

	[NonSerialized]
	public FoodDrinkItemClass FoodToUse;

	[NonSerialized]
	public IPlayer LookTo;

	[NonSerialized]
	public bool ShallStart;

	public bool Using
	{
		get
		{
			return Using_1;
		}
		set
		{
			Using_1 = value;
			Callback?.Invoke(Using_1);
		}
	}

	public BotEatDrinkData(BotOwner owner)
		: base(owner)
	{
	}

	public void StartDo(GClass413 closest)
	{
		LookTo = closest.GetAnother(BotOwner_0);
		ShallStart = true;
	}

	public void Activate()
	{
		NextPossibleUseTime = Time.time + GClass856.GreateRandom(BotOwner_0.Settings.FileSettings.Patrol.EAT_DRINK_PERIOD) + 90f;
		method_1();
	}

	public void ManualUpdate()
	{
		if (!ShallStart)
		{
			return;
		}
		if (LookTo != null && !(BotOwner_0 == null))
		{
			Vector3 point = LookTo.Position + BotOwner.STAY_HEIGHT;
			BotOwner_0.Steering.LookToPoint(point);
			BotOwner_0.StopMove();
			if (NextPossibleUseTime < Time.time)
			{
				Apply();
				method_0();
			}
		}
		else
		{
			ShallStart = false;
		}
	}

	public bool HaveEat()
	{
		return HaveItems;
	}

	public void Apply(int? varianAnim = null, Action callback = null)
	{
		if (!HaveItems || FoodToUse == null)
		{
			return;
		}
		Using = true;
		int animationVariant = (varianAnim.HasValue ? varianAnim.Value : GClass3380.GetRandomAnimationVariant(FoodToUse));
		BotOwner_0.GetPlayer.SetInHands(FoodToUse, 1f, animationVariant, delegate(Result<GInterface203> hands)
		{
			if (hands.Succeed)
			{
				hands.Value.SetOnUsedCallback(delegate
				{
					callback?.Invoke();
					BotOwner_0.WeaponManager.Selector.TakePrevWeapon();
					method_1();
					Using = false;
					method_0();
					ShallStart = false;
				});
			}
		});
	}

	public bool HaveActions()
	{
		return ShallStart;
	}

	public void method_0()
	{
		NextPossibleUseTime = Time.time + GClass856.GreateRandom(BotOwner_0.Settings.FileSettings.Patrol.EAT_DRINK_PERIOD);
	}

	public void method_1()
	{
		if (!BotOwner_0.Settings.FileSettings.Mind.CAN_USE_FOOD_DRINK)
		{
			HaveItems = false;
			return;
		}
		Player getPlayer = BotOwner_0.GetPlayer;
		EquipmentSlot[] anySlots = BotMedecine.anySlots;
		ItemsToUse.Clear();
		getPlayer.InventoryController.GetAcceptableItemsNonAlloc(anySlots, ItemsToUse);
		foreach (FoodDrinkItemClass item in ItemsToUse)
		{
			if (item != null)
			{
				FoodDrinkItemClass foodToUse = item;
				HaveItems = true;
				FoodToUse = foodToUse;
				return;
			}
		}
		HaveItems = false;
		FoodToUse = null;
	}

	public void Dispose()
	{
	}
}
