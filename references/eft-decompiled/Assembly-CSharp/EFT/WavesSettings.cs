using System;
using EFT.Bots;
using Newtonsoft.Json;

namespace EFT;

[Serializable]
public struct WavesSettings(EBotAmount botAmount, EBotDifficulty botDifficulty, bool isBosses, bool isTaggedAndCursed)
{
	[JsonProperty("botAmount")]
	public EBotAmount BotAmount = botAmount;

	[JsonProperty("botDifficulty")]
	public EBotDifficulty BotDifficulty = botDifficulty;

	[JsonProperty("isBosses")]
	public bool IsBosses = isBosses;

	[JsonProperty("isTaggedAndCursed")]
	public bool IsTaggedAndCursed = isTaggedAndCursed;
}
