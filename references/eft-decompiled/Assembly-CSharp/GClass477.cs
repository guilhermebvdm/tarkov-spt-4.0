using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Comfort.Common;
using EFT;
using JetBrains.Annotations;
using UnityEngine;

public class GClass477([NotNull] BotOwner owner) : BotEnemyChooser(owner)
{
	[NonSerialized]
	public HashSet<int> HashSet_0 = new HashSet<int>();

	[NonSerialized]
	public int Int_0 = -1;

	[NonSerialized]
	public float Float_0;

	public override void Activate()
	{
		base.Activate();
		HashSet_0 = BotOwner_0.BotsGroup.BotGame.BotsController.EventsController.BotHalloweenEvent.PlayersIdsToKill;
		if (GClass398.Instance.IsTraceEnable())
		{
			StringBuilder stringBuilder = new StringBuilder("RavangeZryachiyEnemiesChooser playersIdsToKill:");
			foreach (int item in HashSet_0)
			{
				stringBuilder.Append($" {item}");
			}
		}
		Float_0 = Time.time + 2f;
		HashSet<string> playersProfilesIdsToKill = BotOwner_0.BotsGroup.BotGame.BotsController.EventsController.BotHalloweenEvent.PlayersProfilesIdsToKill;
		foreach (string item2 in playersProfilesIdsToKill)
		{
			Player alivePlayerByProfileID = Singleton<GameWorld>.Instance.GetAlivePlayerByProfileID(item2);
			if (alivePlayerByProfileID != null)
			{
				BotOwner_0.BotsGroup.AddEnemy(alivePlayerByProfileID, EBotEnemyCause.ravangeZryachiy);
			}
		}
		if (playersProfilesIdsToKill.Count <= 0)
		{
			return;
		}
		string profileID = GClass856.RandomElement(playersProfilesIdsToKill);
		Player alivePlayerByProfileID2 = Singleton<GameWorld>.Instance.GetAlivePlayerByProfileID(profileID);
		if (alivePlayerByProfileID2 != null)
		{
			CustomNavigationPoint freeClosePoint = BotOwner_0.Covers.GetFreeClosePoint(alivePlayerByProfileID2.Position, 15f);
			if (freeClosePoint != null)
			{
				BotOwner_0.Mover.Teleport(freeClosePoint.Position);
			}
		}
	}

	public override EnemyInfo FindDangerEnemy()
	{
		bool isLogging = GClass398.Instance.IsTraceEnable();
		List<EnemyInfo> list = BotOwner_0.EnemiesController.EnemyInfos.Values.ToList();
		EnemyInfo enemyInfo;
		if (Float_0 > Time.time && HashSet_0.Count > 0)
		{
			List<EnemyInfo> list2 = list.Where((EnemyInfo x) => HashSet_0.Contains(x.Person.Id)).ToList();
			if (list2.Count > 0)
			{
				enemyInfo = BetterEnemy(list2, isLogging);
				if (enemyInfo != null && Int_0 != enemyInfo.Person.Id)
				{
					Int_0 = enemyInfo.Person.Id;
				}
				return enemyInfo;
			}
		}
		enemyInfo = BetterEnemy(list, isLogging);
		if (enemyInfo != null && Int_0 != enemyInfo.Person.Id)
		{
			Int_0 = enemyInfo.Person.Id;
		}
		return enemyInfo;
	}

	[CompilerGenerated]
	public bool method_0(EnemyInfo x)
	{
		return HashSet_0.Contains(x.Person.Id);
	}
}
