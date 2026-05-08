using System;

public abstract class GClass1893
{
	public static void Start(this GameTimerClass gameTimer, float pastTime, int sessionSeconds)
	{
		DateTime value = EFTDateTimeClass.UtcNow - TimeSpan.FromSeconds(pastTime);
		gameTimer.Start(value, TimeSpan.FromSeconds(sessionSeconds));
	}

	public static int SessionSeconds(this GameTimerClass timer)
	{
		TimeSpan? sessionTime = timer.SessionTime;
		if (!sessionTime.HasValue)
		{
			return 0;
		}
		return (int)sessionTime.Value.TotalSeconds;
	}

	public static float PastTimeSeconds(this GameTimerClass timer)
	{
		return (float)timer.PastTime.TotalSeconds;
	}

	public static float EscapeTimeSeconds(this GameTimerClass timer)
	{
		DateTime? escapeDateTime = timer.EscapeDateTime;
		return (float)(escapeDateTime.HasValue ? (escapeDateTime.Value - EFTDateTimeClass.UtcNow) : TimeSpan.MaxValue).TotalSeconds;
	}

	public static TimeSpan EscapeTimeTimeSpan(this GameTimerClass timer)
	{
		DateTime? escapeDateTime = timer.EscapeDateTime;
		if (!escapeDateTime.HasValue)
		{
			return TimeSpan.MaxValue;
		}
		return escapeDateTime.Value - EFTDateTimeClass.UtcNow;
	}

	public static void TryStop(this GameTimerClass timer)
	{
		if (timer.Status == GameTimerClass.EGameTimerStatus.Started)
		{
			timer.Stop();
		}
	}

	public static bool Started(this GameTimerClass timer)
	{
		return timer.Status == GameTimerClass.EGameTimerStatus.Started;
	}
}
