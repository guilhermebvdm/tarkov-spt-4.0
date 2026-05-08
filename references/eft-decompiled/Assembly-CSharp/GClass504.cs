using System;
using System.Runtime.CompilerServices;
using System.Threading;
using EFT;
using JetBrains.Annotations;

public class GClass504 : GClass429
{
	public PatrolWay Way;

	public PatrolPointContainer _reservedPatrolPoint;

	[NonSerialized]
	public Action Action_0;

	[NonSerialized]
	public Action<PatrolPointContainer, int> Action_1;

	[NonSerialized]
	public PatrolPoint PatrolPoint_0;

	[CompilerGenerated]
	private Action<PatrolPointContainer> action_2;

	[NonSerialized]
	[CompilerGenerated]
	public PatrolPointContainer PatrolPointContainer_0;

	public PatrolPointContainer PatrolPoint
	{
		[CompilerGenerated]
		get
		{
			return PatrolPointContainer_0;
		}
		[CompilerGenerated]
		set
		{
			PatrolPointContainer_0 = value;
		}
	}

	public event Action<PatrolPointContainer> OnTargetPatrolPointSetted
	{
		[CompilerGenerated]
		add
		{
			Action<PatrolPointContainer> action = action_2;
			Action<PatrolPointContainer> action2;
			do
			{
				action2 = action;
				Action<PatrolPointContainer> value2 = (Action<PatrolPointContainer>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_2, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<PatrolPointContainer> action = action_2;
			Action<PatrolPointContainer> action2;
			do
			{
				action2 = action;
				Action<PatrolPointContainer> value2 = (Action<PatrolPointContainer>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_2, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public GClass504([NotNull] BotOwner owner)
		: base(owner)
	{
	}

	public void SetPointSetter(Action<PatrolPointContainer, int> pointSetted)
	{
		Action_1 = pointSetted;
	}

	public void SetWayChanged(Action wayChanged)
	{
		Action_0 = wayChanged;
	}

	public PatrolPointContainer GetPreferableNext()
	{
		if (_reservedPatrolPoint != null && _reservedPatrolPoint.TargetPoint.IsFreeFor(BotOwner_0))
		{
			PatrolPointContainer reservedPatrolPoint = _reservedPatrolPoint;
			_reservedPatrolPoint = null;
			return reservedPatrolPoint;
		}
		return null;
	}

	public void SetWay(PatrolWay way, GDelegate5 findPointAction, Action<BotOwner, PatrolWay> findPointForFollower = null)
	{
		if (!(way == null))
		{
			if (Way != null)
			{
				Way.RemoveMe(BotOwner_0);
			}
			Way = way;
			Way.AddUser(BotOwner_0);
			SetTarget(findPointAction(withSetting: true, withoutNext: true, -1, canCut: false));
			Action_0();
		}
	}

	public void SetTarget(PatrolPointContainer point, int index = -1)
	{
		if (PatrolPoint != null && point != null && PatrolPoint.TargetPoint != point.TargetPoint)
		{
			method_1(null);
		}
		if (point != null)
		{
			method_0(point, index);
			Action_1(PatrolPoint, index);
		}
	}

	public void SetReservedPatrolPoint(PatrolPointContainer p, int index = -1)
	{
		if (index >= 0)
		{
			PatrolPoint subPoint = p.TargetPoint.GetSubPoint(index);
			_reservedPatrolPoint = new PatrolPointContainer(subPoint);
		}
		else
		{
			_reservedPatrolPoint = p;
		}
	}

	public void method_0(PatrolPointContainer p, int index = -1)
	{
		PatrolPoint = p;
		if (PatrolPoint != null)
		{
			if (index >= 0)
			{
				PatrolPoint = new PatrolPointContainer(p.TargetPoint.GetSubPoint(index));
				method_1(PatrolPoint.TargetPoint);
			}
			else
			{
				method_1(PatrolPoint.TargetPoint);
			}
		}
	}

	public void method_1(PatrolPoint point)
	{
		if (PatrolPoint_0 != null)
		{
			PatrolPoint_0.SetOwner(null);
		}
		if (point != null)
		{
			point.SetOwner(BotOwner_0);
		}
		PatrolPoint_0 = point;
		action_2?.Invoke(PatrolPoint);
	}

	public void method_2()
	{
		int ownerId = PatrolPoint.TargetPoint.GetOwnerId();
		if (ownerId > 0)
		{
			_ = BotOwner_0.Id;
		}
	}

	public void Dispose()
	{
		method_1(null);
	}
}
