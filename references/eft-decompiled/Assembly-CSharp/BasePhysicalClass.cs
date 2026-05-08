using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Comfort.Common;
using EFT;
using EFT.NextObservedPlayer;
using UnityEngine;

public class BasePhysicalClass
{
	public interface IObserverToPlayerBridge
	{
		IPlayer iPlayer { get; }

		SkillManager Skills { get; }

		float TotalWeight { get; }

		event Action OnTotalWeightUpdated;

		event Action<EPhysicalCondition, EPhysicalCondition> OnPhysicalConditionChanged;

		bool PhysicalConditionContainsAny(EPhysicalCondition c);
	}

	public class PlayerBridge : IObserverToPlayerBridge
	{
		[CompilerGenerated]
		private Action action_0;

		[CompilerGenerated]
		private Action<EPhysicalCondition, EPhysicalCondition> action_1;

		[NonSerialized]
		public Player Player_0;

		public IPlayer iPlayer => Player_0;

		public SkillManager Skills => Player_0.Skills;

		public float TotalWeight => Player_0.Skills.StrengthBuffElite ? Player_0.InventoryController.Inventory.TotalWeightEliteSkill : Player_0.InventoryController.Inventory.TotalWeight;

		public event Action OnTotalWeightUpdated
		{
			[CompilerGenerated]
			add
			{
				Action action = action_0;
				Action action2;
				do
				{
					action2 = action;
					Action value2 = (Action)Delegate.Combine(action2, value);
					action = Interlocked.CompareExchange(ref action_0, value2, action2);
				}
				while ((object)action != action2);
			}
			[CompilerGenerated]
			remove
			{
				Action action = action_0;
				Action action2;
				do
				{
					action2 = action;
					Action value2 = (Action)Delegate.Remove(action2, value);
					action = Interlocked.CompareExchange(ref action_0, value2, action2);
				}
				while ((object)action != action2);
			}
		}

		public event Action<EPhysicalCondition, EPhysicalCondition> OnPhysicalConditionChanged
		{
			[CompilerGenerated]
			add
			{
				Action<EPhysicalCondition, EPhysicalCondition> action = action_1;
				Action<EPhysicalCondition, EPhysicalCondition> action2;
				do
				{
					action2 = action;
					Action<EPhysicalCondition, EPhysicalCondition> value2 = (Action<EPhysicalCondition, EPhysicalCondition>)Delegate.Combine(action2, value);
					action = Interlocked.CompareExchange(ref action_1, value2, action2);
				}
				while ((object)action != action2);
			}
			[CompilerGenerated]
			remove
			{
				Action<EPhysicalCondition, EPhysicalCondition> action = action_1;
				Action<EPhysicalCondition, EPhysicalCondition> action2;
				do
				{
					action2 = action;
					Action<EPhysicalCondition, EPhysicalCondition> value2 = (Action<EPhysicalCondition, EPhysicalCondition>)Delegate.Remove(action2, value);
					action = Interlocked.CompareExchange(ref action_1, value2, action2);
				}
				while ((object)action != action2);
			}
		}

		public bool PhysicalConditionContainsAny(EPhysicalCondition c)
		{
			return Player_0.MovementContext.PhysicalConditionContainsAny(c);
		}

		public PlayerBridge(Player player)
		{
			Player_0 = player;
			action_0 = null;
			action_1 = null;
			player.InventoryController.Inventory.OnWeightUpdated.Subscribe(method_0);
			player.MovementContext.PhysicalConditionChanged += method_1;
		}

		public void method_0()
		{
			action_0?.Invoke();
		}

		public void method_1(EPhysicalCondition adddedOrRemoved, EPhysicalCondition result)
		{
			action_1?.Invoke(adddedOrRemoved, result);
		}
	}

	public class ObserverBridge : IObserverToPlayerBridge
	{
		[CompilerGenerated]
		private Action action_0;

		[CompilerGenerated]
		private Action<EPhysicalCondition, EPhysicalCondition> action_1;

		[NonSerialized]
		public ObservedPlayerView ObservedPlayerView_0;

		public IPlayer iPlayer => ObservedPlayerView_0;

		public SkillManager Skills => null;

		public float TotalWeight => 0f;

