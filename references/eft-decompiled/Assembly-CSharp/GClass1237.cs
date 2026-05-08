using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Koenigz.PerfectCulling;
using Koenigz.PerfectCulling.EFT;
using Unity.Jobs;
using UnityEngine;

public class GClass1237 : IDisposable
{
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct Struct235 : IJobParallelFor
	{
		public void Execute(int index)
		{
			bool isAutocullVisible = Gstruct111_0.TestAABB(List_1[index].AutocullObjectBounds);
			List_1[index].IsAutocullVisible = isAutocullVisible;
		}
	}

	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct Struct236 : IJob
	{
		public void Execute()
		{
			int count = List_0.Count;
			for (int i = 0; i < count; i++)
			{
				if (BitArray_0[i])
				{
					if (!List_0[i].IsAutocullVisible)
					{
						Stack_0.Push(i);
					}
				}
				else if (List_0[i].IsAutocullVisible)
				{
					Stack_1.Push(i);
				}
			}
		}
	}

	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct Struct237 : IJobParallelFor
	{
		public void Execute(int index)
		{
			BitArray_0[index] = Gstruct111_0.TestAABB(List_0[index].AutocullObjectBounds);
		}
	}

	[NonSerialized]
	[CompilerGenerated]
	public static GClass1237 Gclass1237_0;

	public const float FRAME_SWITCH_AMOUNT_FACTOR = 0.1f;

	public const int SWITCH_COUNT_PER_FRAME = 150;

	[NonSerialized]
	public static List<IAutocullAutomated> List_0;

	[NonSerialized]
	public static BitArray BitArray_0;

	[NonSerialized]
	public static Stack<int> Stack_0;

	[NonSerialized]
	public static Stack<int> Stack_1;

	[NonSerialized]
	public static List<IAutocullAutomated> List_1 = new List<IAutocullAutomated>();

	[NonSerialized]
	public static List<IAutocullAutomated> List_2 = new List<IAutocullAutomated>();

	[NonSerialized]
	public static List<IAutocullAutomated> List_3 = new List<IAutocullAutomated>();

	[NonSerialized]
	public JobHandle JobHandle_0;

	[NonSerialized]
	public JobHandle JobHandle_1;

	[NonSerialized]
	public static GStruct111 Gstruct111_0;

	[NonSerialized]
	public GClass1238 Gclass1238_0;

	[NonSerialized]
	public PerfectCullingCamera PerfectCullingCamera_0;

	[NonSerialized]
	public static int Int_0;

	[NonSerialized]
	public static int Int_1;

	[NonSerialized]
	public static bool Bool_0 = true;

	public static GClass1237 Instance
	{
		[CompilerGenerated]
		get
		{
			return Gclass1237_0;
		}
		[CompilerGenerated]
		set
		{
			Gclass1237_0 = value;
		}
	}

	public static (int activate, int deactivate) SwitchQueueSizes => (activate: Stack_0.Count, deactivate: Stack_1.Count);

	public static GStruct111 LastQuery => Gstruct111_0;

	public static int NumDynamicVisibleLastFrame => Int_0;

	public static int NumDynamicObjects => List_1.Count;

	public static void SetCullDynamicObjects(bool flag)
	{
		Bool_0 = flag;
	}

	public GClass1237(GClass1238 sampler, PerfectCullingCamera cullingCamera)
	{
		Instance = this;
		PerfectCullingCamera_0 = cullingCamera;
		Gclass1238_0 = sampler;
		Int_1 = -1;
		method_3();
	}

	public static void AddDynamic(IAutocullAutomated obj)
	{
		if (!List_2.Contains(obj))
		{
			List_2.Add(obj);
		}
	}

	public static void RemoveDynamic(IAutocullAutomated obj)
	{
		List_3.Add(obj);
	}

	public void Update()
	{
		if (!JobHandle_0.IsCompleted)
		{
			JobHandle_0.Complete();
		}
		if (PerfectCullingCrossSceneSampler.Int32_0 != Int_1)
		{
			Int_1 = PerfectCullingCrossSceneSampler.Int32_0;
			Gstruct111_0 = Gclass1238_0.Query(PerfectCullingCamera_0.ObservePosition);
		}
		method_2();
	}

