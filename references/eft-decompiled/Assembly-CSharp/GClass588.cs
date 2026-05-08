using System;
using EFT;
using UnityEngine;

public class GClass588 : GInterface12
{
	[NonSerialized]
	public NavGraphVoxelSimple NavGraphVoxelSimple_0;

	[NonSerialized]
	public AICoversData AicoversData_0;

	[NonSerialized]
	public BotOwner BotOwner_0;

	public GClass588(BotOwner bot)
	{
		BotOwner_0 = bot;
		EBotState botState = bot.BotState;
		if ((uint)(botState - 1) <= 1u)
		{
			AicoversData_0 = BotOwner_0.BotsGroup.BotGame.BotsController.CoversData;
		}
		BotOwner_0.OnBotStateChange += method_0;
	}

	public void method_0(EBotState obj)
	{
		if (obj == EBotState.PreActive)
		{
			AicoversData_0 = BotOwner_0.BotsGroup.BotGame.BotsController.CoversData;
		}
	}

	public void SetPos(Vector3 pos, bool withLogs = false)
	{
		NavGraphVoxelSimple voxelSafe = AicoversData_0.GetVoxelSafe(pos, withLogs);
		if (NavGraphVoxelSimple_0 == null || voxelSafe != NavGraphVoxelSimple_0)
		{
			if (withLogs)
			{
				Debug.LogError($"Voxel changed nextVoxel:{voxelSafe.Id}  {voxelSafe.Position}");
			}
			NavGraphVoxelSimple_0 = voxelSafe;
			BotOwner_0.VoxelesPersonalData.SetCurrectVoxel(NavGraphVoxelSimple_0);
		}
	}

	public void TryDrawAffectionGizmos()
	{
	}

	public void Dispose()
	{
		BotOwner_0.OnBotStateChange -= method_0;
	}
}
