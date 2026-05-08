using System;

[Flags]
public enum EWarnBehaviour
{
	Default = 1,
	Neutral = 2,
	Warn = 4,
	AlwaysEnemies = 8,
	AlwaysFriends = 0x10,
	ChancedEnemies = 0x20
}
