using System.Collections.Generic;
using System.Runtime.CompilerServices;
using EFT.Bots;
using UnityEngine;

namespace EFT;

public class NonWavesSpawnScenario : MonoBehaviour
{
	private AbstractGame abstractGame_0;

	private LocationSettingsClass.Location location_0;

	private BotsController botsController_0;

	private GClass1881<BotDifficulty> gclass1881_0;

	private GClass1881<WildSpawnType> gclass1881_1;

	private GClass1876 gclass1876_0;

	private bool bool_0;

	private bool bool_1;

	private bool bool_2;

	private float? nullable_0;

	private float float_0;

	private const float float_1 = 10f;

	private float float_2 = 10f;

	[CompilerGenerated]
	private WaveInfoClass[] gclass1879_0;

	[CompilerGenerated]
	private bool bool_3;

	public WaveInfoClass[] GClass1879_0
	{
		[CompilerGenerated]
		get
		{
			return gclass1879_0;
		}
		[CompilerGenerated]
		set
		{
			gclass1879_0 = value;
		}
	}

	public bool Enabled
	{
		[CompilerGenerated]
		get
		{
			return bool_3;
		}
		[CompilerGenerated]
		set
		{
			bool_3 = value;
		}
	}

	public int BotMax
	{
		get
		{
			BotsController botsController = botsController_0;
			if (botsController != null && botsController.EventsController?.BotHalloweenWithZombies?.InfectionPercentage > 0)
			{
				return (int)((float)location_0.BotMax * (1f - location_0.Events.Halloween2024.CalcRealInfectionLevel()));
			}
			return location_0.BotMax;
		}
	}

	public static NonWavesSpawnScenario smethod_0(AbstractGame game, LocationSettingsClass.Location location, BotsController botsController)
	{
		NonWavesSpawnScenario nonWavesSpawnScenario = game.gameObject.AddComponent<NonWavesSpawnScenario>();
		nonWavesSpawnScenario.abstractGame_0 = game;
		nonWavesSpawnScenario.location_0 = location;
		nonWavesSpawnScenario.gclass1876_0 = new GClass1876(location.NonWaveGroupScenario);
		nonWavesSpawnScenario.botsController_0 = botsController;
		nonWavesSpawnScenario.method_0();
		if (location.NewSpawn)
		{
			nonWavesSpawnScenario.method_1();
		}
		return nonWavesSpawnScenario;
	}

	public void Run()
	{
		if (location_0.NewSpawn && botsController_0 != null)
		{
			bool_1 = true;
			if (location_0.BotSpawnCountStep < 0)
			{
				location_0.BotSpawnCountStep = 0;
			}
		}
	}

	public void Stop()
	{
		bool_1 = false;
	}

	public void Update()
	{
		if (!bool_1 || abstractGame_0.PastTime < (float)location_0.BotStart || !(abstractGame_0.PastTime <= (float)location_0.BotStop))
		{
			return;
		}
		if (!nullable_0.HasValue || nullable_0.Value <= abstractGame_0.PastTime)
		{
			bool_2 = !bool_2;
			nullable_0 = abstractGame_0.PastTime + (bool_2 ? GClass856.Random(location_0.BotSpawnTimeOnMin, location_0.BotSpawnTimeOnMax) : GClass856.Random(location_0.BotSpawnTimeOffMin, location_0.BotSpawnTimeOffMax));
		}
		if (!bool_2 || abstractGame_0.PastTime - float_0 < float_2)
		{
			return;
		}
		float_0 = abstractGame_0.PastTime;
		int num = BotMax - botsController_0.AliveLoadingDelayedBotsCount;
		if (bool_0)
		{
			if (num < location_0.BotSpawnCountStep)
			{
				return;
			}
			float_2 = 10f;
			bool_0 = false;
		}
		else if (num <= 0)
		{
			if (!bool_0)
			{
				float_2 = location_0.BotSpawnPeriodCheck;
				if (float_2 < 10f)
				{
					float_2 = 10f;
				}
				bool_0 = BotMax - botsController_0.AliveAndLoadingBotsCount <= 0;
			}
			return;
		}
		num = gclass1876_0.TrySpawn(num, botsController_0, gclass1881_0);
		for (int i = 0; i < num; i++)
		{
			WildSpawnType role = gclass1881_1.Random();
			BotDifficulty botDifficulty = gclass1881_0.Random();
			BotProfileDataClass data = new BotProfileDataClass(EPlayerSide.Savage, role, botDifficulty, 0f);
			botsController_0.ActivateBotsWithoutWave(1, data);
		}
	}

