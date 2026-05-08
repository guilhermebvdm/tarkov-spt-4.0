using System;
using System.Runtime.CompilerServices;
using EFT;
using EFT.InventoryLogic;
using JetBrains.Annotations;
using UnityEngine;

public class BotScatteringDataClass : GClass429
{
	[NonSerialized]
	public float Float_0;

	[NonSerialized]
	public bool Bool_0;

	[NonSerialized]
	public bool Bool_1;

	[NonSerialized]
	public bool Bool_2;

	[NonSerialized]
	public bool Bool_3;

	[NonSerialized]
	public bool Bool_4;

	[NonSerialized]
	public bool Bool_5;

	[NonSerialized]
	public BotLastBlindEffectModifierClass BotLastBlindEffectModifierClass;

	[NonSerialized]
	[CompilerGenerated]
	public float Float_1;

	[NonSerialized]
	[CompilerGenerated]
	public bool Bool_6;

	public float YRecoil
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

	public float CurScatering
	{
		get
		{
			return Float_0;
		}
		set
		{
			bool isPeace = BotOwner_0.Memory.IsPeace;
			float currentWorkingScatter = BotOwner_0.Settings.Current.CurrentWorkingScatter;
			float currentMinScatter = BotOwner_0.Settings.Current.CurrentMinScatter;
			float currentMaxScatter = BotOwner_0.Settings.Current.CurrentMaxScatter;
			if (isPeace)
			{
				IsMinScattering = false;
				if (currentWorkingScatter <= currentMaxScatter && currentWorkingScatter >= currentMinScatter)
				{
					Float_0 = Mathf.Clamp(value, currentWorkingScatter, currentMaxScatter);
				}
				else
				{
					Float_0 = Mathf.Clamp(value, currentMinScatter, currentMaxScatter);
				}
			}
			else if (value <= currentMinScatter)
			{
				IsMinScattering = true;
				Float_0 = currentMinScatter;
			}
			else if (value >= currentMaxScatter)
			{
				IsMinScattering = false;
				Float_0 = currentMaxScatter;
			}
			else
			{
				IsMinScattering = false;
				Float_0 = value;
			}
		}
	}

	public bool IsMinScattering
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

	public BotScatteringDataClass([NotNull] BotOwner owner)
		: base(owner)
	{
	}

	public void Activate()
	{
		float handDamageScatteringMinMax = BotOwner_0.Settings.FileSettings.Scattering.HandDamageScatteringMinMax;
		float handDamageAccuracySpeed = BotOwner_0.Settings.FileSettings.Scattering.HandDamageAccuracySpeed;
		BotOwner_0.Medecine.FirstAid.OnNoDamagedParts += method_0;
		BotLastBlindEffectModifierClass = new BotLastBlindEffectModifierClass(1f, handDamageAccuracySpeed, 1f, 1f, 1f, handDamageScatteringMinMax, 1f, 1f, 1f);
		Float_0 = BotOwner_0.Settings.Current.CurrentMaxScatter;
	}

	public void ShootDone(Weapon weapon)
	{
		float recoilTotal = weapon.RecoilTotal;
		YRecoil += recoilTotal * BotOwner_0.Settings.FileSettings.Scattering.RecoilYCoef;
		float num = (BotOwner_0.WeaponManager.IsNowAutomatic ? BotOwner_0.Settings.FileSettings.Scattering.RecoilControlCoefShootDoneAuto : BotOwner_0.Settings.FileSettings.Scattering.RecoilControlCoefShootDone);
		float num2 = recoilTotal * num;
		CurScatering += num2;
	}

	public void ManualUpdate(bool hardAim)
	{
		YRecoil = Mathf.Clamp(YRecoil + BotOwner_0.Settings.FileSettings.Scattering.RecoilYCoefSppedDown * Time.deltaTime, 0f, BotOwner_0.Settings.FileSettings.Scattering.RecoilYMax);
		if (BotOwner_0.FlashGrenade.IsFlashed || BotOwner_0.SmokeGrenade.IsInSmoke || (Bool_4 && Bool_5))
		{
			Bool_1 = true;
		}
		float num = 1f;
		if (hardAim)
		{
			num *= BotOwner_0.Settings.FileSettings.Scattering.SpeedUpAim;
		}
		if (Bool_0)
		{
			num *= BotOwner_0.Settings.FileSettings.Scattering.TracerCoef;
		}
		if (Bool_2)
		{
			num *= BotOwner_0.Settings.FileSettings.Scattering.MovingSlowCoef;
		}
		float num2 = 0f;
		num2 = ((BotOwner_0.Mover.Sprinting || Bool_3) ? BotOwner_0.Settings.FileSettings.Scattering.SpeedDown : (BotOwner_0.Settings.Current.CurrentScatteringUp * num));
		if (num2 < 0f)
		{
			CurScatering -= num2 * Time.deltaTime;
		}
		else if (!Bool_1)
		{
			CurScatering -= num2 * Time.deltaTime;
		}
		Bool_1 = false;
		Bool_3 = false;
	}

