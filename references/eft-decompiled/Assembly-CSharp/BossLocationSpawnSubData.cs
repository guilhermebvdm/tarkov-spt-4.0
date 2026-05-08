using EFT;
using UnityEngine;

public class BossLocationSpawnSubData : MonoBehaviour
{
	public int BossEscortAmount;

	public WildSpawnType BossEscortType;

	public BotDifficulty EscortDifficulty;

	public BossLocationSpawnSubData(int v, WildSpawnType escortType, BotDifficulty difficulty)
	{
		BossEscortAmount = v;
		BossEscortType = escortType;
		EscortDifficulty = difficulty;
	}

	public WaveInfoClass GetTypesBotWave()
	{
		return new WaveInfoClass(BossEscortAmount, BossEscortType, EscortDifficulty);
	}
}
