using System;
using System.Collections.Generic;
using EFT;
using UnityEngine;

public class GClass573
{
	[NonSerialized]
	public const float Float_0 = 3600f;

	[NonSerialized]
	public float Float_1;

	[NonSerialized]
	public BotZoneDangerAreas BotZoneDangerAreas_0;

	[NonSerialized]
	public BotsGroup BotsGroup_0;

	[NonSerialized]
	public List<MiniAssaultBotsGroup> List_0 = new List<MiniAssaultBotsGroup>();

	[NonSerialized]
	public bool Bool_0;

	public int DangerZonesCount => BotZoneDangerAreas_0.ActiveAreas.Count;

	public int AssaultGroupsCount => List_0.Count;

	public GClass573(BotZoneDangerAreas botZoneDangerAreas, BotsGroup group)
	{
		BotsGroup_0 = group;
		BotZoneDangerAreas_0 = botZoneDangerAreas;
		Bool_0 = LocalBotSettingsProviderClass.Core.CAN_ASSAULT_DANGER_ZONES;
	}

	public void ManualUpdate()
	{
		if (!Bool_0 || !(Time.time > Float_1))
		{
			return;
		}
		Float_1 = Time.time + 2f;
		if (BotZoneDangerAreas_0.ActiveAreas.Count <= 0)
		{
			return;
		}
		foreach (AIDangerArea activeArea in BotZoneDangerAreas_0.ActiveAreas)
		{
			method_0(activeArea);
		}
	}

	public void method_0(AIDangerArea botZoneDangerArea)
	{
		if (botZoneDangerArea.IsAssaultIsReady)
		{
			return;
		}
		List<BotOwner> list = new List<BotOwner>();
		for (int i = 0; i < BotsGroup_0.MembersCount; i++)
		{
			BotOwner botOwner = BotsGroup_0.Member(i);
			if (botOwner.Profile.Info.Settings.Role == WildSpawnType.assault || botOwner.Profile.Info.Settings.Role == WildSpawnType.cursedAssault)
			{
				Vector3 vector = botOwner.Position - botZoneDangerArea.Point;
				vector.y = 0f;
				if (vector.sqrMagnitude < 3600f)
				{
					list.Add(botOwner);
				}
			}
		}
		if (list.Count >= 3)
		{
			botZoneDangerArea.IsAssaultIsReady = true;
			List<BotOwner> list2 = new List<BotOwner>();
			for (int j = 0; j < list.Count; j++)
			{
				BotOwner item = list[j];
				list2.Add(item);
			}
			MiniAssaultBotsGroup item2 = new MiniAssaultBotsGroup(botZoneDangerArea, list2);
			List_0.Add(item2);
		}
	}

	public void Dispose()
	{
		foreach (MiniAssaultBotsGroup item in List_0)
		{
			item.Dispose();
		}
		List_0.Clear();
	}
}
