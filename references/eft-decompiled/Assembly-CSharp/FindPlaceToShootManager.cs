using System;
using System.Collections.Generic;
using EFT;

public class FindPlaceToShootManager(BotOwner owner) : GClass429(owner)
{
	[NonSerialized]
	public Dictionary<int, GClass483> Finders = new Dictionary<int, GClass483>();

	public GClass483 Register(int dist, int minDist, float periodCheck)
	{
		if (Finders.TryGetValue(dist, out var value))
		{
			return value;
		}
		GClass483 gClass = new GClass483(BotOwner_0, dist, minDist, periodCheck);
		Finders.Add(dist, gClass);
		return gClass;
	}
}
