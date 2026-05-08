using System.Runtime.Serialization;
using EFT;
using JetBrains.Annotations;
using Newtonsoft.Json;

public class GroupPlayerDataClass : GInterface214
{
	[JsonProperty("aid")]
	public string AccountId = string.Empty;

	[JsonProperty("_id")]
	public string Id = string.Empty;

	[JsonProperty("lookingGroup")]
	public bool LookingForGroup;

	[JsonProperty("IsLeader")]
	public bool IsLeader;

	[JsonProperty("IsReady")]
	public bool IsReady;

	[JsonProperty("Info")]
	public GClass1410 Info;

	[JsonProperty("PlayerVisualRepresentation")]
	[CanBeNull]
	public GClass2214 Gclass2214_0;

	[JsonIgnore]
	[CanBeNull]
	public LastPlayerStateClass PlayerVisualRepresentation;

	string GInterface214.ProfileId => Id;

	string GInterface214.Nickname => Info.Nickname;

	EPlayerSide GInterface214.Side => Info.Side;

	int GInterface214.PrestigeLevel => Info.PrestigeLevel;

	[OnSerializing]
	public void method_0(StreamingContext context)
	{
		if (PlayerVisualRepresentation != null)
		{
			Gclass2214_0 = new GClass2214(PlayerVisualRepresentation);
		}
	}

	[OnDeserialized]
	public void method_1(StreamingContext context)
	{
		if (Gclass2214_0 != null)
		{
			PlayerVisualRepresentation = new LastPlayerStateClass(Gclass2214_0);
		}
	}
}
