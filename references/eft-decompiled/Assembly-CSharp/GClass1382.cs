using System;
using EFT;
using Newtonsoft.Json;

public class GClass1382
{
	[JsonProperty("profile")]
	public CompleteProfileDescriptorClass CompleteProfileDescriptorClass;

	[NonSerialized]
	public Profile Profile_0;

	[JsonProperty("profileToken")]
	public string ProfileToken;

	[JsonProperty("group")]
	public string GroupId;

	[JsonProperty("team")]
	public string TeamId;

	[JsonProperty("entryPoint")]
	public string EntryPoint;

	[JsonProperty("keyId")]
	public string KeyId;

	[JsonProperty("repeatableQuests")]
	public RawQuestClass[] RepeatableQuests;

	[JsonProperty("transitionType")]
	public ELocationTransition transitionType;

	[JsonProperty("transitionRaidId")]
	public string transitionRaidId;

	[JsonProperty("transitionCount")]
	public int transitionCount;

	[JsonProperty("visitedLocations")]
	public string[] visitedLocations;

	[JsonProperty("karmaValue")]
	public float KarmaValue;

	[JsonIgnore]
	public Profile Profile => Profile_0 ?? (Profile_0 = new Profile(CompleteProfileDescriptorClass));
}
