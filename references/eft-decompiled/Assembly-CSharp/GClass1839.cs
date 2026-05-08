public abstract class GClass1839 : SkillBonusAbstractClass
{
	public override string ValueFormat => string.Empty;

	public GClass1839(ProfileBonusesClass descriptor)
		: base(descriptor)
	{
	}

	public override double CalculateValue(double input)
	{
		return (ResultValue > 0.0) ? 1 : 0;
	}
}
