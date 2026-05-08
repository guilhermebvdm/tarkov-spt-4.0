using System;
using EFT;
using UnityEngine;

public class GClass492 : GClass429
{
	[NonSerialized]
	public Vector3 Vector3_0;

	[NonSerialized]
	public Vector3 Vector3_1;

	[NonSerialized]
	public bool Bool_0;

	[NonSerialized]
	public float Float_0;

	[NonSerialized]
	public const float Float_1 = 0.9f;

	[NonSerialized]
	public const float Float_2 = 1.2f;

	[NonSerialized]
	public const float Float_3 = 0.3f;

	[NonSerialized]
	public const float Float_4 = 2.1f;

	[NonSerialized]
	public const float Float_5 = 2.52f;

	[NonSerialized]
	public bool Bool_1;

	public Vector3 TotalOffset => Vector3_0;

	public Vector3 NotLinkedOffset => Vector3_1;

	public GClass492(BotOwner owner)
		: base(owner)
	{
		Vector3_0 = Vector3.zero;
	}

	public void ManualUpdate()
	{
		if (BotOwner_0.Mover.CurrentState == EBotMoverState.NearDoor)
		{
			return;
		}
		NavGraphVoxelSimple curVoxel = BotOwner_0.VoxelesPersonalData.CurVoxel;
		if (!BotOwner_0.HealthController.IsAlive)
		{
			return;
		}
		method_0();
		foreach (BotOwner item in curVoxel.BotsInside)
		{
			if (item.Id == BotOwner_0.Id || item.Mover.CurrentState == EBotMoverState.NearDoor || !item.HealthController.IsAlive)
			{
				continue;
			}
			Vector3 v = item.Mover.PositionOnWay - BotOwner_0.Mover.PositionOnWay;
			if (!(Mathf.Abs(v.y) < 0.5f))
			{
				continue;
			}
			v.y = 0f;
			float magnitude = v.magnitude;
			if (!(magnitude < 2.52f))
			{
				continue;
			}
			Float_0 = Time.time + 0.15f;
			if ((item.Position - BotOwner_0.Position).magnitude < 2.1f && magnitude < 2.1f && v.sqrMagnitude > 0f)
			{
				Vector3 vector = GClass855.NormalizeFastSelf(v);
				if (GClass855.IsAngLessNormalized(BotOwner_0.Mover.NormDirCurPoint, vector, 0.9848077f))
				{
					vector = GClass855.Rotate90(vector, GClass855.SideTurn.right);
				}
				if (item.Mover.HasPathAndNoComplete && BotOwner_0.Mover.HasPathAndNoComplete)
				{
					item.Mover.LocalAvoidance.method_1(vector);
					BotOwner_0.Mover.LocalAvoidance.method_1(-vector);
				}
			}
		}
	}

	public void method_0()
	{
		if (Bool_0 && !(Float_0 > Time.time))
		{
			float magnitude = Vector3_0.magnitude;
			Bool_0 = magnitude > 0.0001f;
			if (Bool_0)
			{
				float num = Mathf.Clamp(Time.deltaTime, 0f, 0.05f);
				Vector3_1 = GClass855.NormalizeFastSelf(Vector3_0) * Mathf.Clamp(magnitude - num * 0.9f, 0f, magnitude);
			}
		}
	}

	public void method_1(Vector3 dir)
	{
		Bool_0 = true;
		float num = Mathf.Clamp(Time.deltaTime, 0f, 0.05f);
		Vector3_1 = Vector3_0 + dir * num * 1.2f;
		float magnitude = Vector3_1.magnitude;
		if (magnitude > 1E-05f)
		{
			Vector3_1 = GClass855.NormalizeFastSelf(Vector3_1) * Mathf.Clamp(magnitude, 0f, 0.3f);
		}
	}

	public void UpdateTotal(float coef)
	{
		Vector3_0 = Vector3_1 * coef;
	}

	public void DropOffset()
	{
		Vector3_0 = Vector3.zero;
		Vector3_1 = Vector3.zero;
	}
}
