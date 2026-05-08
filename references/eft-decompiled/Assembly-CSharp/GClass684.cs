using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using EFT;
using UnityEngine;

public class GClass684
{
	[Serializable]
	[CompilerGenerated]
	public class Class353
	{
		public static readonly Class353 class353_0 = new Class353();

		public static Func<WaveInfoClass, int> func_0;

		public static Func<WaveInfoClass, int> func_1;

		public static Func<WaveInfoClass, int> func_2;

		public static Func<WaveInfoClass, int> func_3;

		public int method_0(WaveInfoClass x)
		{
			return x.Limit;
		}

		public int method_1(WaveInfoClass x)
		{
			return x.Limit;
		}

		public int method_2(WaveInfoClass x)
		{
			return x.Limit;
		}

		public int method_3(WaveInfoClass x)
		{
			return x.Limit;
		}
	}

	[NonSerialized]
	public const int Int_0 = 3;

	[NonSerialized]
	public const int Int_1 = 8;

	[NonSerialized]
	public const int Int_2 = 10;

	[NonSerialized]
	public const float Float_0 = 2f;

	[NonSerialized]
	public const float Float_1 = 50f;

	[NonSerialized]
	public int Int_3;

	[NonSerialized]
	public int Int_4;

	[NonSerialized]
	public List<float> List_0 = new List<float>();

	[NonSerialized]
	public float Float_2;

	[NonSerialized]
	public float Float_3;

	[NonSerialized]
	public GClass680.GDelegate9 Gdelegate9_0;

	[NonSerialized]
	public GClass680 Gclass680_0;

	[NonSerialized]
	public Action<List<Profile>> Action_0;

	[NonSerialized]
	public List<WaveInfoClass> List_1 = new List<WaveInfoClass>();

	[NonSerialized]
	public GClass641.IBotTimer IBotTimer;

	[NonSerialized]
	public GClass641.IBotTimer IBotTimer_1;

	[NonSerialized]
	public GClass624<WildSpawnType, BotDifficulty, int> Gclass624_0 = new GClass624<WildSpawnType, BotDifficulty, int>();

	[NonSerialized]
	public float Float_4;

	[NonSerialized]
	public bool Bool_0 = true;

	public int LoadsProcess => Int_3;

	public int TotalBackups => Int_4;

	public List<float> PeriodsForBackups => List_0;

	public GClass684(GClass680 profileCreator, GClass680.GDelegate9 profileLoader, Action<List<Profile>> callback)
	{
		Gclass624_0.Add(WildSpawnType.assault, BotDifficulty.easy, 8);
		Gclass624_0.Add(WildSpawnType.assault, BotDifficulty.normal, 8);
		Gclass624_0.Add(WildSpawnType.assault, BotDifficulty.hard, 8);
		Gclass624_0.Add(WildSpawnType.marksman, BotDifficulty.easy, 8);
		Gclass624_0.Add(WildSpawnType.marksman, BotDifficulty.normal, 8);
		Gclass624_0.Add(WildSpawnType.marksman, BotDifficulty.hard, 8);
		Gclass680_0 = profileCreator;
		Gdelegate9_0 = profileLoader;
		Action_0 = callback;
		IBotTimer = StaticManager.Instance.TimerManager.MakeTimer(TimeSpan.FromSeconds(5.0), isLopped: true);
		IBotTimer.OnTimer += Update;
		IBotTimer_1 = StaticManager.Instance.TimerManager.MakeTimer(TimeSpan.FromSeconds(5.0), isLopped: true);
		IBotTimer_1.OnTimer += method_0;
		Float_2 = Time.time + 60f;
	}

