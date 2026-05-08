using System;

public abstract class GClass1817 : SkillBonusAbstractClass
{
	[NonSerialized]
	public double Double_1;

	public override string ValueFormat => base.ValueFormat + "%";

	public double BoostValue
	{
		get
		{
			return Double_1;
		}
		set
		{
			Double_1 = (IsPositive ? value : 0.0);
		}
	}

	public override double ResultValue => base.Value + base.Value * BoostValue / 100.0;

	public GClass1817(ProfileBonusesClass descriptor)
		: base(descriptor)
	{
	}

	public override double CalculateValue(double input)
	{
		return input * (1.0 + ResultValue / 100.0);
	}

	public override SkillBonusAbstractClass Clone()
	{
		GClass1817 obj = base.Clone() as GClass1817;
		obj.BoostValue = BoostValue;
		return obj;
	}
}
