using System;
using System.Runtime.CompilerServices;
using System.Threading;
using EFT;
using EFT.InventoryLogic;
using JetBrains.Annotations;
using UnityEngine;

public class GClass530 : GClass429, IBotAiming
{
	public bool _alwaysTurnOnLight;

	[NonSerialized]
	public float Float_0;

	[NonSerialized]
	public bool Bool_0;

	[NonSerialized]
	public bool Bool_1;

	[NonSerialized]
	public bool Bool_2;

	[NonSerialized]
	public Vector3 Vector3_0;

	[NonSerialized]
	public BotLastBlindEffectModifierClass BotLastBlindEffectModifierClass;

	[NonSerialized]
	public IFirearmHandsController IfirearmHandsController_0;

	[NonSerialized]
	public AimStatus AimStatus_0 = AimStatus.NoTarget;

	[NonSerialized]
	public float Float_1;

	[NonSerialized]
	public bool Bool_3;

	[CompilerGenerated]
	private Action<Vector3> action_0;

	[NonSerialized]
	[CompilerGenerated]
	public bool Bool_4;

	[NonSerialized]
	[CompilerGenerated]
	public Vector3 Vector3_1;

	[NonSerialized]
	[CompilerGenerated]
	public float Float_2;

	[NonSerialized]
	[CompilerGenerated]
	public Vector3 Vector3_2;

	[NonSerialized]
	[CompilerGenerated]
	public Vector3 Vector3_3;

	[NonSerialized]
	[CompilerGenerated]
	public BotScatteringDataClass BotScatteringDataClass;

	[NonSerialized]
	[CompilerGenerated]
	public Vector3 Vector3_4;

	[NonSerialized]
	[CompilerGenerated]
	public float Float_3;

	public bool HardAim
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

	public Vector3 From
	{
		[CompilerGenerated]
		get
		{
			return Vector3_1;
		}
		[CompilerGenerated]
		set
		{
			Vector3_1 = value;
		}
	}

	public float SinDist
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

	public Vector3 To
	{
		[CompilerGenerated]
		get
		{
			return Vector3_2;
		}
		[CompilerGenerated]
		set
		{
			Vector3_2 = value;
		}
	}

	public Vector3 TargetToLook
	{
		[CompilerGenerated]
		get
		{
			return Vector3_3;
		}
		[CompilerGenerated]
		set
		{
			Vector3_3 = value;
		}
	}

	public BotScatteringDataClass ScatteringData
	{
		[CompilerGenerated]
		get
		{
			return BotScatteringDataClass;
		}
	}

	public Vector3 EndTargetPoint => TargetToLook;

	public bool AlwaysTurnOnLight => _alwaysTurnOnLight;

	public bool IsReady
	{
		get
		{
			if (!Bool_0 && !Bool_1)
			{
				if (ScatteringData.IsMinScattering)
				{
					return true;
				}
				float float_ = Float_0;
				return ScatteringData.CurScatering <= float_;
			}
			return false;
		}
	}

	public Vector3 RealTargetPoint
	{
		[CompilerGenerated]
		get
		{
			return Vector3_4;
		}
		[CompilerGenerated]
		set
		{
			Vector3_4 = value;
		}
	}

	public float LastDist2Target
	{
		[CompilerGenerated]
		get
		{
			return Float_3;
		}
		[CompilerGenerated]
		set
		{
			Float_3 = value;
		}
	}

