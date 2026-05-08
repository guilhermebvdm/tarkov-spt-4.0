using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using Comfort.Common;
using EFT;
using UnityEngine;

public class BotDifficultySettingsClass
{
	[NonSerialized]
	public BotDifficulty BotDifficulty_0;

	[NonSerialized]
	public WildSpawnType WildSpawnType_0;

	[NonSerialized]
	public List<WildSpawnType> List_0 = new List<WildSpawnType>();

	[NonSerialized]
	public List<WildSpawnType> List_1 = new List<WildSpawnType>();

	[NonSerialized]
	public List<WildSpawnType> List_2 = new List<WildSpawnType>();

	[NonSerialized]
	public List<WildSpawnType> List_3 = new List<WildSpawnType>();

	[NonSerialized]
	public List<WildSpawnType> List_4 = new List<WildSpawnType>();

	[NonSerialized]
	public List<WildSpawnType> List_5 = new List<WildSpawnType>();

	[NonSerialized]
	public List<BotsGroup> List_6 = new List<BotsGroup>();

	[NonSerialized]
	public List<BotsGroup> List_7 = new List<BotsGroup>();

	[NonSerialized]
	[CompilerGenerated]
	public BotCurvSettings BotCurvSettings_0;

	[NonSerialized]
	[CompilerGenerated]
	public GClass615 Gclass615_0;

	[NonSerialized]
	[CompilerGenerated]
	public BotSettingsComponents BotSettingsComponents_0;

	[NonSerialized]
	[CompilerGenerated]
	public ScatteringSettingsClass ScatteringSettingsClass;

	[NonSerialized]
	[CompilerGenerated]
	public bool Bool_0;

	public BotCurvSettings Curv
	{
		[CompilerGenerated]
		get
		{
			return BotCurvSettings_0;
		}
	}

	public GClass615 Current
	{
		[CompilerGenerated]
		get
		{
			return Gclass615_0;
		}
		[CompilerGenerated]
		set
		{
			Gclass615_0 = value;
		}
	}

	public BotSettingsComponents FileSettings
	{
		[CompilerGenerated]
		get
		{
			return BotSettingsComponents_0;
		}
		[CompilerGenerated]
		set
		{
			BotSettingsComponents_0 = value;
		}
	}

	public ScatteringSettingsClass CurrentScatteringSetting
	{
		[CompilerGenerated]
		get
		{
			return ScatteringSettingsClass;
		}
		[CompilerGenerated]
		set
		{
			ScatteringSettingsClass = value;
		}
	}

	public bool HaveDamaged
	{
		[CompilerGenerated]
		get
		{
			return Bool_0;
		}
		[CompilerGenerated]
		set
		{
			Bool_0 = value;
		}
	}

	public BotDifficultySettingsClass(BotDifficulty difficulty, WildSpawnType role, BotCurvSettings curv, GClass612[] scatterings, bool isPve)
	{
		BotDifficulty_0 = difficulty;
		WildSpawnType_0 = role;
		BotCurvSettings_0 = curv;
		FileSettings = LocalBotSettingsProviderClass.GetSettings(difficulty, role, isPve);
		CurrentScatteringSetting = new ScatteringSettingsClass(scatterings);
		method_0();
	}

	public BotDifficultySettingsClass Copy()
	{
		BotDifficultySettingsClass obj = (BotDifficultySettingsClass)MemberwiseClone();
		obj.FileSettings = FileSettings.Copy();
		obj.method_0();
		return obj;
	}

