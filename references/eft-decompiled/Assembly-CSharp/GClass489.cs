using System;
using System.Runtime.CompilerServices;
using Comfort.Common;
using EFT;
using EFT.HealthSystem;
using EFT.InventoryLogic;
using JetBrains.Annotations;
using UnityEngine;

public class GClass489 : GClass485
{
	[CompilerGenerated]
	public class Class248
	{
		public GClass489 gclass489_0;

		public Action callbackEndUse;

		public Callback<IOnHandsUseCallback> callback_0;

		public void method_0(Result<GInterface203> hands)
		{
			if (hands.Succeed)
			{
				hands.Value.SetOnUsedCallback(delegate
				{
					gclass489_0.Using = false;
					gclass489_0.BotOwner_0.WeaponManager.Selector.TakePrevWeapon();
					gclass489_0.BotOwner_0.AITaskManager.RegisterDelayedTask(gclass489_0.BotOwner_0, 1f, gclass489_0.FindDamagedPart);
					gclass489_0.RefreshMeds();
					gclass489_0.BotOwner_0.Medecine.FirstAid.CheckParts();
					gclass489_0.Float_0 = Time.time + gclass489_0.BotOwner_0.Settings.FileSettings.Mind.HEAL_DELAY_SEC;
					callbackEndUse?.Invoke();
				});
			}
		}

		public void method_1(Result<IOnHandsUseCallback> applyResult)
		{
			gclass489_0.Using = false;
			gclass489_0.BotOwner_0.WeaponManager.Selector.TakePrevWeapon();
			gclass489_0.BotOwner_0.AITaskManager.RegisterDelayedTask(gclass489_0.BotOwner_0, 1f, gclass489_0.FindDamagedPart);
			gclass489_0.RefreshMeds();
			gclass489_0.BotOwner_0.Medecine.FirstAid.CheckParts();
			gclass489_0.Float_0 = Time.time + gclass489_0.BotOwner_0.Settings.FileSettings.Mind.HEAL_DELAY_SEC;
			callbackEndUse?.Invoke();
		}
	}

	[NonSerialized]
	public bool Bool_2;

	[NonSerialized]
	[CompilerGenerated]
	public bool Bool_3;

	public bool Damaged
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

	public bool HaveWork
	{
		get
		{
			if (Damaged)
			{
				return base.HaveSmth2Use;
			}
			return false;
		}
	}

	public GClass489(BotOwner owner, Action<bool> callback)
		: base(owner, callback)
	{
	}

	public bool ShallStartUse()
	{
		if (Float_0 > Time.time)
		{
			return false;
		}
		if (BotOwner_0.WeaponManager.Grenades.ThrowindNow)
		{
			return false;
		}
		if (base.Using)
		{
			return false;
		}
		if (HaveWork)
		{
			return true;
		}
		return false;
	}

	public override void Activate()
	{
		IhealthController_0 = BotOwner_0.GetPlayer.HealthController;
		Bool_2 = BotOwner_0.Settings.FileSettings.Mind.SURGE_KIT_ONLY_SAFE_CONTAINER;
		RefreshMeds();
		base.Activate();
	}

	public void SetRandomPartToHeal()
	{
		RefreshMeds();
		FindDamagedPart();
		if (!Nullable_0.HasValue)
		{
			Nullable_0 = EBodyPart.LeftArm;
			((ActiveHealthController)BotOwner_0.HealthController)?.ApplyDamage(Nullable_0.Value, 1f, default(DamageInfoStruct));
		}
	}

	public void ApplyToCurrentPart([CanBeNull] Action callbackEndUse = null)
	{
		if (!Nullable_0.HasValue || !base.HaveSmth2Use || BotOwner_0.WeaponManager.Reload.Reloading || !BotOwner_0.GetPlayer.HealthController.CanApplyItem(base.CurUsingMeds, Nullable_0.Value))
		{
			return;
		}
		base.Using = true;
		BotOwner_0.GetPlayer.SetInHands(base.CurUsingMeds, Nullable_0.Value, GClass3380.GetRandomAnimationVariant(base.CurUsingMeds), delegate(Result<GInterface203> hands)
		{
			if (hands.Succeed)
			{
				hands.Value.SetOnUsedCallback(delegate
				{
					base.Using = false;
					BotOwner_0.WeaponManager.Selector.TakePrevWeapon();
					BotOwner_0.AITaskManager.RegisterDelayedTask(BotOwner_0, 1f, FindDamagedPart);
					RefreshMeds();
					BotOwner_0.Medecine.FirstAid.CheckParts();
					Float_0 = Time.time + BotOwner_0.Settings.FileSettings.Mind.HEAL_DELAY_SEC;
					callbackEndUse?.Invoke();
				});
			}
		});
	}

	public void Refresh()
	{
		base.CurUsingMeds = null;
		RefreshMeds();
	}

	public virtual void GetDamage()
	{
		if (!Damaged)
		{
			FindDamagedPart();
		}
	}

	public virtual void RefreshMeds()
	{
		if (!BotOwner_0.Settings.FileSettings.Mind.CAN_USE_MEDS)
		{
			return;
		}
		Player getPlayer = BotOwner_0.GetPlayer;
		EquipmentSlot[] equipmentSlots = (Bool_2 ? BotMedecine.secureSlots : BotMedecine.anySlots);
		List_0.Clear();
		getPlayer.InventoryController.GetAcceptableItemsNonAlloc(equipmentSlots, List_0);
		base.CurUsingMeds = null;
		foreach (MedsItemClass item in List_0)
		{
			if (item.HealthEffectsComponent.AffectsAny(EDamageEffectType.DestroyedPart))
			{
				base.CurUsingMeds = item;
				break;
			}
		}
	}

	public void FindDamagedPart()
	{
		if (!BotOwner_0.HealthController.IsAlive || !BotOwner_0.Settings.FileSettings.Mind.CAN_USE_MEDS)
		{
			return;
		}
		Nullable_0 = null;
		Damaged = false;
		int num = 0;
		EBodyPart eBodyPart;
		while (true)
		{
			if (num < EbodyPart_0.Length)
			{
				eBodyPart = EbodyPart_0[num];
				if (IhealthController_0.IsBodyPartDestroyed(eBodyPart))
				{
					break;
				}
				num++;
				continue;
			}
			return;
		}
		Nullable_0 = eBodyPart;
		Damaged = true;
	}
}
