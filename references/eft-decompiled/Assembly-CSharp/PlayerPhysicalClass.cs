using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Comfort.Common;
using EFT;
using EFT.WeaponMounting;
using UnityEngine;

public class PlayerPhysicalClass : BasePhysicalClass
{
	public enum EConsumptionType
	{
		OverweightIdle,
		Sprint,
		Walk,
		Jump,
		Aim,
		Melee,
		HoldBreath,
		Wound,
		StandUp,
		Prone,
		SitToStand,
		FastWeaponSwitch,
		VaultLegs,
		ClimbLegs,
		VaultHands,
		ClimbHands
	}

	public enum EPose
	{
		Prone,
		Sit,
		Stand
	}

	[Flags]
	public enum EConsumptionTarget
	{
		Base = 1,
		Hands = 2,
		Oxygen = 4,
		None = 8
	}

	public class GClass773
	{
		[CompilerGenerated]
		public class Class435
		{
			public GClass773 gclass773_0;

			public BasePhysicalClass physical;

			public void method_0()
			{
				gclass773_0.Action_0 = (Action)Delegate.Combine(gclass773_0.Action_0, physical.Stamina.AddConsumption(gclass773_0));
				gclass773_0.Action_0 = (Action)Delegate.Combine(gclass773_0.Action_0, physical.Oxygen.AddConsumption(gclass773_0));
			}

			public void method_1()
			{
				physical.HandsStamina.OnExpired -= delegate
				{
					gclass773_0.Action_0 = (Action)Delegate.Combine(gclass773_0.Action_0, physical.Stamina.AddConsumption(gclass773_0));
					gclass773_0.Action_0 = (Action)Delegate.Combine(gclass773_0.Action_0, physical.Oxygen.AddConsumption(gclass773_0));
				};
			}
		}

		public readonly EConsumptionType ConsumptionType;

		public GClass848<float> Delta;

		public GClass848<bool> Availability;

		public float StartThreshold;

		public EConsumptionTarget Requires = EConsumptionTarget.None;

		public float Downtime;

		public bool AllowsRestoration;

		public EConsumptionTarget PrimaryTarget;

		[NonSerialized]
		public Action Action_0;

		public GClass773(EConsumptionType consumptionType)
		{
			ConsumptionType = consumptionType;
		}

		public void SetActive(BasePhysicalClass physical, bool isActive)
		{
			if (isActive)
			{
				method_0(physical);
				return;
			}
			Action_0?.Invoke();
			Action_0 = null;
		}

		public void Consume(BasePhysicalClass physical, bool fromHit = false)
		{
			if ((PrimaryTarget & EConsumptionTarget.Base) != 0)
			{
				physical.Stamina.Consume(this, fromHit);
			}
			if ((PrimaryTarget & EConsumptionTarget.Hands) != 0 && physical.HandsStamina.Consume(this) > 0f)
			{
				physical.Stamina.Consume(this, fromHit);
			}
			if ((PrimaryTarget & EConsumptionTarget.Oxygen) != 0)
			{
				physical.Oxygen.Consume(this);
			}
		}

		public void method_0(BasePhysicalClass physical)
		{
			Class435 CS_0024_003C_003E8__locals25 = new Class435();
			CS_0024_003C_003E8__locals25.gclass773_0 = this;
			CS_0024_003C_003E8__locals25.physical = physical;
			if ((PrimaryTarget & EConsumptionTarget.Base) != 0)
			{
				Action_0 = (Action)Delegate.Combine(Action_0, CS_0024_003C_003E8__locals25.physical.Stamina.AddConsumption(this));
			}
			if ((PrimaryTarget & EConsumptionTarget.Hands) != 0)
			{
				Action_0 = (Action)Delegate.Combine(Action_0, CS_0024_003C_003E8__locals25.physical.HandsStamina.AddConsumption(this));
				if (CS_0024_003C_003E8__locals25.physical.HandsStamina.Current <= 0f)
				{
					CS_0024_003C_003E8__locals25.method_0();
				}
				else
				{
					CS_0024_003C_003E8__locals25.physical.HandsStamina.OnExpired += delegate
					{
						CS_0024_003C_003E8__locals25.gclass773_0.Action_0 = (Action)Delegate.Combine(CS_0024_003C_003E8__locals25.gclass773_0.Action_0, CS_0024_003C_003E8__locals25.physical.Stamina.AddConsumption(CS_0024_003C_003E8__locals25.gclass773_0));
						CS_0024_003C_003E8__locals25.gclass773_0.Action_0 = (Action)Delegate.Combine(CS_0024_003C_003E8__locals25.gclass773_0.Action_0, CS_0024_003C_003E8__locals25.physical.Oxygen.AddConsumption(CS_0024_003C_003E8__locals25.gclass773_0));
					};
					Action_0 = (Action)Delegate.Combine(Action_0, (Action)delegate
					{
						CS_0024_003C_003E8__locals25.physical.HandsStamina.OnExpired -= delegate
						{
							CS_0024_003C_003E8__locals25.gclass773_0.Action_0 = (Action)Delegate.Combine(CS_0024_003C_003E8__locals25.gclass773_0.Action_0, CS_0024_003C_003E8__locals25.physical.Stamina.AddConsumption(CS_0024_003C_003E8__locals25.gclass773_0));
							CS_0024_003C_003E8__locals25.gclass773_0.Action_0 = (Action)Delegate.Combine(CS_0024_003C_003E8__locals25.gclass773_0.Action_0, CS_0024_003C_003E8__locals25.physical.Oxygen.AddConsumption(CS_0024_003C_003E8__locals25.gclass773_0));
						};
					});
				}
			}
			if ((PrimaryTarget & EConsumptionTarget.Oxygen) != 0)
			{
				Action_0 = (Action)Delegate.Combine(Action_0, CS_0024_003C_003E8__locals25.physical.Oxygen.AddConsumption(this));
			}
		}
	}

	[CompilerGenerated]
	public class Class436
	{
		public PlayerPhysicalClass PlayerPhysicalClass;

		public Action unsub;

