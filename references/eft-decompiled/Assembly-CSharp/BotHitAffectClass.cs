using System;
using EFT;
using UnityEngine;

public class BotHitAffectClass
{
	[NonSerialized]
	public Vector3 Vector3_0 = Vector3.zero;

	[NonSerialized]
	public float Float_0;

	[NonSerialized]
	public float Float_1 = 1f;

	[NonSerialized]
	public bool Bool_0;

	[NonSerialized]
	public float Float_2;

	[NonSerialized]
	public BotOwner BotOwner_0;

	public BotHitAffectClass(BotOwner owner)
	{
		BotOwner_0 = owner;
	}

	public Vector3 Affect(Vector3 dir)
	{
		if (Bool_0)
		{
			method_0();
			return GClass856.RotateAroundPivot(dir, Vector3.zero, Vector3_0 * Float_0);
		}
		return dir;
	}

	public void DoAffection()
	{
		float x = (float)GClass856.RandomSing() * GClass856.Random(BotOwner_0.Settings.FileSettings.Aiming.BASE_HIT_AFFECTION_MIN_ANG, BotOwner_0.Settings.FileSettings.Aiming.BASE_HIT_AFFECTION_MAX_ANG);
		float y = (float)GClass856.RandomSing() * GClass856.Random(BotOwner_0.Settings.FileSettings.Aiming.BASE_HIT_AFFECTION_MIN_ANG, BotOwner_0.Settings.FileSettings.Aiming.BASE_HIT_AFFECTION_MAX_ANG);
		float z = (float)GClass856.RandomSing() * GClass856.Random(BotOwner_0.Settings.FileSettings.Aiming.BASE_HIT_AFFECTION_MIN_ANG, BotOwner_0.Settings.FileSettings.Aiming.BASE_HIT_AFFECTION_MAX_ANG);
		Vector3_0 = new Vector3(x, y, z);
		Bool_0 = true;
		Float_1 = BotOwner_0.Settings.FileSettings.Aiming.BASE_HIT_AFFECTION_DELAY_SEC;
		Float_2 = Time.time + Float_1 * GClass856.Random(0.8f, 1.2f);
	}

	public void method_0()
	{
		if (Bool_0)
		{
			float num = Float_2 - Time.time;
			if (num < 0f)
			{
				Bool_0 = false;
			}
			else
			{
				Float_0 = num / Float_1;
			}
		}
	}
}
