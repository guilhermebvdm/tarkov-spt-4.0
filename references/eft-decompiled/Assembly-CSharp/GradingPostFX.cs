using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using EFT.BlitDebug;
using UnityEngine;

[ExecuteInEditMode]
public class GradingPostFX : MonoBehaviour
{
	public enum ColorBlindMode
	{
		None,
		Deuteranopia,
		Tritanopia,
		Protanopia
	}

	[Serializable]
	public class ColorBlindConnector
	{
		public ColorBlindMode ColorBlindMode;

		public Texture2D Texture;
	}

	[CompilerGenerated]
	public class Class615
	{
		public ColorBlindMode colorBlindMode;

		public bool method_0(ColorBlindConnector colorBlindTexture)
		{
			return colorBlindTexture.ColorBlindMode == colorBlindMode;
		}
	}

	[SerializeField]
	private Shader _shader;

	[SerializeField]
	private List<ColorBlindConnector> _colorBlindTextures = new List<ColorBlindConnector>();

	private Camera camera_0;

	private SSAA ssaa_0;

	private Material material_0;

	private RenderTexture renderTexture_0;

	private RenderTexture renderTexture_1;

	private RenderTexture renderTexture_2;

	private RenderTexture renderTexture_3;

	private RenderTexture renderTexture_4;

	private RenderTexture renderTexture_5;

	private Texture3D texture3D_0;

	private float float_0;

	private float float_1;

	private float float_2;

	private float float_3;

	private float float_4;

	private float float_5;

	private ColorBlindMode colorBlindMode_0;

	private float float_6;

	private CC_Vintage.Filter filter_0;

	private float float_7;

	private CC_Sharpen cc_Sharpen_0;

	private Texture2D texture2D_0;

	private readonly Vector2 vector2_0 = new Vector2(0f, 1f);

	private readonly Vector2 vector2_1 = new Vector2(0.75f, 1.25f);

	private readonly Vector2 vector2_2 = new Vector2(0.5f, 1.5f);

	private readonly Vector2 vector2_3 = new Vector2(0.8f, 2.2f);

	private readonly Vector2 vector2_4 = new Vector2(0f, 1f);

	private readonly Vector2 vector2_5 = new Vector2(0f, 1f);

	private readonly Vector2 vector2_6 = new Vector2(0f, 1f);

	private static readonly int int_0 = Shader.PropertyToID("_ClarityTex3");

	private static readonly int int_1 = Shader.PropertyToID("_AdaptiveSharpen");

	private static readonly int int_2 = Shader.PropertyToID("_ClarityStrength");

	private static readonly int int_3 = Shader.PropertyToID("_Brightness");

	private static readonly int int_4 = Shader.PropertyToID("_Saturation");

	private static readonly int int_5 = Shader.PropertyToID("_Colourfulness");

	private static readonly int int_6 = Shader.PropertyToID("_SharpStrength");

	private static readonly int int_7 = Shader.PropertyToID("_CurveHeight");

	private static readonly int int_8 = Shader.PropertyToID("_DesaturateMultiply");

	private static readonly int int_9 = Shader.PropertyToID("_ColorBlindTex");

	private static readonly int int_10 = Shader.PropertyToID("_ColorBlindScale");

	private static readonly int int_11 = Shader.PropertyToID("_ColorBlindOffset");

	private static readonly int int_12 = Shader.PropertyToID("_ColorBlindAmount");

	private static readonly int int_13 = Shader.PropertyToID("_LUTTex");

	private static readonly int int_14 = Shader.PropertyToID("_LUTAmount");

	[CompilerGenerated]
	private Action action_0;

	private SSAAPropagator ssaapropagator_0;

	public event Action OnAfterDestroy
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

	public void Awake()
	{
		camera_0 = GetComponent<Camera>();
		ssaa_0 = GetComponent<SSAA>();
		cc_Sharpen_0 = camera_0.GetComponent<CC_Sharpen>();
		method_9(method_0(), method_1());
		method_8();
		ssaapropagator_0 = GetComponent<SSAAPropagator>();
	}

	public void OnDestroy()
	{
		method_10();
		action_0?.Invoke();
	}

	public int method_0()
	{
		int num = (camera_0 ? camera_0.pixelWidth : Screen.width);
		if (ssaa_0 != null)
		{
			num = ((ssaa_0.GetOutputWidth() > 0) ? ssaa_0.GetOutputWidth() : num);
		}
		return num;
	}