	public void LateUpdate()
	{
		method_0();
	}

	public void method_0()
	{
		if (List_0 == null || List_0.Count == 0)
		{
			return;
		}
		if (!JobHandle_1.IsCompleted)
		{
			JobHandle_1.Complete();
		}
		if (Stack_0.Count > 0)
		{
			for (int num = Mathf.Min(Stack_0.Count, 150); num > 0; num--)
			{
				List_0[Stack_0.Pop()].IsAutocullVisible = true;
			}
		}
		if (Stack_1.Count > 0)
		{
			for (int num2 = Mathf.Min(Stack_1.Count, 150); num2 > 0; num2--)
			{
				List_0[Stack_1.Pop()].IsAutocullVisible = false;
			}
		}
	}

	public void method_1()
	{
		if (List_0 != null && List_0.Count != 0)
		{
			Stack_0.Clear();
			Stack_1.Clear();
			JobHandle_1 = default(Struct236).Schedule(IJobParallelForExtensions.Schedule(default(Struct237), List_0.Count, 1));
			JobHandle.ScheduleBatchedJobs();
		}
	}

	public void method_2()
	{
		foreach (IAutocullAutomated item in List_3)
		{
			List_1.Remove(item);
		}
		List_3.Clear();
		foreach (IAutocullAutomated item2 in List_2)
		{
			List_1.Add(item2);
		}
		List_2.Clear();
		Int_0 = 0;
		if (List_1.Count > 0)
		{
			Struct235 jobData = default(Struct235);
			JobHandle_0 = IJobParallelForExtensions.Schedule(jobData, List_1.Count, 1);
			JobHandle.ScheduleBatchedJobs();
		}
	}

	public static void smethod_0(GStruct111 query)
	{
		Int_0 = 0;
		if (Bool_0)
		{
			foreach (IAutocullAutomated item in List_1)
			{
				item.IsAutocullVisible = query.TestAABB(item.AutocullObjectBounds);
				if (item.IsAutocullVisible)
				{
					Int_0++;
				}
			}
			return;
		}
		foreach (IAutocullAutomated item2 in List_1)
		{
			item2.IsAutocullVisible = true;
			Int_0++;
		}
	}

	public void method_3()
	{
		Stack_0 = new Stack<int>();
		Stack_1 = new Stack<int>();
		List_0 = method_4();
		BitArray_0 = new BitArray(List_0.Count, defaultValue: true);
	}

	public List<IAutocullAutomated> method_4()
	{
		return new List<IAutocullAutomated>();
	}

	public AutocullLODGroupCell[] method_5()
	{
		AutocullLODGroupCell[] array = new AutocullLODGroupCell[Gclass1238_0.VisibilityCells.Length];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = new AutocullLODGroupCell(new List<LODGroup>());
		}
		GuidReference[] cullingGridContent = Gclass1238_0.PreProc.CullingGridContent;
		for (int j = 0; j < cullingGridContent.Length; j++)
		{
			GameObject gameObject = cullingGridContent[j].gameObject;
			if (gameObject == null)
			{
				continue;
			}
			CullingGridContent component = gameObject.GetComponent<CullingGridContent>();
			if (component == null)
			{
				continue;
			}
			for (int k = 0; k < component.CellContent.Length; k++)
			{
				AutocullLODGroupCell lodGroupCell = component.CellContent[k].LodGroupCell;
				if (lodGroupCell != null)
				{
					array[k].Union(lodGroupCell);
				}
			}
		}
		return array;
	}

	public void Dispose()
	{
		Instance = null;
		List_0.Clear();
		List_0 = null;
		foreach (IAutocullAutomated item in List_3)
		{
			List_1.Remove(item);
		}
		List_3.Clear();
		List_2.Clear();
		List_1.Clear();
		BitArray_0 = null;
		Stack_1 = null;
		Stack_0 = null;
	}
}
