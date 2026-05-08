using System;
using EFT;
using UnityEngine;

public class GClass621
{
	public static void TestMerge()
	{
		LocalBotSettingsProviderClass.LoadByDifficulty(BotDifficulty.normal, WildSpawnType.bossKnight, external: false, isPve: true);
	}

	public static void TestClear()
	{
		GClass619.Clear(WildSpawnType.assault, BotDifficulty.normal);
	}

	public static void AllClear()
	{
		WildSpawnType[] obj = (WildSpawnType[])Enum.GetValues(typeof(WildSpawnType));
		BotDifficulty[] array = (BotDifficulty[])Enum.GetValues(typeof(BotDifficulty));
		WildSpawnType[] array2 = obj;
		foreach (WildSpawnType role in array2)
		{
			BotDifficulty[] array3 = array;
			foreach (BotDifficulty dif in array3)
			{
				GClass619.Clear(role, dif);
			}
		}
		Debug.LogError("Clear all settins finished");
	}
}
