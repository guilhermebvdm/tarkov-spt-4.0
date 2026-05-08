using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Comfort.Common;
using EFT;
using EFT.HealthSystem;
using EFT.InventoryLogic;
using UnityEngine;

public class BotFirstAidClass : GClass485
{
	[Serializable]
	[CompilerGenerated]
	public class Class245
	{
		public static readonly Class245 class245_0 = new Class245();

		public static Func<MedKitItemClass, bool> func_0;

		public bool method_0(MedKitItemClass kit)
		{
			return kit.HealthEffectsComponent.AffectsAny(EDamageEffectType.LightBleeding, EDamageEffectType.HeavyBleeding);
		}
	}

	[CompilerGenerated]
	public class Class246
	{
		public Action callback;

		public BotFirstAidClass BotFirstAidClass;

		public Callback<IOnHandsUseCallback> callback_0;

		public void method_0(Result<GInterface203> hands)
		{
			if (hands.Succeed)
			{
				hands.Value.SetOnUsedCallback(delegate
				{
					callback?.Invoke();
					BotFirstAidClass.BotOwner_0.WeaponManager.Selector.TakePrevWeapon();
					BotFirstAidClass.FindDamagedPart();
					BotFirstAidClass.RefreshMeds();
					BotFirstAidClass.Using = false;
					BotFirstAidClass.Float_1 = Time.time;
					BotFirstAidClass.FirstAidApplied();
					BotFirstAidClass.Float_0 = Time.time + BotFirstAidClass.BotOwner_0.Settings.FileSettings.Mind.HEAL_DELAY_SEC;
					BotFirstAidClass.action_3?.Invoke(BotFirstAidClass.BotOwner_0);
				});
			}
			else if (!BotFirstAidClass.Bool_5)
			{
				BotFirstAidClass.Bool_5 = true;
			}
		}

		public void method_1(Result<IOnHandsUseCallback> applyResult)
		{
			callback?.Invoke();
			BotFirstAidClass.BotOwner_0.WeaponManager.Selector.TakePrevWeapon();
			BotFirstAidClass.FindDamagedPart();
			BotFirstAidClass.RefreshMeds();
			BotFirstAidClass.Using = false;
			BotFirstAidClass.Float_1 = Time.time;
			BotFirstAidClass.FirstAidApplied();
			BotFirstAidClass.Float_0 = Time.time + BotFirstAidClass.BotOwner_0.Settings.FileSettings.Mind.HEAL_DELAY_SEC;
			BotFirstAidClass.action_3?.Invoke(BotFirstAidClass.BotOwner_0);
		}
	}

	[NonSerialized]
	public static EDamageEffectType[] EdamageEffectType_0 = new EDamageEffectType[2]
	{
		EDamageEffectType.HeavyBleeding,
		EDamageEffectType.LightBleeding
	};

	[NonSerialized]
	public float Float_1;

	[NonSerialized]
	public bool Bool_2;

	[NonSerialized]
	public bool Bool_3;

	[NonSerialized]
	public bool Bool_4;

	[NonSerialized]
	public bool Bool_5;

	[CompilerGenerated]
	private Action action_1;

	[CompilerGenerated]
	private Action<BotOwner> action_2;

	[CompilerGenerated]
	private Action<BotOwner> action_3;

	[NonSerialized]
	[CompilerGenerated]
	public bool Bool_6;

	public bool IsBleeding
	{
		get
		{
			if (!Bool_3)
			{
				return Bool_4;
			}
			return true;
		}
	}

	public virtual float min_percent => 0.05f;

	public float LastEndTime => Float_1;

	public bool Have2Do
	{
		get
		{
			if (!base.Using)
			{
				if (Damaged && base.HaveSmth2Use)
				{
					return method_1();
				}
				return false;
			}
			return true;
		}
	}

	public bool Damaged
	{
		[CompilerGenerated]
		get
		{
			return Bool_6;
		}
		[CompilerGenerated]
		set
		{
			Bool_6 = value;
		}
	}

