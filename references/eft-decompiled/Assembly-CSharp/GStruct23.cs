using BitPacking;
using EFT;

public struct GStruct23(WildSpawnType role, BotDifficulty difficulty, int count)
{
	public WildSpawnType Role = role;

	public BotDifficulty Difficulty = difficulty;

	public int Count = count;

	public static void Serialize(GStruct23 data, GInterface131 stream)
	{
		stream.WriteLimitedInt32((int)data.Role, 0, 127, BitPackingTag.SpawnDelayDebugModelRole);
		stream.WriteLimitedInt32((int)data.Difficulty, 0, 5, BitPackingTag.SpawnDelayDebugModelDifficulty);
		stream.WriteLimitedInt32(data.Count, 0, 127, BitPackingTag.SpawnDelayDebugModelCount);
	}

	public static GStruct23 Deserialize(IDataReader stream)
	{
		return new GStruct23
		{
			Role = (WildSpawnType)stream.ReadLimitedInt32(0, 127),
			Difficulty = (BotDifficulty)stream.ReadLimitedInt32(0, 5),
			Count = stream.ReadLimitedInt32(0, 127)
		};
	}

	public override string ToString()
	{
		return string.Format("[DelayGroup] {0}: {1} {2}: {3} {4}:{5}", "Role", Role, "Difficulty", Difficulty, "Count", Count.ToString());
	}
}
