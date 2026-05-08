public abstract class GClass1833 : SkillBonusAbstractClass
{
	public GClass1833(ProfileBonusesClass descriptor)
		: base(descriptor)
	{
	}

	public override double CalculateValue(double input)
	{
		return input + ResultValue;
	}
}
