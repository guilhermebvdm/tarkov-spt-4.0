using System;
using EFT;

public class GClass1827 : GClass1817
{
	public readonly ESkillClass SkillType;

	public override Enum TargetType => SkillType;

	public override EBonusType BonusType => EBonusType.SkillGroupLevelingBoost;

	public override string ValueFormat => GClass2348.Localized(SkillType.ToString()) + ", " + base.ValueFormat;

	public GClass1827(ProfileBonusesClass descriptor)
		: base(descriptor)
	{
		SkillType = descriptor.SkillType.Value;
	}

	public override ProfileBonusesClass ToDescriptor()
	{
		ProfileBonusesClass profileBonusesClass = base.ToDescriptor();
		profileBonusesClass.SkillType = SkillType;
		return profileBonusesClass;
	}
}
