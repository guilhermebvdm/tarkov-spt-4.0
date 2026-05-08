using System.Collections.Generic;
using EFT.BlitDebug;
using UnityEngine;
using UnityEngine.Rendering;

public class DistortCameraFX : MonoBehaviour
{
	private static readonly List<DistortRenderer> list_0 = new List<DistortRenderer>();

	public Shader Shader;

	public float Distortion = 8f;

	[Range(1f, 4f)]
	public int BlurIterations = 1;

	private Material material_0;

	private Camera camera_0;

	private SSAA ssaa_0;

	private SSAAPropagator ssaapropagator_0;

	private CommandBuffer commandBuffer_0;

	private RenderTexture renderTexture_0;

	private RenderTexture renderTexture_1;

	private bool bool_0;

	private Vector2 vector2_0;

	private int int_0;

	private int int_1;

	private static readonly int int_2 = Shader.PropertyToID("_Distortion");

	private static readonly int int_3 = Shader.PropertyToID("_DistortValuesTex");

	private static readonly int int_4 = Shader.PropertyToID("_BlurTexture");

	private static readonly int int_5 = Shader.PropertyToID("_BlurOffsets");

	public Material Material_0
	{
		get
		{
			if (material_0 != null)
			{
				return material_0;
			}
			return material_0 = new Material(Shader);
		}
	}

	public void Awake()
	{
		method_2();
		ssaapropagator_0 = GetComponent<SSAAPropagator>();
	}

	public void OnValidate()
	{
		method_7();
	}

	public void OnEnable()
	{
		if (commandBuffer_0 == null)
		{
			commandBuffer_0 = new CommandBuffer
			{
				name = "Render Distort Values"
			};
		}
		camera_0.RemoveCommandBuffer(CameraEvent.BeforeImageEffectsOpaque, commandBuffer_0);
		camera_0.AddCommandBuffer(CameraEvent.BeforeImageEffectsOpaque, commandBuffer_0);
	}

	public void OnDisable()
	{
		if (commandBuffer_0 != null)
		{
			camera_0.RemoveCommandBuffer(CameraEvent.BeforeImageEffectsOpaque, commandBuffer_0);
		}
	}

	public void OnDestroy()
	{
		if (renderTexture_0 != null)
		{
			Object.DestroyImmediate(renderTexture_0);
		}
		if (renderTexture_1 != null)
		{
			Object.DestroyImmediate(renderTexture_1);
		}
	}

	public int method_0()
	{
		int num = (camera_0 ? camera_0.pixelWidth : Screen.width);
		if (ssaa_0 != null)
		{
			num = ((ssaa_0.GetInputWidth() > 0) ? ssaa_0.GetInputWidth() : num);
		}
		return num;
	}

	public int method_1()
	{
		int num = (camera_0 ? camera_0.pixelHeight : Screen.height);
		if (ssaa_0 != null)
		{
			num = ((ssaa_0.GetInputHeight() > 0) ? ssaa_0.GetInputHeight() : num);
		}
		return num;
	}

	public void Update()
	{
		if (!(camera_0 == null) && (method_1() != int_0 || method_0() != int_1))
		{
			int_0 = method_1();
			int_1 = method_0();
			method_5();
		}
	}

	public void method_2()
	{
		camera_0 = GetComponent<Camera>();
		ssaa_0 = GetComponent<SSAA>();
		material_0 = new Material(Shader);
		int_0 = method_1();
		int_1 = method_0();
		method_5();
		method_6();
		method_7();
	}

	public void OnPreCull()
	{
		if (base.gameObject.activeInHierarchy && base.enabled)
		{
			commandBuffer_0.Clear();
			bool_0 = list_0.Count == 0;
			if (!bool_0)
			{
				bool_0 = !method_4(commandBuffer_0, camera_0);
			}
		}
		else
		{
			bool_0 = false;
		}
	}

	public void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (ssaapropagator_0 != null)
		{
			ssaapropagator_0.GetSourceDestination(out source, out destination);
		}
		method_3(source, destination);
		if (ssaapropagator_0 != null)
		{
			ssaapropagator_0.ReleaseSourceDestination(source, destination);
		}
	}

	public void method_3(RenderTexture src, RenderTexture dest)
	{
		if (bool_0)
		{
			GClass860.BlitOrCopy(src, dest);
			return;
		}
		method_8(src);
		DebugGraphics.Blit(src, dest, Material_0, 1);
	}

	public bool method_4(CommandBuffer buffer, Camera currentCamera)
	{
		buffer.SetRenderTarget(renderTexture_0, BuiltinRenderTextureType.CurrentActive);
		buffer.ClearRenderTarget(clearDepth: false, clearColor: true, new Color(0.5f, 0.5f, 0f, 0f));
		Plane[] planes = GClass823.CalculateFrustumPlanesNonAlloc(currentCamera);
		bool result = false;
		for (int i = 0; i < list_0.Count; i++)
		{
			if (GeometryUtility.TestPlanesAABB(planes, list_0[i].Renderer.bounds))
			{
				buffer.DrawRenderer(list_0[i].Renderer, list_0[i].MeterialToRender);
				result = true;
			}
		}
		Material_0.SetTexture(int_3, renderTexture_0);
		return result;
	}

	public void method_5()
	{
		if (renderTexture_0 != null)
		{
			Object.DestroyImmediate(renderTexture_0);
		}
		renderTexture_0 = new RenderTexture(method_0(), method_1(), 0, RenderTextureFormat.ARGB32)
		{
			name = "DistortCameraFX _distortTexture"
		};
	}

	public void method_6()
	{
		if (!(renderTexture_1 != null))
		{
			int width = method_0() >> 1;
			int height = method_1() >> 1;
			renderTexture_1 = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32)
			{
				name = "DistortCameraFX _blurTexture"
			};
		}
	}

	public void method_7()
	{
		vector2_0 = new Vector2(Distortion / (float)method_0(), Distortion / (float)method_1());
		Material_0.SetVector(int_2, vector2_0);
	}

	public void method_8(RenderTexture src)
	{
		int num = method_0() >> 1;
		int num2 = method_1() >> 1;
		RenderTexture temporary = RenderTexture.GetTemporary(num, num2, 0, RenderTextureFormat.ARGB32);
		temporary.name = "DistortCameraFX TempRT";
		Graphics.Blit(src, renderTexture_1);
		for (int i = 0; i < BlurIterations; i++)
		{
			float num3 = 1 << i;
			Material_0.SetVector(int_5, new Vector4(num3 / (float)num, 0f, 0f, 0f));
			DebugGraphics.Blit(renderTexture_1, temporary, Material_0, 0);
			Material_0.SetVector(int_5, new Vector4(0f, num3 / (float)num2, 0f, 0f));
			DebugGraphics.Blit(temporary, renderTexture_1, Material_0, 0);
		}
		RenderTexture.ReleaseTemporary(temporary);
		Material_0.SetTexture(int_4, renderTexture_1);
	}

	public static void AddRenderer(DistortRenderer renderer)
	{
		if (!list_0.Contains(renderer))
		{
			list_0.Add(renderer);
		}
	}

	public static void RemoveRenderer(DistortRenderer renderer)
	{
		if (list_0.Contains(renderer))
		{
			list_0.Remove(renderer);
		}
	}
}
