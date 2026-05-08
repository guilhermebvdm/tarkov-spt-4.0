using System;
using System.Collections.Generic;
using Comfort.Common;
using EFT;
using UnityEngine;

public class GClass438 : GClass437
{
	[NonSerialized]
	public const string String_2 = "AGRO_HELPERS_EVENT";

	[NonSerialized]
	public const string String_3 = "MASS_HELPERS_EVENT";

	[NonSerialized]
	public float Float_0 = 10f;

	[NonSerialized]
	public int Int_1;

	[NonSerialized]
	public GClass25 Gclass25_0;

	[NonSerialized]
	public GClass25 Gclass25_1;

	[NonSerialized]
	public List<AIMinePoint> List_0 = new List<AIMinePoint>();

	[NonSerialized]
	public const int Int_2 = 3;

	[NonSerialized]
	public const int Int_3 = 10;

	[NonSerialized]
	public const float Float_1 = 225f;

	public const float SDIST_SAFE_PREWARM = 16f;

	[NonSerialized]
	public bool Bool_3;

	[NonSerialized]
	public bool Bool_4;

	public GClass438(BotOwner owner, BotBoss bossLogic)
		: base(owner, bossLogic)
	{
		Gclass25_0 = new GClass25(5f, method_6);
		Gclass25_1 = new GClass25(4f, method_4);
		method_5();
	}

	public void method_4()
	{
		if (!Bool_4)
		{
			EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
			if (goalEnemy != null && (goalEnemy.IsVisible || Time.time - goalEnemy.TimeLastSeenReal < 1f) && BotOwner_0.BotsGroup.Enemies.Count >= 3)
			{
				Bool_4 = true;
				BotOwner_0.BotTalk.Say(EPhraseTrigger.LocateHostiles, sayImmediately: true);
				Singleton<BotEventHandler>.Instance.AnyEvent("MASS_HELPERS_EVENT");
			}
		}
	}

	public override void BossLogicUpdate()
	{
		Gclass25_1.Update();
		Gclass25_0.Update();
		if (List_0.Count > 0)
		{
			AIMinePoint aIMinePoint = List_0[0];
			List_0.Remove(aIMinePoint);
			if (!BotOwner_0.MinesData.PlantHereNow(aIMinePoint))
			{
				return;
			}
		}
		base.BossLogicUpdate();
	}

	public void method_5()
	{
		if (Bool_3)
		{
			return;
		}
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		Bool_3 = true;
		List<Player> allAlivePlayersList = Singleton<GameWorld>.Instance.AllAlivePlayersList;
		List<AIMinePoint> mines = BotOwner_0.BotsGroup.BotGame.BotsController.CoversData.AIMinesPositions.Mines;
		if (mines == null)
		{
			return;
		}
		int num = 0;
		foreach (AIMinePoint item in mines)
		{
			if (num > 10)
			{
				break;
			}
			if (goalEnemy != null && (item.Position - goalEnemy.CurrPosition).sqrMagnitude > 225f)
			{
				continue;
			}
			bool flag = true;
			foreach (Player item2 in allAlivePlayersList)
			{
				if (!item2.IsAI && !((item.Position - item2.Position).sqrMagnitude >= 16f))
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				List_0.Add(item);
				num++;
			}
		}
	}

	public void method_6()
	{
		if (Int_1 <= 0)
		{
			EnemyInfo enemyInfo = BotOwner_0.Memory.GoalEnemy;
			if (enemyInfo == null)
			{
				enemyInfo = BotOwner_0.Memory.LastEnemy;
			}
			if (enemyInfo != null && Time.time - enemyInfo.PersonalLastSeenTime > Float_0)
			{
				Int_1++;
				BotOwner_0.BotTalk.Say(EPhraseTrigger.LocateHostiles, sayImmediately: true);
				Singleton<BotEventHandler>.Instance.AnyEvent("AGRO_HELPERS_EVENT");
			}
		}
	}
}
