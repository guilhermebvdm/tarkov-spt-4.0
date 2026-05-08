using System;
using EFT;
using UnityEngine;

public class GClass188 : GClass178
{
	[NonSerialized]
	public float Float_1;

	[NonSerialized]
	public Vector3 Vector3_1;

	[NonSerialized]
	public Vector3 Vector3_2;

	[NonSerialized]
	public Vector3 Vector3_3;

	[NonSerialized]
	public float Float_2;

	[NonSerialized]
	public float Float_3;

	[NonSerialized]
	public float Float_4 = 0.6f;

	[NonSerialized]
	public int Int_0 = 1;

	[NonSerialized]
	public int Int_1 = 1;

	public static void DrawCross(Vector3 v, Color c)
	{
	}

	public GClass188(BotOwner bot)
		: base(bot)
	{
	}

	public override void UpdateNodeByBrain(GClass27 data)
	{
		BotOwner_0.Mover.Stop();
		if (Time.time > Float_2)
		{
			method_2();
			Float_1 = 0f;
		}
		else
		{
			Float_1 = 1f - (Float_2 - Time.time) / Float_3;
		}
		Float_1 = Mathf.Clamp01(Float_1);
		Vector3_3 = Vector3.Lerp(Vector3_1, Vector3_2, Float_1);
		DrawCross(Vector3_3, new Color(0.9f, 0.2f, 0f));
		DrawCross(Vector3_1, new Color(0.1f, 0.9f, 0f));
		DrawCross(Vector3_2, new Color(0.1f, 0.2f, 0.9f));
		base.UpdateNodeByBrain(data);
	}

	public override Vector3? GetTarget()
	{
		return Vector3_3;
	}

	public void method_2()
	{
		if (Vector3_2 == Vector3.zero)
		{
			Vector3 vector = method_3();
			Vector3_1 = BotOwner_0.Position + vector;
		}
		else
		{
			Vector3_1 = Vector3_2;
		}
		Vector3 vector2 = method_3();
		Vector3_2 = BotOwner_0.Position + vector2;
		Float_3 = GClass856.Random(0.3f, 0.65f);
		Float_2 = Time.time + Float_3;
	}

	public Vector3 method_3()
	{
		Int_0 *= ((GClass856.RandomInclude(0, 100) >= 70) ? 1 : (-1));
		Int_1 *= ((GClass856.RandomInclude(0, 100) >= 70) ? 1 : (-1));
		float x = (float)Int_0 * GClass856.Random(0f, Float_4);
		float z = (float)Int_1 * GClass856.Random(0f, Float_4);
		Vector3 result = BotOwner_0.LookDirection.normalized * 1.6f + new Vector3(x, 0f, z);
		result.y = GClass856.Random(0.9f, 2.1f);
		return result;
	}
}
