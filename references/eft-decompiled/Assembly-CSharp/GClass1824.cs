using EFT;

public class GClass1824 : GClass1817
{
	public override EBonusType BonusType => EBonusType.InsuranceReturnTime;

	public override bool IsPositive => !base.IsPositive;

	public GClass1824(ProfileBonusesClass descriptor)
		: base(descriptor)
	{
	}
}
