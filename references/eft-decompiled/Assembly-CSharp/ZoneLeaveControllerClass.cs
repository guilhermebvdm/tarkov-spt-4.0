using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using EFT;
using EFT.Game.Spawning;
using JetBrains.Annotations;
using UnityEngine;

public class ZoneLeaveControllerClass
{
	[Serializable]
	[CompilerGenerated]
	public class Class350
	{
		public static readonly Class350 class350_0 = new Class350();

		public static Func<BotOwner, bool> func_0;

		public bool method_0(BotOwner x)
		{
			return x.LeaveData.WannaLeave;
		}
	}

	[CompilerGenerated]
	public class Class351
	{
		public ZoneLeaveControllerClass ZoneLeaveControllerClass;

		public WildSpawnType type;

		public void method_0()
		{
			ZoneLeaveControllerClass.method_7(type);
		}
	}

	[NonSerialized]
	public const float Float_0 = 23f;

	[NonSerialized]
	public float Float_1 = 5.5f;

	[NonSerialized]
	public float Float_2 = 6.5f;

	[NonSerialized]
	public BotsClass BotsClass;

	[NonSerialized]
	public float Float_3 = 2f;

	[NonSerialized]
	public float Float_4 = 90f;

	[NonSerialized]
	public int Int_0 = 6;

	[NonSerialized]
	public int Int_1;

	[NonSerialized]
	[CompilerGenerated]
	public bool Bool_0;

	[NonSerialized]
	public Dictionary<BotZone, HashSet<WildSpawnType>> Dictionary_0 = new Dictionary<BotZone, HashSet<WildSpawnType>>();

	[NonSerialized]
	public GameDateTime GameDateTime_0;

	[NonSerialized]
	public DateTime DateTime_0;

	[NonSerialized]
	public IPlayersCollection IplayersCollection_0;

	[NonSerialized]
	public bool Bool_1;

	[NonSerialized]
	public bool Bool_2;

	[NonSerialized]
	public int Int_2;

	public bool NoZoneBlocks
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

	public ZoneLeaveControllerClass([CanBeNull] BotsClass botsList, GameDateTime gameTime, BotZone[] allZones, bool haveSectants, int maxBots, [NotNull] IPlayersCollection players)
	{
		Int_2 = maxBots;
		Bool_2 = haveSectants;
		IplayersCollection_0 = players;
		float num = GClass856.Random(Float_1, Float_2);
		Int_0 = (int)num;
		float remain = num - (float)Int_0;
		Int_1 = method_0(remain);
		foreach (BotZone key in allZones)
		{
			Dictionary_0.Add(key, new HashSet<WildSpawnType>());
		}
		GameDateTime_0 = gameTime;
		DateTime dateTime_ = GameDateTime_0.Calculate();
		DateTime_0 = dateTime_;
		Bool_1 = IsDayByHour(DateTime_0);
		BotsClass = botsList;
		BotsClass.OnBotRemove += method_8;
		BotsClass.OnBotAdd += method_1;
	}

	public void SetMaxBots(int maxBots)
	{
		Int_2 = maxBots;
	}

	public void Activate()
	{
		StaticManager.Instance.TimerManager.MakeTimer(TimeSpan.FromSeconds(Float_3), isLopped: true).OnTimer += method_4;
		if (Bool_2)
		{
			if (IsDayByHour(DateTime_0))
			{
				method_2();
			}
			else
			{
				method_3();
			}
		}
	}

	public bool IsDay()
	{
		return IsDayByHour(DateTime_0);
	}

	public bool IsDayByHour(DateTime dateTime)
	{
		if (dateTime.Hour >= Int_0 + 1 || (dateTime.Minute >= Int_1 && dateTime.Hour >= Int_0))
		{
			return (float)dateTime.Hour < 23f;
		}
		return false;
	}

	public Dictionary<BotZone, HashSet<WildSpawnType>> GetForDebug()
	{
		return Dictionary_0;
	}

	public void RunScenarionTimeChecks()
	{
		method_4();
	}

	public bool IsZoneBlockFor(BotZone zone, WildSpawnType type)
	{
		if (!NoZoneBlocks && !(zone == null))
		{
			if (Dictionary_0.TryGetValue(zone, out var value))
			{
				return value.Contains(type);
			}
			return false;
		}
		return false;
	}

	public void BotSpawned(BotOwner bot)
	{
		BotZone botZone = bot.BotsGroup.BotZone;
		bool flag = Dictionary_0[botZone].Contains(bot.Profile.Info.Settings.Role);
		if ((!DebugBotData.UseDebugData || !DebugBotData.Instance.NoZoneBlocks) && flag)
		{
			bot.LeaveData.DoLeaveExternal();
		}
	}

	public void ClearByTypeAll(WildSpawnType wildSpawnType)
	{
		if (NoZoneBlocks)
		{
			return;
		}
		foreach (BotOwner botOwner in BotsClass.BotOwners)
		{
			if (botOwner.Profile.Info.Settings.Role == wildSpawnType)
			{
				botOwner.LeaveData.DoLeaveExternal();
			}
		}
	}

	public void ClearZone(BotZone zone, WildSpawnType wildSpawnType)
	{
		foreach (BotOwner item in BotsClass.GetListByZone(zone))
		{
			if (item.Profile.Info.Settings.Role == wildSpawnType)
			{
				item.LeaveData.DoLeaveExternal();
			}
		}
	}

	public void UnBlockZoneFor(BotZone botZone, WildSpawnType spawnType)
	{
		if (Dictionary_0.TryGetValue(botZone, out var value))
		{
			value.Remove(spawnType);
		}
	}