	public void ApplyPresetLocation(BotLocationModifier modifier)
	{
		FileSettings.Core.AccuratySpeed *= modifier.AccuracySpeed;
		FileSettings.Look.VISIBILITY_CHANGE_SPEED *= modifier.GainSight;
		FileSettings.Core.ScatteringPerMeter *= modifier.Scattering;
		FileSettings.Core.VisibleDistance *= modifier.VisibleDistance;
		FileSettings.Look.RAIN_DEBUFF_SEENCOEFF_MULTIPLYER = Mathf.LerpUnclamped(FileSettings.Look.RAIN_DEBUFF_SEENCOEFF_MULTIPLYER, 1f, modifier.RainVisibilitySpeedCoef);
		FileSettings.Look.RAIN_DEBUFF_MAXVISIBILITY_MULTIPLYER = Mathf.LerpUnclamped(FileSettings.Look.RAIN_DEBUFF_MAXVISIBILITY_MULTIPLYER, 1f, modifier.RainVisibilityDistanceCoef);
		FileSettings.Look.FOG_DEBUFF_SEENCOEFF_MULTIPLYER = Mathf.LerpUnclamped(FileSettings.Look.FOG_DEBUFF_SEENCOEFF_MULTIPLYER, 1f, modifier.FogVisibilitySpeedCoef);
		FileSettings.Look.FOG_DEBUFF_MAXVISIBILITY_MULTIPLYER = Mathf.LerpUnclamped(FileSettings.Look.FOG_DEBUFF_MAXVISIBILITY_MULTIPLYER, 1f, modifier.FogVisibilityDistanceCoef);
		FileSettings.Mind.MAX_DIST_TO_PERSUE_AXEMAN *= modifier.DistToPersueAxemanCoef;
		FileSettings.Mind.MAX_DIST_TO_RUN_PERSUE_AXEMAN *= modifier.DistToPersueAxemanCoef;
		if (FileSettings.Mind.MAX_DIST_TO_PERSUE_AXEMAN < 0f)
		{
			FileSettings.Mind.WILL_PERSUE_AXEMAN = false;
		}
	}

	public void ApplyPreset(BotPresetClass settings)
	{
		if (settings.UseThis)
		{
			if (!(settings.SCATTERING_DIST_MODIF > 3f) && !(settings.SCATTERING_DIST_MODIF < 0.3f))
			{
				FileSettings.Aiming.SCATTERING_DIST_MODIF = settings.SCATTERING_DIST_MODIF;
			}
			float visibleAngle = settings.VisibleAngle;
			if (!(visibleAngle > 90f) && !(visibleAngle < 50f))
			{
				FileSettings.Core.VisibleAngle = settings.VisibleAngle;
			}
			if (!(settings.VisibleDistance > 500f) && !(settings.VisibleDistance < 40f))
			{
				FileSettings.Core.VisibleDistance = settings.VisibleDistance;
			}
			if (!(settings.ScatteringPerMeter > 0.5f) && !(settings.ScatteringPerMeter <= 0f))
			{
				FileSettings.Core.ScatteringPerMeter = settings.ScatteringPerMeter;
			}
			if (!(settings.HearingSense > 5f) && !(settings.HearingSense <= 0f))
			{
				FileSettings.Core.HearingSense = settings.HearingSense;
			}
			if (!(settings.MAX_AIMING_UPGRADE_BY_TIME < 0f) && !(settings.MAX_AIMING_UPGRADE_BY_TIME >= 2f))
			{
				FileSettings.Aiming.MAX_AIMING_UPGRADE_BY_TIME = settings.MAX_AIMING_UPGRADE_BY_TIME;
			}
			if (!(settings.FIRST_CONTACT_ADD_SEC < 0f) && !(settings.FIRST_CONTACT_ADD_SEC >= 100f))
			{
				FileSettings.Aiming.FIRST_CONTACT_ADD_SEC = settings.FIRST_CONTACT_ADD_SEC;
			}
			if (!(settings.COEF_IF_MOVE < 0f) && !(settings.COEF_IF_MOVE >= 100f))
			{
				FileSettings.Aiming.COEF_IF_MOVE = settings.COEF_IF_MOVE;
			}
			if (!(settings.VISIBILITY_CHANGE_SPEED <= 0f))
			{
				FileSettings.Look.VISIBILITY_CHANGE_SPEED = settings.VISIBILITY_CHANGE_SPEED;
			}
		}
	}

	public void Activate()
	{
		if (CurrentScatteringSetting == null)
		{
			Debug.LogError("Current scattering settings is NULL pls check backend. default settings loaded");
			CurrentScatteringSetting = new ScatteringSettingsClass(null);
		}
		CurrentScatteringSetting.Check(FileSettings.Scattering);
	}

	public void DebugUpdateSettingsExternal(bool isPve)
	{
		BotSettingsComponents settings = LocalBotSettingsProviderClass.GetSettings(BotDifficulty_0, WildSpawnType_0, isPve);
		FileSettings = settings.Copy();
	}

	public bool SetAttribute(object obj, string fieldName, object val)
	{
		FieldInfo field = obj.GetType().GetField(fieldName);
		if (field == null)
		{
			Debug.LogError("can't find bot setting field by name:" + fieldName);
			return false;
		}
		if (field.FieldType == typeof(float))
		{
			float num = Convert.ToSingle(val);
			field.SetValue(obj, num);
			return true;
		}
		if (field.FieldType == typeof(int))
		{
			int num2 = Convert.ToInt32(val);
			field.SetValue(obj, num2);
			return true;
		}
		return false;
	}

