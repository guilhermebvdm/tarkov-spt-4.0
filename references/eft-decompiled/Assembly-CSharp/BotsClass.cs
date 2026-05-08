using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using EFT;

public class BotsClass
{
	[CompilerGenerated]
	public class Class348
	{
		public EPlayerSide side;

		public bool method_0(BotOwner x)
		{
			return x.Side != side;
		}
	}

	[CompilerGenerated]
	public class Class349
	{
		public BotsClass BotsClass;

		public BotOwner owner;

		public bool method_0(BotOwner x)
		{
			return BotsClass.method_0(x, owner, owner.Settings.FileSettings);
		}
	}

	[NonSerialized]
	public HashSet<BotOwner> HashSet_0 = new HashSet<BotOwner>();

	[NonSerialized]
	public GClass412 Gclass412_0;

	[NonSerialized]
	public HashSet<int> HashSet_1 = new HashSet<int>();

	[CompilerGenerated]
	private Action<BotOwner> action_0;

	[CompilerGenerated]
	private Action<BotOwner> action_1;

	[CompilerGenerated]
	private Action<Player> action_2;

	[NonSerialized]
	public List<BotOwner> List_0 = new List<BotOwner>();

	public int Count => HashSet_0.Count;

	public IEnumerable<BotOwner> BotOwners => HashSet_0;

	public event Action<BotOwner> OnBotAdd
	{
		[CompilerGenerated]
		add
		{
			Action<BotOwner> action = action_0;
			Action<BotOwner> action2;
			do
			{
				action2 = action;
				Action<BotOwner> value2 = (Action<BotOwner>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<BotOwner> action = action_0;
			Action<BotOwner> action2;
			do
			{
				action2 = action;
				Action<BotOwner> value2 = (Action<BotOwner>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public event Action<BotOwner> OnBotRemove
	{
		[CompilerGenerated]
		add
		{
			Action<BotOwner> action = action_1;
			Action<BotOwner> action2;
			do
			{
				action2 = action;
				Action<BotOwner> value2 = (Action<BotOwner>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_1, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<BotOwner> action = action_1;
			Action<BotOwner> action2;
			do
			{
				action2 = action;
				Action<BotOwner> value2 = (Action<BotOwner>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_1, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public event Action<Player> OnPlayerRemove
	{
		[CompilerGenerated]
		add
		{
			Action<Player> action = action_2;
			Action<Player> action2;
			do
			{
				action2 = action;
				Action<Player> value2 = (Action<Player>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_2, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<Player> action = action_2;
			Action<Player> action2;
			do
			{
				action2 = action;
				Action<Player> value2 = (Action<Player>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_2, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public BotsClass(GClass412 connections)
	{
		Gclass412_0 = connections;
	}

	public List<BotOwner> GetListByZone(BotZone zone)
	{
		List<BotOwner> list = new List<BotOwner>();
		foreach (BotOwner item in HashSet_0)
		{
			if (item.BotsGroup.BotZone == zone)
			{
				list.Add(item);
			}
		}
		return list;
	}

	public IEnumerable<BotOwner> GetOtherSideRule(EPlayerSide side)
	{
		return HashSet_0.Where((BotOwner x) => x.Side != side);
	}

	public IEnumerable<BotOwner> GetEnemies(BotOwner owner)
	{
		return HashSet_0.Where((BotOwner x) => method_0(x, owner, owner.Settings.FileSettings));
	}

	public bool method_0(BotOwner botToCheck, BotOwner owner, BotSettingsComponents fileSettings)
	{
		bool result = false;
		WildSpawnType role = botToCheck.Profile.Info.Settings.Role;
		if (owner.Settings.IsEnemyByChance(botToCheck))
		{
			result = true;
		}
		else if (owner.Settings.GetFriendlyBotTypes().Contains(role) || owner.Settings.GetWarnBotTypes().Contains(role))
		{
			result = false;
			return false;
		}
		if (owner.Settings.GetEnemyBotTypes().Contains(role))
		{
			result = true;
		}
		return result;
	}

	public void AddFromList()
	{
		if (List_0.Count <= 0)
		{
			return;
		}
		foreach (BotOwner item in List_0)
		{
			HashSet_0.Add(item);
			Gclass412_0.AddPerson(item);
			action_0?.Invoke(item);
		}
		List_0.Clear();
	}

	public void Add(BotOwner bot)
	{
		List_0.Add(bot);
	}

	public bool Remove(BotOwner bot)
	{
		Gclass412_0.Remove(bot);
		bot.BotsGroup.RemoveAlly(bot);
		bool num = HashSet_0.Remove(bot);
		if (num)
		{
			Action<BotOwner> action = action_1;
			if (action == null)
			{
				return num;
			}
			action(bot);
		}
		return num;
	}

	public void RemovePlayer(Player player)
	{
		Gclass412_0.Remove(player);
		action_2?.Invoke(player);
	}

	public void Clear()
	{
		HashSet_0.Clear();
	}

	public void CheckActivation()
	{
		foreach (BotOwner item in HashSet_0)
		{
			item.PostActivate();
		}
	}

	public BotOwner FirstOrDefault()
	{
		return HashSet_0.FirstOrDefault();
	}

	public GClass412 GetConnector()
	{
		return Gclass412_0;
	}

	public void AddPlayer(Player player)
	{
		if (player.Profile.Info.Side == EPlayerSide.Savage)
		{
			Gclass412_0.AddPerson(player);
		}
	}

	public void UpdateByUnity()
	{
		foreach (BotOwner item in HashSet_0)
		{
			try
			{
				item.UpdateManual();
			}
			catch (Exception)
			{
				if (!HashSet_1.Contains(item.Id))
				{
					HashSet_1.Add(item.Id);
				}
			}
		}
		AddFromList();
	}

	public void Stop()
	{
		foreach (BotOwner botOwner in BotOwners)
		{
			botOwner.Dispose();
		}
		Clear();
	}
}