		public event Action OnTotalWeightUpdated
		{
			[CompilerGenerated]
			add
			{
				Action action = action_0;
				Action action2;
				do
				{
					action2 = action;
					Action value2 = (Action)Delegate.Combine(action2, value);
					action = Interlocked.CompareExchange(ref action_0, value2, action2);
				}
				while ((object)action != action2);
			}
			[CompilerGenerated]
			remove
			{
				Action action = action_0;
				Action action2;
				do
				{
					action2 = action;
					Action value2 = (Action)Delegate.Remove(action2, value);
					action = Interlocked.CompareExchange(ref action_0, value2, action2);
				}
				while ((object)action != action2);
			}
		}

		public event Action<EPhysicalCondition, EPhysicalCondition> OnPhysicalConditionChanged
		{
			[CompilerGenerated]
			add
			{
				Action<EPhysicalCondition, EPhysicalCondition> action = action_1;
				Action<EPhysicalCondition, EPhysicalCondition> action2;
				do
				{
					action2 = action;
					Action<EPhysicalCondition, EPhysicalCondition> value2 = (Action<EPhysicalCondition, EPhysicalCondition>)Delegate.Combine(action2, value);
					action = Interlocked.CompareExchange(ref action_1, value2, action2);
				}
				while ((object)action != action2);
			}
			[CompilerGenerated]
			remove
			{
				Action<EPhysicalCondition, EPhysicalCondition> action = action_1;
				Action<EPhysicalCondition, EPhysicalCondition> action2;
				do
				{
					action2 = action;
					Action<EPhysicalCondition, EPhysicalCondition> value2 = (Action<EPhysicalCondition, EPhysicalCondition>)Delegate.Remove(action2, value);
					action = Interlocked.CompareExchange(ref action_1, value2, action2);
				}
				while ((object)action != action2);
			}
		}

		public bool PhysicalConditionContainsAny(EPhysicalCondition c)
		{
			return ObservedPlayerView_0.ObservedPlayerController.HealthController.PhysicalConditionContainsAny(c);
		}

		public ObserverBridge(ObservedPlayerView player)
		{
			ObservedPlayerView_0 = player;
			action_0 = null;
			action_1 = null;
			ObservedPlayerView_0.ObservedPlayerController.HealthController.OnPhysicalConditionChanged += method_1;
		}

		public void method_0()
		{
			action_0?.Invoke();
		}

		public void method_1(EPhysicalCondition adddedOrRemoved, EPhysicalCondition result)
		{
			action_1?.Invoke(adddedOrRemoved, result);
		}
	}

	public struct PhysicalStateStruct
	{
		public bool StaminaExhausted;

		public bool OxygenExhausted;

		public bool HandsExhausted;

		public bool Equals(PhysicalStateStruct other)
		{
			if (other.StaminaExhausted == StaminaExhausted && other.OxygenExhausted == OxygenExhausted)
			{
				return other.HandsExhausted == HandsExhausted;
			}
			return false;
		}
	}

	[CompilerGenerated]
	public class Class438
	{
		public BasePhysicalClass BasePhysicalClass;

		public Action unsub;

		public float method_0()
		{
			return GClass2298.Evaluate(BasePhysicalClass.StaminaParameters.PoseLevelIncreaseSpeed, Mathf.Max(BasePhysicalClass.Overweight, BasePhysicalClass.Stamina.Exhausted ? 0.9f : 0f));
		}

		public float method_1()
		{
			return GClass2298.Evaluate(BasePhysicalClass.StaminaParameters.PoseLevelDecreaseSpeed, Mathf.Max(BasePhysicalClass.Overweight, BasePhysicalClass.Stamina.Exhausted ? 0.9f : 0f));
		}

		public void method_2()
		{
			BasePhysicalClass.IobserverToPlayerBridge_0.OnPhysicalConditionChanged -= BasePhysicalClass.PhysicalConditionChanged;
			unsub();
			BasePhysicalClass.IobserverToPlayerBridge_0.OnTotalWeightUpdated -= BasePhysicalClass.method_1;
			BasePhysicalClass.IobserverToPlayerBridge_0.iPlayer.HealthController.EffectStartedEvent -= BasePhysicalClass.method_6;
			BasePhysicalClass.IobserverToPlayerBridge_0.iPlayer.HealthController.EffectResidualEvent -= BasePhysicalClass.method_6;
			BasePhysicalClass.Stamina.OnThresholdPass -= BasePhysicalClass.method_8;
			BasePhysicalClass.TransitionSpeed.Dispose();
			BasePhysicalClass.MinStepSound.Dispose();
		}