		public void method_0()
		{
			PlayerPhysicalClass.Player_0.HealthController.EnergyChangedEvent -= PlayerPhysicalClass.method_19;
			unsub();
			PlayerPhysicalClass.Player_0.HealthController.HydrationChangedEvent -= PlayerPhysicalClass.method_20;
			PlayerPhysicalClass.Player_0.MovementContext.OnStateChanged -= PlayerPhysicalClass.method_16;
			PlayerPhysicalClass.Player_0.MovementContext.OnPoseChanged -= PlayerPhysicalClass.method_17;
			PlayerPhysicalClass.Stamina.OnChanged -= PlayerPhysicalClass.method_15;
			PlayerPhysicalClass.Stamina.OnValueChanged -= PlayerPhysicalClass.method_14;
			PlayerPhysicalClass.Stamina.OnThresholdPass -= PlayerPhysicalClass.method_9;
			PlayerPhysicalClass.Stamina.OnExpired -= PlayerPhysicalClass.method_12;
			PlayerPhysicalClass.HandsStamina.OnExpired -= PlayerPhysicalClass.method_11;
			PlayerPhysicalClass.Oxygen.OnExpired -= PlayerPhysicalClass.method_13;
			PlayerPhysicalClass.PoseLevelIncreaseSpeed.Dispose();
			PlayerPhysicalClass.PoseLevelDecreaseSpeed.Dispose();
		}

		public float method_1()
		{
			if (PlayerPhysicalClass.Float_13 < Time.time)
			{
				PlayerPhysicalClass.Float_12 = 0f;
				PlayerPhysicalClass.Float_13 = Time.time + 2f;
				return 0f;
			}
			PlayerPhysicalClass.Float_13 = Time.time + 2f;
			PlayerPhysicalClass.Float_12 = Mathf.Min(PlayerPhysicalClass.Float_12 + 0.5f, 5f);
			return PlayerPhysicalClass.Float_12;
		}

		public bool method_2()
		{
			if (PlayerPhysicalClass.Player_0.MovementContext.Slope.sqrMagnitude <= 0f)
			{
				return true;
			}
			Vector3 slope = PlayerPhysicalClass.Player_0.MovementContext.Slope;
			slope.y = 0f;
			return Vector3.Dot(slope.normalized.normalized, PlayerPhysicalClass.Player_0.MovementContext.AbsoluteMovementDirection) < 0.8f;
		}

		public float method_3()
		{
			float num = (PlayerPhysicalClass.Overweight + PlayerPhysicalClass.Single_0) * PlayerPhysicalClass.StaminaParameters.JumpConsumption * (1f - (float)PlayerPhysicalClass.IobserverToPlayerBridge_0.Skills.EnduranceBuffJumpCostRed);
			if (PlayerPhysicalClass.Player_0.MovementContext.PhysicalConditionContainsAny(EPhysicalCondition.LeftLegDamaged | EPhysicalCondition.RightLegDamaged))
			{
				num *= 2f;
			}
			if (PlayerPhysicalClass.Player_0.MovementContext.PhysicalConditionContainsAny(EPhysicalCondition.Panic) && Singleton<BackendConfigSettingsClass>.Instantiated)
			{
				num *= Singleton<BackendConfigSettingsClass>.Instance.EventSettings.StaminaMultiplier;
			}
			return num;
		}

		public float method_4()
		{
			return PlayerPhysicalClass.Float_9 * (1f + PlayerPhysicalClass.Overweight);
		}

		public float method_5()
		{
			return PlayerPhysicalClass.Float_10;
		}

		public float method_6()
		{
			return GClass2298.Evaluate(Singleton<BackendConfigSettingsClass>.Instance.Stamina.StandupConsumption, PlayerPhysicalClass.Overweight);
		}

		public float method_7()
		{
			switch (PlayerPhysicalClass.Epose_0)
			{
			default:
				return 0f;
			case EPose.Stand:
				if (!(PlayerPhysicalClass.WalkOverweight <= 0f))
				{
					return GClass2298.Evaluate(PlayerPhysicalClass.StaminaParameters.WalkConsumption, PlayerPhysicalClass.WalkOverweight);
				}
				return 0f;
			case EPose.Sit:
				return GClass2298.Evaluate(PlayerPhysicalClass.StaminaParameters.CrouchConsumption, PlayerPhysicalClass.Overweight);
			}
		}

		public bool method_8()
		{
			if (PlayerPhysicalClass.Player_0.IsInPronePose)
			{
				return false;
			}
			if (PlayerPhysicalClass.Overweight >= 1f)
			{
				return false;
			}
			if (PlayerPhysicalClass.Player_0.MovementContext.PhysicalConditionContainsAny(EPhysicalCondition.UsingMeds | EPhysicalCondition.SprintDisabled))
			{
				return false;
			}
			if (PlayerPhysicalClass.Player_0.MovementContext.PhysicalConditionContainsAny(EPhysicalCondition.LeftLegDamaged | EPhysicalCondition.RightLegDamaged) && !PlayerPhysicalClass.Player_0.MovementContext.PhysicalConditionIs(EPhysicalCondition.OnPainkillers))
			{
				return false;
			}
			return true;
		}

		public float method_9()
		{
			float num = (PlayerPhysicalClass.Overweight + PlayerPhysicalClass.Single_0) * PlayerPhysicalClass.StaminaParameters.SprintDrainRate;
			if (PlayerPhysicalClass.Player_0.MovementContext.PhysicalConditionContainsAny(EPhysicalCondition.Panic) && Singleton<BackendConfigSettingsClass>.Instantiated)
			{
				num *= Singleton<BackendConfigSettingsClass>.Instance.EventSettings.StaminaMultiplier;
			}
			return num;
		}