	public void BlockZoneFor(BotZone botZone, WildSpawnType spawnType)
	{
		if (!NoZoneBlocks)
		{
			if (Dictionary_0.TryGetValue(botZone, out var value))
			{
				value.Add(spawnType);
				return;
			}
			HashSet<WildSpawnType> hashSet = new HashSet<WildSpawnType>();
			hashSet.Add(spawnType);
			Dictionary_0.Add(botZone, hashSet);
		}
	}

	public GStruct22 GetDebugData()
	{
		return new GStruct22(Dictionary_0);
	}

	public int GetHour()
	{
		return DateTime_0.Hour;
	}

	public void LeaveAll()
	{
		foreach (BotOwner botOwner in BotsClass.BotOwners)
		{
			botOwner.LeaveData.DoLeaveExternal();
		}
	}

	public void ClearForPlaces(int waveEscortCount)
	{
		int count = BotsClass.Count;
		int num = Int_2 - count;
		int num2 = waveEscortCount - num;
		int num3 = BotsClass.BotOwners.Count((BotOwner x) => x.LeaveData.WannaLeave);
		num2 -= num3;
		if (num2 <= 0)
		{
			return;
		}
		HashSet<BotOwner> hashSet = new HashSet<BotOwner>();
		IEnumerable<IPlayer> enumerable = GClass3585.ExceptAI(IplayersCollection_0);
		for (int num4 = 0; num4 < num2; num4++)
		{
			BotOwner botOwner = null;
			float num5 = float.MinValue;
			foreach (BotOwner botOwner2 in BotsClass.BotOwners)
			{
				WildSpawnType role = botOwner2.Profile.Info.Settings.Role;
				if (hashSet.Contains(botOwner2) || BotSettingsRepoClass.IsBossOrFollower(role))
				{
					continue;
				}
				float num6 = float.MinValue;
				foreach (IPlayer item in enumerable)
				{
					float sqrMagnitude = (item.Position - botOwner2.Position).sqrMagnitude;
					if (sqrMagnitude > num6)
					{
						num6 = sqrMagnitude;
					}
				}
				if (num5 < num6)
				{
					botOwner = botOwner2;
				}
			}
			if (botOwner != null)
			{
				hashSet.Add(botOwner);
				botOwner.LeaveData.DoLeaveExternal();
			}
		}
	}

	public void BlockAllZonesSimpleTypes()
	{
		method_6(WildSpawnType.assault);
		method_6(WildSpawnType.assaultGroup);
		method_6(WildSpawnType.marksman);
		method_6(WildSpawnType.cursedAssault);
	}

	public void UnBlockAllZonesAsDay()
	{
		method_7(WildSpawnType.assault);
		method_7(WildSpawnType.assaultGroup);
		method_7(WildSpawnType.marksman);
		method_7(WildSpawnType.cursedAssault);
	}

	public void ClearZoneAndBlockForSimlpe(BotZone botZone)
	{
		ClearZone(botZone, WildSpawnType.assault);
		ClearZone(botZone, WildSpawnType.assaultGroup);
		ClearZone(botZone, WildSpawnType.marksman);
		ClearZone(botZone, WildSpawnType.cursedAssault);
		BlockZoneFor(botZone, WildSpawnType.assault);
		BlockZoneFor(botZone, WildSpawnType.assaultGroup);
		BlockZoneFor(botZone, WildSpawnType.marksman);
		BlockZoneFor(botZone, WildSpawnType.cursedAssault);
	}

	public int method_0(float remain)
	{
		return (int)(60f * Mathf.Clamp01(remain));
	}

	public void method_1(BotOwner obj)
	{
		BotSpawned(obj);
	}

	public void method_2()
	{
		method_5(WildSpawnType.sectantPriest);
		method_5(WildSpawnType.sectantWarrior);
		method_5(WildSpawnType.sectantOni);
		method_5(WildSpawnType.sectantPrizrak);
		method_5(WildSpawnType.sectantPredvestnik);
		method_7(WildSpawnType.assault);
		method_7(WildSpawnType.assaultGroup);
		method_7(WildSpawnType.marksman);
		method_7(WildSpawnType.cursedAssault);
	}

	public void method_3()
	{
		method_7(WildSpawnType.sectantPriest);
		method_7(WildSpawnType.sectantWarrior);
		method_7(WildSpawnType.sectantOni);
		method_7(WildSpawnType.sectantPrizrak);
		method_7(WildSpawnType.sectantPredvestnik);
	}

	public void method_4()
	{
		DateTime dateTime = GameDateTime_0.Calculate();
		bool flag = IsDayByHour(dateTime);
		if (Bool_2)
		{
			if (!flag && Bool_1)
			{
				method_3();
			}
			if (flag && !Bool_1)
			{
				method_2();
			}
		}
		Bool_1 = flag;
		DateTime_0 = dateTime;
	}

	public void method_5(WildSpawnType type, float blockPeriodSec = -1f)
	{
		if (NoZoneBlocks)
		{
			return;
		}
		method_6(type);
		if (blockPeriodSec > 0f)
		{
			StaticManager.Instance.TimerManager.MakeTimer(TimeSpan.FromSeconds(blockPeriodSec)).OnTimer += delegate
			{
				method_7(type);
			};
		}
		ClearByTypeAll(type);
	}

	public void method_6(WildSpawnType typeToBlock)
	{
		foreach (KeyValuePair<BotZone, HashSet<WildSpawnType>> item in Dictionary_0)
		{
			item.Value.Add(typeToBlock);
		}
	}

	public void method_7(WildSpawnType typeToBlock)
	{
		foreach (KeyValuePair<BotZone, HashSet<WildSpawnType>> item in Dictionary_0)
		{
			item.Value.Remove(typeToBlock);
		}
	}

	public void method_8(BotOwner bot)
	{
	}

	public void Dispose()
	{
	}
}
