using EFT;
using UnityEngine;

public class GClass1876
{
	public NonWaveGroupScenarioData _data;

	public GClass1876(NonWaveGroupScenarioData data)
	{
		if (data == null)
		{
			data = new NonWaveGroupScenarioData();
			data.Enabled = false;
		}
		_data = data;
	}

	public int TrySpawn(int botFreeSlotsCount, BotsController botsController, GClass1881<BotDifficulty> botDifficultiesWights)
	{
		if (_data.Enabled && botFreeSlotsCount >= _data.MinToBeGroup)
		{
			int num = (int)((float)botFreeSlotsCount / (float)_data.MaxToBeGroup);
			if (num < 1)
			{
				num = 1;
			}
			for (int i = 0; i < num; i++)
			{
				if (GClass856.IsTrue100(_data.Chance))
				{
					int num2 = GClass856.RandomInclude(_data.MinToBeGroup, Mathf.Min(botFreeSlotsCount, _data.MaxToBeGroup));
					botFreeSlotsCount -= num2;
					method_0(num2, botsController, botDifficultiesWights);
					break;
				}
			}
		}
		return botFreeSlotsCount;
	}

	public void method_0(int groupSize, BotsController botsController, GClass1881<BotDifficulty> botDifficultiesWights)
	{
		BotDifficulty botDifficulty = botDifficultiesWights.Random();
		BotProfileDataClass botProfileDataClass = new BotProfileDataClass(EPlayerSide.Savage, WildSpawnType.assault, botDifficulty, 0f);
		if (botProfileDataClass.SpawnParams == null)
		{
			botProfileDataClass.SpawnParams = new BotSpawnParams();
		}
		botProfileDataClass.SpawnParams.ShallBeGroup = new ShallBeGroupParams(group: true, bossGroup: true, groupSize);
		botsController.ActivateBotsWithoutWave(groupSize, botProfileDataClass);
	}
}