		public float method_10()
		{
			MovementContext movementContext = PlayerPhysicalClass.Player_0.MovementContext;
			float num = (movementContext.PhysicalConditionIs(EPhysicalCondition.OnPainkillers) ? 1f : (movementContext.PhysicalConditionIs(EPhysicalCondition.LeftArmDamaged | EPhysicalCondition.RightArmDamaged) ? 2f : (movementContext.PhysicalConditionContainsAny(EPhysicalCondition.LeftArmDamaged | EPhysicalCondition.RightArmDamaged) ? 1.5f : 1f)));
			float num2 = PlayerPhysicalClass.StaminaParameters.AimDrainRate;
			if (PlayerPhysicalClass.Player_0.HandsController is PortableRangeFinderController)
			{
				num2 = PlayerPhysicalClass.StaminaParameters.AimRangeFinderDrainRate;
			}
			float num3 = 1f;
			if (movementContext.IsInMountedState && PlayerPhysicalClass.Player_0.HandsController is Player.FirearmController firearmController)
			{
				num3 = ((movementContext.PlayerMountingPointData.MountPointData.MountSideDirection != EMountSideDirection.Forward) ? PlayerPhysicalClass.StaminaParameters.MountingVerticalAimDrainRateMultiplier : (firearmController.BipodState ? PlayerPhysicalClass.StaminaParameters.BipodAimDrainRateMultiplier : PlayerPhysicalClass.StaminaParameters.MountingHorizontalAimDrainRateMultiplier));
			}
			return Mathf.Sqrt(PlayerPhysicalClass.Float_5) * num2 * num * PlayerPhysicalClass.Float_6[(int)PlayerPhysicalClass.Epose_0] * (1f - (float)PlayerPhysicalClass.Player_0.Skills.StrengthBuffAimFatigue) * PlayerPhysicalClass.Single_0 * num3;
		}

		public float method_11()
		{
			return (float)PlayerPhysicalClass.Stamina.SelfRestoration + PlayerPhysicalClass.Float_8[(int)PlayerPhysicalClass.Epose_0];
		}

		public float method_12()
		{
			return PlayerPhysicalClass.BaseHoldBreathConsumption * GClass2298.Evaluate(PlayerPhysicalClass.StaminaParameters.HoldBreathStaminaMultiplier, PlayerPhysicalClass.Stamina.NormalValue);
		}

		public float method_13()
		{
			return PlayerPhysicalClass.StaminaParameters.ProneConsumption;
		}

		public float method_14()
		{
			return GClass2298.Evaluate(PlayerPhysicalClass.StaminaParameters.PoseLevelConsumptionPerNotch, PlayerPhysicalClass.Overweight) * PlayerPhysicalClass.Float_11;
		}

		public float method_15()
		{
			float num = PlayerPhysicalClass.StaminaParameters.WeaponFastSwitchConsumption / (float)(PlayerPhysicalClass.Int_0 / 50 + 1);
			float a = PlayerPhysicalClass.HandsStamina.Current - num;
			PlayerPhysicalClass.Float_14 = Mathf.Abs(Mathf.Min(a, 0f));
			return num;
		}

		public float method_16()
		{
			switch (PlayerPhysicalClass.Epose_0)
			{
			default:
				return 0f;
			case EPose.Stand:
				if (!(PlayerPhysicalClass.WalkOverweight <= 0f))
				{
					return GClass2298.Evaluate(PlayerPhysicalClass.StaminaParameters.VaultLegsConsumption, PlayerPhysicalClass.WalkOverweight);
				}
				return 0f;
			case EPose.Sit:
				return GClass2298.Evaluate(PlayerPhysicalClass.StaminaParameters.VaultLegsConsumption, PlayerPhysicalClass.Overweight);
			}
		}

		public float method_17()
		{
			return PlayerPhysicalClass.Epose_0 switch
			{
				EPose.Stand => GClass2298.Evaluate(PlayerPhysicalClass.StaminaParameters.ClimbLegsConsumption, PlayerPhysicalClass.WalkOverweight), 
				EPose.Sit => GClass2298.Evaluate(PlayerPhysicalClass.StaminaParameters.ClimbLegsConsumption, PlayerPhysicalClass.Overweight), 
				_ => 0f, 
			};
		}

		public float method_18()
		{
			if (PlayerPhysicalClass.Player_0.MovementContext.PlayerAnimatorGetObstacleHeight() > Singleton<BackendConfigSettingsClass>.Instance.VaultingSettings.MovesSettings.VaultSettings.MaxWithoutHandHeight)
			{
				switch (PlayerPhysicalClass.Epose_0)
				{
				case EPose.Stand:
					return GClass2298.Evaluate(PlayerPhysicalClass.StaminaParameters.VaultOneHandConsumption, PlayerPhysicalClass.WalkOverweight);
				case EPose.Sit:
					return GClass2298.Evaluate(PlayerPhysicalClass.StaminaParameters.VaultOneHandConsumption, PlayerPhysicalClass.Overweight);
				}
			}
			return 0f;
		}

		public float method_19()
		{
			float num = PlayerPhysicalClass.Player_0.MovementContext.PlayerAnimatorGetObstacleHeight();
			if (num > Singleton<BackendConfigSettingsClass>.Instance.VaultingSettings.MovesSettings.ClimbSettings.MaxOneHandHeight)
			{
				switch (PlayerPhysicalClass.Epose_0)
				{
				case EPose.Stand:
					return GClass2298.Evaluate(PlayerPhysicalClass.StaminaParameters.ClimbTwoHandsConsumption, PlayerPhysicalClass.WalkOverweight);
				case EPose.Sit:
					return GClass2298.Evaluate(PlayerPhysicalClass.StaminaParameters.ClimbTwoHandsConsumption, PlayerPhysicalClass.Overweight);
				}
			}
			else if (num > Singleton<BackendConfigSettingsClass>.Instance.VaultingSettings.MovesSettings.ClimbSettings.MaxWithoutHandHeight)
			{
				switch (PlayerPhysicalClass.Epose_0)
				{
				case EPose.Stand:
					return GClass2298.Evaluate(PlayerPhysicalClass.StaminaParameters.ClimbOneHandConsumption, PlayerPhysicalClass.WalkOverweight);
				case EPose.Sit:
					return GClass2298.Evaluate(PlayerPhysicalClass.StaminaParameters.ClimbOneHandConsumption, PlayerPhysicalClass.Overweight);
				}
			}
			return 0f;
		}
	}

	[NonSerialized]
	public const float Float_4 = 0.9f;

	public readonly Dictionary<EConsumptionType, GClass773> Consumptions = GClass866<EConsumptionType>.GetDictWith<GClass773>();

	public float BaseHoldBreathConsumption = Singleton<BackendConfigSettingsClass>.Instance.Stamina.BaseHoldBreathConsumption;

	[NonSerialized]
	public bool Bool_8;

