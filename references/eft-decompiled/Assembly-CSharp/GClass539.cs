using System;

public class GClass539 : BotSearchPoint
{
	[NonSerialized]
	public GoalTargetClass GoalTargetClass;

	public GClass539(GoalTargetClass goalTarget)
		: base(goalTarget.GoalTarget.Position, EBotSearchPoint.mapPosition)
	{
		CustomNavigationPoint point = goalTarget.GoalTarget.Point;
		Position = (goalTarget.GoalTarget.Reacheble ? goalTarget.GoalTarget.BasePoint : (point?.Position ?? goalTarget.GoalTarget.BasePoint));
		GoalTargetClass = goalTarget;
	}

	public override void Come()
	{
		if (GoalTargetClass.HavePlaceTarget())
		{
			GoalTargetClass.GoalTarget.IsCome = true;
		}
		base.Come();
	}
}
