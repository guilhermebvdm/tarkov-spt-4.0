using System;
using System.Collections.Generic;
using Comfort.Common;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;

[RequireComponent(typeof(Camera))]
[ExecuteInEditMode]
public class VolumetricLightRenderer : MonoBehaviour
{
	public enum VolumtericResolution
	{
		Full,
		Half,
		Quarter
	}

	private static List<VolumetricLight> list_0 = new List<VolumetricLight>();

	private static Mesh mesh_0;

	private static Mesh mesh_1;

	private static Material material_0;

	public bool IsOn = true;

	public bool IsOptic;

	private Camera camera_0;

	private SSAA ssaa_0;

	private CommandBuffer commandBuffer_0;

	private CommandBuffer commandBuffer_1;

	private Matrix4x4 matrix4x4_0;

	private Material material_1;

	private Material material_2;

	private RenderTexture renderTexture_0;

	private RenderTexture renderTexture_1;

	private RenderTexture renderTexture_2;

	private static Texture texture_0;

	private RenderTexture renderTexture_3;

	private RenderTexture renderTexture_4;

	private VolumtericResolution volumtericResolution_0 = VolumtericResolution.Half;

	private static Texture2D texture2D_0;

	private static Texture3D texture3D_0;

	private static bool bool_0;

	private static int int_0 = -1;

	private static RenderTexture renderTexture_5;

	private static RenderTexture renderTexture_6;

	private static RenderTexture renderTexture_7;

	private static RenderTexture renderTexture_8;

	private static RenderTexture renderTexture_9;

	public VolumtericResolution Resolution = VolumtericResolution.Half;

	public Texture DefaultSpotCookie;

	private Matrix4x4 matrix4x4_1;

	private static readonly int int_1 = Shader.PropertyToID("_HalfResDepthBuffer");

	private static readonly int int_2 = Shader.PropertyToID("_HalfResColor");

	private static readonly int int_3 = Shader.PropertyToID("_QuarterResDepthBuffer");

	private static readonly int int_4 = Shader.PropertyToID("_QuarterResColor");

	private static readonly int int_5 = Shader.PropertyToID("_DitherTexture");

	private static readonly int int_6 = Shader.PropertyToID("_NoiseTexture");

	private static readonly int int_7 = Shader.PropertyToID("VolumetricLightRenderer Temp");

	private bool bool_1 = true;

	private bool bool_2 = true;

	public CommandBuffer GlobalCommandBuffer => commandBuffer_0;

	public List<VolumetricLight> Lights => list_0;

	public static void AddLight(VolumetricLight light)
	{
		if (!list_0.Contains(light))
		{
			list_0.Add(light);
		}
	}

	public static void RemoveLight(VolumetricLight light)
	{
		list_0.Remove(light);
	}

	public static Material GetLightMaterial()
	{
		return material_0;
	}

	public static Mesh GetPointLightMesh()
	{
		return mesh_0;
	}

	public static Mesh GetSpotLightMesh()
	{
		return mesh_1;
	}

	public RenderTexture GetVolumeLightBuffer()
	{
		return Resolution switch
		{
			VolumtericResolution.Quarter => renderTexture_2, 
			VolumtericResolution.Half => renderTexture_1, 
			_ => renderTexture_0, 
		};
	}

	public RenderTexture GetVolumeLightDepthBuffer()
	{
		return Resolution switch
		{
			VolumtericResolution.Quarter => renderTexture_4, 
			VolumtericResolution.Half => renderTexture_3, 
			_ => null, 
		};
	}

	public static Texture GetDefaultSpotCookie()
	{
		return texture_0;
	}

