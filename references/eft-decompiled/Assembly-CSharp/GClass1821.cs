using EFT;

public class GClass1821 : GClass1818
{
	public override EBonusType BonusType => EBonusType.HealthRegeneration;

	public override string BaseValueFormat => "{0} hp/hr ({1} hp/hr in total)";

	public GClass1821(ProfileBonusesClass descriptor)
		: base(descriptor)
	{
	}
}