	[NonSerialized]
	public EPose Epose_0 = EPose.Stand;

	[NonSerialized]
	public float Float_5;

	[NonSerialized]
	public float[] Float_6;

	[NonSerialized]
	public float[] Float_7;

	[NonSerialized]
	public float[] Float_8;

	[NonSerialized]
	public float Float_9;

	[NonSerialized]
	public float Float_10;

	[NonSerialized]
	public float Float_11;

	[NonSerialized]
	public float Float_12;

	[NonSerialized]
	public float Float_13;

	[NonSerialized]
	public float Float_14;

	[NonSerialized]
	public int Int_0;

	[NonSerialized]
	public GClass773 Gclass773_0 = new GClass773(EConsumptionType.Sprint);

	[NonSerialized]
	public Player Player_0;

	public override bool Exhausted
	{
		get
		{
			if (!(Stamina.Current < 15f))
			{
				return Oxygen.Current < 15f;
			}
			return true;
		}
	}

	public override bool CanSprint => Stamina.CanAdd(Consumptions[EConsumptionType.Sprint]);

	public override bool Sprinting => Bool_8;

	public override bool HoldingBreath => Oxygen.Consumptions.Contains(Consumptions[EConsumptionType.HoldBreath]);

	public override bool CanJump => Stamina.CanAdd(Consumptions[EConsumptionType.Jump]);

	public override bool CanVault
	{
		get
		{
			if (!(Player_0.VaultingParameters.VaultingHeight <= Singleton<BackendConfigSettingsClass>.Instance.VaultingSettings.MovesSettings.VaultSettings.MaxWithoutHandHeight))
			{
				if (Stamina.CanAdd(Consumptions[EConsumptionType.VaultLegs]))
				{
					return HandsStamina.CanAdd(Consumptions[EConsumptionType.VaultHands]);
				}
				return false;
			}
			return Stamina.CanAdd(Consumptions[EConsumptionType.VaultLegs]);
		}
	}

	public override bool CanClimb
	{
		get
		{
			if (!(Player_0.VaultingParameters.VaultingHeight <= Singleton<BackendConfigSettingsClass>.Instance.VaultingSettings.MovesSettings.ClimbSettings.MaxWithoutHandHeight))
			{
				if (Stamina.CanAdd(Consumptions[EConsumptionType.ClimbLegs]))
				{
					return HandsStamina.CanAdd(Consumptions[EConsumptionType.ClimbHands]);
				}
				return false;
			}
			return Stamina.CanAdd(Consumptions[EConsumptionType.ClimbLegs]);
		}
	}

	public override bool CanMeleeHit
	{
		get
		{
			if (!Stamina.CanAdd(Consumptions[EConsumptionType.Melee]))
			{
				return HandsStamina.CanAdd(Consumptions[EConsumptionType.Melee]);
			}
			return true;
		}
	}

	public override bool BerserkRestorationFactor
	{
		set
		{
			HandsStamina.ForceMode = value;
			Stamina.ForceMode = value;
		}
	}

	public override float FastWeaponSwitchStaminaLack => Float_14;

	public float Single_0 => Singleton<BackendConfigSettingsClass>.Instance.StaminaDrain.GetAt(Player_0.HealthController.Hydration.Normalized);

	public PlayerPhysicalClass()
	{
		BackendConfigSettingsClass.GClass1736 stamina = Singleton<BackendConfigSettingsClass>.Instance.Stamina;
		Float_6 = GClass2298.ToArray(stamina.AimConsumptionByPose);
		Float_7 = GClass2298.ToArray(stamina.RestorationMultiplierByPose);
		Float_8 = GClass2298.ToArray(stamina.OverweightConsumptionByPose);
	}

