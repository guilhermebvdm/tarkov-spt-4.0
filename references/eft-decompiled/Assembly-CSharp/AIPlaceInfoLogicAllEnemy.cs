using System.Collections.Generic;
using EFT;

public class AIPlaceInfoLogicAllEnemy : AIPlaceInfoLogic
{
	public List<WildSpawnType> EnemyList = new List<WildSpawnType>();

	private bool bool_0;

	private readonly HashSet<WildSpawnType> hashSet_0 = new HashSet<WildSpawnType>();

	private AIPlaceInfo aiplaceInfo_0;

	private BotsController botsController_0;

	private readonly List<BotOwner> list_0 = new List<BotOwner>();

	public override void Init(AIPlaceInfo aiPlaceInfo, BotsController botsController)
	{
		botsController_0 = botsController;
		aiplaceInfo_0 = aiPlaceInfo;
		bool_0 = EnemyList.Count > 0;
		foreach (WildSpawnType enemy in EnemyList)
		{
			hashSet_0.Add(enemy);
		}
		foreach (BotOwner botOwner in botsController_0.Bots.BotOwners)
		{
			if (hashSet_0.Contains(botOwner.Profile.Info.Settings.Role))
			{
				list_0.Add(botOwner);
			}
		}
		aiplaceInfo_0.OnPlayerEnter += method_2;
		botsController_0.Bots.OnBotAdd += method_1;
		botsController_0.Bots.OnBotRemove += method_0;
		base.Init(aiPlaceInfo, botsController);
	}

	public void method_0(BotOwner botOwner)
	{
		list_0.Remove(botOwner);
	}

	public void method_1(BotOwner botOwner)
	{
		if (hashSet_0.Contains(botOwner.Profile.Info.Settings.Role))
		{
			list_0.Add(botOwner);
		}
	}

	public void method_2(Player player)
	{
		if (!bool_0)
		{
			return;
		}
		foreach (BotOwner item in list_0)
		{
			item.BotsGroup.CheckAndAddEnemy(player);
		}
	}

	public new void Dispose()
	{
		if (aiplaceInfo_0 != null)
		{
			aiplaceInfo_0.OnPlayerEnter -= method_2;
		}
		if (botsController_0 != null)
		{
			botsController_0.Bots.OnBotAdd -= method_1;
			botsController_0.Bots.OnBotRemove -= method_0;
		}
	}
}
