using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Comfort.Common;
using EFT;
using EFT.Game.Spawning;
using EFT.Quests;

public class GClass675
{
	[NonSerialized]
	public List<BossLocationSpawn> List_0 = new List<BossLocationSpawn>();

	[NonSerialized]
	public GClass641.IBotTimer IBotTimer;

	[NonSerialized]
	public IPlayersCollection IplayersCollection_0;

	[NonSerialized]
	public Action<BossLocationSpawn> Action_0;

	public float Single_0 => 360f;

	public GClass675(Action<BossLocationSpawn> activateWave)
	{
		Action_0 = activateWave;
	}

	public void Run()
	{
		IplayersCollection_0 = Singleton<GameWorld>.Instance;
		IplayersCollection_0.OnPersonAdd += method_0;
		method_2();
	}

	public void AddWave(BossLocationSpawn wave)
	{
		if (IBotTimer == null)
		{
			IBotTimer = StaticManager.Instance.TimerManager.MakeTimer(TimeSpan.FromSeconds(Single_0), isLopped: true);
			IBotTimer.OnTimer += method_2;
		}
		List_0.Add(wave);
		method_2();
	}

	public void method_0(IPlayer playerToTest)
	{
		try
		{
			method_3(playerToTest);
		}
		catch (Exception)
		{
		}
	}

	public bool method_1(BossLocationSpawn wave, string playerQuestId)
	{
		if ((string.IsNullOrEmpty(wave.TriggerId) || wave.TriggerId == playerQuestId) && !wave.Activated)
		{
			Action_0(wave);
			return true;
		}
		return false;
	}

	public void method_2()
	{
		foreach (IPlayer item in IplayersCollection_0.ToList())
		{
			method_3(item);
		}
	}

	public void method_3(IPlayer playerToTest)
	{
		bool flag = GClass398.Instance.IsTraceEnable();
		if (playerToTest.IsAI)
		{
			return;
		}
		Player alivePlayerByProfileID = Singleton<GameWorld>.Instance.GetAlivePlayerByProfileID(playerToTest.ProfileId);
		if (alivePlayerByProfileID == null || alivePlayerByProfileID.AbstractQuestControllerClass == null)
		{
			return;
		}
		List<QuestClass> list = alivePlayerByProfileID.AbstractQuestControllerClass.Quests.Where(alivePlayerByProfileID.AbstractQuestControllerClass.IsQuestForCurrentProfile).ToList();
		StringBuilder stringBuilder = new StringBuilder();
		if (flag)
		{
			List<QuestClass> list2 = alivePlayerByProfileID.AbstractQuestControllerClass.Quests.ToList();
			stringBuilder.Append($"AI checking quests for {playerToTest.Profile.Nickname}  quests:{list.Count} [ALL QUESTS:{list2.Count}");
			foreach (QuestClass item in list2)
			{
				stringBuilder.Append(" " + item.Id + " ");
			}
			stringBuilder.Append("]");
		}
		foreach (QuestClass item2 in list)
		{
			if (item2.QuestStatus != EQuestStatus.Started)
			{
				continue;
			}
			if (flag)
			{
				stringBuilder.Append(" " + item2.Id + " name:" + item2.QuestTypeName + " waveQuest:");
			}
			for (int i = 0; i < List_0.Count; i++)
			{
				BossLocationSpawn bossLocationSpawn = List_0[i];
				if (flag)
				{
					stringBuilder.Append(" (" + bossLocationSpawn.BossZone + "  " + bossLocationSpawn.BossEscortType + ") ");
				}
				if (method_1(bossLocationSpawn, item2.Id))
				{
					List_0.Remove(bossLocationSpawn);
					i--;
				}
			}
		}
	}

	public void Dispose()
	{
		if (IplayersCollection_0 != null)
		{
			IplayersCollection_0.OnPersonAdd -= method_0;
		}
	}
}
