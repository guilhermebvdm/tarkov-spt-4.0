using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using EFT;
using UnityEngine;

public class GClass1201 : GInterface4
{
	[CompilerGenerated]
	private Action<string, GClass408> action_0;

	[CompilerGenerated]
	private Action<string, GClass408> action_1;

	[NonSerialized]
	[CompilerGenerated]
	public Player Player_0;

	[NonSerialized]
	[CompilerGenerated]
	public bool Bool_0;

	[NonSerialized]
	[CompilerGenerated]
	public GameWorld GameWorld_0;

	public Player MyPlayer
	{
		[CompilerGenerated]
		get
		{
			return Player_0;
		}
	}

	public bool IsDeveloperMode
	{
		[CompilerGenerated]
		get
		{
			return Bool_0;
		}
	}

	public GameWorld GameWorld
	{
		[CompilerGenerated]
		get
		{
			return GameWorld_0;
		}
	}

	public event Action<string, GClass408> OnPlayerDataAdded
	{
		[CompilerGenerated]
		add
		{
			Action<string, GClass408> action = action_0;
			Action<string, GClass408> action2;
			do
			{
				action2 = action;
				Action<string, GClass408> value2 = (Action<string, GClass408>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<string, GClass408> action = action_0;
			Action<string, GClass408> action2;
			do
			{
				action2 = action;
				Action<string, GClass408> value2 = (Action<string, GClass408>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public event Action<string, GClass408> OnPlayerDataRemoved
	{
		[CompilerGenerated]
		add
		{
			Action<string, GClass408> action = action_1;
			Action<string, GClass408> action2;
			do
			{
				action2 = action;
				Action<string, GClass408> value2 = (Action<string, GClass408>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_1, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<string, GClass408> action = action_1;
			Action<string, GClass408> action2;
			do
			{
				action2 = action;
				Action<string, GClass408> value2 = (Action<string, GClass408>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_1, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public void Initialize(int playerId = 0)
	{
	}

	public ActorDataStruct GetBotDebugData(IPlayer person, string profileId)
	{
		return default(ActorDataStruct);
	}

	public void FillGroupData(List<GStruct21> resultCache)
	{
	}

	public void FillProfilesData(GClass406 resultCache)
	{
	}

	public GStruct11 GetSpawnsData()
	{
		return default(GStruct11);
	}

	public GStruct22 GetZoneData()
	{
		return default(GStruct22);
	}

	public List<GStruct21> GetGroupData()
	{
		return null;
	}

	public GClass406 GetProfilesData()
	{
		return null;
	}

	public void SpawnBotDebugServer(EPlayerSide side, bool canBeSnipe, WildSpawnType profile = WildSpawnType.assault, BotDifficulty botDifficulty = BotDifficulty.normal, bool forcedSpawn = false)
	{
	}

	public void SpawnBotDebugClient(EPlayerSide side, bool canBeSnipe, DebugBotProfileChooser profile = DebugBotProfileChooser.Random, bool forcedSpawn = false)
	{
	}

	public void SpawnBotDebugServerLocalFiles(EPlayerSide side, bool canBeSnipe, int count, DebugBotProfileChooser profile = DebugBotProfileChooser.Random, bool forcedSpawn = false)
	{
	}

	public void DebugSpawnBot(WildSpawnType profile, BotDifficulty difficulty)
	{
	}

	public void TeleportClosestBot(Vector3 pos)
	{
	}

	public void TeleportSelectedBot(int botId, Vector3 pos)
	{
	}

	public void DeserializeDebugBotStruct(byte[] data)
	{
	}

	public void DeserializeDataZones(byte[] spawns)
	{
	}

	public void DeserializeDataGroups(byte[] data)
	{
	}

	public void StartGettingInfo()
	{
	}

	public void StopGettingInfo()
	{
	}

	public void KillBot(int botId)
	{
	}

	public void KillAllBots()
	{
	}

	public void SetDebugBrain(int botId, DebugBotDesition selectedDebugBrain)
	{
	}

	public void SetDebugBrainAllBots(DebugBotDesition selectedDebugBrain)
	{
	}

	public void DeserializeCanFireAllBots(byte[] data)
	{
	}

	public void SetDataProfiles(byte[] data)
	{
	}

	public void SetCanFireAllBots(bool canFire)
	{
	}

	public bool GetCanFireAllBots()
	{
		return false;
	}

	public void SetIgnoreKarmaAllBots(bool needIgnore)
	{
	}

	public void SetFreeForAll(bool isFree)
	{
	}

	public void AddEffectToBot(int botId, EDebugBotEffect selectedEffect)
	{
	}

	public void AddEffectToAllBots(EDebugBotEffect selectedEffect)
	{
	}

	public void SetDisableGrenadesForAll(bool needDisable)
	{
	}

	public void AddDebugLogMarker(int playerId)
	{
	}

	public void SetNetworkDataUpdateKd(float networkDataUpdateKd)
	{
	}
}
