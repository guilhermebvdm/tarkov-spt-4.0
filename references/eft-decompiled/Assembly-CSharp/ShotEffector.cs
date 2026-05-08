using System;
using System.Linq;
using Comfort.Common;
using EFT;
using EFT.Animations;
using EFT.Animations.NewRecoil;
using EFT.Animations.Recoil;
using EFT.InventoryLogic;
using FitstPersonAnimations.WeaponAnimation.Effectors.Recoil;
using UnityEngine;

[Serializable]
public class ShotEffector : IEffector
{
	[Serializable]
	public class RecoilShotVal
	{
		public ComponentType FromAxis;

		public Target ProcessType;

		public ComponentType ToAxis;

		public float IntensityMultiplicator;

		[NonSerialized]
		public IRecoilShotEffect RecoilShotEffectTarget;

		[NonSerialized]
		public int Component;

		[NonSerialized]
		public int From;

		public void Initialize(IRecoilShotEffect recoilShotEffect)
		{
			RecoilShotEffectTarget = recoilShotEffect;
			Component = (int)ToAxis;
			From = (int)FromAxis;
		}

		public void Process(Vector3 rnd)
		{
			if (RecoilShotEffectTarget != null)
			{
				if (ProcessType == Target.HandsPosition)
				{
					RecoilShotEffectTarget.HandPositionRecoilEffect.AddAccelerationLimitless(Component, rnd[From] * IntensityMultiplicator);
				}
				else if (ProcessType == Target.HandsRotation)
				{
					RecoilShotEffectTarget.HandRotationRecoilEffect.AddAccelerationLimitless(Component, rnd[From] * IntensityMultiplicator);
				}
				else if (ProcessType == Target.CameraRotation)
				{
					RecoilShotEffectTarget.CameraRotationRecoilEffect.AddAccelerationLimitless(Component, rnd[From] * IntensityMultiplicator);
				}
			}
		}
	}

	public RecoilPipelineType RecoilPipeline = RecoilPipelineType.New;

	public OldRecoilShotEffect OldShotRecoil;

	public NewRecoilShotEffect NewShotRecoil;

	[NonSerialized]
	public IRecoilShotEffect CurrentRecoilEffect_1;

	[NonSerialized]
	public Player.FirearmController FirearmController;

	[NonSerialized]
	public const float RECOIL_SUPPRESSION_FACTOR = 0.1f;

	[NonSerialized]
	public IWeapon Weapon;

	[NonSerialized]
	public Weapon MainWeaponInHands;

	[NonSerialized]
	public BackendConfigSettingsClass.AimingConfiguration AimingConfig;

	[NonSerialized]
	public SkillManager.GClass2250 Buffs = new SkillManager.GClass2250();

	[NonSerialized]
	public int Pose_1 = -1;

	public IRecoilShotEffect CurrentRecoilEffect
	{
		get
		{
			if (CurrentRecoilEffect_1 == null)
			{
				if (RecoilPipeline == RecoilPipelineType.New)
				{
					SetNewRecoilMode();
				}
				else if (RecoilPipeline == RecoilPipelineType.Old)
				{
					SetOldRecoilMode();
				}
			}
			return CurrentRecoilEffect_1;
		}
		set
		{
			CurrentRecoilEffect_1 = value;
		}
	}

	public BackendConfigSettingsClass.AimingConfiguration AimingConfiguration_0
	{
		get
		{
			if (AimingConfig == null)
			{
				AimingConfig = Singleton<BackendConfigSettingsClass>.Instance.Aiming;
			}
			return AimingConfig;
		}
	}

