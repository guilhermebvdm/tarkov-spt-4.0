using System;
using System.Collections.Generic;
using EFT;

public class KhorovodBotGameEvent : GClass429
{
	[field: NonSerialized]
	public ChristmasTreePoI ChristmasTree { get; }

	[field: NonSerialized]
	public bool IsActive { get; set; }

	[field: NonSerialized]
	public bool InKhorovod { get; set; }

	public KhorovodBotGameEvent(BotOwner owner)
		: base(owner)
	{
		foreach (AIPlaceInfo place in BotOwner_0.BotsController.CoversData.AIPlaceInfoHolder.Places)
		{
			ChristmasTree = place.InfoLogicAllEnemy as ChristmasTreePoI;
			if (ChristmasTree != null && ChristmasTree.gameObject.activeInHierarchy)
			{
				IsActive = true;
				break;
			}
		}
	}

	public void OnEnterKhorovod()
	{
		if (InKhorovod)
		{
			return;
		}
		foreach (KeyValuePair<IPlayer, EnemyInfo> enemyInfo in BotOwner_0.EnemiesController.EnemyInfos)
		{
			enemyInfo.Value.SetIgnoreState();
		}
		InKhorovod = true;
	}
}
