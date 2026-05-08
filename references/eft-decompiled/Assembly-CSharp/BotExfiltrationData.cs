using System;
using Comfort.Common;
using EFT;

public class BotExfiltrationData : GClass429
{
	[NonSerialized]
	public float MIN_SEC_TO_EXFILTRATION = 1200f;

	[NonSerialized]
	public float MAX_SEC_TO_EXFILTRATION = 1200f;

	[NonSerialized]
	public float TimeToExfiltration;

	[NonSerialized]
	public float LeaveTime_1 = float.MaxValue;

	public float LeaveTime => LeaveTime_1;

	public BotExfiltrationData(BotOwner owner)
		: base(owner)
	{
		float a = MIN_SEC_TO_EXFILTRATION;
		float b = MAX_SEC_TO_EXFILTRATION;
		if (BotOwner_0.BotsController.BotLocationModifier != null && (BotOwner_0.BotsController.BotLocationModifier.MinExfiltrationTime > 0f || BotOwner_0.BotsController.BotLocationModifier.MaxExfiltrationTime > 0f))
		{
			a = BotOwner_0.BotsController.BotLocationModifier.MinExfiltrationTime;
			b = BotOwner_0.BotsController.BotLocationModifier.MaxExfiltrationTime;
		}
		TimeToExfiltration = GClass856.Random(a, b);
	}

	public bool BotInExfiltrationArea()
	{
		if (LeaveTime_1 > 0f)
		{
			return LeaveTime_1 < float.MaxValue;
		}
		return false;
	}

	public void SetLeaveTime(float time)
	{
		LeaveTime_1 = time;
	}

	public void ResetLeaveTime()
	{
		LeaveTime_1 = float.MaxValue;
	}

	public bool WannaLeave()
	{
		AbstractGame instance = Singleton<AbstractGame>.Instance;
		if (instance == null)
		{
			return false;
		}
		return GClass1893.PastTimeSeconds(instance.GameTimer) > TimeToExfiltration;
	}
}
