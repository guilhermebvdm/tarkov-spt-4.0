using System.Collections.Generic;
using EFT;
using UnityEngine;
using UnityEngine.AI;

public class UseSurgeKitReservWay : AReserveWayAction
{
	public Transform lookToPoint;

	public float ChaceToUse100 = 50f;

	public float ChaceToSit100 = 50f;

	private bool bool_0;

	private EMedsType emedsType_0;

	private bool bool_1;

	private float float_0;

	private readonly List<EMedsType> list_0 = new List<EMedsType>();

	public override Vector3 GoTo => base.transform.position;

	public override Vector3 LookShootTo => lookToPoint.position;

	public override ReserveWayResult ManualUpdate(BotOwner bot)
	{
		if (bool_0 && float_0 < Time.time)
		{
			bool_0 = false;
			float_0 = Time.time + 30f;
			switch (emedsType_0)
			{
			case EMedsType.surgialKit:
				bot.Medecine.SurgicalKit.SetRandomPartToHeal();
				bot.Medecine.SurgicalKit.ApplyToCurrentPart();
				break;
			case EMedsType.firstAidKit:
				bot.Medecine.FirstAid.SetRandomPartToHeal();
				bot.Medecine.FirstAid.TryApplyToCurrentPart();
				break;
			case EMedsType.stimulator:
				bot.Medecine.Stimulators.TryApply();
				break;
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
		bot.SetPose(bool_1 ? 0f : 1f);
		return _cuResult;
	}

	public override void RefreshData()
	{
		CheckWayFromParent("Use surge kit item reserv way", base.transform.position);
		if (NavMesh.SamplePosition(base.transform.position, out var hit, 1f, -1))
		{
			base.transform.position = hit.position;
		}
		_ = lookToPoint == null;
		CheckPoint(base.transform.position, "Use surge kit item reserv way ");
	}

	public override void RefreshBot()
	{
		_cuResult = ReserveWayResult.move;
	}

	public override float TimeToUse(BotOwner owner)
	{
		return owner.Settings.FileSettings.Patrol.RESERVE_USE_SURGE_TIME_STAY;
	}

	public override void ComeTo(BotOwner bot)
	{
		bool_1 = GClass856.IsTrue100(ChaceToSit100);
		if (bot.Settings.FileSettings.Patrol.RESERV_CAN_USE_MEDS && GClass856.IsTrue100(ChaceToUse100))
		{
			float_0 = Time.time + 0.3f;
			bool_0 = true;
			list_0.Clear();
			if (bot.Medecine.SurgicalKit.HaveSmth2Use)
			{
				list_0.Add(EMedsType.surgialKit);
			}
			if (bot.Medecine.FirstAid.HaveSmth2Use)
			{
				list_0.Add(EMedsType.firstAidKit);
			}
			if (bot.Medecine.Stimulators.HaveSmt)
			{
				list_0.Add(EMedsType.stimulator);
			}
			emedsType_0 = GClass856.RandomElement(list_0);
		}
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

	public override void DrawGizmos()
	{
		Gizmos.color = new Color(0.5f, 0.1f, 0.4f, 0.9f);
		Gizmos.DrawCube(lookToPoint.position, new Vector3(1f, 4f, 1f) * 0.2f);
		Gizmos.DrawCube(lookToPoint.position, new Vector3(1f, 1f, 4f) * 0.2f);
		Vector3 vector = base.transform.position + Vector3.up * 1.6f;
		Gizmos.DrawCube(vector, new Vector3(1f, 1f, 1f) * 0.3f);
		Gizmos.DrawCube(base.transform.position, new Vector3(1f, 1f, 1f) * 0.4f);
		Gizmos.DrawLine(vector, lookToPoint.position);
	}
}
