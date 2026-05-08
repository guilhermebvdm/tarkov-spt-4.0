using EFT;

public class GClass1822 : GClass1817
{
	public override EBonusType BonusType => EBonusType.DebuffEndDelay;

	public override bool IsPositive => !base.IsPositive;

	public GClass1822(ProfileBonusesClass descriptor)
		: base(descriptor)
	{
	}
}
