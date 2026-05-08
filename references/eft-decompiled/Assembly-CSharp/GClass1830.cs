using EFT;

public class GClass1830 : GClass1817
{
	public override EBonusType BonusType => EBonusType.FuelConsumption;

	public override bool IsPositive => !base.IsPositive;

	public GClass1830(ProfileBonusesClass descriptor)
		: base(descriptor)
	{
	}
}