	public T GetAttribute<T>(object obj, string _name)
	{
		return (T)obj.GetType().GetField(_name).GetValue(obj);
	}

	public bool DebugChangeParameter(string cls, string prm, object val)
	{
		object attribute = GetAttribute<object>(FileSettings, cls);
		if (attribute != null)
		{
			if (!SetAttribute(attribute, prm, val))
			{
				return false;
			}
			return true;
		}
		return false;
	}

	public void UpdateManual()
	{
	}

	public void method_0()
	{
		Current = new GClass615(FileSettings, CurrentScatteringSetting);
	}

	public bool IgnoreBackendHostility()
	{
		if (DebugBotData.Instance != null)
		{
			return DebugBotData.Instance.IgnoreBackendHostilitySettings;
		}
		return false;
	}

	public List<WildSpawnType> GetAlwaysFriendlyBotTypes()
	{
		if (List_0.Count == 0)
		{
			List_0.AddRange(FileSettings.Mind.FRIENDLY_BOT_TYPES);
			AdditionalHostilitySettings[] additionalHostilitySettings = Singleton<IBotGame>.Instance.BotsController.BotLocationModifier.AdditionalHostilitySettings;
			if (additionalHostilitySettings != null && !IgnoreBackendHostility())
			{
				foreach (AdditionalHostilitySettings additionalHostilitySettings2 in additionalHostilitySettings)
				{
					if (additionalHostilitySettings2.BotRole == WildSpawnType_0)
					{
						List_0.AddRange(additionalHostilitySettings2.AlwaysFriends);
						WildSpawnType[] alwaysEnemies = additionalHostilitySettings2.AlwaysEnemies;
						foreach (WildSpawnType item in alwaysEnemies)
						{
							List_0.Remove(item);
						}
						break;
					}
				}
			}
		}
		return List_0;
	}

	public List<WildSpawnType> GetFriendlyBotTypes()
	{
		if (List_2.Count == 0)
		{
			List_2.AddRange(FileSettings.Mind.FRIENDLY_BOT_TYPES);
			AdditionalHostilitySettings[] additionalHostilitySettings = Singleton<IBotGame>.Instance.BotsController.BotLocationModifier.AdditionalHostilitySettings;
			if (additionalHostilitySettings != null && !IgnoreBackendHostility())
			{
				foreach (AdditionalHostilitySettings additionalHostilitySettings2 in additionalHostilitySettings)
				{
					if (additionalHostilitySettings2.BotRole == WildSpawnType_0)
					{
						List_2.AddRange(additionalHostilitySettings2.AlwaysFriends);
						List_2.AddRange(additionalHostilitySettings2.Neutral);
						WildSpawnType[] alwaysEnemies = additionalHostilitySettings2.AlwaysEnemies;
						foreach (WildSpawnType item in alwaysEnemies)
						{
							List_2.Remove(item);
						}
						break;
					}
				}
			}
		}
		return List_2;
	}

	public List<WildSpawnType> GetFriendNoWarnBotTypes()
	{
		AdditionalHostilitySettings[] additionalHostilitySettings = Singleton<IBotGame>.Instance.BotsController.BotLocationModifier.AdditionalHostilitySettings;
		if (List_1.Count == 0 && additionalHostilitySettings != null && !IgnoreBackendHostility())
		{
			foreach (AdditionalHostilitySettings additionalHostilitySettings2 in additionalHostilitySettings)
			{
				if (additionalHostilitySettings2.BotRole == WildSpawnType_0)
				{
					List_1.AddRange(additionalHostilitySettings2.AlwaysFriends);
					WildSpawnType[] alwaysEnemies = additionalHostilitySettings2.AlwaysEnemies;
					foreach (WildSpawnType item in alwaysEnemies)
					{
						List_1.Remove(item);
					}
					break;
				}
			}
		}
		return List_1;
	}

