using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using MultiFlare;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;

public class GClass1030 : GInterface60
{
	[Serializable]
	[CompilerGenerated]
	public class Class718
	{
		public static readonly Class718 class718_0 = new Class718();

		public static Func<KeyValuePair<FlareLight, int>, (FlareLight, int)> func_0;

		public (FlareLight, int) method_0(KeyValuePair<FlareLight, int> pair)
		{
			return (pair.Key, pair.Value);
		}
	}

	[NonSerialized]
	public const int Int_0 = 100;

	[NonSerialized]
	public const int Int_1 = 1000;

	[NonSerialized]
	public static GStruct76 Gstruct76_0;

	[NonSerialized]
	public Dictionary<FlareLight, int> Dictionary_0;

	[NonSerialized]
	public Dictionary<int, FlareLight> Dictionary_1;

	[NonSerialized]
	public NativeList<GStruct76> NativeList_0;

	[NonSerialized]
	public NativeArray<GStruct76> NativeArray_0;

	[NonSerialized]
	public TransformAccessArray TransformAccessArray_0;

	[NonSerialized]
	public int Int_2;

	public int Count => NativeList_0.Length;

	public int Capacity => NativeList_0.Capacity;

	public GClass1030()
	{
		Dictionary_0 = new Dictionary<FlareLight, int>(1000);
		Dictionary_1 = new Dictionary<int, FlareLight>(1000);
		NativeList_0 = new NativeList<GStruct76>(1000, Allocator.Persistent);
		TransformAccessArray_0 = new TransformAccessArray(1000);
	}

	public void Dispose()
	{
		NativeList_0.Dispose();
		TransformAccessArray_0.Dispose();
		if (NativeArray_0.IsCreated)
		{
			NativeArray_0.Dispose();
		}
	}

	public NativeArray<GStruct76>.ReadOnly GetJobData()
	{
		return NativeArray_0.AsReadOnly();
	}

	public void DisposeIntermediateData()
	{
		if (NativeArray_0.IsCreated)
		{
			NativeArray_0.Dispose();
		}
	}

	public JobHandle UpdatePositions()
	{
		NativeArray_0 = new NativeArray<GStruct76>(NativeList_0.AsArray(), Allocator.TempJob);
		return new GStruct77(NativeArray_0).ScheduleReadOnly(TransformAccessArray_0, 100);
	}

	public int Add(FlareLight flareLight)
	{
		int result = method_0(flareLight);
		NativeList_0.Add(new GStruct76(flareLight));
		TransformAccessArray_0.Add(flareLight.transform);
		return result;
	}

	public void Remove(FlareLight flareLight, out int removedIndex, out int replacedIndex)
	{
		removedIndex = Dictionary_0[flareLight];
		NativeList_0.RemoveAtSwapBack(removedIndex);
		TransformAccessArray_0.RemoveAtSwapBack(removedIndex);
		method_1(removedIndex, out replacedIndex);
	}

	public IEnumerable<(FlareLight, int)> GetAll()
	{
		return Dictionary_0.Select((KeyValuePair<FlareLight, int> pair) => (pair.Key, pair.Value)).ToList();
	}

	public ref GStruct76 Get(FlareLight flareLight)
	{
		if (!Dictionary_0.TryGetValue(flareLight, out var value))
		{
			smethod_0(flareLight.name);
			return ref Gstruct76_0;
		}
		return ref NativeList_0.ElementAt(value);
	}

	public bool Contains(FlareLight flareLight)
	{
		return Dictionary_0.ContainsKey(flareLight);
	}

	public int method_0(FlareLight value)
	{
		Dictionary_0[value] = Int_2;
		Dictionary_1[Int_2] = value;
		Int_2++;
		return Int_2 - 1;
	}

	public void method_1(int index, out int replacedIndex)
	{
		FlareLight key = Dictionary_1[index];
		FlareLight flareLight = Dictionary_1[Int_2 - 1];
		Dictionary_1.Remove(index);
		Dictionary_0.Remove(key);
		Int_2--;
		replacedIndex = Int_2;
		if (index != Int_2)
		{
			Dictionary_1.Remove(Int_2);
			Dictionary_0[flareLight] = index;
			Dictionary_1[index] = flareLight;
		}
	}

	public static void smethod_0(string lightName)
	{
		Debug.LogWarning("Trying to access " + lightName + ". But it was not added to LightStorage");
	}
}
