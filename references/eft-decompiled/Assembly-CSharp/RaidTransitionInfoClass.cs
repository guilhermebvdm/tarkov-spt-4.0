using EFT;

public class RaidTransitionInfoClass
{
	public ELocationTransition transitionType;

	public string transitionRaidId;

	public int transitionCount;

	public string[] visitedLocations;

	public override string ToString()
	{
		if (!GClass867.HasFlagNoBox(transitionType, ELocationTransition.Common))
		{
			return "";
		}
		return $"raidId-{transitionRaidId} transitCount-{transitionCount}";
	}
}
