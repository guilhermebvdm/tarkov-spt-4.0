using EFT;

public class WaveInfoClass
{
	public WildSpawnType Role;

	public int Limit;

	public BotDifficulty Difficulty;

	public WaveInfoClass(int count, WildSpawnType roleType, BotDifficulty difficulty)
	{
		Limit = count;
		Difficulty = difficulty;
		Role = roleType;
	}

	public WaveInfoClass Copy()
	{
		return new WaveInfoClass(Limit, Role, Difficulty);
	}

	public string DebugData()
	{
		return $"Role:{Role.ToString()} Limit:{Limit}  Duf:{Difficulty.ToString()}";
	}
}
