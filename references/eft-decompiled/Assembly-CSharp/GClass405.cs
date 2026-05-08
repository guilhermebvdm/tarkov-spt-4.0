using System.Collections.Generic;
using EFT;

public class GClass405
{
	public bool autoRespawn;

	public int respawnCount;

	public int SummonSavages;

	public bool evenlyDistribute;

	public bool useStartWaves;

	public bool badVision;

	public bool badHearing;

	public bool spawnInstantly;

	public List<WildSpawnWave> StartWaves = new List<WildSpawnWave>();

	public List<BossLocationSpawn> BossStartWaves = new List<BossLocationSpawn>();

	public bool useThisData;

	public bool NoMinMax;

	public bool NoSleepMode;

	public bool TrueAim;

	public bool FreeForAll;

	public bool FreeForAllOverride;

	public bool PhraseRequestAlwaysGood;

	public bool UseDebugBrain;

	public DebugBotDesition DebugBotDesition;

	public GClass405()
	{
	}

	public GClass405(DebugBotData debugBotData)
	{
		autoRespawn = debugBotData.autoRespawn;
		useStartWaves = debugBotData.UseDebugWawes;
		badVision = debugBotData.BadVision;
		badHearing = debugBotData.BadHearing;
		respawnCount = debugBotData.respawnCount;
		StartWaves = debugBotData.StartWaves;
		BossStartWaves = debugBotData.BossStartWaves;
		useThisData = debugBotData.useThisData;
		evenlyDistribute = debugBotData.evenlyDistribute;
		spawnInstantly = debugBotData.spawnInstantly;
		NoMinMax = debugBotData.NoMinMax;
		NoSleepMode = debugBotData.NoSleepMode;
		TrueAim = debugBotData.TrueAim;
		DebugBotDesition = debugBotData.DebugBotDesition;
		UseDebugBrain = debugBotData.DebugBrain;
		SummonSavages = debugBotData.SummonSavages;
		PhraseRequestAlwaysGood = debugBotData.PhraseRequestAlwaysGood;
	}
}
