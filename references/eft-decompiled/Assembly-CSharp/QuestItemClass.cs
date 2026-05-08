using System.ComponentModel;
using Newtonsoft.Json;

public class QuestItemClass
{
	public string _id;

	public string _tpl;

	[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
	[DefaultValue(null)]
	public string parentId;
}
