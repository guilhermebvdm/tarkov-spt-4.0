using System.Collections.Generic;
using EFT;
using UnityEngine;
using UnityEngine.AI;

public class ShootReservWay : AReserveWayAction
{
	public List<Transform> TargetToShoot;

	public float StayPeriod = 7f;

	public float ShootPeriod = 3f;

	private float float_0;

	private bool bool_0;

	private bool bool_1;

	private Vector3 vector3_0;

	public override Vector3 GoTo => base.transform.position;

	public override Vector3 LookShootTo => vector3_0;

	public override ReserveWayResult ManualUpdate(BotOwner bot)
	{
		if (bool_0)
		{
			if (bot.PatrollingData.Status != PatrolStatus.stay)
			{
				_cuResult = ReserveWayResult.move;
			}
			if (float_0 < Time.time)
			{
				float num = (bool_1 ? StayPeriod : ShootPeriod);
				float_0 = Time.time + num;
				bool_1 = !bool_1;
				_cuResult = (bool_1 ? ReserveWayResult.shoot : ReserveWayResult.stay);
			}
		}
		else
		{
			_cuResult = ReserveWayResult.move;
		}
		return _cuResult;
	}

	public override void ComeTo(BotOwner bot)
	{
		vector3_0 = GClass856.RandomElement(TargetToShoot).position;
		bool_0 = true;
		_cuResult = ReserveWayResult.stay;
	}

	public override void AutoFix()
	{
		base.transform.localPosition = Vector3.zero;
		if (NavMesh.SamplePosition(base.transform.position, out var hit, 1f, -1))
		{
			base.transform.position = hit.position;
		}
	}

	public override void RefreshData()
	{
		CheckWayFromParent("Walk reserv way", base.transform.position);
		if (NavMesh.SamplePosition(base.transform.position, out var hit, 1f, -1))
		{
			base.transform.position = hit.position;
		}
		else
		{
			Debug.LogError("ShootReservWay transform.position TargetToShoot " + base.gameObject.name + "   can find place to shoot from it");
		}
		if (TargetToShoot != null && TargetToShoot.Count >= 1)
		{
			List<Transform> list = new List<Transform>();
			foreach (Transform item in TargetToShoot)
			{
				if (item != null)
				{
					list.Add(item);
				}
				else
				{
					Debug.LogError("TargetToShoot have zero transform " + base.gameObject.name + "   FIXED");
				}
			}
			TargetToShoot = list;
		}
		else
		{
			Debug.LogError("wrong zero pos TargetToShoot " + base.gameObject.name);
		}
		CheckPoint(base.transform.position, "Shoot reserv way ");
	}

	public override void RefreshBot()
	{
		bool_0 = false;
		_cuResult = ReserveWayResult.stay;
		float_0 = 0f;
	}

	public override void DrawGizmos()
	{
		foreach (Transform item in TargetToShoot)
		{
			Vector3 position = item.position;
			Gizmos.color = new Color(0.2f, 0.8f, 0.4f, 0.9f);
			Gizmos.DrawCube(position, new Vector3(1f, 4f, 1f) * 0.2f);
			Gizmos.DrawCube(position, new Vector3(4f, 1f, 1f) * 0.2f);
			Gizmos.DrawCube(position, new Vector3(1f, 1f, 4f) * 0.2f);
			Vector3 vector = base.transform.position + Vector3.up * 1.6f;
			Gizmos.DrawCube(vector, new Vector3(1f, 1f, 1f) * 0.3f);
			Gizmos.DrawCube(base.transform.position, new Vector3(1f, 1f, 1f) * 0.4f);
			Gizmos.DrawLine(vector, position);
		}
	}
}