	public event Action OnNoDamagedParts
	{
		[CompilerGenerated]
		add
		{
			Action action = action_1;
			Action action2;
			do
			{
				action2 = action;
				Action value2 = (Action)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_1, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action action = action_1;
			Action action2;
			do
			{
				action2 = action;
				Action value2 = (Action)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_1, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public event Action<BotOwner> OnStartApply
	{
		[CompilerGenerated]
		add
		{
			Action<BotOwner> action = action_2;
			Action<BotOwner> action2;
			do
			{
				action2 = action;
				Action<BotOwner> value2 = (Action<BotOwner>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_2, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<BotOwner> action = action_2;
			Action<BotOwner> action2;
			do
			{
				action2 = action;
				Action<BotOwner> value2 = (Action<BotOwner>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_2, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public event Action<BotOwner> OnEndApply
	{
		[CompilerGenerated]
		add
		{
			Action<BotOwner> action = action_3;
			Action<BotOwner> action2;
			do
			{
				action2 = action;
				Action<BotOwner> value2 = (Action<BotOwner>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_3, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<BotOwner> action = action_3;
			Action<BotOwner> action2;
			do
			{
				action2 = action;
				Action<BotOwner> value2 = (Action<BotOwner>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_3, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public BotFirstAidClass(BotOwner owner, Action<bool> callback)
		: base(owner, callback)
	{
	}

	public bool method_1()
	{
		if (Float_0 > Time.time)
		{
			return false;
		}
		return true;
	}

	public bool ShallStartUse()
	{
		FindDamagedPart();
		if (!method_1())
		{
			return false;
		}
		if (base.Using)
		{
			return false;
		}
		if (BotOwner_0.WeaponManager.Grenades.ThrowindNow)
		{
			return false;
		}
		if (Damaged)
		{
			FindDamagedPart();
			if (base.HaveSmth2Use)
			{
				return true;
			}
		}
		return false;
	}

	public float GetHpPercent(EBodyPart percent)
	{
		ValueStruct bodyPartHealth = IhealthController_0.GetBodyPartHealth(percent);
		return bodyPartHealth.Current / bodyPartHealth.Maximum;
	}

	public override void Activate()
	{
		IhealthController_0 = BotOwner_0.GetPlayer.HealthController;
		Bool_2 = BotOwner_0.Settings.FileSettings.Mind.MEDS_ONLY_SAFE_CONTAINER;
		RefreshMeds();
		base.Activate();
	}

	public void GetDamaged()
	{
		FindDamagedPart();
	}

	public bool IsPartDamaged(EBodyPart partType)
	{
		int num = 0;
		while (true)
		{
			if (num < EbodyPart_0.Length)
			{
				EBodyPart eBodyPart = EbodyPart_0[num];
				if (eBodyPart == partType && !(IhealthController_0.GetBodyPartHealth(eBodyPart).Normalized >= BotOwner_0.Settings.FileSettings.Mind.PART_PERCENT_TO_HEAL))
				{
					break;
				}
				num++;
				continue;
			}
			return false;
		}
		return true;
	}

	public void TryApplyToCurrentPart(int? varianAnim = null, Action callback = null)
	{
		if (!BotOwner_0.WeaponManager.Reload.Reloading && base.CurUsingMeds != null && Nullable_0.HasValue && (IsBleeding || BotOwner_0.GetPlayer.HealthController.CanApplyItem(base.CurUsingMeds, Nullable_0.Value)))
		{
			method_3(varianAnim, callback);
		}
	}

	public virtual void SetRandomPartToHeal()
	{
		RefreshMeds();
		if (!Nullable_0.HasValue)
		{
			Nullable_0 = EBodyPart.LeftArm;
			((ActiveHealthController)BotOwner_0.HealthController)?.ApplyDamage(Nullable_0.Value, 1f, default(DamageInfoStruct));
		}
	}

	public void CheckParts()
	{
		FindDamagedPart();
	}

	public void Refresh()
	{
		base.CurUsingMeds = null;
		RefreshMeds();
	}

	public virtual void FirstAidApplied()
	{
	}

	public virtual void RefreshMeds()
	{
		if (BotOwner_0.Settings.FileSettings.Mind.CAN_USE_MEDS)
		{
			Player getPlayer = BotOwner_0.GetPlayer;
			EquipmentSlot[] equipmentSlots = (Bool_2 ? BotMedecine.secureSlots : BotMedecine.anySlots);
			List_0.Clear();
			getPlayer.InventoryController.GetAcceptableItemsNonAlloc(equipmentSlots, List_0);
			MedKitItemClass medKitItemClass = null;
			List<MedKitItemClass> source = List_0.OfType<MedKitItemClass>().ToList();
			medKitItemClass = source.FirstOrDefault((MedKitItemClass kit) => kit.HealthEffectsComponent.AffectsAny(EDamageEffectType.LightBleeding, EDamageEffectType.HeavyBleeding));
			if (medKitItemClass != null)
			{
				base.CurUsingMeds = medKitItemClass;
			}
			else
			{
				base.CurUsingMeds = source.FirstOrDefault();
			}
		}
	}

	public virtual void FindDamagedPart()
	{
		if (base.CurUsingMeds == null || !BotOwner_0.HealthController.IsAlive || !BotOwner_0.Settings.FileSettings.Mind.CAN_USE_MEDS || base.CurUsingMeds == null)
		{
			return;
		}
		Bool_4 = false;
		Bool_3 = false;
		if (method_2(base.CurUsingMeds, EDamageEffectType.HeavyBleeding))
		{
			GInterface340 gInterface = IhealthController_0.FindExistingEffect<GInterface340>();
			if (gInterface != null)
			{
				Nullable_0 = gInterface.BodyPart;
				Bool_4 = true;
				Damaged = true;
				return;
			}
		}
		if (method_2(base.CurUsingMeds, EDamageEffectType.LightBleeding))
		{
			GInterface339 gInterface2 = IhealthController_0.FindExistingEffect<GInterface339>();
			if (gInterface2 != null)
			{
				Nullable_0 = gInterface2.BodyPart;
				Bool_3 = true;
				Damaged = true;
				return;
			}
		}
		int num = 0;
		EBodyPart eBodyPart;
		while (true)
		{
			if (num < EbodyPart_0.Length)
			{
				eBodyPart = EbodyPart_0[num];
				float normalized = IhealthController_0.GetBodyPartHealth(eBodyPart).Normalized;
				if (normalized < BotOwner_0.Settings.FileSettings.Mind.PART_PERCENT_TO_HEAL && !(normalized <= min_percent))
				{
					break;
				}
				num++;
				continue;
			}
			action_1?.Invoke();
			Nullable_0 = null;
			Damaged = false;
			return;
		}
		Nullable_0 = eBodyPart;
		Damaged = true;
	}

	public bool method_2(MedsItemClass med, EDamageEffectType damageEffectType)
	{
		if (med == null)
		{
			return false;
		}
		if (med.TryGetItemComponent<HealthEffectsComponent>(out var component) && component.DamageEffects.ContainsKey(damageEffectType))
		{
			return true;
		}
		return false;
	}

	public void method_3(int? varianAnim = null, Action callback = null)
	{
		base.Using = true;
		action_2?.Invoke(BotOwner_0);
		int animationVariant = (varianAnim.HasValue ? varianAnim.Value : GClass3380.GetRandomAnimationVariant(base.CurUsingMeds));
		BotOwner_0.BotTalk.Say(EPhraseTrigger.StartHeal);
		BotOwner_0.AITaskManager.RegisterDelayedTask(BotOwner_0, 30f, method_4);
		BotOwner_0.GetPlayer.SetInHands(base.CurUsingMeds, Nullable_0.Value, animationVariant, delegate(Result<GInterface203> hands)
		{
			if (hands.Succeed)
			{
				hands.Value.SetOnUsedCallback(delegate
				{
					callback?.Invoke();
					BotOwner_0.WeaponManager.Selector.TakePrevWeapon();
					FindDamagedPart();
					RefreshMeds();
					base.Using = false;
					Float_1 = Time.time;
					FirstAidApplied();
					Float_0 = Time.time + BotOwner_0.Settings.FileSettings.Mind.HEAL_DELAY_SEC;
					action_3?.Invoke(BotOwner_0);
				});
			}
			else if (!Bool_5)
			{
				Bool_5 = true;
			}
		});
	}

	public void method_4()
	{
		base.Using = false;
		BotOwner_0.WeaponManager?.Selector?.TakePrevWeapon();
		Float_0 = Time.time + BotOwner_0.Settings.FileSettings.Mind.HEAL_DELAY_SEC;
		action_3?.Invoke(BotOwner_0);
	}

	public override void Dispose()
	{
		base.Dispose();
	}
}