	public override void Init(IPlayer player)
	{
		if (!(player is Player player_))
		{
			return;
		}
		Player_0 = player_;
		base.Init(player);
		Player_0.HealthController.EnergyChangedEvent += method_19;
		Action unsub = IobserverToPlayerBridge_0.Skills.Endurance.OnLevelUp.Subscribe(method_18);
		unsub = (Action)Delegate.Combine(unsub, Player_0.Skills.Strength.OnLevelUp.Subscribe(method_10));
		Player_0.HealthController.HydrationChangedEvent += method_20;
		Player_0.MovementContext.OnStateChanged += method_16;
		Player_0.MovementContext.OnPoseChanged += method_17;
		Stamina.OnChanged += method_15;
		Stamina.OnValueChanged += method_14;
		Stamina.OnThresholdPass += method_9;
		Stamina.OnExpired += method_12;
		HandsStamina.OnExpired += method_11;
		Oxygen.OnExpired += method_13;
		Action_6 = (Action)Delegate.Combine(Action_6, (Action)delegate
		{
			Player_0.HealthController.EnergyChangedEvent -= method_19;
			unsub();
			Player_0.HealthController.HydrationChangedEvent -= method_20;
			Player_0.MovementContext.OnStateChanged -= method_16;
			Player_0.MovementContext.OnPoseChanged -= method_17;
			Stamina.OnChanged -= method_15;
			Stamina.OnValueChanged -= method_14;
			Stamina.OnThresholdPass -= method_9;
			Stamina.OnExpired -= method_12;
			HandsStamina.OnExpired -= method_11;
			Oxygen.OnExpired -= method_13;
			PoseLevelIncreaseSpeed.Dispose();
			PoseLevelDecreaseSpeed.Dispose();
		});
		Gclass773_0.Delta = new GClass848<float>(delegate
		{
			if (Float_13 < Time.time)
			{
				Float_12 = 0f;
				Float_13 = Time.time + 2f;
				return 0f;
			}
			Float_13 = Time.time + 2f;
			Float_12 = Mathf.Min(Float_12 + 0.5f, 5f);
			return Float_12;
		});
		Consumptions.Clear();
		Consumptions[EConsumptionType.Jump] = new GClass773(EConsumptionType.Jump)
		{
			Availability = new GClass848<bool>(delegate
			{
				if (Player_0.MovementContext.Slope.sqrMagnitude <= 0f)
				{
					return true;
				}
				Vector3 slope = Player_0.MovementContext.Slope;
				slope.y = 0f;
				return Vector3.Dot(slope.normalized.normalized, Player_0.MovementContext.AbsoluteMovementDirection) < 0.8f;
			}),
			Delta = new GClass848<float>(delegate
			{
				float num = (Overweight + Single_0) * base.StaminaParameters.JumpConsumption * (1f - (float)IobserverToPlayerBridge_0.Skills.EnduranceBuffJumpCostRed);
				if (Player_0.MovementContext.PhysicalConditionContainsAny(EPhysicalCondition.LeftLegDamaged | EPhysicalCondition.RightLegDamaged))
				{
					num *= 2f;
				}
				if (Player_0.MovementContext.PhysicalConditionContainsAny(EPhysicalCondition.Panic) && Singleton<BackendConfigSettingsClass>.Instantiated)
				{
					num *= Singleton<BackendConfigSettingsClass>.Instance.EventSettings.StaminaMultiplier;
				}
				return num;
			}),
			StartThreshold = 15f,
			Downtime = 1f,
			PrimaryTarget = (EConsumptionTarget.Base | EConsumptionTarget.Oxygen)
		};
		Consumptions[EConsumptionType.Melee] = new GClass773(EConsumptionType.Melee)
		{
			StartThreshold = 10f,
			Downtime = 0.5f,
			Delta = new GClass848<float>(() => Float_9 * (1f + Overweight)),
			PrimaryTarget = (EConsumptionTarget.Hands | EConsumptionTarget.Oxygen)
		};
		Consumptions[EConsumptionType.Wound] = new GClass773(EConsumptionType.Wound)
		{
			Delta = new GClass848<float>(() => Float_10),
			AllowsRestoration = true,
			PrimaryTarget = EConsumptionTarget.Base
		};
		Consumptions[EConsumptionType.StandUp] = new GClass773(EConsumptionType.StandUp)
		{
			Downtime = 1.5f,
			Delta = new GClass848<float>(() => GClass2298.Evaluate(Singleton<BackendConfigSettingsClass>.Instance.Stamina.StandupConsumption, Overweight)),
			PrimaryTarget = (EConsumptionTarget.Base | EConsumptionTarget.Hands | EConsumptionTarget.Oxygen),
			AllowsRestoration = (Overweight >= 1f)
		};
		Consumptions[EConsumptionType.Walk] = new GClass773(EConsumptionType.Walk)
		{
			Delta = new GClass849<float>(delegate
			{
				switch (Epose_0)
				{
				default:
					return 0f;
				case EPose.Stand:
					if (!(WalkOverweight <= 0f))
					{
						return GClass2298.Evaluate(base.StaminaParameters.WalkConsumption, WalkOverweight);
					}
					return 0f;
				case EPose.Sit:
					return GClass2298.Evaluate(base.StaminaParameters.CrouchConsumption, Overweight);
				}
			}),
			Downtime = 0.1f,
			PrimaryTarget = EConsumptionTarget.Base
		};
		Consumptions[EConsumptionType.Sprint] = new GClass773(EConsumptionType.Sprint)
		{
			Availability = new GClass848<bool>(delegate
			{
				if (Player_0.IsInPronePose)
				{
					return false;
				}
				if (Overweight >= 1f)
				{
					return false;
				}
				if (Player_0.MovementContext.PhysicalConditionContainsAny(EPhysicalCondition.UsingMeds | EPhysicalCondition.SprintDisabled))
				{
					return false;
				}
				return (!Player_0.MovementContext.PhysicalConditionContainsAny(EPhysicalCondition.LeftLegDamaged | EPhysicalCondition.RightLegDamaged) || Player_0.MovementContext.PhysicalConditionIs(EPhysicalCondition.OnPainkillers)) ? true : false;
			}),
			Delta = new GClass849<float>(delegate
			{
				float num = (Overweight + Single_0) * base.StaminaParameters.SprintDrainRate;
				if (Player_0.MovementContext.PhysicalConditionContainsAny(EPhysicalCondition.Panic) && Singleton<BackendConfigSettingsClass>.Instantiated)
				{
					num *= Singleton<BackendConfigSettingsClass>.Instance.EventSettings.StaminaMultiplier;
				}
				return num;
			}),
			Requires = EConsumptionTarget.Base,
			Downtime = 1f,
			StartThreshold = 15f,
			PrimaryTarget = (EConsumptionTarget.Base | EConsumptionTarget.Oxygen)
		};
		Consumptions[EConsumptionType.Aim] = new GClass773(EConsumptionType.Aim)
		{
			Delta = new GClass849<float>(delegate
			{
				MovementContext movementContext = Player_0.MovementContext;
				float num = (movementContext.PhysicalConditionIs(EPhysicalCondition.OnPainkillers) ? 1f : (movementContext.PhysicalConditionIs(EPhysicalCondition.LeftArmDamaged | EPhysicalCondition.RightArmDamaged) ? 2f : (movementContext.PhysicalConditionContainsAny(EPhysicalCondition.LeftArmDamaged | EPhysicalCondition.RightArmDamaged) ? 1.5f : 1f)));
				float num2 = base.StaminaParameters.AimDrainRate;
				if (Player_0.HandsController is PortableRangeFinderController)
				{
					num2 = base.StaminaParameters.AimRangeFinderDrainRate;
				}
				float num3 = 1f;
				if (movementContext.IsInMountedState && Player_0.HandsController is Player.FirearmController firearmController)
				{
					num3 = ((movementContext.PlayerMountingPointData.MountPointData.MountSideDirection != EMountSideDirection.Forward) ? base.StaminaParameters.MountingVerticalAimDrainRateMultiplier : (firearmController.BipodState ? base.StaminaParameters.BipodAimDrainRateMultiplier : base.StaminaParameters.MountingHorizontalAimDrainRateMultiplier));
				}
				return Mathf.Sqrt(Float_5) * num2 * num * Float_6[(int)Epose_0] * (1f - (float)Player_0.Skills.StrengthBuffAimFatigue) * Single_0 * num3;
			}),
			Requires = EConsumptionTarget.None,
			AllowsRestoration = false,
			PrimaryTarget = EConsumptionTarget.Hands
		};
		Consumptions[EConsumptionType.OverweightIdle] = new GClass773(EConsumptionType.OverweightIdle)
		{
			Delta = new GClass849<float>(() => (float)Stamina.SelfRestoration + Float_8[(int)Epose_0]),
			AllowsRestoration = true,
			PrimaryTarget = EConsumptionTarget.Base
		};
		Consumptions[EConsumptionType.HoldBreath] = new GClass773(EConsumptionType.HoldBreath)
		{
			Delta = new GClass848<float>(() => BaseHoldBreathConsumption * GClass2298.Evaluate(base.StaminaParameters.HoldBreathStaminaMultiplier, Stamina.NormalValue)),
			StartThreshold = 15f,
			Requires = EConsumptionTarget.Oxygen,
			PrimaryTarget = EConsumptionTarget.Oxygen
		};
		Consumptions[EConsumptionType.Prone] = new GClass773(EConsumptionType.Prone)
		{
			Delta = new GClass849<float>(() => base.StaminaParameters.ProneConsumption),
			Downtime = 0.5f,
			PrimaryTarget = (EConsumptionTarget.Base | EConsumptionTarget.Hands | EConsumptionTarget.Oxygen)
		};
		Consumptions[EConsumptionType.SitToStand] = new GClass773(EConsumptionType.SitToStand)
		{
			Delta = new GClass848<float>(() => GClass2298.Evaluate(base.StaminaParameters.PoseLevelConsumptionPerNotch, Overweight) * Float_11),
			Downtime = 0.5f,
			PrimaryTarget = EConsumptionTarget.Base,
			AllowsRestoration = (Overweight >= 1f)
		};
		Consumptions[EConsumptionType.FastWeaponSwitch] = new GClass773(EConsumptionType.FastWeaponSwitch)
		{
			Delta = new GClass848<float>(delegate
			{
				float num = base.StaminaParameters.WeaponFastSwitchConsumption / (float)(Int_0 / 50 + 1);
				float a = HandsStamina.Current - num;
				Float_14 = Mathf.Abs(Mathf.Min(a, 0f));
				return num;
			}),
			PrimaryTarget = EConsumptionTarget.Hands
		};
		Consumptions[EConsumptionType.VaultLegs] = new GClass773(EConsumptionType.VaultLegs)
		{
			Delta = new GClass849<float>(delegate
			{
				switch (Epose_0)
				{
				default:
					return 0f;
				case EPose.Stand:
					if (!(WalkOverweight <= 0f))
					{
						return GClass2298.Evaluate(base.StaminaParameters.VaultLegsConsumption, WalkOverweight);
					}
					return 0f;
				case EPose.Sit:
					return GClass2298.Evaluate(base.StaminaParameters.VaultLegsConsumption, Overweight);
				}
			}),
			StartThreshold = 15f,
			Downtime = 0.1f,
			PrimaryTarget = EConsumptionTarget.Base
		};
		Consumptions[EConsumptionType.ClimbLegs] = new GClass773(EConsumptionType.ClimbLegs)
		{
			Delta = new GClass849<float>(() => Epose_0 switch
			{
				EPose.Stand => GClass2298.Evaluate(base.StaminaParameters.ClimbLegsConsumption, WalkOverweight), 
				EPose.Sit => GClass2298.Evaluate(base.StaminaParameters.ClimbLegsConsumption, Overweight), 
				_ => 0f, 
			}),
			StartThreshold = 15f,
			Downtime = 0.1f,
			PrimaryTarget = EConsumptionTarget.Base
		};
		Consumptions[EConsumptionType.VaultHands] = new GClass773(EConsumptionType.VaultHands)
		{
			Delta = new GClass849<float>(delegate
			{
				if (Player_0.MovementContext.PlayerAnimatorGetObstacleHeight() > Singleton<BackendConfigSettingsClass>.Instance.VaultingSettings.MovesSettings.VaultSettings.MaxWithoutHandHeight)
				{
					switch (Epose_0)
					{
					case EPose.Stand:
						return GClass2298.Evaluate(base.StaminaParameters.VaultOneHandConsumption, WalkOverweight);
					case EPose.Sit:
						return GClass2298.Evaluate(base.StaminaParameters.VaultOneHandConsumption, Overweight);
					}
				}
				return 0f;
			}),
			StartThreshold = 15f,
			Downtime = 0.1f,
			PrimaryTarget = EConsumptionTarget.Hands
		};
		Consumptions[EConsumptionType.ClimbHands] = new GClass773(EConsumptionType.ClimbHands)
		{
			Delta = new GClass849<float>(delegate
			{
				float num = Player_0.MovementContext.PlayerAnimatorGetObstacleHeight();
				if (num > Singleton<BackendConfigSettingsClass>.Instance.VaultingSettings.MovesSettings.ClimbSettings.MaxOneHandHeight)
				{
					switch (Epose_0)
					{
					case EPose.Stand:
						return GClass2298.Evaluate(base.StaminaParameters.ClimbTwoHandsConsumption, WalkOverweight);
					case EPose.Sit:
						return GClass2298.Evaluate(base.StaminaParameters.ClimbTwoHandsConsumption, Overweight);
					}
				}
				else if (num > Singleton<BackendConfigSettingsClass>.Instance.VaultingSettings.MovesSettings.ClimbSettings.MaxWithoutHandHeight)
				{
					switch (Epose_0)
					{
					case EPose.Stand:
						return GClass2298.Evaluate(base.StaminaParameters.ClimbOneHandConsumption, WalkOverweight);
					case EPose.Sit:
						return GClass2298.Evaluate(base.StaminaParameters.ClimbOneHandConsumption, Overweight);
					}
				}
				return 0f;
			}),
			StartThreshold = 15f,
			Downtime = 0.1f,
			PrimaryTarget = EConsumptionTarget.Hands
		};
	}

