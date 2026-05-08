using System;
using UnityEngine;

[Serializable]
public class WeaponRecoilProcessBase
{
	public ComponentType ComponentType;

	public float CurveTimeMultiply = 1f;

	public float CurveValueMultiply = 1f;

	public float CurveAimingValueMultiply = 1f;

	public AnimationCurve TransformationCurve;

	[NonSerialized]
	public Vector3 Current;

	[NonSerialized]
	public float CurveTime;

	public WeaponRecoilProcessBase()
	{
	}

	public WeaponRecoilProcessBase(ComponentType ComponentType, float CurveTimeMultiply, float CurveValueMultiply, float CurveAimingValueMultiply, AnimationCurve TransformationCurve)
	{
		this.ComponentType = ComponentType;
		this.CurveTimeMultiply = CurveTimeMultiply;
		this.CurveValueMultiply = CurveValueMultiply;
		this.CurveAimingValueMultiply = CurveAimingValueMultiply;
		this.TransformationCurve = TransformationCurve;
	}

	public void CalculateRecoil(float deltaTime, bool isAiming)
	{
		float num = (isAiming ? CurveAimingValueMultiply : CurveValueMultiply);
		Current = num * TransformationCurve.Evaluate(CurveTime) * method_0();
		CurveTime += deltaTime * CurveTimeMultiply;
	}

	public void OnShoot()
	{
		CurveTime = 0f;
		Current = Vector3.zero;
	}

	public Vector3 GetRecoil()
	{
		return Current;
	}

	public Vector3 method_0()
	{
		return ComponentType switch
		{
			ComponentType.Y => Vector3.up, 
			ComponentType.X => Vector3.right, 
			_ => Vector3.forward, 
		};
	}
}