	public void method_0()
	{
		if (!Bool_0 || Float_4 > Time.time)
		{
			return;
		}
		GClass624<WildSpawnType, BotDifficulty, GClass679> gClass = Gclass680_0.CurCacheddProfilesCount();
		int num = 0;
		StringBuilder stringBuilder = new StringBuilder("Cache results");
		stringBuilder.AppendLine();
		bool flag = GClass398.Instance.IsTraceEnable();
		foreach (GClass623<WildSpawnType, BotDifficulty, int> allVal in Gclass624_0.GetAllVals())
		{
			if (gClass.TryGet(allVal.PrimaryKey, allVal.SecondaryKey, out var val))
			{
				int num2 = allVal.Value - val.Val;
				if (num2 - 3 > 0)
				{
					num += num2;
					WaveInfoClass item = new WaveInfoClass(num2, allVal.PrimaryKey, allVal.SecondaryKey);
					if (flag)
					{
						stringBuilder.AppendLine($"Add to cache:{num2}  {allVal.PrimaryKey.ToString()}  {allVal.SecondaryKey.ToString()}");
					}
					List_1.Add(item);
				}
				else if (flag)
				{
					stringBuilder.AppendLine($"All fine to cache:{num2}  {allVal.PrimaryKey.ToString()}  {allVal.SecondaryKey.ToString()}");
				}
			}
			else
			{
				WaveInfoClass item2 = new WaveInfoClass(allVal.Value, allVal.PrimaryKey, allVal.SecondaryKey);
				List_1.Add(item2);
				if (flag)
				{
					stringBuilder.AppendLine($"Add to cache:{allVal.Value}  {allVal.PrimaryKey.ToString()}  {allVal.SecondaryKey.ToString()}");
				}
			}
		}
		if (flag)
		{
			_ = $"deltaTotal:{num} {stringBuilder.ToString()}";
		}
		if (num > 0)
		{
			Bool_0 = false;
		}
		Float_4 = Time.time + 30f;
		List_1 = GClass378.OptimizeBotWavesSimple(List_1);
		int num3 = List_1.Sum((WaveInfoClass x) => x.Limit);
		if (num3 <= 100)
		{
			return;
		}
		int num4 = 100 / num3;
		foreach (WaveInfoClass item3 in List_1)
		{
			item3.Limit = Mathf.Clamp(item3.Limit * num4, 1, 100);
		}
		List_1.Sum((WaveInfoClass x) => x.Limit);
	}

	public void AddProfileForBackup(BotCreationDataClass data)
	{
		WaveInfoClass[] collection = data.PrepareToLoadBackend(1);
		List_1.AddRange(collection);
		List_1 = GClass378.OptimizeBotWavesSimple(List_1);
		if (List_1.Sum((WaveInfoClass x) => x.Limit) > 10)
		{
			method_1().HandleExceptions();
		}
	}

	public void Update()
	{
		if (Float_3 < Time.time)
		{
			Float_3 = Time.time + 2f;
			if (List_1.Count > 0 && Time.time - Float_2 > 50f)
			{
				method_1().HandleExceptions();
			}
		}
	}

	public async Task method_1()
	{
		Float_2 = Time.time;
		if (Int_3 > 1)
		{
			return;
		}
		Int_3++;
		if (List_1.Sum((WaveInfoClass x) => x.Limit) > 100 && GClass398.Instance.IsTraceEnable())
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (WaveInfoClass item2 in List_1)
			{
				stringBuilder.Append(item2.DebugData());
				stringBuilder.Append(";");
			}
		}
		List<WaveInfoClass> data = List_1.ToList();
		float time = Time.time;
		List_1.Clear();
		Profile[] array;
		try
		{
			array = await Gdelegate9_0(data, EProfilesAskingStat.byBackup);
			float item = Time.time - time;
			List_0.Add(item);
			Int_4++;
			Int_3--;
		}
		catch (Exception e)
		{
			GClass398.Instance.LogException(e);
			return;
		}
		if (array != null && array.Length != 0)
		{
			List<Profile> obj = array.ToList();
			Bool_0 = true;
			Action_0(obj);
		}
	}

	public void AddToTargetBackup(BotDifficulty difficulty, WildSpawnType role, int count)
	{
		if (!Gclass624_0.ContainsKey(role, difficulty))
		{
			Gclass624_0.Add(role, difficulty, count);
		}
	}
}