	public int Pose
	{
		get
		{
			return Pose_1;
		}
		set
		{
			Pose_1 = value;
			float z = ((Weapon == null) ? AimingConfiguration_0.RecoilZIntensityByPose[Pose_1] : Mathf.Min(Weapon.WeaponTemplate.RecoilPosZMult, AimingConfiguration_0.RecoilZIntensityByPose[Pose_1]));
			CurrentRecoilEffect.SeparateIntensityFactors = new Vector3(AimingConfiguration_0.RecoilXIntensityByPose[Pose_1], AimingConfiguration_0.RecoilYIntensityByPose[Pose_1], z);
		}
	}

	public float ConvertFromTaxanomy(float f)
	{
		return f * 0.1399f;
	}

	public void Initialize(PlayerSpring playerSpring)
	{
		RecoilShotVal[] shotRecoilProcessValues = OldShotRecoil.ShotRecoilProcessValues;
		for (int i = 0; i < shotRecoilProcessValues.Length; i++)
		{
			shotRecoilProcessValues[i].Initialize(OldShotRecoil);
		}
		OldShotRecoil.BasicRecoilRadian = OldShotRecoil.BasicPlayerRecoilDegreeRange * (MathF.PI / 180f);
		shotRecoilProcessValues = NewShotRecoil.ShotRecoilProcessValues;
		for (int i = 0; i < shotRecoilProcessValues.Length; i++)
		{
			shotRecoilProcessValues[i].Initialize(NewShotRecoil);
		}
		NewShotRecoil.BasicRecoilRadian = NewShotRecoil.BasicPlayerRecoilDegreeRange * (MathF.PI / 180f);
	}

	public void Process(float str = 1f)
	{
		CurrentRecoilEffect.AddRecoilForce(str);
	}

	public void method_0(IWeapon activeWeapon, SkillManager.GClass2250 buffInfo, Weapon mainWeaponInHands, Player.FirearmController firearms)
	{
		FirearmController = firearms;
		if (buffInfo != null)
		{
			Buffs = buffInfo;
		}
		if (Weapon != null)
		{
			Weapon.Item.Attributes.ElementAt(0).OnUpdate -= OnWeaponParametersChanged;
		}
		Weapon = activeWeapon;
		Weapon.Item.Attributes.ElementAt(0).OnUpdate += OnWeaponParametersChanged;
		Weapon.Item.UpdateAttributes();
		MainWeaponInHands = mainWeaponInHands;
		CurrentRecoilEffect?.CalculateBaseRecoilParameters(Buffs.RecoilSupression.y, Weapon.WeaponTemplate.RecoilAngle, Weapon.WeaponTemplate.RecolDispersion, Weapon.WeaponTemplate.ShotsGroupSettings.ToList());
	}

	public float method_1()
	{
		float num = Weapon.RecoilDelta;
		if (Weapon.IsUnderbarrelWeapon)
		{
			num += MainWeaponInHands.StockRecoilDelta;
		}
		return num;
	}

	public void OnWeaponParametersChanged()
	{
		CurrentRecoilEffect?.RecalculateRecoilParamsOnChangeWeapon(Weapon.WeaponTemplate, AimingConfiguration_0, FirearmController, Buffs.RecoilSupression.x, Buffs.RecoilSupression.y, 0.1f, method_1());
	}

	public string DebugOutput()
	{
		throw new NotImplementedException();
	}

	public void SetOldRecoilMode()
	{
		NewShotRecoil.RecoilEffectOn = false;
		CurrentRecoilEffect = OldShotRecoil;
		OldShotRecoil.RecoilEffectOn = true;
	}

	public void SetNewRecoilMode()
	{
		OldShotRecoil.RecoilEffectOn = false;
		CurrentRecoilEffect = NewShotRecoil;
		NewShotRecoil.RecoilEffectOn = true;
	}

	public void CalculateRecoilParameters()
	{
		CurrentRecoilEffect.CalculateBaseRecoilParameters(Buffs.RecoilSupression.y, Weapon.WeaponTemplate.RecoilAngle, Weapon.WeaponTemplate.RecolDispersion, Weapon.WeaponTemplate.ShotsGroupSettings.ToList());
	}
}
