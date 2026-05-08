using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using EFT;
using JetBrains.Annotations;

public class GClass412
{
	[CompilerGenerated]
	public class Class154
	{
		public GClass412 gclass412_0;

		public BotOwner bot;

		public Action<BotOwner, FollowerStatusChange> action_0;

		public void method_0(BotOwner follower, FollowerStatusChange status)
		{
			gclass412_0.method_3(follower, bot, status);
		}

		public void method_1(BotOwner owner)
		{
			gclass412_0.method_1(bot);
			bot.Boss.OnFollowerStatusChange += delegate(BotOwner follower, FollowerStatusChange b)
			{
				gclass412_0.method_3(follower, bot, b);
			};
		}

		public void method_2(BotOwner follower, FollowerStatusChange b)
		{
			gclass412_0.method_3(follower, bot, b);
		}
	}

	[NonSerialized]
	public const float Float_0 = 2f;

	[NonSerialized]
	public Dictionary<BotOwner, List<BotOwner>> Dictionary_0;

	[NonSerialized]
	public GClass641.IBotTimer IBotTimer;

	[NonSerialized]
	public Dictionary<IPlayer, Dictionary<IPlayer, GClass413>> Dictionary_1;

	[NonSerialized]
	public List<GClass413> List_0 = new List<GClass413>();

	[NonSerialized]
	public HashSet<IPlayer> HashSet_0 = new HashSet<IPlayer>();

	[NonSerialized]
	public bool Bool_0;

	[NonSerialized]
	public bool Bool_1;

	public GClass412()
	{
		Dictionary_1 = new Dictionary<IPlayer, Dictionary<IPlayer, GClass413>>();
	}

	public void Activate()
	{
		Bool_1 = true;
		IBotTimer = StaticManager.Instance.TimerManager.MakeTimer(TimeSpan.FromSeconds(2.0), isLopped: true);
		IBotTimer.OnTimer += method_0;
	}

	public void Stop()
	{
		Bool_1 = false;
	}

	public void ManualUpdate()
	{
		foreach (GClass413 item in List_0)
		{
			item.ManualUpdate();
		}
	}

	public void AddPerson(IPlayer gamePerson)
	{
		Dictionary<IPlayer, GClass413> dictionary = new Dictionary<IPlayer, GClass413>();
		if (gamePerson.IsAI)
		{
			BotOwner bot = gamePerson.AIData.BotOwner;
			if (bot.Boss.IamBoss)
			{
				bot.Boss.OnFollowerStatusChange += delegate(BotOwner follower, FollowerStatusChange status)
				{
					method_3(follower, bot, status);
				};
			}
			else
			{
				bot.Boss.OnBecomeBoss += delegate
				{
					method_1(bot);
					bot.Boss.OnFollowerStatusChange += delegate(BotOwner follower, FollowerStatusChange b)
					{
						method_3(follower, bot, b);
					};
				};
			}
		}
		foreach (KeyValuePair<IPlayer, Dictionary<IPlayer, GClass413>> item in Dictionary_1)
		{
			GClass413 gClass = new GClass413(gamePerson, item.Key);
			List_0.Add(gClass);
			dictionary.Add(item.Key, gClass);
			item.Value.Add(gamePerson, gClass);
		}
		Dictionary_1.Add(gamePerson, dictionary);
	}

	public void Remove(IPlayer gamePerson)
	{
		Bool_0 = true;
		HashSet_0.Add(gamePerson);
		method_4();
	}

	[CanBeNull]
	public GClass413 GetClosest(IPlayer gamePerson)
	{
		if (Dictionary_1.TryGetValue(gamePerson, out var value))
		{
			GClass413 result = null;
			float num = float.MaxValue;
			{
				foreach (KeyValuePair<IPlayer, GClass413> item in value)
				{
					if (item.Value.CanInteract && item.Value.Dist < num)
					{
						num = item.Value.Dist;
						result = item.Value;
					}
				}
				return result;
			}
		}
		return null;
	}

	public List<GClass413> GetClosest(IPlayer gamePerson, float dist)
	{
		if (Dictionary_1.TryGetValue(gamePerson, out var value))
		{
			List<GClass413> list = new List<GClass413>();
			{
				foreach (KeyValuePair<IPlayer, GClass413> item in value)
				{
					if (item.Value.Dist < dist)
					{
						list.Add(item.Value);
					}
				}
				return list;
			}
		}
		return null;
	}

	public void method_0()
	{
		if (Bool_1)
		{
			method_4();
			ManualUpdate();
		}
	}

	public void method_1(BotOwner boss)
	{
		foreach (GClass413 item in List_0)
		{
			if (item.Person1 == boss)
			{
				method_2(item, boss);
			}
			else if (item.Person2 == boss)
			{
				method_2(item, boss);
			}
		}
	}

	public void method_2(GClass413 botsPairData, BotOwner boss)
	{
		if (botsPairData.Person1 != boss)
		{
			return;
		}
		foreach (BotOwner follower in boss.Boss.Followers)
		{
			if (botsPairData.Person2 == follower)
			{
				botsPairData.StopPosibleInteractions();
			}
		}
	}

	public void method_3(BotOwner follower, BotOwner boss, FollowerStatusChange status)
	{
		if (status != FollowerStatusChange.Add)
		{
			return;
		}
		foreach (GClass413 item in List_0)
		{
			if (item.Person1 == follower && item.Person2 == boss)
			{
				item.StopPosibleInteractions();
			}
			else if (item.Person2 == follower && item.Person1 == boss)
			{
				item.StopPosibleInteractions();
			}
		}
	}

	public void method_4()
	{
		if (!Bool_0)
		{
			return;
		}
		foreach (IPlayer item in HashSet_0)
		{
			foreach (KeyValuePair<IPlayer, Dictionary<IPlayer, GClass413>> item2 in Dictionary_1)
			{
				item2.Value.Remove(item);
			}
			if (!Dictionary_1.ContainsKey(item))
			{
				continue;
			}
			foreach (KeyValuePair<IPlayer, GClass413> item3 in Dictionary_1[item])
			{
				List_0.Remove(item3.Value);
			}
			Dictionary_1.Remove(item);
		}
		Bool_0 = false;
		HashSet_0.Clear();
	}
}