		public float method_3()
		{
			if (!BasePhysicalClass.IobserverToPlayerBridge_0.PhysicalConditionContainsAny(EPhysicalCondition.LeftLegDamaged | EPhysicalCondition.RightLegDamaged))
			{
				return 0.6f * BasePhysicalClass.Overweight;
			}
			return 0.6f;
		}

		public float method_4()
		{
			return GClass2298.Evaluate(BasePhysicalClass.StaminaParameters.TransitionSpeed, Mathf.Max(BasePhysicalClass.Overweight, BasePhysicalClass.Stamina.Exhausted ? 1 : 0));
		}

		public void method_5(GClass3546 calledEvent)
		{
			BasePhysicalClass.method_0(calledEvent);
		}
	}

	[CompilerGenerated]
	public class Class439
	{
		public BasePhysicalClass BasePhysicalClass;

		public Action updateBreathStatus;

		public void method_0()
		{
			BasePhysicalClass.Oxygen.OnValueChanged -= updateBreathStatus;
			BasePhysicalClass.HoldBreathChanged -= updateBreathStatus;
		}

		public void method_1()
		{
			BasePhysicalClass.Stamina.OnValueChanged -= updateBreathStatus;
		}
	}

	[NonSerialized]
	public const float Float_0 = 0.6f;

	[NonSerialized]
	public IObserverToPlayerBridge IobserverToPlayerBridge_0;

	public float MaxPoseLevel = 1f;

	public float Overweight;

	public float WalkOverweight;

	public float WalkSpeedLimit = 1f;

	public float Inertia;

	public float MoveSideInertia;

	public float MoveDiagonalInertia;

	public GClass849<float> MinStepSound;

	public GClass849<float> TransitionSpeed;

	public GClass849<float> PoseLevelIncreaseSpeed;

	public GClass849<float> PoseLevelDecreaseSpeed;

	public float SoundRadius = 1f;

	[NonSerialized]
	public bool Bool_0;

	[NonSerialized]
	public float Float_1;

	public float Fatigue;

	public GClass774 Stamina;

	public GClass774 HandsStamina;

	public GClass774 Oxygen;

	public float CapacityBuff;

	[NonSerialized]
	public float Float_2;

	public float FallDamageMultiplier = 1f;

	public float StaminaCapacity = Singleton<BackendConfigSettingsClass>.Instance.Stamina.Capacity;

	public float StaminaRestoreRate = Singleton<BackendConfigSettingsClass>.Instance.Stamina.BaseRestorationRate;

	public float HandsCapacity = Singleton<BackendConfigSettingsClass>.Instance.Stamina.HandsCapacity;

	public float HandsRestoreRate = Singleton<BackendConfigSettingsClass>.Instance.Stamina.HandsRestoration;

	public float OxygenCapacity = Singleton<BackendConfigSettingsClass>.Instance.Stamina.OxygenCapacity;

	public float OxygenRestoreRate = Singleton<BackendConfigSettingsClass>.Instance.Stamina.OxygenRestoration;

	public Vector2 WalkOverweightLimits;

	public Vector2 BaseOverweightLimits;

	public Vector3 BaseInertiaLimits;

	public Vector2 SprintOverweightLimits;

	public Vector2 WalkSpeedOverweightLimits;

	[NonSerialized]
	public float Float_3;

	public float PreviousWeight;

	[NonSerialized]
	public bool Bool_1;

	[NonSerialized]
	public bool Bool_2;

	[NonSerialized]
	public PhysicalStateStruct PhysicalStateStruct;

	[NonSerialized]
	public bool Bool_3 = true;

	[CompilerGenerated]
	private Action<bool> action_0;

	[CompilerGenerated]
	private Action action_1;

	[CompilerGenerated]
	private Action action_2;

	[CompilerGenerated]
	private Action<bool> action_3;

	[CompilerGenerated]
	private Action<bool> action_4;

	[CompilerGenerated]
	private Action action_5;

	[NonSerialized]
	[CompilerGenerated]
	public bool Bool_4;

	public float SprintAcceleration;

	public float PreSprintAcceleration;

	[NonSerialized]
	public bool Bool_5;

	[NonSerialized]
	public bool Bool_6;

	[NonSerialized]
	public Action Action_6;

	[NonSerialized]
	public bool Bool_7;

