using System;
using EFT;
using UnityEngine;

public class GClass497 : GClass429
{
	public const float MAX_INERTION = 0.3f;

	public const float CLAMP_ANG = 85f;

	[NonSerialized]
	public Vector3 Vector3_0 = Vector3.zero;

	[NonSerialized]
	public PathControllerClass PathControllerClass;

	[NonSerialized]
	public float Float_0;

	[NonSerialized]
	public Vector3 Vector3_1 = Vector3.zero;

	public float InertiaonDownSpeed => BotOwner_0.InertiaonDownSpeedTEMP;

	public float _collectedAngDownSpeed => BotOwner_0.CollectedAngDownSpeedTEMP;

	public GClass497(BotOwner owner)
		: base(owner)
	{
	}

	public void Activate(PathControllerClass pathController)
	{
		if (PathControllerClass != null)
		{
			PathControllerClass.OnWayChanged -= method_0;
		}
		PathControllerClass = pathController;
		PathControllerClass.OnWayChanged += method_0;
	}

	public void DropOffset()
	{
		Vector3_0 = Vector3.zero;
		Vector3_1 = Vector3.zero;
	}

	public Vector3 AddToInertion(Vector3 realDelta, Vector3 curPos)
	{
		float num = Mathf.Abs(Float_0);
		bool isLeft = Vector3.Dot(GClass856.Rotate90(Vector3_0, 1), realDelta) > 0f;
		float num2 = Vector3.Angle(to: new Vector3(realDelta.x, 0f, realDelta.z), from: new Vector3(Vector3_0.x, 0f, Vector3_0.z));
		if (Mathf.Abs(num2) > 0f)
		{
			method_2(num2, isLeft);
			num = Mathf.Abs(Float_0);
		}
		Vector3 vector = realDelta;
		Vector3 vector3_ = Vector3_1;
		_ = vector3_.magnitude;
		if (num > 0f)
		{
			float num3 = Mathf.Cos(Mathf.Clamp(num, 1f, 85f) * (MathF.PI / 180f));
			vector = realDelta * num3;
			GClass855.SideTurn side = ((!(Float_0 > 0f)) ? GClass855.SideTurn.right : GClass855.SideTurn.left);
			Vector3 vector2 = GClass855.Rotate90(realDelta, side) * (1f - num3);
			method_1();
			vector3_ = Vector3_1 + vector2;
		}
		realDelta = vector;
		Vector3_1 = vector3_;
		Vector3 result = curPos + realDelta;
		Vector3_0 = realDelta;
		float num4 = 0f;
		if (Vector3_1.magnitude > 0f)
		{
			Vector3 vector3_2 = Vector3_1;
			vector3_2 = GClass855.NormalizeFastSelf(vector3_2);
			num4 = InertiaonDownSpeed * Time.deltaTime;
			num4 = Mathf.Clamp(num4, 0f, Vector3_1.magnitude);
			Vector3_1 -= vector3_2 * num4;
		}
		if (Vector3_1.magnitude > 0.3f)
		{
			Vector3 vector3 = GClass855.NormalizeFastSelf(Vector3_1);
			Vector3_1 = vector3 * 0.3f;
		}
		Vector3_0 = realDelta;
		return result;
	}

	public Vector3 GetOffset()
	{
		return Vector3_1;
	}

	public void method_0(bool havePrevWay)
	{
		DropOffset();
	}

	public void method_1()
	{
		float num = _collectedAngDownSpeed * Time.deltaTime;
		if (Float_0 > 0f)
		{
			Float_0 -= num;
			if (Float_0 < 0f)
			{
				Float_0 = 0f;
			}
		}
		else if (Float_0 < 0f)
		{
			Float_0 += num;
			if (Float_0 > 0f)
			{
				Float_0 = 0f;
			}
		}
	}

	public void method_2(float ang, bool isLeft)
	{
		if (isLeft)
		{
			Float_0 -= ang;
		}
		else
		{
			Float_0 += ang;
		}
		Float_0 = Mathf.Clamp(Float_0, -85f, 85f);
	}

	public void Dispose()
	{
		PathControllerClass.OnWayChanged -= method_0;
	}

	public void UpdateOnCoef(float coef)
	{
		Vector3_0 *= coef;
	}
}
