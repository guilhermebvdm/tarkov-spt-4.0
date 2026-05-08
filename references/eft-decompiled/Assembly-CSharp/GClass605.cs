using System;
using System.Runtime.CompilerServices;
using System.Threading;
using EFT;
using EFT.InventoryLogic;
using UnityEngine;

public class GClass605 : IBotAiming
{
	[NonSerialized]
	public const float Float_0 = 60f;

	[NonSerialized]
	public const float Float_1 = 0.4f;

	[NonSerialized]
	public const float Float_2 = 0.9f;

	[NonSerialized]
	public const float Float_3 = 2.25f;

	[NonSerialized]
	public const float Float_4 = 1.5f;

	public bool CanChangePart;

	[NonSerialized]
	public bool Bool_0;

	[NonSerialized]
	public float Float_5;

	[NonSerialized]
	public float Float_6;

	[NonSerialized]
	public float Float_7;

	[NonSerialized]
	public Vector3 Vector3_0;

	[NonSerialized]
	public Vector3 Vector3_1;

	[NonSerialized]
	public bool Bool_1;

	[NonSerialized]
	public float Float_8;

	[NonSerialized]
	public Vector3 Vector3_2;

	[NonSerialized]
	public float Float_9;

	[NonSerialized]
	public Vector3 Vector3_3;

	[NonSerialized]
	public BifacialTransform BifacialTransform_0;

	[NonSerialized]
	public float Float_10;

	[NonSerialized]
	public bool Bool_2;

	[NonSerialized]
	public int Int_0;

	[NonSerialized]
	public Vector3 Vector3_4;

	[NonSerialized]
	public Vector3 Vector3_5;

	[NonSerialized]
	public BotOwner BotOwner_0;

	[NonSerialized]
	public float Float_11 = -1f;

	[NonSerialized]
	public bool Bool_3;

	[NonSerialized]
	public float Float_12;

	[NonSerialized]
	public BotDifficultySettingsClass BotDifficultySettingsClass;

	[NonSerialized]
	public int Int_1;

	[NonSerialized]
	public Vector3 Vector3_6;

	[NonSerialized]
	public AimStatus AimStatus_0;

	[NonSerialized]
	public float Float_13 = 1f;

	[NonSerialized]
	public BotHitAffectClass BotHitAffectClass;

	[NonSerialized]
	public int Int_2;

	[NonSerialized]
	public int Int_3 = 2;

	[NonSerialized]
	public int Int_4;

	[CompilerGenerated]
	private Action<Vector3> action_0;

	[NonSerialized]
	[CompilerGenerated]
	public float Float_14;

	[NonSerialized]
	[CompilerGenerated]
	public float Float_15;

	[NonSerialized]
	[CompilerGenerated]
	public bool Bool_4;

	public BotUnderbarrelLauncherAimingSettings AimingSettings;

	[NonSerialized]
	[CompilerGenerated]
	public bool Bool_5;

	[NonSerialized]
	[CompilerGenerated]
	public Vector3 Vector3_7;

	[NonSerialized]
	[CompilerGenerated]
	public float Float_16;

	public float LastSpreadCount
	{
		[CompilerGenerated]
		get
		{
			return Float_14;
		}
		[CompilerGenerated]
		set
		{
			Float_14 = value;
		}
	}

	public float LastAimTime
	{
		[CompilerGenerated]
		get
		{
			return Float_15;
		}
		[CompilerGenerated]
		set
		{
			Float_15 = value;
		}
	}

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

	public bool Boolean_0 => Float_12 > Time.time;

	public AimStatus Status
	{
		get
		{
			return AimStatus_0;
		}
		set
		{
			if (AimStatus_0 != value && BotOwner_0.BotState == EBotState.Active)
			{
				AimStatus_0 = value;
				bool flag;
				if ((flag = (Bool_0 || BotOwner_0.Memory.IsInCover || method_1()) && AimStatus_0 != AimStatus.NoTarget && method_0()) != BotOwner_0.WeaponManager.ShootController.IsAiming)
				{
					BotOwner_0.WeaponManager.ShootController.SetAim(flag);
				}
				HardAim = flag;
				if (AimStatus_0 == AimStatus.AimComplete)
				{
					BotOwner_0.BotPersonalStats.Aim(EndTargetPoint, Float_7);
				}
			}
		}
	}

