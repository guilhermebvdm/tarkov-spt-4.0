using EFT;

public class GClass561 : PatrolPointChooserBasic
{
	public GClass561(BotOwner owner)
		: base(owner)
	{
		Owner = owner;
		CanChangeWays = Owner.BotsGroup.BotZone.PatrolWays.Length > 1;
	}

	public override PatrolPointContainer FindNextPoint(bool withSetting, bool withoutNext, int minSubTargets = -1, bool canCut = true, GDelegate4 pointFilter = null)
	{
		if (Owner.PatrollingData.PointControl.PatrolPoint != null)
		{
			return Owner.PatrollingData.PointControl.PatrolPoint;
		}
		PatrolWay way = Owner.BotFollower.BossToFollow.PatrollingData.Way;
		Owner.PatrollingData.PointControl.SetTarget(Owner.BotFollower.BossToFollow.PatrollingData.PointChooser.FindPointForFollower(Owner, way));
		return Owner.PatrollingData.PointControl.PatrolPoint;
	}

	public override bool TryToFindWay(out PatrolWay way, out float delta)
	{
		way = null;
		delta = 0f;
		return false;
	}
}
