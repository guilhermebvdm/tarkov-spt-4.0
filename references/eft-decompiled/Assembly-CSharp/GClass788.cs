using EFT;
using Newtonsoft.Json;

[GAttribute29]
public class GClass788 : GInterface214
{
	[JsonProperty("AccountId")]
	public string AccountId;

	[JsonProperty("ProfileId")]
	public string ProfileId;

	[JsonProperty("MainProfileNickname")]
	public string MainProfileNickname;

	[JsonProperty("Name")]
	public string Name;

	[JsonProperty("Side")]
	public EPlayerSide Side;

	[JsonProperty("PrestigeLevel")]
	public int PrestigeLevel;

	[JsonProperty("ColliderType")]
	public EBodyPartColliderType ColliderType;

	[JsonProperty("WeaponName")]
	public string WeaponName;

	[JsonProperty("Category")]
	public EMemberCategory Category;

	[JsonProperty("Role")]
	public WildSpawnType Role;

	string GInterface214.ProfileId => ProfileId;

	string GInterface214.Nickname => Name;

	EPlayerSide GInterface214.Side => Side;

	int GInterface214.PrestigeLevel => PrestigeLevel;

	public GClass788()
	{
	}

	public GClass788(string accountId, string profileId, string name, string mainCharacterNickName, EPlayerSide side, EBodyPartColliderType colliderType, string weaponName, EMemberCategory memberCategory, WildSpawnType role, int prestigeLevel)
	{
		AccountId = accountId;
		ProfileId = profileId;
		Name = name;
		MainProfileNickname = mainCharacterNickName;
		Side = side;
		WeaponName = weaponName;
		Category = memberCategory;
		ColliderType = colliderType;
		Role = role;
		PrestigeLevel = prestigeLevel;
	}

	public override string ToString()
	{
		return string.Format("{0}={1} {2}={3} {4}={5} {6}={7} {8}={9} {10}={11} {12}={13} {14}={15}", "ProfileId", ProfileId, "Name", Name, "Side", Side, "ColliderType", ColliderType, "WeaponName", WeaponName, "Category", Category, "Role", Role, "PrestigeLevel", PrestigeLevel);
	}
}
