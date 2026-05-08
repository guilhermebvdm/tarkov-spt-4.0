using EFT;
using UnityEngine;
using UnityEngine.AI;

public class DropItemAndHealReservWay : AReserveWayAction
{
	public Transform lookToPoint;

	public float STAY_PERIOD = 35f;

	private bool bool_0;

	private bool bool_1;

	private float float_0;

	public override Vector3 GoTo => base.transform.position;

	public override Vector3 LookShootTo => lookToPoint.position;

	public override ReserveWayResult ManualUpdate(BotOwner bot)
	{
		if (bool_0)
		{
			if (bool_1)
			{
				if (float_0 < Time.time)
				{
					bool_0 = false;
					bot.Medecine.SurgicalKit.SetRandomPartToHeal();
					bot.Medecine.SurgicalKit.ApplyToCurrentPart();
				}
			}
			else if (float_0 < Time.time)
			{
				float_0 = Time.time + 3f;
				bot.ItemDropper.RefreshItemToDrop();
				bot.ItemDropper.TryDoDrop();
				bool_1 = true;
			}
		}
		if (bot.PatrollingData.Status == PatrolStatus.stay)
		{
			_cuResult = ReserveWayResult.stay;
		}
		else
		{
			_cuResult = ReserveWayResult.move;
		}
		return _cuResult;
	}

	public override void RefreshData()
	{
		CheckWayFromParent("Drop item and heal reserv way", base.transform.position);
		if (NavMesh.SamplePosition(base.transform.position, out var hit, 1f, -1))
		{
			base.transform.position = hit.position;
		}
		_ = lookToPoint == null;
		CheckPoint(base.transform.position, "Drop item reserv way ");
	}

	public override void RefreshBot()
	{
		_cuResult = ReserveWayResult.move;
	}

	public override float TimeToUse(BotOwner owner)
	{
		return STAY_PERIOD;
	}

	public override void ComeTo(BotOwner bot)
	{
		_cuResult = ReserveWayResult.drop;
		float_0 = Time.time + 0.3f;
		bool_0 = true;
		bool_1 = false;
	}

	public override void AutoFix()
	{
		base.transform.localPosition = Vector3.zero;
		if (NavMesh.SamplePosition(base.transform.position, out var hit, 1f, -1))
		{
			base.transform.position = hit.position;
		}
	}

	public override void DrawGizmos()
	{
		Gizmos.color = new Color(0.5f, 0.1f, 0.7f, 0.9f);
		Gizmos.DrawCube(lookToPoint.position, new Vector3(1f, 4f, 1f) * 0.2f);
		Gizmos.DrawCube(lookToPoint.position, new Vector3(1f, 1f, 4f) * 0.2f);
		Vector3 vector = base.transform.position + Vector3.up * 1.6f;
		Gizmos.DrawCube(vector, new Vector3(1f, 1f, 1f) * 0.3f);
		Gizmos.DrawCube(base.transform.position, new Vector3(1f, 1f, 1f) * 0.4f);
		Gizmos.DrawLine(vector, lookToPoint.position);
	}
}
