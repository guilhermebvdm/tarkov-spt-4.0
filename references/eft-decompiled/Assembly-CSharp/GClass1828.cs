using System;
using EFT;
using Newtonsoft.Json;

public class GClass1828 : GClass1817
{
	[JsonProperty("skillName")]
	public ESkillId SkillName;

	public override Enum TargetType => SkillName;

	public override EBonusType BonusType => EBonusType.SkillLevelingBoost;

	public override string ValueFormat => GClass2348.Localized(SkillName.ToString()) + ", " + base.ValueFormat;

	public GClass1828(ProfileBonusesClass descriptor)
		: base(descriptor)
	{
		SkillName = descriptor.SkillName.Value;
	}

	public override ProfileBonusesClass ToDescriptor()
	{
		ProfileBonusesClass profileBonusesClass = base.ToDescriptor();
		profileBonusesClass.SkillName = SkillName;
		return profileBonusesClass;
	}
}
