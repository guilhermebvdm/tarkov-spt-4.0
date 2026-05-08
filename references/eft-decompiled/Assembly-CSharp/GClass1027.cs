using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MultiFlare;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Rendering;

public class GClass1027 : GInterface58
{
	[NonSerialized]
	public const int Int_0 = 100;

	[NonSerialized]
	public static int Int_1 = Shader.PropertyToID("_FlareBuffer");

	[NonSerialized]
	public ProFlareAtlas ProFlareAtlas_0;

	[NonSerialized]
	public BatchSettings BatchSettings_0;

	[NonSerialized]
	public GClass1030 Gclass1030_0;

	[NonSerialized]
	public GraphicsBuffer GraphicsBuffer_0;

	[NonSerialized]
	public NativeList<GStruct74> NativeList_0;

	[NonSerialized]
	public NativeList<GStruct75> NativeList_1;

	[NonSerialized]
	public JobHandle JobHandle_0;

	[NonSerialized]
	[CompilerGenerated]
	public Material Material_0;

	public virtual bool IsInstant => false;

	public Material Material
	{
		[CompilerGenerated]
		get
		{
			return Material_0;
		}
	}

	public int Length => NativeList_0.Length;

	public int Capacity => NativeList_0.Capacity;

	public GClass1027(ProFlareAtlas atlas, BatchSettings settings, GClass1030 lightStorage)
	{
		ProFlareAtlas_0 = atlas;
		BatchSettings_0 = settings;
		Gclass1030_0 = lightStorage;
		Material_0 = UnityEngine.Object.Instantiate(settings.Material);
		NativeList_0 = new NativeList<GStruct74>(settings.Capacity, Allocator.Persistent);
		NativeList_1 = new NativeList<GStruct75>(NativeList_0.Capacity, Allocator.Persistent);
		GraphicsBuffer_0 = new GraphicsBuffer(GraphicsBuffer.Target.Structured, NativeList_0.Capacity, Marshal.SizeOf<GStruct75>());
		Material.SetBuffer(Int_1, GraphicsBuffer_0);
	}

	public void Dispose()
	{
		if (Application.isPlaying)
		{
			UnityEngine.Object.Destroy(Material);
		}
		else
		{
			UnityEngine.Object.DestroyImmediate(Material);
		}
		NativeList_1.Dispose();
		GraphicsBuffer_0.Dispose();
		NativeList_0.Dispose();
	}

	public void Schedule(JobHandle dependsOn)
	{
		NativeList_1.Clear();
		JobHandle_0 = ScheduleInternal(dependsOn, NativeList_0, NativeList_1);
	}

	public void Complete()
	{
		JobHandle_0.Complete();
		GraphicsBuffer_0.SetData(NativeList_1.AsArray(), 0, 0, NativeList_1.Length);
	}

	public void ApplyRenderCommands(CommandBuffer commandBuffer)
	{
		commandBuffer.DrawProcedural(Matrix4x4.identity, Material, 0, MeshTopology.Points, NativeList_1.Length);
	}

	public void Add(int lightIndex, MultiFlare.Flare flare)
	{
		if (NativeList_0.Length >= NativeList_0.Capacity)
		{
			method_1();
			method_0();
		}
		Rect uvRectByIndex = ProFlareAtlas_0.GetUvRectByIndex(flare.TexId);
		NativeList_0.Add(new GStruct74(flare, lightIndex, uvRectByIndex));
	}

	public void Remove(int removedLightIndex, int replacedLightIndex)
	{
		for (int num = NativeList_0.Length - 1; num >= 0; num--)
		{
			if (NativeList_0[num].LightIndex == removedLightIndex)
			{
				NativeList_0.RemoveAt(num);
			}
			else
			{
				ref GStruct74 reference = ref NativeList_0.ElementAt(num);
				if (reference.LightIndex == replacedLightIndex)
				{
					reference.LightIndex = removedLightIndex;
				}
			}
		}
	}

	public virtual JobHandle ScheduleInternal(JobHandle dependsOn, NativeList<GStruct74> flares, NativeList<GStruct75> gpuData)
	{
		return IJobParallelForExtensions.Schedule(new GStruct79(Gclass1030_0, flares, BatchSettings_0, gpuData), flares.Length, 100, dependsOn);
	}

	public void method_0()
	{
		int num = NativeList_0.Capacity * 2;
		GraphicsBuffer_0.Dispose();
		GraphicsBuffer_0 = new GraphicsBuffer(GraphicsBuffer.Target.Structured, num, Marshal.SizeOf<GStruct75>());
		NativeList_1.Dispose();
		NativeList_1 = new NativeList<GStruct75>(num, Allocator.Persistent);
		Material.SetBuffer(Int_1, GraphicsBuffer_0);
	}

	public void method_1()
	{
		Debug.LogWarning($"Flare capacity dynamically increased. NewValue: {NativeList_0.Capacity * 2}. Fix settings. Material: {Material.name}");
	}
}
