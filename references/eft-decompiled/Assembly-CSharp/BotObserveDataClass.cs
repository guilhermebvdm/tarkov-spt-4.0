using System;
using System.Runtime.CompilerServices;
using EFT;
using UnityEngine;

public class BotObserveDataClass : GClass429
{
	[NonSerialized]
	public const float Float_0 = 33f;

	[NonSerialized]
	public const float Float_1 = 15f;

	[NonSerialized]
	public const float Float_2 = 4f;

	[NonSerialized]
	public const float Float_3 = 15f;

	public Vector3 CoreDirection;

	[NonSerialized]
	public bool Bool_0;

	[NonSerialized]
	public Vector3 Vector3_0;

	[NonSerialized]
	public Vector3 Vector3_1;

	[NonSerialized]
	public float Float_4;

	[NonSerialized]
	public float Float_5 = 1f;

	[NonSerialized]
	public float Float_6 = 1f;

	[NonSerialized]
	public float Float_7;

	[NonSerialized]
	public GClass503 Gclass503_0;

	[NonSerialized]
	[CompilerGenerated]
	public bool Bool_1;

	public bool IsActive
	{
		[CompilerGenerated]
		get
		{
			return Bool_1;
		}
		[CompilerGenerated]
		set
		{
			Bool_1 = value;
		}
	}

	public BotObserveDataClass(BotOwner owner)
		: base(owner)
	{
		Gclass503_0 = new GClass503(owner);
	}

	public void Reset()
	{
		Float_4 = 0f;
	}

	public void Stop()
	{
		IsActive = false;
	}

	public void SetVector(bool noTimeCheck = false)
	{
		if (noTimeCheck || Float_7 < Time.time)
		{
			Float_7 = Time.time + GClass856.Random(0.8f, 1.5f);
			int num = UnityEngine.Random.Range(-15, 25);
			Vector3 direction = GClass855.RotateOnAngUp(BotOwner_0.Mover.NormDirCurPoint, num);
			SetVector(direction);
		}
	}

	public void SetVectorToLook(Vector3 dir)
	{
		if (Float_7 < Time.time)
		{
			Float_7 = Time.time + 2f;
			float angDegree = UnityEngine.Random.Range(-33f, 33f);
			Vector3 direction = GClass855.RotateOnAngUp(GClass855.NormalizeFastSelf(dir), angDegree);
			SetVector(direction);
		}
	}

	public void SetVector(Vector3 direction, float speed = 15f)
	{
		Vector3 lookDirection = BotOwner_0.Steering.LookDirection;
		lookDirection.y = 0f;
		if (lookDirection.magnitude > 0.2f)
		{
			CoreDirection = lookDirection;
		}
		Float_6 = Vector3.Angle(direction, BotOwner_0.LookDirection);
		Vector3_1 = BotOwner_0.Steering.LookDirection;
		float num = Vector3.Dot(CoreDirection, direction);
		Float_5 = Float_6 / speed;
		if (num > 0f)
		{
			Float_6 = 0f - Float_6;
		}
		if ((GClass855.RotateOnAngUp(CoreDirection, Float_6).normalized - direction).sqrMagnitude > 0.5f)
		{
			Float_6 = 0f - Float_6;
		}
		Float_4 = 0f;
		IsActive = true;
	}

	public void Update()
	{
		if (IsActive)
		{
			Gclass503_0.ManualUpdate();
			float verAng = Gclass503_0.GetVerAng();
			Quaternion quaternion = Quaternion.Euler(verAng, 0f, verAng);
			if (BotOwner_0.HasPathAndNotComplete)
			{
				BotOwner_0.Steering.LookToMovingDirection();
				Vector3 dirCurPoint = BotOwner_0.Mover.DirCurPoint;
				dirCurPoint = GClass856.RotateAroundPivot(dirCurPoint, new Vector3(dirCurPoint.y, 0f, dirCurPoint.z), quaternion);
				BotOwner_0.Steering.LookToDirection(dirCurPoint);
			}
			else
			{
				float t = Mathf.Clamp01(Float_4);
				float angDegree = Mathf.Lerp(0f, Float_6, t);
				Vector3 dirCurPoint = (quaternion * GClass855.RotateOnAngUp(CoreDirection, angDegree)).normalized;
				dirCurPoint = GClass856.RotateAroundPivot(dirCurPoint, new Vector3(Vector3_0.y, 0f, Vector3_0.z), quaternion);
				Vector3_0 = Vector3.Lerp(Vector3_1, dirCurPoint, t);
				BotOwner_0.Steering.LookToDirection(Vector3_0);
			}
			Float_4 += Time.deltaTime;
			if (Float_4 > Float_5)
			{
				IsActive = false;
				Float_4 = 0f;
			}
		}
	}
}
