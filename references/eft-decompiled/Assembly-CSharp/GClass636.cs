using System;
using System.Collections.Generic;
using EFT;
using UnityEngine;

public class GClass636
{
	[NonSerialized]
	public List<NavMeshCutElement> List_0 = new List<NavMeshCutElement>();

	[NonSerialized]
	public BotsController BotsController_0;

	public Action<BotOwner, NavMeshCutElement> OnCutComplete;

	public void Init(BotsController botsController, BotZone[] botZones)
	{
		BotsController_0 = botsController;
		BotsController_0.Bots.OnBotRemove += method_0;
		foreach (BotZone botZone in botZones)
		{
			if (botZone.ZoneNavMeshCutters == null)
			{
				continue;
			}
			foreach (NavMeshCutGroup cutterGroup in botZone.ZoneNavMeshCutters.CutterGroups)
			{
				foreach (NavMeshCutElement element in cutterGroup._elements)
				{
					List_0.Add(element);
					element.Obstacle.carving = false;
				}
			}
		}
	}

	public void method_0(BotOwner bot)
	{
		Vector3 position = bot.Position;
		foreach (NavMeshCutElement item in List_0)
		{
			if (item.IsInside(position))
			{
				if (item.TryCut())
				{
					OnCutComplete?.Invoke(bot, item);
				}
				break;
			}
		}
	}

	public void Dispose()
	{
		BotsController_0.Bots.OnBotRemove -= method_0;
	}
}
