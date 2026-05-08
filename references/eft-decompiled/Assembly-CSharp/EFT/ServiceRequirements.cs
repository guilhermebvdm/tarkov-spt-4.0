using System;
using Newtonsoft.Json;

namespace EFT;

[Serializable]
public class ServiceRequirements
{
	[JsonProperty("CompletedQuests")]
	[field: NonSerialized]
	public global::QuestDictionaryClass<string> CompletedQuests { get; set; }

	[JsonProperty("Standings")]
	[field: NonSerialized]
	public GClass2638<float> Standings { get; set; }

	[JsonProperty("Side")]
	[field: NonSerialized]
	public ESideType SideType { get; set; }
}
