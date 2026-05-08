using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Comfort.Common;
using EFT;
using UnityEngine;

public class AIPlaceLogicPartisan : AIPlaceInfoLogic
{
	[Serializable]
	[CompilerGenerated]
	public class Class294
	{
		public static readonly Class294 class294_0 = new Class294();

		public static Func<BossLocationSpawn, bool> func_0;

		public bool method_0(BossLocationSpawn x)
		{
			return x.BossType == WildSpawnType.bossPartisan;
		}
	}

	private AIPlaceInfo aiplaceInfo_0;

	public const string PARTISAN_TRIGGER = "PARTISAN_TRIGGER";

	public HashSet<string> IdsOfVisitedPlayers = new HashSet<string>();

	private float float_0 = 35f;

	private float float_1 = 50f;

	private BotSettingsComponents botSettingsComponents_0;

	private static bool bool_0;

	private List<BossLocationSpawn> list_0;

	private bool bool_1;

	private float float_2;

	public bool IsEnableTimeTest;

	public override void Init(AIPlaceInfo aiPlaceInfo, BotsController botsController)
	{
		list_0 = Singleton<IBotGame>.Instance.BossSpawnScenario.BossSpawnWaves.Where((BossLocationSpawn x) => x.BossType == WildSpawnType.bossPartisan).ToList();
		bool_1 = list_0.Count > 0;
		base.Init(aiPlaceInfo, botsController);
		aiplaceInfo_0 = aiPlaceInfo;
		List<AIPlaceInfo> places = botsController.CoversData.AIPlaceInfoHolder.Places;
		bool flag = false;
		foreach (AIPlaceInfo item in places)
		{
			if (item.InfoLogicAllEnemy is AIPlaceLogicPartisan aIPlaceLogicPartisan && aIPlaceLogicPartisan != this && aIPlaceLogicPartisan.IsEnableTimeTest)
			{
				flag = true;
				break;
			}
		}
		if (flag)
		{
			IsEnableTimeTest = false;
		}
		else
		{
			IsEnableTimeTest = true;
		}
		aiplaceInfo_0.OnPlayerEnter += method_1;
		botSettingsComponents_0 = LocalBotSettingsProviderClass.GetSettings(BotDifficulty.normal, WildSpawnType.bossPartisan, botsController.IsPvE);
		if (!(aiplaceInfo_0 == null) && !bool_0)
		{
			bool_0 = true;
			StringBuilder stringBuilder = new StringBuilder();
			WildSpawnType[] eNEMY_BOT_TYPES = botSettingsComponents_0.Mind.ENEMY_BOT_TYPES;
			for (int num = 0; num < eNEMY_BOT_TYPES.Length; num++)
			{
				WildSpawnType wildSpawnType = eNEMY_BOT_TYPES[num];
				stringBuilder.Append(" " + wildSpawnType.ToString() + " ");
			}
		}
	}

	public void Update()
	{
		if (!bool_1 || !IsEnableTimeTest || !(float_2 < Time.time))
		{
			return;
		}
		float_2 = Time.time + 15f;
		IPlayer player = method_0();
		if (player == null)
		{
			return;
		}
		foreach (BossLocationSpawn item in list_0)
		{
			Vector3 vector = new Vector3(method_2(), 0f, method_2());
			Vector3 value = player.Position + vector;
			item.PerfectPos = value;
		}
	}

	public IPlayer method_0()
	{
		List<Player> allAlivePlayersList = Singleton<GameWorld>.Instance.AllAlivePlayersList;
		Player result = null;
		float num = float.MaxValue;
		foreach (Player item in allAlivePlayersList)
		{
			if (!item.IsAI)
			{
				float karmaValue = item.Profile.KarmaValue;
				if (karmaValue < num)
				{
					num = karmaValue;
					result = item;
				}
			}
		}
		return result;
	}

	public void method_1(Player player)
	{
		if (!GClass442.CanPersuit(player, botSettingsComponents_0))
		{
			return;
		}
		Vector3 position = player.Position;
		bool flag;
		if (flag = player.Profile.KarmaValue > 0.5f)
		{
			IPlayer player2 = method_0();
			if (player2 != null)
			{
				position = player2.Position;
			}
		}
		bool_1 = false;
		foreach (BossLocationSpawn item in list_0)
		{
			Vector3 vector = new Vector3(method_2(), 0f, method_2());
			Vector3 value = position + vector;
			item.PerfectPos = value;
		}
		if (!flag)
		{
			IdsOfVisitedPlayers.Add(player.ProfileId);
			IPlayer[] array = GClass3585.GroupPlayers(Singleton<GameWorld>.Instance.AllAlivePlayersList, player.GroupId, player.TeamId).ToArray();
			foreach (IPlayer player3 in array)
			{
				IdsOfVisitedPlayers.Add(player3.ProfileId);
			}
		}
		Singleton<BotEventHandler>.Instance.AnyEvent("PARTISAN_TRIGGER");
	}

	public float method_2()
	{
		return GClass856.Random(float_0, float_1) * (float)GClass856.RandomSing();
	}

	public override void Dispose()
	{
		if (aiplaceInfo_0 != null)
		{
			aiplaceInfo_0.OnPlayerEnter -= method_1;
		}
		base.Dispose();
	}
}
