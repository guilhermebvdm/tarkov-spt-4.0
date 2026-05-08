using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using EFT;
using Newtonsoft.Json;

public class GClass1838 : GClass1833
{
	[JsonProperty("filter")]
	public List<MongoID> Filters;

	public override EBonusType BonusType => EBonusType.AdditionalSlots;

	public int NumberOfSlots => (int)base.Value;

	public GClass1838(ProfileBonusesClass descriptor)
		: base(descriptor)
	{
		Filters = descriptor.Filters ?? new List<MongoID>();
	}

	public void Add(BackendConfigSettingsClass.GClass1754.GClass1756 eliteSlots)
	{
		base.Value += eliteSlots.Slots;
	}

	public override void Add(SkillBonusAbstractClass bonus)
	{
		base.Add(bonus);
		IEnumerable<MongoID> collection = ((GClass1838)bonus).Filters.Where((MongoID filter) => !Filters.Contains(filter));
		Filters.AddRange(collection);
	}

	public override ProfileBonusesClass ToDescriptor()
	{
		ProfileBonusesClass profileBonusesClass = base.ToDescriptor();
		profileBonusesClass.Filters = Filters.ToList();
		return profileBonusesClass;
	}

	[CompilerGenerated]
	public bool method_0(MongoID filter)
	{
		return !Filters.Contains(filter);
	}
}
