using System;
using EFT;
using UnityEngine;

public class GClass240 : GClass239
{
	public bool NoSprint;

	[NonSerialized]
	public float Float_14;

	[NonSerialized]
	public float Float_15;

	[NonSerialized]
	public bool Bool_3;

	[NonSerialized]
	public bool Bool_4 = true;

	[NonSerialized]
	public bool Bool_5;

	[NonSerialized]
	public bool Bool_6;

	[NonSerialized]
	public Transform Transform_0;

	[NonSerialized]
	public Transform Transform_1;

	[NonSerialized]
	public Transform Transform_2;

	[NonSerialized]
	public GameObject GameObject_0;

	[NonSerialized]
	public float Float_16;

	[NonSerialized]
	public float Float_17;

	public GClass240(BotOwner bot)
		: base(bot)
	{
		method_18();
	}

	public void method_18()
	{
		ShuttleMoveDebugObject[] array = GClass870.FindUnityObjectsOfType<ShuttleMoveDebugObject>();
		GameObject gameObject = null;
		GameObject gameObject2 = null;
		if (array.Length >= 2)
		{
			gameObject = array[0].gameObject;
			gameObject2 = array[1].gameObject;
		}
		else
		{
			gameObject = GameObject.Find("Shuttle1");
			gameObject2 = GameObject.Find("Shuttle2");
		}
		GameObject_0 = GameObject.Find("ShuttleLookAt");
		Bool_5 = GameObject_0 != null;
		if (gameObject != null && gameObject2 != null)
		{
			Transform_0 = gameObject.transform;
			Transform_1 = gameObject2.transform;
			Bool_6 = true;
		}
		else if (DebugBotData.UseDebugData && DebugBotData.Instance.DebugBrain)
		{
			BotOwner_0.Mover.Stop();
			Debug.LogError("SHUTTLE MOVE CAN'T BE EXECUTED!   pls create right objects (look at this cs file to understand)");
		}
		if (!(Transform_2 == null))
		{
			Transform_2 = Transform_0;
			BotOwner_0.GoToSomePointData.SetPoint(Transform_2.position);
		}
	}

	public override void UpdateNodeByBrain(GClass26 data)
	{
		if (!Bool_6)
		{
			if (!(Float_16 > Time.time))
			{
				method_18();
				Float_16 = Time.time + 2f;
			}
			return;
		}
		if ((BotOwner_0.GoToSomePointData.Point - BotOwner_0.Position).sqrMagnitude < 1f)
		{
			method_19();
		}
		if (!BotOwner_0.Mover.HasPathAndNoComplete || !BotOwner_0.GoToSomePointData.HaveTarget())
		{
			if (Float_17 > Time.time)
			{
				return;
			}
			Float_17 = Time.time + 2f;
			if (Transform_2 == null)
			{
				Transform_2 = Transform_0;
			}
			if (Transform_2 == null)
			{
				Debug.LogError("can't set taregt");
				return;
			}
			BotOwner_0.GoToSomePointData.SetPoint(Transform_2.position);
		}
		base.UpdateNodeByBrain(data);
	}

	public void method_19()
	{
		if (!(Float_16 > Time.time))
		{
			Float_16 = Time.time + 2f;
			if (Transform_2 == Transform_0)
			{
				Transform_2 = Transform_1;
			}
			else
			{
				Transform_2 = Transform_0;
			}
			if (Bool_4)
			{
				BotOwner_0.SetPose(0f);
			}
			BotOwner_0.StopMove();
			Bool_3 = false;
			BotOwner_0.GoToSomePointData.SetPoint(Transform_2.position);
		}
	}
}
