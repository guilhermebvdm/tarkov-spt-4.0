using System;
using EFT.Bots;
using UnityEngine;

public abstract class GClass1896
{
	public static BotDifficulty smethod_0()
	{
		Array values = Enum.GetValues(typeof(BotDifficulty));
		return (BotDifficulty)values.GetValue(UnityEngine.Random.Range(0, values.Length));
	}

	public static BotDifficulty ToBotDifficulty(this EBotDifficulty botDifficulty)
	{
		return botDifficulty switch
		{
			EBotDifficulty.Easy => BotDifficulty.easy, 
			EBotDifficulty.Medium => BotDifficulty.normal, 
			EBotDifficulty.Hard => BotDifficulty.hard, 
			EBotDifficulty.Impossible => BotDifficulty.impossible, 
			EBotDifficulty.Random => smethod_0(), 
			_ => BotDifficulty.normal, 
		};
	}
}
