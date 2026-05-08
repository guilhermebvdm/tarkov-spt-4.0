using System;
using Comfort.Common;
using Diz.Binding;
using EFT;
using Newtonsoft.Json;

[GAttribute29]
public class GClass2201 : IUpdatable<GClass2201>, GInterface214
{
	[JsonProperty("AccountId")]
	public string AccountId;

	[JsonProperty("ProfileId")]
	public string ProfileId;

	[JsonProperty("Name")]
	public string Name;

	[JsonProperty("Side")]
	public EPlayerSide Side;

	[JsonProperty("Time")]
	public TimeSpan Time;

	[JsonProperty("Level")]
	public int Level;

	[JsonProperty("PrestigeLevel")]
	public int PrestigeLevel;

	[JsonProperty("BodyPart")]
	public EBodyPart BodyPart;

	[JsonProperty("Weapon")]
	public string Weapon;

	[JsonProperty("Distance")]
	public float Distance;

	[JsonProperty("Role")]
	public WildSpawnType Role;

	[JsonProperty("Location")]
	public string Location;

	string GInterface214.ProfileId => ProfileId;

	string GInterface214.Nickname => Name;

	EPlayerSide GInterface214.Side => Side;

	int GInterface214.PrestigeLevel => PrestigeLevel;

	public GClass2201()
	{
	}

	public GClass2201(string accountId, string profileId, string name, EPlayerSide side, TimeSpan time, int level, EBodyPart part, string weapon, EDamageType damageType, float distance, WildSpawnType role)
	{
		AccountId = accountId;
		ProfileId = profileId;
		Name = name;
		Side = side;
		Time = time;
		Level = level;
		BodyPart = part;
		Weapon = weapon ?? string.Empty;
		Distance = ((damageType == EDamageType.Bullet) ? distance : 0f);
		Role = role;
		Location = Singleton<GameWorld>.Instance.LocationId;
	}

	public bool Compare(GClass2201 other)
	{
		return this == other;
	}

	public void UpdateFromAnotherItem(GClass2201 other)
	{
		AccountId = other.AccountId;
		ProfileId = other.ProfileId;
		Name = other.Name;
		Side = other.Side;
		Time = other.Time;
		Level = other.Level;
		BodyPart = other.BodyPart;
		Weapon = other.Weapon;
		Distance = other.Distance;
	}
}
