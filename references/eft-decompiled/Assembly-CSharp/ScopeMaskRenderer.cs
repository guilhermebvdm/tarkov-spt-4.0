using System;
using System.Collections.Generic;
using EFT.CameraControl;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]
public class ScopeMaskRenderer : MonoBehaviour
{
	private readonly GClass1001 gclass1001_0 = new GClass1001("Scope Mask", CameraEvent.BeforeGBuffer);

	private Shader shader_0;

	private Material material_0;

	private Material material_1;

	private Shader shader_1;

	private Material material_2;

	private RenderTexture renderTexture_0;

	private CollimatorSight collimatorSight_0;

	private HashSet<CollimatorSight> hashSet_0 = new HashSet<CollimatorSight>();

	private static readonly Color color_0 = Color.red;

	private static readonly Color color_1 = Color.blue;

	private static readonly Color color_2 = Color.green;

	private static readonly float float_0 = 9f;

	private static readonly int int_0 = Shader.PropertyToID("_ScopeMask");

	private static readonly int int_1 = Shader.PropertyToID("_Color");

	public OpticSight OpticSight_0 => CameraClass.Instance.OpticCameraManager.CurrentOpticSight;

	public void Awake()
	{
		Camera.onPreCull = (Camera.CameraCallback)Delegate.Combine(Camera.onPreCull, new Camera.CameraCallback(method_4));
		CollimatorSight.OnCollimatorEnabled += method_0;
		CollimatorSight.OnCollimatorDisabled += method_1;
		CollimatorSight.OnCollimatorUpdated += method_2;
		method_3();
	}

	public void OnDestroy()
	{
		Camera.onPreCull = (Camera.CameraCallback)Delegate.Remove(Camera.onPreCull, new Camera.CameraCallback(method_4));
		CollimatorSight.OnCollimatorEnabled -= method_0;
		CollimatorSight.OnCollimatorDisabled -= method_1;
		CollimatorSight.OnCollimatorUpdated -= method_2;
		if (renderTexture_0 != null)
		{
			UnityEngine.Object.DestroyImmediate(renderTexture_0);
		}
	}

	public void method_0(CollimatorSight collimatorSight)
	{
		hashSet_0.Add(collimatorSight);
	}

	public void method_1(CollimatorSight collimatorSight)
	{
		hashSet_0.Remove(collimatorSight);
	}

	public void method_2(CollimatorSight collimatorSight)
	{
		if (collimatorSight.isActiveAndEnabled)
		{
			collimatorSight_0 = collimatorSight;
		}
	}

	public void method_3()
	{
		hashSet_0 = new HashSet<CollimatorSight>();
		if (shader_0 == null)
		{
			shader_0 = GClass872.Find("Hidden/ScopeMask");
		}
		if (material_0 == null)
		{
			material_0 = new Material(shader_0);
			material_0.SetColor(int_1, color_0);
		}
		if (material_1 == null)
		{
			material_1 = new Material(shader_0);
			material_1.SetColor(int_1, color_1);
		}
		if (shader_1 == null)
		{
			shader_1 = GClass872.Find("CW FX/Collimator");
		}
		if (material_2 == null)
		{
			material_2 = new Material(shader_1);
		}
		Shader.SetGlobalTexture(int_0, Texture2D.blackTexture);
	}

	public void method_4(Camera currentCamera)
	{
		if (!(currentCamera == null) && !(CameraClass.Instance.Camera != currentCamera) && gclass1001_0.UpdateOnPreCullRender(out var buffer))
		{
			buffer.Clear();
			method_5(buffer, currentCamera, gclass1001_0.GetSSAAComponent());
		}
	}

	public void method_5(CommandBuffer buffer, Camera currentCamera, SSAA ssaa)
	{
		method_6(currentCamera, ssaa);
		method_7(buffer);
		method_9(buffer, currentCamera);
		method_10(buffer);
		method_8(buffer);
		Shader.SetGlobalTexture(int_0, renderTexture_0);
	}

	public void method_6(Camera currentCamera, SSAA ssaa)
	{
		int num = (ssaa ? ssaa.GetInputWidth() : currentCamera.pixelWidth);
		if (num == 0)
		{
			num = currentCamera.pixelWidth;
		}
		int num2 = (ssaa ? ssaa.GetInputHeight() : currentCamera.pixelHeight);
		if (num2 == 0)
		{
			num2 = currentCamera.pixelHeight;
		}
		if (renderTexture_0 == null)
		{
			method_11(currentCamera, num, num2);
		}
		if (renderTexture_0.width != num || renderTexture_0.height != num2)
		{
			UnityEngine.Object.DestroyImmediate(renderTexture_0);
			method_11(currentCamera, num, num2);
		}
	}

	public void method_7(CommandBuffer buffer)
	{
		buffer.SetRenderTarget(renderTexture_0, BuiltinRenderTextureType.CameraTarget);
		buffer.ClearRenderTarget(clearDepth: false, clearColor: true, Color.black);
	}

	public void method_8(CommandBuffer buffer)
	{
		material_0.SetColor(int_1, color_0);
		Renderer renderer = OpticSight_0?.LensRenderer;
		if (renderer != null)
		{
			buffer.DrawRenderer(renderer, material_0, 0, 0);
		}
	}

	public void method_9(CommandBuffer buffer, Camera currentCamera)
	{
		material_1.SetColor(int_1, color_1);
		Vector3 position = currentCamera.transform.position;
		foreach (CollimatorSight item in hashSet_0)
		{
			Vector3 position2 = item.CollimatorMeshRenderer.transform.position;
			if (!(Vector3.SqrMagnitude(position - position2) > float_0))
			{
				buffer.DrawRenderer(item.CollimatorMeshRenderer, material_1);
			}
		}
	}

	public void method_10(CommandBuffer buffer)
	{
		if (collimatorSight_0 != null)
		{
			material_2.CopyPropertiesFromMaterial(collimatorSight_0.CollimatorMaterial);
			material_2.SetColor(int_1, color_2);
			buffer.DrawRenderer(collimatorSight_0.CollimatorMeshRenderer, material_2);
		}
	}

	public void method_11(Camera currentCamera, int width, int height)
	{
		renderTexture_0 = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32)
		{
			name = "ScopeMask _maskTexture " + currentCamera.name
		};
	}
}
