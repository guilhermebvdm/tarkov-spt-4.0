using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Comfort.Common;
using EFT;
using UnityEngine;

public class GClass620 : Singleton<GClass620>
{
	[CompilerGenerated]
	public class Class289
	{
		public WildSpawnType role;
	}

	[CompilerGenerated]
	public class Class290
	{
		public BotDifficulty difficulty;

		public Class289 class289_0;

		public bool method_0(BotPresetClass x)
		{
			if (x.BotDifficulty == difficulty)
			{
				return x.Role == class289_0.role;
			}
			return false;
		}
	}

	public BotLocationModifier BotLocationModifier;

	public BotCurvSettings _CurvSettings;

	[NonSerialized]
	public GClass624<BotDifficulty, WildSpawnType, BotDifficultySettingsClass> Gclass624_0 = new GClass624<BotDifficulty, WildSpawnType, BotDifficultySettingsClass>();

	public void SetSettings(BotPresetClass[] botPresets, GClass612[] botScatterings, BotLocationModifier botLocationModifier, bool isPve)
	{
		if (botLocationModifier == null)
		{
			BotLocationModifier = new BotLocationModifier();
		}
		else
		{
			BotLocationModifier = botLocationModifier;
		}
		WildSpawnType[] allTypes = BotsController.AllTypes;
		foreach (WildSpawnType role in allTypes)
		{
			foreach (BotDifficulty difficulty in Enum.GetValues(typeof(BotDifficulty)))
			{
				BotPresetClass botPresetClass = null;
				if (botPresets != null)
				{
					botPresetClass = botPresets.FirstOrDefault((BotPresetClass x) => x.BotDifficulty == difficulty && x.Role == role);
				}
				if (!(BotCurvSettings.Instance == null))
				{
					BotCurvSettings curv = BotCurvSettings.Instance.Copy();
					GClass612[] array;
					if (botScatterings != null)
					{
						array = new GClass612[botScatterings.Length];
						for (int num = 0; num < botScatterings.Length; num++)
						{
							GClass612 gClass = new GClass612(botScatterings[num], botPresetClass);
							array[num] = gClass;
						}
					}
					else
					{
						array = new GClass612[0];
					}
					BotDifficultySettingsClass botDifficultySettingsClass = new BotDifficultySettingsClass(difficulty, role, curv, array, isPve);
					if (botPresetClass != null)
					{
						botDifficultySettingsClass.ApplyPreset(botPresetClass);
					}
					botDifficultySettingsClass.ApplyPresetLocation(BotLocationModifier);
					Gclass624_0.Add(difficulty, role, botDifficultySettingsClass);
					continue;
				}
				Debug.LogError("But curv settings is null!");
				return;
			}
		}
	}

	public BotDifficultySettingsClass GetSettings(BotDifficulty difficulty, WildSpawnType role, bool isPve)
	{
		if (Gclass624_0 != null && Gclass624_0.ContainsKey(difficulty, role))
		{
			BotDifficulty primary = LocalBotSettingsProviderClass.CheckOnExclude(difficulty, role);
			return Gclass624_0.Get(primary, role).Copy();
		}
		return new BotDifficultySettingsClass(difficulty, role, _CurvSettings, null, isPve);
	}
}
