using ChatShared;
using EFT;
using Newtonsoft.Json;

[GAttribute29]
public class GClass1410
{
	[JsonProperty("Nickname")]
	public string Nickname;

	[JsonProperty("Side")]
	public EPlayerSide Side;

	[JsonProperty("Level")]
	public int Level;

	[JsonProperty("PrestigeLevel")]
	public int PrestigeLevel;

	[JsonProperty("MemberCategory")]
	public EMemberCategory MemberCategory;

	[JsonProperty("SelectedMemberCategory")]
	public EMemberCategory SelectedMemberCategory;

	[JsonProperty("SavageLockTime")]
	public double SavageLockTime;

	[JsonProperty("SavageNickname")]
	public string SavageNickname;

	[JsonProperty("GameVersion")]
	public string GameVersion;

	[JsonProperty("HasCoopExtension")]
	public bool HasCoopExtension;

	[JsonProperty("Health")]
	public Profile.ProfileHealthClass Health;

	public string CorrectedNickname
	{
		get
		{
			if (Side == EPlayerSide.Savage)
			{
				return GClass953.Transliterate(Nickname);
			}
			return Nickname;
		}
	}

	public static GClass1410 GetFromChatMemberInfo(UpdatableChatMember.UpdatableChatMemberInfo info)
	{
		return new GClass1410
		{
			Nickname = info.Nickname,
			Side = GClass2333.ToPlayerSide(info.Side),
			Level = info.Level,
			PrestigeLevel = info.PrestigeLevel,
			MemberCategory = info.MemberCategory,
			SelectedMemberCategory = info.SelectedMemberCategory
		};
	}
}
