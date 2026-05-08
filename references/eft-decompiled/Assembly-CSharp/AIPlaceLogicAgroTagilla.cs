using System.Collections.Generic;
using Comfort.Common;
using EFT;

public class AIPlaceLogicAgroTagilla : AIPlaceInfoLogic
{
	private AIPlaceInfo aiplaceInfo_0;

	public HashSet<IPlayer> IdsOfVisitedPlayers = new HashSet<IPlayer>();

	private float float_0;

	public bool IsEnableTimeTest;

	public bool _isEventStopped;

	private GClass25 gclass25_0;

	private const string string_0 = "alarm_stop";

	private const string string_1 = "alarm";

	private bool bool_0;

	public override void Init(AIPlaceInfo aiPlaceInfo, BotsController botsController)
	{
		gclass25_0 = new GClass25(10f, method_1);
		base.Init(aiPlaceInfo, botsController);
		aiplaceInfo_0 = aiPlaceInfo;
		_ = botsController.CoversData.AIPlaceInfoHolder.Places;
		IsEnableTimeTest = true;
		Singleton<BotEventHandler>.Instance.OnEvent += method_0;
		aiplaceInfo_0.OnPlayerEnter += method_3;
		aiplaceInfo_0.OnPlayerExit += method_2;
	}

	public void method_0(string obj)
	{
		if (!_isEventStopped && !bool_0)
		{
			if (obj.Equals("alarm"))
			{
				bool_0 = true;
			}
			else if (obj.Equals("alarm_stop"))
			{
				Singleton<BotEventHandler>.Instance.OnEvent -= method_0;
				_isEventStopped = true;
				IdsOfVisitedPlayers.Clear();
			}
		}
	}

	public void method_1()
	{
		foreach (BotOwner botOwner in Singleton<IBotGame>.Instance.BotsController.Bots.BotOwners)
		{
			if (botOwner.Profile.Info.Settings.Role != WildSpawnType.bossTagillaAgro)
			{
				continue;
			}
			foreach (IPlayer idsOfVisitedPlayer in IdsOfVisitedPlayers)
			{
				if (!botOwner.EnemiesController.IsEnemy(idsOfVisitedPlayer))
				{
					botOwner.BotsGroup.AddEnemy(idsOfVisitedPlayer, EBotEnemyCause.tagillaAlarm);
				}
				botOwner.BotsGroup.ReportAboutEnemy(idsOfVisitedPlayer, EEnemyPartVisibleType.Visible, botOwner);
			}
		}
	}

	public void method_2(Player player)
	{
	}

	public void Update()
	{
		if (!_isEventStopped)
		{
			gclass25_0?.Update();
		}
	}

	public void method_3(Player player)
	{
		if (!_isEventStopped && !player.IsAI)
		{
			IdsOfVisitedPlayers.Add(player);
		}
	}

	public override void Dispose()
	{
		Singleton<BotEventHandler>.Instance.OnEvent -= method_0;
		if (aiplaceInfo_0 != null)
		{
			aiplaceInfo_0.OnPlayerEnter -= method_3;
			aiplaceInfo_0.OnPlayerExit -= method_2;
		}
		base.Dispose();
	}
}
