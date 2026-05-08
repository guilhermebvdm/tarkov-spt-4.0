using EFT;
using EFT.Quests;
using Newtonsoft.Json;

public class ConditionHideoutArea : Condition
{
	[JsonProperty("area")]
	public EAreaType Area;

	[JsonProperty("areaType")]
	public int _area
	{
		set
		{
			Area = (EAreaType)value;
		}
	}

	public override string FormattedDescription => string.Format(GClass2348.Localized("QuestCondition/HideoutArea"), GClass2348.LocalizeAreaName(Area), (int)base.value);
}
