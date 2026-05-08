public class GClass790
{
	public bool IsBoss;

	public bool IsFollower;

	public bool IsHostileToEverybody;

	public bool? CountAsBossForStatistics;

	public string ScavRoleKey;

	public ETagStatus PhraseTag;

	public GClass790(bool isBoss, bool isFollower, bool isHostileToEverybody, string scavRoleKey, ETagStatus phraseTag = (ETagStatus)0)
	{
		IsBoss = isBoss;
		IsFollower = isFollower;
		IsHostileToEverybody = isHostileToEverybody;
		ScavRoleKey = scavRoleKey;
		CountAsBossForStatistics = null;
		PhraseTag = phraseTag;
	}
}
