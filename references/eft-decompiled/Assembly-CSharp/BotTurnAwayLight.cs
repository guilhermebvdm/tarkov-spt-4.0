using System;
using System.Runtime.CompilerServices;
using EFT;
using JetBrains.Annotations;
using UnityEngine;

public class BotTurnAwayLight : GClass429
{
	[CompilerGenerated]
	public class Class244
	{
		public BotTurnAwayLight botTurnAwayLight_0;

		public Func<Vector3?> posToTurnAway;

		public void method_0()
		{
			if (botTurnAwayLight_0.Timer != null && botTurnAwayLight_0.Timer.IsActive)
			{
				botTurnAwayLight_0.Timer.Stop();
				botTurnAwayLight_0.Timer = null;
			}
			botTurnAwayLight_0.PosToTurnAway = posToTurnAway;
			botTurnAwayLight_0.IsActive_1 = true;
			botTurnAwayLight_0.StartTime = Time.time;
		}
	}

	[NonSerialized]
	public const float MAX_TURN_AWAY_TIME = 4f;

	[NonSerialized]
	public bool IsActive_1;

	[NonSerialized]
	public float StartTime;

	[NonSerialized]
	public Func<Vector3?> PosToTurnAway;

	[NonSerialized]
	public GClass641.IBotTimer Timer;

	public bool IsActive
	{
		get
		{
			if (Time.time - StartTime > 4f)
			{
				IsActive_1 = false;
			}
			return IsActive_1;
		}
	}

	public BotTurnAwayLight([NotNull] BotOwner owner)
		: base(owner)
	{
	}

	public void Update()
	{
		if (!IsActive_1)
		{
			return;
		}
		Vector3? vector = PosToTurnAway();
		if (vector.HasValue)
		{
			Vector3 dir = BotOwner_0.Position - vector.Value;
			BotOwner_0.Steering.LookToDirection(dir);
			if (Time.time - StartTime > 4f)
			{
				IsActive_1 = false;
			}
		}
		else
		{
			IsActive_1 = false;
		}
	}

	public void Activate(Func<Vector3?> posToTurnAway, float delay = 0f)
	{
		Action action = delegate
		{
			if (Timer != null && Timer.IsActive)
			{
				Timer.Stop();
				Timer = null;
			}
			PosToTurnAway = posToTurnAway;
			IsActive_1 = true;
			StartTime = Time.time;
		};
		if (delay > 0f)
		{
			if (Timer != null && Timer.IsActive)
			{
				Timer.Stop();
			}
			Timer = StaticManager.Instance.TimerManager.MakeTimer(TimeSpan.FromSeconds(delay));
			Timer.OnTimer += action;
		}
		else
		{
			action();
		}
	}
}
