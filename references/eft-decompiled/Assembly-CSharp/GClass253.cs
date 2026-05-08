using System;
using EFT;
using UnityEngine;

public class GClass253 : GClass200<GClass26>
{
	public bool NoSprint;

	[NonSerialized]
	public float Float_0;

	[NonSerialized]
	public float Float_1;

	[NonSerialized]
	public bool Bool_0;

	[NonSerialized]
	public bool Bool_1 = true;

	[NonSerialized]
	public bool Bool_2;

	[NonSerialized]
	public bool Bool_3;

	[NonSerialized]
	public Transform Transform_0;

	[NonSerialized]
	public Transform Transform_1;

	[NonSerialized]
	public Transform Transform_2;

	[NonSerialized]
	public GameObject GameObject_0;

	[NonSerialized]
	public bool Bool_4 = true;

	[NonSerialized]
	public GClass178 Gclass178_0;

	[NonSerialized]
	public float Float_2;

	[NonSerialized]
	public Vector3 Vector3_0;

	public GClass253(BotOwner bot)
		: base(bot)
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
		Gclass178_0 = new GClass183(bot);
		GameObject_0 = GameObject.Find("ShuttleLookAt");
		Bool_2 = GameObject_0 != null;
		if (gameObject != null && gameObject2 != null)
		{
			Transform_0 = gameObject.transform;
			Transform_1 = gameObject2.transform;
			Bool_3 = true;
		}
		else if (DebugBotData.UseDebugData && DebugBotData.Instance.DebugBrain)
		{
			Debug.LogError("SHUTTLE MOVE CAN'T BE EXECUTED!   pls create right objects (look at this cs file to understand)");
		}
		Transform_2 = Transform_0;
	}

	public override void UpdateNodeByBrain(GClass26 data)
	{
		if (!Bool_3)
		{
			return;
		}
		method_5();
		if (Bool_0)
		{
			if (WithDoor)
			{
				bool val = method_0() == DoorInteractionStatus.CanRun;
				if (NoSprint)
				{
					BotOwner_0.Sprint(val: false);
				}
				else
				{
					BotOwner_0.Sprint(val);
				}
			}
			else
			{
				BotOwner_0.Sprint(!NoSprint);
			}
		}
		Vector3 vector = BotOwner_0.Position - Transform_2.position;
		if (Bool_2)
		{
			BotOwner_0.Mover.SetTargetMoveSpeed(GameObject_0.transform.lossyScale.x);
		}
		else
		{
			BotOwner_0.Mover.SetTargetMoveSpeed(1f);
		}
		vector.y = ((Math.Abs(vector.y) > 1f) ? vector.y : 0f);
		Float_1 = vector.magnitude;
		if (Bool_4)
		{
			Gclass178_0.UpdateNodeByBrain(data as GClass27);
		}
		else if (Bool_2 && NoSprint)
		{
			BotOwner_0.Steering.LookToPoint(GameObject_0.transform.position);
		}
		else if (Bool_0)
		{
			BotOwner_0.Steering.LookToMovingDirection();
		}
		else
		{
			BotOwner_0.Steering.LookToDirection(Vector3_0);
		}
		if (!(Float_1 > BotOwner_0.Settings.FileSettings.Move.REACH_DIST))
		{
			method_6();
		}
	}

	public void method_5()
	{
		if (Float_2 < Time.time)
		{
			Float_2 = Time.time + 2f;
			BotOwner_0.GoToPoint(Transform_2.position);
			BotOwner_0.SetPose(1f);
			Bool_0 = true;
		}
	}

	public void method_6()
	{
		if (Bool_0)
		{
			if (Transform_2 == Transform_0)
			{
				Transform_2 = Transform_1;
			}
			else
			{
				Transform_2 = Transform_0;
			}
			if (Bool_1)
			{
				BotOwner_0.SetPose(0f);
			}
			BotOwner_0.StopMove();
			Vector3_0 = new Vector3(GClass856.Random(-1f, 1f), 0f, GClass856.Random(-1f, 1f));
			Float_2 = Time.time + 5f;
			Bool_0 = false;
		}
	}
}