	public List<WildSpawnType> GetWarnBotTypes()
	{
		if (List_3.Count == 0)
		{
			List_3.AddRange(FileSettings.Mind.WARN_BOT_TYPES);
			AdditionalHostilitySettings[] additionalHostilitySettings = Singleton<IBotGame>.Instance.BotsController.BotLocationModifier.AdditionalHostilitySettings;
			if (additionalHostilitySettings != null && !IgnoreBackendHostility())
			{
				foreach (AdditionalHostilitySettings additionalHostilitySettings2 in additionalHostilitySettings)
				{
					if (additionalHostilitySettings2.BotRole == WildSpawnType_0)
					{
						List_3.AddRange(additionalHostilitySettings2.Warn);
						WildSpawnType[] alwaysFriends = additionalHostilitySettings2.AlwaysFriends;
						foreach (WildSpawnType item in alwaysFriends)
						{
							List_3.Remove(item);
						}
						alwaysFriends = additionalHostilitySettings2.Neutral;
						foreach (WildSpawnType item2 in alwaysFriends)
						{
							List_3.Remove(item2);
						}
						alwaysFriends = additionalHostilitySettings2.AlwaysEnemies;
						foreach (WildSpawnType item3 in alwaysFriends)
						{
							List_3.Remove(item3);
						}
						break;
					}
				}
			}
		}
		return List_3;
	}

	public List<WildSpawnType> GetEnemyBotTypes()
	{
		if (List_4.Count == 0)
		{
			List_4.AddRange(FileSettings.Mind.ENEMY_BOT_TYPES);
			AdditionalHostilitySettings[] additionalHostilitySettings = Singleton<IBotGame>.Instance.BotsController.BotLocationModifier.AdditionalHostilitySettings;
			if (additionalHostilitySettings != null && !IgnoreBackendHostility())
			{
				foreach (AdditionalHostilitySettings additionalHostilitySettings2 in additionalHostilitySettings)
				{
					if (additionalHostilitySettings2.BotRole == WildSpawnType_0)
					{
						List_4.AddRange(additionalHostilitySettings2.AlwaysEnemies);
						WildSpawnType[] alwaysFriends = additionalHostilitySettings2.AlwaysFriends;
						foreach (WildSpawnType item in alwaysFriends)
						{
							List_4.Remove(item);
						}
						alwaysFriends = additionalHostilitySettings2.Neutral;
						foreach (WildSpawnType item2 in alwaysFriends)
						{
							List_4.Remove(item2);
						}
						alwaysFriends = additionalHostilitySettings2.Warn;
						foreach (WildSpawnType item3 in alwaysFriends)
						{
							List_4.Remove(item3);
						}
						AdditionalHostilitySettings.ChancedEnemy[] chancedEnemies = additionalHostilitySettings2.ChancedEnemies;
						foreach (AdditionalHostilitySettings.ChancedEnemy chancedEnemy in chancedEnemies)
						{
							List_4.Remove(chancedEnemy.Role);
						}
						break;
					}
				}
			}
		}
		return List_4;
	}

	public List<WildSpawnType> GetPotentialByChanceEnemyBotTypes()
	{
		if (List_5.Count == 0)
		{
			AdditionalHostilitySettings[] additionalHostilitySettings = Singleton<IBotGame>.Instance.BotsController.BotLocationModifier.AdditionalHostilitySettings;
			if (additionalHostilitySettings != null && !IgnoreBackendHostility())
			{
				foreach (AdditionalHostilitySettings additionalHostilitySettings2 in additionalHostilitySettings)
				{
					if (additionalHostilitySettings2.BotRole != WildSpawnType_0)
					{
						continue;
					}
					for (int j = 0; j < additionalHostilitySettings2.ChancedEnemies.Length; j++)
					{
						AdditionalHostilitySettings.ChancedEnemy chancedEnemy = additionalHostilitySettings2.ChancedEnemies[j];
						if (chancedEnemy.EnemyChance > 0)
						{
							List_5.Add(chancedEnemy.Role);
						}
					}
				}
			}
		}
		return List_5;
	}

	public bool IsEnemyByChance(BotOwner bot)
	{
		AdditionalHostilitySettings[] additionalHostilitySettings = Singleton<IBotGame>.Instance.BotsController.BotLocationModifier.AdditionalHostilitySettings;
		if (List_6.Contains(bot.BotsGroup))
		{
			return true;
		}
		if (List_7.Contains(bot.BotsGroup))
		{
			return false;
		}
		if (additionalHostilitySettings != null && !IgnoreBackendHostility())
		{
			foreach (AdditionalHostilitySettings additionalHostilitySettings2 in additionalHostilitySettings)
			{
				if (additionalHostilitySettings2.BotRole != WildSpawnType_0)
				{
					continue;
				}
				for (int j = 0; j < additionalHostilitySettings2.ChancedEnemies.Length; j++)
				{
					AdditionalHostilitySettings.ChancedEnemy chancedEnemy = additionalHostilitySettings2.ChancedEnemies[j];
					if (chancedEnemy.Role == bot.Profile.Info.Settings.Role)
					{
						bool num = GClass856.IsTrue100(chancedEnemy.EnemyChance);
						if (num)
						{
							List_6.Add(bot.BotsGroup);
							return num;
						}
						List_7.Add(bot.BotsGroup);
						return num;
					}
				}
			}
		}
		return false;
	}

