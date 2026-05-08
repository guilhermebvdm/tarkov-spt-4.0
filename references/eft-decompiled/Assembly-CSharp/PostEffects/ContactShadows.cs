using UnityEngine;
using UnityEngine.Rendering;

namespace PostEffects;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class ContactShadows : MonoBehaviour
{
	[SerializeField]
	private Light _light;

	[SerializeField]
	[Range(0f, 5f)]
	private float _rejectionDepth = 0.5f;

	[SerializeField]
	[Range(0f, 32f)]
	private int _sampleCount = 16;

	[SerializeField]
	[Range(0f, 1f)]
	private float _temporalFilter = 0.5f;

	[SerializeField]
	[Range(0f, 1f)]
	private float _contrast = 0.5f;

	[SerializeField]
	private bool _downsample;

	[SerializeField]
	private NoiseTextureSet _noiseTextures;

	[SerializeField]
	[HideInInspector]
	private Shader _shader;

	private Material material_0;

	private RenderTexture renderTexture_0;

	private RenderTexture renderTexture_1;

	private CommandBuffer commandBuffer_0;

	private CommandBuffer commandBuffer_1;

	private Matrix4x4 matrix4x4_0 = Matrix4x4.identity;

	private Camera camera_0;

	private SSAA ssaa_0;

	private int int_0;

	private int int_1;

	private int int_2;

	private int int_3;

	private static readonly int int_4 = Shader.PropertyToID("_RejectionDepth");

	private static readonly int int_5 = Shader.PropertyToID("_Contrast");

	private static readonly int int_6 = Shader.PropertyToID("_SampleCount");

	private static readonly int int_7 = Shader.PropertyToID("_Convergence");

	private static readonly int int_8 = Shader.PropertyToID("_LightVector");

	private static readonly int int_9 = Shader.PropertyToID("_NoiseScale");

	private static readonly int int_10 = Shader.PropertyToID("_NoiseTex");

	private static readonly int int_11 = Shader.PropertyToID("_Reprojection");

	public void Awake()
	{
		method_0();
		method_1();
	}

	public void OnValidate()
	{
		method_1();
	}

	public void method_0()
	{
		camera_0 = GetComponent<Camera>();
		ssaa_0 = GetComponent<SSAA>();
		if (_shader == null)
		{
			_shader = GClass872.Find("Hidden/PostEffects/ContactShadows");
		}
		if (material_0 == null)
		{
			material_0 = new Material(_shader);
			material_0.hideFlags = HideFlags.DontSave;
		}
		int_0 = Shader.PropertyToID("_ShadowMask");
		int_1 = Shader.PropertyToID("_PrevMask");
		int_2 = Shader.PropertyToID("_TempMask");
		int_3 = Shader.PropertyToID("_UnfilteredMask");
	}

	public void method_1()
	{
		if (!(material_0 == null))
		{
			material_0.SetFloat(int_4, _rejectionDepth);
			material_0.SetFloat(int_5, _contrast);
			material_0.SetInt(int_6, _sampleCount);
			float value = Mathf.Pow(1f - _temporalFilter, 2f);
			material_0.SetFloat(int_7, value);
		}
	}

	public void OnDestroy()
	{
		if (material_0 != null)
		{
			if (Application.isPlaying)
			{
				Object.Destroy(material_0);
			}
			else
			{
				Object.DestroyImmediate(material_0);
			}
		}
		if (renderTexture_0 != null)
		{
			RenderTexture.ReleaseTemporary(renderTexture_0);
		}
		if (renderTexture_1 != null)
		{
			RenderTexture.ReleaseTemporary(renderTexture_1);
		}
		if (commandBuffer_0 != null)
		{
			commandBuffer_0.Release();
		}
		if (commandBuffer_1 != null)
		{
			commandBuffer_1.Release();
		}
	}

	public void OnPreCull()
	{
		method_3();
		if (_light != null)
		{
			method_4();
			_light.AddCommandBuffer(LightEvent.AfterScreenspaceMask, commandBuffer_0);
			_light.AddCommandBuffer(LightEvent.AfterScreenspaceMask, commandBuffer_1);
		}
	}

	public void OnPreRender()
	{
		if (_light != null)
		{
			_light.RemoveCommandBuffer(LightEvent.AfterScreenspaceMask, commandBuffer_0);
			_light.RemoveCommandBuffer(LightEvent.AfterScreenspaceMask, commandBuffer_1);
			commandBuffer_0.Clear();
			commandBuffer_1.Clear();
		}
	}