	public void Awake()
	{
		SharedGameSettingsClass instance = Singleton<SharedGameSettingsClass>.Instance;
		if (instance != null && !instance.Graphics.Settings.VolumetricLight.Value && Application.isPlaying)
		{
			GClass972.SafeDestroy(this);
			return;
		}
		if (camera_0 == null)
		{
			camera_0 = GetComponent<Camera>();
		}
		ssaa_0 = GetComponent<SSAA>();
		if (camera_0.actualRenderingPath == RenderingPath.Forward)
		{
			camera_0.depthTextureMode = DepthTextureMode.Depth;
		}
		volumtericResolution_0 = Resolution;
		Shader shader = GClass872.Find("Hidden/BlitAdd");
		material_1 = new Material(shader);
		shader = GClass872.Find("Hidden/BilateralBlur");
		material_2 = new Material(shader);
		commandBuffer_0 = new CommandBuffer
		{
			name = "PreLight"
		};
		commandBuffer_1 = new CommandBuffer
		{
			name = "PostRender Volumetric Lights"
		};
		if (mesh_0 == null)
		{
			GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
			mesh_0 = obj.GetComponent<MeshFilter>().sharedMesh;
			UnityEngine.Object.DestroyImmediate(obj);
		}
		if (mesh_1 == null)
		{
			mesh_1 = method_9();
		}
		if (material_0 == null)
		{
			shader = GClass872.Find("Sandbox/VolumetricLight");
			material_0 = new Material(shader);
		}
		if (texture_0 == null)
		{
			texture_0 = DefaultSpotCookie;
		}
		list_0.Clear();
		int opticRenderResolution = CameraClass.Instance.OpticCameraManager.OpticRenderResolution;
		method_0(opticRenderResolution);
		method_7();
		method_8();
		if (IsOptic)
		{
			method_1();
		}
		else
		{
			method_2();
		}
		camera_0.RemoveCommandBuffer(CameraEvent.BeforeLighting, commandBuffer_0);
		camera_0.AddCommandBuffer(CameraEvent.BeforeLighting, commandBuffer_0);
		camera_0.RemoveCommandBuffer(CameraEvent.BeforeImageEffects, commandBuffer_1);
		camera_0.AddCommandBuffer(CameraEvent.BeforeImageEffects, commandBuffer_1);
	}

