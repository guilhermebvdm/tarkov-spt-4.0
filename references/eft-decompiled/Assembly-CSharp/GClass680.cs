using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using EFT;

public abstract class GClass680 : GInterface21
{
	public delegate Task<Profile[]> GDelegate9(List<WaveInfoClass> data, EProfilesAskingStat stat);

	[Serializable]
	[CompilerGenerated]
	public class Class352
	{
		public static readonly Class352 class352_0 = new Class352();

		public static Func<WaveInfoClass, int> func_0;

		public static Func<WaveInfoClass, int> func_1;

		public int method_0(WaveInfoClass x)
		{
			return x.Limit;
		}

		public int method_1(WaveInfoClass x)
		{
			return x.Limit;
		}
	}

	public const int BACKEND_MAX = 100;

	[NonSerialized]
	public List<Profile> List_0 = new List<Profile>();

	[NonSerialized]
	public bool Bool_0 = true;

	public bool StartProfilesLoaded => Bool_0;

	public abstract int BundlesLoading { get; }

	public GClass624<WildSpawnType, BotDifficulty, GClass679> CurCacheddProfilesCount()
	{
		GClass624<WildSpawnType, BotDifficulty, GClass679> gClass = new GClass624<WildSpawnType, BotDifficulty, GClass679>();
		foreach (Profile item in List_0)
		{
			if (gClass.TryGet(item.Info.Settings.Role, item.Info.Settings.BotDifficulty, out var val))
			{
				val.Val++;
			}
			else
			{
				gClass.Add(item.Info.Settings.Role, item.Info.Settings.BotDifficulty, new GClass679(1));
			}
		}
		return gClass;
	}

	public static List<List<WaveInfoClass>> DelemiterCountBotTypes(List<WaveInfoClass> listWaves)
	{
		List<WaveInfoClass> list = GClass855.ServerOptimizeCountWaves(listWaves);
		int num = list.Sum((WaveInfoClass x) => x.Limit);
		if (num <= 100)
		{
			return new List<List<WaveInfoClass>> { list };
		}
		List<List<WaveInfoClass>> list2 = new List<List<WaveInfoClass>>();
		List<WaveInfoClass> list3 = new List<WaveInfoClass>();
		foreach (WaveInfoClass item in list)
		{
			list3.Add(item.Copy());
		}
		int num2 = num;
		int num3 = 20;
		bool flag = true;
		while (flag && num3 > 0)
		{
			num3--;
			flag = false;
			List<WaveInfoClass> list4 = new List<WaveInfoClass>();
			int num4 = 100;
			foreach (WaveInfoClass item2 in list3)
			{
				if (item2.Limit != 0)
				{
					WaveInfoClass waveInfoClass = item2.Copy();
					if (item2.Limit >= num4)
					{
						waveInfoClass.Limit = num4;
						num4 -= waveInfoClass.Limit;
						list4.Add(waveInfoClass);
						list2.Add(list4);
						flag = true;
						item2.Limit -= waveInfoClass.Limit;
						num2 -= waveInfoClass.Limit;
						break;
					}
					num4 -= item2.Limit;
					list4.Add(waveInfoClass);
					item2.Limit = 0;
					num2 -= waveInfoClass.Limit;
					if (num2 <= 0)
					{
						list2.Add(list4);
						break;
					}
				}
			}
		}
		int num5 = 0;
		foreach (List<WaveInfoClass> item3 in list2)
		{
			num5 += item3.Sum((WaveInfoClass x) => x.Limit);
		}
		return list2;
	}

	public virtual void CreateProfile(EPlayerSide side, WildSpawnType type, BotDifficulty botDifficulty, Action<Profile> callback, bool withDelete)
	{
	}

	public virtual Task<Profile> CreateProfile(BotCreationDataClass data, CancellationToken cancellationToken, bool withDelete)
	{
		throw new InvalidOperationException();
	}

	public virtual void FillBackupProfilesData(GClass406 resultCache)
	{
		GClass624<WildSpawnType, BotDifficulty, GClass679> gClass = new GClass624<WildSpawnType, BotDifficulty, GClass679>();
		foreach (Profile item in List_0)
		{
			WildSpawnType role = item.Info.Settings.Role;
			BotDifficulty botDifficulty = item.Info.Settings.BotDifficulty;
			if (gClass.TryGet(role, botDifficulty, out var val))
			{
				val.Val++;
			}
			else
			{
				gClass.Add(role, botDifficulty, new GClass679(1));
			}
		}
		resultCache.Struct = new GClass407();
		resultCache.Struct.SetData(gClass);
	}

	public abstract void AddToTargetBackup(BotDifficulty difficulty, WildSpawnType role, int count);

	public Profile GetNewProfile()
	{
		if (List_0.Count > 0)
		{
			Profile profile = List_0[0];
			List_0.Remove(profile);
			return profile;
		}
		return null;
	}

	public virtual Profile GetNewProfile(BotCreationDataClass data, bool withDelete)
	{
		if (List_0.Count > 0)
		{
			return data.ChooseProfile(List_0, withDelete);
		}
		return null;
	}

	public GClass680()
	{
	}
}