	public bool IsPlayerEnemy(IPlayer player)
	{
		AdditionalHostilitySettings[] additionalHostilitySettings = Singleton<IBotGame>.Instance.BotsController.BotLocationModifier.AdditionalHostilitySettings;
		AdditionalHostilitySettings additionalHostilitySettings2 = null;
		if (additionalHostilitySettings != null && !IgnoreBackendHostility())
		{
			foreach (AdditionalHostilitySettings additionalHostilitySettings3 in additionalHostilitySettings)
			{
				if (additionalHostilitySettings3.BotRole == WildSpawnType_0)
				{
					additionalHostilitySettings2 = additionalHostilitySettings3;
					break;
				}
			}
		}
		bool flag = false;
		switch (player.Side)
		{
		case EPlayerSide.Usec:
			if (additionalHostilitySettings2 != null)
			{
				if (additionalHostilitySettings2.UsecPlayerBehaviour.HasFlag(EWarnBehaviour.AlwaysFriends) || additionalHostilitySettings2.UsecPlayerBehaviour.HasFlag(EWarnBehaviour.Neutral) || additionalHostilitySettings2.UsecPlayerBehaviour.HasFlag(EWarnBehaviour.Warn))
				{
					return false;
				}
				if (additionalHostilitySettings2.UsecPlayerBehaviour.HasFlag(EWarnBehaviour.AlwaysEnemies))
				{
					flag = true;
					return true;
				}
				if (additionalHostilitySettings2.UsecPlayerBehaviour.HasFlag(EWarnBehaviour.ChancedEnemies))
				{
					flag = flag || GClass856.IsTrue100(additionalHostilitySettings2.UsecEnemyChance);
				}
			}
			return FileSettings.Mind.DEFAULT_USEC_BEHAVIOUR.HasFlag(EWarnBehaviour.AlwaysEnemies) || flag;
		case EPlayerSide.Bear:
			if (additionalHostilitySettings2 != null)
			{
				if (additionalHostilitySettings2.BearPlayerBehaviour.HasFlag(EWarnBehaviour.AlwaysFriends) || additionalHostilitySettings2.BearPlayerBehaviour.HasFlag(EWarnBehaviour.Neutral) || additionalHostilitySettings2.BearPlayerBehaviour.HasFlag(EWarnBehaviour.Warn))
				{
					return false;
				}
				if (additionalHostilitySettings2.BearPlayerBehaviour.HasFlag(EWarnBehaviour.AlwaysEnemies))
				{
					flag = true;
					return true;
				}
				if (additionalHostilitySettings2.BearPlayerBehaviour.HasFlag(EWarnBehaviour.ChancedEnemies))
				{
					return flag = flag || GClass856.IsTrue100(additionalHostilitySettings2.BearEnemyChance);
				}
			}
			return FileSettings.Mind.DEFAULT_BEAR_BEHAVIOUR.HasFlag(EWarnBehaviour.AlwaysEnemies) || flag;
		default:
			return false;
		case EPlayerSide.Savage:
			if (additionalHostilitySettings2 != null)
			{
				if (additionalHostilitySettings2.SavagePlayerBehaviour.HasFlag(EWarnBehaviour.AlwaysFriends) || additionalHostilitySettings2.SavagePlayerBehaviour.HasFlag(EWarnBehaviour.Neutral) || additionalHostilitySettings2.SavagePlayerBehaviour.HasFlag(EWarnBehaviour.Warn))
				{
					return false;
				}
				if (additionalHostilitySettings2.SavagePlayerBehaviour.HasFlag(EWarnBehaviour.AlwaysEnemies))
				{
					flag = true;
					return true;
				}
				if (additionalHostilitySettings2.SavagePlayerBehaviour.HasFlag(EWarnBehaviour.ChancedEnemies))
				{
					flag = flag || GClass856.IsTrue100(additionalHostilitySettings2.SavageEnemyChance);
				}
			}
			return FileSettings.Mind.DEFAULT_SAVAGE_BEHAVIOUR.HasFlag(EWarnBehaviour.AlwaysEnemies) || flag;
		}
	}

