using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Koenigz.PerfectCulling;
using UnityEngine;
using UnityEngine.Rendering;

public class GClass1217 : IDisposable
{
	[CompilerGenerated]
	public class Class827
	{
		public HashSet<Renderer> allRenderers;

		public GClass1217 gclass1217_0;

		public MaterialPropertyBlock propBlock;

		public int groupIndex;

		public Action<Renderer> action_0;

		public void method_0(Renderer renderer)
		{
			allRenderers.Add(renderer);
			gclass1217_0.method_2(renderer, gclass1217_0.Color32_0[groupIndex], propBlock);
		}
	}

	[NonSerialized]
	public PerfectCullingBakeGroup[] PerfectCullingBakeGroup_0;

	[NonSerialized]
	public List<Renderer> List_0;

	[NonSerialized]
	public Color32[] Color32_0;

	[NonSerialized]
	public int[] Int_0;

	[NonSerialized]
	public int Int_1 = Shader.PropertyToID("_Color");

	[NonSerialized]
	public Material Material_0;

	[NonSerialized]
	public List<UnityEngine.Object> List_1 = new List<UnityEngine.Object>();

	public int[] Hash => Int_0;

	public Color32[] Colors => Color32_0;

	public GClass1217(PerfectCullingBakeGroup[] groups, List<Renderer> additionalOccluders, bool setupRenderers = true)
	{
		PerfectCullingBakeGroup_0 = groups;
		List_0 = additionalOccluders;
		Int_0 = new int[16777216];
		Color32_0 = new Color32[PerfectCullingBakeGroup_0.Length];
		method_0();
		if (Material_0 == null)
		{
			Debug.LogError("Missing material");
			return;
		}
		Material_0 = UnityEngine.Object.Instantiate(Material_0);
		List_1.Add(Material_0);
		Material_0.enableInstancing = false;
		if (setupRenderers)
		{
			method_1();
		}
	}

	public void Dispose()
	{
		foreach (UnityEngine.Object item in List_1)
		{
			UnityEngine.Object.DestroyImmediate(item);
		}
		List_1.Clear();
		List_1 = null;
		Int_0 = null;
	}

	public void method_0()
	{
		Color32[] colors = PerfectCullingColorTable.Instance.Colors;
		for (int i = 0; i < Color32_0.Length; i++)
		{
			byte r = colors[i].r;
			byte g = colors[i].g;
			byte b = colors[i].b;
			int num = b * 256 * 256 + g * 256 + r;
			Color32_0[i] = new Color32(r, g, b, byte.MaxValue);
			Int_0[num] = i;
		}
	}

	public void method_1()
	{
		MaterialPropertyBlock propBlock = new MaterialPropertyBlock();
		HashSet<Renderer> allRenderers = new HashSet<Renderer>();
		int groupIndex = 0;
		while (groupIndex < PerfectCullingBakeGroup_0.Length)
		{
			PerfectCullingBakeGroup_0[groupIndex].ForeachRenderer(delegate(Renderer renderer)
			{
				allRenderers.Add(renderer);
				method_2(renderer, Color32_0[groupIndex], propBlock);
			});
			int num = groupIndex + 1;
			groupIndex = num;
		}
		if (List_0 == null)
		{
			return;
		}
		foreach (Renderer item in List_0)
		{
			if (!allRenderers.Contains(item))
			{
				method_2(item, GClass1224.ClearColor, propBlock);
			}
		}
	}

	public void method_2(Renderer renderer, Color col, MaterialPropertyBlock propBlock)
	{
		Material[] sharedMaterials = renderer.sharedMaterials;
		renderer.enabled = true;
		renderer.shadowCastingMode = ShadowCastingMode.Off;
		renderer.receiveShadows = false;
		renderer.lightProbeUsage = LightProbeUsage.Off;
		renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
		renderer.motionVectorGenerationMode = MotionVectorGenerationMode.Camera;
		renderer.allowOcclusionWhenDynamic = false;
		renderer.lightmapIndex = -1;
		renderer.realtimeLightmapIndex = -1;
		if (!Application.isPlaying)
		{
			renderer.gameObject.layer = 30;
		}
		for (int i = 0; i < sharedMaterials.Length; i++)
		{
			bool flag = GClass1228.IsMaterialTransparent(sharedMaterials[i]);
			sharedMaterials[i] = Material_0;
			col.a = (int)(byte)(flag ? 128 : 0);
			renderer.name += $", Color: {col}";
			propBlock.SetColor(Int_1, col);
			renderer.SetPropertyBlock(propBlock, i);
		}
		renderer.sharedMaterials = sharedMaterials;
	}
}
