using System;
using System.Collections.Generic;
using EFT;
using UnityEngine.AI;

public class DeadBodiesController
{
	public List<GClass386> Bodies = new List<GClass386>(10);

	[NonSerialized]
	public Dictionary<BotsGroup, List<GClass386>> Bodiesgroups = new Dictionary<BotsGroup, List<GClass386>>(10);

	[NonSerialized]
	public BotZoneGroupsDictionary Groups = new BotZoneGroupsDictionary();

	public int BodiesCount => Bodies.Count;

	public DeadBodiesController(BotZoneGroupsDictionary groups)
	{
		Groups = groups;
	}

	public void RemoveBody(GClass386 body)
	{
		Bodies.Remove(body);
		foreach (List<GClass386> value in Bodiesgroups.Values)
		{
			value.Remove(body);
		}
	}

	public List<GClass386> BodiesByGroup(BotsGroup botsGroup)
	{
		if (!Bodiesgroups.ContainsKey(botsGroup))
		{
			Bodiesgroups.Add(botsGroup, new List<GClass386>());
		}
		return Bodiesgroups[botsGroup];
	}

	public void AddBody(BotsGroup botsGroup, GClass386 body)
	{
		if (!Bodiesgroups.TryGetValue(botsGroup, out var value))
		{
			value = new List<GClass386>();
			Bodiesgroups.Add(botsGroup, value);
		}
		value.Add(body);
	}

	public bool HaveBody(IPlayer player)
	{
		foreach (KeyValuePair<BotsGroup, List<GClass386>> bodiesgroup in Bodiesgroups)
		{
			List<GClass386> value = bodiesgroup.Value;
			if (value == null)
			{
				continue;
			}
			foreach (GClass386 item in value)
			{
				if (item.Player == player)
				{
					return true;
				}
			}
		}
		return false;
	}

	public void AddBody(IPlayer player)
	{
		if (HaveBody(player))
		{
			return;
		}
		List<BotsGroup> list = new List<BotsGroup>();
		foreach (KeyValuePair<BotZone, GClass575> group in Groups)
		{
			List<PatrolPoint> points = group.Key.PatrolWays[0].Points;
			for (int i = 0; i < points.Count; i++)
			{
				if (!((points[i].position - player.Transform.position).sqrMagnitude >= LocalBotSettingsProviderClass.Core.DIST_NOT_TO_GROUP_SQR))
				{
					list.AddRange(group.Value.GetGroups(notNull: true));
					break;
				}
			}
		}
		if (list.Count == 0)
		{
			return;
		}
		BotsGroup botsGroup = null;
		foreach (BotsGroup item in list)
		{
			NavMeshPath path = new NavMeshPath();
			if (NavMesh.CalculatePath(item.BotZone.PatrolWays[0].Points[0].position, player.Transform.position, -1, path))
			{
				botsGroup = item;
				break;
			}
		}
		if (botsGroup == null)
		{
			return;
		}
		GClass386 gClass = new GClass386(botsGroup, list, player.AIData.IsAI, player.Side, player.Transform.position, player);
		foreach (BotsGroup item2 in list)
		{
			AddBody(item2, gClass);
		}
		Bodies.Add(gClass);
	}
}
