using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class TextureDecalsPainter : MonoBehaviour
{
	public struct GStruct67
	{
		public Renderer Renderer;

		public Vector3 Point;

		public Vector3 Normal;

		public Texture2D Texture;

		public bool HasToSetShaderVars;

		public bool HasToSetTexture;
	}

	[Header("Render textures settings")]
	[SerializeField]
	private PowOfTwoDimensions _renderTexDimension = PowOfTwoDimensions._256;

	[SerializeField]
	private DepthSize _renderTexDepthSize;

	[Header("Shaders")]
	[SerializeField]
	private Shader _drawInterceptionShader;

	[SerializeField]
	private Shader _blitShader;

	[Header("Textures")]
	[SerializeField]
	private Texture2D _bloodDecalTexture;

	[SerializeField]
	private Texture2D _vestDecalTexture;

	[SerializeField]
	private Texture2D _backDecalTexture;

	[Header("Decal settings")]
	[SerializeField]
	private float _projectorHeight = 0.2f;

	[SerializeField]
	[GAttribute6(0f, 0.5f, -1f)]
	private Vector2 _decalSize = new Vector2(0.1f, 0.17f);

	private Material material_0;

	private Material material_1;

	private RenderTexture renderTexture_0;

	private RenderTexture renderTexture_1;

	private CommandBuffer commandBuffer_0;

	private Dictionary<Renderer, RenderTexture> dictionary_0 = new Dictionary<Renderer, RenderTexture>(20);

	private List<GStruct67> list_0 = new List<GStruct67>(20);

	private static readonly int int_0 = 20;

	private static int int_1 = 0;

	private const int int_2 = 128;

	private GClass835<RenderTexture> gclass835_0;

	private static readonly int int_3 = Shader.PropertyToID("_Decal");

	private static readonly int int_4 = Shader.PropertyToID("_DecalsTex");

	private static readonly int int_5 = Shader.PropertyToID("_DecalPower");

	private static readonly int int_6 = Shader.PropertyToID("_ObjectDecalTexture");

	private static readonly int int_7 = Shader.PropertyToID("_Right");

	private static readonly int int_8 = Shader.PropertyToID("_Up");

	private static readonly int int_9 = Shader.PropertyToID("_Fwd");

	private static readonly int int_10 = Shader.PropertyToID("_Point");

	public void Awake()
	{
		list_0 = new List<GStruct67>();
		material_0 = new Material(_drawInterceptionShader);
		material_1 = new Material(_blitShader);
		renderTexture_0 = new RenderTexture((int)_renderTexDimension, (int)_renderTexDimension, (int)_renderTexDepthSize)
		{
			name = "TextureDecalsPainter _renderTargetTex"
		};
		renderTexture_1 = new RenderTexture((int)_renderTexDimension, (int)_renderTexDimension, (int)_renderTexDepthSize)
		{
			name = "TextureDecalsPainter _tmpTex"
		};
		commandBuffer_0 = new CommandBuffer
		{
			name = "TextureDecalsPainter Buffer"
		};
		gclass835_0 = new GClass835<RenderTexture>(128, method_0, null, method_1);
	}

	public RenderTexture method_0()
	{
		RenderTexture renderTexture = new RenderTexture((int)_renderTexDimension, (int)_renderTexDimension, (int)_renderTexDepthSize);
		renderTexture.name = "TextureDecalsPainter rendererDecalTex " + int_1++;
		renderTexture.useMipMap = false;
		renderTexture.Create();
		return renderTexture;
	}

	public void method_1(RenderTexture rendererDecalTex)
	{
		rendererDecalTex.Release();
		GClass972.SafeDestroy(rendererDecalTex);
	}

	public void Update()
	{
		if (list_0.Count == 0)
		{
			return;
		}
		int num = Mathf.Min(list_0.Count, int_0);
		for (int i = 0; i < num; i++)
		{
			GStruct67 item = list_0[0];
			if (item.HasToSetShaderVars)
			{
				method_2(item.Point, -item.Normal);
			}
			if (item.HasToSetTexture)
			{
				material_0.SetTexture(int_3, item.Texture);
			}
			DrawDecal(item.Renderer);
			list_0.Remove(item);
		}
	}

	public void DrawDecal(Renderer objRenderer)
	{
		if (!dictionary_0.TryGetValue(objRenderer, out var value))
		{
			value = gclass835_0.Withdraw();
			GClass6.PreventMaterialChangeInEditor(objRenderer);
			Material material = objRenderer.material;
			material.SetTexture(int_4, value);
			material.SetFloat(int_5, 1f);
			dictionary_0.Add(objRenderer, value);
		}
		commandBuffer_0.Clear();
		commandBuffer_0.SetRenderTarget(renderTexture_0);
		commandBuffer_0.ClearRenderTarget(clearDepth: true, clearColor: true, new Color(0f, 0f, 0f, 0f));
		commandBuffer_0.DrawRenderer(objRenderer, material_0);
		material_1.SetTexture(int_6, value);
		commandBuffer_0.Blit(renderTexture_0, renderTexture_1, material_1);
		commandBuffer_0.Blit(renderTexture_1, value);
		Graphics.ExecuteCommandBuffer(commandBuffer_0);
	}

	public void method_2(Vector3 point, Vector3 normal)
	{
		Vector4 vector = normal.normalized;
		float num = Mathf.Abs(Vector3.Dot(vector, Vector3.up));
		Vector4 vector2 = Vector3.Cross(vector, (num > 0.999f) ? Vector3.right : Vector3.up).normalized;
		Vector4 value = Vector3.Cross(vector, vector2).normalized;
		float w = (vector2.w = Random.Range(_decalSize.x, _decalSize.y));
		vector.w = _projectorHeight;
		value.w = w;
		material_0.SetVector(int_7, vector2);
		material_0.SetVector(int_8, vector);
		material_0.SetVector(int_9, value);
		material_0.SetVector(int_10, point);
	}

	public Texture2D method_3(EDecalTextureType type)
	{
		return type switch
		{
			EDecalTextureType.Fabric => _backDecalTexture, 
			EDecalTextureType.Metal => _vestDecalTexture, 
			EDecalTextureType.Blood => _bloodDecalTexture, 
			_ => _backDecalTexture, 
		};
	}

	public void DrawDecal(List<BodyRendererDataStruct> renderers, Vector3 point, Vector3 normal)
	{
		int num = 0;
		foreach (BodyRendererDataStruct renderer in renderers)
		{
			Texture2D texture = method_3(renderer.DecalType);
			int num2 = 0;
			for (int i = 0; i < renderer.Renderers.Length; i++)
			{
				Renderer objRenderer = renderer.Renderers[i];
				if (method_5(objRenderer))
				{
					bool hasToSetShaderVars = num == 0;
					bool hasToSetTexture = num2 == 0;
					method_4(objRenderer, point, normal, texture, hasToSetShaderVars, hasToSetTexture);
					num++;
					num2++;
				}
			}
		}
	}

	public void method_4(Renderer objRenderer, Vector3 point, Vector3 normal, Texture2D texture, bool hasToSetShaderVars, bool hasToSetTexture)
	{
		GStruct67 item = new GStruct67
		{
			Renderer = objRenderer,
			Point = point,
			Normal = normal,
			Texture = texture,
			HasToSetShaderVars = hasToSetShaderVars,
			HasToSetTexture = hasToSetTexture
		};
		list_0.Add(item);
	}

	public bool method_5(Renderer objRenderer)
	{
		if (objRenderer.enabled && objRenderer.isVisible && objRenderer.gameObject.activeSelf)
		{
			return objRenderer.material.shader.name.Contains("_Decal");
		}
		return false;
	}

	public void OnDestroy()
	{
		foreach (RenderTexture value in dictionary_0.Values)
		{
			method_1(value);
		}
		dictionary_0.Clear();
		gclass835_0.Dispose();
		if (renderTexture_0 != null)
		{
			renderTexture_0.Release();
			GClass972.SafeDestroy(renderTexture_0);
			renderTexture_0 = null;
		}
		if (renderTexture_1 != null)
		{
			renderTexture_1.Release();
			GClass972.SafeDestroy(renderTexture_1);
			renderTexture_1 = null;
		}
	}

	public void Clear()
	{
		list_0.Clear();
	}
}
