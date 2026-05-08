using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using EFT;
using UnityEngine;

public abstract class GClass378
{
	[CompilerGenerated]
	public class Class107
	{
		public WaveInfoClass countTypeBotWave;

		public bool method_0(WaveInfoClass x)
		{
			if (x.Role == countTypeBotWave.Role)
			{
				return x.Difficulty == countTypeBotWave.Difficulty;
			}
			return false;
		}
	}

	[CompilerGenerated]
	public class Class108
	{
		public WaveInfoClass botWave;

		public bool method_0(WaveInfoClass x)
		{
			if (x.Role == botWave.Role)
			{
				return x.Difficulty == botWave.Difficulty;
			}
			return false;
		}

		public bool method_1(WaveInfoClass x)
		{
			if (x.Role == botWave.Role)
			{
				return x.Difficulty == botWave.Difficulty;
			}
			return false;
		}
	}

	[CompilerGenerated]
	public class Class109
	{
		public WildSpawnType role;

		public BotDifficulty difficulty;

		public bool method_0(WaveInfoClass x)
		{
			if (x.Role == role)
			{
				return x.Difficulty == difficulty;
			}
			return false;
		}
	}

	[NonSerialized]
	public const int Int_0 = 55;

	[NonSerialized]
	public const int Int_1 = 30;

	[NonSerialized]
	public const int Int_2 = 10;

	public static List<WaveInfoClass> OptimizeBotWavesSimple(List<WaveInfoClass> wavesProfile)
	{
		List<WaveInfoClass> list = new List<WaveInfoClass>();
		foreach (WaveInfoClass countTypeBotWave in wavesProfile)
		{
			WaveInfoClass waveInfoClass = list.FirstOrDefault((WaveInfoClass x) => x.Role == countTypeBotWave.Role && x.Difficulty == countTypeBotWave.Difficulty);
			if (waveInfoClass != null)
			{
				waveInfoClass.Limit += countTypeBotWave.Limit;
			}
			else
			{
				list.Add(new WaveInfoClass(countTypeBotWave.Limit, countTypeBotWave.Role, countTypeBotWave.Difficulty));
			}
		}
		return list;
	}

	public static List<WaveInfoClass> OptimizeBotWaves(List<WaveInfoClass> wavesProfiles, out List<WaveInfoClass> delayed)
	{
		List<WaveInfoClass> list = new List<WaveInfoClass>();
		delayed = new List<WaveInfoClass>();
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		List<WaveInfoClass> list2 = new List<WaveInfoClass>();
		List<WaveInfoClass> list3 = new List<WaveInfoClass>();
		List<WaveInfoClass> list4 = new List<WaveInfoClass>();
		foreach (WaveInfoClass botWave in wavesProfiles)
		{
			WaveInfoClass waveInfoClass = list.FirstOrDefault((WaveInfoClass x) => x.Role == botWave.Role && x.Difficulty == botWave.Difficulty);
			List<WaveInfoClass> list5;
			if (BotSettingsRepoClass.IsBossOrFollower(botWave.Role))
			{
				if (BotSettingsRepoClass.IsMidPriorityBossOrFollower(botWave.Role))
				{
					list5 = list3;
					num3 += botWave.Limit;
				}
				else
				{
					list5 = list2;
					num2 += botWave.Limit;
				}
			}
			else
			{
				list5 = list4;
				num4 += botWave.Limit;
			}
			if (waveInfoClass == null)
			{
				if (botWave.Limit > 0)
				{
					waveInfoClass = new WaveInfoClass(botWave.Limit, botWave.Role, botWave.Difficulty);
					list.Add(waveInfoClass);
					list5.Add(new WaveInfoClass(botWave.Limit, botWave.Role, botWave.Difficulty));
					num += waveInfoClass.Limit;
				}
				continue;
			}
			waveInfoClass.Limit += botWave.Limit;
			WaveInfoClass waveInfoClass2 = list5.FirstOrDefault((WaveInfoClass x) => x.Role == botWave.Role && x.Difficulty == botWave.Difficulty);
			if (waveInfoClass2 != null)
			{
				waveInfoClass2.Limit += botWave.Limit;
			}
			num += botWave.Limit;
		}
		if (num2 + num3 + num4 <= 55)
		{
			smethod_0(list, delayed, 30);
			return list;
		}
		smethod_1(list4, delayed, WildSpawnType.marksman, 10, num2 + num3 + num4 - 55);
		num2 = 0;
		num3 = 0;
		num4 = 0;
		foreach (WaveInfoClass item in list2)
		{
			num2 += item.Limit;
		}
		foreach (WaveInfoClass item2 in list3)
		{
			num3 += item2.Limit;
		}
		foreach (WaveInfoClass item3 in list4)
		{
			num4 += item3.Limit;
		}
		num = num2 + num3 + num4;
		int targetDelta = num2 + num3 + num4 - 55;
		if (num3 <= 3)
		{
			float coeff = (float)(55 - num2 - num3) / (float)(num - num2 - num3);
			int delta = 0;
			smethod_2(list4, delayed, coeff, ref delta, ref targetDelta, alignLast: true);
		}
		else
		{
			float coeff2 = (float)(55 - num2) / (float)(num - num2);
			int delta2 = 0;
			smethod_2(list3, delayed, coeff2, ref delta2, ref targetDelta, alignLast: false);
			smethod_2(list4, delayed, coeff2, ref delta2, ref targetDelta, alignLast: true);
		}
		list.Clear();
		list.AddRange(list4);
		list.AddRange(list3);
		list.AddRange(list2);
		smethod_0(list, delayed, 30);
		foreach (WaveInfoClass item4 in list)
		{
			item4.Limit = Mathf.Max(1, item4.Limit);
		}
		return list;
	}

