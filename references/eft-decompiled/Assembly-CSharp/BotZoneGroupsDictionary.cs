using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using EFT;

public class BotZoneGroupsDictionary : Dictionary<BotZone, GClass575>
{
	[Serializable]
	[CompilerGenerated]
	public class Class271
	{
		public static readonly Class271 class271_0 = new Class271();

		public static Func<BotsGroup, int> func_0;

		public int method_0(BotsGroup x)
		{
			return x.MembersCount;
		}
	}

	[CompilerGenerated]
	private Action<BotsGroup> action_0;

	public event Action<BotsGroup> OnAddGroup
	{
		[CompilerGenerated]
		add
		{
			Action<BotsGroup> action = action_0;
			Action<BotsGroup> action2;
			do
			{
				action2 = action;
				Action<BotsGroup> value2 = (Action<BotsGroup>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<BotsGroup> action = action_0;
			Action<BotsGroup> action2;
			do
			{
				action2 = action;
				Action<BotsGroup> value2 = (Action<BotsGroup>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public bool TryGetValue(BotZone zone, EPlayerSide side, WildSpawnType spawnType, out BotsGroup group, bool isBossOrFollower)
	{
		if (TryGetValue(zone, out var value))
		{
			if (value.GetGroups(notNull: true).Sum((BotsGroup x) => x.MembersCount) >= zone.MaxPersons)
			{
				_ = zone.MaxPersons;
			}
			BotsGroup botsGroup = value.Group(isBossOrFollower, spawnType);
			if (botsGroup != null && !botsGroup.Locked)
			{
				group = botsGroup;
				return true;
			}
		}
		group = null;
		return false;
	}

	public void Add(BotZone zone, EPlayerSide side, BotsGroup gr, bool isBossOrFollower)
	{
		if (!TryGetValue(zone, out var value))
		{
			value = new GClass575();
			value.Set(side, gr, isBossOrFollower);
			Add(zone, value);
		}
		else
		{
			value.Set(side, gr, isBossOrFollower);
		}
		action_0?.Invoke(gr);
	}

	public void AddNoKey(BotsGroup gr, BotZone zone)
	{
		if (!TryGetValue(zone, out var value))
		{
			value = new GClass575();
			value.AddNoKey(gr);
			Add(zone, value);
		}
		else
		{
			value.AddNoKey(gr);
		}
		action_0?.Invoke(gr);
	}
}