	public int method_1()
	{
		int num = (camera_0 ? camera_0.pixelHeight : Screen.height);
		if (ssaa_0 != null)
		{
			num = ((ssaa_0.GetOutputHeight() > 0) ? ssaa_0.GetOutputHeight() : num);
		}
		return num;
	}

	public virtual void Update()
	{
		int num = method_0();
		int num2 = method_1();
		if (ssaa_0 != null)
		{
			num = ssaa_0.GetOutputWidth();
			num2 = ssaa_0.GetOutputHeight();
		}
		if (renderTexture_1 == null || renderTexture_1.width != num || renderTexture_1.height != num2)
		{
			method_9(num, num2);
		}
	}

	public void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (ssaapropagator_0 != null)
		{
			ssaapropagator_0.GetSourceDestination(out source, out destination);
		}
		Graphics.Blit(source, renderTexture_1);
		method_2();
		method_5();
		if (float_5 > 0f)
		{
			method_6();
		}
		if (float_0 > 0f)
		{
			method_4();
		}
		if (filter_0 != CC_Vintage.Filter.None)
		{
			method_7();
		}
		if (colorBlindMode_0 != ColorBlindMode.None)
		{
			method_3();
		}
		Graphics.Blit(renderTexture_1, destination);
		if (ssaapropagator_0 != null)
		{
			ssaapropagator_0.ReleaseSourceDestination(source, destination);
		}
	}

	public void UpdateClarityStrength(float coef)
	{
		float_0 = Mathf.Lerp(vector2_0.x, vector2_0.y, coef);
		material_0.SetFloat(int_2, float_0);
	}

	public void UpdateTechnicolorBrightness(float coef)
	{
		float_1 = Mathf.Lerp(vector2_1.x, vector2_1.y, coef);
		material_0.SetFloat(int_3, float_1);
	}

	public void UpdateTechnicolorSaturation(float coef)
	{
		float_2 = Mathf.Lerp(vector2_2.x, vector2_2.y, coef);
		material_0.SetFloat(int_4, float_2);
	}

	public void UpdateColourfullness(float coef)
	{
		float_3 = Mathf.Lerp(vector2_4.x, vector2_4.y, coef);
		material_0.SetFloat(int_5, float_3);
	}

	public void UpdateLumaSharpenStrength(float coef)
	{
		float_4 = Mathf.Lerp(vector2_5.x, vector2_5.y, coef);
		material_0.SetFloat(int_6, float_4);
	}

	public void UpdateAdaptiveSharpenStrength(float coef)
	{
		float_5 = Mathf.Lerp(vector2_6.x, vector2_6.y, coef);
		material_0.SetFloat(int_7, float_5);
	}

	public void SetColorBlindMode(ColorBlindMode colorBlindMode)
	{
		colorBlindMode_0 = colorBlindMode;
		method_11(method_12(colorBlindMode_0));
		material_0.SetTexture(int_9, texture3D_0);
		if (texture3D_0 != null)
		{
			int width = texture3D_0.width;
			material_0.SetFloat(int_10, (float)(width - 1) / (1f * (float)width));
			material_0.SetFloat(int_11, 1f / (2f * (float)width));
		}
	}

	public void UpdateColorBlindAmount(float coef)
	{
		float_6 = coef;
		material_0.SetFloat(int_12, float_6);
	}

	public void SetFilter(CC_Vintage.Filter filter)
	{
		filter_0 = filter;
		if (filter_0 == CC_Vintage.Filter.None)
		{
			texture2D_0 = null;
		}
		else
		{
			texture2D_0 = GClass861.Load<Texture2D>("Instagram/" + filter);
		}
		material_0.SetTexture(int_13, texture2D_0);
	}

	public void UpdateFilterAmount(float coef)
	{
		float_7 = coef;
		material_0.SetFloat(int_14, float_7);
	}

	public void method_2()
	{
		if (!(cc_Sharpen_0 == null))
		{
			float t = Mathf.Clamp01(cc_Sharpen_0.WeatherDesaturate);
			float value = Mathf.Lerp(vector2_3.x, vector2_3.y, t);
			material_0.SetFloat(int_8, value);
		}
	}

	public void method_3()
	{
		DebugGraphics.Blit(renderTexture_1, renderTexture_0, material_0, 7);
		Graphics.CopyTexture(renderTexture_0, renderTexture_1);
	}

	public void method_4()
	{
		DebugGraphics.Blit(renderTexture_1, renderTexture_2, material_0, 1);
		DebugGraphics.Blit(renderTexture_2, renderTexture_3, material_0, 2);
		DebugGraphics.Blit(renderTexture_3, renderTexture_4, material_0, 3);
		material_0.SetTexture(int_0, renderTexture_4);
		DebugGraphics.Blit(renderTexture_1, renderTexture_0, material_0, 4);
		Graphics.CopyTexture(renderTexture_0, renderTexture_1);
	}

	public void method_5()
	{
		DebugGraphics.Blit(renderTexture_1, renderTexture_0, material_0, 0);
		Graphics.CopyTexture(renderTexture_0, renderTexture_1);
	}

	public void method_6()
	{
		DebugGraphics.Blit(renderTexture_1, renderTexture_5, material_0, 5);
		material_0.SetTexture(int_1, renderTexture_5);
		DebugGraphics.Blit(renderTexture_1, renderTexture_0, material_0, 6);
		Graphics.CopyTexture(renderTexture_0, renderTexture_1);
	}

	public void method_7()
	{
		DebugGraphics.Blit(renderTexture_1, renderTexture_0, material_0, 8);
		Graphics.CopyTexture(renderTexture_0, renderTexture_1);
	}

	public void method_8()
	{
		material_0 = new Material(_shader);
	}

	public void method_9(int cameraWidth, int cameraHeight)
	{
		int width = (int)((float)cameraWidth * 0.5f);
		int height = (int)((float)cameraHeight * 0.5f);
		int width2 = (int)((float)cameraWidth * 0.25f);
		int height2 = (int)((float)cameraHeight * 0.25f);
		renderTexture_0?.Release();
		renderTexture_0 = new RenderTexture(cameraWidth, cameraHeight, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
		renderTexture_1?.Release();
		renderTexture_1 = new RenderTexture(cameraWidth, cameraHeight, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
		renderTexture_2?.Release();
		renderTexture_2 = new RenderTexture(width, height, 24, RenderTextureFormat.R8, RenderTextureReadWrite.Default);
		renderTexture_3?.Release();
		renderTexture_3 = new RenderTexture(width, height, 24, RenderTextureFormat.R8, RenderTextureReadWrite.Default);
		renderTexture_4?.Release();
		renderTexture_4 = new RenderTexture(width2, height2, 24, RenderTextureFormat.R8, RenderTextureReadWrite.Default);
		renderTexture_5?.Release();
		renderTexture_5 = new RenderTexture(cameraWidth, cameraHeight, 24, RenderTextureFormat.RG32, RenderTextureReadWrite.Default);
	}

	public void method_10()
	{
		if (material_0 != null)
		{
			UnityEngine.Object.DestroyImmediate(material_0);
		}
		if (renderTexture_0 != null)
		{
			UnityEngine.Object.DestroyImmediate(renderTexture_0);
		}
		if (renderTexture_1 != null)
		{
			UnityEngine.Object.DestroyImmediate(renderTexture_1);
		}
		if (renderTexture_2 != null)
		{
			UnityEngine.Object.DestroyImmediate(renderTexture_2);
		}
		if (renderTexture_3 != null)
		{
			UnityEngine.Object.DestroyImmediate(renderTexture_3);
		}
		if (renderTexture_4 != null)
		{
			UnityEngine.Object.DestroyImmediate(renderTexture_4);
		}
		if (renderTexture_5 != null)
		{
			UnityEngine.Object.DestroyImmediate(renderTexture_5);
		}
		if (texture3D_0 != null)
		{
			UnityEngine.Object.DestroyImmediate(texture3D_0);
		}
	}

	public void method_11(Texture2D texture)
	{
		if (texture3D_0 != null)
		{
			UnityEngine.Object.DestroyImmediate(texture3D_0);
		}
		if (texture == null)
		{
			return;
		}
		int num = texture.width * texture.height;
		num = texture.height;
		Color[] pixels = texture.GetPixels();
		Color[] array = new Color[pixels.Length];
		for (int i = 0; i < num; i++)
		{
			for (int j = 0; j < num; j++)
			{
				for (int k = 0; k < num; k++)
				{
					int num2 = num - j - 1;
					array[i + j * num + k * num * num] = pixels[k * num + i + num2 * num * num];
				}
			}
		}
		if ((bool)texture3D_0)
		{
			UnityEngine.Object.DestroyImmediate(texture3D_0);
		}
		texture3D_0 = new Texture3D(num, num, num, TextureFormat.ARGB32, mipChain: false);
		texture3D_0.SetPixels(array);
		texture3D_0.Apply();
	}

	public Texture2D method_12(ColorBlindMode colorBlindMode)
	{
		return _colorBlindTextures.First((ColorBlindConnector colorBlindTexture) => colorBlindTexture.ColorBlindMode == colorBlindMode).Texture;
	}
}
