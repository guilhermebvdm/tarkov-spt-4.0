using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using EFT;

public class GClass687
{
	[NonSerialized]
	public WildSpawnType WildSpawnType_0;

	[NonSerialized]
	public BotDifficulty BotDifficulty_0;

	[NonSerialized]
	[CompilerGenerated]
	public EPlayerSide? Nullable_0;

	public EPlayerSide? Side
	{
		[CompilerGenerated]
		get
		{
			return Nullable_0;
		}
	}

	public Profile ChooseProfile(List<Profile> profiles2Select, bool withDelete)
	{
		List<Profile> list = profiles2Select.Where((Profile x) => x.Info.Side == Side && x.Info.Settings.Role == WildSpawnType_0 && x.Info.Settings.BotDifficulty == BotDifficulty_0).ToList();
		if (list.Count == 0)
		{
			return null;
		}
		Profile profile = GClass856.RandomElement(list);
		if (withDelete)
		{
			profiles2Select.Remove(profile);
		}
		return profile;
	}

	public WaveInfoClass[] PrepareToLoadBackend(int count)
	{
		WaveInfoClass waveInfoClass = new WaveInfoClass(count, WildSpawnType_0, BotDifficulty_0);
		return new WaveInfoClass[1] { waveInfoClass };
	}

	[CompilerGenerated]
	public bool method_0(Profile x)
	{
		if (x.Info.Side == Side && x.Info.Settings.Role == WildSpawnType_0)
		{
			return x.Info.Settings.BotDifficulty == BotDifficulty_0;
		}
		return false;
	}
}
