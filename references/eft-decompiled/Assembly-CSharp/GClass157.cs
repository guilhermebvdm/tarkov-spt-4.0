using System;
using System.Collections.Generic;
using EFT;
using UnityEngine;

public abstract class GClass157 : BaseLogicLayerSimpleAbstractClass
{
	[NonSerialized]
	public GClass459 Gclass459_0;

	[NonSerialized]
	public float Float_3;

	[NonSerialized]
	public const float Float_4 = 3f;

	[NonSerialized]
	public int Int_1;

	[NonSerialized]
	public bool Bool_4;

	[NonSerialized]
	public List<EnemyInfo> List_0 = new List<EnemyInfo>();

	public GClass157(BotOwner bot, int priority)
		: base(bot, priority)
	{
		Gclass459_0 = new GClass459(bot);
	}

	public int method_13(out bool anywayAttack, bool resetCache = false)
	{
		if (Float_3 + 3f > Time.time && !resetCache)
		{
			anywayAttack = Bool_4;
			return Int_1;
		}
		int num = 0;
		List_0.Clear();
		anywayAttack = false;
		foreach (KeyValuePair<IPlayer, EnemyInfo> enemyInfo in BotOwner_0.EnemiesController.EnemyInfos)
		{
			if (Mathf.Abs(enemyInfo.Value.CurrPosition.y - BotOwner_0.Position.y) < BotOwner_0.Settings.FileSettings.Boss.KILLA_Y_DELTA_TO_BE_ENEMY_BOSS)
			{
				bool num2 = enemyInfo.Value.Distance < BotOwner_0.Settings.FileSettings.Boss.KILLA_DITANCE_TO_BE_ENEMY_BOSS;
				if (enemyInfo.Value.Distance < BotOwner_0.Settings.FileSettings.Boss.KILLA_ONE_IS_CLOSE)
				{
					anywayAttack = true;
				}
				if (num2)
				{
					num++;
					List_0.Add(enemyInfo.Value);
				}
			}
		}
		Float_3 = Time.time;
		Int_1 = num;
		Bool_4 = anywayAttack;
		return num;
	}
}