	public void method_0(int opticResolution)
	{
		if (bool_0)
		{
			return;
		}
		int_0 = opticResolution;
		RenderTextureFormat format = (SystemInfo.SupportsRenderTextureFormat(RuntimeUtilities.defaultHDRRenderTextureFormat) ? RuntimeUtilities.defaultHDRRenderTextureFormat : RenderTextureFormat.ARGB32);
		if (renderTexture_5 != null)
		{
			renderTexture_5.Release();
			GClass972.SafeDestroy(renderTexture_5);
			renderTexture_5 = null;
		}
		renderTexture_5 = new RenderTexture(opticResolution, opticResolution, 0, format)
		{
			name = "OpticVolumeLightBuffer",
			filterMode = FilterMode.Bilinear
		};
		if (Resolution == VolumtericResolution.Half || Resolution == VolumtericResolution.Quarter)
		{
			RenderTextureFormat format2 = (SystemInfo.SupportsRenderTextureFormat(RuntimeUtilities.defaultHDRRenderTextureFormat) ? RuntimeUtilities.defaultHDRRenderTextureFormat : RenderTextureFormat.ARGB32);
			if (renderTexture_6 != null)
			{
				renderTexture_6.Release();
				GClass972.SafeDestroy(renderTexture_6);
				renderTexture_6 = null;
			}
			renderTexture_6 = new RenderTexture(opticResolution / 2, opticResolution / 2, 0, format2)
			{
				name = "OpticVolumeLightBufferHalf",
				filterMode = FilterMode.Bilinear
			};
			RenderTextureFormat format3 = (SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.RHalf) ? RenderTextureFormat.RHalf : RenderTextureFormat.ARGB32);
			if (renderTexture_7 != null)
			{
				renderTexture_7.Release();
				GClass972.SafeDestroy(renderTexture_7);
				renderTexture_7 = null;
			}
			renderTexture_7 = new RenderTexture(opticResolution / 2, opticResolution / 2, 0, format3)
			{
				name = "OpticVolumeLightHalfDepth",
				filterMode = FilterMode.Point
			};
			renderTexture_7.Create();
			RenderTextureFormat format4 = (SystemInfo.SupportsRenderTextureFormat(RuntimeUtilities.defaultHDRRenderTextureFormat) ? RuntimeUtilities.defaultHDRRenderTextureFormat : RenderTextureFormat.ARGB32);
			if (renderTexture_8 != null)
			{
				renderTexture_8.Release();
				GClass972.SafeDestroy(renderTexture_8);
				renderTexture_8 = null;
			}
			renderTexture_8 = new RenderTexture(opticResolution / 4, opticResolution / 4, 0, format4)
			{
				name = "OpticVolumeLightBufferQuarter",
				filterMode = FilterMode.Bilinear
			};
			RenderTextureFormat format5 = (SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.RHalf) ? RenderTextureFormat.RHalf : RenderTextureFormat.ARGB32);
			if (renderTexture_9 != null)
			{
				renderTexture_9.Release();
				GClass972.SafeDestroy(renderTexture_9);
				renderTexture_9 = null;
			}
			renderTexture_9 = new RenderTexture(opticResolution / 4, opticResolution / 4, 0, format5)
			{
				name = "OpticVolumeLightQuarterDepth",
				filterMode = FilterMode.Point
			};
			renderTexture_9.Create();
		}
		bool_0 = true;
	}

	public void method_1()
	{
		renderTexture_0 = renderTexture_5;
		renderTexture_1 = renderTexture_6;
		renderTexture_3 = renderTexture_7;
		renderTexture_2 = renderTexture_8;
		renderTexture_4 = renderTexture_9;
	}

	public void method_2()
	{
		int num = (ssaa_0 ? ssaa_0.GetInputWidth() : camera_0.pixelWidth);
		int num2 = (ssaa_0 ? ssaa_0.GetInputHeight() : camera_0.pixelHeight);
		num = ((num > 0) ? num : camera_0.pixelWidth);
		num2 = ((num2 > 0) ? num2 : camera_0.pixelHeight);
		if (renderTexture_0 != null)
		{
			renderTexture_0.Release();
			GClass972.SafeDestroy(renderTexture_0);
			renderTexture_0 = null;
		}
		RenderTextureFormat format = (SystemInfo.SupportsRenderTextureFormat(RuntimeUtilities.defaultHDRRenderTextureFormat) ? RuntimeUtilities.defaultHDRRenderTextureFormat : RenderTextureFormat.ARGB32);
		renderTexture_0 = new RenderTexture(num, num2, 0, format)
		{
			name = "VolumeLightBuffer",
			filterMode = FilterMode.Bilinear
		};
		if (renderTexture_3 != null)
		{
			renderTexture_3.Release();
			GClass972.SafeDestroy(renderTexture_3);
			renderTexture_3 = null;
		}
		if (renderTexture_1 != null)
		{
			renderTexture_1.Release();
			GClass972.SafeDestroy(renderTexture_1);
			renderTexture_1 = null;
		}
		if (Resolution == VolumtericResolution.Half || Resolution == VolumtericResolution.Quarter)
		{
			RenderTextureFormat format2 = (SystemInfo.SupportsRenderTextureFormat(RuntimeUtilities.defaultHDRRenderTextureFormat) ? RuntimeUtilities.defaultHDRRenderTextureFormat : RenderTextureFormat.ARGB32);
			renderTexture_1 = new RenderTexture(num / 2, num2 / 2, 0, format2)
			{
				name = "VolumeLightBufferHalf",
				filterMode = FilterMode.Bilinear
			};
			RenderTextureFormat format3 = (SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.RHalf) ? RenderTextureFormat.RHalf : RenderTextureFormat.ARGB32);
			renderTexture_3 = new RenderTexture(num / 2, num2 / 2, 0, format3)
			{
				name = "VolumeLightHalfDepth",
				filterMode = FilterMode.Point
			};
			renderTexture_3.Create();
		}
		if (renderTexture_2 != null)
		{
			renderTexture_2.Release();
			GClass972.SafeDestroy(renderTexture_2);
			renderTexture_2 = null;
		}
		if (renderTexture_4 != null)
		{
			renderTexture_4.Release();
			GClass972.SafeDestroy(renderTexture_4);
			renderTexture_4 = null;
		}
		if (Resolution == VolumtericResolution.Quarter)
		{
			RenderTextureFormat format4 = (SystemInfo.SupportsRenderTextureFormat(RuntimeUtilities.defaultHDRRenderTextureFormat) ? RuntimeUtilities.defaultHDRRenderTextureFormat : RenderTextureFormat.ARGB32);
			renderTexture_2 = new RenderTexture(num / 4, num2 / 4, 0, format4)
			{
				name = "VolumeLightBufferQuarter",
				filterMode = FilterMode.Bilinear
			};
			RenderTextureFormat format5 = (SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.RHalf) ? RenderTextureFormat.RHalf : RenderTextureFormat.ARGB32);
			renderTexture_4 = new RenderTexture(num / 4, num2 / 4, 0, format5)
			{
				name = "VolumeLightQuarterDepth",
				filterMode = FilterMode.Point
			};
			renderTexture_4.Create();
		}
	}

	public void OnPreRender()
	{
		if (camera_0 == null)
		{
			return;
		}
		method_3();
		matrix4x4_1 = Matrix4x4.Perspective(camera_0.fieldOfView, camera_0.aspect, 0.01f, camera_0.farClipPlane);
		matrix4x4_1 = GL.GetGPUProjectionMatrix(matrix4x4_1, renderIntoTexture: true);
		matrix4x4_0 = matrix4x4_1 * camera_0.worldToCameraMatrix;
		method_4();
		method_6();
		foreach (VolumetricLight item in list_0)
		{
			item.VolumetricLightPreRender(this, matrix4x4_0);
		}
		commandBuffer_0.SetRenderTarget(BuiltinRenderTextureType.CameraTarget, BuiltinRenderTextureType.CameraTarget);
		method_5();
	}

	public void method_3()
	{
		if (IsOptic)
		{
			int opticRenderResolution = CameraClass.Instance.OpticCameraManager.OpticRenderResolution;
			if (int_0 != opticRenderResolution || bool_2)
			{
				bool_0 = false;
				bool_2 = false;
				method_0(opticRenderResolution);
				method_1();
			}
		}
		else if (volumtericResolution_0 != Resolution || bool_1)
		{
			volumtericResolution_0 = Resolution;
			bool_1 = false;
			method_2();
		}
	}

	public void method_4()
	{
		bool flag = SystemInfo.graphicsShaderLevel > 40;
		if (commandBuffer_0 == null)
		{
			Awake();
		}
		commandBuffer_0.Clear();
		switch (Resolution)
		{
		case VolumtericResolution.Quarter:
		{
			Texture source2 = null;
			commandBuffer_0.Blit(source2, renderTexture_3, material_2, flag ? 4 : 10);
			commandBuffer_0.Blit(source2, renderTexture_4, material_2, flag ? 6 : 11);
			commandBuffer_0.SetRenderTarget(renderTexture_2);
			break;
		}
		default:
			commandBuffer_0.SetRenderTarget(renderTexture_0);
			break;
		case VolumtericResolution.Half:
		{
			Texture source = null;
			commandBuffer_0.Blit(source, renderTexture_3, material_2, flag ? 4 : 10);
			commandBuffer_0.SetRenderTarget(renderTexture_1);
			break;
		}
		}
		commandBuffer_0.ClearRenderTarget(clearDepth: false, clearColor: true, new Color(0f, 0f, 0f, 1f));
	}

	public void method_5()
	{
		commandBuffer_1.Clear();
		if (IsOn)
		{
			string text = "Volumetric light Post Render";
			commandBuffer_1.BeginSample(text);
			switch (Resolution)
			{
			case VolumtericResolution.Quarter:
			{
				string text4 = "Volumetric light Quarter";
				commandBuffer_1.BeginSample(text4);
				commandBuffer_1.GetTemporaryRT(int_7, renderTexture_4.width, renderTexture_4.height, 0, FilterMode.Bilinear, RuntimeUtilities.defaultHDRRenderTextureFormat);
				commandBuffer_1.Blit(renderTexture_2, int_7, material_2, 8);
				commandBuffer_1.Blit(int_7, renderTexture_2, material_2, 9);
				commandBuffer_1.Blit(renderTexture_2, renderTexture_0, material_2, 7);
				commandBuffer_1.EndSample(text4);
				break;
			}
			default:
			{
				string text3 = "Volumetric light Default";
				commandBuffer_1.BeginSample(text3);
				commandBuffer_1.GetTemporaryRT(int_7, renderTexture_0.width, renderTexture_0.height, 0, FilterMode.Bilinear, RuntimeUtilities.defaultHDRRenderTextureFormat);
				commandBuffer_1.Blit(renderTexture_0, int_7, material_2, 0);
				commandBuffer_1.Blit(int_7, renderTexture_0, material_2, 1);
				commandBuffer_1.EndSample(text3);
				break;
			}
			case VolumtericResolution.Half:
			{
				string text2 = "Volumetric light Half";
				commandBuffer_1.BeginSample(text2);
				commandBuffer_1.GetTemporaryRT(int_7, renderTexture_1.width, renderTexture_1.height, 0, FilterMode.Bilinear, RuntimeUtilities.defaultHDRRenderTextureFormat);
				commandBuffer_1.Blit(renderTexture_1, int_7, material_2, 2);
				commandBuffer_1.Blit(int_7, renderTexture_1, material_2, 3);
				commandBuffer_1.Blit(renderTexture_1, renderTexture_0, material_2, 5);
				commandBuffer_1.EndSample(text2);
				break;
			}
			}
			commandBuffer_1.BeginSample("VolumetricBlit");
			if ((bool)ssaa_0)
			{
				commandBuffer_1.Blit(renderTexture_0, ssaa_0.GetRTIdentifier(), material_1);
			}
			else
			{
				commandBuffer_1.Blit(renderTexture_0, BuiltinRenderTextureType.CameraTarget, material_1);
			}
			commandBuffer_1.EndSample("VolumetricBlit");
			commandBuffer_1.ReleaseTemporaryRT(int_7);
			commandBuffer_1.SetRenderTarget(BuiltinRenderTextureType.CameraTarget, BuiltinRenderTextureType.CameraTarget);
			commandBuffer_1.EndSample(text);
		}
	}

	public void method_6()
	{
		material_2.SetTexture(int_1, renderTexture_3);
		material_2.SetTexture(int_2, renderTexture_1);
		material_2.SetTexture(int_3, renderTexture_4);
		material_2.SetTexture(int_4, renderTexture_2);
		Shader.SetGlobalTexture(int_5, texture2D_0);
		Shader.SetGlobalTexture(int_6, texture3D_0);
	}

	public void Update()
	{
		method_3();
		int num = (ssaa_0 ? ssaa_0.GetInputWidth() : camera_0.pixelWidth);
		int num2 = (ssaa_0 ? ssaa_0.GetInputHeight() : camera_0.pixelHeight);
		if (renderTexture_0 != null && (renderTexture_0.width != num || renderTexture_0.height != num2))
		{
			method_2();
		}
	}

	public void method_7()
	{
		if (texture3D_0 != null)
		{
			return;
		}
		TextAsset textAsset = GClass861.Load("NoiseVolume") as TextAsset;
		byte[] bytes = textAsset.bytes;
		uint num = BitConverter.ToUInt32(textAsset.bytes, 12);
		uint num2 = BitConverter.ToUInt32(textAsset.bytes, 16);
		uint num3 = BitConverter.ToUInt32(textAsset.bytes, 20);
		uint num4 = BitConverter.ToUInt32(textAsset.bytes, 24);
		uint num5 = BitConverter.ToUInt32(textAsset.bytes, 80);
		uint num6 = BitConverter.ToUInt32(textAsset.bytes, 88);
		if (num6 == 0)
		{
			num6 = num3 / num2 * 8;
		}
		texture3D_0 = new Texture3D((int)num2, (int)num, (int)num4, TextureFormat.RGBA32, mipChain: false);
		texture3D_0.name = "3D Noise";
		Color[] array = new Color[num2 * num * num4];
		uint num7 = 128u;
		if (textAsset.bytes[84] == 68 && textAsset.bytes[85] == 88 && textAsset.bytes[86] == 49 && textAsset.bytes[87] == 48 && (num5 & 4) != 0)
		{
			uint num8 = BitConverter.ToUInt32(textAsset.bytes, (int)num7);
			if (num8 >= 60 && num8 <= 65)
			{
				num6 = 8u;
			}
			else if (num8 >= 48 && num8 <= 52)
			{
				num6 = 16u;
			}
			else if (num8 >= 27 && num8 <= 32)
			{
				num6 = 32u;
			}
			num7 += 20;
		}
		uint num9 = num6 / 8;
		num3 = (num2 * num6 + 7) / 8;
		for (int i = 0; i < num4; i++)
		{
			for (int j = 0; j < num; j++)
			{
				for (int k = 0; k < num2; k++)
				{
					float num10 = (float)(int)bytes[num7 + k * num9] / 255f;
					array[k + j * num2 + i * num2 * num] = new Color(num10, num10, num10, num10);
				}
				num7 += num3;
			}
		}
		texture3D_0.SetPixels(array);
		texture3D_0.Apply();
	}

	public void method_8()
	{
		if (!(texture2D_0 != null))
		{
			texture2D_0 = new Texture2D(8, 8, TextureFormat.Alpha8, mipChain: false, linear: true);
			texture2D_0.name = "VLRDitheringTex";
			texture2D_0.filterMode = FilterMode.Point;
			Color32[] pixels = new Color32[64]
			{
				new Color32(3, 3, 3, 3),
				new Color32(192, 192, 192, 192),
				new Color32(51, 51, 51, 51),
				new Color32(239, 239, 239, 239),
				new Color32(15, 15, 15, 15),
				new Color32(204, 204, 204, 204),
				new Color32(62, 62, 62, 62),
				new Color32(251, 251, 251, 251),
				new Color32(129, 129, 129, 129),
				new Color32(66, 66, 66, 66),
				new Color32(176, 176, 176, 176),
				new Color32(113, 113, 113, 113),
				new Color32(141, 141, 141, 141),
				new Color32(78, 78, 78, 78),
				new Color32(188, 188, 188, 188),
				new Color32(125, 125, 125, 125),
				new Color32(35, 35, 35, 35),
				new Color32(223, 223, 223, 223),
				new Color32(19, 19, 19, 19),
				new Color32(207, 207, 207, 207),
				new Color32(47, 47, 47, 47),
				new Color32(235, 235, 235, 235),
				new Color32(31, 31, 31, 31),
				new Color32(219, 219, 219, 219),
				new Color32(160, 160, 160, 160),
				new Color32(98, 98, 98, 98),
				new Color32(145, 145, 145, 145),
				new Color32(82, 82, 82, 82),
				new Color32(172, 172, 172, 172),
				new Color32(109, 109, 109, 109),
				new Color32(156, 156, 156, 156),
				new Color32(94, 94, 94, 94),
				new Color32(11, 11, 11, 11),
				new Color32(200, 200, 200, 200),
				new Color32(58, 58, 58, 58),
				new Color32(247, 247, 247, 247),
				new Color32(7, 7, 7, 7),
				new Color32(196, 196, 196, 196),
				new Color32(54, 54, 54, 54),
				new Color32(243, 243, 243, 243),
				new Color32(137, 137, 137, 137),
				new Color32(74, 74, 74, 74),
				new Color32(184, 184, 184, 184),
				new Color32(121, 121, 121, 121),
				new Color32(133, 133, 133, 133),
				new Color32(70, 70, 70, 70),
				new Color32(180, 180, 180, 180),
				new Color32(117, 117, 117, 117),
				new Color32(43, 43, 43, 43),
				new Color32(231, 231, 231, 231),
				new Color32(27, 27, 27, 27),
				new Color32(215, 215, 215, 215),
				new Color32(39, 39, 39, 39),
				new Color32(227, 227, 227, 227),
				new Color32(23, 23, 23, 23),
				new Color32(211, 211, 211, 211),
				new Color32(168, 168, 168, 168),
				new Color32(105, 105, 105, 105),
				new Color32(153, 153, 153, 153),
				new Color32(90, 90, 90, 90),
				new Color32(164, 164, 164, 164),
				new Color32(102, 102, 102, 102),
				new Color32(149, 149, 149, 149),
				new Color32(86, 86, 86, 86)
			};
			texture2D_0.SetPixels32(pixels);
			texture2D_0.Apply();
		}
	}

	public Mesh method_9()
	{
		Mesh mesh = new Mesh();
		Vector3[] array = new Vector3[50];
		Color32[] array2 = new Color32[50];
		array[0] = new Vector3(0f, 0f, 0f);
		array[1] = new Vector3(0f, 0f, 1f);
		float num = 0f;
		float num2 = MathF.PI / 8f;
		float num3 = 0.9f;
		for (int i = 0; i < 16; i++)
		{
			array[i + 2] = new Vector3((0f - Mathf.Cos(num)) * num3, Mathf.Sin(num) * num3, num3);
			array2[i + 2] = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
			array[i + 2 + 16] = new Vector3(0f - Mathf.Cos(num), Mathf.Sin(num), 1f);
			array2[i + 2 + 16] = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, 0);
			array[i + 2 + 32] = new Vector3((0f - Mathf.Cos(num)) * num3, Mathf.Sin(num) * num3, 1f);
			array2[i + 2 + 32] = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
			num += num2;
		}
		mesh.vertices = array;
		mesh.colors32 = array2;
		int[] array3 = new int[288];
		int num4 = 0;
		for (int j = 2; j < 17; j++)
		{
			array3[num4++] = 0;
			array3[num4++] = j;
			array3[num4++] = j + 1;
		}
		array3[num4++] = 0;
		array3[num4++] = 17;
		array3[num4++] = 2;
		for (int k = 2; k < 17; k++)
		{
			array3[num4++] = k;
			array3[num4++] = k + 16;
			array3[num4++] = k + 1;
			array3[num4++] = k + 1;
			array3[num4++] = k + 16;
			array3[num4++] = k + 16 + 1;
		}
		array3[num4++] = 2;
		array3[num4++] = 17;
		array3[num4++] = 18;
		array3[num4++] = 18;
		array3[num4++] = 17;
		array3[num4++] = 33;
		for (int l = 18; l < 33; l++)
		{
			array3[num4++] = l;
			array3[num4++] = l + 16;
			array3[num4++] = l + 1;
			array3[num4++] = l + 1;
			array3[num4++] = l + 16;
			array3[num4++] = l + 16 + 1;
		}
		array3[num4++] = 18;
		array3[num4++] = 33;
		array3[num4++] = 34;
		array3[num4++] = 34;
		array3[num4++] = 33;
		array3[num4++] = 49;
		for (int m = 34; m < 49; m++)
		{
			array3[num4++] = 1;
			array3[num4++] = m + 1;
			array3[num4++] = m;
		}
		array3[num4++] = 1;
		array3[num4++] = 34;
		array3[num4++] = 49;
		mesh.triangles = array3;
		mesh.RecalculateBounds();
		return mesh;
	}

	public void OnDestroy()
	{
		method_10();
		method_11();
	}

	public void method_10()
	{
		bool_1 = true;
		if (renderTexture_0 != null)
		{
			renderTexture_0.Release();
			GClass972.SafeDestroy(renderTexture_0);
			renderTexture_0 = null;
		}
		if (renderTexture_3 != null)
		{
			renderTexture_3.Release();
			GClass972.SafeDestroy(renderTexture_3);
			renderTexture_3 = null;
		}
		if (renderTexture_1 != null)
		{
			renderTexture_1.Release();
			GClass972.SafeDestroy(renderTexture_1);
			renderTexture_1 = null;
		}
		if (renderTexture_2 != null)
		{
			renderTexture_2.Release();
			GClass972.SafeDestroy(renderTexture_2);
			renderTexture_2 = null;
		}
		if (renderTexture_4 != null)
		{
			renderTexture_4.Release();
			GClass972.SafeDestroy(renderTexture_4);
			renderTexture_4 = null;
		}
	}

	public void method_11()
	{
		bool_2 = true;
		if (renderTexture_5 != null)
		{
			renderTexture_5.Release();
			GClass972.SafeDestroy(renderTexture_5);
			renderTexture_5 = null;
		}
		if (renderTexture_6 != null)
		{
			renderTexture_6.Release();
			GClass972.SafeDestroy(renderTexture_6);
			renderTexture_6 = null;
		}
		if (renderTexture_7 != null)
		{
			renderTexture_7.Release();
			GClass972.SafeDestroy(renderTexture_7);
			renderTexture_7 = null;
		}
		if (renderTexture_8 != null)
		{
			renderTexture_8.Release();
			GClass972.SafeDestroy(renderTexture_8);
			renderTexture_8 = null;
		}
		if (renderTexture_9 != null)
		{
			renderTexture_9.Release();
			GClass972.SafeDestroy(renderTexture_9);
			renderTexture_9 = null;
		}
	}
}
