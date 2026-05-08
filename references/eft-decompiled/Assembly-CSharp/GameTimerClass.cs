using System;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;

public class GameTimerClass
{
	public enum EGameTimerStatus
	{
		Unknown,
		Started,
		Stopped
	}

	[NonSerialized]
	public DateTime? Nullable_0;

	[NonSerialized]
	public DateTime? Nullable_1;

	[NonSerialized]
	public DateTime? Nullable_2;

	[NonSerialized]
	[CompilerGenerated]
	public EGameTimerStatus EgameTimerStatus_0;

	[NonSerialized]
	[CompilerGenerated]
	public TimeSpan? Nullable_3;

	[CompilerGenerated]
	private Action action_0;

	[CompilerGenerated]
	private Action action_1;

	[CompilerGenerated]
	private Action action_2;

	public EGameTimerStatus Status
	{
		[CompilerGenerated]
		get
		{
			return EgameTimerStatus_0;
		}
		[CompilerGenerated]
		set
		{
			EgameTimerStatus_0 = value;
		}
	}

	public TimeSpan? SessionTime
	{
		[CompilerGenerated]
		get
		{
			return Nullable_3;
		}
		[CompilerGenerated]
		set
		{
			Nullable_3 = value;
		}
	}

	public DateTime? StartDateTime => Nullable_0;

	public DateTime? EscapeDateTime => Nullable_1;

	public TimeSpan PastTime => method_0();

	public DateTime StopDateTime => Nullable_2.GetValueOrDefault();

	public event Action StartedEvent
	{
		[CompilerGenerated]
		add
		{
			Action action = action_0;
			Action action2;
			do
			{
				action2 = action;
				Action value2 = (Action)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action action = action_0;
			Action action2;
			do
			{
				action2 = action;
				Action value2 = (Action)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public event Action StoppedEvent
	{
		[CompilerGenerated]
		add
		{
			Action action = action_1;
			Action action2;
			do
			{
				action2 = action;
				Action value2 = (Action)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_1, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action action = action_1;
			Action action2;
			do
			{
				action2 = action;
				Action value2 = (Action)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_1, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public event Action SessionTimeChanged
	{
		[CompilerGenerated]
		add
		{
			Action action = action_2;
			Action action2;
			do
			{
				action2 = action;
				Action value2 = (Action)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_2, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action action = action_2;
			Action action2;
			do
			{
				action2 = action;
				Action value2 = (Action)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_2, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public GameTimerClass(TimeSpan? sessionTime)
	{
		SessionTime = sessionTime;
		Status = EGameTimerStatus.Unknown;
	}

	public void Start(DateTime? startTime = null, TimeSpan? sessionTime = null)
	{
		if (!Nullable_0.HasValue && !Nullable_2.HasValue)
		{
			DateTime utcNow = EFTDateTimeClass.UtcNow;
			if (!startTime.HasValue)
			{
				Nullable_0 = utcNow;
			}
			else
			{
				if (startTime > utcNow)
				{
					Debug.LogError($"Invalid startTime:{startTime} utcNow:{utcNow}");
					startTime = utcNow;
				}
				Nullable_0 = startTime;
			}
			if (sessionTime.HasValue)
			{
				ChangeSessionTime(sessionTime.Value);
			}
			else
			{
				Nullable_1 = Nullable_0 + SessionTime;
			}
			Debug.Log($"GameTimer utcNow:{utcNow} start:{StartDateTime} escape:{EscapeDateTime} PastTime:{PastTime} stop:{StopDateTime} (startTime:{startTime}, sessionTime:{sessionTime})");
			Status = EGameTimerStatus.Started;
			action_0?.Invoke();
			return;
		}
		throw new InvalidOperationException("Start");
	}

	public void Stop()
	{
		if (Nullable_2.HasValue)
		{
			throw new InvalidOperationException("Stop");
		}
		Nullable_2 = EFTDateTimeClass.UtcNow;
		Status = EGameTimerStatus.Stopped;
		action_1?.Invoke();
	}

	public void ChangeSessionTime(TimeSpan sessionTime)
	{
		if (Nullable_2.HasValue)
		{
			throw new InvalidOperationException("ChangeSessionTime");
		}
		Debug.Log($"Session time update to:{sessionTime} from:{SessionTime}");
		SessionTime = sessionTime;
		if (Nullable_0.HasValue)
		{
			DateTime value = Nullable_0.Value;
			TimeSpan? sessionTime2 = SessionTime;
			DateTime? dateTime = value + sessionTime2;
			Debug.Log($"Escape dateTime update to:{dateTime} from:{Nullable_1}");
			Nullable_1 = dateTime;
		}
		action_2?.Invoke();
	}

	public TimeSpan method_0()
	{
		if (!Nullable_0.HasValue)
		{
			return TimeSpan.Zero;
		}
		TimeSpan timeSpan = (Nullable_2.HasValue ? (Nullable_2.Value - Nullable_0.Value) : (EFTDateTimeClass.UtcNow - Nullable_0.Value));
		if (timeSpan < TimeSpan.Zero)
		{
			return TimeSpan.Zero;
		}
		if (SessionTime.HasValue && timeSpan > SessionTime.Value)
		{
			timeSpan = SessionTime.Value;
		}
		return timeSpan;
	}
}
