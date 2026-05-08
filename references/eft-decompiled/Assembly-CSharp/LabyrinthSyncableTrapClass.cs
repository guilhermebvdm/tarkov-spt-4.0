using System;
using System.Linq;
using System.Runtime.CompilerServices;
using CommonAssets.Scripts.Game.LabyrinthEvent;
using UnityEngine;

public abstract class LabyrinthSyncableTrapClass
{
	[Serializable]
	[CompilerGenerated]
	public class Class996
	{
		public static readonly Class996 class996_0 = new Class996();

		public static Func<TrapSyncable, bool> func_0;

		public static Func<TrapSyncable, bool> func_1;

		public bool method_0(TrapSyncable x)
		{
			return x.TrapType == ETrapType.BarbedWire;
		}

		public bool method_1(TrapSyncable x)
		{
			return x.TrapType == ETrapType.TrapDoor;
		}
	}

	public static void InitLabyrinthSyncableTraps(LabyrinthSyncableTrapDataClass trapsData)
	{
		TrapSyncable[] source = LocationScene.GetAllObjects<ISyncAble>().OfType<TrapSyncable>().ToArray();
		TrapSyncable[] traps = source.Where((TrapSyncable x) => x.TrapType == ETrapType.BarbedWire).ToArray();
		TrapSyncable[] traps2 = source.Where((TrapSyncable x) => x.TrapType == ETrapType.TrapDoor).ToArray();
		smethod_0(trapsData.MinTrapDoors, trapsData.MaxTrapDoors, traps2, inverseActive: true);
		smethod_0(trapsData.MinBarbedWires, trapsData.MaxBarbedWires, traps);
	}

	public static void smethod_0(int minTraps, int maxTraps, TrapSyncable[] traps, bool inverseActive = false)
	{
		int minInclusive = Mathf.Min(minTraps, traps.Length);
		int maxExclusive = Mathf.Min(maxTraps, traps.Length);
		int num = UnityEngine.Random.Range(minInclusive, maxExclusive);
		TrapSyncable[] array = GClass856.Randomize(traps).ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].gameObject.SetActive(inverseActive ? (i >= num) : (i < num));
		}
	}
}