	public bool IsPlayerWarn(IPlayer player)
	{
		AdditionalHostilitySettings[] additionalHostilitySettings = Singleton<IBotGame>.Instance.BotsController.BotLocationModifier.AdditionalHostilitySettings;
		AdditionalHostilitySettings additionalHostilitySettings2 = null;
		if (additionalHostilitySettings != null && !IgnoreBackendHostility())
		{
			foreach (AdditionalHostilitySettings additionalHostilitySettings3 in additionalHostilitySettings)
			{
				if (additionalHostilitySettings3.BotRole == WildSpawnType_0)
				{
					additionalHostilitySettings2 = additionalHostilitySettings3;
					break;
				}
			}
		}
		bool flag = false;
		switch (player.Side)
		{
		case EPlayerSide.Usec:
			if (additionalHostilitySettings2 != null)
			{
				if (additionalHostilitySettings2.UsecPlayerBehaviour.HasFlag(EWarnBehaviour.Warn))
				{
					flag = true;
					return true;
				}
				if (additionalHostilitySettings2.UsecPlayerBehaviour.HasFlag(EWarnBehaviour.Neutral) || additionalHostilitySettings2.UsecPlayerBehaviour.HasFlag(EWarnBehaviour.AlwaysFriends))
				{
					return false;
				}
			}
			return FileSettings.Mind.DEFAULT_USEC_BEHAVIOUR.HasFlag(EWarnBehaviour.Warn) || flag;
		case EPlayerSide.Bear:
			if (additionalHostilitySettings2 != null)
			{
				if (additionalHostilitySettings2.BearPlayerBehaviour.HasFlag(EWarnBehaviour.Warn))
				{
					flag = true;
					return true;
				}
				if (additionalHostilitySettings2.BearPlayerBehaviour.HasFlag(EWarnBehaviour.Neutral) || additionalHostilitySettings2.BearPlayerBehaviour.HasFlag(EWarnBehaviour.AlwaysFriends))
				{
					return false;
				}
			}
			return FileSettings.Mind.DEFAULT_BEAR_BEHAVIOUR.HasFlag(EWarnBehaviour.Warn);
		default:
			return false;
		case EPlayerSide.Savage:
			if (additionalHostilitySettings2 != null)
			{
				if (additionalHostilitySettings2.SavagePlayerBehaviour.HasFlag(EWarnBehaviour.Warn))
				{
					flag = true;
					return true;
				}
				if (additionalHostilitySettings2.SavagePlayerBehaviour.HasFlag(EWarnBehaviour.Neutral) || additionalHostilitySettings2.SavagePlayerBehaviour.HasFlag(EWarnBehaviour.AlwaysFriends))
				{
					return false;
				}
			}
			return FileSettings.Mind.DEFAULT_SAVAGE_BEHAVIOUR.HasFlag(EWarnBehaviour.Warn) || flag;
		}
	}

	public bool IsPlayerAlwaysFriends(IPlayer player)
	{
		AdditionalHostilitySettings[] additionalHostilitySettings = Singleton<IBotGame>.Instance.BotsController.BotLocationModifier.AdditionalHostilitySettings;
		AdditionalHostilitySettings additionalHostilitySettings2 = null;
		if (additionalHostilitySettings != null && !IgnoreBackendHostility())
		{
			foreach (AdditionalHostilitySettings additionalHostilitySettings3 in additionalHostilitySettings)
			{
				if (additionalHostilitySettings3.BotRole == WildSpawnType_0)
				{
					additionalHostilitySettings2 = additionalHostilitySettings3;
					break;
				}
			}
		}
		switch (player.Side)
		{
		case EPlayerSide.Usec:
			if (additionalHostilitySettings2 != null)
			{
				return additionalHostilitySettings2.UsecPlayerBehaviour.HasFlag(EWarnBehaviour.AlwaysFriends);
			}
			break;
		case EPlayerSide.Bear:
			if (additionalHostilitySettings2 != null)
			{
				return additionalHostilitySettings2.BearPlayerBehaviour.HasFlag(EWarnBehaviour.AlwaysFriends);
			}
			break;
		case EPlayerSide.Savage:
			if (additionalHostilitySettings2 != null)
			{
				return additionalHostilitySettings2.SavagePlayerBehaviour.HasFlag(EWarnBehaviour.AlwaysFriends);
			}
			break;
		}
		return false;
	}
}