	public static void smethod_0(List<WaveInfoClass> array, List<WaveInfoClass> delayed, int limit)
	{
		foreach (WaveInfoClass item in array)
		{
			if (item.Limit <= limit)
			{
				continue;
			}
			bool flag = false;
			for (int i = 0; i < delayed.Count; i++)
			{
				if (delayed[i].Role == item.Role && delayed[i].Difficulty == item.Difficulty)
				{
					int num = limit;
					if (item.Role == WildSpawnType.marksman)
					{
						num = 10;
					}
					delayed[i].Limit += item.Limit - num;
					item.Limit = num;
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				int num2 = limit;
				if (item.Role == WildSpawnType.marksman)
				{
					num2 = 10;
				}
				delayed.Add(new WaveInfoClass(item.Limit - num2, item.Role, item.Difficulty));
				item.Limit = num2;
			}
		}
	}

	public static void smethod_1(List<WaveInfoClass> array, List<WaveInfoClass> delayed, WildSpawnType role, int maxAmount, int maxReduce)
	{
		WaveInfoClass waveInfoClass = new WaveInfoClass(0, role, BotDifficulty.easy);
		WaveInfoClass waveInfoClass2 = new WaveInfoClass(0, role, BotDifficulty.normal);
		WaveInfoClass waveInfoClass3 = new WaveInfoClass(0, role, BotDifficulty.hard);
		WaveInfoClass waveInfoClass4 = new WaveInfoClass(0, role, BotDifficulty.impossible);
		List<WaveInfoClass> list = new List<WaveInfoClass>();
		foreach (WaveInfoClass item in array)
		{
			if (item.Role == role)
			{
				if (item.Difficulty == BotDifficulty.easy)
				{
					waveInfoClass = item;
					list.Add(waveInfoClass);
				}
				else if (item.Difficulty == BotDifficulty.normal)
				{
					waveInfoClass2 = item;
					list.Add(waveInfoClass2);
				}
				else if (item.Difficulty == BotDifficulty.hard)
				{
					waveInfoClass3 = item;
					list.Add(waveInfoClass3);
				}
				else if (item.Difficulty == BotDifficulty.impossible)
				{
					waveInfoClass4 = item;
					list.Add(waveInfoClass4);
				}
			}
		}
		int num = waveInfoClass.Limit + waveInfoClass2.Limit + waveInfoClass3.Limit + waveInfoClass4.Limit;
		if (num > maxAmount)
		{
			int targetDelta = Mathf.Min(maxReduce, num - maxAmount);
			int delta = 0;
			float coeff = (float)maxAmount / (float)num;
			smethod_2(list, delayed, coeff, ref delta, ref targetDelta, alignLast: true);
		}
	}

	public static void smethod_2(List<WaveInfoClass> array, List<WaveInfoClass> delayed, float coeff, ref int delta, ref int targetDelta, bool alignLast)
	{
		int num = 0;
		WaveInfoClass waveInfoClass;
		while (true)
		{
			if (num < array.Count)
			{
				waveInfoClass = array[num];
				if (alignLast && num == array.Count - 1)
				{
					break;
				}
				int num2 = Mathf.Clamp(waveInfoClass.Limit, 0, 55);
				delta += waveInfoClass.Limit - num2;
				smethod_3(waveInfoClass.Role, waveInfoClass.Difficulty, waveInfoClass.Limit - num2, delayed);
				waveInfoClass.Limit = num2;
				if (waveInfoClass.Limit == 0)
				{
					array.RemoveAt(num);
					num--;
				}
				num++;
				continue;
			}
			return;
		}
		int num3 = Mathf.Clamp(waveInfoClass.Limit, 0, 55);
		smethod_3(waveInfoClass.Role, waveInfoClass.Difficulty, num3, delayed);
		waveInfoClass.Limit -= num3;
	}

	public static void smethod_3(WildSpawnType role, BotDifficulty difficulty, int count, List<WaveInfoClass> delayed)
	{
		WaveInfoClass waveInfoClass = delayed.FirstOrDefault((WaveInfoClass x) => x.Role == role && x.Difficulty == difficulty);
		if (waveInfoClass == null)
		{
			waveInfoClass = new WaveInfoClass(count, role, difficulty);
			delayed.Add(waveInfoClass);
		}
		else
		{
			waveInfoClass.Limit += count;
		}
	}
}
