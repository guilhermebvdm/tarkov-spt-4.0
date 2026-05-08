using System.Collections.Generic;
using EFT;
using UnityEngine;
using UnityEngine.AI;

public class WalkReservWay : AReserveWayAction
{
	public List<Vector3> PositionsToWalk = new List<Vector3>();

	private float float_0;

	private float float_1;

	private Vector3 vector3_0;

	public override Vector3 GoTo => vector3_0;

	public override Vector3 LookShootTo => Vector3.right;

	public void method_0()
	{
		_cuResult = ReserveWayResult.stay;
		float_0 = Time.time + 15f;
	}

	public override ReserveWayResult ManualUpdate(BotOwner bot)
	{
		if (float_1 < Time.time)
		{
			switch (_cuResult)
			{
			case ReserveWayResult.move:
				if (bot.Mover.IsComeTo(0.4f, onCover: false))
				{
					method_0();
				}
				break;
			case ReserveWayResult.stay:
				if (float_0 < Time.time)
				{
					vector3_0 = GClass856.RandomElement(PositionsToWalk);
					float_0 = Time.time + 15f;
					_cuResult = ReserveWayResult.move;
				}
				break;
			}
			float_1 = Time.time + 0.2f;
		}
		return _cuResult;
	}

	public override void RefreshData()
	{
		PositionsToWalk = new List<Vector3>();
		foreach (Transform item in base.transform)
		{
			if (NavMesh.SamplePosition(item.position, out var hit, 1f, -1))
			{
				PositionsToWalk.Add(hit.position);
			}
			CheckPoint(item.position, "Sit reserv way ");
		}
		if (PositionsToWalk.Count <= 0)
		{
			Debug.LogError("wrong way at WalkReservWay have hot enought points " + base.gameObject.name);
			return;
		}
		foreach (Vector3 item2 in PositionsToWalk)
		{
			CheckWayFromParent("Walk reserv way", item2);
		}
	}

	public override void RefreshBot()
	{
		_cuResult = ReserveWayResult.stay;
		float_0 = 0f;
	}

	public override void DrawGizmos()
	{
		Gizmos.color = new Color(0.8f, 0.1f, 0f, 6.9f);
		Vector3 vector = Vector3.up * 0.2f;
		for (int i = 0; i < PositionsToWalk.Count - 1; i++)
		{
			Vector3 vector2 = PositionsToWalk[i];
			Vector3 vector3 = PositionsToWalk[i + 1];
			Gizmos.DrawCube(vector2, Vector3.one * 0.2f);
			Gizmos.DrawLine(vector2 + vector, vector + vector3);
			if (i == PositionsToWalk.Count - 2)
			{
				Gizmos.DrawCube(vector3, Vector3.one * 0.2f);
			}
		}
	}

	public override void ComeTo(BotOwner bot)
	{
	}

	public override void AutoFix()
	{
		Debug.LogError("Walk reserv way have no auto fix. Make it by yourself");
	}
}
