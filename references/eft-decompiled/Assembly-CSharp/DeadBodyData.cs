using System;
using System.Collections;
using EFT;
using UnityEngine;

public class DeadBodyData : GClass429
{
	public GClass386 TargetDeadBody;

	public bool IsNear;

	public bool WillDoControlShoot;

	[NonSerialized]
	public Vector3 CachePos;

	public DeadBodyData(BotOwner owner)
		: base(owner)
	{
		BotOwner_0.ShootData.OnTriggerPressed += method_0;
	}

	public void EndInspection()
	{
		TargetDeadBody.InspectionComplete();
		TargetDeadBody = null;
	}

	public void Come()
	{
		IsNear = true;
		BotOwner_0.BotTalk.TrySay(EPhraseTrigger.OnEnemyDown);
		WillDoControlShoot = GClass856.Random(0f, 100f) < (float)BotOwner_0.Settings.FileSettings.Patrol.CHANCE_TO_SHOOT_DEADBODY;
	}

	public void SetBody(GClass386 body)
	{
		if (body != TargetDeadBody)
		{
			IsNear = false;
			TargetDeadBody = body;
			CachePos = TargetDeadBody.BodyPosition;
			if (Physics.Raycast(CachePos, Vector3.down, out var hitInfo, 5f))
			{
				CachePos = hitInfo.point + Vector3.up * 0.2f;
			}
		}
	}

	public Vector3 GetPointToShoot()
	{
		return CachePos;
	}

	public bool HaveBodyToCheck()
	{
		return TargetDeadBody != null;
	}

	public void method_0()
	{
		if (TargetDeadBody != null && IsNear)
		{
			EndInspection();
			BotOwner_0.StartCoroutine(method_1());
		}
	}

	public IEnumerator method_1()
	{
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();
		BotOwner_0.Steering.SetYAngle(0f);
	}
}
