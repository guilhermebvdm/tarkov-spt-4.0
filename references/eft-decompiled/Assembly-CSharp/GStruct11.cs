using System.Collections.Generic;
using BitPacking;
using UnityEngine;

public struct GStruct11
{
	public int Alive;

	public int ProfileWait;

	public int SpawnBundlesWait;

	public int Delayed;

	public int SpawnProcess;

	public int Hour;

	public int DelayedGroups;

	public GStruct23[] DelayDebugModels;

	public GStruct11(int alive, int profileWait, int delayed, int spawnProcess, int bundlesProcess, int hour, List<GInterface175> delayModels)
	{
		this = default(GStruct11);
		Hour = hour;
		Alive = alive;
		SpawnBundlesWait = bundlesProcess;
		ProfileWait = profileWait;
		Delayed = delayed;
		SpawnProcess = spawnProcess;
		DelayedGroups = delayModels.Count;
		DelayDebugModels = new GStruct23[DelayedGroups];
		for (int i = 0; i < delayModels.Count; i++)
		{
			GInterface175 gInterface = delayModels[i];
			gInterface.Data._profileData.TryGetRole(out var role, out var difficulty);
			DelayDebugModels[i] = new GStruct23(role, difficulty, gInterface.Data.Count);
		}
	}

	public static void Serialize(GStruct11 data, GInterface131 stream)
	{
		stream.WriteLimitedInt32(data.Alive, 0, 255, BitPackingTag.DebugBotSpawnStructDataAlive);
		stream.WriteLimitedInt32(Mathf.Clamp(data.ProfileWait, 0, 64), 0, 32, BitPackingTag.DebugBotSpawnStructDataProfileWait);
		stream.WriteLimitedInt32(data.Delayed, 0, 128, BitPackingTag.DebugBotSpawnStructDataDelayed);
		stream.WriteLimitedInt32(data.SpawnProcess, 0, 128, BitPackingTag.DebugBotSpawnStructDataSpawnProcess);
		stream.WriteLimitedInt32(data.SpawnBundlesWait, 0, 128, BitPackingTag.DebugBotSpawnStructDataSpawnBundlesWait);
		stream.WriteLimitedInt32(data.Hour, 0, 31, BitPackingTag.DebugBotSpawnStructDataSpawnHour);
		stream.WriteLimitedInt32(data.DelayedGroups, 0, 63, BitPackingTag.DebugBotSpawnStructDataGroupsCount);
		for (int i = 0; i < data.DelayedGroups; i++)
		{
			GStruct23.Serialize(data.DelayDebugModels[i], stream);
		}
	}

	public static GStruct11 Deserialize(IDataReader stream)
	{
		GStruct11 result = default(GStruct11);
		result.Alive = stream.ReadLimitedInt32(0, 255);
		result.ProfileWait = stream.ReadLimitedInt32(0, 32);
		result.Delayed = stream.ReadLimitedInt32(0, 128);
		result.SpawnProcess = stream.ReadLimitedInt32(0, 128);
		result.SpawnBundlesWait = stream.ReadLimitedInt32(0, 128);
		result.Hour = stream.ReadLimitedInt32(0, 31);
		result.DelayedGroups = stream.ReadLimitedInt32(0, 63);
		result.DelayDebugModels = new GStruct23[result.DelayedGroups];
		for (int i = 0; i < result.DelayedGroups; i++)
		{
			result.DelayDebugModels[i] = GStruct23.Deserialize(stream);
		}
		return result;
	}
}