	public void method_9()
	{
		PoseLevelIncreaseSpeed.SetDirty();
		PoseLevelDecreaseSpeed.SetDirty();
	}

	public void method_10()
	{
		Consumptions[EConsumptionType.Aim].Delta.SetDirty();
	}

	public void method_11()
	{
		for (int num = Oxygen.Consumptions.Count - 1; num >= 0; num--)
		{
			if ((Oxygen.Consumptions[num].Requires & EConsumptionTarget.Hands) != 0)
			{
				Oxygen.Consumptions[num].SetActive(this, isActive: false);
			}
		}
	}

	public void method_12()
	{
		for (int num = Stamina.Consumptions.Count - 1; num >= 0; num--)
		{
			if ((Stamina.Consumptions[num].Requires & EConsumptionTarget.Base) != 0)
			{
				Stamina.Consumptions[num].SetActive(this, isActive: false);
			}
		}
	}

	public void method_13()
	{
		for (int num = Oxygen.Consumptions.Count - 1; num >= 0; num--)
		{
			if ((Oxygen.Consumptions[num].Requires & EConsumptionTarget.Oxygen) != 0)
			{
				Oxygen.Consumptions[num].SetActive(this, isActive: false);
			}
		}
	}

	public void method_14()
	{
		Consumptions[EConsumptionType.HoldBreath].Delta.SetDirty();
	}

