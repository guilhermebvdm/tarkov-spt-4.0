using EFT;

public class GClass1826 : GClass1817
{
	public override EBonusType BonusType => EBonusType.RagfairCommission;

	public override bool IsPositive => !base.IsPositive;

	public GClass1826(ProfileBonusesClass descriptor)
		: base(descriptor)
	{
	}
}
