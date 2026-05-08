using System;
using System.Collections.Generic;
using Comfort.Common;
using EFT;
using JetBrains.Annotations;

public class GClass478([NotNull] BotOwner owner) : BotEnemyChooser(owner)
{
	[NonSerialized]
	public string String_0;

	[NonSerialized]
	public Dictionary<EnemyInfo, bool> Dictionary_0 = new Dictionary<EnemyInfo, bool>();

	public override void Activate()
	{
		if (BotOwner_0.SpawnProfileData.SpawnParams.TriggerType == SpawnTriggerType.byQuest)
		{
			String_0 = BotOwner_0.SpawnProfileData.SpawnParams.Id_spawn;
		}
	}

	public override float CalcWeight(float dist, EnemyInfo enemyInfo)
	{
		float num = dist;
		bool value = false;
		if (!GClass856.IsNullOrEmpty(String_0) && !Dictionary_0.TryGetValue(enemyInfo, out value))
		{
			Player everExistedPlayerByID = Singleton<GameWorld>.Instance.GetEverExistedPlayerByID(enemyInfo.Person.ProfileId);
			if (everExistedPlayerByID != null && everExistedPlayerByID.AbstractQuestControllerClass != null)
			{
				value = everExistedPlayerByID.AbstractQuestControllerClass.Quests.GetConditional(String_0) != null;
				Dictionary_0.Add(enemyInfo, value);
			}
		}
		if (!value)
		{
			num += 10000f;
		}
		if (!enemyInfo.IsVisible)
		{
			num += 10000f;
		}
		if (!enemyInfo.CanShoot)
		{
			num += 10000f;
		}
		return num;
	}

	public override void Dispose()
	{
		Dictionary_0.Clear();
		base.Dispose();
	}
}
