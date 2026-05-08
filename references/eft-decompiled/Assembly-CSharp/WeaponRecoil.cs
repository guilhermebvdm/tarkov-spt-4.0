using System;
using UnityEngine;

[Serializable]
public class WeaponRecoil
{
	public bool Enable;

	public WeaponRecoilValue[] Values;

	[NonSerialized]
	public bool IsAiming;

	public WeaponRecoil()
	{
	}

	public WeaponRecoil(bool enable)
	{
		Enable = enable;
	}

	public void Initialize(WeaponRecoilValue[] values)
	{
		Values = new WeaponRecoilValue[values.Length];
		for (int i = 0; i < values.Length; i++)
		{
			Values[i] = new WeaponRecoilValue();
			Values[i].Enable = values[i].Enable;
			Values[i].Target = values[i].Target;
			Values[i].Process = new WeaponRecoilProcessBase(values[i].Process.ComponentType, values[i].Process.CurveTimeMultiply, values[i].Process.CurveValueMultiply, values[i].Process.CurveAimingValueMultiply, values[i].Process.TransformationCurve);
		}
	}

	public void OnShoot()
	{
		WeaponRecoilValue[] values = Values;
		foreach (WeaponRecoilValue weaponRecoilValue in values)
		{
			if (weaponRecoilValue.Enable)
			{
				weaponRecoilValue.Process.OnShoot();
			}
		}
	}

	public void SetAiming(bool value)
	{
		IsAiming = value;
	}

	public void CalculateRecoil(float deltaTime)
	{
		if (!Enable)
		{
			return;
		}
		WeaponRecoilValue[] values = Values;
		foreach (WeaponRecoilValue weaponRecoilValue in values)
		{
			if (weaponRecoilValue.Enable)
			{
				weaponRecoilValue.Process.CalculateRecoil(deltaTime, IsAiming);
			}
		}
	}

	public Vector3 GetPositionRecoil()
	{
		Vector3 zero = Vector3.zero;
		WeaponRecoilValue[] values = Values;
		foreach (WeaponRecoilValue weaponRecoilValue in values)
		{
			if (weaponRecoilValue.Target == Target.HandsPosition && weaponRecoilValue.Enable)
			{
				zero += weaponRecoilValue.Process.GetRecoil();
			}
		}
		return zero;
	}

	public Vector3 GetRotationRecoil()
	{
		Vector3 zero = Vector3.zero;
		WeaponRecoilValue[] values = Values;
		foreach (WeaponRecoilValue weaponRecoilValue in values)
		{
			if (weaponRecoilValue.Target == Target.HandsRotation && weaponRecoilValue.Enable)
			{
				zero += weaponRecoilValue.Process.GetRecoil();
			}
		}
		return zero;
	}

	public Vector3 GetCameraRotationRecoil()
	{
		Vector3 zero = Vector3.zero;
		WeaponRecoilValue[] values = Values;
		foreach (WeaponRecoilValue weaponRecoilValue in values)
		{
			if (weaponRecoilValue.Target == Target.CameraRotation && weaponRecoilValue.Enable)
			{
				zero += weaponRecoilValue.Process.GetRecoil();
			}
		}
		return zero;
	}
}
