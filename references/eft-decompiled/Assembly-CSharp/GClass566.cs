using System;
using EFT;
using UnityEngine;
using UnityEngine.AI;

public class GClass566 : GClass565
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

	[NonSerialized]
	public GClass510 Gclass510_0;

	[NonSerialized]
	public GClass570 Gclass570_0;

	[NonSerialized]
	public float Float_13;

	public GClass566(BotOwner owner, int index, GClass510 bossPatrolMoveSimple)
		: base(owner, index)
	{
		Gclass510_0 = bossPatrolMoveSimple;
		Gclass570_0 = bossPatrolMoveSimple.AllFollowersPatrolInfo;
	}

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
		method_8();
		BotOwner_0.Sprint(Bool_7);
		if (Bool_6)
		{
			BotOwner_0.StopMove();
			method_7();
			return;
		}
		bool extraTarget;
		GStruct25 gStruct = method_1(out extraTarget);
		bool flag;
		float dist;
		bool num = (flag = GClass510.IsCome(BotOwner_0, gStruct.Position, extraTarget, out dist)) != Bool_5;
		if (Float_11 < Time.time)
		{
			Float_11 = Time.time + 5f;
			method_4(gStruct);
		}
		if (num && BotOwner_0.Settings.FileSettings.Patrol.FOLLOWER_START_MOVE_DELAY > 0f)
		{
			Float_0 = Time.time + BotOwner_0.Settings.FileSettings.Patrol.FOLLOWER_START_MOVE_DELAY;
		}
		Bool_5 = flag;
		if (flag)
		{
			float pose = ((!gStruct.HasValue) ? 1f : (gStruct.SitAtEnd ? 0f : 1f));
			BotOwner_0.SetPose(pose);
			method_2(gStruct, isClose: true);
			BotOwner_0.StopMove();
		}
		else
		{
			if (base.HaveProblems)
			{
				BotObserveDataClass.SetVector(BotOwner_0.Mover.NormDirCurPoint);
			}
			if (Bool_2)
			{
				method_3(gStruct.Position, withSprint: false);
			}
			else
			{
				PatrolPoint patrolPoint = BossPatrolPoint();
				if (patrolPoint != null)
				{
					method_3(patrolPoint.Position, withSprint: false);
				}
			}
			BotObserveDataClass.Update();
		}
		if (Bool_1)
		{
			base.HaveProblems = false;
		}
		else if (!Bool_4)
		{
			base.HaveProblems = false;
		}
		else
		{
			base.HaveProblems = false;
		}
	}

	public void method_4(GStruct25 finalPos)
	{
		if (Time.time - Float_13 < 10f)
		{
			return;
		}
		Vector3 sourcePosition;
		if (Gclass570_0 == null)
		{
			GClass855.SideTurn side = ((!GClass856.IsTrue100(50f)) ? GClass855.SideTurn.right : GClass855.SideTurn.left);
			Vector3 vector = GClass855.Rotate90(BotOwner_0.LookDirection, side);
			sourcePosition = BotOwner_0.Position - vector * 10f;
		}
		else
		{
			float offset = Gclass570_0.GetOffset();
			Vector3 vector2 = GClass855.RotateOnAngUp(BotOwner_0.LookDirection, offset);
			sourcePosition = BotOwner_0.Position - vector2 * 10f;
		}
		if (!NavMesh.SamplePosition(sourcePosition, out var hit, 5f, -1))
		{
			return;
		}
		NavMeshPath navMeshPath = new NavMeshPath();
		NavMesh.CalculatePath(BotOwner_0.Position, hit.position, -1, navMeshPath);
		if (navMeshPath.status != NavMeshPathStatus.PathComplete)
		{
			return;
		}
		NavMeshPath navMeshPath2 = new NavMeshPath();
		float num = GClass371.CalculatePathLength(navMeshPath);
		float magnitude = (BotOwner_0.Position - hit.position).magnitude;
		if (!(num > magnitude * 2f))
		{
			NavMesh.CalculatePath(navMeshPath.corners[navMeshPath.corners.Length - 1], finalPos.Position, -1, navMeshPath2);
			if (navMeshPath2.status == NavMeshPathStatus.PathComplete)
			{
				Vector3[] way = method_5(navMeshPath.corners, navMeshPath2.corners);
				BotOwner_0.Mover.GoToByWay(way, 0.5f);
				Float_13 = Time.time;
			}
		}
	}

	public Vector3[] method_5(Vector3[] p1, Vector3[] p2)
	{
		Vector3[] array = new Vector3[p1.Length + p2.Length];
		int num = 0;
		for (int i = 0; i < p1.Length; i++)
		{
			array[i] = p1[i];
			num++;
		}
		for (int j = 0; j < p2.Length; j++)
		{
			array[num] = p2[j];
			num++;
		}
		return array;
	}

	public void method_6()
	{
		float x = GClass856.Random(-1f, 1f);
		float z = GClass856.Random(-1f, 1f);
		Vector3 direction = GClass369.Test4Sides(new Vector3(x, 0f, z), BotOwner_0.GetPlayer.PlayerBones.Head.position);
		BotObserveDataClass.SetVector(direction);
	}

	public void method_7()
	{
		BotObserveDataClass.Update();
	}

	public void method_8()
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
