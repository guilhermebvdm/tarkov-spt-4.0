using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
[CreateAssetMenu(menuName = "Seasons/SeasonsMaterialsGroup")]
public class SeasonsMaterialsGroup : SerializedScriptableObject, GInterface43
{
	[Serializable]
	[CompilerGenerated]
	public class Class576
	{
		public static readonly Class576 class576_0 = new Class576();

		public static Func<GInterface43, Task> func_0;

		public static Func<GInterface43, Task> func_1;

		public static Func<GInterface43, Task> func_2;

		public static Func<GInterface43, Task> func_3;

		public static Func<GInterface43, Task> func_4;

		public static Func<GInterface43, Task> func_5;

		public Task _003CLoadSummer_003Eb__4_0(GInterface43 material)
		{
			return material.LoadSummer();
		}

		public Task _003CLoadWinter_003Eb__5_0(GInterface43 material)
		{
			return material.LoadWinter();
		}

		public Task _003CLoadSpring_003Eb__6_0(GInterface43 material)
		{
			return material.LoadSpring();
		}

		public Task _003CLoadSpringEarly_003Eb__7_0(GInterface43 material)
		{
			return material.LoadSpringEarly();
		}

		public Task _003CLoadAutumn_003Eb__8_0(GInterface43 material)
		{
			return material.LoadAutumn();
		}

		public Task _003CLoadAutumnLate_003Eb__9_0(GInterface43 material)
		{
			return material.LoadAutumnLate();
		}
	}

	public List<GInterface43> Materials;

	public static LoggerClass Logger => Class443.Logger;

	public static SeasonsMaterialsGroup Create(string name)
	{
		SeasonsMaterialsGroup seasonsMaterialsGroup = ScriptableObject.CreateInstance<SeasonsMaterialsGroup>();
		seasonsMaterialsGroup.name = name;
		seasonsMaterialsGroup.Materials = new List<GInterface43>();
		return seasonsMaterialsGroup;
	}

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
		if (Materials == null)
		{
			Logger.LogError("SeasonsMaterialsGroup:" + base.name + " Materials is null", this);
			return Task.CompletedTask;
		}
		Task[] array = new Task[Materials.Count];
		for (int i = 0; i < Materials.Count; i++)
		{
			GInterface43 gInterface = Materials[i];
			if (gInterface != null)
			{
				array[i] = load(gInterface);
				continue;
			}
			array[i] = Task.CompletedTask;
			Logger.LogError($"Materials[{i} is null]", this);
		}
		return Task.WhenAll(array);
	}

	public void Fix()
	{
		if (Materials == null)
		{
			Logger.LogError("SeasonsMaterialsGroup:" + base.name + " Materials is null", this);
			return;
		}
		for (int i = 0; i < Materials.Count; i++)
		{
			GInterface43 gInterface = Materials[i];
			if (gInterface != null)
			{
				gInterface.Fix();
				continue;
			}
			Logger.LogError($"Materials[{i} is null]", this);
		}
		Logger.LogTrace($"Materials group:{base.name} count:{Materials.Count} fixed", this);
	}

	public void Unload()
	{
		if (Materials == null)
		{
			Logger.LogError("SeasonsMaterialsGroup:" + base.name + " Materials is null", this);
			return;
		}
		for (int i = 0; i < Materials.Count; i++)
		{
			GInterface43 gInterface = Materials[i];
			if (gInterface != null)
			{
				gInterface.Unload();
				continue;
			}
			Logger.LogError($"Materials[{i} is null]", this);
		}
	}
}
