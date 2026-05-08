using System;
using EFT;
using UnityEngine;

public class GClass244 : GClass200<GClass26>
{
	[NonSerialized]
	public const float Float_0 = 1f;

	[NonSerialized]
	public GClass25 Gclass25_0;

	[NonSerialized]
	public Vector3 Vector3_0;

	public GClass244(BotOwner bot)
		: base(bot)
	{
		Gclass25_0 = new GClass25(5f, method_5);
	}

	public void method_5()
	{
		Vector3 position = BotOwner_0.PatrollingData.ExfiltrationData.CachedExfiltrationPoint.GetPosition(BotOwner_0);
		BotOwner_0.GoToPoint(position);
	}

	public override void UpdateNodeByBrain(GClass26 data)
	{
		method_0();
		PatrolExfiltrationPointsData exfiltrationData = BotOwner_0.PatrollingData.ExfiltrationData;
		AIExfiltrationPoint cachedExfiltrationPoint = exfiltrationData.CachedExfiltrationPoint;
		Vector3 position = BotOwner_0.PatrollingData.ExfiltrationData.CachedExfiltrationPoint.GetPosition(BotOwner_0);
		float num = GClass856.SqrDistance(BotOwner_0.Position, position);
		if (num < 1f)
		{
			BotOwner_0.StopMove();
			BotOwner_0.Steering.LookToPoint(Vector3_0);
			exfiltrationData.ComeToExfiltrationPoint();
		}
		else
		{
			BotOwner_0.Sprint(val: true);
			Gclass25_0.Update();
			Vector3_0 = BotOwner_0.Position + BotOwner.STAY_HEIGHT;
		}
		if (num < 1f)
		{
			if (BotOwner_0.Exfiltration.LeaveTime > Time.time + 99999f)
			{
				BotOwner_0.Exfiltration.SetLeaveTime(Time.time + cachedExfiltrationPoint.ExfiltrationTime);
			}
			if (Time.time > BotOwner_0.Exfiltration.LeaveTime)
			{
				BotOwner_0.LeaveData.RemoveFromMap();
			}
		}
		else
		{
			BotOwner_0.Exfiltration.ResetLeaveTime();
		}
	}
}