	public Vector3 EndTargetPoint
	{
		get
		{
			return Vector3_1;
		}
		set
		{
			Vector3_1 = value;
			method_2();
		}
	}

	public bool IsReady => AimStatus_0 == AimStatus.AimComplete;

	public bool AlwaysTurnOnLight
	{
		[CompilerGenerated]
		get
		{
			return Bool_5;
		}
		[CompilerGenerated]
		set
		{
			Bool_5 = value;
		}
	}

	public Vector3 RealTargetPoint
	{
		[CompilerGenerated]
		get
		{
			return Vector3_7;
		}
		[CompilerGenerated]
		set
		{
			Vector3_7 = value;
		}
	}

	public float LastDist2Target
	{
		[CompilerGenerated]
		get
		{
			return Float_16;
		}
		[CompilerGenerated]
		set
		{
			Float_16 = value;
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

	public GClass605(BotOwner owner)
	{
		AlwaysTurnOnLight = false;
		BotHitAffectClass = new BotHitAffectClass(owner);
		BotOwner_0 = owner;
		BifacialTransform_0 = BotOwner_0.WeaponRoot;
		BotDifficultySettingsClass = BotOwner_0.Settings;
	}

	public void Activate()
	{
		AimingSettings = BotOwner_0.Settings.FileSettings.Aiming.UnderbarrelLauncherAiming;
		Bool_0 = GClass856.IsTrue100(AimingSettings.AIMING_ON_WAY);
		AlwaysTurnOnLight = GClass856.IsTrue100(AimingSettings.ANYTIME_LIGHT_WHEN_AIM_100);
		Int_4 = GClass856.RandomInclude(AimingSettings.BAD_SHOOTS_MIN, AimingSettings.BAD_SHOOTS_MAX);
		Float_13 = AimingSettings.START_TIME_COEF;
	}

	public void GetHit(DamageInfoStruct damageInfo)
	{
		if (GClass856.Random(0f, 100f) < AimingSettings.DAMAGE_TO_DISCARD_AIM_0_100)
		{
			BotHitAffectClass.DoAffection();
		}
		else
		{
			Float_7 += GClass856.Random(AimingSettings.MIN_TIME_DISCARD_AIM_SEC, AimingSettings.MAX_TIME_DISCARD_AIM_SEC);
		}
	}

	public void DrawGizmosSelected()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawSphere(RealTargetPoint, GetCurRadius());
		Gizmos.color = Color.green;
		Gizmos.DrawLine(BotOwner_0.MyHead.position, EndTargetPoint);
	}

	public void ManualUpdate()
	{
	}

	public void RotateX(float angToRotate)
	{
	}

	public void RotateY(float deltaAngle)
	{
	}

	public void SetWeapon(Weapon weapon)
	{
	}

	public void SetTracers(bool isTracers)
	{
	}

	public void LoseTarget()
	{
		Status = AimStatus.NoTarget;
		HardAim = false;
		BotOwner_0.WeaponManager?.ShootController?.SetAim(HardAim);
	}

	public void SetTarget(Vector3 trg)
	{
		if ((RealTargetPoint - trg).sqrMagnitude > 0f)
		{
			action_0?.Invoke(trg);
		}
		switch (AimStatus_0)
		{
		case AimStatus.NoTarget:
			method_5(trg);
			break;
		case AimStatus.Aiming:
		case AimStatus.AimComplete:
			method_4(trg);
			break;
		}
	}

	public void SetNextAimingDelay(float nextAimingDelay)
	{
		if (Float_10 < nextAimingDelay)
		{
			Float_10 = nextAimingDelay;
		}
	}

	public void ShootDone(Weapon weapon)
	{
		float magnitude = (EndTargetPoint - BifacialTransform_0.position).magnitude;
		Debug.DrawRay(BifacialTransform_0.position, GClass855.NormalizeFastSelf(BotOwner_0.LookDirection) * magnitude, Color.green, 2f);
		Debug.DrawLine(BifacialTransform_0.position, EndTargetPoint, Color.red, 2f);
		_ = EndTargetPoint - RealTargetPoint;
	}

	public void NodeUpdate()
	{
		Float_5 += Time.deltaTime;
		method_2();
		if (Float_5 > Float_7)
		{
			float value = (AimingSettings.MAX_AIM_PRECICING - Float_5 * BotOwner_0.Settings.Current.CurrentPrecicingSpeed) / AimingSettings.MAX_AIM_PRECICING;
			Float_13 = Mathf.Clamp(value, AimingSettings.MAX_AIMING_UPGRADE_BY_TIME, 1f);
			Status = AimStatus.AimComplete;
			Vector3 dir = BotHitAffectClass.Affect(Vector3_0);
			method_11(dir);
		}
		else
		{
			Status = AimStatus.Aiming;
			float num = Float_5 / Float_7;
			float t = num * num;
			Vector3 dir2 = Vector3.Lerp(Vector3_6, Vector3_0, t);
			Vector3 dir3 = BotHitAffectClass.Affect(dir2);
			method_11(dir3);
		}
	}

	public void Panic()
	{
		Float_12 = Time.time + AimingSettings.PANIC_TIME;
	}

	public void OnDrawGizmos()
	{
		Gizmos.color = Color.yellow;
		Gizmos.DrawRay(Vector3_3, Vector3_2);
	}

	public void DebugDraw()
	{
		Debug.DrawLine(BifacialTransform_0.position, EndTargetPoint, Color.yellow);
		Debug.DrawLine(BifacialTransform_0.position, RealTargetPoint, Color.red);
	}

	public void Move(float speed = 0f)
	{
		float a = speed * Time.deltaTime;
		Float_8 = Mathf.Lerp(a, Float_8, 0.1f);
		Bool_1 = Float_8 > AimingSettings.BOT_MOVE_IF_DELTA;
	}

	public void NextShotMiss(int missCount)
	{
		Int_0 = missCount;
	}

	public void TriggerPressedDone()
	{
		Int_1++;
		if (Int_1 > Int_3)
		{
			Int_3 = GClass856.RandomInclude(AimingSettings.RECALC_MUST_TIME_MIN, AimingSettings.RECALC_MUST_TIME_MAX);
			Bool_2 = true;
		}
	}

	public float GetCurRadius()
	{
		return Float_13 * LastSpreadCount;
	}

	public override string ToString()
	{
		return (Float_5 / Float_7).ToString("0.00") + " %  " + AimStatus_0;
	}

	public bool method_0()
	{
		if (BotOwner_0.Settings.FileSettings.Aiming.HARD_AIM_CHANCE_100 > 100)
		{
			Bool_3 = true;
			return Bool_3;
		}
		if (Time.time > Float_11)
		{
			Bool_3 = GClass856.IsTrue100(BotOwner_0.Settings.FileSettings.Aiming.HARD_AIM_CHANCE_100);
			Float_11 = Time.time + 10f;
		}
		return Bool_3;
	}

	public bool method_1()
	{
		if (BotOwner_0.Brain != null && BotOwner_0.Brain.LastDecision.HasValue)
		{
			if (BotOwner_0.Brain.LastDecision != BotLogicDecision.shootFromPlace && BotOwner_0.Brain.LastDecision != BotLogicDecision.dogFight)
			{
				return BotOwner_0.Brain.LastDecision == BotLogicDecision.lay;
			}
			return true;
		}
		return false;
	}

	public void method_2()
	{
		Vector3 vector = Vector3_1 - BifacialTransform_0.position;
		if (vector.sqrMagnitude > 2.25f)
		{
			Vector3_3 = BifacialTransform_0.position;
			Vector3_0 = vector;
			return;
		}
		float num = method_3(vector);
		float num2 = 1.5f;
		if (BotOwner_0.GetPlayer.PoseLevel < 0.9f)
		{
			num2 = 0.9f;
		}
		Vector3 lookDirection = BotOwner_0.LookDirection;
		lookDirection.y = 0f;
		lookDirection = GClass855.NormalizeFastSelf(lookDirection);
		Vector3 vector2 = new Vector3(BotOwner_0.Position.x, BotOwner_0.Position.y + num2, BotOwner_0.Position.z);
		if (num > 0.25f)
		{
			Vector3 vector3 = GClass855.Rotate90(lookDirection, GClass855.SideTurn.right);
			vector3 *= 0.4f;
			vector2 += vector3;
		}
		Vector3 vector3_ = Vector3_1 - vector2;
		Vector3_3 = vector2;
		Vector3_0 = vector3_;
	}

	public float method_3(Vector3 v)
	{
		return v.x * v.x + v.z * v.z;
	}

	public void method_4(Vector3 target)
	{
		if (Bool_2)
		{
			method_5(target);
			return;
		}
		Vector3 vector = target - RealTargetPoint;
		bool num = Mathf.Abs(vector.y) > AimingSettings.RECLC_Y_DIST;
		vector.y = 0f;
		if (num | (vector.sqrMagnitude > AimingSettings.RECALC_SQR_DIST))
		{
			method_5(target);
		}
		else
		{
			method_12(target);
		}
		method_13();
	}

	public void method_5(Vector3 v)
	{
		Bool_2 = false;
		BotOwner_0.Mover.Sprint(val: false);
		Int_1 = 0;
		Status = AimStatus.Aiming;
		Vector3_6 = BotOwner_0.LookDirection;
		RealTargetPoint = v;
		method_6(withTimeRecalc: true);
		Float_5 = 0f;
	}

	public void method_6(bool withTimeRecalc)
	{
		Vector3 to = RealTargetPoint - BifacialTransform_0.position;
		float num = Vector3.Angle(Vector3_6, to);
		LastDist2Target = to.magnitude;
		Float_9 = Time.time;
		Vector3_4 = method_9(LastDist2Target, num);
		if (withTimeRecalc)
		{
			Float_7 = method_7(LastDist2Target, num);
		}
	}

	public float method_7(float dist, float ang)
	{
		float num = 1f;
		if (Bool_1)
		{
			num *= AimingSettings.TIME_COEF_IF_MOVE;
		}
		float num2 = (Boolean_0 ? AimingSettings.PANIC_COEF : 1f);
		float num3 = BotDifficultySettingsClass.Curv.AimAngCoef.Evaluate(ang);
		float num4 = BotDifficultySettingsClass.Curv.AimTime2Dist.Evaluate(dist);
		float num5 = 1f;
		if (BotOwner_0.Memory.IsInCover)
		{
			num5 = AimingSettings.COEF_FROM_COVER;
		}
		float num6 = num5 * AimingSettings.BOTTOM_COEF;
		float num7 = num3 * num4 * BotDifficultySettingsClass.Current.CurrentAccuratySpeed * num2;
		float num8 = (num6 + num7 + Float_10) * num;
		float mAX_AIM_TIME = AimingSettings.MAX_AIM_TIME;
		if (num8 > mAX_AIM_TIME)
		{
			num8 = mAX_AIM_TIME;
		}
		Float_10 = 0f;
		LastAimTime = num8;
		return num8;
	}

	public float method_8(float dist)
	{
		float f = BotOwner_0.WeaponManager.WeaponAIPreset.BaseShift + dist;
		float p = (BotOwner_0.WeaponManager.IsCloseWeapon ? AimingSettings.SCATTERING_DIST_MODIF_CLOSE : AimingSettings.SCATTERING_DIST_MODIF);
		float num = Mathf.Pow(f, p);
		float num2 = (BotOwner_0.WeaponManager.IsCloseWeapon ? BotDifficultySettingsClass.Current.CurrentScatteringClose : BotDifficultySettingsClass.Current.CurrentScattering);
		return num * num2;
	}

	public Vector3 method_9(float dist, float angCoef, float additionCoef = 1f)
	{
		if (AimingSettings.DIST_TO_SHOOT_NO_OFFSET > dist)
		{
			return Vector3.zero;
		}
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		bool flag = false;
		if (goalEnemy != null)
		{
			int shootByTarget = BotOwner_0.BotPersonalStats.GetShootByTarget(goalEnemy);
			int num = method_10(dist);
			if (shootByTarget < num)
			{
				flag = true;
			}
		}
		LastSpreadCount = method_8(dist) * additionCoef;
		if (Boolean_0)
		{
			LastSpreadCount *= AimingSettings.PANIC_ACCURATY_COEF;
		}
		if (HardAim)
		{
			LastSpreadCount *= AimingSettings.HARD_AIM;
		}
		if (BotOwner_0.BotLay.IsLay)
		{
			LastSpreadCount *= BotOwner_0.Settings.FileSettings.Lay.LAY_AIM;
		}
		if (Bool_1)
		{
			LastSpreadCount *= AimingSettings.COEF_IF_MOVE;
		}
		float num2 = 0f;
		float y = 0f;
		float num3 = 0f;
		float xZ_COEF = BotOwner_0.WeaponManager.WeaponAIPreset.XZ_COEF;
		float num4 = Mathf.Clamp(angCoef, 0f, 60f);
		float num5 = 2f * dist * Mathf.Sin(MathF.PI / 180f * num4 / 2f) * xZ_COEF + LastSpreadCount;
		float num6 = LastSpreadCount * AimingSettings.Y_TOP_OFFSET_COEF;
		float num7 = (0f - LastSpreadCount) * AimingSettings.Y_BOTTOM_OFFSET_COEF;
		switch (BotOwner_0.Settings.FileSettings.Core.AimingType)
		{
		case AimingType.regular:
			num2 = GClass856.Random(0f - num5, num5);
			y = GClass856.Random(num7, num6);
			num3 = GClass856.Random(0f - num5, num5);
			break;
		case AimingType.normal:
			num2 = GClass855.RandomNormal(0f - num5, num5);
			y = GClass855.RandomNormal(num7, num6);
			num3 = GClass855.RandomNormal(0f - num5, num5);
			break;
		}
		if (Int_0 > 0)
		{
			Int_0--;
			y = AimingSettings.NEXT_SHOT_MISS_Y_OFFSET * 2f;
		}
		if (flag)
		{
			float x = ((num2 > 0f) ? AimingSettings.BAD_SHOOTS_OFFSET : (0f - AimingSettings.BAD_SHOOTS_OFFSET));
			float z = ((num3 > 0f) ? AimingSettings.BAD_SHOOTS_OFFSET : (0f - AimingSettings.BAD_SHOOTS_OFFSET));
			Vector3_5 = new Vector3(x, 0f, z);
		}
		else
		{
			Vector3_5 = Vector3.zero;
		}
		return new Vector3(num2, y, num3);
	}

	public int method_10(float dist)
	{
		return Int_4 + (int)(AimingSettings.BAD_SHOOTS_MAIN_COEF * Mathf.Log(1.2f + dist * 0.2f));
	}

	public void method_11(Vector3 dir)
	{
		Vector3_2 = dir;
		BotOwner_0.Steering.LookToDirection(dir, 500f);
		BotOwner_0.Steering.SetYByDir(Vector3_0);
	}

	public void method_12(Vector3 v)
	{
		if (Time.time - Float_9 > AimingSettings.OFFSET_RECAL_ANYWAY_TIME)
		{
			method_6(withTimeRecalc: false);
		}
		RealTargetPoint = v;
	}

	public void method_13()
	{
		if (DebugBotData.UseDebugData && DebugBotData.Instance.TrueAim)
		{
			EndTargetPoint = RealTargetPoint;
		}
		else
		{
			EndTargetPoint = RealTargetPoint + Vector3_5 + Float_13 * (Vector3_4 + BotOwner_0.RecoilData.RecoilOffset);
		}
	}

	public void Dispose()
	{
	}
}