	public float RestoreRateBuff
	{
		get
		{
			return Float_2;
		}
		set
		{
			Float_2 = value;
			Stamina.BuffRestoration = Float_2;
			HandsStamina.BuffRestoration = Float_2 / 2f;
			Oxygen.BuffRestoration = Float_2;
		}
	}

	public virtual bool Exhausted => false;

	public virtual bool CanSprint => true;

	public virtual bool Sprinting => Bool_2;

	public virtual bool HoldingBreath => false;

	public virtual bool CanJump => true;

	public virtual bool CanVault => true;

	public virtual bool CanClimb => true;

	public virtual bool CanMeleeHit => true;

	public virtual float FastWeaponSwitchStaminaLack => 0f;

	public bool CanFatigue => Bool_3;

	public BackendConfigSettingsClass.GClass1736 StaminaParameters => Singleton<BackendConfigSettingsClass>.Instance.Stamina;

	public virtual bool BerserkRestorationFactor
	{
		[CompilerGenerated]
		get
		{
			return Bool_4;
		}
		[CompilerGenerated]
		set
		{
			Bool_4 = value;
		}
	}

	public float SprintSpeed => Mathf.Lerp(1f, StaminaParameters.SprintSpeedLowerLimit, Float_3);

	public float SprintSensitivity => Mathf.Lerp(1f, StaminaParameters.SprintSensitivityLowerLimit, Float_3);

	public float MeleeSpeed
	{
		get
		{
			if (!HandsStamina.Exhausted)
			{
				return 1f;
			}
			return StaminaParameters.ExhaustedMeleeSpeed;
		}
	}

	public PhysicalStateStruct SerializationStruct
	{
		get
		{
			this.PhysicalStateStruct.StaminaExhausted = Stamina.Exhausted;
			this.PhysicalStateStruct.OxygenExhausted = Oxygen.Exhausted;
			this.PhysicalStateStruct.HandsExhausted = HandsStamina.Exhausted;
			return this.PhysicalStateStruct;
		}
		set
		{
			if (!SerializationStruct.Equals(value))
			{
				Stamina.UpdateStamina((!value.StaminaExhausted) ? 100 : 0);
				Oxygen.UpdateStamina((!value.OxygenExhausted) ? 100 : 0);
				HandsStamina.UpdateStamina((!value.HandsExhausted) ? 100 : 0);
			}
		}
	}

	public bool EncumberDisabled
	{
		get
		{
			return Bool_7;
		}
		set
		{
			Bool_7 = value;
			if (IobserverToPlayerBridge_0 != null)
			{
				UpdateWeightLimits();
				OnWeightUpdated();
			}
		}
	}

	public bool Boolean_0
	{
		get
		{
			if (!(Overweight > 0f) && !(WalkOverweight > 0f) && !(WalkSpeedLimit < 1f))
			{
				return Float_3 > 0f;
			}
			return true;
		}
	}

	public bool BreathIsAudible
	{
		get
		{
			if (HoldingBreath)
			{
				return false;
			}
			if (StaminaParameters.StaminaExhaustionStartsBreathSound && Stamina.Exhausted)
			{
				return true;
			}
			return Oxygen.Exhausted;
		}
	}

