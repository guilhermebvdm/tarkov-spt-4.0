using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

public class FastAnimatorControllerClass : GClass1313
{
	[Serializable]
	[CompilerGenerated]
	public class Class906<T>
	{
		public static readonly Class906<T> class906_0 = new Class906<T>();

		public static Func<GClass1312, IEnumerable<GClass1328>> func_0;

		public IEnumerable<GClass1328> method_0(GClass1312 s)
		{
			return s.Gclass1328_0;
		}
	}

	[NonSerialized]
	public string String_0;

	[NonSerialized]
	public List<SpeedAnimParamClass> List_0;

	[NonSerialized]
	public List<GClass1316> List_1;

	[NonSerialized]
	[CompilerGenerated]
	public List<GClass1312> List_2;

	public List<GClass1312> AllStates
	{
		[CompilerGenerated]
		get
		{
			return List_2;
		}
		[CompilerGenerated]
		set
		{
			List_2 = value;
		}
	}

	public int LayersCount => List_1.Count;

	public int ParametersCount => List_0.Count;

	public string UnityAnimatorControllerObjectHash => String_0;

	public FastAnimatorControllerClass(string name, int hash, string unityAnimatorControllerObjectHash)
		: base(name, hash)
	{
		String_0 = unityAnimatorControllerObjectHash;
		List_1 = new List<GClass1316>();
		List_0 = new List<SpeedAnimParamClass>();
		RefreshAllStates();
	}

	public void RefreshAllStates()
	{
		int num = 0;
		foreach (GClass1316 item in List_1)
		{
			num += method_1(item);
		}
		AllStates = new List<GClass1312>(num);
		foreach (GClass1316 item2 in List_1)
		{
			method_0(item2);
		}
	}

	public GClass1316 GetLayerStateMachine(int layerIndex)
	{
		return List_1[layerIndex];
	}

	public SpeedAnimParamClass GetParameter(int parameterIndex)
	{
		return List_0[parameterIndex];
	}

	public SpeedAnimParamClass FindParameter(string name)
	{
		int num = 0;
		while (true)
		{
			if (num < List_0.Count)
			{
				if (string.Equals(List_0[num].Name, name, StringComparison.Ordinal))
				{
					break;
				}
				num++;
				continue;
			}
			return null;
		}
		return List_0[num];
	}

	public SpeedAnimParamClass FindParameter(int nameHash)
	{
		int num = 0;
		while (true)
		{
			if (num < List_0.Count)
			{
				if (List_0[num].NameHash == nameHash)
				{
					break;
				}
				num++;
				continue;
			}
			return null;
		}
		return List_0[num];
	}

	public IEnumerable<GClass1316> GetLayerStateMachinesEnumerator()
	{
		return List_1;
	}

	public IEnumerable<SpeedAnimParamClass> GetParameters()
	{
		return List_0;
	}

	public T[] GetBehaviors<T>()
	{
		return AllStates.SelectMany((GClass1312 s) => s.Gclass1328_0).OfType<T>().ToArray();
	}

	public override void Dispose(bool disposing)
	{
		if (disposing)
		{
			if (List_0 != null)
			{
				for (int i = 0; i < List_0.Count; i++)
				{
					SpeedAnimParamClass speedAnimParamClass = List_0[i];
					List_0[i] = null;
					speedAnimParamClass.Dispose();
				}
				List_0 = null;
			}
			if (List_1 != null)
			{
				for (int j = 0; j < List_1.Count; j++)
				{
					GClass1316 gClass = List_1[j];
					List_1[j] = null;
					gClass.Dispose();
				}
				List_1 = null;
			}
		}
		base.Dispose(disposing);
	}

	public override void ResetCache()
	{
		for (int i = 0; i < List_1.Count; i++)
		{
			List_1[i].ResetCache();
		}
	}

	public void method_0(GClass1315 stateMachine)
	{
		AllStates.AddRange(stateMachine.List_1);
		foreach (GClass1315 item in stateMachine.List_0)
		{
			method_0(item);
		}
	}

	public int method_1(GClass1315 stateMachine)
	{
		int num = stateMachine.List_1.Count;
		foreach (GClass1315 item in stateMachine.List_0)
		{
			num += method_1(item);
		}
		return num;
	}
}