	public override void StandUp()
	{
		Consumptions[EConsumptionType.StandUp].Consume(this);
	}

	public void method_15()
	{
		Bool_8 = Stamina.Consumptions.Contains(Consumptions[EConsumptionType.Sprint]);
		method_2();
	}

	public void method_16(EPlayerState previousState, EPlayerState nextState)
	{
		switch (nextState)
		{
		case EPlayerState.Run:
			Consumptions[EConsumptionType.Walk].SetActive(this, isActive: true);
			break;
		case EPlayerState.ProneMove:
			if (Overweight >= 1f)
			{
				Consumptions[EConsumptionType.Prone].SetActive(this, isActive: true);
			}
			break;
		}
		switch (previousState)
		{
		case EPlayerState.Run:
			Consumptions[EConsumptionType.Walk].SetActive(this, isActive: false);
			break;
		case EPlayerState.ProneMove:
			Consumptions[EConsumptionType.Prone].SetActive(this, isActive: false);
			break;
		}
	}

	public override void PhysicalConditionChanged(EPhysicalCondition diff, EPhysicalCondition full)
	{
		switch (diff)
		{
		case EPhysicalCondition.LeftLegDamaged:
		case EPhysicalCondition.RightLegDamaged:
			MinStepSound.SetDirty();
			break;
		case EPhysicalCondition.LeftArmDamaged:
		case EPhysicalCondition.RightArmDamaged:
			Consumptions[EConsumptionType.Aim].Delta.SetDirty();
			break;
		}
	}

	public void method_17(int i)
	{
		EPose epose_ = Epose_0;
		Epose_0 = (EPose)i;
		if (Epose_0 != epose_)
		{
			Consumptions[EConsumptionType.OverweightIdle].Delta.SetDirty();
			Consumptions[EConsumptionType.Aim].Delta.SetDirty();
			Consumptions[EConsumptionType.Walk].Delta.SetDirty();
			Stamina.SelfRestoration.SetDirty();
			HandsStamina.SelfRestoration.SetDirty();
		}
	}

	public override void OnWeightUpdated()
	{
		BackendConfigSettingsClass.InertiaSettings inertia = Singleton<BackendConfigSettingsClass>.Instance.Inertia;
		float num = (IobserverToPlayerBridge_0.Skills.StrengthBuffElite ? Player_0.InventoryController.Inventory.TotalWeightEliteSkill : Player_0.InventoryController.Inventory.TotalWeight);
		Inertia = CalculateValue(BaseInertiaLimits, num);
		SprintAcceleration = GClass2298.InverseLerp(inertia.SprintAccelerationLimits, Inertia);
		PreSprintAcceleration = GClass2298.Evaluate(inertia.PreSprintAccelerationLimits, Inertia);
		float time = Mathf.Lerp(inertia.MinMovementAccelerationRangeRight.x, inertia.MaxMovementAccelerationRangeRight.x, Inertia);
		float value = Mathf.Lerp(inertia.MinMovementAccelerationRangeRight.y, inertia.MaxMovementAccelerationRangeRight.y, Inertia);
		EFTHardSettings.Instance.MovementAccelerationRange.MoveKey(1, new Keyframe(time, value));
		Overweight = GClass2298.InverseLerp(BaseOverweightLimits, num);
		WalkOverweight = GClass2298.InverseLerp(WalkOverweightLimits, num);
		WalkSpeedLimit = 1f - GClass2298.InverseLerp(WalkSpeedOverweightLimits, num);
		Float_3 = GClass2298.InverseLerp(SprintOverweightLimits, num);
		MoveSideInertia = GClass2298.Evaluate(inertia.SideTime, Inertia);
		MoveDiagonalInertia = GClass2298.Evaluate(inertia.DiagonalTime, Inertia);
		if (Player_0.IsAI)
		{
			Float_3 = 0f;
		}
		MaxPoseLevel = ((Overweight >= 1f) ? 0.9f : 1f);
		Consumptions[EConsumptionType.OverweightIdle].SetActive(this, Overweight >= 1f);
		Consumptions[EConsumptionType.OverweightIdle].Delta.SetDirty();
		Consumptions[EConsumptionType.SitToStand].AllowsRestoration = Overweight >= 1f;
		Consumptions[EConsumptionType.StandUp].AllowsRestoration = Overweight >= 1f;
		Consumptions[EConsumptionType.Walk].Delta.SetDirty();
		Consumptions[EConsumptionType.Sprint].Delta.SetDirty();
		Consumptions[EConsumptionType.VaultLegs].Delta.SetDirty();
		Consumptions[EConsumptionType.VaultHands].Delta.SetDirty();
		Consumptions[EConsumptionType.ClimbLegs].Delta.SetDirty();
		Consumptions[EConsumptionType.ClimbHands].Delta.SetDirty();
		TransitionSpeed.SetDirty();
		PoseLevelDecreaseSpeed.SetDirty();
		PoseLevelIncreaseSpeed.SetDirty();
		FallDamageMultiplier = Mathf.Lerp(1f, base.StaminaParameters.FallDamageMultiplier, Overweight);
		SoundRadius = GClass2298.Evaluate(base.StaminaParameters.SoundRadius, Overweight);
		MinStepSound.SetDirty();
		method_3();
		method_7(num);
	}

