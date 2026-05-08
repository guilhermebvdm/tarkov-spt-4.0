using EFT;

public class GClass1834 : GClass1833
{
	public override EBonusType BonusType => EBonusType.MaximumEnergyReserve;

	public override string ValueFormat => GClass2348.Localized(base.ValueFormat + " point(s)");

	public GClass1834(ProfileBonusesClass descriptor)
		: base(descriptor)
	{
	}
}
