using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using EFT;
using JetBrains.Annotations;
using UnityEngine;

public abstract class LocalBotSettingsProviderClass
{
	[NonSerialized]
	public const string String_0 = "Settings/{0}_{1}_BotGlobalSettings";

	[NonSerialized]
	public const string String_1 = "Settings/PvE/{0}_{1}_PvE_BotGlobalSettings";

	[NonSerialized]
	public static string String_2 = "Assets/CommonAssets/Scripts/AI/Resources/Settings/{0}_{1}_BotGlobalSettings.json";

	[NonSerialized]
	public static string String_3 = "Settings/{0}_{1}_BotGlobalSettings.json";

	[NonSerialized]
	public static string String_4 = "Settings/PvE/{0}_{1}_PvE_BotGlobalSettings.json";

	[NonSerialized]
	public static string String_5 = "Settings/{0}_{1}_BotGlobalSettings";

	[NonSerialized]
	public static string String_6 = "Settings/PvE/{0}_{1}_PvE_BotGlobalSettings";

	[NonSerialized]
	public static GClass624<BotDifficulty, WildSpawnType, BotSettingsComponents> Gclass624_0 = new GClass624<BotDifficulty, WildSpawnType, BotSettingsComponents>();

	[NonSerialized]
	public static GClass624<BotDifficulty, WildSpawnType, BotSettingsComponents> Gclass624_1 = new GClass624<BotDifficulty, WildSpawnType, BotSettingsComponents>();

	public static CoreBotSettingsClass Core = new CoreBotSettingsClass();

	[NonSerialized]
	public static Dictionary<WildSpawnType, WildSpawnType> Dictionary_0 = new Dictionary<WildSpawnType, WildSpawnType> { 
	{
		WildSpawnType.assaultGroup,
		WildSpawnType.assault
	} };

