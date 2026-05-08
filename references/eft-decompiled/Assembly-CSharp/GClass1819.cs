using EFT;

public class GClass1819 : GClass1818
{
	public override EBonusType BonusType => EBonusType.EnergyRegeneration;

	public override string BaseValueFormat => "{0} ep/hr ({1} ep/hr in total)";

	public GClass1819(ProfileBonusesClass descriptor)
		: base(descriptor)
	{
	}
}
