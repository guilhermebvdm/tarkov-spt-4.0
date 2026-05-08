using EFT;

public class GClass1835 : GClass1833, GInterface172
{
	public override EBonusType BonusType => EBonusType.StashRows;

	public EAreaType AreaType => EAreaType.Stash;

	public GClass1835(ProfileBonusesClass descriptor)
		: base(descriptor)
	{
	}
}
