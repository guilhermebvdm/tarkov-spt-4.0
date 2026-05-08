using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace WaterSSR;

[ExecuteAlways]
[GAttribute8(19000)]
public class WaterRendererv3 : MonoBehaviour
{
	private static readonly int int_0 = Shader.PropertyToID("_WaterForSSR_SavedG0");

	private static readonly int int_1 = Shader.PropertyToID("_WaterForSSR_SavedG2");

	private static RenderTargetIdentifier[] renderTargetIdentifier_0;

	private readonly GClass1001 gclass1001_0 = new GClass1001("Draw Water v3", CameraEvent.BeforeReflections);

	private readonly List<WaterForSSRv3> list_0 = new List<WaterForSSRv3>();

	private readonly List<WaterObject> list_1 = new List<WaterObject>();

	private static bool bool_0 = false;

	private static bool bool_1 = false;

	[SerializeField]
	private Shader _shader;

	private Material material_0;

	private const string string_0 = "GBuffer Pass";

	private const string string_1 = "Alpha Restore Pass";

	private const string string_2 = "Init";

	public void OnEnable()
	{
		if (_shader == null)
		{
			base.enabled = false;
			return;
		}
		if (material_0 == null)
		{
			material_0 = new Material(_shader);
		}
		Camera.onPreCull = (Camera.CameraCallback)Delegate.Combine(Camera.onPreCull, new Camera.CameraCallback(method_2));
		WaterForSSRv3.OnAdd += method_0;
		WaterForSSRv3.OnRemove += method_1;
		method_3();
	}

	public void OnDisable()
	{
		Camera.onPreCull = (Camera.CameraCallback)Delegate.Remove(Camera.onPreCull, new Camera.CameraCallback(method_2));
		WaterForSSRv3.OnAdd -= method_0;
		WaterForSSRv3.OnRemove -= method_1;
		list_0.Clear();
		list_1.Clear();
		gclass1001_0.DestroyBuffers();
		if (material_0 != null)
		{
			UnityEngine.Object.Destroy(material_0);
			material_0 = null;
		}
	}

	public void method_0(WaterForSSRv3 water)
	{
		if (!list_0.Contains(water))
		{
			list_0.Add(water);
		}
	}

	public void method_1(WaterForSSRv3 water)
	{
		if (list_0.Contains(water))
		{
			list_0.Remove(water);
		}
	}

	public void method_2(Camera camera)
	{
		if ((!camera.CompareTag("MainCamera") && !camera.CompareTag("OpticCamera") && Application.isPlaying) || !gclass1001_0.UpdateOnPreCullRender(out var buffer) || !method_4())
		{
			return;
		}
		smethod_1(camera, this);
		buffer.Clear();
		list_1.Clear();
		buffer.GetTemporaryRT(int_0, -1, -1, 16, FilterMode.Point, RenderTextureFormat.ARGB32);
		buffer.GetTemporaryRT(int_1, -1, -1, 16, FilterMode.Point, RenderTextureFormat.ARGB32);
		smethod_0(buffer, int_0, int_1);
		buffer.BeginSample("GBuffer Pass");
		buffer.SetRenderTarget(renderTargetIdentifier_0, BuiltinRenderTextureType.CameraTarget);
		foreach (WaterForSSRv3 item in list_0)
		{
			if (item == null || !item.isActiveAndEnabled)
			{
				continue;
			}
			WaterObject[] targets = item.Targets;
			MaterialPropertyBlock settingsBlock = item.GetSettingsBlock();
			WaterObject[] array = targets;
			foreach (WaterObject waterObject in array)
			{
				if (waterObject.Enabled)
				{
					list_1.Add(waterObject);
					buffer.DrawMesh(waterObject.Mesh, waterObject.TRS, material_0, 0, 0, settingsBlock);
				}
			}
		}
		buffer.EndSample("GBuffer Pass");
		buffer.BeginSample("Alpha Restore Pass");
		buffer.SetRenderTarget(BuiltinRenderTextureType.CameraTarget, BuiltinRenderTextureType.ResolvedDepth);
		foreach (WaterObject item2 in list_1)
		{
			buffer.DrawMesh(item2.Mesh, item2.TRS, material_0, 0, 1);
		}
		buffer.EndSample("Alpha Restore Pass");
		buffer.ReleaseTemporaryRT(int_0);
		buffer.ReleaseTemporaryRT(int_1);
	}

	public void method_3()
	{
		WaterForSSRv3[] array = GClass870.FindUnityObjectsOfType<WaterForSSRv3>();
		foreach (WaterForSSRv3 water in array)
		{
			method_0(water);
		}
	}

	public bool method_4()
	{
		if (list_0.Count < 1)
		{
			return false;
		}
		for (int i = 0; i < list_0.Count; i++)
		{
			WaterForSSRv3 waterForSSRv = list_0[i];
			if (!(waterForSSRv != null) || !waterForSSRv.isActiveAndEnabled)
			{
				continue;
			}
			WaterObject[] targets = waterForSSRv.Targets;
			for (int j = 0; j < targets.Length; j++)
			{
				if (targets[j].Enabled)
				{
					return true;
				}
			}
		}
		return false;
	}

	public static void smethod_0(CommandBuffer buffer, int G0ID, int G2ID)
	{
		buffer.BeginSample("Init");
		buffer.Blit(BuiltinRenderTextureType.GBuffer0, G0ID);
		buffer.Blit(BuiltinRenderTextureType.GBuffer2, G2ID);
		buffer.SetGlobalTexture(G0ID, G0ID);
		buffer.SetGlobalTexture(G2ID, G2ID);
		buffer.EndSample("Init");
	}

	public static void DisableSnowMask(bool disable)
	{
		bool_0 = disable;
	}

	public static void smethod_1(Camera currentCamera, UnityEngine.Object owner)
	{
		if (bool_0 != bool_1)
		{
			renderTargetIdentifier_0 = null;
		}
		if (renderTargetIdentifier_0 == null)
		{
			if (bool_0)
			{
				renderTargetIdentifier_0 = new RenderTargetIdentifier[4]
				{
					BuiltinRenderTextureType.GBuffer0,
					BuiltinRenderTextureType.GBuffer1,
					BuiltinRenderTextureType.GBuffer2,
					currentCamera.allowHDR ? BuiltinRenderTextureType.CameraTarget : BuiltinRenderTextureType.GBuffer3
				};
			}
			else
			{
				renderTargetIdentifier_0 = new RenderTargetIdentifier[5]
				{
					BuiltinRenderTextureType.GBuffer0,
					BuiltinRenderTextureType.GBuffer1,
					BuiltinRenderTextureType.GBuffer2,
					currentCamera.allowHDR ? BuiltinRenderTextureType.CameraTarget : BuiltinRenderTextureType.GBuffer3,
					Class679.Class679_0.method_1(currentCamera, owner)
				};
			}
			bool_1 = bool_0;
		}
		renderTargetIdentifier_0[3] = (currentCamera.allowHDR ? BuiltinRenderTextureType.CameraTarget : BuiltinRenderTextureType.GBuffer3);
		if (!bool_1)
		{
			renderTargetIdentifier_0[4] = Class679.Class679_0.method_1(currentCamera, owner);
		}
	}
}
