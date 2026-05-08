using System;
using EFT;

public class GClass1850 : SkillBonusAbstractClass
{
	[NonSerialized]
	public Func<string> Func_0;

	public override EBonusType BonusType => EBonusType.TextBonus;

	public override string ValueFormat
	{
		get
		{
			if (Func_0 == null)
			{
				if (base.Value != 0.0)
				{
					return "{0}";
				}
				return "";
			}
			return Func_0();
		}
	}

	public override string LocalizationKey => $"hideout_{base.Id}";

	public EBonusType UniqueType
	{
		get
		{
			int num = GClass2348.Localized(LocalizationKey).GetHashCode();
			if (num >= 0)
			{
				num += GClass866<EBonusType>.Count;
			}
			return (EBonusType)num;
		}
	}

	public GClass1850(ProfileBonusesClass descriptor, Func<string> valueFormatGetter = null)
		: base(descriptor)
	{
		Func_0 = valueFormatGetter;
	}

	public override double CalculateValue(double input)
	{
		return 0.0;
	}

	public override SkillBonusAbstractClass Clone()
	{
		return new GClass1850(ToDescriptor(), Func_0);
	}

	public override void Add(SkillBonusAbstractClass bonus)
	{
		base.Value = bonus.Value;
	}
}
