using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Koenigz.PerfectCulling.EFT;
using UnityEngine;

public class GClass1238 : IDisposable
{
	public struct Struct238
	{
		public global::VisibilityIndicesClass<ushort> Indices;

		public void DecompressCallback(GStruct113 cd)
		{
			Indices = cd.Indices;
		}
	}

	[NonSerialized]
	[CompilerGenerated]
	public static GClass1238 Gclass1238_0;

	[CompilerGenerated]
	private static Action action_0;

	[NonSerialized]
	public PerfectCullingAdaptiveGrid PerfectCullingAdaptiveGrid_0;

	[NonSerialized]
	public int Int_0;

	[NonSerialized]
	public GClass1250.GClass1251 Gclass1251_0;

	[NonSerialized]
	public CullingGridPreProcess CullingGridPreProcess_0;

	[NonSerialized]
	public global::VisibilityIndicesClass<ushort> Gclass1222_0;

	public static GClass1238 Instance
	{
		[CompilerGenerated]
		get
		{
			return Gclass1238_0;
		}
		[CompilerGenerated]
		set
		{
			Gclass1238_0 = value;
		}
	}

	public CullingGridPreProcess PreProc => CullingGridPreProcess_0;

	public Bounds[] VisibilityCells => CullingGridPreProcess_0.VisibilityCells;

	public int GroupId => Int_0;

	public static event Action OnCullingGridCreated
	{
		[CompilerGenerated]
		add
		{
			Action action = action_0;
			Action action2;
			do
			{
				action2 = action;
				Action value2 = (Action)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action action = action_0;
			Action action2;
			do
			{
				action2 = action;
				Action value2 = (Action)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public GClass1238(PerfectCullingAdaptiveGrid grid, CullingGridPreProcess preProc)
	{
		if (Instance != null)
		{
			throw new InvalidOperationException("Duplicate visibility sampler instance exists");
		}
		Instance = this;
		CullingGridPreProcess_0 = preProc;
		PerfectCullingAdaptiveGrid_0 = grid;
		Int_0 = -1;
		Gclass1222_0 = new global::VisibilityIndicesClass<ushort>(65535);
		method_0();
		action_0?.Invoke();
	}

	public void method_0()
	{
		if (Int_0 == -1)
		{
			Int_0 = PerfectCullingAdaptiveGrid_0.GetGroupIndex(new GuidReference(CullingGridPreProcess_0.GetComponent<GuidComponent>()));
		}
		if (Gclass1251_0 == null)
		{
			Gclass1251_0 = PerfectCullingAdaptiveGrid_0.PackedData.AllocateDecompressor();
		}
	}

	public void Dispose()
	{
		Instance = null;
		action_0 = null;
		Gclass1222_0 = null;
		if (Gclass1251_0 != null)
		{
			Gclass1251_0.Dispose();
			Gclass1251_0 = null;
		}
	}

	public GStruct111 Query(Vector3 point)
	{
		method_0();
		if (PerfectCullingAdaptiveGrid_0.RuntimeBVH != null && CullingGridPreProcess_0.VisibilityCells != null && Int_0 != -1)
		{
			if (PerfectCullingAdaptiveGrid_0.RuntimeBVH.QueryNearest(point, PerfectCullingCrossSceneSampler.float_3) != null && Gclass1251_0 != null)
			{
				Gclass1251_0.DecompressCullingCell(PerfectCullingCrossSceneSampler.Int32_0, Int_0, Gclass1222_0);
				return new GStruct111(point, CullingGridPreProcess_0.VisibilityCells, Gclass1222_0);
			}
			return default(GStruct111);
		}
		return default(GStruct111);
	}
}