	public event Action<bool> OnSprintStateChangedEvent
	{
		[CompilerGenerated]
		add
		{
			Action<bool> action = action_0;
			Action<bool> action2;
			do
			{
				action2 = action;
				Action<bool> value2 = (Action<bool>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<bool> action = action_0;
			Action<bool> action2;
			do
			{
				action2 = action;
				Action<bool> value2 = (Action<bool>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public event Action WeightRelatedValuesUpdated
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

	public event Action OverweightIncreased
	{
		[CompilerGenerated]
		add
		{
			Action action = action_2;
			Action action2;
			do
			{
				action2 = action;
				Action value2 = (Action)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_2, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action action = action_2;
			Action action2;
			do
			{
				action2 = action;
				Action value2 = (Action)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_2, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public event Action<bool> EncumberedChanged
	{
		[CompilerGenerated]
		add
		{
			Action<bool> action = action_3;
			Action<bool> action2;
			do
			{
				action2 = action;
				Action<bool> value2 = (Action<bool>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_3, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<bool> action = action_3;
			Action<bool> action2;
			do
			{
				action2 = action;
				Action<bool> value2 = (Action<bool>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_3, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public event Action<bool> OverEncumberedChanged
	{
		[CompilerGenerated]
		add
		{
			Action<bool> action = action_4;
			Action<bool> action2;
			do
			{
				action2 = action;
				Action<bool> value2 = (Action<bool>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_4, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<bool> action = action_4;
			Action<bool> action2;
			do
			{
				action2 = action;
				Action<bool> value2 = (Action<bool>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_4, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public event Action HoldBreathChanged
	{
		[CompilerGenerated]
		add
		{
			Action action = action_5;
			Action action2;
			do
			{
				action2 = action;
				Action value2 = (Action)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_5, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action action = action_5;
			Action action2;
			do
			{
				action2 = action;
				Action value2 = (Action)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_5, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public virtual float GetOxygenRestorationFunc()
	{
		return 0f;
	}

	public virtual float GetOxygenCapacityFunc()
	{
		return 100f;
	}

	public virtual float GetStaminaRestorationFunc()
	{
		return 0f;
	}

	public virtual float GetStaminaCapacityFunc()
	{
		return 100f;
	}

	public virtual float GetHandsRestorationFunc()
	{
		return 0f;
	}

	public virtual float GetHandsCapacityFunc()
	{
		return 100f;
	}

	public virtual void Init(IPlayer iPlayer)
	{
		if (!(iPlayer is Player player))
		{
			if (iPlayer is ObservedPlayerView)
			{
				return;
			}
		}
		else
		{
			IobserverToPlayerBridge_0 = new PlayerBridge(player);
		}
		UpdateWeightLimits();
		Stamina = new GClass774(new GClass849<float>(GetStaminaCapacityFunc), new GClass849<float>(GetStaminaRestorationFunc));
		Oxygen = new GClass774(new GClass849<float>(GetOxygenCapacityFunc), new GClass849<float>(GetOxygenRestorationFunc));
		HandsStamina = new GClass774(new GClass849<float>(GetHandsCapacityFunc), new GClass849<float>(GetHandsRestorationFunc));
		PoseLevelIncreaseSpeed = new GClass849<float>(() => GClass2298.Evaluate(StaminaParameters.PoseLevelIncreaseSpeed, Mathf.Max(Overweight, Stamina.Exhausted ? 0.9f : 0f)));
		PoseLevelDecreaseSpeed = new GClass849<float>(() => GClass2298.Evaluate(StaminaParameters.PoseLevelDecreaseSpeed, Mathf.Max(Overweight, Stamina.Exhausted ? 0.9f : 0f)));
		Action unsub = IobserverToPlayerBridge_0.Skills.Strength.OnLevelUp.Subscribe(OnWeightLimitsUpdated);
		IobserverToPlayerBridge_0.OnTotalWeightUpdated += method_1;
		IobserverToPlayerBridge_0.iPlayer.HealthController.EffectStartedEvent += method_6;
		IobserverToPlayerBridge_0.iPlayer.HealthController.EffectResidualEvent += method_6;
		IobserverToPlayerBridge_0.OnPhysicalConditionChanged += PhysicalConditionChanged;
		Action_6 = delegate
		{
			IobserverToPlayerBridge_0.OnPhysicalConditionChanged -= PhysicalConditionChanged;
			unsub();
			IobserverToPlayerBridge_0.OnTotalWeightUpdated -= method_1;
			IobserverToPlayerBridge_0.iPlayer.HealthController.EffectStartedEvent -= method_6;
			IobserverToPlayerBridge_0.iPlayer.HealthController.EffectResidualEvent -= method_6;
			Stamina.OnThresholdPass -= method_8;
			TransitionSpeed.Dispose();
			MinStepSound.Dispose();
		};
		MinStepSound = new GClass849<float>(() => (!IobserverToPlayerBridge_0.PhysicalConditionContainsAny(EPhysicalCondition.LeftLegDamaged | EPhysicalCondition.RightLegDamaged)) ? (0.6f * Overweight) : 0.6f);
		TransitionSpeed = new GClass849<float>(() => GClass2298.Evaluate(StaminaParameters.TransitionSpeed, Mathf.Max(Overweight, Stamina.Exhausted ? 1 : 0)));
		Stamina.OnThresholdPass += method_8;
		if (IobserverToPlayerBridge_0.iPlayer.IsYourPlayer)
		{
			unsub = (Action)Delegate.Combine(unsub, GlobalEventHandlerClass.Instance.SubscribeOnEvent(delegate(GClass3546 calledEvent)
			{
				method_0(calledEvent);
			}));
		}
	}

	public void LateUpdate()
	{
		if (Bool_1)
		{
			Bool_1 = false;
			float totalWeight = IobserverToPlayerBridge_0.TotalWeight;
			if (Math.Abs(PreviousWeight - totalWeight) > 0.01f)
			{
				OnWeightUpdated();
			}
		}
		if (!Bool_3)
		{
			return;
		}
		if (Stamina.Exhausted)
		{
			Fatigue += Time.deltaTime;
			if (Fatigue > StaminaParameters.FatigueAmountToCreateEffect)
			{
				IobserverToPlayerBridge_0.iPlayer.HealthController.AddFatigue();
				Fatigue = 0f;
			}
		}
		else if (Fatigue > 0f)
		{
			Fatigue -= StaminaParameters.FatigueRestorationRate * Time.deltaTime;
		}
	}

	public void Unsubscribe()
	{
		Action_6?.Invoke();
		Action_6 = null;
	}

	public void method_0(GClass3546 invokedEvent)
	{
		if (IobserverToPlayerBridge_0.iPlayer.ProfileId == invokedEvent.PlayerProfileID)
		{
			Bool_3 = !invokedEvent.IsPaused;
		}
	}

	public void method_1()
	{
		Bool_1 = true;
	}

	public virtual void PhysicalConditionChanged(EPhysicalCondition diff, EPhysicalCondition full)
	{
		if (diff == EPhysicalCondition.LeftLegDamaged || diff == EPhysicalCondition.RightLegDamaged)
		{
			MinStepSound.SetDirty();
		}
	}

	public void method_2()
	{
		action_0?.Invoke(Sprinting);
	}

	public virtual void InvokeInsufficient()
	{
		action_0?.Invoke(obj: true);
	}

	public void method_3()
	{
		action_1?.Invoke();
	}

	public void method_4()
	{
		action_2?.Invoke();
	}

	public void method_5()
	{
		action_5?.Invoke();
	}

	public void method_6(IEffect effect)
	{
		if (effect is GInterface375)
		{
			OnWeightLimitsUpdated();
		}
	}

	public void OnWeightLimitsUpdated()
	{
		UpdateWeightLimits();
		OnWeightUpdated();
	}

	public void UpdateWeightLimits()
	{
		if (!Bool_7 && !IobserverToPlayerBridge_0.iPlayer.IsAI)
		{
			BackendConfigSettingsClass.GClass1736 stamina = Singleton<BackendConfigSettingsClass>.Instance.Stamina;
			float num = IobserverToPlayerBridge_0.Skills.CarryingWeightRelativeModifier * IobserverToPlayerBridge_0.iPlayer.HealthController.CarryingWeightRelativeModifier;
			Vector2 vector = new Vector2(IobserverToPlayerBridge_0.iPlayer.HealthController.CarryingWeightAbsoluteModifier, IobserverToPlayerBridge_0.iPlayer.HealthController.CarryingWeightAbsoluteModifier);
			BackendConfigSettingsClass.InertiaSettings inertia = Singleton<BackendConfigSettingsClass>.Instance.Inertia;
			Vector3 vector2 = new Vector3(inertia.InertiaLimitsStep * (float)IobserverToPlayerBridge_0.Skills.Strength.SummaryLevel, inertia.InertiaLimitsStep * (float)IobserverToPlayerBridge_0.Skills.Strength.SummaryLevel, 0f);
			BaseInertiaLimits = inertia.InertiaLimits + vector2;
			WalkOverweightLimits = stamina.WalkOverweightLimits * num + vector;
			BaseOverweightLimits = stamina.BaseOverweightLimits * num + vector;
			SprintOverweightLimits = stamina.SprintOverweightLimits * num + vector;
			WalkSpeedOverweightLimits = stamina.WalkSpeedOverweightLimits * num + vector;
		}
		else
		{
			WalkOverweightLimits.Set(9000f, 10000f);
			BaseOverweightLimits.Set(9000f, 10000f);
			SprintOverweightLimits.Set(9000f, 10000f);
			WalkSpeedOverweightLimits.Set(9000f, 10000f);
		}
	}

	public float CalculateValue(Vector3 dependency, float weight)
	{
		float num = GClass2298.InverseLerp(dependency, weight);
		return Mathf.Lerp(num * num * num, num, dependency[2]);
	}

	public virtual void OnWeightUpdated()
	{
		float totalWeight = IobserverToPlayerBridge_0.TotalWeight;
		BackendConfigSettingsClass.InertiaSettings inertia = Singleton<BackendConfigSettingsClass>.Instance.Inertia;
		Inertia = CalculateValue(BaseInertiaLimits, totalWeight);
		SprintAcceleration = GClass2298.InverseLerp(inertia.SprintAccelerationLimits, Inertia);
		PreSprintAcceleration = GClass2298.Evaluate(inertia.PreSprintAccelerationLimits, Inertia);
		float time = Mathf.Lerp(inertia.MinMovementAccelerationRangeRight.x, inertia.MaxMovementAccelerationRangeRight.x, Inertia);
		float value = Mathf.Lerp(inertia.MinMovementAccelerationRangeRight.y, inertia.MaxMovementAccelerationRangeRight.y, Inertia);
		EFTHardSettings.Instance.MovementAccelerationRange.MoveKey(1, new Keyframe(time, value));
		Overweight = GClass2298.InverseLerp(BaseOverweightLimits, totalWeight);
		WalkOverweight = GClass2298.InverseLerp(WalkOverweightLimits, totalWeight);
		Float_3 = GClass2298.InverseLerp(SprintOverweightLimits, totalWeight);
		WalkSpeedLimit = 1f - GClass2298.InverseLerp(WalkSpeedOverweightLimits, totalWeight);
		MoveSideInertia = GClass2298.Evaluate(inertia.SideTime, Inertia);
		MoveDiagonalInertia = GClass2298.Evaluate(inertia.DiagonalTime, Inertia);
		if (IobserverToPlayerBridge_0.iPlayer.IsAI)
		{
			Float_3 = 0f;
		}
		FallDamageMultiplier = Mathf.Lerp(1f, StaminaParameters.FallDamageMultiplier, Overweight);
		SoundRadius = GClass2298.Evaluate(StaminaParameters.SoundRadius, Overweight);
		MinStepSound.SetDirty();
		TransitionSpeed.SetDirty();
		method_3();
		method_7(totalWeight);
	}

	public void method_7(float weight)
	{
		if (!IobserverToPlayerBridge_0.iPlayer.IsAI && !Bool_7)
		{
			bool boolean_ = Boolean_0;
			bool flag = Overweight >= 1f;
			if (Bool_5 != boolean_)
			{
				action_3?.Invoke(boolean_);
			}
			if (Bool_6 != flag)
			{
				action_4?.Invoke(flag);
			}
			if (Bool_5 && weight > PreviousWeight)
			{
				method_4();
			}
			PreviousWeight = weight;
			Bool_5 = boolean_;
			Bool_6 = flag;
		}
		else
		{
			PreviousWeight = weight;
		}
	}

	public virtual void Sprint(bool target)
	{
		Bool_2 = target;
	}

	public virtual void Aim(float mass = 0f)
	{
	}

	public virtual void HoldBreath(bool enable)
	{
	}

	public virtual void Update(float dt)
	{
	}

	public virtual void OnBreach()
	{
	}

	public virtual void OnThrow(bool lowThrow)
	{
	}

	public virtual void ConsumeAsMelee(float expense)
	{
	}

	public virtual void OnJump(bool enabled)
	{
	}

	public virtual void BulletHit(float staminaBurnRate)
	{
	}

	public virtual void StandUp()
	{
	}

	public virtual void ConsumePoseLevelChange(float notches)
	{
	}

	public virtual void OnWeaponSwitchFast(int weaponTypeMasteringLevel)
	{
	}

	public virtual void OnVault()
	{
	}

	public virtual void OnClimb()
	{
	}

	public Action SubscribeToAudibleEffects(Action updateBreathStatus)
	{
		HoldBreathChanged += updateBreathStatus;
		Oxygen.OnValueChanged += updateBreathStatus;
		Action action = delegate
		{
			Oxygen.OnValueChanged -= updateBreathStatus;
			HoldBreathChanged -= updateBreathStatus;
		};
		if (StaminaParameters.StaminaExhaustionStartsBreathSound)
		{
			Stamina.OnValueChanged += updateBreathStatus;
			action = (Action)Delegate.Combine(action, (Action)delegate
			{
				Stamina.OnValueChanged -= updateBreathStatus;
			});
		}
		return action;
	}

	public void method_8()
	{
		TransitionSpeed.SetDirty();
	}
}
