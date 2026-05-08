using System;
using System.Collections.Generic;
using System.Text;
using EFT;
using JetBrains.Annotations;

public class GClass575
{
	[NonSerialized]
	public List<BotsGroup> List_0 = new List<BotsGroup>();

	[NonSerialized]
	public Dictionary<WildSpawnType, BotsGroup> Data;

	[NonSerialized]
	public EPlayerSide[] EplayerSide_0;

	[NonSerialized]
	public HashSet<BotsGroup> HashSet_0 = new HashSet<BotsGroup>();

	[NonSerialized]
	public static WildSpawnType[] WildSpawnType_0 = (WildSpawnType[])Enum.GetValues(typeof(WildSpawnType));

	public GClass575()
	{
		Data = new Dictionary<WildSpawnType, BotsGroup>();
		WildSpawnType[] wildSpawnType_ = WildSpawnType_0;
		foreach (WildSpawnType key in wildSpawnType_)
		{
			Data.Add(key, null);
		}
	}

	[CanBeNull]
	public BotsGroup Group(bool isBossOrFollower, WildSpawnType spawnType)
	{
		if (isBossOrFollower)
		{
			foreach (BotsGroup item in HashSet_0)
			{
				if (method_0(item, spawnType))
				{
					return item;
				}
			}
			return null;
		}
		foreach (KeyValuePair<WildSpawnType, BotsGroup> datum in Data)
		{
			BotsGroup value = datum.Value;
			if (value != null && method_0(value, spawnType))
			{
				return value;
			}
		}
		foreach (BotsGroup item2 in List_0)
		{
			if (method_0(item2, spawnType))
			{
				return item2;
			}
		}
		return null;
	}

	[CanBeNull]
	public List<BotsGroup> GetAllGroupsDebug()
	{
		List<BotsGroup> list = new List<BotsGroup>();
		foreach (BotsGroup item in HashSet_0)
		{
			if (item.MembersCount > 0)
			{
				list.Add(item);
			}
		}
		foreach (KeyValuePair<WildSpawnType, BotsGroup> datum in Data)
		{
			BotsGroup value = datum.Value;
			if (value != null && value.MembersCount > 0)
			{
				list.Add(value);
			}
		}
		return list;
	}

	public HashSet<BotsGroup> GetGroups(bool notNull)
	{
		HashSet<BotsGroup> hashSet = new HashSet<BotsGroup>();
		WildSpawnType[] allTypes = BotsController.AllTypes;
		foreach (WildSpawnType spawnType in allTypes)
		{
			BotsGroup botsGroup = Group(isBossOrFollower: false, spawnType);
			if (botsGroup != null && (!notNull || botsGroup.MembersCount > 0))
			{
				hashSet.Add(botsGroup);
			}
		}
		foreach (BotsGroup item in HashSet_0)
		{
			if (item != null && (!notNull || item.MembersCount > 0))
			{
				hashSet.Add(item);
			}
		}
		return hashSet;
	}

	public void Set(EPlayerSide side, BotsGroup gr, bool bossOrFollower)
	{
		if (bossOrFollower)
		{
			HashSet_0.Add(gr);
			return;
		}
		WildSpawnType initialBotType = gr.InitialBotType;
		if (Data.ContainsKey(initialBotType))
		{
			Data[initialBotType] = gr;
		}
		else
		{
			Data.Add(initialBotType, gr);
		}
	}

	public List<string> MessageInfo()
	{
		List<string> list = new List<string>();
		foreach (KeyValuePair<WildSpawnType, BotsGroup> datum in Data)
		{
			BotsGroup value = datum.Value;
			if (value != null && value.MembersCount > 0)
			{
				list.Add(value.MessageInfo() + ";");
			}
		}
		return list;
	}

	public string MessageInfoWide()
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (KeyValuePair<WildSpawnType, BotsGroup> datum in Data)
		{
			BotsGroup value = datum.Value;
			if (value != null && value.MembersCount > 0)
			{
				stringBuilder.AppendLine(value.MessageInfoWide() + ";");
			}
		}
		return stringBuilder.ToString();
	}

	public void AddPlayer(IPlayer player)
	{
		if (HashSet_0 != null)
		{
			foreach (BotsGroup item in HashSet_0)
			{
				if (item.IsPlayerEnemy(player) || item.ForcedAggressiveForNewPlayers)
				{
					item.AddEnemy(player, EBotEnemyCause.addPlayerToBoss);
					item.AddEnemyGroupIfAllowed(player.GroupId, player.Side);
				}
				else
				{
					item.AddNeutral(player);
				}
			}
		}
		foreach (KeyValuePair<WildSpawnType, BotsGroup> datum in Data)
		{
			BotsGroup value = datum.Value;
			if (value != null)
			{
				if (value.IsPlayerEnemy(player) || value.ForcedAggressiveForNewPlayers)
				{
					value.AddEnemy(player, EBotEnemyCause.addPlayer);
					value.AddEnemyGroupIfAllowed(player.GroupId, player.Side);
				}
				else
				{
					value.AddNeutral(player);
				}
			}
		}
	}

	public void AddNoKey(BotsGroup gr)
	{
		List_0.Add(gr);
	}

	public void AddBot(BotOwner bot, bool freeForAll)
	{
		foreach (KeyValuePair<WildSpawnType, BotsGroup> datum in Data)
		{
			BotsGroup value = datum.Value;
			if (value != null)
			{
				if (value == bot.BotsGroup)
				{
					value.AddMember(bot, onActivation: false);
				}
				else if (value != bot.BotsGroup && (bot.Side != value.Side || freeForAll))
				{
					value.AddEnemy(bot, EBotEnemyCause.addBotAtGroup);
				}
			}
		}
		if (List_0.Count > 0)
		{
			foreach (BotsGroup item in List_0)
			{
				if ((bot.Side != item.Side || freeForAll) && !item.ContainsEnemy(bot))
				{
					item.AddEnemy(bot, EBotEnemyCause.addBotNoGroup);
				}
			}
		}
		if (freeForAll)
		{
			List_0.Add(bot.BotsGroup);
		}
	}

	public bool method_0(BotsGroup group, WildSpawnType spawnType)
	{
		return group.IsSuitable(spawnType);
	}

	public void Dispose()
	{
		foreach (KeyValuePair<WildSpawnType, BotsGroup> datum in Data)
		{
			datum.Value?.Dispose();
		}
		foreach (BotsGroup item in HashSet_0)
		{
			item?.Dispose();
		}
		foreach (BotsGroup item2 in List_0)
		{
			item2?.Dispose();
		}
		HashSet_0.Clear();
		List_0.Clear();
		Data.Clear();
	}
}
