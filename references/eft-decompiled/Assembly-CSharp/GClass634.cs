using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public class GClass634
{
	public Dictionary<int, GClass633> BotDistanceStat = new Dictionary<int, GClass633>();

	[NonSerialized]
	public int Int_0 = -1;

	[NonSerialized]
	[CompilerGenerated]
	public int Int_1;

	[NonSerialized]
	[CompilerGenerated]
	public int Int_2;

	public int TotalShots
	{
		[CompilerGenerated]
		get
		{
			return Int_1;
		}
		[CompilerGenerated]
		set
		{
			Int_1 = value;
		}
	}

	public int TotalHit
	{
		[CompilerGenerated]
		get
		{
			return Int_2;
		}
		[CompilerGenerated]
		set
		{
			Int_2 = value;
		}
	}

	public GClass634()
	{
		int num = 0;
		float[] dISTANCES = BotPersonalStats.DISTANCES;
		foreach (float distance in dISTANCES)
		{
			GClass633 gClass = new GClass633();
			BotDistanceStat.Add(num, gClass);
			gClass.distance = distance;
			num++;
		}
	}

	public void AddDistShot(float d)
	{
		int index = BotPersonalStats.GetIndex(d);
		BotDistanceStat[index].shoots++;
		TotalShots++;
	}

	public int AddDistHit(float d, string bone, float damage, int fireIndex, EBodyPart bodyType, float armorDamage)
	{
		int index = BotPersonalStats.GetIndex(d);
		GClass633 gClass = BotDistanceStat[index];
		int result = 0;
		gClass.damage += damage;
		gClass.armorDamage += armorDamage;
		if (fireIndex > Int_0)
		{
			gClass.hits++;
			result = gClass.hits;
		}
		gClass.BoneHit(bone, bodyType);
		Int_0 = fireIndex;
		TotalHit++;
		return result;
	}
}