	public void method_0()
	{
		gclass1881_0 = new GClass1881<BotDifficulty>(new KeyValuePair<BotDifficulty, float>(BotDifficulty.easy, location_0.BotEasy), new KeyValuePair<BotDifficulty, float>(BotDifficulty.normal, location_0.BotNormal), new KeyValuePair<BotDifficulty, float>(BotDifficulty.hard, location_0.BotHard), new KeyValuePair<BotDifficulty, float>(BotDifficulty.impossible, location_0.BotImpossible));
		gclass1881_1 = new GClass1881<WildSpawnType>(new KeyValuePair<WildSpawnType, float>(WildSpawnType.assault, location_0.BotAssault), new KeyValuePair<WildSpawnType, float>(WildSpawnType.marksman, location_0.BotMarksman));
	}

	public void method_1()
	{
		Enabled = true;
		if (gclass1881_1.Count == 0)
		{
			gclass1881_1.Add(1f, WildSpawnType.assault);
		}
		_ = BotMax;
		method_2();
	}

	public void method_2()
	{
		List<WaveInfoClass> list = new List<WaveInfoClass>();
		for (int i = 0; i < BotMax; i++)
		{
			WaveInfoClass item = new WaveInfoClass(2, gclass1881_1.Random(), gclass1881_0.Random());
			list.Add(item);
		}
		List<WildSpawnType> list2 = new List<WildSpawnType>
		{
			WildSpawnType.assault,
			WildSpawnType.marksman
		};
		foreach (KeyValuePair<float, BotDifficulty> item3 in gclass1881_0)
		{
			foreach (WildSpawnType item4 in list2)
			{
				WaveInfoClass item2 = new WaveInfoClass(2, item4, item3.Value);
				list.Add(item2);
			}
		}
		list.Add(new WaveInfoClass(8, WildSpawnType.assault, BotDifficulty.normal));
		list.Add(new WaveInfoClass(3, WildSpawnType.marksman, BotDifficulty.normal));
		GClass1879_0 = list.ToArray();
	}

	public void ImplementWaveSettings(WavesSettings wavesSettings)
	{
		switch (wavesSettings.BotDifficulty)
		{
		default:
			gclass1881_0 = new GClass1881<BotDifficulty>(new KeyValuePair<BotDifficulty, float>(BotDifficulty.easy, location_0.BotEasy), new KeyValuePair<BotDifficulty, float>(BotDifficulty.normal, location_0.BotNormal), new KeyValuePair<BotDifficulty, float>(BotDifficulty.hard, location_0.BotHard), new KeyValuePair<BotDifficulty, float>(BotDifficulty.impossible, location_0.BotImpossible));
			break;
		case EBotDifficulty.Easy:
			gclass1881_0 = new GClass1881<BotDifficulty> { 
			{
				1f,
				BotDifficulty.easy
			} };
			break;
		case EBotDifficulty.Medium:
			gclass1881_0 = new GClass1881<BotDifficulty> { 
			{
				1f,
				BotDifficulty.normal
			} };
			break;
		case EBotDifficulty.Hard:
			gclass1881_0 = new GClass1881<BotDifficulty> { 
			{
				1f,
				BotDifficulty.hard
			} };
			break;
		case EBotDifficulty.Impossible:
			gclass1881_0 = new GClass1881<BotDifficulty> { 
			{
				1f,
				BotDifficulty.impossible
			} };
			break;
		case EBotDifficulty.Random:
			gclass1881_0 = new GClass1881<BotDifficulty>(new KeyValuePair<BotDifficulty, float>(BotDifficulty.easy, GClass856.Random(0f, 1f)), new KeyValuePair<BotDifficulty, float>(BotDifficulty.normal, GClass856.Random(0f, 1f)), new KeyValuePair<BotDifficulty, float>(BotDifficulty.hard, GClass856.Random(0f, 1f)), new KeyValuePair<BotDifficulty, float>(BotDifficulty.impossible, GClass856.Random(0f, 1f)));
			break;
		}
		method_2();
	}
}
