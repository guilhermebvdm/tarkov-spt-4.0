using System;
using System.Runtime.CompilerServices;
using System.Threading;
using EFT;
using JetBrains.Annotations;
using UnityEngine;

public class GoalTargetClass : GClass429
{
	[NonSerialized]
	public ZeroGoalTarget ZeroGoalTarget_0;

	[NonSerialized]
	public PlaceForCheck PlaceForCheck_0;

	[CompilerGenerated]
	private Action<PlaceForCheck, PlaceForCheck> action_0;

	[CompilerGenerated]
	private Action action_1;

	public PlaceForCheck GoalTarget
	{
		get
		{
			return PlaceForCheck_0;
		}
		set
		{
			if (PlaceForCheck_0 != null)
			{
				if (PlaceForCheck_0 != value)
				{
					PlaceForCheck_0.IsCome = false;
					if (value != null)
					{
						value.IsCome = false;
					}
				}
				PlaceForCheck_0.CheckingPlayer = null;
			}
			PlaceForCheck placeForCheck_ = PlaceForCheck_0;
			PlaceForCheck_0 = value;
			if (PlaceForCheck_0 != null)
			{
				PlaceForCheck_0.CheckingPlayer = BotOwner_0;
			}
			BotOwner_0.Memory.CheckIsInCover2();
			if (PlaceForCheck_0 != placeForCheck_)
			{
				action_0?.Invoke(placeForCheck_, value);
			}
		}
	}

	public double CreatedTime => PlaceForCheck_0.CreatedTime;

	public bool IsDanger => PlaceForCheck_0.IsDanger;

	public PlaceForCheckType Type => PlaceForCheck_0.Type;

	public int EnvironmentId
	{
		get
		{
			if (PlaceForCheck_0 != null)
			{
				return PlaceForCheck_0.EnvironmentId;
			}
			return 0;
		}
	}

	public Vector3? Position
	{
		get
		{
			if (PlaceForCheck_0 != null)
			{
				return PlaceForCheck_0.Position;
			}
			return null;
		}
	}

	public event Action<PlaceForCheck, PlaceForCheck> OnGoalTargetChange
	{
		[CompilerGenerated]
		add
		{
			Action<PlaceForCheck, PlaceForCheck> action = action_0;
			Action<PlaceForCheck, PlaceForCheck> action2;
			do
			{
				action2 = action;
				Action<PlaceForCheck, PlaceForCheck> value2 = (Action<PlaceForCheck, PlaceForCheck>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<PlaceForCheck, PlaceForCheck> action = action_0;
			Action<PlaceForCheck, PlaceForCheck> action2;
			do
			{
				action2 = action;
				Action<PlaceForCheck, PlaceForCheck> value2 = (Action<PlaceForCheck, PlaceForCheck>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public event Action OnZeroGoalSetted
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

	public GoalTargetClass([NotNull] BotOwner owner)
		: base(owner)
	{
	}

	public bool HavePlaceTarget()
	{
		return PlaceForCheck_0 != null;
	}

	public bool HaveZeroTarget()
	{
		return ZeroGoalTarget_0 != null;
	}

	public void SetZeroGoal()
	{
		if (ZeroGoalTarget_0 == null)
		{
			ZeroGoalTarget_0 = new ZeroGoalTarget();
			action_1?.Invoke();
		}
	}

	public void SetTarget(PlaceForCheck place)
	{
		GoalTarget = place;
		for (int i = 0; i < BotOwner_0.BotsGroup.MembersCount; i++)
		{
			BotOwner botOwner = BotOwner_0.BotsGroup.Member(i);
			if (botOwner.Id != BotOwner_0.Id && !botOwner.Memory.GoalTarget.HavePlaceTarget())
			{
				botOwner.Memory.GoalTarget.SetZeroGoal();
			}
		}
	}

	public void Clear()
	{
		method_0();
	}

	public bool CanCheckBody()
	{
		if (HavePlaceTarget())
		{
			if (BotOwner_0.DeadBodyData.TargetDeadBody == null)
			{
				return false;
			}
			if (Time.time - PlaceForCheck_0.CreatedTime > 30f)
			{
				return true;
			}
		}
		return false;
	}

	public void PointLookComplete(int lookingIndex)
	{
		PlaceForCheck_0.PointLookComplete(lookingIndex);
	}

	public bool HaveMainTarget()
	{
		if (!HavePlaceTarget())
		{
			return HaveZeroTarget();
		}
		return true;
	}

	public void method_0()
	{
		GoalTarget = null;
		ZeroGoalTarget_0 = null;
	}
}
