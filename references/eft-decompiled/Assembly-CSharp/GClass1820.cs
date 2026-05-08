using EFT;

public class GClass1820 : GClass1818
{
	public override EBonusType BonusType => EBonusType.HydrationRegeneration;

	public override string BaseValueFormat => "{0} wp/hr ({1} wp/hr in total)";

	public GClass1820(ProfileBonusesClass descriptor)
		: base(descriptor)
	{
	}
}