	[NonSerialized]
	public static Dictionary<WildSpawnType, List<BotDifficulty>> Dictionary_1 = new Dictionary<WildSpawnType, List<BotDifficulty>>
	{
		{
			WildSpawnType.infectedAssault,
			new List<BotDifficulty> { BotDifficulty.impossible }
		},
		{
			WildSpawnType.infectedPmc,
			new List<BotDifficulty> { BotDifficulty.impossible }
		},
		{
			WildSpawnType.infectedCivil,
			new List<BotDifficulty> { BotDifficulty.impossible }
		},
		{
			WildSpawnType.assaultGroup,
			new List<BotDifficulty> { BotDifficulty.impossible }
		},
		{
			WildSpawnType.infectedLaborant,
			new List<BotDifficulty> { BotDifficulty.impossible }
		},
		{
			WildSpawnType.pmcBEAR,
			new List<BotDifficulty> { BotDifficulty.impossible }
		},
		{
			WildSpawnType.pmcUSEC,
			new List<BotDifficulty>
			{
				BotDifficulty.easy,
				BotDifficulty.hard,
				BotDifficulty.impossible
			}
		},
		{
			WildSpawnType.exUsec,
			new List<BotDifficulty>
			{
				BotDifficulty.easy,
				BotDifficulty.hard,
				BotDifficulty.impossible
			}
		},
		{
			WildSpawnType.sectactPriestEvent,
			new List<BotDifficulty>
			{
				BotDifficulty.easy,
				BotDifficulty.hard,
				BotDifficulty.impossible
			}
		},
		{
			WildSpawnType.peacefullZryachiyEvent,
			new List<BotDifficulty>
			{
				BotDifficulty.easy,
				BotDifficulty.hard,
				BotDifficulty.impossible
			}
		},
		{
			WildSpawnType.ravangeZryachiyEvent,
			new List<BotDifficulty>
			{
				BotDifficulty.easy,
				BotDifficulty.hard,
				BotDifficulty.impossible
			}
		},
		{
			WildSpawnType.arenaFighterEvent,
			new List<BotDifficulty>
			{
				BotDifficulty.easy,
				BotDifficulty.hard,
				BotDifficulty.impossible
			}
		},
		{
			WildSpawnType.skier,
			new List<BotDifficulty>
			{
				BotDifficulty.easy,
				BotDifficulty.hard,
				BotDifficulty.impossible
			}
		},
		{
			WildSpawnType.peacemaker,
			new List<BotDifficulty>
			{
				BotDifficulty.easy,
				BotDifficulty.hard,
				BotDifficulty.impossible
			}
		},
		{
			WildSpawnType.pmcBot,
			new List<BotDifficulty>
			{
				BotDifficulty.easy,
				BotDifficulty.hard,
				BotDifficulty.impossible
			}
		},
		{
			WildSpawnType.crazyAssaultEvent,
			new List<BotDifficulty>
			{
				BotDifficulty.easy,
				BotDifficulty.hard,
				BotDifficulty.impossible
			}
		},
		{
			WildSpawnType.arenaFighter,
			new List<BotDifficulty>
			{
				BotDifficulty.easy,
				BotDifficulty.hard,
				BotDifficulty.impossible
			}
		},
		{
			WildSpawnType.followerTest,
			new List<BotDifficulty>
			{
				BotDifficulty.easy,
				BotDifficulty.hard,
				BotDifficulty.impossible
			}
		},
		{
			WildSpawnType.bossTest,
			new List<BotDifficulty>
			{
				BotDifficulty.easy,
				BotDifficulty.hard,
				BotDifficulty.impossible
			}
		},
		{
			WildSpawnType.bossKilla,
			new List<BotDifficulty>
			{
				BotDifficulty.easy,
				BotDifficulty.hard,
				BotDifficulty.impossible
			}
		},
		{
			WildSpawnType.bossBully,
			new List<BotDifficulty>
			{
				BotDifficulty.easy,
				BotDifficulty.hard,
				BotDifficulty.impossible
			}
		},
		{
			WildSpawnType.followerBully,
			new List<BotDifficulty>
			{
				BotDifficulty.easy,
				BotDifficulty.hard,
				BotDifficulty.impossible
			}
		},
		{
			WildSpawnType.bossKojaniy,
			new List<BotDifficulty>
			{
				BotDifficulty.easy,
				BotDifficulty.hard,
				BotDifficulty.impossible
			}
		},
		{
			WildSpawnType.bossZryachiy,
			new List<BotDifficulty>
			{
				BotDifficulty.easy,
				BotDifficulty.hard,
				BotDifficulty.impossible
			}
		},
		{
			WildSpawnType.followerZryachiy,
			new List<BotDifficulty>
			{
				BotDifficulty.easy,
				BotDifficulty.hard,
				BotDifficulty.impossible
			}
		},
		{
			WildSpawnType.followerKojaniy,
			new List<BotDifficulty>
			{
				BotDifficulty.easy,
				BotDifficulty.hard,
				BotDifficulty.impossible
			}
		},
		{
			WildSpawnType.bossGluhar,
			new List<BotDifficulty>
			{
				BotDifficulty.easy,
				BotDifficulty.hard,
				BotDifficulty.impossible
			}
		},
		{
			WildSpawnType.followerKolontaySecurity,
			new List<BotDifficulty>
			{
				BotDifficulty.easy,
				BotDifficulty.hard,
				BotDifficulty.impossible
			}
		},
		{
			WildSpawnType.followerKolontayAssault,
			new List<BotDifficulty>
			{
				BotDifficulty.easy,
				BotDifficulty.hard,
				BotDifficulty.impossible
			}
		},
		{
			WildSpawnType.followerGluharAssault,
			new List<BotDifficulty>
			{
				BotDifficulty.easy,
				BotDifficulty.hard,
				BotDifficulty.impossible
			}
		},
		{
			WildSpawnType.followerGluharScout,
			new List<BotDifficulty>
			{
				BotDifficulty.easy,
				BotDifficulty.hard,
				BotDifficulty.impossible
			}
		},
		{
			WildSpawnType.followerGluharSecurity,
			new List<BotDifficulty>
			{
				BotDifficulty.easy,
				BotDifficulty.hard,
				BotDifficulty.impossible
			}
		},
		{
			WildSpawnType.followerGluharSnipe,
			new List<BotDifficulty>
			{
				BotDifficulty.easy,
				BotDifficulty.hard,
				BotDifficulty.impossible
			}
		},
		{
			WildSpawnType.test,
			new List<BotDifficulty>
			{
				BotDifficulty.easy,
				BotDifficulty.hard,
				BotDifficulty.impossible
			}
		},
		{
			WildSpawnType.spiritWinter,
			new List<BotDifficulty>
			{
				BotDifficulty.easy,
				BotDifficulty.hard,
				BotDifficulty.impossible
			}
		},
		{
			WildSpawnType.spiritSpring,
			new List<BotDifficulty>
			{
				BotDifficulty.easy,
				BotDifficulty.hard,
				BotDifficulty.impossible
			}
		},
		{
			WildSpawnType.bossSanitar,
			new List<BotDifficulty>
			{
				BotDifficulty.easy,
				BotDifficulty.hard,
				BotDifficulty.impossible
			}
		},
		{
			WildSpawnType.followerSanitar,
			new List<BotDifficulty>
			{
				BotDifficulty.easy,
				BotDifficulty.hard,
				BotDifficulty.impossible
			}
		},
		{
			WildSpawnType.sectantPrizrak,
			new List<BotDifficulty>
			{
				BotDifficulty.easy,
				BotDifficulty.hard,
				BotDifficulty.impossible
			}
		},
		{
			WildSpawnType.sectantPredvestnik,
			new List<BotDifficulty>
			{
				BotDifficulty.easy,
				BotDifficulty.hard,
				BotDifficulty.impossible
			}
		},
		{
			WildSpawnType.sectantOni,
			new List<BotDifficulty>
			{
				BotDifficulty.easy,
				BotDifficulty.hard,
				BotDifficulty.impossible
			}
		},
		{
			WildSpawnType.sectantPriest,
			new List<BotDifficulty>
			{
				BotDifficulty.easy,
				BotDifficulty.hard,
				BotDifficulty.impossible
			}
		},
		{
			WildSpawnType.sectantWarrior,
			new List<BotDifficulty>
			{
				BotDifficulty.easy,
				BotDifficulty.hard,
				BotDifficulty.impossible
			}
		},
		{
			WildSpawnType.bossTagilla,
			new List<BotDifficulty>
			{
				BotDifficulty.easy,
				BotDifficulty.hard,
				BotDifficulty.impossible
			}
		},
		{
			WildSpawnType.infectedTagilla,
			new List<BotDifficulty>
			{
				BotDifficulty.easy,
				BotDifficulty.hard,
				BotDifficulty.impossible
			}
		},
		{
			WildSpawnType.bossTagillaAgro,
			new List<BotDifficulty>
			{
				BotDifficulty.easy,
				BotDifficulty.hard,
				BotDifficulty.impossible
			}
		},
		{
			WildSpawnType.tagillaHelperAgro,
			new List<BotDifficulty>
			{
				BotDifficulty.easy,
				BotDifficulty.hard,
				BotDifficulty.impossible
			}
		},
		{
			WildSpawnType.bossKillaAgro,
			new List<BotDifficulty>
			{
				BotDifficulty.easy,
				BotDifficulty.hard,
				BotDifficulty.impossible
			}
		},
		{
			WildSpawnType.followerTagilla,
			new List<BotDifficulty>
			{
				BotDifficulty.easy,
				BotDifficulty.hard,
				BotDifficulty.impossible
			}
		},
		{
			WildSpawnType.gifter,
			new List<BotDifficulty>
			{
				BotDifficulty.easy,
				BotDifficulty.hard,
				BotDifficulty.impossible
			}
		},
		{
			WildSpawnType.bossKnight,
			new List<BotDifficulty>
			{
				BotDifficulty.easy,
				BotDifficulty.hard,
				BotDifficulty.impossible
			}
		},
		{
			WildSpawnType.bossPartisan,
			new List<BotDifficulty>
			{
				BotDifficulty.easy,
				BotDifficulty.hard,
				BotDifficulty.impossible
			}
		},
		{
			WildSpawnType.followerBoar,
			new List<BotDifficulty>
			{
				BotDifficulty.easy,
				BotDifficulty.hard,
				BotDifficulty.impossible
			}
		},
		{
			WildSpawnType.followerBoarClose1,
			new List<BotDifficulty>
			{
				BotDifficulty.easy,
				BotDifficulty.hard,
				BotDifficulty.impossible
			}
		},
		{
			WildSpawnType.followerBoarClose2,
			new List<BotDifficulty>
			{
				BotDifficulty.easy,
				BotDifficulty.hard,
				BotDifficulty.impossible
			}
		},
		{
			WildSpawnType.bossBoar,
			new List<BotDifficulty>
			{
				BotDifficulty.easy,
				BotDifficulty.hard,
				BotDifficulty.impossible
			}
		},
		{
			WildSpawnType.bossKolontay,
			new List<BotDifficulty>
			{
				BotDifficulty.easy,
				BotDifficulty.hard,
				BotDifficulty.impossible
			}
		},
		{
			WildSpawnType.bossBoarSniper,
			new List<BotDifficulty>
			{
				BotDifficulty.easy,
				BotDifficulty.hard,
				BotDifficulty.impossible
			}
		},
		{
			WildSpawnType.followerBigPipe,
			new List<BotDifficulty>
			{
				BotDifficulty.easy,
				BotDifficulty.hard,
				BotDifficulty.impossible
			}
		},
		{
			WildSpawnType.followerBirdEye,
			new List<BotDifficulty>
			{
				BotDifficulty.easy,
				BotDifficulty.hard,
				BotDifficulty.impossible
			}
		},
		{
			WildSpawnType.shooterBTR,
			new List<BotDifficulty>
			{
				BotDifficulty.easy,
				BotDifficulty.hard,
				BotDifficulty.impossible
			}
		}
	};