	public void GetHit(DamageInfoStruct dmgInfo)
	{
		CurScatering += dmgInfo.Damage * BotOwner_0.Settings.FileSettings.Scattering.FromShot;
		if (!BotLastBlindEffectModifierClass.IsApplyed && (BotOwner_0.Medecine.FirstAid.IsPartDamaged(EBodyPart.LeftArm) || BotOwner_0.Medecine.FirstAid.IsPartDamaged(EBodyPart.RightArm)))
		{
			BotOwner_0.Settings.Current.Apply(BotLastBlindEffectModifierClass);
		}
		if (!Bool_4)
		{
			Bool_4 = BotOwner_0.Medecine.FirstAid.IsPartDamaged(EBodyPart.LeftLeg) || BotOwner_0.Medecine.FirstAid.IsPartDamaged(EBodyPart.RightLeg);
		}
	}

	public void RotateX(float angToRotate)
	{
		method_1(angToRotate);
	}

	public void RotateY(float angToRotate)
	{
	}

	public void BotMove(float dist)
	{
		Bool_2 = false;
		float num = dist / Time.deltaTime;
		if (num > BotOwner_0.Settings.FileSettings.Scattering.ToUpBotSpeed)
		{
			Bool_5 = true;
			Bool_3 = true;
		}
		else if (num > BotOwner_0.Settings.FileSettings.Scattering.ToLowBotSpeed)
		{
			Bool_5 = true;
			Bool_1 = true;
		}
		else if (num > BotOwner_0.Settings.FileSettings.Scattering.ToSlowBotSpeed)
		{
			Bool_5 = true;
			Bool_2 = true;
		}
		else
		{
			Bool_5 = false;
		}
	}

	public void DrawGizmosSelected()
	{
		Vector3 position = BotOwner_0.MyHead.position;
		Vector3 realTargetPoint = BotOwner_0.AimingManager.CurrentAiming.RealTargetPoint;
		float magnitude = (realTargetPoint - position).magnitude;
		Gizmos.color = new Color(1f, 1f, 0f, 0.6f);
		float radius = magnitude * BotOwner_0.Settings.Current.CurrentMaxScatter;
		Gizmos.DrawWireSphere(realTargetPoint, radius);
		Gizmos.color = new Color(0f, 1f, 1f);
		float radius2 = magnitude * BotOwner_0.Settings.Current.CurrentMinScatter;
		Gizmos.DrawWireSphere(realTargetPoint, radius2);
	}

	public void SetTracers(bool isTracers)
	{
		Bool_0 = isTracers;
	}

	public void PoseChange(float f)
	{
		CurScatering += f * BotOwner_0.Settings.FileSettings.Scattering.PoseChnageCoef;
	}

	public void Lay(bool b)
	{
		CurScatering += BotOwner_0.Settings.FileSettings.Scattering.LayFactor;
	}

	public void method_0()
	{
		if (BotLastBlindEffectModifierClass.IsApplyed)
		{
			BotOwner_0.Settings.Current.Dismiss(BotLastBlindEffectModifierClass);
		}
		if (Bool_4)
		{
			Bool_4 = false;
		}
	}

	public void method_1(float angToRotate)
	{
		float num = Mathf.Abs(angToRotate) / Time.deltaTime;
		if (num > BotOwner_0.Settings.FileSettings.Scattering.ToLowBotAngularSpeed)
		{
			Bool_3 = true;
		}
		else if (num > BotOwner_0.Settings.FileSettings.Scattering.ToStopBotAngularSpeed)
		{
			Bool_1 = true;
		}
	}

	public void Dispose()
	{
		BotOwner_0.Medecine.FirstAid.OnNoDamagedParts -= method_0;
	}
}
