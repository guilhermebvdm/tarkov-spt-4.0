using System;
using Comfort.Common;
using EFT;
using Newtonsoft.Json;

public class GClass1816 : GClass1815
{
	[JsonProperty("templateId")]
	public MongoID TemplateId;

	[NonSerialized]
	[JsonIgnore]
	public StashItemClass StashItemClass_1;

	public override EBonusType BonusType => EBonusType.StashSize;

	public override string ValueFormat => StashItemClass.Grid.GridWidth + "x" + StashItemClass.Grid.GridHeight;

	[JsonIgnore]
	public StashItemClass StashItemClass => StashItemClass_1 ?? (StashItemClass_1 = Singleton<ItemFactoryClass>.Instance.CreateItem(MongoID.Generate(), TemplateId, null) as StashItemClass);

	public GClass1816(ProfileBonusesClass descriptor)
		: base(descriptor)
	{
		TemplateId = descriptor.TemplateId.Value;
	}

	public override void Add(SkillBonusAbstractClass bonus)
	{
		TemplateId = ((GClass1816)bonus).TemplateId;
		base.Add(bonus);
	}

	public override ProfileBonusesClass ToDescriptor()
	{
		ProfileBonusesClass profileBonusesClass = base.ToDescriptor();
		profileBonusesClass.TemplateId = TemplateId;
		return profileBonusesClass;
	}
}