	public event Action<Vector3> OnSettingsTarget
	{
		[CompilerGenerated]
		add
		{
			Action<Vector3> action = action_0;
			Action<Vector3> action2;
			do
			{
				action2 = action;
				Action<Vector3> value2 = (Action<Vector3>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<Vector3> action = action_0;
			Action<Vector3> action2;
			do
			{
				action2 = action;
				Action<Vector3> value2 = (Action<Vector3>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public GClass530([NotNull] BotOwner owner)
		: base(owner)
	{
		BotScatteringDataClass = new BotScatteringDataClass(owner);
	}

	public void GetHit(DamageInfoStruct damageInfo)
	{
		ScatteringData.GetHit(damageInfo);
	}

	public void Activate()
	{
		IfirearmHandsController_0 = BotOwner_0.GetPlayer.HandsController as IFirearmHandsController;
		Bool_2 = UnityEngine.Random.Range(0, 100) < 50;
		BotOwner_0.ShootData.OnTriggerPressed += method_3;
		BotOwner_0.Mover.OnPoseChange += method_2;
		BotOwner_0.BotLay.OnLay += method_1;
		ScatteringData.Activate();
		BotLastBlindEffectModifierClass = new BotLastBlindEffectModifierClass(1f, 1f, 1f, 1f, 1f, 1f, 1f, BotOwner_0.Settings.FileSettings.Scattering.ToCaution, 1f);
	}

	public void NodeUpdate()
	{
		AimStatus_0 = AimStatus.Aiming;
		method_6(RealTargetPoint, BotOwner_0.MyHead.position);
	}

	public void LoseTarget()
	{
		AimStatus_0 = AimStatus.NoTarget;
		if (HardAim)
		{
			HardAim = false;
			IfirearmHandsController_0.SetAim(HardAim);
		}
	}

	public void SetTarget(Vector3 trg)
	{
		float sqrMagnitude = (trg - RealTargetPoint).sqrMagnitude;
		Bool_0 = sqrMagnitude > BotOwner_0.Settings.FileSettings.Scattering.DIST_FROM_OLD_POINT_TO_NOT_AIM_SQRT;
		RealTargetPoint = trg;
	}

	public void SetNextAimingDelay(float nextAimingDelay)
	{
	}

	public void TriggerPressedDone()
	{
	}

	public void ShootDone(Weapon weapon)
	{
		ScatteringData.ShootDone(weapon);
	}

	public void DrawGizmosSelected()
	{
		ScatteringData.DrawGizmosSelected();
		Gizmos.color = new Color(0.3f, 0.3f, 0.9f, 1f);
		Gizmos.DrawSphere(To, 0.11f);
		Gizmos.color = new Color(0.3f, 0.3f, 0.9f, 1f);
		Gizmos.DrawSphere(From, 0.11f);
		float num = method_4(LastDist2Target);
		Gizmos.color = new Color(0f, 0.95f, 0f, 1f);
		Gizmos.DrawWireSphere(RealTargetPoint, ScatteringData.CurScatering * LastDist2Target);
		Gizmos.color = (IsReady ? new Color(0.2f, 0.8f, 0.2f, 0.9f) : new Color(0.8f, 0.2f, 0.2f, 0.9f));
		Gizmos.DrawWireSphere(RealTargetPoint, num * LastDist2Target);
		if (IsReady)
		{
			Gizmos.color = new Color(0.1f, 1f, 0.1f, 1f);
		}
		else
		{
			Gizmos.color = new Color(1f, 0.1f, 0.1f, 1f);
		}
		Gizmos.DrawLine(TargetToLook, TargetToLook + new Vector3(0f, ScatteringData.YRecoil, 0f));
		Gizmos.DrawSphere(TargetToLook, 0.21f);
	}

	public void ManualUpdate()
	{
		method_0();
		ScatteringData.ManualUpdate(HardAim);
	}

	public void RotateX(float angToRotate)
	{
		ScatteringData.RotateX(angToRotate);
	}

	public void RotateY(float deltaAngle)
	{
		ScatteringData.RotateY(deltaAngle);
	}

	public void SetWeapon(Weapon weapon)
	{
		MagazineItemClass currentMagazine = weapon.GetCurrentMagazine();
		bool tracers = false;
		if (currentMagazine != null && currentMagazine.Cartridges.Count > 0 && currentMagazine.Cartridges != null && currentMagazine.Cartridges.Last != null)
		{
			tracers = ((AmmoItemClass)currentMagazine.Cartridges.Last).Tracer;
		}
		SetTracers(tracers);
		BotOwner_0.Settings.CurrentScatteringSetting.SetWeapon(weapon);
	}

	public void SetTracers(bool isTracers)
	{
		ScatteringData.SetTracers(isTracers);
	}

	public void Move(float delta = 0f)
	{
	}

	public void NextShotMiss(int missCount)
	{
	}

	public void OnDrawGizmos()
	{
	}

	public void DebugDraw()
	{
	}

	public void method_0()
	{
		bool flag;
		if ((flag = ((Bool_2 && BotOwner_0.Tactic.IsCurTactic(BotsGroup.BotCurrentTactic.Attack)) || BotOwner_0.Memory.IsInCover) && AimStatus_0 != AimStatus.NoTarget) != HardAim)
		{
			IfirearmHandsController_0.SetAim(flag);
		}
		HardAim = flag;
	}

	public void method_1(bool obj)
	{
		ScatteringData.Lay(obj);
	}

	public void method_2(float obj)
	{
		ScatteringData.PoseChange(obj);
	}

	public void method_3()
	{
		int bulletCount = BotOwner_0.WeaponManager.Reload.BulletCount;
		int maxBulletCount = BotOwner_0.WeaponManager.Reload.MaxBulletCount;
		if ((float)bulletCount / (float)maxBulletCount < BotOwner_0.Settings.FileSettings.Scattering.Caution)
		{
			BotOwner_0.Settings.Current.Apply(BotLastBlindEffectModifierClass);
		}
		else
		{
			BotOwner_0.Settings.Current.Dismiss(BotLastBlindEffectModifierClass);
		}
	}

	public float method_4(float dist)
	{
		if (dist < BotOwner_0.Settings.Current.PriorityScatter1meter)
		{
			return BotOwner_0.Settings.Current.PriorityScatter1meter;
		}
		if (dist < BotOwner_0.Settings.Current.PriorityScatter10meter)
		{
			return method_5(dist, 1f, 10f, BotOwner_0.Settings.Current.PriorityScatter1meter, BotOwner_0.Settings.Current.PriorityScatter10meter);
		}
		if (dist < BotOwner_0.Settings.Current.PriorityScatter100meter)
		{
			return method_5(dist, 10f, 100f, BotOwner_0.Settings.Current.PriorityScatter1meter, BotOwner_0.Settings.Current.PriorityScatter10meter);
		}
		return BotOwner_0.Settings.Current.PriorityScatter100meter;
	}

	public float method_5(float cur, float min, float max, float lowVal, float hightVal)
	{
		float num = (cur - min) / (max - min);
		return (hightVal - lowVal) * num + lowVal;
	}

	public void method_6(Vector3 trg, Vector3 center)
	{
		Vector3 v = trg - center;
		LastDist2Target = v.magnitude;
		Bool_1 = LastDist2Target < BotOwner_0.Settings.FileSettings.Scattering.DIST_NOT_TO_SHOOT;
		Vector3_0 = GClass855.NormalizeFastSelf(v);
		float num = v.magnitude * ScatteringData.CurScatering;
		Vector3 vector = GClass855.Rotate90(Vector3_0, GClass855.SideTurn.left);
		Vector3 vector2 = GClass855.Rotate90(Vector3_0, GClass855.SideTurn.right);
		SinDist = num * BotOwner_0.Settings.FileSettings.Scattering.AMPLITUDE_FACTOR;
		From = vector * num + trg;
		To = vector2 * num + trg;
		method_7();
		Float_0 = method_4(LastDist2Target);
		Vector3 vector3 = new Vector3(TargetToLook.x, TargetToLook.y + ScatteringData.YRecoil, TargetToLook.z);
		method_8(vector3 - center);
	}

	public void method_7()
	{
		Float_1 += BotOwner_0.Settings.FileSettings.Scattering.AMPLITUDE_SPEED * Time.deltaTime;
		if (Float_1 > 1f)
		{
			Float_1 = 0f;
			Bool_3 = !Bool_3;
		}
		float num = (Bool_3 ? (1f - Float_1) : Float_1);
		TargetToLook = Vector3.Lerp(From, To, num);
		float num2 = num * (MathF.PI * 2f);
		float num3 = (Bool_3 ? Mathf.Sin(num2) : Mathf.Sin(num2 - MathF.PI));
		float num4 = SinDist * num3;
		TargetToLook = new Vector3(TargetToLook.x, TargetToLook.y + num4, TargetToLook.z);
	}

	public void method_8(Vector3 dir)
	{
		BotOwner_0.Steering.LookToDirection(dir, 500f);
		BotOwner_0.Steering.SetYByDir(dir);
	}

	public void Dispose()
	{
		ScatteringData.Dispose();
		BotOwner_0.Mover.OnPoseChange -= method_2;
		BotOwner_0.ShootData.OnTriggerPressed -= method_3;
		BotOwner_0.BotLay.OnLay -= method_1;
	}
}
