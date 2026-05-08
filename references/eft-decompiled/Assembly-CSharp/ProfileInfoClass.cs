using System.Collections.Generic;
using EFT;
using Newtonsoft.Json;

[GAttribute29]
public class ProfileInfoClass
{
	[JsonProperty("Nickname")]
	public string Nickname;

	[JsonProperty("MainProfileNickname")]
	public string MainProfileNickname;

	[JsonProperty("Side")]
	public EPlayerSide Side;

	[JsonProperty("PrestigeLevel")]
	public int PrestigeLevel;

	[JsonProperty("RegistrationDate")]
	public int RegistrationDate;

	[JsonProperty("SavageLockTime")]
	public double SavageLockTime;

	[JsonProperty("GroupId")]
	public string GroupId;

	[JsonProperty("TeamId")]
	public string TeamId;

	[JsonProperty("EntryPoint")]
	public string EntryPoint;

	[JsonProperty("NicknameChangeDate")]
	public long NicknameChangeDate;

	[JsonProperty("GameVersion")]
	public string GameVersion;

	[JsonProperty("Type")]
	public EProfileType Type;

	[JsonProperty("HasCoopExtension")]
	public bool HasCoopExtension;

	[JsonProperty("HasPveGame")]
	public bool HasPveGame;

	[JsonProperty("IsStreamerModeAvailable")]
	public bool IsStreamerModeAvailable;

	[JsonProperty("SquadInviteRestriction")]
	public bool GroupInviteRestriction;

	[JsonProperty("Bans")]
	public List<GClass2222> Bans = new List<GClass2222>();

	[JsonProperty("Settings")]
	public ProfileInfoSettingsClass Settings = new ProfileInfoSettingsClass();

	[JsonProperty("MemberCategory")]
	public EMemberCategory MemberCategory;

	[JsonProperty("SelectedMemberCategory")]
	public EMemberCategory SelectedMemberCategory;

	[JsonProperty("Experience")]
	public int Experience;

	[JsonProperty("Level")]
	public int Level;

	[JsonProperty("lockedMoveCommands")]
	public bool LockedMoveCommands;

	public ProfileInfoClass()
	{
	}

	public ProfileInfoClass(InfoClass profileInfo)
	{
		Nickname = profileInfo.Nickname;
		MainProfileNickname = profileInfo.MainProfileNickname;
		Side = profileInfo.Side;
		Level = profileInfo.Level;
		Experience = profileInfo.Experience;
		RegistrationDate = profileInfo.RegistrationDate;
		GameVersion = profileInfo.GameVersion;
		MemberCategory = profileInfo.MemberCategory;
		SelectedMemberCategory = profileInfo.SelectedMemberCategory;
		SavageLockTime = profileInfo.SavageLockTime;
		Settings = profileInfo.Settings;
		NicknameChangeDate = profileInfo.NicknameChangeDate;
		IsStreamerModeAvailable = profileInfo.IsStreamerModeAvailable;
		GroupInviteRestriction = profileInfo.GroupInviteRestriction;
		HasCoopExtension = profileInfo.HasCoopExtension;
		HasPveGame = profileInfo.HasPveGame;
		GroupId = profileInfo.GroupId;
		TeamId = profileInfo.TeamId;
		EntryPoint = profileInfo.EntryPoint;
		Type = profileInfo.Type;
		LockedMoveCommands = profileInfo.LockedMoveCommands;
		PrestigeLevel = profileInfo.PrestigeLevel;
		Bans = new List<GClass2222>();
		foreach (EBanType value in GClass866<EBanType>.Values)
		{
			VOIPBanDataClass ban = profileInfo.GetBan(value);
			if (ban != null)
			{
				Bans.Add(new GClass2222(ban));
			}
		}
	}
}
