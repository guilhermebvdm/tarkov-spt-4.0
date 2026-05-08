using System;
using System.Runtime.CompilerServices;
using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using UnityEngine;

public class GClass491 : GClass484
{
	[CompilerGenerated]
	public class Class247
	{
		public GClass491 gclass491_0;

		public Action<bool> callback;

		public Callback<IOnHandsUseCallback> callback_0;

		public void method_0(Result<GInterface203> hands)
		{
			if (!hands.Succeed)
			{
				return;
			}
			hands.Value.SetOnUsedCallback(delegate(Result<IOnHandsUseCallback> applyResult)
			{
				gclass491_0.Using = false;
				gclass491_0.LastEndUseTime = Time.time;
				gclass491_0.BotOwner_0.WeaponManager.Selector.TakePrevWeapon();
				gclass491_0.method_1();
				gclass491_0.Float_0 = Time.time + gclass491_0.BotOwner_0.Settings.FileSettings.Mind.FOOD_DRINK_DELAY_SEC;
				if (applyResult.Succeed)
				{
					callback?.Invoke(obj: true);
				}
			});
		}

		public void method_1(Result<IOnHandsUseCallback> applyResult)
		{
			gclass491_0.Using = false;
			gclass491_0.LastEndUseTime = Time.time;
			gclass491_0.BotOwner_0.WeaponManager.Selector.TakePrevWeapon();
			gclass491_0.method_1();
			gclass491_0.Float_0 = Time.time + gclass491_0.BotOwner_0.Settings.FileSettings.Mind.FOOD_DRINK_DELAY_SEC;
			if (applyResult.Succeed)
			{
				callback?.Invoke(obj: true);
			}
		}
	}

	[NonSerialized]
	public StimulatorItemClass StimulatorItemClass;

	[NonSerialized]
	public bool Bool_2;

	[NonSerialized]
	[CompilerGenerated]
	public bool Bool_3;

	[NonSerialized]
	[CompilerGenerated]
	public float Float_1;

	[NonSerialized]
	[CompilerGenerated]
	public float Float_2;

	public bool HaveSmt
	{
		[CompilerGenerated]
		get
		{
			return Bool_3;
		}
		[CompilerGenerated]
		set
		{
			Bool_3 = value;
		}
	}

	public float LastUseTime
	{
		[CompilerGenerated]
		get
		{
			return Float_1;
		}
		[CompilerGenerated]
		set
		{
			Float_1 = value;
		}
	}

	public float LastEndUseTime
	{
		[CompilerGenerated]
		get
		{
			return Float_2;
		}
		[CompilerGenerated]
		set
		{
			Float_2 = value;
		}
	}

	public GClass491(BotOwner owner, Action<bool> callback)
		: base(owner, callback)
	{
	}

	public void StartApplyToTarget(Action<bool> callback = null)
	{
		TryApply(noCheckDelay: true, null, callback);
	}

	public override void Activate()
	{
		Bool_2 = BotOwner_0.Settings.FileSettings.Mind.MEDS_ONLY_SAFE_CONTAINER;
		method_1();
		base.Activate();
	}

	public bool CanUseNow()
	{
		if (BotOwner_0.WeaponManager.Grenades.ThrowindNow)
		{
			return false;
		}
		return Float_0 < Time.time;
	}

	public void TryApply(bool noCheckDelay = false, int? animVarian = null, Action<bool> callback = null)
	{
		if (!BotOwner_0.WeaponManager.Reload.Reloading && !base.Using && StimulatorItemClass != null)
		{
			if (!CanUseNow() && !noCheckDelay)
			{
				callback?.Invoke(obj: false);
				return;
			}
			base.Using = true;
			LastUseTime = Time.time;
			BotOwner_0.BotTalk.Say(EPhraseTrigger.StartHeal);
			int animationVariant = (animVarian.HasValue ? animVarian.Value : GClass3380.GetRandomAnimationVariant(StimulatorItemClass));
			BotOwner_0.GetPlayer.SetInHands(StimulatorItemClass, EBodyPart.Chest, animationVariant, delegate(Result<GInterface203> hands)
			{
				if (hands.Succeed)
				{
					hands.Value.SetOnUsedCallback(delegate(Result<IOnHandsUseCallback> applyResult)
					{
						base.Using = false;
						LastEndUseTime = Time.time;
						BotOwner_0.WeaponManager.Selector.TakePrevWeapon();
						method_1();
						Float_0 = Time.time + BotOwner_0.Settings.FileSettings.Mind.FOOD_DRINK_DELAY_SEC;
						if (applyResult.Succeed)
						{
							callback?.Invoke(obj: true);
						}
					});
				}
			});
		}
		else
		{
			callback?.Invoke(obj: false);
		}
	}

	public void Refresh()
	{
		StimulatorItemClass = null;
		HaveSmt = false;
		method_1();
	}

	public void method_1()
	{
		if (!BotOwner_0.Settings.FileSettings.Mind.CAN_USE_MEDS)
		{
			return;
		}
		Player getPlayer = BotOwner_0.GetPlayer;
		EquipmentSlot[] equipmentSlots = (Bool_2 ? BotMedecine.secureSlots : BotMedecine.anySlots);
		List_0.Clear();
		getPlayer.InventoryController.GetAcceptableItemsNonAlloc(equipmentSlots, List_0);
		foreach (MedsItemClass item in List_0)
		{
			if (item is StimulatorItemClass stimulatorItemClass)
			{
				StimulatorItemClass = stimulatorItemClass;
				HaveSmt = true;
				return;
			}
		}
		HaveSmt = false;
	}
}
