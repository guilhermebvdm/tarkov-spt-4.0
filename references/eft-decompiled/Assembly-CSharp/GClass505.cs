using System;
using EFT;
using UnityEngine;
using UnityEngine.AI;

public class GClass505 : GClass429
{
	[NonSerialized]
	public const float Float_0 = 5.2f;

	[NonSerialized]
	public const float Float_1 = 10.5f;

	[NonSerialized]
	public const float Float_2 = 3.5f;

	[NonSerialized]
	public float Float_3;

	[NonSerialized]
	public float Float_4;

	[NonSerialized]
	public Player Player_0;

	[NonSerialized]
	public Vector3 Vector3_0;

	[NonSerialized]
	public bool Bool_0;

	[NonSerialized]
	public bool Bool_1;

	public GClass505(Player player, BotOwner owner)
		: base(owner)
	{
		Vector3_0 = owner.Position;
		Player_0 = player;
	}

	public void Update()
	{
		BotOwner_0.LookData.SetLookPointByHearing();
		if (!(Float_3 < Time.time))
		{
			return;
		}
		Float_3 = Time.time + GClass856.Random(1f, 2f);
		float num = Mathf.Abs((Bool_0 ? Vector3_0 : (Player_0.Position - BotOwner_0.Position)).magnitude);
		bool flag2;
		bool flag = (flag2 = num < 5.2f) != Bool_1;
		Bool_1 = flag2;
		if (flag2)
		{
			BotOwner_0.Mover.Sprint(val: false);
			if (Bool_0)
			{
				BotOwner_0.StopMove();
			}
			else
			{
				if (!(Float_4 < Time.time || flag))
				{
					return;
				}
				Float_4 = Time.time + 8f;
				float num2 = (float)GClass856.RandomSing() * GClass856.Random(0.3f, 3.5f);
				float num3 = (float)GClass856.RandomSing() * GClass856.Random(0.3f, 3.5f);
				float x = num2 + Player_0.Position.x;
				float z = num3 + Player_0.Position.z;
				if (NavMesh.SamplePosition(new Vector3(x, Player_0.Position.y, z), out var hit, 2f, -1))
				{
					if (BotOwner_0.GoToPoint(hit.position) != NavMeshPathStatus.PathComplete)
					{
						BotOwner_0.StopMove();
					}
				}
				else
				{
					BotOwner_0.StopMove();
				}
			}
		}
		else
		{
			method_0();
			bool val = num > 10.5f;
			BotOwner_0.Mover.Sprint(val);
		}
	}

	public void method_0()
	{
		Bool_0 = false;
		NavMeshHit hit;
		if (method_1(Player_0.Position) == NavMeshPathStatus.PathComplete)
		{
			Bool_0 = false;
		}
		else if (NavMesh.SamplePosition(Player_0.Position, out hit, 2f, -1) && method_1(Player_0.Position) != NavMeshPathStatus.PathComplete)
		{
			Bool_0 = true;
		}
		if (Bool_0)
		{
			CustomNavigationPoint freeClosePoint = BotOwner_0.Covers.GetFreeClosePoint(Player_0.Position, 0f);
			if (freeClosePoint != null)
			{
				Bool_0 = true;
				method_1(freeClosePoint.Position);
			}
		}
	}

	public NavMeshPathStatus method_1(Vector3 v)
	{
		NavMeshPathStatus num = BotOwner_0.GoToPoint(v);
		if (num == NavMeshPathStatus.PathComplete)
		{
			Vector3_0 = v;
		}
		return num;
	}
}
