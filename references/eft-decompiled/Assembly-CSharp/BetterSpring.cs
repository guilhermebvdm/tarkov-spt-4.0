using System;
using UnityEngine;

[Serializable]
public class BetterSpring
{
	[NonSerialized]
	public Vector3 PPos;

	[NonSerialized]
	public Vector3 PVel;

	[SerializeField]
	public Vector3 _equilibriumPos = Vector3.zero;

	[SerializeField]
	public float _angularFrequency;

	[SerializeField]
	public float _dampingRatio = 0.5f;

	[NonSerialized]
	public const float Epsilon = 0.0001f;

	[NonSerialized]
	public float Za;

	[NonSerialized]
	public float Zb;

	[NonSerialized]
	public float Z1;

	[NonSerialized]
	public float Z2;

	[NonSerialized]
	public float OmegaZeta;

	[NonSerialized]
	public float Alpha;

	public Vector3 Value => PPos;

	public Vector3 EquilibriumPos
	{
		get
		{
			return _equilibriumPos;
		}
		set
		{
			_equilibriumPos = value;
		}
	}

	[ContextMenu("Runtime update")]
	public void Cache()
	{
		OmegaZeta = _angularFrequency * _dampingRatio;
		Alpha = _angularFrequency * Mathf.Sqrt(1f - _dampingRatio * _dampingRatio);
		Za = (0f - _angularFrequency) * _dampingRatio;
		Zb = _angularFrequency * Mathf.Sqrt(_dampingRatio * _dampingRatio - 1f);
		Z1 = Za - Zb;
		Z2 = Za + Zb;
	}

	public void Process(float deltaTime)
	{
		Vector3 vector = PPos - EquilibriumPos;
		Vector3 pVel = PVel;
		if (_dampingRatio > 1.0001f)
		{
			float num = Mathf.Exp(Z1 * deltaTime);
			float num2 = Mathf.Exp(Z2 * deltaTime);
			Vector3 vector2 = (pVel - vector * Z2) / (-2f * Zb);
			Vector3 vector3 = vector - vector2;
			PPos = EquilibriumPos + vector2 * num + vector3 * num2;
			PVel = vector2 * (Z1 * num) + vector3 * (Z2 * num2);
		}
		else if (_dampingRatio > 0.9999f)
		{
			float num3 = Mathf.Exp((0f - _angularFrequency) * deltaTime);
			Vector3 vector4 = pVel + _angularFrequency * vector;
			Vector3 vector5 = vector;
			Vector3 vector6 = (vector4 * deltaTime + vector5) * num3;
			PPos = EquilibriumPos + vector6;
			PVel = vector4 * num3 - vector6 * _angularFrequency;
		}
		else
		{
			float num4 = Mathf.Exp((0f - OmegaZeta) * deltaTime);
			float num5 = Mathf.Cos(Alpha * deltaTime);
			float num6 = Mathf.Sin(Alpha * deltaTime);
			Vector3 vector7 = vector;
			Vector3 vector8 = (pVel + OmegaZeta * vector) / Alpha;
			PPos = EquilibriumPos + num4 * (vector7 * num5 + vector8 * num6);
			PVel = (0f - num4) * ((vector7 * OmegaZeta - vector8 * Alpha) * num5 + (vector7 * Alpha + vector8 * OmegaZeta) * num6);
		}
	}

	public void ApplyVelocity(Vector3 val)
	{
		PVel += val;
	}

	public void ApplyVelocity(int comp, float val)
	{
		PVel[comp] += val;
	}

	public void ApplyPosition(Vector3 position)
	{
		PPos = position;
	}
}
