using System;
using System.Collections.Generic;
using EFT;

public abstract class GClass628
{
	[NonSerialized]
	public static List<BotPersonalStats> List_0 = new List<BotPersonalStats>();

	[NonSerialized]
	public static bool Bool_0;

	[NonSerialized]
	public static GClass626 Gclass626_0 = new GClass626();

	[NonSerialized]
	public static GClass629 Gclass629_0 = new GClass629();

	public static void Enable(bool val = true)
	{
		Bool_0 = val;
	}

	public static void AddPlayer()
	{
		if (Gclass626_0 != null)
		{
			Gclass626_0.PlayersCount++;
		}
	}

	public static void DeadPlayer()
	{
		if (Gclass626_0 != null)
		{
			Gclass626_0.PlayersDead++;
		}
	}

	public static void WritetoFile()
	{
	}

	public static void AddBot(BotPersonalStats stat)
	{
		stat.Enable(Bool_0);
		List_0.Add(stat);
	}

	public static void SendCurrentStats(bool withDispose)
	{
		Gclass629_0.DoTotalLog();
	}

	public static List<string> smethod_0()
	{
		List<string> list = new List<string>();
		foreach (BotPersonalStats item in List_0)
		{
			list.Add(JsonParserClass.ToPrettyJson(item));
		}
		return list;
	}

	public static void AskBackendBotProfiles(List<WaveInfoClass> data, EProfilesAskingStat onStart)
	{
		Gclass629_0.AddProfileExteranl(data, onStart);
	}

	public static void smethod_1(Profile profile)
	{
		Gclass629_0.UseProfile(profile);
	}
}
