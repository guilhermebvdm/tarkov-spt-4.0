using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]
[RequireComponent(typeof(Light))]
public class ShadowDepthMapToGlobal : MonoBehaviour
{
	public string textureSemanticName = "_SunCascadedShadowMap";

	private RenderTexture renderTexture_0;

	private CommandBuffer commandBuffer_0;

	private Light light_0;

	public void OnEnable()
	{
		light_0 = GetComponent<Light>();
		method_0();
	}

	public void OnDisable()
	{
		light_0.RemoveCommandBuffer(LightEvent.AfterShadowMap, commandBuffer_0);
		method_1();
	}

	public void method_0()
	{
		commandBuffer_0 = new CommandBuffer();
		RenderTargetIdentifier value = BuiltinRenderTextureType.CurrentActive;
		commandBuffer_0.SetGlobalTexture(textureSemanticName, value);
		light_0.AddCommandBuffer(LightEvent.AfterShadowMap, commandBuffer_0);
	}

	public void method_1()
	{
		commandBuffer_0.Clear();
	}
}