	[NonSerialized]
	public static bool Bool_0;

	public static WildSpawnType[] WildSpawnType_0 => (WildSpawnType[])Enum.GetValues(typeof(WildSpawnType));

	public static string smethod_0(bool isPve)
	{
		if (!isPve)
		{
			return String_3;
		}
		return String_4;
	}

	public static string smethod_1(bool isPve)
	{
		if (!isPve)
		{
			return String_5;
		}
		return String_6;
	}

	public static GClass624<BotDifficulty, WildSpawnType, BotSettingsComponents> smethod_2(bool isPve)
	{
		if (!isPve)
		{
			return Gclass624_0;
		}
		return Gclass624_1;
	}

	public static BotDifficulty CheckOnExclude(BotDifficulty d, WildSpawnType wst)
	{
		if (Dictionary_1.TryGetValue(wst, out var value) && value.Contains(d))
		{
			return BotDifficulty.normal;
		}
		return d;
	}

	public static BotSettingsComponents GetSettings(BotDifficulty difficulty, WildSpawnType role, bool isPve)
	{
		if (!Bool_0)
		{
			Load();
		}
		try
		{
			return smethod_2(isPve).Get(CheckOnExclude(difficulty, role), role).Copy();
		}
		catch (Exception)
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (GClass623<BotDifficulty, WildSpawnType, BotSettingsComponents> allVal in smethod_2(isPve).GetAllVals())
			{
				stringBuilder.Append(" " + allVal.PrimaryKey.ToString() + "  " + allVal.SecondaryKey);
			}
			throw;
		}
	}

	public static void Save(GClass624<BotDifficulty, WildSpawnType, BotSettingsComponents> internalSettings)
	{
		foreach (GClass623<BotDifficulty, WildSpawnType, BotSettingsComponents> allVal in internalSettings.GetAllVals())
		{
			smethod_4(allVal.PrimaryKey, allVal.SecondaryKey, allVal.Value);
		}
	}

	public static void Save(BotDifficulty difficulty, WildSpawnType botRole, BotSettingsComponents internalSettings, Func<string, BotDifficulty, string> jsonPostprocessor = null)
	{
		smethod_4(difficulty, botRole, internalSettings, jsonPostprocessor);
	}

	public static string LoadCoreByString()
	{
		TextAsset textAsset = GClass861.Load<TextAsset>(string.Format(String_5, "", ""));
		if (textAsset == null)
		{
			return null;
		}
		return textAsset.text;
	}

	[CanBeNull]
	public static string LoadDifficultyStringInternal(BotDifficulty botDifficulty, WildSpawnType role, bool isPve)
	{
		TextAsset textAsset = GClass861.Load<TextAsset>(string.Format(smethod_1(isPve), botDifficulty.ToString(), role.ToString()));
		if (textAsset == null)
		{
			return null;
		}
		return textAsset.text;
	}

	public static void SaveDifficultyStringInternal(BotDifficulty botDifficulty, WildSpawnType role, bool isPve, string data)
	{
		string text = string.Format(smethod_1(isPve), botDifficulty.ToString(), role.ToString());
		string path = Application.dataPath + "/CommonAssets/Scripts/AI/Resources/" + text + ".json";
		string directoryName = Path.GetDirectoryName(path);
		if (!Directory.Exists(directoryName))
		{
			Directory.CreateDirectory(directoryName);
		}
		if (File.Exists(path))
		{
			File.Delete(path);
		}
		File.WriteAllText(path, data);
	}

	public static string LoadInternalCoreByString()
	{
		if (File.Exists(String_3))
		{
			return File.ReadAllText(String_3);
		}
		return null;
	}

	public static bool LoadInternal(out CoreBotSettingsClass core)
	{
		string text = LoadCoreByString();
		if (text == null)
		{
			core = null;
			return false;
		}
		core = CoreBotSettingsClass.Create(text);
		if (GClass861.Load<TextAsset>(string.Format(String_5, "", "")) != null)
		{
			foreach (WildSpawnType value in Enum.GetValues(typeof(WildSpawnType)))
			{
				foreach (BotDifficulty value2 in Enum.GetValues(typeof(BotDifficulty)))
				{
					BotDifficulty d = CheckOnExclude(value2, value);
					BotSettingsComponents botSettingsComponents = LoadByDifficulty(d, value, external: false, isPve: false);
					if (botSettingsComponents != null)
					{
						if (!Gclass624_0.ContainsKey(value2, value))
						{
							Gclass624_0.Add(value2, value, botSettingsComponents);
						}
						BotSettingsComponents botSettingsComponents2 = LoadByDifficulty(d, value, external: false, isPve: true);
						if (botSettingsComponents2 != null)
						{
							if (!Gclass624_1.ContainsKey(value2, value))
							{
								Gclass624_1.Add(value2, value, botSettingsComponents2);
							}
						}
						else if (!Gclass624_1.ContainsKey(value2, value))
						{
							Gclass624_1.Add(value2, value, botSettingsComponents);
						}
						continue;
					}
					return false;
				}
			}
			Debug.Log("Internal bot settings load");
			return true;
		}
		return false;
	}

	public static bool LoadExternal()
	{
		try
		{
			string path = string.Format(String_3, "", "");
			if (!File.Exists(path))
			{
				return false;
			}
			Core = CoreBotSettingsClass.Create(File.ReadAllText(path));
			foreach (BotDifficulty value in Enum.GetValues(typeof(BotDifficulty)))
			{
				WildSpawnType[] wildSpawnType_ = WildSpawnType_0;
				foreach (WildSpawnType wildSpawnType in wildSpawnType_)
				{
					BotDifficulty d = CheckOnExclude(value, wildSpawnType);
					BotSettingsComponents botSettingsComponents = LoadByDifficulty(d, wildSpawnType, external: true, isPve: false);
					if (botSettingsComponents != null)
					{
						if (!Gclass624_0.ContainsKey(value, wildSpawnType))
						{
							Gclass624_0.Add(value, wildSpawnType, botSettingsComponents);
						}
						BotSettingsComponents botSettingsComponents2 = LoadByDifficulty(d, wildSpawnType, external: true, isPve: true);
						if (botSettingsComponents2 != null)
						{
							if (!Gclass624_1.ContainsKey(value, wildSpawnType))
							{
								Gclass624_1.Add(value, wildSpawnType, botSettingsComponents2);
							}
						}
						else if (!Gclass624_1.ContainsKey(value, wildSpawnType))
						{
							Gclass624_1.Add(value, wildSpawnType, botSettingsComponents);
						}
						continue;
					}
					return false;
				}
			}
			return true;
		}
		catch (Exception ex)
		{
			Debug.LogError("Can't load external settings. ex:" + ex);
			return false;
		}
	}

	public static void Load()
	{
		if (Bool_0)
		{
			return;
		}
		bool flag = false;
		bool flag2 = false;
		try
		{
			if (flag = LoadExternal())
			{
				Debug.Log("External bot settings load");
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("can't load external bots global settings ex:" + ex.StackTrace);
		}
		if (!flag)
		{
			flag2 = LoadInternal(out Core);
		}
		if (!flag2 && !flag)
		{
			Debug.Log("Code bot settings load");
		}
		Bool_0 = true;
	}

	public static void Save(bool codeSettings, bool isPve)
	{
		string path = string.Format(String_2, "", "");
		LoadInternal(out var core);
		if (codeSettings)
		{
			smethod_6(path, JsonParserClass.ToPrettyJson(Core));
		}
		else
		{
			smethod_6(path, JsonParserClass.ToPrettyJson(core));
		}
		foreach (BotDifficulty value in Enum.GetValues(typeof(BotDifficulty)))
		{
			WildSpawnType[] wildSpawnType_ = WildSpawnType_0;
			foreach (WildSpawnType role in wildSpawnType_)
			{
				smethod_5(value, role, codeSettings, isPve);
			}
		}
	}

	public static WildSpawnType smethod_3(WildSpawnType role)
	{
		if (!Dictionary_0.TryGetValue(role, out var value))
		{
			return role;
		}
		return value;
	}

	public static BotSettingsComponents LoadByDifficulty(BotDifficulty d, WildSpawnType role, bool external, bool isPve)
	{
		try
		{
			string pve = null;
			WildSpawnType role2 = smethod_3(role);
			string pvp;
			if (external)
			{
				string path = string.Format(smethod_0(isPve: false), d.ToString(), role2.ToString());
				if (isPve)
				{
					string path2 = string.Format(smethod_0(isPve: true), d.ToString(), role2.ToString());
					if (File.Exists(path2))
					{
						pve = File.ReadAllText(path2);
					}
				}
				if (!File.Exists(path))
				{
					return null;
				}
				pvp = File.ReadAllText(path);
			}
			else
			{
				string text = LoadDifficultyStringInternal(d, role2, isPve: false);
				if (text == null)
				{
					return null;
				}
				if (isPve)
				{
					string text2 = LoadDifficultyStringInternal(d, role2, isPve: true);
					if (text2 != null)
					{
						pve = text2;
					}
				}
				pvp = text;
			}
			return BotSettingsComponents.Create(pvp, pve, d, role);
		}
		catch (Exception ex)
		{
			Debug.LogError("Load AI settings error: BotDifficulty" + d.ToString() + "  role:" + role);
			throw ex;
		}
	}

	public static void smethod_4(BotDifficulty botDifficulty, WildSpawnType role, BotSettingsComponents settings, Func<string, BotDifficulty, string> jsonPostprocessor = null)
	{
		if (CheckOnExclude(botDifficulty, role) == botDifficulty)
		{
			string path = string.Format(String_2, botDifficulty.ToString(), role.ToString());
			string text = JsonParserClass.ToPrettyJson(settings);
			if (jsonPostprocessor != null)
			{
				text = jsonPostprocessor(text, botDifficulty);
			}
			smethod_6(path, text);
		}
	}

	public static void smethod_5(BotDifficulty botDifficulty, WildSpawnType role, bool dropSettings, bool isPve)
	{
		botDifficulty = CheckOnExclude(botDifficulty, role);
		role = smethod_3(role);
		BotSettingsComponents botSettingsComponents = ((!smethod_2(isPve).ContainsKey(botDifficulty, role)) ? new BotSettingsComponents() : smethod_2(isPve).Get(CheckOnExclude(botDifficulty, role), role));
		if (dropSettings)
		{
			botSettingsComponents.Lay = new BotGlobalLayData();
			botSettingsComponents.Aiming = new BotGlobalAimingSettings();
			botSettingsComponents.Look = new BotGlobalLookData();
			botSettingsComponents.Shoot = new BotGlobalShootData();
			botSettingsComponents.Move = new BotGlobalsMoveSettings();
			botSettingsComponents.Grenade = new BotGlobalsGrenadeSettings();
			botSettingsComponents.Change = new BotGlobalsChangeSettings();
			botSettingsComponents.Cover = new BotGlobalsCoverSettings();
			botSettingsComponents.Patrol = new BotGlobalPatrolSettings();
			botSettingsComponents.Hearing = new BotGlobasHearingSettings();
			botSettingsComponents.Mind = new BotGlobalsMindSettings();
			botSettingsComponents.Boss = new BotGlobalsBossSettings();
			botSettingsComponents.Core = new BotGlobalsCoreSettingsClass();
			botSettingsComponents.Scattering = new BotGlobalsScatteringSettings();
		}
		smethod_4(botDifficulty, role, botSettingsComponents);
	}

	public static void smethod_6(string path, string data)
	{
		if (File.Exists(path))
		{
			File.Delete(path);
		}
		File.Create(path).Dispose();
		StreamWriter streamWriter = new StreamWriter(path);
		streamWriter.Write(data);
		streamWriter.Flush();
		streamWriter.Close();
	}
}