	public void Update()
	{
		camera_0.depthTextureMode |= DepthTextureMode.Depth;
		if (!(_light != null) && GClass4.IsAvailable)
		{
			_light = GClass4.Instance.LightObject;
		}
	}

	public static Matrix4x4 smethod_0()
	{
		Camera current = Camera.current;
		Matrix4x4 nonJitteredProjectionMatrix = current.nonJitteredProjectionMatrix;
		Matrix4x4 worldToCameraMatrix = current.worldToCameraMatrix;
		return GL.GetGPUProjectionMatrix(nonJitteredProjectionMatrix, renderIntoTexture: true) * worldToCameraMatrix;
	}

	public Vector2 method_2()
	{
		Camera current = Camera.current;
		int num = ((!_downsample) ? 1 : 2);
		int num2 = (ssaa_0 ? ssaa_0.GetInputWidth() : current.pixelWidth);
		return new Vector2(y: (ssaa_0 ? ssaa_0.GetInputHeight() : current.pixelHeight) / num, x: num2 / num);
	}

	public void method_3()
	{
		if (renderTexture_1 != null)
		{
			RenderTexture.ReleaseTemporary(renderTexture_1);
			renderTexture_1 = null;
		}
		if (!(_light == null))
		{
			if (commandBuffer_0 == null)
			{
				commandBuffer_0 = new CommandBuffer();
				commandBuffer_1 = new CommandBuffer();
				commandBuffer_0.name = "Contact Shadow Ray Tracing";
				commandBuffer_1.name = "Contact Shadow Temporal Filter";
			}
			else
			{
				commandBuffer_0.Clear();
				commandBuffer_1.Clear();
			}
			material_0.SetVector(int_8, base.transform.InverseTransformDirection(-_light.transform.forward) * _light.shadowBias / ((float)_sampleCount - 1.5f));
			if (_noiseTextures != null)
			{
				Texture2D texture = _noiseTextures.GetTexture();
				Vector2 vector = method_2() / texture.width;
				material_0.SetVector(int_9, vector);
				material_0.SetTexture(int_10, texture);
			}
			material_0.SetMatrix(int_11, matrix4x4_0 * base.transform.localToWorldMatrix);
			matrix4x4_0 = smethod_0();
		}
	}

	public void method_4()
	{
		Vector2 vector = method_2();
		RenderTextureFormat format = RenderTextureFormat.R8;
		RenderTexture temporary = RenderTexture.GetTemporary(Mathf.FloorToInt(vector.x), Mathf.FloorToInt(vector.y), 0, RenderTextureFormat.R8);
		if (_temporalFilter == 0f)
		{
			commandBuffer_0.SetGlobalTexture(int_0, BuiltinRenderTextureType.CurrentActive);
			commandBuffer_0.SetRenderTarget(temporary);
			commandBuffer_0.DrawProcedural(Matrix4x4.identity, material_0, 0, MeshTopology.Triangles, 3);
		}
		else
		{
			commandBuffer_0.SetGlobalTexture(int_0, BuiltinRenderTextureType.CurrentActive);
			commandBuffer_0.GetTemporaryRT(int_3, Mathf.FloorToInt(vector.x), Mathf.FloorToInt(vector.y), 0, FilterMode.Point, format);
			commandBuffer_0.SetRenderTarget(int_3);
			commandBuffer_0.DrawProcedural(Matrix4x4.identity, material_0, 0, MeshTopology.Triangles, 3);
			commandBuffer_0.SetGlobalTexture(int_1, renderTexture_0);
			commandBuffer_0.SetRenderTarget(temporary);
			commandBuffer_0.DrawProcedural(Matrix4x4.identity, material_0, 1 + (Time.frameCount & 1), MeshTopology.Triangles, 3);
		}
		if (_downsample)
		{
			commandBuffer_1.SetGlobalTexture(int_2, temporary);
			commandBuffer_1.DrawProcedural(Matrix4x4.identity, material_0, 3, MeshTopology.Triangles, 3);
		}
		else
		{
			commandBuffer_1.Blit(temporary, BuiltinRenderTextureType.CurrentActive);
		}
		renderTexture_1 = renderTexture_0;
		renderTexture_0 = temporary;
	}
}
