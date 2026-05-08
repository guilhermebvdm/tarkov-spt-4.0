using System;
using EFT.Quests;

public class GClass4034 : ConditionProgressChecker
{
	[NonSerialized]
	public ConditionHideoutArea ConditionHideoutArea_0;

	public GClass4034(ConditionHideoutArea condition)
		: base(condition)
	{
		ConditionHideoutArea_0 = condition;
	}
}
