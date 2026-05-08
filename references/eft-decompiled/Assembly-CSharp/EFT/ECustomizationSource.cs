namespace EFT;

public enum ECustomizationSource
{
	None,
	[GAttribute24("quest")]
	Quest,
	[GAttribute24("prestige")]
	Prestige,
	[GAttribute24("achievement")]
	Achievement,
	[GAttribute24("unlockedInGame")]
	UnlockedInGame,
	[GAttribute24("paid")]
	Paid,
	[GAttribute24("drop")]
	Drops,
	[GAttribute24("default")]
	Default,
	[GAttribute24("arenaBattlePass")]
	ArenaBattlePass
}
