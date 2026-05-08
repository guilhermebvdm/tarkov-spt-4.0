using EFT;

public class GClass1825 : GClass1817
{
	public override EBonusType BonusType => EBonusType.ScavCooldownTimer;

	public override bool IsPositive => !base.IsPositive;

	public GClass1825(ProfileBonusesClass descriptor)
		: base(descriptor)
	{
	}
}
