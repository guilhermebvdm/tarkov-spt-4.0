using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]
public class BlurTextureCommandBuffer : MonoBehaviour
{
	public Shader BlurShader;

	private Material material_0;

	private Camera camera_0;

	private CommandBuffer commandBuffer_0;

	public void method_0()
	{
		if (camera_0 != null && commandBuffer_0 != null)
		{
			camera_0.RemoveCommandBuffer(CameraEvent.AfterImageEffectsOpaque, commandBuffer_0);
		}
		Object.DestroyImmediate(material_0);
	}

	public void OnEnable()
	{
		method_0();
	}

	public void OnDisable()
	{
		method_0();
	}

	public void OnRenderImage(RenderTexture src, RenderTexture dest)
	{
		camera_0 = Camera.current;
		if ((bool)camera_0)
		{
			method_0();
			if (!material_0)
			{
				material_0 = new Material(BlurShader);
				material_0.hideFlags = HideFlags.HideAndDontSave;
			}
			commandBuffer_0 = new CommandBuffer();
			commandBuffer_0.name = "Grab screen and blur";
			int num = Shader.PropertyToID("_ScreenCopyTexture");
			commandBuffer_0.GetTemporaryRT(num, -1, -1, 0, FilterMode.Bilinear);
			commandBuffer_0.Blit(BuiltinRenderTextureType.CurrentActive, num);
			int num2 = Shader.PropertyToID("_Temp1");
			int num3 = Shader.PropertyToID("_Temp2");
			commandBuffer_0.GetTemporaryRT(num2, -2, -2, 0, FilterMode.Bilinear);
			commandBuffer_0.GetTemporaryRT(num3, -2, -2, 0, FilterMode.Bilinear);
			commandBuffer_0.Blit(num, num2);
			commandBuffer_0.ReleaseTemporaryRT(num);
			commandBuffer_0.SetGlobalVector("offsets", new Vector4(2f / (float)Screen.width, 0f, 0f, 0f));
			commandBuffer_0.Blit(num2, num3, material_0);
			commandBuffer_0.SetGlobalVector("offsets", new Vector4(0f, 2f / (float)Screen.height, 0f, 0f));
			commandBuffer_0.Blit(num3, num2, material_0);
			commandBuffer_0.SetGlobalVector("offsets", new Vector4(4f / (float)Screen.width, 0f, 0f, 0f));
			commandBuffer_0.Blit(num2, num3, material_0);
			commandBuffer_0.SetGlobalVector("offsets", new Vector4(0f, 4f / (float)Screen.height, 0f, 0f));
			commandBuffer_0.Blit(num3, num2, material_0);
			commandBuffer_0.SetGlobalVector("offsets", new Vector4(8f / (float)Screen.width, 0f, 0f, 0f));
			commandBuffer_0.Blit(num2, num3, material_0);
			commandBuffer_0.SetGlobalVector("offsets", new Vector4(0f, 8f / (float)Screen.height, 0f, 0f));
			commandBuffer_0.Blit(num3, num2, material_0);
			commandBuffer_0.SetGlobalTexture("_GrabBlurTexture", num2);
			camera_0.AddCommandBuffer(CameraEvent.AfterImageEffectsOpaque, commandBuffer_0);
			Graphics.Blit(src, dest);
		}
	}
}
