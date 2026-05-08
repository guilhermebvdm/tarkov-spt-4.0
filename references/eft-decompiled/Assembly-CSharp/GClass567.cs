using System;
using EFT;
using UnityEngine;

public class GClass567(BotOwner owner, int index) : GClass565(owner, index)
{
	[NonSerialized]
	public const float Float_4 = 50f;

	[NonSerialized]
	public const float Float_5 = 5f;

	[NonSerialized]
	public const float Float_6 = 33f;

	[NonSerialized]
	public const float Float_7 = 3f;

	[NonSerialized]
	public float Float_8 = 10f;

	[NonSerialized]
	public float Float_9 = 400f;

	[NonSerialized]
	public float Float_10 = 900f;

	[NonSerialized]
	public float Float_11;

	[NonSerialized]
	public float Float_12;

	[NonSerialized]
	public bool Bool_6;

	[NonSerialized]
	public bool Bool_7;

	public override void Activate()
	{
		Float_8 = BotOwner_0.Settings.FileSettings.Boss.DELTA_DIST_DEST_BOSS_START_RUN_FOR_COVER_WITH_STOP;
		float dIST_TO_START_RUN_FOR_COVER_WITH_STOP = BotOwner_0.Settings.FileSettings.Boss.DIST_TO_START_RUN_FOR_COVER_WITH_STOP;
		Float_10 = dIST_TO_START_RUN_FOR_COVER_WITH_STOP * dIST_TO_START_RUN_FOR_COVER_WITH_STOP;
		float num = dIST_TO_START_RUN_FOR_COVER_WITH_STOP * 0.6f;
		Float_9 = num * num;
		base.Activate();
	}

	public override void Update()
	{
		method_6();
		BotOwner_0.Sprint(Bool_7);
		if (Float_11 < Time.time)
		{
			bool num = Bool_6 || GClass856.IsTrue100(50f);
			float num2 = (Bool_6 ? GClass856.GreateRandom(33f) : GClass856.GreateRandom(3f));
			Float_11 = Time.time + num2;
			if (num)
			{
				Bool_6 = !Bool_6;
				if (Bool_6)
				{
					method_4();
				}
				else
				{
					Float_0 = 0f;
				}
			}
		}
		if (Bool_6)
		{
			BotOwner_0.StopMove();
			method_5();
		}
		else
		{
			base.Update();
		}
	}

	public void method_4()
	{
		float x = GClass856.Random(-1f, 1f);
		float z = GClass856.Random(-1f, 1f);
		Vector3 direction = GClass369.Test4Sides(new Vector3(x, 0f, z), BotOwner_0.GetPlayer.PlayerBones.Head.position);
		BotObserveDataClass.SetVector(direction);
	}

	public void method_5()
	{
		BotObserveDataClass.Update();
	}

	public void method_6()
	{
		if (!(Float_12 < Time.time))
		{
			return;
		}
		Float_12 = Time.time + 2f;
		float distDestination = BotOwner_0.Mover.DistDestination;
		if (distDestination < 5f)
		{
			Bool_7 = false;
		}
		else if (BotOwner_0.BotFollower.HaveBoss)
		{
			float sqrMagnitude = (BotOwner_0.BotFollower.BossToFollow.Position - BotOwner_0.Position).sqrMagnitude;
			if (Bool_7)
			{
				if (sqrMagnitude < Float_9)
				{
					Bool_7 = false;
				}
			}
			else if (sqrMagnitude > Float_10)
			{
				float distDestination2 = BotOwner_0.BotFollower.BossToFollow.Player().AIData.BotOwner.Mover.DistDestination;
				if (distDestination - distDestination2 > Float_8)
				{
					Bool_7 = true;
				}
			}
		}
		else
		{
			Bool_7 = false;
		}
	}
}
