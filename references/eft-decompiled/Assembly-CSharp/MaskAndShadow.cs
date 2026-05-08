using UnityEngine;
using UnityEngine.Rendering;

public class MaskAndShadow : MonoBehaviour
{
	public Shader Shader;

	public Texture2D Mask;

	[Header("Shadow")]
	public Vector2 ShadowShift;

	public int ShadowBlurIterations = 1;

	public float ShadowBlurStrength = 1f;

	[Range(0f, 1f)]
	public float ShadowStrength = 1f;

	private Camera camera_0;

	private Material material_0;

	private CommandBuffer commandBuffer_0;

	private static readonly int int_0 = Shader.PropertyToID("_MnS_MaskTexture");

	public Material method_0()
	{
		if (material_0 != null)
		{
			return material_0;
		}
		return material_0 = new Material(Shader);
	}

	public void OnValidate()
	{
		if (camera_0 == null)
		{
			camera_0 = GetComponent<Camera>();
		}
		if (commandBuffer_0 == null)
		{
			commandBuffer_0 = new CommandBuffer
			{
				name = "grab alpha and blur"
			};
		}
		method_1();
		method_0().SetTexture(int_0, Mask);
	}

	public void Awake()
	{
		OnValidate();
		camera_0 = GetComponent<Camera>();
		CommandBuffer commandBuffer = new CommandBuffer
		{
			name = "grab background"
		};
		CameraEvent evt = ((camera_0.actualRenderingPath == RenderingPath.DeferredShading) ? CameraEvent.BeforeGBuffer : CameraEvent.BeforeForwardOpaque);
		int num = Shader.PropertyToID("_MnS_Background");
		commandBuffer.GetTemporaryRT(num, -1, -1, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32);
		commandBuffer.Blit(BuiltinRenderTextureType.CurrentActive, num);
		commandBuffer.ClearRenderTarget(clearDepth: false, clearColor: true, Color.clear);
		commandBuffer.SetGlobalTexture("_MnS_Background", num);
		camera_0.AddCommandBuffer(evt, commandBuffer);
		method_1();
	}

	public void method_1()
	{
		if (commandBuffer_0 == null)
		{
			commandBuffer_0 = new CommandBuffer
			{
				name = "grab alpha and blur"
			};
		}
		camera_0.RemoveCommandBuffer(CameraEvent.BeforeImageEffectsOpaque, commandBuffer_0);
		commandBuffer_0.Clear();
		Material mat = method_0();
		int num = Shader.PropertyToID("_MnS_Alpha");
		commandBuffer_0.GetTemporaryRT(num, -1, -1, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32);
		commandBuffer_0.Blit(BuiltinRenderTextureType.CurrentActive, num);
		int num2 = method_2(commandBuffer_0, mat, num);
		commandBuffer_0.SetGlobalTexture("_MnS_BluredAlpha", num2);
		commandBuffer_0.SetGlobalVector("_MnS_ShadowShift", ShadowShift);
		commandBuffer_0.SetGlobalFloat("_MnS_ShadowStrength", ShadowStrength);
		commandBuffer_0.Blit(num, BuiltinRenderTextureType.CameraTarget, mat, 2);
		camera_0.AddCommandBuffer(CameraEvent.BeforeImageEffectsOpaque, commandBuffer_0);
	}

	public int method_2(CommandBuffer buffer, Material mat, int blurTex)
	{
		int pixelWidth = camera_0.pixelWidth;
		int pixelHeight = camera_0.pixelHeight;
		int num = Shader.PropertyToID("tempTex7bc8_0");
		int num2 = Shader.PropertyToID("tempTex7bc8_1");
		buffer.GetTemporaryRT(num, -2, -2, 0, FilterMode.Bilinear, RenderTextureFormat.R8);
		buffer.GetTemporaryRT(num2, -2, -2, 0, FilterMode.Bilinear, RenderTextureFormat.R8);
		buffer.Blit(blurTex, num, mat, 1);
		for (int i = 0; i < ShadowBlurIterations; i++)
		{
			float num3 = (float)(1 << i) * ShadowBlurStrength;
			buffer.SetGlobalVector("_MnSBlurOffsets", new Vector4(num3 / (float)pixelWidth, 0f, 0f, 0f));
			buffer.Blit(num, num2, mat, 0);
			buffer.SetGlobalVector("_MnSBlurOffsets", new Vector4(0f, num3 / (float)pixelHeight, 0f, 0f));
			buffer.Blit(num2, num, mat, 0);
		}
		buffer.ReleaseTemporaryRT(num2);
		return num;
	}
}