	public void method_18()
	{
		Stamina.SelfRestoration.SetDirty();
		HandsStamina.SelfRestoration.SetDirty();
		Stamina.TotalCapacity.SetDirty();
		Oxygen.TotalCapacity.SetDirty();
		Oxygen.SelfRestoration.SetDirty();
		Consumptions[EConsumptionType.OverweightIdle].Delta.SetDirty();
	}

	public void method_19(float delta)
	{
		Stamina.SelfRestoration.SetDirty();
		HandsStamina.SelfRestoration.SetDirty();
		Consumptions[EConsumptionType.OverweightIdle].Delta.SetDirty();
		Oxygen.TotalCapacity.SetDirty();
	}

	public void method_20(float delta)
	{
		Consumptions[EConsumptionType.Jump].Delta.SetDirty();
		Consumptions[EConsumptionType.Sprint].Delta.SetDirty();
		Stamina.SelfRestoration.SetDirty();
		HandsStamina.SelfRestoration.SetDirty();
	}

	public override float GetStaminaRestorationFunc()
	{
		return method_21(StaminaRestoreRate);
	}

	public override float GetHandsRestorationFunc()
	{
		return method_21(HandsRestoreRate);
	}

	public float method_21(float baseValue)
	{
		return baseValue * Float_7[(int)Epose_0] * Singleton<BackendConfigSettingsClass>.Instance.StaminaRestoration.GetAt(Player_0.HealthController.Energy.Normalized) * ((float)IobserverToPlayerBridge_0.Skills.EnduranceBuffRestoration + 1f) / Single_0;
	}

	public override float GetStaminaCapacityFunc()
	{
		return method_22(StaminaCapacity);
	}

	public override float GetHandsCapacityFunc()
	{
		return method_23(HandsCapacity);
	}

	public float method_22(float baseValue)
	{
		return Mathf.Max(1f, baseValue + baseValue * (float)IobserverToPlayerBridge_0.Skills.EnduranceBuffEnduranceInc + CapacityBuff);
	}

	public float method_23(float baseValue)
	{
		return Mathf.Max(1f, baseValue + baseValue * (float)IobserverToPlayerBridge_0.Skills.EnduranceHands + CapacityBuff);
	}

	public override float GetOxygenRestorationFunc()
	{
		return OxygenRestoreRate + OxygenRestoreRate * (float)IobserverToPlayerBridge_0.Skills.EnduranceBuffRestoration;
	}

	public override float GetOxygenCapacityFunc()
	{
		return OxygenCapacity * (0.5f + 0.5f * Mathf.Max(Player_0.HealthController.Energy.Normalized, IobserverToPlayerBridge_0.Skills.EnduranceBreathElite) * (2f * (float)IobserverToPlayerBridge_0.Skills.EnduranceBuffBreathTimeInc + Player_0.HealthController.GetBodyPartHealth(EBodyPart.Common).Normalized));
	}

	public override void Sprint(bool target)
	{
		bool flag;
		if (target != Sprinting && (flag = target && CanSprint) != Sprinting)
		{
			Consumptions[EConsumptionType.Sprint].SetActive(this, flag);
			if (flag)
			{
				Stamina.Consume(Gclass773_0);
			}
		}
	}

	public void method_24()
	{
		Stamina.Overuse = 0f;
		HandsStamina.Overuse = 0f;
		Oxygen.Overuse = 0f;
	}

	public override void Aim(float mass = 0f)
	{
		Float_5 = mass;
		Consumptions[EConsumptionType.Aim].Delta.SetDirty();
		Consumptions[EConsumptionType.Aim].SetActive(this, Float_5 > 0f);
		if (Float_5 <= 0f)
		{
			Stamina.RemoveConsumption(Consumptions[EConsumptionType.Aim]);
		}
	}

	public override void HoldBreath(bool enable)
	{
		if (!(Stamina.Exhausted && enable))
		{
			bool holdingBreath = HoldingBreath;
			GClass773 gClass = Consumptions[EConsumptionType.HoldBreath];
			bool flag = enable && Oxygen.CanAdd(gClass);
			gClass.SetActive(this, flag);
			if (flag != holdingBreath)
			{
				method_5();
			}
		}
	}

	public override void Update(float dt)
	{
		Stamina.Process(dt);
		HandsStamina.Process(dt);
		Oxygen.Process(dt);
	}

	public override void OnBreach()
	{
		OnJump(enabled: true);
	}

	public override void OnThrow(bool lowThrow)
	{
		ConsumeAsMelee(IobserverToPlayerBridge_0.Skills.GetBuffedGrenadeExpense(lowThrow ? base.StaminaParameters.GrenadeLowThrow : base.StaminaParameters.GrenadeHighThrow));
	}

	public override void ConsumePoseLevelChange(float notches)
	{
		Float_11 = notches;
		Consumptions[EConsumptionType.SitToStand].Consume(this);
	}

	public override void ConsumeAsMelee(float expense)
	{
		if (!(expense <= 0f))
		{
			Float_9 = expense;
			Consumptions[EConsumptionType.Melee].Consume(this);
		}
	}

	public override void OnJump(bool enabled)
	{
		if (enabled)
		{
			Consumptions[EConsumptionType.Jump].Consume(this);
		}
	}

	public override void OnVault()
	{
		Consumptions[EConsumptionType.VaultLegs].Delta.SetDirty();
		Consumptions[EConsumptionType.VaultHands].Delta.SetDirty();
		Consumptions[EConsumptionType.VaultLegs].Consume(this);
		Consumptions[EConsumptionType.VaultHands].Consume(this);
	}

	public override void OnClimb()
	{
		Consumptions[EConsumptionType.ClimbLegs].Delta.SetDirty();
		Consumptions[EConsumptionType.ClimbHands].Delta.SetDirty();
		Consumptions[EConsumptionType.ClimbLegs].Consume(this);
		Consumptions[EConsumptionType.ClimbHands].Consume(this);
	}

	public override void BulletHit(float staminaBurnRate)
	{
		Float_10 = staminaBurnRate;
		Consumptions[EConsumptionType.Wound].Consume(this, fromHit: true);
	}

	public override void OnWeaponSwitchFast(int level)
	{
		Int_0 = level;
		Consumptions[EConsumptionType.FastWeaponSwitch].Consume(this);
	}
}
