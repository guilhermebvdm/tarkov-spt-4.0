using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
[CreateAssetMenu(menuName = "Seasons/SeasonsMaterialsMap")]
public class SeasonsMaterialsMap : ScriptableObject, GInterface43
{
	[Serializable]
	[CompilerGenerated]
	public class Class577
	{
		public static readonly Class577 class577_0 = new Class577();

		public static Func<GInterface43, Task> func_0;

		public static Func<GInterface43, Task> func_1;

		public static Func<GInterface43, Task> func_2;

		public static Func<GInterface43, Task> func_3;

		public static Func<GInterface43, Task> func_4;

		public static Func<GInterface43, Task> func_5;

		public Task _003CLoadSummer_003Eb__3_0(GInterface43 material)
		{
			return material.LoadSummer();
		}

		public Task _003CLoadWinter_003Eb__4_0(GInterface43 material)
		{
			return material.LoadWinter();
		}

		public Task _003CLoadSpring_003Eb__5_0(GInterface43 material)
		{
			return material.LoadSpring();
		}

		public Task _003CLoadSpringEarly_003Eb__6_0(GInterface43 material)
		{
			return material.LoadSpringEarly();
		}

		public Task _003CLoadAutumn_003Eb__7_0(GInterface43 material)
		{
			return material.LoadAutumn();
		}

		public Task _003CLoadAutumnLate_003Eb__8_0(GInterface43 material)
		{
			return material.LoadAutumnLate();
		}
	}

	public SeasonsMaterialsGroup[] Groups;

	public static LoggerClass Logger => Class443.Logger;

	public Task LoadSummer()
	{
		return Load((GInterface43 material) => material.LoadSummer());
	}

	public Task LoadWinter()
	{
		return Load((GInterface43 material) => material.LoadWinter());
	}

	public Task LoadSpring()
	{
		return Load((GInterface43 material) => material.LoadSpring());
	}

	public Task LoadSpringEarly()
	{
		return Load((GInterface43 material) => material.LoadSpringEarly());
	}

	public Task LoadAutumn()
	{
		return Load((GInterface43 material) => material.LoadAutumn());
	}

	public Task LoadAutumnLate()
	{
		return Load((GInterface43 material) => material.LoadAutumnLate());
	}

	public Task Load(Func<GInterface43, Task> load)
	{
		if (Groups == null)
		{
			Logger.LogError("SeasonsMaterialsMap:" + base.name + " Groups is null", this);
			return Task.CompletedTask;
		}
		Task[] array = new Task[Groups.Length];
		for (int i = 0; i < Groups.Length; i++)
		{
			SeasonsMaterialsGroup seasonsMaterialsGroup = Groups[i];
			if (seasonsMaterialsGroup != null)
			{
				array[i] = load(seasonsMaterialsGroup);
				continue;
			}
			array[i] = Task.CompletedTask;
			Logger.LogError($"SeasonsMaterialsMap:{base.name} Group[{i}] is null", this);
		}
		return Task.WhenAll(array);
	}

	public void Fix()
	{
		Logger.LogTrace("SeasonsMaterialsMap:" + base.name + " fix begin");
		if (Groups == null)
		{
			Logger.LogError("SeasonsMaterialsMap:" + base.name + " Groups is null", this);
			return;
		}
		for (int i = 0; i < Groups.Length; i++)
		{
			SeasonsMaterialsGroup seasonsMaterialsGroup = Groups[i];
			if (seasonsMaterialsGroup != null)
			{
				seasonsMaterialsGroup.Fix();
				continue;
			}
			Logger.LogError($"SeasonsMaterialsMap:{base.name} Group[{i}] is null", this);
		}
		Logger.LogTrace("SeasonsMaterials:" + base.name + " fix complete");
	}

	public void Unload()
	{
		if (Groups == null)
		{
			Logger.LogError("SeasonsMaterialsMap:" + base.name + " Groups is null", this);
			return;
		}
		for (int i = 0; i < Groups.Length; i++)
		{
			SeasonsMaterialsGroup seasonsMaterialsGroup = Groups[i];
			if (seasonsMaterialsGroup != null)
			{
				seasonsMaterialsGroup.Unload();
				continue;
			}
			Logger.LogError($"SeasonsMaterialsMap:{base.name} Group[{i}] is null", this);
		}
	}
}
